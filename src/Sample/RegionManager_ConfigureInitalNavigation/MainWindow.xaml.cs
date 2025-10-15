using RegionManager_ConfigureInitalNavigation.ViewModels;
using System.Windows;

namespace RegionManager_ConfigureInitalNavigation
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow(MainViewModel vm)
        {
            InitializeComponent ();

            this.DataContext = vm;
        }
    }
}