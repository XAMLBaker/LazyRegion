using LazyRegion.Core;

namespace MauiInteractiveRegionSample
{
    public partial class App : Application
    {
        private readonly ILazyRegionManager _regionManager;

        public App(ILazyRegionManager regionManager)
        {
            InitializeComponent ();
            this._regionManager = regionManager;
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            return new Window (new MainPage (this._regionManager));
        }
    }
}