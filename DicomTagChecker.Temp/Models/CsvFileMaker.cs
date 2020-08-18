using DicomTagChecker.Temp.Properties;
using System;
using System.Collections.Generic;
using System.IO;

namespace DicomTagChecker.Temp
{
    public class CsvFileMaker
    {
        public void RecordErrorFiles(DicomTagContents dicomTagContents)
        {
            //書き出すのはファイル名、判定したタグ
            string fileName = $"{DateTime.Today.ToString("yyyyMMdd")}.csv";
            string path = Settings.Default.TemporaryFolder;
            string outputFolder = Path.Combine(path, fileName);

            List<string> csvData = new List<string>();

            using (StreamWriter sw = new StreamWriter(outputFolder))
            {
                csvData.Add(fileName);
                csvData.Add(dicomTagContents.PatientId);

                string contents = string.Join(",", csvData);
                sw.WriteLine(contents);
                csvData.Clear();

                sw.Close();
            }
        }
    }
}