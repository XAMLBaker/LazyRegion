using LazyRegion.Core;
using LazyRegion.Maui.ReactorMaui;

namespace MauiReactorSample.Components;

internal class HomePageState
{
    public int Counter { get; set; }
    public string ContentName { get; set; } = "A";
}

partial class HomePage : Component<HomePageState>
{
    private int idx = 1;
    private string[] viewkeys = new string[] { "A", "B" };
    [Inject] ILazyRegionManager _regionManager;
    public override VisualNode Render()
        => ContentPage (
            //Grid(
            //    //new LazyRegionView ()
            //    //    .RegionName("Root")
            //    //    .RegionContent(State.ContentName),

            //    Button()
            //        .GridColumn(1)
            //        .OnClicked (() =>
            //        {
            //            if (idx == 2)
            //                idx = 0;
            //            SetState (x => x.ContentName = viewkeys[idx++]);
            //        })
            //)
            //.Columns("*, auto")
        );
}
