using LazyRegion.Core;

namespace MauiSample
{
    public partial class MainPage : ContentPage
    {
        private readonly ILazyRegionManager _regionManager;

        public MainPage(ILazyRegionManager regionManager)
        {
            InitializeComponent ();
            this._regionManager = regionManager;

            this.piu.SelectedIndex = 0;
        }

        private int idx = 1;
        private string[] viewkeys = new string[] { "A", "B" };
        private void Button_Clicked(object sender, EventArgs e)
        {
            if (idx == 2)
                idx = 0;
            this._regionManager.NavigateAsync ("Root", viewkeys[idx++]);
        }

        private void Picker_SelectedIndexChanged(object sender, EventArgs e)
        {
            var name = ((Picker)sender).SelectedItem;

            TransitionAnimation MyStatus = (TransitionAnimation)Enum.Parse (typeof (TransitionAnimation), name.ToString (), true);
            region.TransitionAnimation = MyStatus;
        }
    }
}
