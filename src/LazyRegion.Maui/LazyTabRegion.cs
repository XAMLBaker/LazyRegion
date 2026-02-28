using LazyRegion.Core;
using Microsoft.Maui.Controls.Shapes;
using System.Windows.Input;

namespace LazyRegion.Maui;

public enum NavigationBarPlacement
{
    Top,
    Bottom
}

[ContentProperty ("Items")]
public class LazyTabRegion : ContentView
{
    #region BindableProperties

    public static readonly BindableProperty ItemsProperty =
        BindableProperty.Create (nameof (Items), typeof (IList<LazyTabItem>), typeof (LazyTabRegion),
            defaultValueCreator: _ => new List<LazyTabItem> ());

    public static readonly BindableProperty SelectedIndexProperty =
        BindableProperty.Create (nameof (SelectedIndex), typeof (int), typeof (LazyTabRegion),
            0, BindingMode.TwoWay, propertyChanged: OnSelectedIndexChanged);

    public static readonly BindableProperty NavigationBarProperty =
        BindableProperty.Create (nameof (NavigationBar), typeof (View), typeof (LazyTabRegion),
            null, propertyChanged: OnNavigationBarChanged);

    public static readonly BindableProperty NavigationBarPlacementProperty =
        BindableProperty.Create (nameof (NavigationBarPlacement), typeof (NavigationBarPlacement), typeof (LazyTabRegion),
            NavigationBarPlacement.Bottom, propertyChanged: OnNavigationBarPlacementChanged);

    public static readonly BindableProperty TransitionAnimationProperty =
        BindableProperty.Create (nameof (TransitionAnimation), typeof (TransitionAnimation), typeof (LazyTabRegion),
            TransitionAnimation.SlideLeft);

    public static readonly BindableProperty TransitionDurationProperty =
        BindableProperty.Create (nameof (TransitionDuration), typeof (TimeSpan), typeof (LazyTabRegion),
            TimeSpan.FromMilliseconds (300));

    public static readonly BindableProperty SwipeThresholdProperty =
        BindableProperty.Create (nameof (SwipeThreshold), typeof (double), typeof (LazyTabRegion), 0.5);

    private static readonly BindablePropertyKey ItemCountPropertyKey =
        BindableProperty.CreateReadOnly (nameof (ItemCount), typeof (int), typeof (LazyTabRegion), 0);

    public static readonly BindableProperty ItemCountProperty = ItemCountPropertyKey.BindableProperty;

    #endregion

    #region Properties

    public IList<LazyTabItem> Items
    {
        get => (IList<LazyTabItem>)GetValue (ItemsProperty);
        set => SetValue (ItemsProperty, value);
    }

    public int SelectedIndex
    {
        get => (int)GetValue (SelectedIndexProperty);
        set => SetValue (SelectedIndexProperty, value);
    }

    public View? NavigationBar
    {
        get => (View?)GetValue (NavigationBarProperty);
        set => SetValue (NavigationBarProperty, value);
    }

    public NavigationBarPlacement NavigationBarPlacement
    {
        get => (NavigationBarPlacement)GetValue (NavigationBarPlacementProperty);
        set => SetValue (NavigationBarPlacementProperty, value);
    }

    public TransitionAnimation TransitionAnimation
    {
        get => (TransitionAnimation)GetValue (TransitionAnimationProperty);
        set => SetValue (TransitionAnimationProperty, value);
    }

    public TimeSpan TransitionDuration
    {
        get => (TimeSpan)GetValue (TransitionDurationProperty);
        set => SetValue (TransitionDurationProperty, value);
    }

    public double SwipeThreshold
    {
        get => (double)GetValue (SwipeThresholdProperty);
        set => SetValue (SwipeThresholdProperty, value);
    }

    public int ItemCount
    {
        get => (int)GetValue (ItemCountProperty);
        private set => SetValue (ItemCountPropertyKey, value);
    }

    public ICommand SelectCommand { get; }

    /// <summary>
    /// SelectedIndex가 변경될 때 발생합니다.
    /// </summary>
    public event EventHandler<int>? SelectionChanged;

    #endregion

    #region Internal State

    private readonly Grid _root;
    private readonly Grid _contentArea;
    private ContentView _currentPresenter;
    private ContentView _stagingPresenter;

    private readonly RowDefinition _navBarRow;
    private readonly RowDefinition _contentRow;

    private bool _isTransitioning;
    private bool _isSwiping;
    private int _swipeTargetIndex = -1;
    private int _currentIndex = -1;

    #endregion

    public LazyTabRegion()
    {
        SelectCommand = new Command<string> (param =>
        {
            if (int.TryParse (param, out var index))
                SelectedIndex = index;
        });

        _currentPresenter = new ContentView
        {
            IsVisible = true, Opacity = 1,
            TranslationX = 0, TranslationY = 0, Scale = 1
        };
        _currentPresenter.ZIndex = 1;

        _stagingPresenter = new ContentView
        {
            IsVisible = false, Opacity = 1,
            TranslationX = 0, TranslationY = 0, Scale = 1
        };
        _stagingPresenter.ZIndex = 0;

        _contentArea = new Grid ();
        _contentArea.Add (_currentPresenter);
        _contentArea.Add (_stagingPresenter);

        _contentArea.SizeChanged += (s, e) =>
        {
            if (s is Grid g && g.Width > 0 && g.Height > 0)
                _contentArea.Clip = new RectangleGeometry (new Rect (0, 0, g.Width, g.Height));
        };

        // PanGesture
        var pan = new PanGestureRecognizer ();
        pan.PanUpdated += OnPanUpdated;
        _contentArea.GestureRecognizers.Add (pan);

        // Root layout
        _navBarRow = new RowDefinition (GridLength.Auto);
        _contentRow = new RowDefinition (GridLength.Star);

        _root = new Grid ();
        ApplyRowLayout ();

        Content = _root;

        // Show initial item when loaded
        this.Loaded += OnLoaded;
    }

    private void OnLoaded(object? sender, EventArgs e)
    {
        this.Loaded -= OnLoaded;
        ItemCount = Items.Count;
        if (Items.Count > 0 && SelectedIndex >= 0 && SelectedIndex < Items.Count)
        {
            _currentIndex = SelectedIndex;
            _currentPresenter.Content = ResolveView (Items[_currentIndex]);
        }
    }

    private View ResolveView(LazyTabItem item)
    {
        var obj = LazyRegionApp.Default.RegionManager.GetView (item.Key);
        return obj as View
            ?? throw new InvalidOperationException (
                $"View for key '{item.Key}' is not a MAUI View.");
    }

    #region Layout

    private void ApplyRowLayout()
    {
        _root.RowDefinitions.Clear ();
        _root.Children.Clear ();

        if (NavigationBarPlacement == NavigationBarPlacement.Top)
        {
            _root.RowDefinitions.Add (_navBarRow);
            _root.RowDefinitions.Add (_contentRow);

            if (NavigationBar != null)
            {
                Grid.SetRow (NavigationBar, 0);
                _root.Add (NavigationBar);
            }
            Grid.SetRow (_contentArea, 1);
            _root.Add (_contentArea);
        }
        else // Bottom
        {
            _root.RowDefinitions.Add (_contentRow);
            _root.RowDefinitions.Add (_navBarRow);

            Grid.SetRow (_contentArea, 0);
            _root.Add (_contentArea);

            if (NavigationBar != null)
            {
                Grid.SetRow (NavigationBar, 1);
                _root.Add (NavigationBar);
            }
        }
    }

    #endregion

    #region Property Changed Handlers

    private static void OnSelectedIndexChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is LazyTabRegion tab && oldValue is int oldIdx && newValue is int newIdx)
        {
            tab.HandleSelectedIndexChanged (oldIdx, newIdx);
        }
    }

    private static void OnNavigationBarChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is LazyTabRegion tab)
            tab.ApplyRowLayout ();
    }

    private static void OnNavigationBarPlacementChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is LazyTabRegion tab)
            tab.ApplyRowLayout ();
    }

    #endregion

    #region Tab Transition

    private void HandleSelectedIndexChanged(int oldIndex, int newIndex)
    {
        SelectionChanged?.Invoke (this, newIndex);

        if (_isSwiping) return;
        if (Items.Count == 0) return;
        if (newIndex < 0 || newIndex >= Items.Count) return;
        if (oldIndex == newIndex) return;
        if (_currentIndex == newIndex) return;

        var direction = newIndex > oldIndex ? NavigationDirection.Forward : NavigationDirection.Backward;
        _ = PerformTabTransition (newIndex, direction);
    }

    private async Task PerformTabTransition(int targetIndex, NavigationDirection direction)
    {
        if (_isTransitioning) return;
        _isTransitioning = true;

        try
        {
            _stagingPresenter.Content = ResolveView (Items[targetIndex]);
            _stagingPresenter.IsVisible = true;

            var resolved = ResolveAnimation (TransitionAnimation, direction);

            if (resolved != TransitionAnimation.None)
                await RunAnimation (_currentPresenter, _stagingPresenter, resolved, TransitionDuration);

            CompleteTransition ();
            _currentIndex = targetIndex;
            ItemCount = Items.Count;
        }
        finally
        {
            _isTransitioning = false;
        }
    }

    private TransitionAnimation ResolveAnimation(TransitionAnimation animation, NavigationDirection direction)
    {
        if (direction == NavigationDirection.Forward)
            return animation;

        return animation switch
        {
            TransitionAnimation.SlideLeft => TransitionAnimation.SlideRight,
            TransitionAnimation.SlideRight => TransitionAnimation.SlideLeft,
            TransitionAnimation.SlideUp => TransitionAnimation.SlideDown,
            TransitionAnimation.SlideDown => TransitionAnimation.SlideUp,
            _ => animation
        };
    }

    #endregion

    #region Animation (LazyStage 패턴 재활용)

    private async Task RunAnimation(ContentView outgoing, ContentView incoming, TransitionAnimation animation, TimeSpan duration)
    {
        outgoing.CancelAnimations ();
        incoming.CancelAnimations ();

        ResetTransforms (outgoing);
        ResetTransforms (incoming);

        outgoing.ZIndex = 0;
        incoming.ZIndex = 1;

        var ms = (uint)duration.TotalMilliseconds;
        var easing = Easing.CubicInOut;

        Task inTask = Task.CompletedTask;
        Task outTask = Task.CompletedTask;

        var w = _contentArea.Width;
        var h = _contentArea.Height;

        switch (animation)
        {
            case TransitionAnimation.Fade:
                incoming.Opacity = 0;
                inTask = incoming.FadeTo (1, ms, easing);
                outTask = outgoing.FadeTo (0, ms, easing);
                break;

            case TransitionAnimation.NewFromLeft:
                incoming.TranslationX = -w;
                inTask = incoming.TranslateTo (0, 0, ms, easing);
                break;

            case TransitionAnimation.NewFromRight:
                incoming.TranslationX = w;
                inTask = incoming.TranslateTo (0, 0, ms, easing);
                break;

            case TransitionAnimation.NewFromUp:
                incoming.TranslationY = -h;
                inTask = incoming.TranslateTo (0, 0, ms, easing);
                break;

            case TransitionAnimation.NewFromDown:
                incoming.TranslationY = h;
                inTask = incoming.TranslateTo (0, 0, ms, easing);
                break;

            case TransitionAnimation.SlideLeft:
                outTask = outgoing.TranslateTo (-w, 0, ms, easing);
                incoming.TranslationX = w;
                inTask = incoming.TranslateTo (0, 0, ms, easing);
                break;

            case TransitionAnimation.SlideRight:
                outTask = outgoing.TranslateTo (w, 0, ms, easing);
                incoming.TranslationX = -w;
                inTask = incoming.TranslateTo (0, 0, ms, easing);
                break;

            case TransitionAnimation.SlideUp:
                outTask = outgoing.TranslateTo (0, -h, ms, easing);
                incoming.TranslationY = h;
                inTask = incoming.TranslateTo (0, 0, ms, easing);
                break;

            case TransitionAnimation.SlideDown:
                outTask = outgoing.TranslateTo (0, h, ms, easing);
                incoming.TranslationY = -h;
                inTask = incoming.TranslateTo (0, 0, ms, easing);
                break;

            case TransitionAnimation.Scale:
                incoming.Scale = 0;
                inTask = incoming.ScaleTo (1, ms, easing);
                outTask = outgoing.ScaleTo (0, ms, easing);
                break;

            case TransitionAnimation.ZoomIn:
                incoming.Scale = 0.5;
                incoming.Opacity = 0;
                inTask = Task.WhenAll (incoming.ScaleTo (1, ms, easing), incoming.FadeTo (1, ms, easing));
                break;

            case TransitionAnimation.ZoomOut:
                outgoing.ZIndex = 1;
                incoming.ZIndex = 0;
                outTask = Task.WhenAll (outgoing.ScaleTo (0.8, ms, easing), outgoing.FadeTo (0, ms, easing));
                break;
        }

        try { await Task.WhenAll (inTask, outTask); }
        catch { /* animation cancellation */ }
    }

    private void CompleteTransition()
    {
        _currentPresenter.Content = null;
        ResetPresenter (_currentPresenter);

        (_currentPresenter, _stagingPresenter) = (_stagingPresenter, _currentPresenter);

        _currentPresenter.ZIndex = 1;
        _currentPresenter.IsVisible = true;

        _stagingPresenter.ZIndex = 0;
        _stagingPresenter.IsVisible = false;
    }

    private void ResetTransforms(ContentView presenter)
    {
        presenter.TranslationX = 0;
        presenter.TranslationY = 0;
        presenter.Scale = 1;
        presenter.Opacity = 1;
    }

    private void ResetPresenter(ContentView presenter)
    {
        presenter.CancelAnimations ();
        presenter.IsVisible = false;
        presenter.TranslationX = 0;
        presenter.TranslationY = 0;
        presenter.Scale = 1;
        presenter.Opacity = 1;
        presenter.ZIndex = 0;
    }

    #endregion

    #region Swipe Gesture

    private bool IsSwipeEnabled => TransitionAnimation is
        TransitionAnimation.SlideLeft or TransitionAnimation.SlideRight or
        TransitionAnimation.SlideUp or TransitionAnimation.SlideDown;

    private bool IsHorizontalSwipe => TransitionAnimation is
        TransitionAnimation.SlideLeft or TransitionAnimation.SlideRight;

    private void OnPanUpdated(object? sender, PanUpdatedEventArgs e)
    {
        if (!IsSwipeEnabled) return;
        if (_isTransitioning) return;

        switch (e.StatusType)
        {
            case GestureStatus.Started:
                break;

            case GestureStatus.Running:
                HandlePanRunning (e.TotalX, e.TotalY);
                break;

            case GestureStatus.Completed:
                HandlePanCompleted ();
                break;

            case GestureStatus.Canceled:
                HandlePanCancelled ();
                break;
        }
    }

    private void HandlePanRunning(double totalX, double totalY)
    {
        var isHorizontal = IsHorizontalSwipe;
        var displacement = isHorizontal ? totalX : totalY;
        var dimension = isHorizontal ? _contentArea.Width : _contentArea.Height;

        if (dimension <= 0) return;

        // Determine target index based on swipe direction
        int targetIndex;
        if (displacement < 0) // swipe left/up → forward
            targetIndex = _currentIndex + 1;
        else // swipe right/down → backward
            targetIndex = _currentIndex - 1;

        // Boundary check
        if (targetIndex < 0 || targetIndex >= Items.Count)
        {
            // If swiping in a direction with no target, apply rubber-band resistance
            if (_isSwiping)
                CancelSwipeAsync ();
            return;
        }

        // Clamp displacement
        displacement = Math.Clamp (displacement, -dimension, dimension);

        // Initialize swipe if not started
        if (!_isSwiping || _swipeTargetIndex != targetIndex)
        {
            // If target changed mid-swipe, reset staging
            if (_isSwiping && _swipeTargetIndex != targetIndex)
            {
                _stagingPresenter.Content = null;
                ResetTransforms (_stagingPresenter);
            }

            _isSwiping = true;
            _swipeTargetIndex = targetIndex;
            _stagingPresenter.Content = ResolveView (Items[targetIndex]);
            _stagingPresenter.IsVisible = true;
            _stagingPresenter.ZIndex = 1;
            _currentPresenter.ZIndex = 0;
        }

        // Move presenters to follow finger
        if (isHorizontal)
        {
            _currentPresenter.TranslationX = displacement;
            if (displacement < 0) // forward: staging comes from right
                _stagingPresenter.TranslationX = dimension + displacement;
            else // backward: staging comes from left
                _stagingPresenter.TranslationX = -dimension + displacement;
        }
        else
        {
            _currentPresenter.TranslationY = displacement;
            if (displacement < 0) // forward: staging comes from bottom
                _stagingPresenter.TranslationY = dimension + displacement;
            else // backward: staging comes from top
                _stagingPresenter.TranslationY = -dimension + displacement;
        }
    }

    private void HandlePanCompleted()
    {
        if (!_isSwiping) return;

        var isHorizontal = IsHorizontalSwipe;
        var dimension = isHorizontal ? _contentArea.Width : _contentArea.Height;
        var currentDisplacement = isHorizontal
            ? _currentPresenter.TranslationX
            : _currentPresenter.TranslationY;
        var progress = Math.Abs (currentDisplacement) / dimension;

        if (progress >= SwipeThreshold)
            _ = CommitSwipeAsync (dimension, isHorizontal);
        else
            _ = CancelSwipeAsync ();
    }

    private void HandlePanCancelled()
    {
        if (_isSwiping)
            _ = CancelSwipeAsync ();
    }

    private async Task CommitSwipeAsync(double dimension, bool isHorizontal)
    {
        var currentDisplacement = isHorizontal
            ? _currentPresenter.TranslationX
            : _currentPresenter.TranslationY;
        var isForward = currentDisplacement < 0;
        var targetDisplacement = isForward ? -dimension : dimension;

        var remaining = Math.Abs (targetDisplacement - currentDisplacement) / dimension;
        var ms = (uint)(remaining * TransitionDuration.TotalMilliseconds);
        ms = Math.Max (ms, 50); // minimum 50ms

        var easing = Easing.CubicOut;

        try
        {
            if (isHorizontal)
            {
                await Task.WhenAll (
                    _currentPresenter.TranslateTo (targetDisplacement, 0, ms, easing),
                    _stagingPresenter.TranslateTo (0, 0, ms, easing));
            }
            else
            {
                await Task.WhenAll (
                    _currentPresenter.TranslateTo (0, targetDisplacement, ms, easing),
                    _stagingPresenter.TranslateTo (0, 0, ms, easing));
            }
        }
        catch { /* animation cancellation */ }

        var targetIndex = _swipeTargetIndex;

        CompleteTransition ();
        _currentIndex = targetIndex;

        _isSwiping = false;
        _swipeTargetIndex = -1;

        // Update SelectedIndex without triggering another transition
        _isSwiping = true;
        SelectedIndex = targetIndex;
        _isSwiping = false;

        ItemCount = Items.Count;
    }

    private async Task CancelSwipeAsync()
    {
        if (!_isSwiping) return;

        var isHorizontal = IsHorizontalSwipe;
        var currentDisplacement = isHorizontal
            ? _currentPresenter.TranslationX
            : _currentPresenter.TranslationY;
        var dimension = isHorizontal ? _contentArea.Width : _contentArea.Height;

        var progress = dimension > 0 ? Math.Abs (currentDisplacement) / dimension : 0;
        var ms = (uint)(progress * TransitionDuration.TotalMilliseconds);
        ms = Math.Max (ms, 50);

        var easing = Easing.CubicOut;

        var stagingTarget = currentDisplacement < 0
            ? (isHorizontal ? dimension : dimension)
            : (isHorizontal ? -dimension : -dimension);

        try
        {
            if (isHorizontal)
            {
                await Task.WhenAll (
                    _currentPresenter.TranslateTo (0, 0, ms, easing),
                    _stagingPresenter.TranslateTo (stagingTarget, 0, ms, easing));
            }
            else
            {
                await Task.WhenAll (
                    _currentPresenter.TranslateTo (0, 0, ms, easing),
                    _stagingPresenter.TranslateTo (0, stagingTarget, ms, easing));
            }
        }
        catch { /* animation cancellation */ }

        // Clean up staging
        _stagingPresenter.Content = null;
        ResetPresenter (_stagingPresenter);

        // Restore current
        _currentPresenter.ZIndex = 1;
        ResetTransforms (_currentPresenter);

        _isSwiping = false;
        _swipeTargetIndex = -1;
    }

    #endregion

    private enum NavigationDirection
    {
        Forward,
        Backward
    }
}
