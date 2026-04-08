# LazyRegion
[한국어 버전 보기 (View Korean version)](README.ko.md)

**WPF**
[![NuGet](https://img.shields.io/nuget/v/LazyRegion.WPF.svg)](https://www.nuget.org/packages/LazyRegion.WPF/) 
[![NuGet Downloads](https://img.shields.io/nuget/dt/LazyRegion.WPF.svg)](https://www.nuget.org/packages/LazyRegion.WPF/)

**MAUI**
[![NuGet](https://img.shields.io/nuget/v/LazyRegion.MAUI.svg)](https://www.nuget.org/packages/LazyRegion.MAUI/) 
[![NuGet Downloads](https://img.shields.io/nuget/dt/LazyRegion.MAUI.svg)](https://www.nuget.org/packages/LazyRegion.MAUI/)

**WinUI3**
[![NuGet](https://img.shields.io/nuget/v/LazyRegion.WinUI3.svg)](https://www.nuget.org/packages/LazyRegion.WinUI3/) 
[![NuGet Downloads](https://img.shields.io/nuget/dt/LazyRegion.WinUI3.svg)](https://www.nuget.org/packages/LazyRegion.WinUI3/)

**LazyRegion** is a region library for screen transitions that supports smooth animations and the MVVM pattern.
It can be used in WPF, .NET MAUI, and WinUI3 environments, providing natural, region-based screen transitions and state management.

---

## ✨ Key Features

- 🎞 **Smooth Screen Transitions**: Applies natural animations based on the selected effect.
- 🧩 **Full MVVM Support**: Automatically transitions screens just by changing a ViewModel.
- 💻 **Code-Behind Support**: Handles transitions identically even when changing `Content` directly in code.
- 📦 **RegionManager Integration**: Dynamically switch views in a specified region with ease.
- 🌍 **Multi-Platform Support**: Use it in **WPF**, **.NET MAUI**, and **WinUI3**.
- 🔌 **Static Entry Point**: Use `LazyRegionApp.Default` for immediate access without DI.
- 🗂 **Tab Navigation**: `LazyTabRegion` supports swipe gesture-based tab transitions (MAUI).
- 🔔 **ViewModel Lifecycle Hooks**: Receive transition events and pass parameters with `ILazyNavigationAware`.
- 🛡 **Navigation Guard**: Cancel a transition before it starts with `ILazyNavigationGuard`.
- 🎬 **Per-Navigation Animation Override**: Specify a different animation for each navigation.
- ⏪ **GoBack**: Return to the previous screen with a reverse animation.
- 🔗 **Region Group Navigation**: Transition multiple regions simultaneously.

---

## 📦 Installation

Install the package for your desired platform from NuGet.

**WPF:**
```powershell
dotnet add package LazyRegion.WPF
```
**MAUI:**
```powershell
dotnet add package LazyRegion.MAUI
```
**WinUI3:**
```powershell
dotnet add package LazyRegion.WinUI3
```

---

## 🚀 Quick Start

### 1. DataTemplate-Based Example
Transition screens by simply changing a ViewModel property.

```xml
<!-- Add namespace -->
xmlns:lr="clr-namespace:LazyRegion.WPF;assembly=LazyRegion.WPF"
<!-- or for MAUI/WinUI3 -->
xmlns:lr="clr-namespace:LazyRegion.MAUI;assembly=LazyRegion.MAUI"
xmlns:lr="using:LazyRegion.WinUI3"

<!-- Use LazyRegion -->
<lr:LazyRegion Content="{Binding CurrentPage}" TransitionAnimation="Fade"/>
```
When you change the `CurrentPage` property in your ViewModel, the screen will transition with a fade animation.

### 2. Code-Behind Example
Set the `Content` property directly to transition screens.

```csharp
var region = new LazyRegion
{
    Content = new HomeView()
};

// Automatically transitions to DetailView with an animation.
region.Content = new DetailView();
```

### 3. RegionManager Example
Use `RegionManager` to replace the view in a named region.

```xml
<lr:LazyRegion RegionName="MainRegion" TransitionAnimation="Fade"/>
```
```csharp
// Navigates the region named "MainRegion" to HomeView.
RegionManager.RequestNavigate("MainRegion", new HomeView());
```

---

## ⚙️ Advanced Features

<details>
<summary>🔹 Initial Navigation and Region Settings (Initial Flow, Timeout, State)</summary>

### Initial Navigation Flow
Define an initial flow to automatically navigate to a specified view when the app starts or a region is first activated.

```csharp
.UseLazyRegion(lazy =>
{
    lazy.Register<SplashView>("Splash");
    lazy.ConfigureRegions(config =>
    {
        config.ForRegion("Root")
              .WithInitialFlow(flow =>
              {
                  flow.Show("Splash"); // Show Splash screen on app start
              });
    });
});
```

### Conditional Flow Control
Use `Then` and `Then<TService>` to display different views based on conditions, useful for checks like login status.

```csharp
.WithInitialFlow(flow =>
{
    flow.Show("Splash")
        .Then<ILoginService>("Main", async service => await service.IsLoggedInAsync()) // If logged in, go to Main
        .Then("Login"); // Otherwise, go to Login
});
```

### Region Timeout
If a navigation request is made for a region that is not yet registered, it will wait for a specified duration before automatically canceling the request.

```csharp
// If the "Root" region is not registered within 30 seconds, the navigation is canceled.
regionManager.NavigateAsync("Root", "a", TimeSpan.FromSeconds(30));
```

### Region State Management (Loading / Error)
Automatically display a `Loading` view while waiting for navigation and an `Error` view on timeout.

```csharp
.ConfigureRegions(config =>
{
    config.ForRegion("Root")
          .WithLoadingBehavior(state =>
          {
              state.Loading("LoadingViewKey") // View to show while loading
                   .MinDisplayTime(TimeSpan.FromSeconds(1)); // Minimum display time

              state.Error("ErrorViewKey") // View to show on error
                   .Timeout(TimeSpan.FromSeconds(10)); // Timeout after 10 seconds
          });
});
```
</details>

<details>
<summary>🔹 ViewModel-Centric Navigation (Lifecycle, Parameters, Guard)</summary>

### ViewModel Lifecycle Hooks (ILazyNavigationAware)
If a ViewModel implements the `ILazyNavigationAware` interface, it can receive events before and after navigation.

```csharp
public class HomeViewModel : ILazyNavigationAware
{
    // Called when navigation to this view is complete.
    public void OnNavigatedTo(LazyNavigationContext context)
    {
        // Receive parameters, load data, etc.
    }

    // Called just before navigating away from this view.
    public void OnNavigatedFrom(LazyNavigationContext context)
    {
        // Clean up resources, unsubscribe from events, etc.
    }
}
```

### Navigation Parameters (LazyNavigationParameters)
Pass data to a ViewModel during navigation.

```csharp
// Pass parameters when calling navigation
await regionManager.NavigateAsync(
    "MainRegion",
    "Detail",
    new LazyNavigationParameters { { "OrderId", 42 } }
);

// Receive parameters in the ViewModel
public void OnNavigatedTo(LazyNavigationContext context)
{
    var orderId = context.Parameters.GetValue<int>("OrderId");
}
```

### Navigation Guard (ILazyNavigationGuard)
Implement `ILazyNavigationGuard` to conditionally cancel a navigation. This is useful for asking for user confirmation when there are unsaved changes.

```csharp
public class EditViewModel : ILazyNavigationGuard
{
    public async Task<bool> CanNavigateAsync(LazyNavigationContext context)
    {
        if (HasUnsavedChanges)
        {
            // Returning false cancels the navigation.
            return await ShowConfirmDialogAsync("You have unsaved changes. Are you sure you want to leave?");
        }
        return true; // Returning true allows the navigation to proceed.
    }
}
```
</details>

<details>
<summary>🔹 Advanced Navigation Control (GoBack, Animation Override, Group)</summary>

### GoBack
Return to the previous screen with a reverse animation.

```csharp
if (regionManager.CanGoBack("MainRegion"))
{
    await regionManager.GoBackAsync("MainRegion");
}
```
- Animations are automatically mapped, e.g., `SlideLeft` ↔ `SlideRight`, `ZoomIn` ↔ `ZoomOut`.
- The stack is kept simple with a single depth (depth=1).

### Per-Navigation Animation Override
Specify a different animation for a single navigation call.

```csharp
// Execute this transition with a SlideLeft animation
await regionManager.NavigateAsync("MainRegion", "Detail", TransitionAnimation.SlideLeft);
```

### Region Group Navigation
Transition multiple regions simultaneously with a single call. You can apply different animations to each region.

```csharp
await regionManager.NavigateGroupAsync(
    ("HeaderRegion", "DetailHeader", TransitionAnimation.Fade),
    ("ContentRegion", "DetailContent", TransitionAnimation.SlideLeft),
    ("SidebarRegion", "DetailSidebar", null) // null uses the default animation
);
```
</details>

<details>
<summary>🔹 Platform-Specific Features & More</summary>

### LazyTabRegion (MAUI Only)
A custom tab control that supports swipe gestures.

```xml
<lz:LazyTabRegion SwipeThreshold="0.4">
    <lz:LazyTabRegion.NavigationBar>
        <!-- Custom tab bar UI -->
    </lz:LazyTabRegion.NavigationBar>

    <lz:LazyTabItem Key="home"/>
    <lz:LazyTabItem Key="search"/>
</lz:LazyTabRegion>
```
- Swiping is automatically enabled when using `Slide` family animations.
- The `Key` matches a view registered with `LazyRegionApp.Default.Register<HomeView>("home")`.

### Static Entry Point (LazyRegionApp.Default)
Use `LazyRegion`'s features without a DI container. This is useful in environments where constructor injection is difficult, like WPF's `CustomControl`.

```csharp
// Register views on app startup
LazyRegionApp.Default
    .UseWpf() // or .UseMaui() / .UseWinUI3()
    .Register<HomeView>("home");

// Access RegionManager from a ViewModel or control
LazyRegionApp.Default.RegionManager.NavigateAsync("Root", "home");
```
If used with `UseLazyRegion()`, registered information is automatically integrated with the DI container.
</details>

---

### Summary

- **Initial Flow**: Declaratively configure app startup scenarios.
- **ViewModel Hooks**: Control MVVM navigation with `ILazyNavigationAware` and `ILazyNavigationGuard`.
- **Advanced Navigation**: Manage complex UI flows with `GoBack`, animation overrides, and group navigation.
- **Static Access**: Access all features without DI using `LazyRegionApp.Default`.
