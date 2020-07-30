using System;
using System.Collections.ObjectModel;

namespace DicomTagChecker.Temp
{
    public class LogWriter
    {
        private ObservableCollection<LogContents> logContents = new ObservableCollection<LogContents>();

        LogContents log = new LogContents();

        public ObservableCollection<LogContents> WriteLog()
        {

            log.Date = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");
            log.Status = "OK";
            log.Contents = "ButtonClick";

            logContents.Add(log);

            return logContents;
        }
    }
}