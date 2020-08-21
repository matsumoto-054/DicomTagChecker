using DicomTagChecker.Temp.Properties;
using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace DicomTagChecker.Temp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private StatusBarController statusBarController = new StatusBarController();
        private LogWriter logWriter = new LogWriter();
        private bool isReading = false;

        public CancellationTokenSource Cancellation;

        public MainWindow()
        {
            InitializeComponent();

            CancelButton.IsEnabled = false;
            StatusBarLabel.Content = statusBarController.ChangeStatusBar(isReading);
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
                this.LogDataGrid.ItemsSource = logWriter.WriteLog("エラー", $"フォルダ未選択");
                return;
            }

            StartButton.IsEnabled = false;
            CancelButton.IsEnabled = true;

            string targetFolder = FolderPathTextBox.Text;
            string temporaryFolder = Settings.Default.TemporaryFolder;

            this.LogDataGrid.ItemsSource = logWriter.WriteLog("開始", $"\"{FolderPathTextBox.Text}\"内のdcmファイルを取得開始");
            isReading = true;
            StatusBarLabel.Content = statusBarController.ChangeStatusBar(isReading);

            try
            {
                DicomFileReader dicomFileReader = new DicomFileReader();
                await Task.Run(() => dicomFileReader.ReadDicomFilesAsync(targetFolder, temporaryFolder));
            }
            catch (OperationCanceledException)
            {
                this.LogDataGrid.ItemsSource = logWriter.WriteLog("中断", $"\"{FolderPathTextBox.Text}\"内のdcmファイル取得を中断");
                isReading = false;
                StatusBarLabel.Content = statusBarController.ChangeStatusBar(isReading);
            }
            catch (Exception ex)
            {
                this.LogDataGrid.ItemsSource = logWriter.WriteLog("エラー", ex.Message);
            }

            this.LogDataGrid.ItemsSource = logWriter.WriteLog("終了", $"\"{FolderPathTextBox.Text}\"内のdcmファイル取得が完了");
            isReading = false;
            StatusBarLabel.Content = statusBarController.ChangeStatusBar(isReading);

            StartButton.IsEnabled = true;
            CancelButton.IsEnabled = false;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            if (isReading)
            {
                var result = MessageBox.Show("取り込み処理を中断しますか？", "確認", MessageBoxButton.YesNo, MessageBoxImage.Exclamation, MessageBoxResult.No);
                if (result == MessageBoxResult.Yes)
                {
                    if (Cancellation != null)
                    {
                        Cancellation.Cancel();
                    }
                }
            }
            else
            {
                this.LogDataGrid.ItemsSource = logWriter.WriteLog("エラー", $"処理未実行");
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
                    this.LogDataGrid.ItemsSource = logWriter.WriteLog("中断", $"\"{FolderPathTextBox.Text}\"内のdcmファイル取得を中断");
                }
                else
                {
                    e.Cancel = true;
                }
            }
        }
    }
}