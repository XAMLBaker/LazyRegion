using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LazyRegion.Core;
using System.Security.Policy;
using System.Windows.Controls;

namespace DataTemplateChange.ViewModels;

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

    public MainViewModel()
    {
        this.CurrentViewModel = Activator.CreateInstance (views[idx++]);
    }
    private Type[] views = new Type[] { typeof(AViewModel), typeof (BViewModel)};
    int idx = 0;
    [RelayCommand]
    private void Go()
    {
        if (idx == 2)
            idx = 0;
        this.CurrentViewModel = Activator.CreateInstance(views[idx++]);
    }
}

public partial class AViewModel : ObservableObject
{

}

public partial class BViewModel : ObservableObject
{

}
