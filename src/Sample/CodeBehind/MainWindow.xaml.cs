using SampleScreen.Base;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace CodeBehind
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private FrameworkElement[] views = new FrameworkElement[] { new ScreenA (), new ScreenB () };
        int idx = 0;
        public MainWindow()
        {
            InitializeComponent ();
            Run ();

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (idx == 2)
                idx = 0;

            Run ();
        }

        private void RadioButton_Checked(object sender, RoutedEventArgs e)
        {

        }
        private void Run()
        {
            this.region.Content = views[idx++];
        }

    }
}