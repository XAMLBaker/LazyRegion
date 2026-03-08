using System.Threading.Tasks;

namespace LazyRegion.Core;

public interface ILazyNavigationGuard
{
    /// <summary>
    /// 네비게이션이 허용되는지 확인합니다.
    /// false를 반환하면 전환이 취소됩니다. OnNavigatedFrom도 호출되지 않습니다.
    /// </summary>
    Task<bool> CanNavigateAsync(LazyNavigationContext context);
}
