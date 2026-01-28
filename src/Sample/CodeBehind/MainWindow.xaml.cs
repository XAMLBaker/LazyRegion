using LazyRegion.Core;
using SampleScreen.Base;
using System.Windows;
using System.Windows.Controls;

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
            RadioButton rb = (RadioButton)sender;
            if (rb.IsChecked == false)
                return;
            TransitionAnimation MyStatus = (TransitionAnimation)Enum.Parse (typeof (TransitionAnimation), rb.Content.ToString (), true);
            stage.TransitionAnimation = MyStatus;
        }
        private void Run()
        {
            this.stage.Content = views[idx++];
        }
    }
}