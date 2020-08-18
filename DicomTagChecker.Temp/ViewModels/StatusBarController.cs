namespace DicomTagChecker.Temp
{
    public class StatusBarController
    {
        public string ChangeStatusBar(bool isReading)
        {
            string message = isReading ? "取込中" : "準備完了";

            return message;
        }
    }
}