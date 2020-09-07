using Dicom;
using DicomTagChecker.Temp.Models;
using DicomTagChecker.Temp.Properties;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DicomTagChecker.Temp
{
    public class DicomFileReader
    {
        private string filePattern = Settings.Default.FilePattern;
        private CsvFileMaker csvFileMaker = new CsvFileMaker();

        /// <summary>
        /// 非同期によるDicomファイルの読み込み
        /// </summary>
        /// <param name="targetFolderPath"></param>
        /// <param name="temporaryFolderPath"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task ReadDicomFilesAsync(string targetFolderPath, string temporaryFolderPath, CancellationToken cancellationToken)
        {
            var tasks = new List<Task>();
            var error = new List<CsvFileContents>();

            //ファイルのコピー（フォルダごとTemporaryへ）
            this.CopyDirectory(targetFolderPath, temporaryFolderPath);

            //ファイル読込
            //Validate
            foreach (var file in this.MonitorTemporaryDirectory(temporaryFolderPath))
            {
                var task = Task.Run(() =>
                {
                    //キャンセルされたかどうかはこのタイミングで検知
                    cancellationToken.ThrowIfCancellationRequested();

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

                tasks.Add(task);
            }

            //csv出力
            csvFileMaker.RecordErrorFiles(error);

            return Task.WhenAll(tasks);
        }

        /// <summary>
        /// 選択したフォルダの中身をテンポラリーフォルダへコピー
        /// </summary>
        /// <param name="sourceDirName"></param>
        /// <param name="destDirName"></param>
        private void CopyDirectory(string sourceDirName, string destDirName)
        {
            //コピー先のディレクトリがないときは作る
            if (!Directory.Exists(destDirName))
            {
                Directory.CreateDirectory(destDirName);
                //属性もコピー
                File.SetAttributes(destDirName, File.GetAttributes(sourceDirName));
            }

            //コピー先のディレクトリ名の末尾に"\"をつける
            if (destDirName[destDirName.Length - 1] != Path.DirectorySeparatorChar)
            {
                destDirName = destDirName + Path.DirectorySeparatorChar;
            }

            //コピー元のディレクトリにあるファイルをコピー
            string[] files = Directory.GetFiles(sourceDirName);
            foreach (string file in files)
            {
                File.Copy(file, destDirName + Path.GetFileName(file), true);
            }

            //コピー元のディレクトリにあるディレクトリについて、再帰的に呼び出す
            string[] dirs = Directory.GetDirectories(sourceDirName);
            foreach (string dir in dirs)
            {
                CopyDirectory(dir, destDirName + Path.GetFileName(dir));
            }
        }

        /// <summary>
        /// 拡張子が.dcmのファイルのみを取得
        /// </summary>
        /// <param name="folder"></param>
        /// <returns></returns>
        private string[] MonitorTemporaryDirectory(string folder)
        {
            string extention = Path.GetExtension(filePattern);

            //監視対象フォルダにある、指定した拡張子で終わるファイルを取得。
            //"*.dcm"と指定したとき、"*.dcmabc"は取得しない。
            return Directory.GetFiles(folder, filePattern).
                Where(x => x.EndsWith(extention, StringComparison.OrdinalIgnoreCase)).ToArray();
        }
    }
}