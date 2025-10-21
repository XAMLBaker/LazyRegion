using LazyRegion.Core;
using Microsoft.Maui.Controls.Shapes;

namespace LazyRegion.Maui;
public class LazyRegion : ContentView, ILazyRegion
{
    public static readonly BindableProperty RegionNameProperty =
        BindableProperty.Create (nameof (RegionName), typeof (string), typeof (LazyRegion), default (string), propertyChanged: OnRegionNameChanged);

    public static readonly BindableProperty TransitionAnimationProperty =
        BindableProperty.Create (nameof (TransitionAnimation), typeof (TransitionAnimation), typeof (LazyRegion), TransitionAnimation.Fade);

    public static readonly BindableProperty TransitionDurationProperty =
        BindableProperty.Create (nameof (TransitionDuration), typeof (TimeSpan), typeof (LazyRegion), TimeSpan.FromMilliseconds (300));

    // The property that consumers set to change displayed content in the region.
    public static readonly BindableProperty RegionContentProperty =
        BindableProperty.Create (nameof (RegionContent), typeof (object), typeof (LazyRegion), default (object), propertyChanged: OnRegionContentChanged);

    public string RegionName
    {
        get => (string)GetValue (RegionNameProperty);
        set => SetValue (RegionNameProperty, value);
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

    // Use this to set content (similar role to WPF ContentControl.Content)
    public object RegionContent
    {
        get => GetValue (RegionContentProperty);
        set => SetValue (RegionContentProperty, value);
    }

    private ContentView _currentPresenter;
    private ContentView _stagingPresenter;
    private bool _isNavigating;

    public LazyRegion()
    {
        // Build lightweight visual tree: Grid with two ContentViews stacked.
        var root = new Grid ();

        _currentPresenter = new ContentView
        {
            IsVisible = true,
            Opacity = 1,
            TranslationX = 0,
            TranslationY = 0,
            Scale = 1
        };

        _stagingPresenter = new ContentView
        {
            IsVisible = false,
            Opacity = 1,
            TranslationX = 0,
            TranslationY = 0,
            Scale = 1
        };
        _currentPresenter.ZIndex = 1;
        _stagingPresenter.ZIndex = 0;

        root.Add (_currentPresenter);
        root.Add (_stagingPresenter);

        Content = root;

        this.SizeChanged += (s, e) =>
        {
            if (s is ContentView g && g.Width > 0 && g.Height > 0)
            {
                this.Clip = new RectangleGeometry (new Rect (0, 0, g.Width, g.Height));
            }
        }; // 로드 완료 후 한 번만 부모 이벤트 구독
    }

    private static void OnRegionNameChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is LazyRegion region && !string.IsNullOrEmpty (region.RegionName))
        {
            LazyRegionRegistry.RegisterRegion (region.RegionName, region);
        }
    }

    private static void OnRegionContentChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is LazyRegion region)
        {
            region.HandleRegionContentChanged (oldValue, newValue);
        }
    }

    private void HandleRegionContentChanged(object oldContent, object newContent)
    {
        MainThread.BeginInvokeOnMainThread (() => {
           _ = NavigationInternal (newContent);
        });
    }

    // Public API similar to WPF Set
    public void Set(object content, object dataContext = null)
    {
        RegionContent = content;
        if (dataContext != null && _currentPresenter != null)
        {
            _currentPresenter.BindingContext = dataContext;
        }
    }

    private void ResetInitTransforms(ContentView presenter)
    {
        if (presenter == null)
            return;

        presenter.AbortAnimation ("transition"); // best-effort cancel
        presenter.TranslationX = 0;
        presenter.TranslationY = 0;
        presenter.Scale = 1;
        presenter.Opacity = 1;
    }

    private async Task NavigationInternal(object newContent)
    {
        if (_currentPresenter == null || _stagingPresenter == null)
            return;

        if (_isNavigating)
            return;

        _isNavigating = true;

        if (newContent is View content)
        {
            _stagingPresenter.Content = content;
        }

        // Prepare presenters
        ResetInitTransforms (_currentPresenter);
        ResetInitTransforms (_stagingPresenter);

        // Bring incoming on top when appropriate
        _currentPresenter.ZIndex = 0;
        _stagingPresenter.ZIndex = 1;

        _stagingPresenter.IsVisible = true;

        var durationMs = (int)TransitionDuration.TotalMilliseconds;
        var easing = Easing.CubicInOut;

        Task incomingTask = Task.CompletedTask;
        Task outgoingTask = Task.CompletedTask;

        switch (TransitionAnimation)
        {
            case TransitionAnimation.Fade:
                _stagingPresenter.Opacity = 0;
                incomingTask = _stagingPresenter.FadeTo (1, (uint)durationMs, easing);
                outgoingTask = _currentPresenter.FadeTo (0, (uint)durationMs, easing);
                break;

            case TransitionAnimation.NewFromLeft:
                _stagingPresenter.TranslationX = -Width;
                incomingTask = _stagingPresenter.TranslateTo (0, 0, (uint)durationMs, easing);
                break;

            case TransitionAnimation.NewFromRight:
                _stagingPresenter.TranslationX = Width;
                incomingTask = _stagingPresenter.TranslateTo (0, 0, (uint)durationMs, easing);
                break;

            case TransitionAnimation.NewFromUp:
                _stagingPresenter.TranslationY = -Height;
                incomingTask = _stagingPresenter.TranslateTo (0, 0, (uint)durationMs, easing);
                break;

            case TransitionAnimation.NewFromDown:
                _stagingPresenter.TranslationY = Height;
                incomingTask = _stagingPresenter.TranslateTo (0, 0, (uint)durationMs, easing);
                break;

            case TransitionAnimation.SlideLeft:
                outgoingTask = _currentPresenter.TranslateTo (-Width, 0, (uint)durationMs, easing);
                _stagingPresenter.TranslationX = Width;
                incomingTask = _stagingPresenter.TranslateTo (0, 0, (uint)durationMs, easing);
                break;

            case TransitionAnimation.SlideRight:
                outgoingTask = _currentPresenter.TranslateTo (Width, 0, (uint)durationMs, easing);
                _stagingPresenter.TranslationX = -Width;
                incomingTask = _stagingPresenter.TranslateTo (0, 0, (uint)durationMs, easing);
                break;

            case TransitionAnimation.SlideDown:
                outgoingTask = _currentPresenter.TranslateTo (0, Height, (uint)durationMs, easing);
                _stagingPresenter.TranslationY = -Height;
                incomingTask = _stagingPresenter.TranslateTo (0, 0, (uint)durationMs, easing);
                break;

            case TransitionAnimation.SlideUp:
                outgoingTask = _currentPresenter.TranslateTo (0, -Height, (uint)durationMs, easing);
                _stagingPresenter.TranslationY = Height;
                incomingTask = _stagingPresenter.TranslateTo (0, 0, (uint)durationMs, easing);
                break;

            case TransitionAnimation.ZoomIn:
                _stagingPresenter.Scale = 0.5;
                _stagingPresenter.Opacity = 0;
                incomingTask = Task.WhenAll (_stagingPresenter.ScaleTo (1, (uint)durationMs, easing), _stagingPresenter.FadeTo (1, (uint)durationMs, easing));
                break;

            case TransitionAnimation.ZoomOut:
                // Make outgoing appear on top while zooming out
                _currentPresenter.ZIndex = 1;
                _stagingPresenter.ZIndex = 0;
                outgoingTask = Task.WhenAll (_currentPresenter.ScaleTo (0.8, (uint)durationMs, easing), _currentPresenter.FadeTo (0, (uint)durationMs, easing));
                break;

            case TransitionAnimation.Scale:
                _stagingPresenter.Scale = 0;
                incomingTask = _stagingPresenter.ScaleTo (1, (uint)durationMs, easing);
                outgoingTask = _currentPresenter.ScaleTo (0, (uint)durationMs, easing);
                break;

            default:
                // No animation: immediate swap
                break;
        }

        // Wait for animations (if any)
        try
        {
            await Task.WhenAll (incomingTask, outgoingTask);
        }
        catch
        {
            // ignore animation cancellation exceptions
        }

        // Complete swap
        CompleteTransition ();
    }

    private void CompleteTransition()
    {
        // Clear previous content
        _currentPresenter.Content = null;
        _currentPresenter.IsVisible = false;

        // Swap presenters
        (_currentPresenter, _stagingPresenter) = (_stagingPresenter, _currentPresenter);

        _currentPresenter.ZIndex = 1;
        _stagingPresenter.ZIndex = 0;

        _currentPresenter.IsVisible = true;
        _stagingPresenter.IsVisible = false;

        // Reset transforms for both
        ResetInitTransforms (_currentPresenter);
        ResetInitTransforms (_stagingPresenter);

        _isNavigating = false;
    }
}