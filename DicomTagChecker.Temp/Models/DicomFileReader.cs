using Dicom;
using DicomTagChecker.Temp.Models;
using DicomTagChecker.Temp.Properties;
using NLog;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DicomTagChecker.Temp
{
    public class DicomFileReader
    {
        private CsvFileMaker csvFileMaker = new CsvFileMaker();

        /// <summary>
        ///
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public void ReadDicomFilesAsync(string file)
        {
            var error = new List<CsvFileContents>();

            var task = Task.Run(() =>
            {
                var dcmFile = DicomFile.Open(file);
                DicomTagContents dicomTagContents = new DicomTagContents
                {
                    //ここに判定するタグを追加
                    PatientId = dcmFile.Dataset.Get<string>(DicomTag.PatientID)
                };

                ValidateDicomTagContents validateContents = new ValidateDicomTagContents();
                if (validateContents.HasErrorTag(dicomTagContents))
                {
                    //csv出力する内容をListに用意
                    csvFileMaker.AddCsvContents(file, dicomTagContents, error);
                }
            });

            //csv出力
            csvFileMaker.RecordErrorFiles(error);
        }
    }
}