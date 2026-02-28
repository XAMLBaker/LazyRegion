# LazyRegion - CLAUDE.md

## 프로젝트 개요

WPF / .NET MAUI용 **UI 애니메이션 전환 컨트롤 라이브러리**.
Content ↔ Content 간 전환 시 애니메이션을 담당하는 `LazyRegion` 컨트롤을 NuGet 패키지로 배포하는 프로젝트.

- 작성자: lukewire (GitHub: XAMLBaker)
- NuGet 패키지: `LazyRegion`, `LazyRegion.WPF`, `LazyRegion.MAUI`

---

## 디렉토리 구조

```
LazyRegion/
├── src/
│   ├── LazyRegion.Core/          # 플랫폼 독립 핵심 로직
│   ├── LazyRegion.WPF/           # WPF 전용 컨트롤
│   ├── LazyRegion.Maui/          # .NET MAUI 전용 컨트롤
│   └── Sample/                   # 용례별 예제 프로젝트
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

## 기술 스택

| 항목 | 내용 |
|---|---|
| 언어 | C# 12, Nullable enable |
| 타겟 프레임워크 | netstandard2.0/2.1, net48, net6~9 |
| UI 프레임워크 | WPF (Windows), .NET MAUI (멀티플랫폼) |
| DI 컨테이너 | `Microsoft.Extensions.DependencyInjection` 9.0.x |
| 패키지 배포 | NuGet |
| CI/CD | GitHub Actions (`.github/workflows/publish.yml`) |

---

## 주요 파일

| 파일 | 역할 |
|---|---|
| `src/LazyRegion.Core/LazyRegionBuilder.cs` | Core 빌더 패턴 진입점 |
| `src/LazyRegion.Core/LazyRegionManager.cs` | Region 관리 핵심 클래스 |
| `src/LazyRegion.Core/Interfaces/` | 핵심 인터페이스 모음 |
| `src/LazyRegion.WPF/LazyRegionControl.cs` | WPF 커스텀 컨트롤 |
| `src/LazyRegion.WPF/LazyStage.cs` | WPF Stage 컴포넌트 |
| `src/LazyRegion.WPF/Themes/Generic.xaml` | WPF 컨트롤 기본 테마 |
| `src/LazyRegion.Maui/LazyRegionControl.cs` | MAUI 커스텀 컨트롤 |
| `src/LazyRegion.Maui/LazyStage.cs` | MAUI Stage 컴포넌트 |
| `src/Sample/*/App.xaml.cs` | 각 샘플 앱 진입점 |
| `src/Sample/*/Program.cs` | 일부 샘플의 명시적 진입점 |

---

## 프로젝트 의존 관계

```
LazyRegion.WPF  ──┐
                  ├──▶  LazyRegion.Core
LazyRegion.Maui ──┘
```

- Debug 빌드: `ProjectReference` 직접 참조
- Release 빌드: Core를 패키지에 번들링 (`PrivateAssets="all"`)
