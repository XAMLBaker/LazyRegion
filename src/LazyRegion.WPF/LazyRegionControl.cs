using LazyRegion.Core;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace LazyRegion.WPF;

/// <summary>
/// 상단에 탭/목록(LazyRegionItemsControl)이 있고,
/// 하단에 선택된 항목을 표시하는 ContentControl(LazyRegion)이 있는 복합 컨트롤
/// </summary>
public class LazyRegionControl : Control, ILazyRegionItems
{
    public static readonly DependencyProperty RegionNameProperty =
        DependencyProperty.Register(nameof(RegionName), typeof(string), typeof(LazyRegionControl),
            new PropertyMetadata(null, OnRegionNameChanged));

    public static readonly DependencyProperty ItemsProperty =
        DependencyProperty.Register(nameof(Items), typeof(ObservableCollection<object>), typeof(LazyRegionControl),
            new PropertyMetadata(null));

    public static readonly DependencyProperty SelectedItemProperty =
        DependencyProperty.Register(nameof(SelectedItem), typeof(object), typeof(LazyRegionControl),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnSelectedItemChanged));

    public static readonly DependencyProperty SelectedIndexProperty =
        DependencyProperty.Register(nameof(SelectedIndex), typeof(int), typeof(LazyRegionControl),
            new FrameworkPropertyMetadata(-1, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnSelectedIndexChanged));

    public static readonly DependencyProperty ItemsControlTemplateProperty =
        DependencyProperty.Register(nameof(ItemsControlTemplate), typeof(DataTemplate), typeof(LazyRegionControl),
            new PropertyMetadata(null));

    public static readonly DependencyProperty ContentControlTemplateProperty =
        DependencyProperty.Register(nameof(ContentControlTemplate), typeof(DataTemplate), typeof(LazyRegionControl),
            new PropertyMetadata(null));

    public static readonly DependencyProperty TransitionAnimationProperty =
        DependencyProperty.Register(nameof(TransitionAnimation), typeof(TransitionAnimation), typeof(LazyRegionControl),
            new PropertyMetadata(TransitionAnimation.Fade));

    public static readonly DependencyProperty TransitionDurationProperty =
        DependencyProperty.Register(nameof(TransitionDuration), typeof(Duration), typeof(LazyRegionControl),
            new PropertyMetadata(new Duration(TimeSpan.FromMilliseconds(300))));

    public static readonly DependencyProperty ItemsPanelTemplateProperty =
        DependencyProperty.Register(nameof(ItemsPanelTemplate), typeof(ItemsPanelTemplate), typeof(LazyRegionControl),
            new PropertyMetadata(null));

    public string RegionName
    {
        get => (string)GetValue(RegionNameProperty);
        set => SetValue(RegionNameProperty, value);
    }

    public ObservableCollection<object> Items
    {
        get => (ObservableCollection<object>)GetValue(ItemsProperty);
        set => SetValue(ItemsProperty, value);
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

    public DataTemplate ItemsControlTemplate
    {
        get => (DataTemplate)GetValue(ItemsControlTemplateProperty);
        set => SetValue(ItemsControlTemplateProperty, value);
    }

    public DataTemplate ContentControlTemplate
    {
        get => (DataTemplate)GetValue(ContentControlTemplateProperty);
        set => SetValue(ContentControlTemplateProperty, value);
    }

    public TransitionAnimation TransitionAnimation
    {
        get => (TransitionAnimation)GetValue(TransitionAnimationProperty);
        set => SetValue(TransitionAnimationProperty, value);
    }

    public Duration TransitionDuration
    {
        get => (Duration)GetValue(TransitionDurationProperty);
        set => SetValue(TransitionDurationProperty, value);
    }

    public ItemsPanelTemplate ItemsPanelTemplate
    {
        get => (ItemsPanelTemplate)GetValue(ItemsPanelTemplateProperty);
        set => SetValue(ItemsPanelTemplateProperty, value);
    }

    private LazyRegionItemsControl _itemsControl;
    private LazyRegion _contentRegion;

    static LazyRegionControl()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(LazyRegionControl),
            new FrameworkPropertyMetadata(typeof(LazyRegionControl)));
    }

    public LazyRegionControl()
    {
        var items = new ObservableCollection<object>();
        SetValue(ItemsProperty, items);
    }

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        _itemsControl = GetTemplateChild("PART_ItemsControl") as LazyRegionItemsControl;
        _contentRegion = GetTemplateChild("PART_ContentRegion") as LazyRegion;

        if (_itemsControl != null)
        {
            _itemsControl.ItemsCollection = Items;
            if (ItemsControlTemplate != null)
                _itemsControl.ItemTemplate = ItemsControlTemplate;
            if (ItemsPanelTemplate != null)
                _itemsControl.ItemsPanel = ItemsPanelTemplate;

            // ItemsControl의 SelectedItem을 이 컨트롤의 SelectedItem과 바인딩
            _itemsControl.SetBinding(LazyRegionItemsControl.SelectedItemProperty,
                new System.Windows.Data.Binding(nameof(SelectedItem)) { Source = this, Mode = System.Windows.Data.BindingMode.TwoWay });
        }

        if (_contentRegion != null)
        {
            if (ContentControlTemplate != null)
                _contentRegion.ContentTemplate = ContentControlTemplate;

            // ContentRegion의 Content를 SelectedItem과 바인딩
            _contentRegion.SetBinding(LazyRegion.ContentProperty,
                new System.Windows.Data.Binding(nameof(SelectedItem)) { Source = this });
        }
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

    private static void OnRegionNameChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is LazyRegionControl region && !string.IsNullOrEmpty(region.RegionName))
        {
            LazyRegionRegistry.RegisterRegion(region.RegionName, region);
        }
    }

    private static void OnSelectedItemChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is LazyRegionControl control && control.Items?.Count > 0)
        {
            var index = control.Items.IndexOf(e.NewValue);
            if (index >= 0)
                control.SetValue(SelectedIndexProperty, index);
        }
    }

    private static void OnSelectedIndexChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is LazyRegionControl control && e.NewValue is int index && control.Items != null && index >= 0 && index < control.Items.Count)
        {
            control.SetValue(SelectedItemProperty, control.Items[index]);
        }
    }
}
