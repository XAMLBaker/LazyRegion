using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LazyRegion.Core;

namespace MauiInteractiveRegionSample.Local.ViewModels;

public partial  class MainPageModel : ObservableObject
{
    private readonly ILazyRegionManager _regionManager;

    public MainPageModel(ILazyRegionManager regionManager)
    {
        this._regionManager = regionManager;
    }
    private int idx = 1;
    private string[] viewkeys = new string[] { "A", "B" };
    [RelayCommand]
    private void Start()
    {
        if (idx == 2)
            idx = 0;
        this._regionManager.NavigateAsync("MainRegion", viewkeys[idx++]);
    }
}
