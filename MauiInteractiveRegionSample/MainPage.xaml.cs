using LazyRegion.Core;
using MauiInteractiveRegionSample.Local.ViewModels;

namespace MauiInteractiveRegionSample
{
    public partial class MainPage : ContentPage
    {
        private readonly ILazyRegionManager _regionManager;

        public MainPage(ILazyRegionManager regionManager)
        {
            InitializeComponent ();

            this.BindingContext = new MainPageModel (regionManager);
            this._regionManager = regionManager;
        }
    }
}
