using Prism.Mvvm;

namespace DicomTagChecker.Main.ViewModels
{
    public class MainViewModel : BindableBase
    {
        private string _title = "DICOMタグチェッカー";

        public string Title
        {
            get { return _title; }
            set { SetProperty(ref _title, value); }
        }

        public MainViewModel()
        {
        }
    }
}