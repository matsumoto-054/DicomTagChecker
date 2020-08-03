﻿using Dicom;
using System;
using System.IO;
using System.Linq;

namespace DicomTagChecker.Temp
{
    public class DicomFileReader
    {
        private string filePattern;

        public void ReadDicomFiles(string targetFolderPath, string temporaryFolderPath)
        {
            //ファイルの移動（Temporaryへ）
            if (!Directory.Exists(temporaryFolderPath))
            {
                Directory.CreateDirectory(temporaryFolderPath);
            }

            File.Copy(targetFolderPath, temporaryFolderPath);

            //ファイル読込
            //Validate
            foreach (var file in this.MonitorTargetDirectory(temporaryFolderPath))
            {
                var dcmFile = DicomFile.Open(file);
                DicomTagContents dicomTagContents = new DicomTagContents
                {
                    //ここに判定するタグを追加
                    PatientId = dcmFile.Dataset.Get<string>(DicomTag.PatientID)
                };

                ValidateContents validateContents = new ValidateContents();
                if (validateContents.HasErrorTag(dicomTagContents))
                {
                    //引っかかったものをcsv出力

                }
            }
        }

        private string[] MonitorTargetDirectory(string folder)
        {
            string extention = Path.GetExtension(filePattern);

            //監視対象フォルダにある、指定した拡張子で終わるファイルを取得。
            //"*.csv"と指定したとき、"*.csvabc"は取得しない。
            return Directory.GetFiles(folder, filePattern).
                Where(x => x.EndsWith(extention, StringComparison.OrdinalIgnoreCase)).ToArray();
        }
    }
}