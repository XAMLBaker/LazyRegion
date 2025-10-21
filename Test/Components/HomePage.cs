using LazyRegion.Core;
using LazyRegion.Maui.ReactorMaui;

namespace Test.Components
{
    internal class HomePageState
    {
        public int Counter { get; set; }
    }

    partial class HomePage : Component<HomePageState>
    {
        private ILazyRegionManager _regionManger;

        public HomePage()
        {
            _regionManger = Services.GetRequiredService<ILazyRegionManager> ();
        }
        private int idx = 1;
        private string[] viewkeys = new string[] { "A", "B" };
        public override VisualNode Render()
            => ContentPage (
                    Grid (
                        new LazyRegionView ("Root"),

                        Button ()
                            .GridColumn (1)
                            .OnClicked (() =>
                            {
                                if (idx == 2)
                                    idx = 0;
                                _regionManger.NavigateAsync ("Root", viewkeys[idx++]);
                            })
                    )
                    .Columns ("*, auto")
            );
    }
}
