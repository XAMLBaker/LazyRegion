using LazyRegion.Core;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using System;
using System.Threading.Tasks;

namespace LazyRegion.WinUI3;

/// <summary>
/// (KO) LazyRegion의 새로운 이름입니다. 무대(Stage) 기반의 세련된 전환을 제공합니다.
/// (EN) The new identity of LazyRegion. Provides sophisticated stage-based transitions.
/// </summary>
[Obsolete ("LazyRegion is now LazyStage. Please migrate to LazyStage for the new world.")]
public class LazyRegion : LazyStage
{
}

public class LazyStage : Control, ILazyRegion
{
    public static readonly DependencyProperty RegionNameProperty =
        DependencyProperty.Register (nameof (RegionName), typeof (string), typeof (LazyStage),
            new PropertyMetadata (null, OnRegionNameChanged));

    public static readonly DependencyProperty TransitionAnimationProperty =
        DependencyProperty.Register (nameof (TransitionAnimation), typeof (TransitionAnimation), typeof (LazyStage),
            new PropertyMetadata (TransitionAnimation.Fade));

    public static readonly DependencyProperty TransitionDurationProperty =
            DependencyProperty.Register (nameof (TransitionDuration), typeof (TimeSpan), typeof (LazyStage),
                new PropertyMetadata (TimeSpan.FromMilliseconds (300)));

    /// <summary>
    /// ⭐️ ContentControl.Content 대신 별도의 DependencyProperty 사용.
    /// Control을 상속하므로 Content를 set해도 비주얼 트리에 자동 배치되지 않습니다.
    /// </summary>
    public static readonly DependencyProperty StageContentProperty =
        DependencyProperty.Register (nameof (StageContent), typeof (object), typeof (LazyStage),
            new PropertyMetadata (null, OnStageContentChanged));

    public TimeSpan TransitionDuration
    {
        get => (TimeSpan)GetValue (TransitionDurationProperty);
        set => SetValue (TransitionDurationProperty, value);
    }

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

    public object StageContent
    {
        get => GetValue (StageContentProperty);
        set => SetValue (StageContentProperty, value);
    }

    private bool _isNavigating;
    private bool _isTemplateApplied;
    private TransitionAnimation? _animationOverride;
    private object? _currentDataContext;
    private object? _stagingDataContext;

    // Staging 방식: 현재 표시되는 presenter와 준비 중인 presenter
    private ContentControl _currentPresenter;
    private ContentControl _stagingPresenter;

    public LazyStage()
    {
        this.DefaultStyleKey = typeof (LazyStage);
    }

    protected override void OnApplyTemplate()
    {
        base.OnApplyTemplate ();

        var presenter1 = GetTemplateChild ("PART_Content1") as ContentControl;
        var presenter2 = GetTemplateChild ("PART_Content2") as ContentControl;

        if (presenter1 != null && presenter2 != null)
        {
            _currentPresenter = presenter1;
            _stagingPresenter = presenter2;
            _isTemplateApplied = true;

            // 초기 콘텐츠가 이미 설정되어 있으면 presenter에 배치
            if (StageContent != null)
            {
                _currentPresenter.Content = StageContent;
            }

            _currentPresenter.Visibility = Visibility.Visible;
            _currentPresenter.Opacity = 1;

            _stagingPresenter.Visibility = Visibility.Collapsed;
            _stagingPresenter.Opacity = 1;
        }
    }

    private static void OnStageContentChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is LazyStage stage)
        {
            stage.HandleContentChanged (e.NewValue);
        }
    }

    private void HandleContentChanged(object newContent)
    {
        if (!_isTemplateApplied || _currentPresenter == null || _stagingPresenter == null)
            return;

        try
        {
            if (!DispatcherQueue.HasThreadAccess)
            {
                DispatcherQueue.TryEnqueue (DispatcherQueuePriority.Normal, () =>
                {
                    SafeNavigate (newContent);
                });
                return;
            }

            SafeNavigate (newContent);
        }
        catch (Exception)
        {
            // TODO: Add logging
        }
    }

    private static void OnRegionNameChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is LazyStage region && !string.IsNullOrEmpty (region.RegionName))
        {
            LazyRegionRegistry.RegisterRegion (region.RegionName, region);
        }
    }

    private static void ResetInitTransforms(ContentControl presenter)
    {
        if (presenter == null)
            return;

        if (presenter.RenderTransform is TranslateTransform translate)
        {
            translate.X = 0;
            translate.Y = 0;
        }
        if (presenter.RenderTransform is ScaleTransform scale)
        {
            scale.ScaleX = 1.0;
            scale.ScaleY = 1.0;
        }

        presenter.Opacity = 1.0;
    }

    private void SafeNavigate(object newContent)
    {
        var task = InternalNavigation (newContent);
        task.ContinueWith (t =>
        {
            var _ = t.Exception;
        }, TaskContinuationOptions.OnlyOnFaulted);
    }

    private async Task InternalNavigation(object newContent)
    {
        if (_isNavigating)
            return;

        try
        {
            await Navigation (newContent);
        }
        catch (Exception)
        {
            _isNavigating = false;
            // TODO: Add logging
        }
    }

    private async Task Navigation(object newContent)
    {
        _isNavigating = true;

        // 어느 presenter든 동일 인스턴스를 가지고 있으면 먼저 제거
        if (_currentPresenter.Content == newContent)
            _currentPresenter.Content = null;

        if (_stagingPresenter.Content == newContent)
            _stagingPresenter.Content = null;

        _stagingPresenter.Content = newContent;

        var animation = ResolveAnimation ();
        if (animation != TransitionAnimation.None)
        {
            var duration = TransitionDuration;
            var sb = new Storyboard ();
            var tcs = new TaskCompletionSource<bool> ();

            sb.Completed += (_, _) =>
            {
                if (!tcs.Task.IsCompleted)
                {
                    tcs.SetResult (true);
                }
            };
            PrepareAnimation (sb, _currentPresenter, _stagingPresenter, duration, animation);

            sb.Begin ();
            await tcs.Task;
        }
        CompleteTransition ();
    }

    private void PrepareAnimation(Storyboard sb, ContentControl outgoing, ContentControl incoming, TimeSpan duration, TransitionAnimation animation)
    {
        incoming.Visibility = Visibility.Visible;
        ResetInitTransforms (_currentPresenter);
        ResetInitTransforms (_stagingPresenter);

        Canvas.SetZIndex (outgoing, 0);
        Canvas.SetZIndex (incoming, 1);

        switch (animation)
        {
            case TransitionAnimation.Fade:
                AddFadeAnimation (sb, outgoing, 1, 0, duration);
                AddFadeAnimation (sb, incoming, 0, 1, duration);
                break;

            case TransitionAnimation.NewFromLeft:
                AddSlideXAnimation (sb, incoming, ActualWidth, 0, duration);
                break;

            case TransitionAnimation.NewFromRight:
                AddSlideXAnimation (sb, incoming, -ActualWidth, 0, duration);
                break;

            case TransitionAnimation.NewFromUp:
                AddSlideYAnimation (sb, incoming, ActualHeight, 0, duration);
                break;

            case TransitionAnimation.NewFromDown:
                AddSlideYAnimation (sb, incoming, -ActualHeight, 0, duration);
                break;

            case TransitionAnimation.SlideLeft:
                AddSlideXAnimation (sb, outgoing, 0, -ActualWidth, duration);
                AddSlideXAnimation (sb, incoming, ActualWidth, 0, duration);
                break;

            case TransitionAnimation.SlideRight:
                AddSlideXAnimation (sb, outgoing, 0, ActualWidth, duration);
                AddSlideXAnimation (sb, incoming, -ActualWidth, 0, duration);
                break;

            case TransitionAnimation.SlideDown:
                AddSlideYAnimation (sb, outgoing, 0, ActualHeight, duration);
                AddSlideYAnimation (sb, incoming, -ActualHeight, 0, duration);
                break;

            case TransitionAnimation.SlideUp:
                AddSlideYAnimation (sb, outgoing, 0, -ActualHeight, duration);
                AddSlideYAnimation (sb, incoming, ActualHeight, 0, duration);
                break;

            case TransitionAnimation.ZoomIn:
                AddZoomAnimation (sb, incoming, 0, 1, duration);
                AddFadeAnimation (sb, incoming, 0, 1, duration);
                break;

            case TransitionAnimation.ZoomOut:
                Canvas.SetZIndex (outgoing, 1);
                Canvas.SetZIndex (incoming, 0);
                AddZoomAnimation (sb, outgoing, 1, 0.8, duration);
                AddFadeAnimation (sb, outgoing, 1, 0, duration);
                break;

            case TransitionAnimation.Scale:
                AddZoomAnimation (sb, outgoing, 1, 0, duration);
                AddZoomAnimation (sb, incoming, 0, 1, duration);
                break;

            default:
                break;
        }
    }

    private void CompleteTransition()
    {
        _currentPresenter.Content = null;
        _currentPresenter.Visibility = Visibility.Collapsed;
        (_currentPresenter, _stagingPresenter) = (_stagingPresenter, _currentPresenter);
        Canvas.SetZIndex (_currentPresenter, 1);
        Canvas.SetZIndex (_stagingPresenter, 0);

        _currentPresenter.Visibility = Visibility.Visible;
        _stagingPresenter.Visibility = Visibility.Collapsed;

        _currentDataContext = _stagingDataContext;
        _stagingDataContext = null;

        _isNavigating = false;
    }

    private void AddFadeAnimation(Storyboard sb, ContentControl target, double from, double to, TimeSpan duration)
    {
        var anim = new DoubleAnimation ();
        anim.From = from;
        anim.To = to;
        anim.Duration = new Duration (duration);
        Storyboard.SetTarget (anim, target);
        Storyboard.SetTargetProperty (anim, "Opacity");
        sb.Children.Add (anim);
    }

    private void AddSlideXAnimation(Storyboard sb, ContentControl target, double fromX, double toX, TimeSpan duration)
    {
        target.RenderTransformOrigin = new Windows.Foundation.Point (0.5, 0.5);
        target.RenderTransform = new TranslateTransform ();

        var anim = new DoubleAnimation ();
        anim.From = fromX;
        anim.To = toX;
        anim.Duration = new Duration (duration);
        Storyboard.SetTarget (anim, target);
        Storyboard.SetTargetProperty (anim, "(UIElement.RenderTransform).(TranslateTransform.X)");
        sb.Children.Add (anim);
    }

    private void AddSlideYAnimation(Storyboard sb, ContentControl target, double fromY, double toY, TimeSpan duration)
    {
        target.RenderTransformOrigin = new Windows.Foundation.Point (0.5, 0.5);
        target.RenderTransform = new TranslateTransform ();

        var anim = new DoubleAnimation ();
        anim.From = fromY;
        anim.To = toY;
        anim.Duration = new Duration (duration);
        Storyboard.SetTarget (anim, target);
        Storyboard.SetTargetProperty (anim, "(UIElement.RenderTransform).(TranslateTransform.Y)");
        sb.Children.Add (anim);
    }

    private void AddZoomAnimation(Storyboard sb, ContentControl target, double fromScale, double toScale, TimeSpan duration)
    {
        target.RenderTransformOrigin = new Windows.Foundation.Point (0.5, 0.5);
        target.RenderTransform = new ScaleTransform ();

        var animX = new DoubleAnimation ();
        animX.From = fromScale;
        animX.To = toScale;
        animX.Duration = new Duration (duration);

        var animY = new DoubleAnimation ();
        animY.From = fromScale;
        animY.To = toScale;
        animY.Duration = new Duration (duration);

        Storyboard.SetTarget (animX, target);
        Storyboard.SetTargetProperty (animX, "(UIElement.RenderTransform).(ScaleTransform.ScaleX)");

        Storyboard.SetTarget (animY, target);
        Storyboard.SetTargetProperty (animY, "(UIElement.RenderTransform).(ScaleTransform.ScaleY)");

        sb.Children.Add (animX);
        sb.Children.Add (animY);
    }

    public TransitionAnimation CurrentAnimation => TransitionAnimation;

    public void Set(object content, object dataContext = null)
    {
        if (dataContext != null && content is FrameworkElement element)
        {
            element.DataContext = dataContext;
        }
        _stagingDataContext = content is FrameworkElement fe ? fe.DataContext : dataContext;
        this.StageContent = content;
    }

    public object? GetCurrentDataContext() => _currentDataContext;
    public object? GetStagingDataContext() => _stagingDataContext;

    public void Set(object content, object dataContext, TransitionAnimation? animationOverride)
    {
        _animationOverride = animationOverride;
        Set (content, dataContext);
    }

    private TransitionAnimation ResolveAnimation()
    {
        var anim = _animationOverride ?? TransitionAnimation;
        _animationOverride = null;
        return anim;
    }
}