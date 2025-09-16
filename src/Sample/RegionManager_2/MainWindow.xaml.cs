using RegionManager_2.ViewModels;
using System.Windows;

namespace RegionManager_2
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