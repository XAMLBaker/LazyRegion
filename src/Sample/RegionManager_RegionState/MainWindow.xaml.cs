using RegionManager_RegionState.ViewModels;
using System.Windows;

namespace RegionManager_RegionState
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