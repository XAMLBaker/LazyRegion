# LazyRegion  
**WPF**
[![NuGet](https://img.shields.io/nuget/v/LazyRegion.WPF.svg)](https://www.nuget.org/packages/LazyRegion.WPF/1.2.0) 
[![NuGet Downloads](https://img.shields.io/nuget/dt/LazyRegion.WPF.svg)](https://www.nuget.org/packages/LazyRegion.WPF/)

**MAUI**
[![NuGet](https://img.shields.io/nuget/v/LazyRegion.MAUI.svg)](https://www.nuget.org/packages/LazyRegion.MAUI/1.0.0) 
[![NuGet Downloads](https://img.shields.io/nuget/dt/LazyRegion.MAUI.svg)](https://www.nuget.org/packages/LazyRegion.MAUI/)

**LazyRegion**은 부드러운 전환 애니메이션과 MVVM 구조를 지원하는 화면 전환용 Region 라이브러리입니다.  
WPF 및 .NET MAUI 환경 모두에서 사용할 수 있으며, Region 기반의 자연스러운 화면 전환과 상태 관리를 제공합니다.

---

## ✨ 주요 특징

- 🎞 **부드러운 화면 전환** – 설정된 효과에 따라 자연스러운 애니메이션 적용  
- 🧩 **MVVM 완전 지원** – ViewModel 변경만으로 자동 전환  
- 💻 **Code-behind 지원** – 코드에서 직접 `Content` 변경 시에도 동일한 전환 처리  
- 📦 **RegionManager 통합** – 지정된 Region을 동적으로 전환 가능  
- 🌍 **멀티 플랫폼 지원** – **WPF**와 **.NET MAUI**에서 모두 사용 가능  

---

## 📦 설치

NuGet에서 설치:

```powershell
dotnet add package LazyRegion.WPF
dotnet add package LazyRegion.MAUI
```
또는 패키지 매니저 콘솔:

```powershell
Install-Package LazyRegion.WPF
Install-Package LazyRegion.MAUI
```

## 🚀 빠른 시작
### DataTemplate 기반 예시
```xml

        xmlns:lr="clr-namespace:LazyRegion.WPF;assembly=LazyRegion.WPF"> // or xmlns:lr="clr-namespace:LazyRegion.MAUI;assembly=LazyRegion.MAUI">

        <lr:LazyRegion Content="{Binding CurrentPage}" TransitionAnimation="Fade"/>

```
ViewModel에서 CurrentPage를 바꾸면 자동으로 페이드 애니메이션과 함께 전환됩니다.

### 비하인드코드예시
```csharp
var region = new LazyRegion
{
    Content = new HomeView()
};

region.Content = new DetailView(); // 자동으로 애니메이션 전환
```
MAUI에서도 동일한 방식으로 Content 전환 시 애니메이션이 적용됩니다.

### ⚙ RegionManager 예시
```xml

        xmlns:lr="clr-namespace:LazyRegion.WPF;assembly=LazyRegion.WPF">

        <lr:LazyRegion RegionName="MainRegion" TransitionAnimation="Fade"/>
```
```csharp
RegionManager.RequestNavigate("MainRegion", new HomeView());
```
RegionManager를 통해 지정된 Region의 View를 손쉽게 교체할 수 있습니다.

# 확장 기능
### 🔹 초기 네비게이션 구성 (ConfigureInitialNavigation)
앱 시작 시 자동으로 특정 Region으로 이동할 수 있습니다.

```csharp
        .UseLazyRegion()
        .AddLazyView<ScreenA>("a")
        .AddLazyView<ScreenB>("b")
        .ConfigureInitialNavigation(config =>
        {
                config.NavigateAsync("Root", "a"); // 앱 시작 시 Root Region → ScreenA
        });
```
### 🔹 RegionTimeout
Region 등록이 지연될 경우, 지정된 시간 내 등록되지 않으면 자동 취소됩니다.

```csharp
regionManager.NavigateAsync("Root", "a", TimeSpan.FromSeconds(30));
```
### 🔹 RegionState
Region 단위로 Loading / Error 상태를 자동 관리합니다.
```
        .UseLazyRegion ()
        .AddLazyView<ScreenA> ("a")
        .AddLazyView<ScreenB> ("b")
        .AddLazyView<LoadingView>("Loading")
        .AddLazyView<ErrorView>("Error")
        .ConfigureRegions (configure =>
        {
                configure.ForRegion ("Root")
                          .WithLoadingBehavior (state =>
                          {
                              state.Loading ("Loading")
                                     .MinDisplayTime (TimeSpan.FromSeconds (5));
                        
                              state.Error ("Error")
                                     .Timeout (TimeSpan.FromSeconds (10));
                          });
        });
```
- 로딩 대기 시간 동안 Loading 화면 표시
- 지정 시간 초과 시 자동으로 Error 화면 표시
- Error 이후에도 View 전환 정상 작동
