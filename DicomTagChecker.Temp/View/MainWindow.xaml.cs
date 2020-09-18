using DicomTagChecker.Temp.Models;
using DicomTagChecker.Temp.Properties;
using Microsoft.WindowsAPICodePack.Dialogs;
using NLog;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace DicomTagChecker.Temp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // logger
        private static Logger logger = LogManager.GetCurrentClassLogger();

        // テンポラリーフォルダ
        private string temporaryFolderPath = Settings.Default.TemporaryFolder;

        // ログ出力用
        private ObservableCollection<LogContents> logContents = new ObservableCollection<LogContents>();

        private LogContents log = new LogContents();

        // 読込処理中かどうか
        private bool isReading = false;

        // 処理キャンセル判定
        public CancellationTokenSource cancellation = null;

        public MainWindow()
        {
            InitializeComponent();

            logger.Info("アプリケーションの起動");

            this.ChangeCursorAndEnableButtons(isReading);
            StatusBarLabel.Content = this.ChangeStatusBar(isReading);

            // ListViewの値が変わったときのイベント
            logContents.CollectionChanged += items_CollectionChanged;
        }

        /// <summary>
        /// フォルダの選択
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SelectFolderButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new CommonOpenFileDialog();
            dialog.IsFolderPicker = true;

            dialog.Title = "フォルダを選択してください";

            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                FolderPathTextBox.Text = dialog.FileName;
                logger.Info($"フォルダ'{FolderPathTextBox.Text}'を監視");
            }
        }

        /// <summary>
        /// 読込開始ボタン
        /// 非同期によるタグ判定処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void StartButton_Click(object sender, RoutedEventArgs e)
        {
            if (String.IsNullOrWhiteSpace(FolderPathTextBox.Text))
            {
                this.LogDataGrid.ItemsSource = this.WriteLog("エラー", "フォルダ未選択");
                logger.Warn("フォルダ未選択");

                return;
            }

            // 監視対象フォルダを変数に用意
            string targetFolderPath = FolderPathTextBox.Text;

            this.LogDataGrid.ItemsSource = this.WriteLog("開始", $"\"{targetFolderPath}\"内のdcmファイルを読込開始");
            logger.Info("dcmファイルの読込開始");

            // 読込処理中は、マウスカーソルを矢印+待機にする。
            isReading = true;
            this.ChangeCursorAndEnableButtons(isReading);
            StatusBarLabel.Content = this.ChangeStatusBar(isReading);

            var error = new List<CsvFileContents>();

            try
            {
                // ファイルのコピー（フォルダごとTemporaryへ）
                this.CopyDirectory(targetFolderPath, temporaryFolderPath);
                logger.Info($"'{temporaryFolderPath}'へファイルのコピー完了");

                // ファイル読込
                // Validate
                foreach (var file in this.MonitorTemporaryDirectory(temporaryFolderPath))
                {
                    if (cancellation == null)
                    {
                        cancellation = new CancellationTokenSource();
                    }

                    var token = cancellation.Token;

                    DicomFileReader dicomFileReader = new DicomFileReader();
                    await Task.Run(() =>
                    {
                        dicomFileReader.ReadDicomFilesAsync(file);
                    }, token).ContinueWith(t =>
                    {
                        cancellation.Dispose();
                        cancellation = null;

                        if (t.IsCanceled)
                        {
                            // キャンセルボタンによる取込中断
                            this.LogDataGrid.ItemsSource = this.WriteLog("中断", $"\"{FolderPathTextBox.Text}\"内のdcmファイル読込を中断");
                            logger.Info("dcmファイルの読込を中断");

                            isReading = false;
                            this.ChangeCursorAndEnableButtons(isReading);
                            StatusBarLabel.Content = this.ChangeStatusBar(isReading);
                        }
                    });
                }

                this.LogDataGrid.ItemsSource = this.WriteLog("終了", $"\"{FolderPathTextBox.Text}\"内のdcmファイル読込が完了");
                logger.Info("dcmファイルの読込完了");
            }
            catch (Exception ex)
            {
                // 読込中に例外が発生したら、中断される
                this.LogDataGrid.ItemsSource = this.WriteLog("エラー", ex.Message);
                logger.Error(ex.Message);

                isReading = false;
                this.ChangeCursorAndEnableButtons(isReading);
                StatusBarLabel.Content = this.ChangeStatusBar(isReading);
            }

            // 読込が終了したらtempフォルダの中身を削除する
            foreach(var file in this.MonitorTemporaryDirectory(temporaryFolderPath))
            {
                File.Delete(file);
            }

            isReading = false;
            this.ChangeCursorAndEnableButtons(isReading);
            StatusBarLabel.Content = this.ChangeStatusBar(isReading);
        }

        /// <summary>
        /// 読込中かどうかでカーソルとボタンのアクティブを変更
        /// </summary>
        /// <param name="isReading"></param>
        private void ChangeCursorAndEnableButtons(bool isReading)
        {
            if (isReading)
            {
                Cursor = Cursors.AppStarting;
                StartButton.IsEnabled = false;
                CancelButton.IsEnabled = true;
            }
            else
            {
                Cursor = Cursors.Arrow;
                StartButton.IsEnabled = true;
                CancelButton.IsEnabled = false;
            }
        }

        /// <summary>
        /// 選択したフォルダの中身をテンポラリーフォルダへコピー
        /// </summary>
        /// <param name="sourceDirName"></param>
        /// <param name="destDirName"></param>
        private void CopyDirectory(string sourceDirName, string destDirName)
        {
            // コピー先のディレクトリがないときは作る
            if (!Directory.Exists(destDirName))
            {
                Directory.CreateDirectory(destDirName);
                // 属性もコピー
                File.SetAttributes(destDirName, File.GetAttributes(sourceDirName));
            }

            // コピー先のディレクトリ名の末尾に"\"をつける
            if (destDirName[destDirName.Length - 1] != Path.DirectorySeparatorChar)
            {
                destDirName = destDirName + Path.DirectorySeparatorChar;
            }

            // コピー元のディレクトリにあるファイルをコピー
            string[] files = Directory.GetFiles(sourceDirName);
            foreach (string file in files)
            {
                File.Copy(file, destDirName + Path.GetFileName(file), true);
            }

            // コピー元のディレクトリにあるディレクトリについて、再帰的に呼び出す
            string[] dirs = Directory.GetDirectories(sourceDirName);
            foreach (string dir in dirs)
            {
                CopyDirectory(dir, destDirName + Path.GetFileName(dir));
            }
        }

        /// <summary>
        /// 拡張子が.dcmのファイルのみを取得
        /// </summary>
        /// <param name="folder"></param>
        /// <returns></returns>
        private string[] MonitorTemporaryDirectory(string folder)
        {
            string filePattern = Settings.Default.FilePattern;
            string extention = Path.GetExtension(filePattern);

            // 監視対象フォルダにある、指定した拡張子で終わるファイルを取得。
            // "*.dcm"と指定したとき、"*.dcmabc"は取得しない。
            return Directory.GetFiles(folder, filePattern).
                Where(x => x.EndsWith(extention, StringComparison.OrdinalIgnoreCase)).ToArray();
        }

        /// <summary>
        /// ログの内容
        /// </summary>
        /// <param name="status"></param>
        /// <param name="contents"></param>
        /// <returns></returns>
        private ObservableCollection<LogContents> WriteLog(string status, string contents)
        {
            log.Date = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");
            log.Status = status;
            log.Contents = contents;

            logContents.Add(log);

            return logContents;
        }

        /// <summary>
        /// ListViewの値が変わったとき（＝ログが追記されたとき）のイベント
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void items_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            var view = LogDataGrid.View as GridView;
            this.AutoResizeGridViewColumns(view);
        }

        /// <summary>
        /// ログの長さによって、ListViewの幅を自動調整
        /// </summary>
        /// <param name="view"></param>
        private void AutoResizeGridViewColumns(GridView view)
        {
            if (view == null || view.Columns.Count < 1) return;

            foreach (var column in view.Columns)
            {
                if (double.IsNaN(column.Width))
                    column.Width = 1;
                column.Width = double.NaN;
            }
        }

        /// <summary>
        /// キャンセルボタン
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            if (isReading)
            {
                var result = MessageBox.Show("取り込み処理を中断しますか？", "確認", MessageBoxButton.YesNo, MessageBoxImage.Exclamation, MessageBoxResult.No);
                if (result == MessageBoxResult.Yes)
                {
                    if (cancellation != null)
                    {
                        cancellation.Cancel();
                    }
                }
            }
            else
            {
                // 取り込み処理していないときはキャンセルボタンを押せないので、ここに来ることはないが一応...
                this.LogDataGrid.ItemsSource = this.WriteLog("エラー", $"処理未実行");
            }
        }

        /// <summary>
        /// ログを消去
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DeleteLogButton_Click(object sender, RoutedEventArgs e)
        {
            logContents.Clear();
        }

        /// <summary>
        /// 終了ボタン
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TerminateButton_Click(object sender, RoutedEventArgs e)
        {
            logger.Info("アプリケーションの終了");
            this.Close();
        }

        /// <summary>
        /// ステータスバーの表示を変更
        /// </summary>
        private string ChangeStatusBar(bool isReading)
        {
            string message = isReading ? "読込中" : "準備完了";

            return message;
        }

        /// <summary>
        /// アプリを閉じるときに取り込み処理中だった場合、ダイアログを表示して確認
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (isReading)
            {
                var result = MessageBox.Show("読込処理を中断してアプリを終了しますか？", "確認", MessageBoxButton.YesNo, MessageBoxImage.Exclamation, MessageBoxResult.No);
                if (result == MessageBoxResult.Yes)
                {
                    this.LogDataGrid.ItemsSource = this.WriteLog("中断", $"\"{FolderPathTextBox.Text}\"内のdcmファイル読込を中断");
                    logger.Info("dcmファイルの読込を中断");
                }
                else
                {
                    e.Cancel = true;
                }
            }
        }
    }
}