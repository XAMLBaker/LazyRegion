using LazyRegion.Core;
using Microsoft.Maui.Controls.Shapes;

namespace LazyRegion.Maui;

/// <summary>
/// (KO) LazyRegion의 새로운 이름입니다. 무대(Stage) 기반의 세련된 전환을 제공합니다.
/// (EN) The new identity of LazyRegion. Provides sophisticated stage-based transitions.
/// </summary>
[Obsolete ("LazyRegion is now LazyStage. Please migrate to LazyStage for the new world.")]
public class LazyRegion : LazyStage
{
}

public class LazyStage : ContentView, ILazyRegion
{
    public static readonly BindableProperty RegionNameProperty =
        BindableProperty.Create (nameof (RegionName), typeof (string), typeof (LazyStage), default (string), propertyChanged: OnRegionNameChanged);

    public static readonly BindableProperty TransitionAnimationProperty =
        BindableProperty.Create (nameof (TransitionAnimation), typeof (TransitionAnimation), typeof (LazyStage), TransitionAnimation.Fade);

    public static readonly BindableProperty TransitionDurationProperty =
        BindableProperty.Create (nameof (TransitionDuration), typeof (TimeSpan), typeof (LazyStage), TimeSpan.FromMilliseconds (300));

    // The property that consumers set to change displayed content in the region.
    public static readonly BindableProperty RegionContentProperty =
        BindableProperty.Create (nameof (RegionContent), typeof (object), typeof (LazyStage), default (object), propertyChanged: OnRegionContentChanged);

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
    private TransitionAnimation? _animationOverride;
    private object? _currentDataContext;
    private object? _stagingDataContext;

    public LazyStage()
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
        if (bindable is LazyStage region && !string.IsNullOrEmpty (region.RegionName))
        {
            LazyRegionRegistry.RegisterRegion (region.RegionName, region);
        }
    }

    private static void OnRegionContentChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is LazyStage region)
        {
            region.HandleRegionContentChanged (oldValue, newValue);
        }
    }

    private void HandleRegionContentChanged(object oldContent, object newContent)
    {
        try
        {
            // 항상 MainThread에서 처리하되, 중복 호출 방지
            if (MainThread.IsMainThread)
            {
                // 이미 UI 스레드에서는 즉시 실행하지 않고 다음 UI cycle로 연기
                Dispatcher.Dispatch(async () =>
                {
                    try
                    {
                        await InternalNavigation(newContent);
                    }
                    catch (Exception)
                    {
                        // TODO: Add logging
                    }
                });
            }
            else
            {
                MainThread.BeginInvokeOnMainThread(async () =>
                {
                    try
                    {
                        await InternalNavigation(newContent);
                    }
                    catch (Exception)
                    {
                        // TODO: Add logging
                    }
                });
            }
        }
        catch (Exception)
        {
            // TODO: Add logging
        }
    }

    public TransitionAnimation CurrentAnimation => TransitionAnimation;

    // Public API similar to WPF Set
    public void Set(object content, object dataContext = null)
    {
        if (dataContext != null && content is View v)
            v.BindingContext = dataContext;

        _stagingDataContext = content is BindableObject bo ? bo.BindingContext : dataContext;
        RegionContent = content;
    }

    public void Set(object content, object dataContext, TransitionAnimation? animationOverride)
    {
        _animationOverride = animationOverride;
        Set(content, dataContext);
    }

    public object? GetCurrentDataContext() => _currentDataContext;
    public object? GetStagingDataContext() => _stagingDataContext;

    private TransitionAnimation ResolveAnimation()
    {
        var anim = _animationOverride ?? TransitionAnimation;
        _animationOverride = null;
        return anim;
    }

    private void ResetInitTransforms(ContentView presenter)
    {
        if (presenter == null)
            return;

        presenter.TranslationX = 0;
        presenter.TranslationY = 0;
        presenter.Scale = 1;
        presenter.Opacity = 1;
    }

    private object _pendingContent;
    private readonly object _navigationLock = new object();

    private async Task InternalNavigation(object newContent)
    {
        lock (_navigationLock)
        {
            if (_isNavigating)
            {
                // Queue the latest request to process after current navigation completes
                _pendingContent = newContent;
                return;
            }
            _isNavigating = true;
        }

        await ProcessNavigationQueue(newContent);
    }

    private async Task ProcessNavigationQueue(object initialContent)
    {
        object currentContent = initialContent;

        try
        {
            while (currentContent != null)
            {
                try
                {
                    await PerformTransition(currentContent);
                }
                catch
                {
                    // Transition failed - force clean state so presenters aren't stuck
                    // in an intermediate visual state (e.g., wrong Opacity/Translation)
                    ForceResetPresenters();
                }

                // Allow MAUI's rendering pipeline to commit visual tree changes
                // from CompleteTransition before starting the next transition.
                // Without this, back-to-back transitions starve the layout system.
                await Task.Yield();

                // Check for pending content
                lock (_navigationLock)
                {
                    currentContent = _pendingContent;
                    _pendingContent = null;
                }
            }
        }
        finally
        {
            lock (_navigationLock)
            {
                _isNavigating = false;
            }
        }
    }

    private async Task PerformTransition(object newContent)
    {
        if (newContent is View content)
        {
            _stagingPresenter.Content = content;
        }

        // Ensure staging presenter is visible before any transition
        _stagingPresenter.IsVisible = true;

        // Yield to allow MAUI to measure/arrange the new content.
        // Without this, the staging content may have zero size during animation,
        // or Width/Height used for slide calculations may be incorrect.
        await Task.Yield();

        var animation = ResolveAnimation();
        if (animation != TransitionAnimation.None)
        {
            var duration = TransitionDuration;
            await PrepareAnimation(_currentPresenter, _stagingPresenter, duration, animation);
        }

        // Complete swap
        CompleteTransition();
    }

    private async Task PrepareAnimation(ContentView outgoing, ContentView incoming, TimeSpan duration, TransitionAnimation animation)
    {
        // Cancel any running animations on both presenters
        _currentPresenter.CancelAnimations();
        _stagingPresenter.CancelAnimations();

        // Reset transforms (visibility is already set in PerformTransition)
        ResetInitTransforms (_currentPresenter);
        ResetInitTransforms (_stagingPresenter);

        // Bring incoming on top when appropriate
        _currentPresenter.ZIndex = 0;
        _stagingPresenter.ZIndex = 1;

        var durationMs = (int)duration.TotalMilliseconds;
        var easing = Easing.CubicInOut;

        Task incomingTask = Task.CompletedTask;
        Task outgoingTask = Task.CompletedTask;

        switch (animation)
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
    }

    private void CompleteTransition()
    {
        // Batch outgoing presenter cleanup to prevent intermediate native view states
        _currentPresenter.BatchBegin();
        _currentPresenter.Content = null;
        ResetPresenter(_currentPresenter);
        _currentPresenter.BatchCommit();

        // Swap presenters
        (_currentPresenter, _stagingPresenter) = (_stagingPresenter, _currentPresenter);
        _currentDataContext = _stagingDataContext;
        _stagingDataContext = null;

        // Batch all property changes on the new current presenter so MAUI applies
        // them as a single native update (prevents flicker from intermediate states
        // such as Opacity=0 being briefly visible before being reset to 1)
        _currentPresenter.BatchBegin();
        _currentPresenter.CancelAnimations();
        _currentPresenter.TranslationX = 0;
        _currentPresenter.TranslationY = 0;
        _currentPresenter.Scale = 1;
        _currentPresenter.Opacity = 1;
        _currentPresenter.ZIndex = 1;
        _currentPresenter.IsVisible = true;
        _currentPresenter.BatchCommit();

        // Configure new staging presenter (hidden and ready for next use)
        _stagingPresenter.BatchBegin();
        _stagingPresenter.ZIndex = 0;
        _stagingPresenter.IsVisible = false;
        _stagingPresenter.BatchCommit();
    }

    private void ResetPresenter(ContentView presenter)
    {
        if (presenter == null)
            return;

        // Cancel any running animations
        presenter.CancelAnimations();

        presenter.IsVisible = false;
        presenter.TranslationX = 0;
        presenter.TranslationY = 0;
        presenter.Scale = 1;
        presenter.Opacity = 1;
        presenter.ZIndex = 0;
    }

    private void ForceResetPresenters()
    {
        _currentPresenter.CancelAnimations();
        _stagingPresenter.CancelAnimations();

        // Current presenter: keep its content, restore to visible clean state
        _currentPresenter.BatchBegin();
        _currentPresenter.TranslationX = 0;
        _currentPresenter.TranslationY = 0;
        _currentPresenter.Scale = 1;
        _currentPresenter.Opacity = 1;
        _currentPresenter.ZIndex = 1;
        _currentPresenter.IsVisible = true;
        _currentPresenter.BatchCommit();

        // Staging presenter: clear and hide for next use
        _stagingPresenter.BatchBegin();
        _stagingPresenter.Content = null;
        _stagingPresenter.TranslationX = 0;
        _stagingPresenter.TranslationY = 0;
        _stagingPresenter.Scale = 1;
        _stagingPresenter.Opacity = 1;
        _stagingPresenter.ZIndex = 0;
        _stagingPresenter.IsVisible = false;
        _stagingPresenter.BatchCommit();
    }
}