using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LazyRegion.Core;
using LazyRegion.WPF;
using LazyRegionItemsControlSample;
using LinqUI.WPF;
using SampleScreen.Base;
using System.Windows;
using System.Windows.Controls;

var builder = Host.CreateApplicationBuilder ();

builder.Services.AddSingleton<MainViewModel> ();
builder.Services.UseLazyRegion (Lazy =>
{
    Lazy.Register<ScreenA> ("a");
    Lazy.Register<ScreenB> ("b");
    Lazy.Register<ScreenC> ("c");
    Lazy.Register<ScreenD> ("d");
});

var app = builder.BuildApp<App, MainWindow> ();  // 🔥

app.OnStartUpAsync = async provider =>
{

};
// Exit 시 정리
app.OnExitAsync = async provider =>
{
    Console.WriteLine ("앱 종료 중...");
    await Task.Delay (200);
};

app.Run ();

public class MainWindow : Window
{
    public MainWindow (MainViewModel vm)
    {
        this.DataContext = vm;

        this.Content = new Grid ()
                            .Children (
                                new StackPanel ()
                                    .Children (
                                        new Button ()
                                        {
                                            Content = "Add Screen All"
                                        }
                                        .Command (static (MainViewModel vm) => vm.AddScreenACommand),
                                        new Button ()
                                        {
                                            Content = "Remove Screen B"
                                        }
                                        .Command (static (MainViewModel vm) => vm.AddScreenBCommand),
                                         new Button ()
                                         {
                                             Content = "Insert Screen C"
                                         }
                                        .Command (static (MainViewModel vm) => vm.AddScreenCCommand)
                                    ),
                                new Grid()
                                    .Children (
                                        new LazyBoard ()
                                        {
                                            RegionName = "MainRegion",       
                                            TransitionAnimation = TransitionAnimation.ZoomIn
                                        }
                                        .Bind(x=>x.ItemRemoveCommand, static (MainViewModel vm) => vm.AddRootReginCommand, System.Windows.Data.BindingMode.OneWay)
                                    )
                                    .Row(1),
                                new LazyStage ()
                                {
                                    RegionName = "RootRegion"
                                }
                                .Row(2)
                            )
                            .Rows("auto,auto,*");
    }
}

public partial class MainViewModel : ObservableObject
{
    private readonly ILazyRegionManager _manager;

    public MainViewModel(ILazyRegionManager manager)
    {
        this._manager = manager;
    }

    [RelayCommand]
    private async Task AddScreenA()
    {
        await _manager.AddItemsAsync ("MainRegion", new string[]
        {
            "a","b","c","d"
        });
    }

    [RelayCommand]
    private async Task AddScreenB()
    {
        await _manager.RemoveItemAsync ("MainRegion", "b");
    }
    [RelayCommand]
    private async Task AddScreenC()
    {
        await _manager.InsertItemAsync ("MainRegion", 1, "c");
    }

    [RelayCommand]
    private async Task AddRootRegin(string viewKey)
    {
        await _manager.NavigateAsync ("RootRegion", viewKey);
    }
}