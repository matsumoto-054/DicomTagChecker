using Prism.Ioc;
using DicomTagChecker.Main.Views;
using System.Windows;
using Prism.Unity;

namespace DicomTagChecker.Main
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : PrismApplication
    {
        protected override Window CreateShell()
        {
            return Container.Resolve<MainWindow>();
        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {

        }
    }
}
