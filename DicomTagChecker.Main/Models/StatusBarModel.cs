using Prism.Mvvm;

namespace DicomTagChecker.Main.Models
{
    public class StatusBarModel : BindableBase
    {
        public string _status = "準備完了";

        public string Status
        {
            get { return _status; }
            set { SetProperty(ref _status, value); }
        }

        public StatusBarModel()
        {
        }

        public void ChangeStatus(string status)
        {
            this.Status = status;
        }
    }
}