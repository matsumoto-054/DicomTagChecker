using DicomTagChecker.Temp.Models;
using DicomTagChecker.Temp.Properties;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace DicomTagChecker.Temp
{
    public class CsvFileMaker
    {
        CsvFileContents csvFileContents = new CsvFileContents();

        public void AddCsvContents(string fileName, DicomTagContents dicomTagContents, List<CsvFileContents> error)
        {
            csvFileContents.FileName = fileName;
            csvFileContents.PatientId = dicomTagContents.PatientId;

            error.Add(csvFileContents);
        }

        public void RecordErrorFiles(List<CsvFileContents> errorContents)
        {
            //書き出すのはファイル名、判定したタグ
            string fileName = $"{DateTime.Today.ToString("yyyyMMdd")}.csv";
            string path = Settings.Default.TemporaryFolder;
            string outputFolder = Path.Combine(path, fileName);

            List<CsvFileContents> csvData = new List<CsvFileContents>();

            //Shift-JISを使うのに必要とのこと
            //https://qiita.com/sugasaki/items/0639ea9ca07f1ba7a9e0
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            using (StreamWriter sw = new StreamWriter(outputFolder, true, Encoding.GetEncoding("Shift-JIS")))
            {
                foreach(var error in errorContents)
                {
                    csvData.Add(error);
                }

                string contents = string.Join(",", csvData);
                sw.WriteLine(contents);
                csvData.Clear();

                sw.Close();
            }
        }
    }
}