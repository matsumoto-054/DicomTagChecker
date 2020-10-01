using Prism.Ioc;
using Prism.Regions;
using System.Windows;
using Unity;

namespace DicomTagChecker.Main.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        [Dependency]
        public IContainerExtension ContainerExtention { get; set; }

        [Dependency]
        public IRegionManager RegionManager { get; set; }

        public MainWindow()
        {
            InitializeComponent();
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            RegionManager.AddToRegion("SelectDirectoryRegion", ContainerExtention.Resolve<SelectDirectory>());
            RegionManager.AddToRegion("OperationRegion", ContainerExtention.Resolve<Operation>());
            RegionManager.AddToRegion("StatusBarRegion", ContainerExtention.Resolve<StatusBar>());
        }
    }
}