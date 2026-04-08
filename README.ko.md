# LazyRegion
**WPF**
[![NuGet](https://img.shields.io/nuget/v/LazyRegion.WPF.svg)](https://www.nuget.org/packages/LazyRegion.WPF/) 
[![NuGet Downloads](https://img.shields.io/nuget/dt/LazyRegion.WPF.svg)](https://www.nuget.org/packages/LazyRegion.WPF/)

**MAUI**
[![NuGet](https://img.shields.io/nuget/v/LazyRegion.MAUI.svg)](https://www.nuget.org/packages/LazyRegion.MAUI/) 
[![NuGet Downloads](https://img.shields.io/nuget/dt/LazyRegion.MAUI.svg)](https://www.nuget.org/packages/LazyRegion.MAUI/)

**WinUI3**
[![NuGet](https://img.shields.io/nuget/v/LazyRegion.WinUI3.svg)](https://www.nuget.org/packages/LazyRegion.WinUI3/) 
[![NuGet Downloads](https://img.shields.io/nuget/dt/LazyRegion.WinUI3.svg)](https://www.nuget.org/packages/LazyRegion.WinUI3/)

**LazyRegion**은 부드러운 전환 애니메이션과 MVVM 구조를 지원하는 화면 전환용 Region 라이브러리입니다.
WPF, .NET MAUI, WinUI3 환경에서 모두 사용할 수 있으며, Region 기반의 자연스러운 화면 전환과 상태 관리를 제공합니다.

---

## ✨ 주요 특징

- 🎞 **부드러운 화면 전환**: 설정된 효과에 따라 자연스러운 애니메이션을 적용합니다.
- 🧩 **MVVM 완벽 지원**: ViewModel 변경만으로 화면이 자동으로 전환됩니다.
- 💻 **Code-behind 지원**: 코드에서 직접 `Content`를 변경해도 동일한 전환 효과를 제공합니다.
- 📦 **RegionManager 통합**: 지정된 Region을 동적으로 손쉽게 전환할 수 있습니다.
- 🌍 **멀티 플랫폼 지원**: **WPF**, **.NET MAUI**, **WinUI3**에서 모두 사용 가능합니다.
- 🔌 **Static 진입점**: `LazyRegionApp.Default`를 통해 DI 없이도 바로 사용할 수 있습니다.
- 🗂 **탭 내비게이션**: `LazyTabRegion`으로 스와이프 제스처 기반의 탭 전환을 지원합니다 (MAUI).
- 🔔 **ViewModel 라이프사이클**: `ILazyNavigationAware`로 전환 이벤트를 수신하고 파라미터를 전달받을 수 있습니다.
- 🛡 **네비게이션 가드**: `ILazyNavigationGuard`를 통해 전환을 취소할 수 있습니다.
- 🎬 **전환별 애니메이션 재정의**: 각 네비게이션마다 다른 애니메이션을 지정할 수 있습니다.
- ⏪ **뒤로 가기 (GoBack)**: 이전 화면으로 역방향 애니메이션과 함께 복귀합니다.
- 🔗 **Region 그룹 네비게이션**: 여러 Region을 한 번에 동시 전환할 수 있습니다.

---

## 📦 설치

NuGet에서 원하는 플랫폼에 맞춰 설치합니다.

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

## 🚀 빠른 시작

### 1. DataTemplate 기반 예시
ViewModel의 프로퍼티 변경만으로 화면을 전환합니다.

```xml
<!-- 네임스페이스 추가 -->
xmlns:lr="clr-namespace:LazyRegion.WPF;assembly=LazyRegion.WPF"
<!-- 또는 MAUI/WinUI3 -->
xmlns:lr="clr-namespace:LazyRegion.MAUI;assembly=LazyRegion.MAUI"
xmlns:lr="using:LazyRegion.WinUI3"

<!-- LazyRegion 사용 -->
<lr:LazyRegion Content="{Binding CurrentPage}" TransitionAnimation="Fade"/>
```
ViewModel에서 `CurrentPage` 프로퍼티 값을 바꾸면 페이드 애니메이션과 함께 화면이 전환됩니다.

### 2. 비하인드 코드 예시
`Content` 프로퍼티를 직접 설정하여 화면을 전환합니다.

```csharp
var region = new LazyRegion
{
    Content = new HomeView()
};

// 자동으로 애니메이션과 함께 DetailView로 전환됩니다.
region.Content = new DetailView();
```

### 3. RegionManager 예시
`RegionManager`를 사용하여 이름으로 지정된 Region의 View를 교체합니다.

```xml
<lr:LazyRegion RegionName="MainRegion" TransitionAnimation="Fade"/>
```
```csharp
// "MainRegion"이라는 이름의 Region을 HomeView로 전환합니다.
RegionManager.RequestNavigate("MainRegion", new HomeView());
```

---

## ⚙️ 확장 기능

<details>
<summary>🔹 초기 네비게이션 및 Region 설정 (Initial Flow, Timeout, State)</summary>

### 초기 네비게이션 구성 (Initial Flow)
앱 시작 시 또는 Region이 처음 활성화될 때 지정된 View로 자동 이동하는 초기 흐름을 정의합니다.

```csharp
.UseLazyRegion(lazy =>
{
    lazy.Register<SplashView>("Splash");
    lazy.ConfigureRegions(config =>
    {
        config.ForRegion("Root")
              .WithInitialFlow(flow =>
              {
                  flow.Show("Splash"); // 앱 시작 시 Splash 화면 표시
              });
    });
});
```

### 조건 기반 흐름 제어
`Then`과 `Then<TService>`를 사용하여 조건에 따라 다른 View를 표시할 수 있습니다. 로그인 여부 확인 등에 유용합니다.

```csharp
.WithInitialFlow(flow =>
{
    flow.Show("Splash")
        .Then<ILoginService>("Main", async service => await service.IsLoggedInAsync()) // 로그인 상태면 Main으로
        .Then("Login"); // 아니면 Login으로
});
```

### Region Timeout
Region이 아직 등록되지 않았을 때 네비게이션 요청이 오면, 지정된 시간 동안 대기 후 자동으로 요청을 취소합니다.

```csharp
// 30초 안에 "Root" Region이 등록되지 않으면 네비게이션이 취소됩니다.
regionManager.NavigateAsync("Root", "a", TimeSpan.FromSeconds(30));
```

### Region 상태 관리 (Loading / Error)
네비게이션 대기 중 `Loading` 화면을, 시간 초과 시 `Error` 화면을 자동으로 표시합니다.

```csharp
.ConfigureRegions(config =>
{
    config.ForRegion("Root")
          .WithLoadingBehavior(state =>
          {
              state.Loading("LoadingViewKey") // 로딩 시 보일 View
                   .MinDisplayTime(TimeSpan.FromSeconds(1)); // 최소 표시 시간

              state.Error("ErrorViewKey") // 오류 시 보일 View
                   .Timeout(TimeSpan.FromSeconds(10)); // 10초 후 타임아웃
          });
});
```
</details>

<details>
<summary>🔹 ViewModel 중심 네비게이션 (라이프사이클, 파라미터, 가드)</summary>

### ViewModel 라이프사이클 훅 (ILazyNavigationAware)
ViewModel이 `ILazyNavigationAware` 인터페이스를 구현하면, 네비게이션 전후에 이벤트를 받을 수 있습니다.

```csharp
public class HomeViewModel : ILazyNavigationAware
{
    // 이 View로 네비게이션이 완료되었을 때 호출
    public void OnNavigatedTo(LazyNavigationContext context)
    {
        // 파라미터 수신, 데이터 로딩 등
    }

    // 다른 View로 네비게이션이 시작되기 직전에 호출
    public void OnNavigatedFrom(LazyNavigationContext context)
    {
        // 리소스 정리, 구독 해제 등
    }
}
```

### 네비게이션 파라미터 (LazyNavigationParameters)
네비게이션 시 ViewModel에 데이터를 전달합니다.

```csharp
// 네비게이션 호출 시 파라미터 전달
await regionManager.NavigateAsync(
    "MainRegion",
    "Detail",
    new LazyNavigationParameters { { "OrderId", 42 } }
);

// ViewModel에서 파라미터 수신
public void OnNavigatedTo(LazyNavigationContext context)
{
    var orderId = context.Parameters.GetValue<int>("OrderId");
}
```

### 네비게이션 가드 (ILazyNavigationGuard)
`ILazyNavigationGuard`를 구현하여 네비게이션을 조건부로 취소할 수 있습니다. 예를 들어, 저장되지 않은 변경 사항이 있을 때 사용자에게 확인을 요청하는 데 사용됩니다.

```csharp
public class EditViewModel : ILazyNavigationGuard
{
    public async Task<bool> CanNavigateAsync(LazyNavigationContext context)
    {
        if (HasUnsavedChanges)
        {
            // false를 반환하면 네비게이션이 취소됩니다.
            return await ShowConfirmDialogAsync("저장하지 않은 변경사항이 있습니다. 나가시겠습니까?");
        }
        return true; // true를 반환하면 네비게이션이 계속 진행됩니다.
    }
}
```
</details>

<details>
<summary>🔹 고급 네비게이션 제어 (뒤로 가기, 애니메이션 재정의, 그룹)</summary>

### 뒤로 가기 (GoBack)
이전 화면으로 역방향 애니메이션과 함께 돌아갑니다.

```csharp
if (regionManager.CanGoBack("MainRegion"))
{
    await regionManager.GoBackAsync("MainRegion");
}
```
- `SlideLeft` ↔ `SlideRight`, `ZoomIn` ↔ `ZoomOut` 등 애니메이션이 자동으로 매핑됩니다.
- 스택은 단일 깊이(depth=1)만 지원하여 간단하게 유지됩니다.

### 전환별 애니메이션 재정의
`NavigateAsync` 호출 시 일회성으로 다른 애니메이션을 지정할 수 있습니다.

```csharp
// 이번 전환만 SlideLeft 애니메이션으로 실행
await regionManager.NavigateAsync("MainRegion", "Detail", TransitionAnimation.SlideLeft);
```

### Region 그룹 네비게이션
여러 Region을 한 번의 호출로 동시에 전환합니다. 각 Region에 다른 애니메이션을 적용할 수 있습니다.

```csharp
await regionManager.NavigateGroupAsync(
    ("HeaderRegion", "DetailHeader", TransitionAnimation.Fade),
    ("ContentRegion", "DetailContent", TransitionAnimation.SlideLeft),
    ("SidebarRegion", "DetailSidebar", null) // null은 기본 애니메이션 사용
);
```
</details>

<details>
<summary>🔹 플랫폼별 기능 및 기타</summary>

### LazyTabRegion (MAUI 전용)
스와이프 제스처를 지원하는 커스텀 탭 컨트롤입니다.

```xml
<lz:LazyTabRegion SwipeThreshold="0.4">
    <lz:LazyTabRegion.NavigationBar>
        <!-- 사용자 정의 탭 바 UI -->
    </lz:LazyTabRegion.NavigationBar>

    <lz:LazyTabItem Key="home"/>
    <lz:LazyTabItem Key="search"/>
</lz:LazyTabRegion>
```
- `Slide` 계열 애니메이션 사용 시 스와이프가 자동으로 활성화됩니다.
- `Key`는 `LazyRegionApp.Default.Register<HomeView>("home")` 등으로 등록된 View와 매칭됩니다.

### Static 진입점 (LazyRegionApp.Default)
DI 컨테이너 없이도 `LazyRegion`의 기능을 사용할 수 있습니다. WPF의 `CustomControl`처럼 생성자 주입이 어려운 환경에서 유용합니다.

```csharp
// 앱 시작 시 View 등록
LazyRegionApp.Default
    .UseWpf() // 또는 .UseMaui() / .UseWinUI3()
    .Register<HomeView>("home");

// ViewModel 또는 컨트롤에서 RegionManager 접근
LazyRegionApp.Default.RegionManager.NavigateAsync("Root", "home");
```
`UseLazyRegion()`과 함께 사용하면 등록된 정보가 DI 컨테이너와 자동으로 통합됩니다.
</details>

---

### 요약

- **Initial Flow**: 앱 시작 시나리오를 선언적으로 구성합니다.
- **ViewModel Hooks**: `ILazyNavigationAware`와 `ILazyNavigationGuard`로 MVVM 네비게이션을 제어합니다.
- **Advanced Navigation**: `GoBack`, 애니메이션 재정의, 그룹 네비게이션으로 복잡한 UI 흐름을 관리합니다.
- **Static Access**: `LazyRegionApp.Default`로 DI 없이도 모든 기능에 접근할 수 있습니다.
