# LazyRegion - CLAUDE.md

## Project Overview

**UI animation transition control library** for WPF / .NET MAUI.
Provides the `LazyRegion` control responsible for animations during Content ↔ Content transitions, distributed as NuGet packages.

- Author: lukewire (GitHub: XAMLBaker)
- NuGet: `LazyRegion`, `LazyRegion.WPF`, `LazyRegion.MAUI`

## Architecture Documentation

For a detailed understanding of the LazyRegion project's architecture, refer to the [LazyRegion_Architecture.md](docs/LazyRegion_Architecture.md) document. This document provides an overview of the project's structure, implemented components, planned features, and platform-specific differences.

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
│       ├── MauiTabSample/
│       ├── RegionManager_1/
│       ├── RegionManager_2/
│       ├── RegionManager_New/
│       ├── RegionManager_ConfigureInitalNavigation/
│       ├── RegionManager_InitialFlow_Then/
│       ├── RegionManager_RegionState/
│       ├── SampleScreen/
│       └── StaticSample/
├── LazyRegion.sln
└── LazyRegion_Build.sln
```

---

## Tech Stack

| Item              | Detail                                           |
| ----------------- | ------------------------------------------------ |
| Language          | C# 12, Nullable enable                           |
| Target Frameworks | netstandard2.0/2.1, net48, net6~9                |
| UI Frameworks     | WPF (Windows), .NET MAUI (cross-platform)        |
| DI Container      | `Microsoft.Extensions.DependencyInjection` 9.0.x |
| Distribution      | NuGet                                            |
| CI/CD             | GitHub Actions (`.github/workflows/publish.yml`) |

---

## Key Files

| File                                       | Role                                   |
| ------------------------------------------ | -------------------------------------- |
| `src/LazyRegion.Core/LazyRegionBuilder.cs` | Core builder pattern entry point       |
| `src/LazyRegion.Core/LazyRegionManager.cs` | Core region management class           |
| `src/LazyRegion.Core/LazyRegionApp.cs`     | Static entry point singleton           |
| `src/LazyRegion.Core/Interfaces/`          | Core interfaces                        |
| `src/LazyRegion.WPF/LazyRegionControl.cs`  | WPF custom control                     |
| `src/LazyRegion.WPF/LazyStage.cs`          | WPF Stage component                    |
| `src/LazyRegion.WPF/Themes/Generic.xaml`   | WPF default theme                      |
| `src/LazyRegion.Maui/LazyRegionControl.cs` | MAUI custom control                    |
| `src/LazyRegion.Maui/LazyStage.cs`         | MAUI Stage component                   |
| `src/LazyRegion.Maui/LazyTabRegion.cs`     | MAUI tab navigation control            |
| `src/LazyRegion.Maui/LazyTabItem.cs`       | Tab item key descriptor                |
| `src/Sample/*/App.xaml.cs`                 | Sample app entry points                |
| `src/Sample/*/Program.cs`                  | Explicit entry points for some samples |

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

## Implemented Features

| Feature                 | Description                                                                       |
| ----------------------- | --------------------------------------------------------------------------------- |
| `LazyRegionApp.Default` | Static entry point usable without DI. Shares the same instance as the DI path     |
| `LazyTabRegion`         | MAUI-only tab navigation control with swipe gesture and custom NavigationBar slot |
| `LazyTabItem`           | Lightweight tab descriptor for XAML Key-based view registration                   |

---

## Sample Projects

| Project                                   | Platform | Demonstrates                                                              |
| ----------------------------------------- | -------- | ------------------------------------------------------------------------- |
| `CodeBehind`                              | WPF      | Direct `LazyStage.Content` switching from code-behind, no DI or ViewModel |
| `DataTemplateChange`                      | WPF      | ViewModel binding with automatic DataTemplate resolution by type          |
| `RegionManager_1`                         | WPF      | Basic DI setup — `ILazyRegionManager` injected into the View              |
| `RegionManager_2`                         | WPF      | `ILazyRegionManager` injected into a ViewModel                            |
| `RegionManager_New`                       | WPF      | Minimal DI setup (baseline)                                               |
| `RegionManager_ConfigureInitalNavigation` | WPF      | `WithInitialFlow()` — auto-show first view on startup                     |
| `RegionManager_InitialFlow_Then`          | WPF      | Multi-step conditional initial flow with `.Then()`                        |
| `RegionManager_RegionState`               | WPF      | `WithLoadingBehavior()` — loading/error view state management             |
| `LazyRegionItemsControlSample`            | WPF      | Multiple region management with ItemsControl                              |
| `StaticSample`                            | WPF      | View registration using `LazyRegionApp.Default` without DI                |
| `SampleScreen`                            | Shared   | Shared ScreenA/B, LoadingView, ErrorView components used across samples   |
| `MauiSample`                              | MAUI     | DI-based `ILazyRegionManager` + `UseLazyRegion()` basics                  |
| `MauiTabSample`                           | MAUI     | `LazyTabRegion` + `LazyRegionApp.Default` tab navigation                  |

---

## ⚠️ To Verify

- Check whether `CancellationToken` is wired up inside Region Timeout waiting logic
- If not, a pending Task may prevent the app from shutting down cleanly
