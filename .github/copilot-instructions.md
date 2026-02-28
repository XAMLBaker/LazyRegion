# Copilot Instructions

## 프로젝트 지침
- Prefer keeping LazyRegionRegistry.OnManagerRequested lazy initialization to allow UseWpf to be called in any order; also prefer providing UseWpf(Action<LazyRegionApp>) overload for safer configuration.