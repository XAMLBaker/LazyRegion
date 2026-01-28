using LazyRegion.Core;
using System.Collections.ObjectModel;
using Microsoft.Maui.Controls;

namespace LazyRegion.Maui;

/// <summary>
/// MAUI용 ItemsControl 기반의 LazyRegion
/// 여러 항목을 컬렉션으로 관리하고 선택된 항목을 표시합니다.
/// </summary>
public class LazyRegionItemsControl : CollectionView, ILazyRegionItems
{
    public static readonly BindableProperty RegionNameProperty =
        BindableProperty.Create(nameof(RegionName), typeof(string), typeof(LazyRegionItemsControl),
            default(string), propertyChanged: OnRegionNameChanged);

    public static readonly BindableProperty SelectedIndexProperty =
        BindableProperty.Create(nameof(SelectedIndex), typeof(int), typeof(LazyRegionItemsControl),
            -1, BindingMode.TwoWay, propertyChanged: OnSelectedIndexChanged);

    public static readonly BindableProperty TransitionAnimationProperty =
        BindableProperty.Create(nameof(TransitionAnimation), typeof(TransitionAnimation), typeof(LazyRegionItemsControl),
            TransitionAnimation.Fade);

    public static readonly BindableProperty TransitionDurationProperty =
        BindableProperty.Create(nameof(TransitionDuration), typeof(TimeSpan), typeof(LazyRegionItemsControl),
            TimeSpan.FromMilliseconds(300));

    public string RegionName
    {
        get => (string)GetValue(RegionNameProperty);
        set => SetValue(RegionNameProperty, value);
    }

    public int SelectedIndex
    {
        get => (int)GetValue(SelectedIndexProperty);
        set => SetValue(SelectedIndexProperty, value);
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

    public new ObservableCollection<object> ItemsSource
    {
        get
        {
            if (base.ItemsSource == null)
            {
                var items = new ObservableCollection<object>();
                base.ItemsSource = items;
            }
            return base.ItemsSource as ObservableCollection<object> ?? new ObservableCollection<object>();
        }
        set => base.ItemsSource = value;
    }

    public ObservableCollection<object> Items => ItemsSource;

    public LazyRegionItemsControl()
    {
        if (ItemsSource == null)
        {
            ItemsSource = new ObservableCollection<object>();
        }

        SelectionChanged += (s, e) =>
        {
            if (e.CurrentSelection.Count > 0)
            {
                var index = Items.IndexOf(e.CurrentSelection[0]);
                if (index >= 0)
                    SelectedIndex = index;
            }
        };
    }

    public void AddItem(object item)
    {
        Items.Add(item);
    }

    public void RemoveItem(object item)
    {
        Items.Remove(item);
    }

    public void ClearItems()
    {
        Items.Clear();
    }

    public void InsertItem(int index, object item)
    {
        Items.Insert(index, item);
    }

    private static void OnRegionNameChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is LazyRegionItemsControl control && !string.IsNullOrEmpty(control.RegionName))
        {
            // MAUI 버전의 Region 등록은 별도 처리 필요
            // LazyRegionRegistry.RegisterRegion(control.RegionName, (ILazyRegion)control);
        }
    }

    private static void OnSelectedIndexChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is LazyRegionItemsControl control && newValue is int index && control.Items != null && index >= 0 && index < control.Items.Count)
        {
            control.SelectedItem = control.Items[index];
        }
    }
}
