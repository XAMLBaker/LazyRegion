using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LazyRegion.Core;

namespace RegionManager_ConfigureInitalNavigation.ViewModels;

public partial class MainViewModel : ObservableObject
{
    [ObservableProperty] TransitionAnimation selectAnimationType;

    [ObservableProperty] List<TransitionAnimation> transitionAnimations = new ()
                                                                        {
                                                                            TransitionAnimation.None,
                                                                            TransitionAnimation.Fade,
                                                                            TransitionAnimation.NewFromLeft,
                                                                            TransitionAnimation.NewFromRight,
                                                                            TransitionAnimation.NewFromUp,
                                                                            TransitionAnimation.NewFromDown,
                                                                            TransitionAnimation.SlideLeft,
                                                                            TransitionAnimation.SlideRight,
                                                                            TransitionAnimation.SlideUp,
                                                                            TransitionAnimation.SlideDown,
                                                                            TransitionAnimation.Scale,
                                                                            TransitionAnimation.ZoomIn,
                                                                            TransitionAnimation.ZoomOut
                                                                        };
    private readonly ILazyRegionManager _lazyRegionManager;

    private string[] views = new string[] { "a", "b" };
    public MainViewModel(ILazyRegionManager lazyRegionManager)
    {
        this._lazyRegionManager = lazyRegionManager;
        
        //Run ();
    }

    int idx = 1;

    [RelayCommand]
    private void Go()
    {
        if (idx == 2)
            idx = 0;

        Run ();
    }

    private void Run()
    {
        this._lazyRegionManager.NavigateAsync ("Root", views[idx++]);
    }
}
