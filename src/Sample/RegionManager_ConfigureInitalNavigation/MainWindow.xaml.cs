using RegionManager_InitialFlow.ViewModels;
using System.Windows;

namespace RegionManager_InitialFlow
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