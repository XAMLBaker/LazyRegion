# LazyRegion Architecture

## Overview

This document describes the architecture of the LazyRegion project, clarifies which components are implemented in the repository, and lists planned or not-yet-implemented features. It also documents platform-specific differences (WPF vs MAUI) and runtime initialization recommendations.

## Implemented Components (confirmed in repo)

- LazyRegion.Core
  - LazyRegionBuilder — src/LazyRegion.Core/LazyRegionBuilder.cs
  - LazyRegionManager — src/LazyRegion.Core/LazyRegionManager.cs
  - LazyRegionApp — src/LazyRegion.Core/LazyRegionApp.cs
- LazyRegion.WPF
  - LazyRegionControl — src/LazyRegion.WPF/LazyRegionControl.cs
  - LazyStage — src/LazyRegion.WPF/LazyStage.cs
  - Themes/Generic.xaml — src/LazyRegion.WPF/Themes/Generic.xaml
- LazyRegion.Maui
  - LazyRegionControl — src/LazyRegion.Maui/LazyRegionControl.cs
  - LazyStage — src/LazyRegion.Maui/LazyStage.cs
  - LazyTabRegion / LazyTabItem — src/LazyRegion.Maui/LazyTabRegion.cs, src/LazyRegion.Maui/LazyTabItem.cs

Any other components referenced in older documents should be treated as planned or missing unless explicitly implemented above.

## Planned / Not Implemented (documented here for roadmap)

- LazyRegionAnimator
- LazyRegionNavigationService
- LazyRegionServiceLocator
- LazyRegionEventAggregator
- LazyRegionLogger
- LazyRegionCache
- LazyRegionFactory
- LazyRegionThemeManager
- LazyRegionLocalizationService
- LazyRegionErrorHandler
- LazyRegionPerformanceMonitor
- LazyRegionTestingFramework
- LazyRegionDocumentationGenerator
- LazyRegionCommunityHub
- LazyRegionPluginSystem
- LazyRegionAnalytics
- LazyRegionSecurityManager
- LazyRegionAccessibilityManager
- LazyRegionDeploymentManager
- LazyRegionConfigurationManager
- LazyRegionStateManager
- LazyRegionDataService

Mark these items as "Planned" in docs and link to issues or TODOs if you intend to implement them.

## Core Roles & Responsibilities

- LazyRegionManager  
  Central coordinator for region registration, lifecycle, and navigation orchestration. Interfaces with builders and (planned) animators/navigation services.

- LazyRegionBuilder  
  Constructs UI for a region from configuration and data. In WPF this typically produces XAML-backed UI/templates; in MAUI it assembles controls in C#.

- LazyRegionAnimator (planned)  
  Will handle transition animations between regions. Not currently implemented.

- LazyRegionNavigationService (planned)  
  Will manage navigation requests, current region tracking, and navigation history.

- LazyRegionViewModel / LazyRegionView (patterns)  
  The architecture encourages MVVM: ViewModels expose data/commands; Views are built by the builder and bind to ViewModels.

## WPF vs MAUI — Key Differences

- WPF
  - Visuals and templates use XAML (Themes/Generic.xaml).
  - Typical animation mechanisms: Storyboards, Triggers.
  - Implementations found under src/LazyRegion.WPF.

- MAUI
  - Controls and layouts are constructed primarily in C#.
  - Animations use MAUI animation APIs.
  - Tab-related helpers exist: LazyTabRegion / LazyTabItem.
  - Implementations found under src/LazyRegion.Maui.

Core behavior and manager responsibilities remain consistent across platforms; platform-specific code lives in the respective project folders.

## Animation & Navigation (current status)

- Animation support is planned but not fully implemented as a separable LazyRegionAnimator component.
- Navigation management is coordinated by LazyRegionManager today; a dedicated LazyRegionNavigationService is planned to encapsulate navigation stack/history and expose a clearer API.

## LazyStage and LazyRegionControl

- LazyStage: container that hosts multiple LazyRegionControls and manages layout/aggregation.
- LazyRegionControl: per-region control responsible for rendering content built by LazyRegionBuilder and binding to a ViewModel.

## Runtime & Initialization Notes (recommended)

- Prefer lazy initialization for LazyRegionRegistry.OnManagerRequested to allow flexible call order during startup. This avoids strict ordering requirements between platform setup (UseWpf / UseMaui) and registry/manager requests.
- Provide a UseWpf(Action<LazyRegionApp>) overload (or equivalent platform-specific configuration callback) to allow safe, centralized configuration of LazyRegionApp at startup.
- When adding platform initialization helpers, document expected call order and thread/context requirements.

## Actionable Recommendations

1. Update docs to list implemented files (above) and mark remaining features as "Planned".
2. Add TODOs or issues for each planned component with priority and owner.
3. Add a brief code comment in LazyRegionRegistry explaining the lazy initialization intent and linking to this document.
4. Consider implementing UseWpf(Action<LazyRegionApp>) overload and ensure LazyRegionRegistry.OnManagerRequested remains lazy.

## References

- MVVM Pattern — https://docs.microsoft.com/dotnet/desktop/wpf/data/mvvm
- WPF Animation — https://docs.microsoft.com/dotnet/desktop/wpf/graphics-multimedia/animation-overview
- MAUI Animation — https://docs.microsoft.com/dotnet/maui/user-interface/animations
- WPF Navigation — https://docs.microsoft.com/dotnet/desktop/wpf/app-development/navigation-overview
- MAUI Navigation — https://docs.microsoft.com/dotnet/maui/fundamentals/navigation
