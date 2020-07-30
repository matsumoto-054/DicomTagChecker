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

            this.LogDataGrid.ItemsSource = logWriter.WriteLog();
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

        private void LogDataGrid_AutoGeneratingColumn(object sender, System.Windows.Controls.DataGridAutoGeneratingColumnEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "Date":
                    e.Column.Header = "日時";
                    e.Column.DisplayIndex = 0;
                    break;
                case "Status":
                    e.Column.Header = "状態";
                    e.Column.DisplayIndex = 1;
                    break;
                case "Contents":
                    e.Column.Header = "処理";
                    e.Column.DisplayIndex = 2;
                    break;
                default:
                    throw new InvalidOperationException();
            }
        }
    }
}