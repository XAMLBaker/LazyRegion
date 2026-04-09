using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace LazyRegion.Core;

public interface ILazyRegionBase
{
}

public interface ILazyRegion : ILazyRegionBase
{
    TransitionAnimation CurrentAnimation { get; }
    void Set(object content = null, object dataContext = null);
    void Set(object content, object dataContext, TransitionAnimation? animationOverride);
    /// <summary>현재 표시 중인 뷰의 DataContext (구 ViewModel) — Guard/OnNavigatedFrom 용</summary>
    object? GetCurrentDataContext();
    /// <summary>마지막으로 Set()된 뷰의 DataContext (신 ViewModel) — OnNavigatedTo 용</summary>
    object? GetStagingDataContext();
}


/// <summary>
/// Items 컬렉션을 관리하는 Region 인터페이스
/// ItemsControl 기반으로 여러 뷰를 동적으로 추가/제거할 수 있습니다.
/// </summary>
public interface ILazyRegionItems : ILazyRegionBase
{
    ObservableCollection<object> Items { get; }
    /// <summary>
    /// 현재 선택된 항목
    /// </summary>
    object SelectedItem { get; set; }

    /// <summary>
    /// 현재 선택된 항목의 인덱스
    /// </summary>
    int SelectedIndex { get; set; }

    /// <summary>
    /// Region에 항목 추가
    /// </summary>
    Task AddItem(string viewKey, object item);

    /// <summary>
    /// Region에서 항목 제거
    /// </summary>
    Task RemoveItem(string viewKey, object item);

    /// <summary>
    /// Region의 모든 항목 제거
    /// </summary>
    Task ClearItems();

    /// <summary>
    /// Region에 항목 삽입
    /// </summary>
    Task InsertItem(int index, string viewKey, object item);
}
