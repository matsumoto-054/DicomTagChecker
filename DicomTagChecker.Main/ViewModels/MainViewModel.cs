using DicomTagChecker.Main.Models;
using Prism.Mvvm;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System.ComponentModel;
using System.Data;

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

        public ReactiveProperty<string> FolderPath { get; }
        public ReactiveCommand SelectFolder { get; }

        public ReactiveCommand Start { get; }
        public ReactiveCommand Cancel { get; }
        public ReactiveCommand Close { get; }

        public ReactiveProperty<string> StatusBar { get; }

        private readonly FolderPathModel _folderPathModel;
        private readonly StatusBarModel _statusBarModel;


        public MainViewModel(FolderPathModel folderPathModel, StatusBarModel statusBarModel)
        {
            this._folderPathModel = folderPathModel;
            this._statusBarModel = statusBarModel;

            this.StatusBar = this._statusBarModel
                .ObserveProperty(m => m.Status)
                .ToReactiveProperty();

            this.Start = new ReactiveCommand()
                .WithSubscribe(() => this.StartWorking());
        }

        private void StartWorking()
        {
            this._statusBarModel.ChangeStatus("取込中");
        }
    }
}
