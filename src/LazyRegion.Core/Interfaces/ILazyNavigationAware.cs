namespace LazyRegion.Core;

public interface ILazyNavigationAware
{
    /// <summary>
    /// 이 ViewModel의 뷰가 Region의 활성 콘텐츠가 될 때 호출됩니다.
    /// 애니메이션 시작 직전에 호출됩니다.
    /// </summary>
    void OnNavigatedTo(LazyNavigationContext context);

    /// <summary>
    /// 이 ViewModel의 뷰에서 다른 뷰로 전환될 때 호출됩니다.
    /// 애니메이션 시작 직전, Set() 호출 전에 호출됩니다.
    /// </summary>
    void OnNavigatedFrom(LazyNavigationContext context);
}
