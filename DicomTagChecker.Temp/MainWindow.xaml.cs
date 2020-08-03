using Microsoft.WindowsAPICodePack.Dialogs;
using DicomTagChecker.Temp.Properties;
using System.Windows;
using System;

namespace DicomTagChecker.Temp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        LogWriter logWriter = new LogWriter();
        bool isReading;

        public MainWindow()
        {
            InitializeComponent();
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

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            if(String.IsNullOrWhiteSpace(FolderPathTextBox.Text))
            {
                this.LogDataGrid.ItemsSource = logWriter.WriteLog("エラー", $"フォルダ未選択");
                return;
            }

            string temporaryFolder = Settings.Default.TemporaryFolder;

            this.LogDataGrid.ItemsSource = logWriter.WriteLog("開始", $"\"{FolderPathTextBox.Text}\"内のdcmファイルを取得開始");
            isReading = true;

            DicomFileReader dicomFileReader = new DicomFileReader();
            dicomFileReader.ReadDicomFiles(FolderPathTextBox.Text, temporaryFolder);

            this.LogDataGrid.ItemsSource = logWriter.WriteLog("終了", $"\"{FolderPathTextBox.Text}\"内のdcmファイル取得が完了");
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            if (isReading)
            {
                var result = MessageBox.Show("取り込み処理を中断しますか？", "確認", MessageBoxButton.YesNo, MessageBoxImage.Exclamation, MessageBoxResult.No);
                if(result == MessageBoxResult.Yes)
                {
                    this.LogDataGrid.ItemsSource = logWriter.WriteLog("中断", $"\"{FolderPathTextBox.Text}\"内のdcmファイル取得を中断");
                    isReading = false;
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