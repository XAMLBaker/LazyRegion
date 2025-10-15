using LazyRegion.Core;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace LazyRegion.WPF;
public class LazyRegion : ContentControl, ILazyRegion
{
    public static readonly DependencyProperty RegionNameProperty =
        DependencyProperty.Register (nameof (RegionName), typeof (string), typeof (LazyRegion),
            new PropertyMetadata (null, OnRegionNameChanged));

    public static readonly DependencyProperty TransitionAnimationProperty =
        DependencyProperty.Register (nameof (TransitionAnimation), typeof (TransitionAnimation), typeof (LazyRegion),
            new PropertyMetadata (TransitionAnimation.Fade));

    public static readonly DependencyProperty TransitionDurationProperty =
            DependencyProperty.Register (nameof (TransitionDuration), typeof (Duration), typeof (LazyRegion),
                new PropertyMetadata (new Duration (TimeSpan.FromMilliseconds (300))));

    public Duration TransitionDuration
    {
        get => (Duration)GetValue (TransitionDurationProperty);
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

    private bool _isNavigating;

    // Staging 방식: 현재 표시되는 presenter와 준비 중인 presenter
    private ContentControl _currentPresenter;
    private ContentControl _stagingPresenter;

    static LazyRegion()
    {
        DefaultStyleKeyProperty.OverrideMetadata (typeof (LazyRegion), new FrameworkPropertyMetadata (typeof (LazyRegion)));
    }

    public LazyRegion()
    {
    }

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate ();

        var presenter1 = GetTemplateChild ("PART_Content1") as ContentControl;
        var presenter2 = GetTemplateChild ("PART_Content2") as ContentControl;

        // 초기 상태 설정
        if (presenter1 != null && presenter2 != null)
        {
            // 처음에는 presenter1을 current로, presenter2를 staging으로 설정
            _currentPresenter = presenter1;
            _stagingPresenter = presenter2;

            // 현재 표시될 내용을 current에 설정
            _currentPresenter.Content = Content;
            _currentPresenter.Visibility = Visibility.Visible;
            _currentPresenter.Opacity = 1;

            // staging은 숨김
            _stagingPresenter.Visibility = Visibility.Collapsed;
            _stagingPresenter.Opacity = 1;
        }
    }

    protected override async void OnContentChanged(object oldContent, object newContent)
    {
        base.OnContentChanged (oldContent, newContent);

        if (_currentPresenter == null || _stagingPresenter == null)
            return;

        if (_isNavigating)
            return;

        await this.Navigation (newContent);
    }

    private static void OnRegionNameChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is LazyRegion region && !string.IsNullOrEmpty (region.RegionName))
        {
            LazyRegionRegistry.RegisterRegion (region.RegionName, region);
        }
    }

    private static void ResetInitTransforms(ContentControl presenter)
    {
        if (presenter == null)
            return;
        if(presenter.RenderTransform is TranslateTransform translate)
        {
            translate.BeginAnimation (TranslateTransform.XProperty, null);
            translate.BeginAnimation (TranslateTransform.YProperty, null);
            translate.X = 0;
            translate.Y = 0;
        }
        if (presenter.RenderTransform is ScaleTransform scale)
        {
            scale.BeginAnimation (ScaleTransform.ScaleXProperty, null);
            scale.BeginAnimation (ScaleTransform.ScaleYProperty, null);
            scale.ScaleX = 1.0;
            scale.ScaleY = 1.0;
        }

        presenter.BeginAnimation (UIElement.OpacityProperty, null);
        presenter.Opacity = 1.0;
    }

    private TaskCompletionSource<bool> _animationCompletionSource;

    private async Task Navigation(object newContent)
    {
        _isNavigating = true;

        if (newContent is FrameworkElement contentControl)
        {
            _stagingPresenter.Content = contentControl;
        }
        else
        {
            // ViewModel이나 다른 객체인 경우
            _stagingPresenter.Content = newContent;
        }

        // 2. 애니메이션 준비
        var duration = TransitionDuration.TimeSpan;
        var sb = new Storyboard ();
        _animationCompletionSource = new TaskCompletionSource<bool> ();

        sb.Completed += (_, __) =>
        {
            _animationCompletionSource.SetResult (true);
        };
        // 3. 애니메이션 타입에 따른 처리
        PrepareAnimation (sb, _currentPresenter, _stagingPresenter, duration);

        // 4. 애니메이션 실행
        sb.Begin ();
        await _animationCompletionSource.Task;

        // 5. 애니메이션 완료 후 정리
        CompleteTransition ();
}

    private void PrepareAnimation(Storyboard sb, ContentControl outgoing, ContentControl incoming, TimeSpan duration)
    {
        incoming.Visibility = Visibility.Visible;
        ResetInitTransforms (_currentPresenter);
        ResetInitTransforms (_stagingPresenter);

        // Z-Index 초기화
        Panel.SetZIndex (outgoing, 0);
        Panel.SetZIndex (incoming, 1);

        switch (TransitionAnimation)
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
                Panel.SetZIndex (outgoing, 1);
                Panel.SetZIndex (incoming, 0);
                AddZoomAnimation (sb, outgoing, 1, 0.8, duration);
                AddFadeAnimation (sb, outgoing, 1, 0, duration);
                break;

            case TransitionAnimation.Scale:
                AddZoomAnimation (sb, outgoing, 1, 0, duration);
                AddZoomAnimation (sb, incoming, 0, 1, duration);
                break;

            default:
                // 애니메이션 없음 - 즉시 전환
                break;
        }
    }

    private void CompleteTransition()
    {
        _currentPresenter.Content = null;
        _currentPresenter.Visibility = Visibility.Hidden;
        (_currentPresenter, _stagingPresenter) = (_stagingPresenter, _currentPresenter);
        Panel.SetZIndex (_currentPresenter, 1);
        Panel.SetZIndex (_stagingPresenter, 0);

        _currentPresenter.Visibility = Visibility.Visible;
        _stagingPresenter.Visibility = Visibility.Hidden;

        _isNavigating = false;
    }

    private void AddFadeAnimation(Storyboard sb, ContentControl target, double from, double to, TimeSpan duration)
    {
        var anim = new DoubleAnimation (from, to, new Duration (duration));
        Storyboard.SetTarget (anim, target);
        Storyboard.SetTargetProperty (anim, new PropertyPath (ContentControl.OpacityProperty));
        sb.Children.Add (anim);
    }

    private void AddSlideXAnimation(Storyboard sb, ContentControl target, double fromX, double toX, TimeSpan duration)
    {
        target.RenderTransformOrigin = new Point (0.5, 0.5);
        target.RenderTransform = new TranslateTransform (0, 0);

        var anim = new DoubleAnimation (fromX, toX, new Duration (duration));
        Storyboard.SetTarget (anim, target);
        Storyboard.SetTargetProperty (anim, new PropertyPath ("RenderTransform.(TranslateTransform.X)"));
        sb.Children.Add (anim);
    }

    private void AddSlideYAnimation(Storyboard sb, ContentControl target, double fromY, double toY, TimeSpan duration)
    {
        target.RenderTransformOrigin = new Point (0.5, 0.5);
        target.RenderTransform = new TranslateTransform (0, 0);

        var anim = new DoubleAnimation (fromY, toY, new Duration (duration));
        Storyboard.SetTarget (anim, target);
        Storyboard.SetTargetProperty (anim, new PropertyPath ("RenderTransform.(TranslateTransform.Y)"));
        sb.Children.Add (anim);
    }

    private void AddZoomAnimation(Storyboard sb, ContentControl target, double fromScale, double toScale, TimeSpan duration)
    {
        target.RenderTransformOrigin = new Point (0.5, 0.5);
        target.RenderTransform = new ScaleTransform (fromScale, fromScale);

        var animX = new DoubleAnimation (fromScale, toScale, new Duration (duration));
        var animY = new DoubleAnimation (fromScale, toScale, new Duration (duration));

        Storyboard.SetTarget (animX, target);
        Storyboard.SetTargetProperty (animX, new PropertyPath ("RenderTransform.ScaleX"));

        Storyboard.SetTarget (animY, target);
        Storyboard.SetTargetProperty (animY, new PropertyPath ("RenderTransform.ScaleY"));

        sb.Children.Add (animX);
        sb.Children.Add (animY);
    }

    public void Set(object content, object dataContext = null)
    {
        this.Content = content;
        if (dataContext == null)
            return;
        this._currentPresenter.DataContext = dataContext;
    }
}