# LazyRegion - CLAUDE.md

## Project Overview

**UI animation transition control library** for WPF / .NET MAUI.
Provides the `LazyRegion` control responsible for animations during Content ↔ Content transitions, distributed as NuGet packages.

- Author: lukewire (GitHub: XAMLBaker)
- NuGet: `LazyRegion`, `LazyRegion.WPF`, `LazyRegion.MAUI`

---

## Directory Structure

```
LazyRegion/
├── src/
│   ├── LazyRegion.Core/          # Platform-agnostic core logic
│   ├── LazyRegion.WPF/           # WPF-specific controls
│   ├── LazyRegion.Maui/          # .NET MAUI-specific controls
│   └── Sample/                   # Example projects
│       ├── CodeBehind/
│       ├── DataTemplateChange/
│       ├── LazyRegionItemsControlSample/
│       ├── MauiSample/
│       ├── RegionManager_1/
│       ├── RegionManager_2/
│       ├── RegionManager_New/
│       ├── RegionManager_ConfigureInitalNavigation/
│       ├── RegionManager_InitialFlow_Then/
│       ├── RegionManager_RegionState/
│       └── SampleScreen/
├── LazyRegion.sln
└── LazyRegion_Build.sln
```

---

## Tech Stack

| Item | Detail |
|---|---|
| Language | C# 12, Nullable enable |
| Target Frameworks | netstandard2.0/2.1, net48, net6~9 |
| UI Frameworks | WPF (Windows), .NET MAUI (cross-platform) |
| DI Container | `Microsoft.Extensions.DependencyInjection` 9.0.x |
| Distribution | NuGet |
| CI/CD | GitHub Actions (`.github/workflows/publish.yml`) |

---

## Key Files

| File | Role |
|---|---|
| `src/LazyRegion.Core/LazyRegionBuilder.cs` | Core builder pattern entry point |
| `src/LazyRegion.Core/LazyRegionManager.cs` | Core region management class |
| `src/LazyRegion.Core/Interfaces/` | Core interfaces |
| `src/LazyRegion.WPF/LazyRegionControl.cs` | WPF custom control |
| `src/LazyRegion.WPF/LazyStage.cs` | WPF Stage component |
| `src/LazyRegion.WPF/Themes/Generic.xaml` | WPF default theme |
| `src/LazyRegion.Maui/LazyRegionControl.cs` | MAUI custom control |
| `src/LazyRegion.Maui/LazyStage.cs` | MAUI Stage component |
| `src/Sample/*/App.xaml.cs` | Sample app entry points |
| `src/Sample/*/Program.cs` | Explicit entry points for some samples |

---

## Project Dependencies

```
LazyRegion.WPF  ──┐
                  ├──▶  LazyRegion.Core
LazyRegion.Maui ──┘
```

- Debug: `ProjectReference` direct reference
- Release: Core bundled into package (`PrivateAssets="all"`)

---

## Roadmap

### Priority 1: Static Entry Point (LazyRegionApp.Default)

#### Background
- WPF has no built-in DI — requires manual setup
- Constructor DI is impossible in CustomControl environments
- Goal: lower the barrier to entry across more usage scenarios

#### Design
```csharp
// Initialization (App.xaml.cs)
LazyRegionApp.Default
    .Register<HomeView>("home")
    .Register<LoginView>("login")
    .Configure(config => {
        config.ForRegion("Root")
              .WithInitialFlow(flow => flow.Show("home"));
    });

// ViewModel → DI (existing, unchanged)
public MainViewModel(ILazyRegionManager regionManager) { }

// CustomControl / no-DI environment → Static
LazyRegionApp.Default.RegionManager.NavigateAsync("Root", "home");
```

#### Decisions
- DI and Static modes **coexist** — both supported
- Both share the **same internal instance** (Static is just a different access path)
- Accessing before initialization throws a clear exception:
  ```csharp
  throw new InvalidOperationException(
      "LazyRegionApp is not initialized. Call Default.Use... first.");
  ```
- View ↔ ViewModel binding is the developer's responsibility
- Thread safety handled via existing `Dispatcher.InvokeAsync` pattern

#### ⚠️ To Verify
- Check whether `CancellationToken` is wired up inside Region Timeout waiting logic
- If not, a pending Task may prevent the app from shutting down cleanly

---

### Priority 2: MAUI LazyTabRegion

#### Background
- Needed for tab-based navigation with a flexible, user-defined NavigationBar
- Too complex to add to existing `LazyRegionControl` → **separate control**

#### Control Structure
```xml
<LazyTabRegion TransitionAnimation="SlideLeft" SwipeThreshold="0.5">
    <LazyTabRegion.NavigationBar>
        <TabNavigationBar VerticalOptions="End"/>
        <!-- Position and style fully defined by the user -->
    </LazyTabRegion.NavigationBar>
    <!-- Main content area -->
</LazyTabRegion>
```

#### Decisions
- `TabNavigationBar` is a separate control; position is set by the user
- Reuses existing `TransitionAnimation` enum as-is
- **Interactive transitions** (swipe gesture)
  - Auto-enabled when a Slide-family animation is selected
  - Fade / Scale family: tap only, no swipe
  - `SwipeThreshold` default `0.5` (50%), user-configurable
  - On cancel: smooth return to original position (fixed, not configurable)
