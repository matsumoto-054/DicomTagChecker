using Prism.Mvvm;
using Reactive.Bindings;

namespace DicomTagChecker.Main.ViewModels
{
    public class MainFormViewModel : BindableBase
    {
        private string _title = "DICOMタグチェッカー";
        public string Title
        {
            get { return _title; }
            set { SetProperty(ref _title, value); }
        }

        public ReactiveProperty<string> FolderPath { get; }
        public ReactiveCommand SelectPathButton { get; }
        public ReactiveCommand StartButton { get; }
        public ReactiveCommand CancelButton { get; }
        public ReactiveCommand CloseButton { get; }
        public ReactiveProperty<string> StatusBarText { get; }

        public MainFormViewModel()
        {

        }
    }
}
