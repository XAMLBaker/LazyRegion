using LazyRegion.Core;
using LazyRegion.WinUI3;
using Microsoft.UI.Xaml;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace LazyRegionWinUI3Sample
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public partial class App : Application
    {
        private Window? _window;

        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            InitializeComponent ();

            LazyRegionApp.Default.UseWinUI3 (lazy =>
                {
                    lazy.Register<AControl> ("A");
                    lazy.Register<BControl> ("B");
                    lazy.Register<CControl> ("C");

                    lazy.Configure (configure =>
                    {
                        configure.ForRegion ("MainRegion")
                                .WithInitialFlow(flow =>
                                {
                                    flow.Show ("A");
                                });
                    });
                });
        }

        /// <summary>
        /// Invoked when the application is launched.
        /// </summary>
        /// <param name="args">Details about the launch request and process.</param>
        protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
        {
            _window = new MainWindow ();
            _window.Activate ();
        }
    }
}
