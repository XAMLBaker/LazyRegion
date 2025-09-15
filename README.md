# LazyRegion.WPF  

[![NuGet](https://img.shields.io/nuget/v/LazyRegion.WPF.svg)](https://www.nuget.org/packages/LazyRegion.WPF/)  
[![NuGet Downloads](https://img.shields.io/nuget/dt/LazyRegion.WPF.svg)](https://www.nuget.org/packages/LazyRegion.WPF/)

부드러운 전환 애니메이션을 지원하는 **WPF용 LazyRegion** 라이브러리입니다.  
기존 `ContentControl`과 유사하지만, `Content`가 변경될 때 애니메이션을 통해 자연스럽게 화면을 전환합니다.  

---

## ✨ 주요 특징

- 🎞 **애니메이션 전환 지원** – 설정한 효과와 속도에 맞춰 자연스럽게 변경  
- 🧩 **MVVM 친화적** – ViewModel 변경 시 자동으로 감지하여 전환  
- 💻 **비하인드 코드 친화적** – Code-behind에서 `Content` 변경 시에도 적용  
- 📦 **RegionManager 지원** – 지정된 RegionManager를 통해 특정 구역을 동적으로 변경 가능  

---

## 📦 설치

NuGet에서 설치 가능합니다:

```bash
dotnet add package LazyRegion.WPF
```

또는 패키지 매니저 콘솔:
```
Install-Package LazyRegion.WPF
```

## 🚀 빠른 시작
**XAML 예시**
```xml
<Window x:Class="SampleApp.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        xmlns:lr="clr-namespace:LazyRegion.WPF;assembly=LazyRegion.WPF">

    <Grid>
        <lr:LazyRegion Content="{Binding CurrentPage}" TranstionAimation="Fade"/>
    </Grid>
</Window>
```
ViewModel에서 CurrentPage를 바꾸면 자동으로 페이드 애니메이션과 함께 전환됩니다.


**코드 비하인드 예시**
```csharp
myLazyRegion.Content = new DetailView();
```

코드에서 직접 Content를 바꿔도 동일하게 애니메이션이 적용됩니다.


**RegionManager 예시**
```csharp
RegionManager.RequestNavigate("MainRegion", new HomeView());
```
RegionManager와 함께 사용하면 지정한 Region의 View를 손쉽게 전환할 수 있습니다.