using DicomTagChecker.Temp.Properties;
using Microsoft.WindowsAPICodePack.Dialogs;
using NLog;
using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
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
        private static Logger logger = LogManager.GetCurrentClassLogger();

        private ObservableCollection<LogContents> logContents = new ObservableCollection<LogContents>();
        private LogContents log = new LogContents();

        private bool isReading = false;

        public CancellationTokenSource cancellation;

        public MainWindow()
        {
            InitializeComponent();

            logger.Info("アプリケーションの起動");

            CancelButton.IsEnabled = false;
            StatusBarLabel.Content = this.ChangeStatusBar(isReading);

            //ListViewの値が変わったときのイベント
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
        /// 取込開始ボタン
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

            cancellation = new CancellationTokenSource();
            var cancelToken = cancellation.Token;

            StartButton.IsEnabled = false;
            CancelButton.IsEnabled = true;

            string targetFolder = FolderPathTextBox.Text;
            string temporaryFolder = Settings.Default.TemporaryFolder;

            this.LogDataGrid.ItemsSource = this.WriteLog("開始", $"\"{FolderPathTextBox.Text}\"内のdcmファイルを取得開始");
            logger.Info("dcmファイルの取込開始");

            //取り込み処理中は、マウスカーソルを矢印+待機にする。
            Cursor = Cursors.AppStarting;
            isReading = true;
            StatusBarLabel.Content = this.ChangeStatusBar(isReading);

            try
            {
                DicomFileReader dicomFileReader = new DicomFileReader();
                await Task.Run(() => dicomFileReader.ReadDicomFilesAsync(targetFolder, temporaryFolder, cancelToken));

                this.LogDataGrid.ItemsSource = this.WriteLog("終了", $"\"{FolderPathTextBox.Text}\"内のdcmファイル取得が完了");
                logger.Info("dcmファイルの取得完了");
                isReading = false;
                StatusBarLabel.Content = this.ChangeStatusBar(isReading);
            }
            catch (OperationCanceledException)
            {
                //キャンセルボタンによる取込中断
                this.LogDataGrid.ItemsSource = this.WriteLog("中断", $"\"{FolderPathTextBox.Text}\"内のdcmファイル取得を中断");
                logger.Info("dcmファイルの取得を中断");

                Cursor = Cursors.Arrow;
                isReading = false;
                StatusBarLabel.Content = this.ChangeStatusBar(isReading);
            }
            catch (Exception ex)
            {
                //取込中に例外が発生したら、中断される
                this.LogDataGrid.ItemsSource = this.WriteLog("エラー", ex.Message);
                logger.Error(ex.Message);

                Cursor = Cursors.Arrow;
                isReading = false;
                StartButton.IsEnabled = true;
                CancelButton.IsEnabled = false;
            }

            cancellation.Dispose();
            cancellation = null;

            Cursor = Cursors.Arrow;
            StartButton.IsEnabled = true;
            CancelButton.IsEnabled = false;
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
                        cancellation?.Cancel();
                    }
                }
            }
            else
            {
                //取り込み処理していないときはキャンセルボタンを押せないので、ここに来ることはないが一応...
                this.LogDataGrid.ItemsSource = this.WriteLog("エラー", $"処理未実行");
            }
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
            string message = isReading ? "取込中" : "準備完了";

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
                var result = MessageBox.Show("取り込み処理を中断してアプリを終了しますか？", "確認", MessageBoxButton.YesNo, MessageBoxImage.Exclamation, MessageBoxResult.No);
                if (result == MessageBoxResult.Yes)
                {
                    this.LogDataGrid.ItemsSource = this.WriteLog("中断", $"\"{FolderPathTextBox.Text}\"内のdcmファイル取得を中断");
                    logger.Info("dcmファイルの取得を中断");
                }
                else
                {
                    e.Cancel = true;
                }
            }
        }
    }
}