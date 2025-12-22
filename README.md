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

### 정리

- **Initial Flow**는 “앱 시작 시 실행되는 시나리오”를 선언적으로 표현하기 위한 기능입니다.
- Region 생성 시점, 로딩 상태, 실패 처리, 조건 분기를 하나의 흐름으로 구성할 수 있습니다.
- 복잡한 초기 네비게이션 로직을 코드 흐름 그대로 읽을 수 있도록 설계되었습니다.