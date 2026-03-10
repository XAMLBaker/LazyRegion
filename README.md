# LazyRegion  
**WPF**
[![NuGet](https://img.shields.io/nuget/v/LazyRegion.WPF.svg)](https://www.nuget.org/packages/LazyRegion.WPF/) 
[![NuGet Downloads](https://img.shields.io/nuget/dt/LazyRegion.WPF.svg)](https://www.nuget.org/packages/LazyRegion.WPF/)

**MAUI**
[![NuGet](https://img.shields.io/nuget/v/LazyRegion.MAUI.svg)](https://www.nuget.org/packages/LazyRegion.MAUI/) 
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
- 🔌 **Static 진입점** – `LazyRegionApp.Default`로 DI 없이도 바로 사용 가능
- 🗂 **Tab 내비게이션** – `LazyTabRegion`으로 스와이프 제스처 기반 탭 전환 (MAUI)
- 🔔 **ViewModel 라이프사이클 훅** – `ILazyNavigationAware`로 전환 이벤트 수신 및 파라미터 전달
- 🛡 **네비게이션 Guard** – `ILazyNavigationGuard`로 전환 전 취소 가능
- 🎬 **Per-Navigation 애니메이션 오버라이드** – 전환별로 다른 애니메이션 지정 가능
- ⏪ **GoBack** – 직전 화면으로 역방향 애니메이션과 함께 복귀
- 🔗 **Region 그룹 네비게이션** – 여러 Region을 동시에 전환 (리전별 애니메이션)

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
### 🔹 초기 네비게이션 구성 (Initial Flow)

애플리케이션 시작 시 또는 Region이 최초 활성화될 때
지정된 View로 자동 이동하는 초기 흐름(Initial Flow)를 정의할 수 있습니다.
```csharp
.UseLazyRegion()
.AddLazyView<ScreenA>("a")
.AddLazyView<ScreenB>("b")
.ConfigureRegions(configure =>
{
    configure.ForRegion("Root")
             .WithInitialFlow(flow =>
             {
                 flow.Show("Splash");
             });
});
```
- 앱 시작 시 Root Region이 활성화되면 Splash 화면이 자동으로 표시됩니다.
- Initial Flow는 Region 단위로 한 번만 실행됩니다.

### 🔹 Region Timeout

Region이 아직 생성·등록되지 않은 상태에서 네비게이션 요청이 발생할 경우,
지정된 시간 동안 대기 후 자동으로 요청을 취소할 수 있습니다.
```csharp
regionManager.NavigateAsync(
    "Root",
    "a",
    TimeSpan.FromSeconds(30)
);
```
- 30초 이내에 Root Region이 등록되면 정상적으로 이동합니다.
- 제한 시간 초과 시 네비게이션은 자동 취소됩니다.
- Region 생성 지연으로 인한 무한 대기를 방지할 수 있습니다.

### 🔹 Region State (Loading / Error)

Region 단위로 **Loading / Error 상태 전환을 자동 관리**할 수 있습니다.
```csharp
.UseLazyRegion()
.AddLazyView<ScreenA>("a")
.AddLazyView<ScreenB>("b")
.AddLazyView<LoadingView>("Loading")
.AddLazyView<ErrorView>("Error")
.ConfigureRegions(configure =>
{
    configure.ForRegion("Root")
             .WithLoadingBehavior(state =>
             {
                 state.Loading("Loading")
                      .MinDisplayTime(TimeSpan.FromSeconds(5));

                 state.Error("Error")
                      .Timeout(TimeSpan.FromSeconds(10));
             });
});
```
#### 동작 방식

- 네비게이션 대기 중에는 Loading 화면이 표시됩니다.
- Loading 화면은 최소 5초간 유지됩니다.
- 지정된 시간 내 처리가 완료되지 않으면 자동으로 Error 화면으로 전환됩니다.
- Error 상태 이후에도 정상적인 View 전환은 계속 가능합니다.

### 🔹 Initial Flow – 조건 기반 흐름 제어

Initial Flow에서는 **단계별 View 전환과 조건 분기**를 정의할 수 있습니다.

#### 기본 조건 흐름
```csharp
.UseLazyRegion()
.AddLazyView<ScreenA>("a")
.AddLazyView<ScreenB>("b")
.ConfigureRegions(configure =>
{
    configure.ForRegion("Root")
             .WithInitialFlow(flow =>
             {
                 flow.Show("Splash")
                     .Then("Main", async () =>
                     {
                         await Task.Delay(5000);
                         return false;
                     })
                     .Then("Login");
             });
});
```

- Splash → Main 순서로 시도합니다.
- Then의 조건 함수가 false를 반환하면 다음 단계(Login)로 이동합니다.
- 비동기 로직을 통해 초기 상태 판단이 가능합니다.

#### 서비스 주입 기반 조건 흐름
```csharp
.UseLazyRegion()
.AddLazyView<ScreenA>("a")
.AddLazyView<ScreenB>("b")
.ConfigureRegions(configure =>
{
    configure.ForRegion("Root")
             .WithInitialFlow(flow =>
             {
                 flow.Show("Splash")
                     .Then<ILoginService>("Main", async service =>
                     {
                         return await service.LoginAsync();
                     })
                     .Then("Login");
             });
});
```

- Then<TService>를 통해 필요한 서비스를 자동 주입받을 수 있습니다.
- 로그인 여부, 권한 확인 등 초기 진입 조건을 자연스럽게 구성할 수 있습니다.
- 조건이 충족되면 해당 View로 이동하고, 실패 시 다음 단계로 진행됩니다.


### 🔹 UseLazyRegion메서드의 확장
UseLazyRegion은 람다 구성을 통해 **View 등록과 Region 설정**을 한 번에 정의할 수 있습니다.
이를 통해 초기화 코드를 하나의 응집된 블록으로 구성할 수 있습니다.
```csharp
.UseLazyRegion(lazy =>
{
    // View 등록
    lazy.Register<SplashView>("Splash");
    lazy.Register<LoginView>("Login");
    lazy.Register<MainView>("Main");

    // Region 설정
    lazy.ConfigureRegions(config =>
    {
        config.ForRegion("Root")
              .WithInitialFlow(flow =>
              {
                  flow.Show("Splash")
                      .Then("Main", async () =>
                      {
                          await Task.Delay(5000);
                          return false;
                      })
                      .Then("Login");
              });
    });
});
```
#### 특징

- Register<TView>를 통해 Lazy View를 명시적으로 등록합니다.
- ConfigureRegions를 함께 사용하여 Region 설정을 한 곳에서 관리할 수 있습니다.
- View 등록 → Region 구성 → Initial Flow 정의가 하나의 초기화 흐름으로 묶입니다.
- 기존의 AddLazyView, ConfigureRegions 호출을 분리해서 작성하지 않아도 됩니다.

#### 사용 권장 시점
- 앱 초기 부트스트랩 코드를 간결하게 유지하고 싶은 경우
- Region 및 View 구성을 한 파일에서 명확히 정의하고 싶은 경우
- 초기 네비게이션 흐름을 선언적으로 관리하고 싶은 경우

---

### 🔹 Static 진입점 (LazyRegionApp.Default)

DI 환경이 없어도 `LazyRegionApp.Default`를 통해 뷰 등록 및 내비게이션이 가능합니다.
WPF CustomControl처럼 생성자 DI가 불가능한 환경에서 특히 유용합니다.

```csharp
// 앱 시작 시 등록 (DI 불필요)
LazyRegionApp.Default
    .UseWpf()
    .Register<HomeView>("home")
    .Register<LoginView>("login");

<!-- 혹은 -->
LazyRegionApp.Default.UseWpf((app)=>{
    app.Register<HomeView>("home")
       .Register<LoginView>("login")
});


// ViewModel 또는 CustomControl에서 접근
LazyRegionApp.Default.RegionManager.NavigateAsync("Root", "home");
```

- DI(`UseLazyRegion()`)와 Static 경로를 동시에 사용 가능하며, 동일한 인스턴스를 공유합니다.
- `Register()` 전에 `UseLazyRegion()`을 호출하면 등록 내용이 자동으로 DI 컨테이너에 통합됩니다.

---

### 🔹 LazyTabRegion (MAUI)

MAUI 전용 탭 내비게이션 컨트롤입니다.
스와이프 제스처와 완전한 커스텀 NavigationBar를 지원합니다.

```xml
<lz:LazyTabRegion
    TransitionAnimation="SlideLeft"
    SwipeThreshold="0.4"
    NavigationBarPlacement="Bottom"
    SelectionChanged="OnSelectionChanged">

    <lz:LazyTabRegion.NavigationBar>
        <!-- 사용자가 자유롭게 구성하는 탭 바 -->
    </lz:LazyTabRegion.NavigationBar>

    <lz:LazyTabItem Key="home"/>
    <lz:LazyTabItem Key="search"/>
    <lz:LazyTabItem Key="settings"/>
</lz:LazyTabRegion>
```

```csharp
// MauiProgram.cs — Key 등록
LazyRegionApp.Default
    .Register<HomeView>("home")
    .Register<SearchView>("search")
    .Register<SettingsView>("settings");
```

- Slide 계열 애니메이션 선택 시 스와이프 제스처 자동 활성화
- NavigationBar는 빈 슬롯으로 제공되어 자유롭게 배치 가능
- `LazyTabItem Key="..."` 방식으로 LazyRegion의 Key 등록 패턴을 그대로 유지

---

### 🔹 ViewModel 라이프사이클 훅 (ILazyNavigationAware)

ViewModel이 `ILazyNavigationAware`를 구현하면 Region 전환 시 라이프사이클 이벤트를 받을 수 있습니다.
인터페이스를 구현하지 않은 ViewModel은 기존과 완전히 동일하게 동작합니다.

```csharp
public class HomeViewModel : ILazyNavigationAware
{
    public void OnNavigatedTo(LazyNavigationContext context)
    {
        // 이 뷰가 Region의 활성 콘텐츠가 될 때 호출
        // context.Parameters로 전달된 파라미터 수신 가능
    }

    public void OnNavigatedFrom(LazyNavigationContext context)
    {
        // 다른 뷰로 전환되기 직전에 호출
        // 구독 해제, 리소스 정리 등
    }
}
```

#### 호출 시점

| 메서드 | 호출 시점 |
|--------|-----------|
| `OnNavigatedFrom` | `Set()` 호출 전 (애니메이션 시작 전) |
| `OnNavigatedTo` | `Set()` 호출 직후 (애니메이션 시작 직후) |

> `ILazyNavigationAware`는 `NavigateAsync<T>(...)`로 ViewModel을 DI에서 주입받는 경로에서만 동작합니다.

---

### 🔹 네비게이션 파라미터 (LazyNavigationParameters)

전환 시 ViewModel에 데이터를 전달할 수 있습니다.

```csharp
// 전환 호출
await regionManager.NavigateAsync<DetailViewModel>(
    “MainRegion”,
    “Detail”,
    new LazyNavigationParameters
    {
        { “OrderId”, 42 },
        { “Mode”, EditMode.ReadOnly }
    });

// ViewModel에서 수신
public void OnNavigatedTo(LazyNavigationContext context)
{
    var orderId = context.Parameters.GetValue<int>(“OrderId”);
    var mode = context.Parameters.GetValueOrDefault<EditMode>(“Mode”, EditMode.View);
}
```

#### LazyNavigationParameters API

| 메서드 | 설명 |
|--------|------|
| `Add(key, value)` | 파라미터 추가 |
| `GetValue<T>(key)` | 값 반환, 없으면 예외 |
| `GetValueOrDefault<T>(key, default)` | 값 반환, 없으면 기본값 |
| `TryGetValue<T>(key, out value)` | 안전한 값 조회 |
| `ContainsKey(key)` | 키 존재 여부 확인 |

---

### 🔹 네비게이션 Guard (ILazyNavigationGuard)

`ILazyNavigationGuard`를 구현하면 전환이 시작되기 전에 취소 여부를 결정할 수 있습니다.
`false`를 반환하면 전환이 중단되고 `OnNavigatedFrom`도 호출되지 않습니다.

```csharp
public class EditViewModel : ILazyNavigationAware, ILazyNavigationGuard
{
    private bool _hasUnsavedChanges;

    public async Task<bool> CanNavigateAsync(LazyNavigationContext context)
    {
        if (!_hasUnsavedChanges)
            return true;

        // 비동기 확인 다이얼로그 등
        return await ShowConfirmDialogAsync(“저장하지 않은 변경사항이 있습니다. 나가시겠습니까?”);
    }

    public void OnNavigatedTo(LazyNavigationContext context) { }
    public void OnNavigatedFrom(LazyNavigationContext context) { }
}
```

#### 전환 흐름

```
NavigateAsync 호출
  ├─ CanNavigateAsync() → false  →  전환 취소 (종료)
  ├─ CanNavigateAsync() → true
  ├─ OnNavigatedFrom() (이전 VM)
  ├─ Set() → 애니메이션 시작
  └─ OnNavigatedTo() (새 VM)
```

---

### 🔹 Per-Navigation 애니메이션 오버라이드

기본 `TransitionAnimation` 프로퍼티와 별개로, 개별 전환 호출마다 다른 애니메이션을 지정할 수 있습니다.

```csharp
// 이번 전환만 SlideLeft로 실행
await regionManager.NavigateAsync("MainRegion", "Detail", TransitionAnimation.SlideLeft);

// ViewModel 주입 + 파라미터 + 애니메이션 오버라이드 조합
await regionManager.NavigateAsync<DetailViewModel>(
    "MainRegion",
    "Detail",
    new LazyNavigationParameters { { "Id", 42 } },
    TransitionAnimation.ZoomIn);
```

- 오버라이드는 1회성으로, 해당 전환에만 적용되고 이후에는 컨트롤의 기본 애니메이션으로 복귀합니다.
- 오버라이드를 지정하지 않으면 기존과 동일하게 `TransitionAnimation` 프로퍼티 값이 사용됩니다.

---

### 🔹 GoBack (이전 화면 복귀)

직전에 표시되었던 화면으로 자동 역방향 애니메이션과 함께 복귀합니다.

```csharp
// 뒤로 갈 수 있는지 확인
if (regionManager.CanGoBack("MainRegion"))
{
    await regionManager.GoBackAsync("MainRegion");
}
```

#### 역방향 애니메이션 매핑

| 원래 애니메이션 | GoBack 시 |
|----------------|-----------|
| `SlideLeft` | `SlideRight` |
| `SlideRight` | `SlideLeft` |
| `SlideUp` | `SlideDown` |
| `SlideDown` | `SlideUp` |
| `NewFromLeft` | `NewFromRight` |
| `NewFromRight` | `NewFromLeft` |
| `ZoomIn` | `ZoomOut` |
| `ZoomOut` | `ZoomIn` |
| `Fade`, `Scale`, `None` | 동일 (대칭) |

- GoBack은 depth=1로, 직전 1개 화면만 지원합니다.
- GoBack 후에는 `CanGoBack`이 `false`가 됩니다.
- Singleton View의 경우 DataContext가 유지되므로 ViewModel 상태도 보존됩니다.

---

### 🔹 Region 그룹 네비게이션

여러 Region을 동시에 전환할 수 있습니다. 각 Region별로 독립적인 애니메이션을 지정할 수 있습니다.

```csharp
await regionManager.NavigateGroupAsync(
    ("HeaderRegion", "DetailHeader", TransitionAnimation.Fade),
    ("ContentRegion", "DetailContent", TransitionAnimation.SlideLeft),
    ("SidebarRegion", "DetailSidebar", null)  // null이면 기본 애니메이션 사용
);
```

- `Task.WhenAll`로 병렬 실행되어 모든 Region이 동시에 전환됩니다.
- Guard가 거부한 Region은 해당 Region만 전환이 취소되고, 나머지는 정상 진행됩니다.
- 각 Region은 독립적으로 동작하므로 서로 영향을 주지 않습니다.

---

### 정리

- **Initial Flow**는 “앱 시작 시 실행되는 시나리오”를 선언적으로 표현하기 위한 기능입니다.
- Region 생성 시점, 로딩 상태, 실패 처리, 조건 분기를 하나의 흐름으로 구성할 수 있습니다.
- 복잡한 초기 네비게이션 로직을 코드 흐름 그대로 읽을 수 있도록 설계되었습니다.
- **ViewModel 라이프사이클 훅**은 선택적 구현으로, 기존 코드에 영향을 주지 않습니다.
- **애니메이션 오버라이드**는 전환별 1회성 적용으로, 기본 설정에 영향을 주지 않습니다.
- **GoBack**은 depth=1로 간결하게 유지하여, 불필요한 메모리 사용 없이 직전 화면 복귀를 지원합니다.
- **Region 그룹 네비게이션**으로 복잡한 다중 Region 레이아웃을 한 번의 호출로 제어할 수 있습니다.
