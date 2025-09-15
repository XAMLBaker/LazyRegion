using LazyRegion.Core;
using System.Windows;
using System.Windows.Controls;

namespace WpfApp1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly ILazyRegionManager _lazyRegionManager;

        public MainWindow(ILazyRegionManager lazyRegionManager)
        {
            InitializeComponent ();
            this._lazyRegionManager = lazyRegionManager;
            this._lazyRegionManager.NavigateAsync ("Root", "a");
        }

        private string[] views = new string[] { "a", "b" };
        int idx =0;

        
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (idx == 2)
                idx = 0;
            this._lazyRegionManager.NavigateAsync ("Root", views[idx++]);
        }

        private void RadioButton_Checked(object sender, RoutedEventArgs e)
        {
            RadioButton rb = (RadioButton)sender;
            if (rb.IsChecked == false)
                return;
            TransitionAnimation MyStatus = (TransitionAnimation)Enum.Parse (typeof (TransitionAnimation), rb.Content.ToString(), true);
            region.TransitionAnimation = MyStatus;
        }
    }
}