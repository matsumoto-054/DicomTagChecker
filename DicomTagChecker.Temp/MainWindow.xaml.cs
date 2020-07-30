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

        public MainWindow()
        {
            InitializeComponent();

            DataContext = logWriter;
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
            string temporaryFolder = Settings.Default.TemporaryFolder;

            //DicomFileReader dicomFileReader = new DicomFileReader();
            //dicomFileReader.ReadDicomFiles(FolderPathTextBox.Text, temporaryFolder);

            this.LogDataGrid.ItemsSource = logWriter.WriteLog("開始", $"{FolderPathTextBox.Text}内のファイルを取得開始");
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
        }
    }
}