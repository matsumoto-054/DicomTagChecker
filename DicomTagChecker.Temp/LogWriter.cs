using System;
using System.Collections.ObjectModel;

namespace DicomTagChecker.Temp
{
    public class LogWriter
    {
        private ObservableCollection<LogContents> logContents = new ObservableCollection<LogContents>();

        LogContents log = new LogContents();

        public ObservableCollection<LogContents> WriteLog(string status, string contents)
        {

            log.Date = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");
            log.Status = status;
            log.Contents = contents;

            logContents.Add(log);

            return logContents;
        }
    }
}