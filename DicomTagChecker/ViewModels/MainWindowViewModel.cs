using DicomTagChecker.Main.Models;
using Prism.Mvvm;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;

namespace DicomTagChecker.ViewModels
{
    public class MainWindowViewModel : BindableBase
    {
        private readonly StatusBarModel _statusBarModel;

        public ReactiveProperty<string> StatusBar { get; }

        public MainWindowViewModel(StatusBarModel statusBarModel)
        {
            this._statusBarModel = statusBarModel;

            this.StatusBar = this._statusBarModel
                .ObserveProperty(m => m.Status)
                .ToReactiveProperty();
        }
    }
}
