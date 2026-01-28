using LazyRegion.Core;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace LazyRegion.WPF;

/// <summary>
/// ItemsControl 기반의 LazyRegion
/// 여러 항목을 컬렉션으로 관리하고 선택된 항목을 표시합니다.
/// </summary>
public class LazyBoard : ListBox, ILazyRegionItems
{
    public static readonly DependencyProperty RegionNameProperty =
        DependencyProperty.Register (nameof (RegionName), typeof (string), typeof (LazyBoard),
            new PropertyMetadata (null, OnRegionNameChanged));

    public static readonly DependencyProperty ItemsCollectionProperty =
        DependencyProperty.Register (nameof (ItemsCollection), typeof (ObservableCollection<object>), typeof (LazyBoard),
            new PropertyMetadata (null, OnItemsCollectionChanged));

    public static readonly DependencyProperty TransitionAnimationProperty =
       DependencyProperty.Register (nameof (TransitionAnimation), typeof (TransitionAnimation), typeof (LazyBoard),
           new PropertyMetadata (TransitionAnimation.Fade));

    public static readonly DependencyProperty TransitionDurationProperty =
        DependencyProperty.Register (nameof (TransitionDuration), typeof (Duration), typeof (LazyBoard),
            new PropertyMetadata (new Duration (TimeSpan.FromMilliseconds (300))));

    public static readonly DependencyProperty ItemAddCommandProperty =
        DependencyProperty.Register (nameof (ItemAddCommand), typeof (ICommand), typeof (LazyBoard));

    public ICommand ItemAddCommand
    {
        get => (ICommand)GetValue (ItemAddCommandProperty);
        set => SetValue (ItemAddCommandProperty, value);
    }

    public static readonly DependencyProperty ItemRemoveCommandProperty =
        DependencyProperty.Register (nameof (ItemRemoveCommand), typeof (ICommand), typeof (LazyBoard));

    public ICommand ItemRemoveCommand
    {
        get => (ICommand)GetValue (ItemRemoveCommandProperty);
        set => SetValue (ItemRemoveCommandProperty, value);
    }

    public string RegionName
    {
        get => (string)GetValue (RegionNameProperty);
        set => SetValue (RegionNameProperty, value);
    }

    /// <summary>
    /// 아이템 컬렉션 - XAML에서 바인딩용
    /// </summary>
    public ObservableCollection<object> ItemsCollection
    {
        get => (ObservableCollection<object>)GetValue (ItemsCollectionProperty);
        set => SetValue (ItemsCollectionProperty, value);
    }

    /// <summary>
    /// ILazyRegionItems 인터페이스 구현용 Items 프로퍼티 (ItemsCollection의 별칭)
    /// </summary>
    public ObservableCollection<object> Items => ItemsCollection;
    public TransitionAnimation TransitionAnimation
    {
        get => (TransitionAnimation)GetValue (TransitionAnimationProperty);
        set => SetValue (TransitionAnimationProperty, value);
    }

    public Duration TransitionDuration
    {
        get => (Duration)GetValue (TransitionDurationProperty);
        set => SetValue (TransitionDurationProperty, value);
    }

    public static readonly RoutedEvent ItemAddedEvent =
        EventManager.RegisterRoutedEvent (nameof (ItemAdded), RoutingStrategy.Bubble, typeof (EventHandler<LaunchEventArgs>), typeof (LazyBoard));

    public event EventHandler<LaunchEventArgs> ItemAdded
    {
        add => AddHandler (ItemAddedEvent, value);
        remove => RemoveHandler (ItemAddedEvent, value);
    }

    public static readonly RoutedEvent ItemRemovedEvent =
    EventManager.RegisterRoutedEvent (nameof (ItemRemoved), RoutingStrategy.Bubble, typeof (EventHandler<LaunchEventArgs>), typeof (LazyBoard));

    public event EventHandler<LaunchEventArgs> ItemRemoved
    {
        add => AddHandler (ItemRemovedEvent, value);
        remove => RemoveHandler (ItemRemovedEvent, value);
    }

    static LazyBoard()
    {
        DefaultStyleKeyProperty.OverrideMetadata (typeof (LazyBoard),
            new FrameworkPropertyMetadata (typeof (LazyBoard)));
    }

    public LazyBoard()
    {
        // ItemsCollection 초기화
        ItemsCollection = new ObservableCollection<object> ();
    }

    private Dictionary<string, object> _loadedViews = new Dictionary<string, object> ();
    public void AddItem(string viewKey, object item)
    {
        if (_loadedViews.ContainsKey (viewKey))
            return;
        _loadedViews[viewKey] = item;
        if (!this.Dispatcher.CheckAccess ())
        {
            this.Dispatcher.Invoke (async () =>
            {
                ItemsCollection.Add (item);
            });
            return;
        }
        ItemsCollection.Add (item);
    }

    public async void RemoveItem(string viewKey, object item)
    {
        if (_loadedViews.ContainsKey (viewKey) == false)
            return;

        if (!this.Dispatcher.CheckAccess ())
        {
            await this.Dispatcher.Invoke (async () =>
            {
                await PlayLaunchRemoveAnimationAsync (item);
                ItemsCollection.Remove (item);
            });
            return;
        }
        await PlayLaunchRemoveAnimationAsync (item);
        ItemsCollection.Remove (item);
    }

    public void ClearItems()
    {
        _loadedViews.Clear ();
        if (!this.Dispatcher.CheckAccess ())
        {
            this.Dispatcher.Invoke (() => ItemsCollection.Clear ());
            return;
        }
        ItemsCollection.Clear ();
    }

    public void InsertItem(int index, string viewKey, object item)
    {
        if (_loadedViews.ContainsKey (viewKey))
            return;
        _loadedViews[viewKey] = item;
        if (!this.Dispatcher.CheckAccess ())
        {
            this.Dispatcher.Invoke (async () =>
            {
                ItemsCollection.Insert (index, item);
            });
            return;
        }
        ItemsCollection.Insert (index, item);
    }

    private static void OnRegionNameChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is LazyBoard region && !string.IsNullOrEmpty (region.RegionName))
        {
            LazyRegionRegistry.RegisterRegion (region.RegionName, region);
        }
    }

    private static void OnItemsCollectionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is LazyBoard control)
        {
            // 기존 컬렉션의 이벤트 구독 해제
            if (e.OldValue is ObservableCollection<object> oldCollection)
            {
                oldCollection.CollectionChanged += control.ItemsCollection_CollectionChanged;
            }

            // 새 컬렉션의 이벤트 구독
            if (e.NewValue is ObservableCollection<object> newCollection)
            {
                newCollection.CollectionChanged += control.ItemsCollection_CollectionChanged;
                control.ItemsSource = newCollection;  // ItemsSource 설정
            }
        }
    }

    private async void ItemsCollection_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        // Add/Remove/Insert 등 모든 컬렉션 변경에 반응
        // ItemsSource는 이미 ObservableCollection을 가리키고 있으니 UI 자동 갱신됨

        if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
        {
            var item = e.NewItems[0];
            await PlayLaunchAddAnimationAsync (item);


            var data = _loadedViews.FirstOrDefault (x => x.Value == item);
            // [개발자 입장] 외부로 알림 1: 이벤트 발생
            var args = new LaunchEventArgs (ItemAddedEvent, this, data.Key);
            RaiseEvent (args);

            // [개발자 입장] 외부로 알림 2: 커맨드 실행
            if (ItemAddCommand != null)
            {
                ItemAddCommand.Execute (data.Key);
            }
        }
        else if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove)
        {
            var item = e.OldItems[0];

            var data = _loadedViews.FirstOrDefault (x => x.Value == item);
            // [개발자 입장] 외부로 알림 1: 이벤트 발생
            var args = new LaunchEventArgs (ItemRemovedEvent, this, data.Key);
            RaiseEvent (args);

            // [개발자 입장] 외부로 알림 2: 커맨드 실행
            if (ItemRemoveCommand != null)
            {
                ItemRemoveCommand.Execute (data.Key);
            }
            _loadedViews.Remove (data.Key);
        }
    }

    protected override void OnMouseDoubleClick(MouseButtonEventArgs e)
    {
        base.OnMouseDoubleClick (e);
        if (SelectedItem != null)
        {
            var data = _loadedViews.FirstOrDefault (x => x.Value == SelectedItem);

            RemoveItem (data.Key, data.Value);
        }
    }

    private async Task PlayLaunchRemoveAnimationAsync(object view)
    {
        if (ItemContainerGenerator.ContainerFromItem (view) is FrameworkElement container)
        {
            var sb = new Storyboard ();
            var tcs = new TaskCompletionSource<bool> ();

            sb.Completed += (_, __) =>
            {
                if (!tcs.Task.IsCompleted)
                {
                    tcs.SetResult (true);
                }
            };
            if (TransitionAnimation ==  TransitionAnimation.ZoomIn || TransitionAnimation == TransitionAnimation.ZoomOut)
            {
                AddZoomAnimation (sb, container, 1, 0, TransitionDuration);
                AddFadeAnimation (sb, container, 1, 0, TransitionDuration);
            }
            else if(TransitionAnimation == TransitionAnimation.Scale)
            {
                AddZoomAnimation (sb, container, 1, 0, TransitionDuration);
            }
            else
            {
                AddFadeAnimation (sb, container, 1, 0, TransitionDuration);
            }
            sb.Begin();
            await tcs.Task;
        }
    }

    private async Task PlayLaunchAddAnimationAsync(object view)
    {
        await Dispatcher.InvokeAsync (() => { }, DispatcherPriority.Loaded);
        if (ItemContainerGenerator.ContainerFromItem (view) is FrameworkElement container)
        {
            var sb = new Storyboard ();
            var tcs = new TaskCompletionSource<bool> ();

            sb.Completed += (_, __) =>
            {
                if (!tcs.Task.IsCompleted)
                {
                    tcs.SetResult (true);
                }
            };
            switch (TransitionAnimation)
            {
                case TransitionAnimation.Fade:
                    AddFadeAnimation (sb, container, 0, 1, TransitionDuration);
                    break;
                case TransitionAnimation.SlideLeft:
                case TransitionAnimation.NewFromLeft:
                    AddSlideXAnimation (sb, container, container.ActualWidth, TransitionDuration);
                    break;
                case TransitionAnimation.SlideRight:
                case TransitionAnimation.NewFromRight:
                    AddSlideXAnimation (sb, container, -container.ActualWidth, TransitionDuration);
                    break;
                case TransitionAnimation.SlideUp:
                case TransitionAnimation.NewFromUp:
                    AddSlideYAnimation (sb, container, -container.ActualHeight, TransitionDuration);
                    break;
                case TransitionAnimation.NewFromDown:
                case TransitionAnimation.SlideDown:
                    AddSlideYAnimation (sb,container, container.ActualHeight, TransitionDuration);
                    break;
                case TransitionAnimation.ZoomIn:
                case TransitionAnimation.ZoomOut:
                    AddZoomAnimation (sb, container, 0, 1, TransitionDuration);
                    AddFadeAnimation (sb, container, 0, 1, TransitionDuration);                
                    break;
                case TransitionAnimation.Scale:
                    AddZoomAnimation (sb, container, 0, 1, TransitionDuration);
                    break;
                default:
                    break;
            }
            sb.Begin ();
            await tcs.Task;
        }
    }
    private void AddFadeAnimation(Storyboard sb, FrameworkElement target, double from, double to, Duration duration)
    {
        var anim = new DoubleAnimation (from, to, duration);
        Storyboard.SetTarget (anim, target);
        Storyboard.SetTargetProperty (anim, new PropertyPath (ContentControl.OpacityProperty));
        sb.Children.Add (anim);
    }
    private void AddSlideXAnimation(Storyboard sb, FrameworkElement target, double from, Duration duration)
    {
        var anim = new DoubleAnimation (from, 0, duration);
        Storyboard.SetTarget (anim, target);
        Storyboard.SetTargetProperty (anim, new PropertyPath ("RenderTransform.(TranslateTransform.X)"));
        sb.Children.Add (anim);
    }
    private void AddSlideYAnimation(Storyboard sb, FrameworkElement target, double from, Duration duration)
    {
        var anim = new DoubleAnimation (from, 0, duration);
        Storyboard.SetTarget (anim, target);
        Storyboard.SetTargetProperty (anim, new PropertyPath ("RenderTransform.(TranslateTransform.Y)"));
        sb.Children.Add (anim);
    }

    private void AddZoomAnimation(Storyboard sb, FrameworkElement target, double fromScale, double toScale, Duration duration)
    {
        var animX = new DoubleAnimation (fromScale, toScale, duration);
        var animY = new DoubleAnimation (fromScale, toScale, duration);

        Storyboard.SetTarget (animX, target);
        Storyboard.SetTargetProperty (animX, new PropertyPath ("RenderTransform.ScaleX"));

        Storyboard.SetTarget (animY, target);
        Storyboard.SetTargetProperty (animY, new PropertyPath ("RenderTransform.ScaleY"));

        sb.Children.Add (animX);
        sb.Children.Add (animY);
    }

    protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
    {
        base.PrepareContainerForItemOverride (element, item);

        if (element is FrameworkElement container)
        {

            switch (TransitionAnimation)
            {
                case TransitionAnimation.Fade:
                case TransitionAnimation.ZoomIn:
                case TransitionAnimation.ZoomOut:
                    container.Opacity = 0;
                    break;
                case TransitionAnimation.SlideLeft:
                case TransitionAnimation.NewFromLeft:
                case TransitionAnimation.SlideRight:
                case TransitionAnimation.NewFromRight:
                case TransitionAnimation.SlideUp:
                case TransitionAnimation.NewFromUp:
                case TransitionAnimation.NewFromDown:
                case TransitionAnimation.SlideDown:
                    container.RenderTransformOrigin = new Point (0.5, 0.5);
                    container.RenderTransform = new TranslateTransform (0, 0);
                    break;
                default:
                    break;
            }

            if(TransitionAnimation == TransitionAnimation.ZoomIn || TransitionAnimation == TransitionAnimation.ZoomOut || TransitionAnimation == TransitionAnimation.Scale)
            {
                container.RenderTransformOrigin = new Point (0.5, 0.5);
                container.RenderTransform = new ScaleTransform (0, 0);
            }
        }
    }

    public class LaunchEventArgs : RoutedEventArgs
    {
        public string ViewKey { get; }
        public LaunchEventArgs(RoutedEvent routedEvent, object source, string viewKey)
            : base (routedEvent, source) => ViewKey = viewKey;
    }
}