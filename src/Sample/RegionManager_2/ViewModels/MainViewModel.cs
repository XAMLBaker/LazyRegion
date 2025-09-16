using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LazyRegion.Core;

namespace RegionManager_2.ViewModels;

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
        
        // 주의! 화면 생성되기도 전부터 화면을 그릴 수 없기에 사용할 수 없음
        //Run ();
    }

    int idx = 0;

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
