using LazyRegion.Core;
using System.Collections.ObjectModel;
using Microsoft.Maui.Controls;

namespace LazyRegion.Maui;

/// <summary>
/// MAUI용 복합 컨트롤
/// 상단에 탭/목록(LazyRegionItemsControl)이 있고,
/// 하단에 선택된 항목을 표시하는 ContentView(LazyRegion)가 있습니다.
/// </summary>
public class LazyRegionControl : ContentView, ILazyRegionItems
{
    public static readonly BindableProperty RegionNameProperty =
        BindableProperty.Create(nameof(RegionName), typeof(string), typeof(LazyRegionControl),
            default(string), propertyChanged: OnRegionNameChanged);

    public static readonly BindableProperty SelectedItemProperty =
        BindableProperty.Create(nameof(SelectedItem), typeof(object), typeof(LazyRegionControl),
            default(object), BindingMode.TwoWay, propertyChanged: OnSelectedItemChanged);

    public static readonly BindableProperty SelectedIndexProperty =
        BindableProperty.Create(nameof(SelectedIndex), typeof(int), typeof(LazyRegionControl),
            -1, BindingMode.TwoWay, propertyChanged: OnSelectedIndexChanged);

    public static readonly BindableProperty ItemsSourceTemplateProperty =
        BindableProperty.Create(nameof(ItemsSourceTemplate), typeof(DataTemplate), typeof(LazyRegionControl),
            default(DataTemplate));

    public static readonly BindableProperty SelectedContentTemplateProperty =
        BindableProperty.Create(nameof(SelectedContentTemplate), typeof(DataTemplate), typeof(LazyRegionControl),
            default(DataTemplate));

    public static readonly BindableProperty TransitionAnimationProperty =
        BindableProperty.Create(nameof(TransitionAnimation), typeof(TransitionAnimation), typeof(LazyRegionControl),
            TransitionAnimation.Fade);

    public static readonly BindableProperty TransitionDurationProperty =
        BindableProperty.Create(nameof(TransitionDuration), typeof(TimeSpan), typeof(LazyRegionControl),
            TimeSpan.FromMilliseconds(300));

    public static readonly BindableProperty ItemsLayoutProperty =
        BindableProperty.Create(nameof(ItemsLayout), typeof(IItemsLayout), typeof(LazyRegionControl),
            default(IItemsLayout));

    public string RegionName
    {
        get => (string)GetValue(RegionNameProperty);
        set => SetValue(RegionNameProperty, value);
    }

    public object SelectedItem
    {
        get => GetValue(SelectedItemProperty);
        set => SetValue(SelectedItemProperty, value);
    }

    public int SelectedIndex
    {
        get => (int)GetValue(SelectedIndexProperty);
        set => SetValue(SelectedIndexProperty, value);
    }

    public DataTemplate ItemsSourceTemplate
    {
        get => (DataTemplate)GetValue(ItemsSourceTemplateProperty);
        set => SetValue(ItemsSourceTemplateProperty, value);
    }

    public DataTemplate SelectedContentTemplate
    {
        get => (DataTemplate)GetValue(SelectedContentTemplateProperty);
        set => SetValue(SelectedContentTemplateProperty, value);
    }

    public TransitionAnimation TransitionAnimation
    {
        get => (TransitionAnimation)GetValue(TransitionAnimationProperty);
        set => SetValue(TransitionAnimationProperty, value);
    }

    public TimeSpan TransitionDuration
    {
        get => (TimeSpan)GetValue(TransitionDurationProperty);
        set => SetValue(TransitionDurationProperty, value);
    }

    public IItemsLayout ItemsLayout
    {
        get => (IItemsLayout)GetValue(ItemsLayoutProperty);
        set => SetValue(ItemsLayoutProperty, value);
    }

    public LazyRegionControl()
    {
    }

    public void AddItem(object item)
    {
        Items?.Add(item);
    }

    public void RemoveItem(object item)
    {
        Items?.Remove(item);
    }

    public void ClearItems()
    {
        Items?.Clear();
    }

    public void InsertItem(int index, object item)
    {
        Items?.Insert(index, item);
    }

    private static void OnRegionNameChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is LazyRegionControl control && !string.IsNullOrEmpty(control.RegionName))
        {
            // MAUI 버전의 Region 등록은 별도 처리 필요
        }
    }

    private static void OnSelectedItemChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is LazyRegionControl control && control.Items?.Count > 0)
        {
            var index = control.Items.IndexOf(newValue);
            if (index >= 0)
                control.SelectedIndex = index;
        }
    }

    private static void OnSelectedIndexChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is LazyRegionControl control && newValue is int index && control.Items != null && index >= 0 && index < control.Items.Count)
        {
            control.SelectedItem = control.Items[index];
        }
    }
}
