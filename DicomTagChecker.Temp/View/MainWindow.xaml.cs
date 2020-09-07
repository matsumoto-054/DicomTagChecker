using DicomTagChecker.Temp.Properties;
using Microsoft.WindowsAPICodePack.Dialogs;
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
        private StatusBarController statusBarController = new StatusBarController();

        private ObservableCollection<LogContents> logContents = new ObservableCollection<LogContents>();
        private LogContents log = new LogContents();

        private bool isReading = false;

        public CancellationTokenSource cancellation;

        public MainWindow()
        {
            InitializeComponent();

            CancelButton.IsEnabled = false;
            StatusBarLabel.Content = statusBarController.ChangeStatusBar(isReading);

            logContents.CollectionChanged += items_CollectionChanged;
        }

        private void SelectFolderButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new CommonOpenFileDialog();
            dialog.IsFolderPicker = true;

            dialog.Title = "フォルダを選択してください";

            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                FolderPathTextBox.Text = dialog.FileName;
            }
        }

        private async void StartButton_Click(object sender, RoutedEventArgs e)
        {
            if (String.IsNullOrWhiteSpace(FolderPathTextBox.Text))
            {
                this.LogDataGrid.ItemsSource = this.WriteLog("エラー", $"フォルダ未選択");
                return;
            }

            cancellation = new CancellationTokenSource();
            var cancelToken = cancellation.Token;

            StartButton.IsEnabled = false;
            CancelButton.IsEnabled = true;

            string targetFolder = FolderPathTextBox.Text;
            string temporaryFolder = Settings.Default.TemporaryFolder;

            this.LogDataGrid.ItemsSource = this.WriteLog("開始", $"\"{FolderPathTextBox.Text}\"内のdcmファイルを取得開始");

            Cursor = Cursors.AppStarting;
            isReading = true;
            StatusBarLabel.Content = statusBarController.ChangeStatusBar(isReading);

            try
            {
                DicomFileReader dicomFileReader = new DicomFileReader();
                await Task.Run(() => dicomFileReader.ReadDicomFilesAsync(targetFolder, temporaryFolder, cancelToken));

                this.LogDataGrid.ItemsSource = this.WriteLog("終了", $"\"{FolderPathTextBox.Text}\"内のdcmファイル取得が完了");
                isReading = false;
                StatusBarLabel.Content = statusBarController.ChangeStatusBar(isReading);
            }
            catch (OperationCanceledException)
            {
                this.LogDataGrid.ItemsSource = this.WriteLog("中断", $"\"{FolderPathTextBox.Text}\"内のdcmファイル取得を中断");

                Cursor = Cursors.Arrow;
                isReading = false;
                StatusBarLabel.Content = statusBarController.ChangeStatusBar(isReading);
            }
            catch (Exception ex)
            {
                this.LogDataGrid.ItemsSource = this.WriteLog("エラー", ex.Message);

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

        private ObservableCollection<LogContents> WriteLog(string status, string contents)
        {
            log.Date = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");
            log.Status = status;
            log.Contents = contents;

            logContents.Add(log);

            return logContents;
        }

        private void items_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            var view = LogDataGrid.View as GridView;
            this.AutoResizeGridViewColumns(view);
        }

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
                this.LogDataGrid.ItemsSource = this.WriteLog("エラー", $"処理未実行");
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (isReading)
            {
                var result = MessageBox.Show("取り込み処理を中断してアプリを終了しますか？", "確認", MessageBoxButton.YesNo, MessageBoxImage.Exclamation, MessageBoxResult.No);
                if (result == MessageBoxResult.Yes)
                {
                    this.LogDataGrid.ItemsSource = this.WriteLog("中断", $"\"{FolderPathTextBox.Text}\"内のdcmファイル取得を中断");
                }
                else
                {
                    e.Cancel = true;
                }
            }
        }
    }
}