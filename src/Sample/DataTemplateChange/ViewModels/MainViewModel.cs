using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LazyRegion.Core;

namespace DataTemplateChange.ViewModels;

public partial class AViewModel : ObservableObject
{

}

public partial class BViewModel : ObservableObject
{

}

public partial class MainViewModel : ObservableObject
{
    [ObservableProperty] object currentViewModel;
    [ObservableProperty] TransitionAnimation selectAnimationType;

    [ObservableProperty] List<TransitionAnimation> transitionAnimations =new()
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
    private List<ObservableObject> views = new ();

    public MainViewModel()
    {
        views.Add (new AViewModel ());
        views.Add (new BViewModel ());

        Run ();
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
        this.CurrentViewModel = views[idx++];
    }
}