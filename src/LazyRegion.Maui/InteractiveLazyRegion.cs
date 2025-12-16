using LazyRegion.Core;
using Microsoft.Maui.Controls.Shapes;
using System.Windows.Input;

namespace LazyRegion.Maui;

public class InteractiveLazyRegion : ContentView, ILazyRegion
{
    public static readonly BindableProperty RegionNameProperty =
        BindableProperty.Create (nameof (RegionName), typeof (string), typeof (InteractiveLazyRegion), default (string), propertyChanged: OnRegionNameChanged);

    public static readonly BindableProperty TransitionAnimationProperty =
        BindableProperty.Create (nameof (TransitionAnimation), typeof (TransitionAnimation), typeof (InteractiveLazyRegion), TransitionAnimation.Fade);

    public static readonly BindableProperty TransitionDurationProperty =
        BindableProperty.Create (nameof (TransitionDuration), typeof (TimeSpan), typeof (InteractiveLazyRegion), TimeSpan.FromMilliseconds (300));

    public static readonly BindableProperty RegionContentProperty =
        BindableProperty.Create (nameof (RegionContent), typeof (object), typeof (InteractiveLazyRegion), default (object), propertyChanged: OnRegionContentChanged);

    // 드래그 관련 속성
    public static readonly BindableProperty EnableDragNavigationProperty =
        BindableProperty.Create (nameof (EnableDragNavigation), typeof (bool), typeof (InteractiveLazyRegion), false);

    public static readonly BindableProperty DragThresholdProperty =
        BindableProperty.Create (nameof (DragThreshold), typeof (double), typeof (InteractiveLazyRegion), 0.3);

    public static readonly BindableProperty DragDirectionProperty =
        BindableProperty.Create (nameof (DragDirection), typeof (DragDirection), typeof (InteractiveLazyRegion), DragDirection.Horizontal);

    // Command 속성
    public static readonly BindableProperty DragStartCommandProperty =
        BindableProperty.Create (nameof (DragStartCommand), typeof (ICommand), typeof (InteractiveLazyRegion));

    public static readonly BindableProperty DragUpdateCommandProperty =
        BindableProperty.Create (nameof (DragUpdateCommand), typeof (ICommand), typeof (InteractiveLazyRegion));

    public static readonly BindableProperty DragCompletedCommandProperty =
        BindableProperty.Create (nameof (DragCompletedCommand), typeof (ICommand), typeof (InteractiveLazyRegion));

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

    public object RegionContent
    {
        get => GetValue (RegionContentProperty);
        set => SetValue (RegionContentProperty, value);
    }

    public bool EnableDragNavigation
    {
        get => (bool)GetValue (EnableDragNavigationProperty);
        set => SetValue (EnableDragNavigationProperty, value);
    }

    public double DragThreshold
    {
        get => (double)GetValue (DragThresholdProperty);
        set => SetValue (DragThresholdProperty, value);
    }

    public DragDirection DragDirection
    {
        get => (DragDirection)GetValue (DragDirectionProperty);
        set => SetValue (DragDirectionProperty, value);
    }

    public ICommand DragStartCommand
    {
        get => (ICommand)GetValue (DragStartCommandProperty);
        set => SetValue (DragStartCommandProperty, value);
    }

    public ICommand DragUpdateCommand
    {
        get => (ICommand)GetValue (DragUpdateCommandProperty);
        set => SetValue (DragUpdateCommandProperty, value);
    }

    public ICommand DragCompletedCommand
    {
        get => (ICommand)GetValue (DragCompletedCommandProperty);
        set => SetValue (DragCompletedCommandProperty, value);
    }

    private ContentView _currentPresenter;
    private ContentView _stagingPresenter;
    private bool _isNavigating;
    private bool _isDragging;
    private bool _dragDirectionDetermined;
    private bool _isAnimating; // 애니메이션 진행 중 플래그
    private object _pendingContent;
    private PanGestureRecognizer _panGesture;
    private double _lastTotalX;
    private double _lastTotalY;
    private CancellationTokenSource _animationCts;

    public InteractiveLazyRegion()
    {
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

        // PanGesture 설정
        _panGesture = new PanGestureRecognizer ();
        _panGesture.PanUpdated += OnPanUpdated;
        GestureRecognizers.Add (_panGesture);

        this.SizeChanged += (s, e) =>
        {
            if (s is ContentView g && g.Width > 0 && g.Height > 0)
            {
                this.Clip = new RectangleGeometry (new Rect (0, 0, g.Width, g.Height));
            }
        };
    }
    private void OnPanUpdated(object sender, PanUpdatedEventArgs e)
    {
        switch (e.StatusType)
        {
            case GestureStatus.Started:
                // 이전 애니메이션 강제 중단
                if (_isAnimating)
                {
                    if (_animationCts != null)
                    {
                        _animationCts?.Cancel ();
                        _animationCts?.Dispose ();
                        _animationCts = null;
                    }
                    ForceResetState ();
                }

                DragStartCommand?.Execute (null);
                
                StartDrag ();
                break;

            case GestureStatus.Running:
                if (!_isDragging)
                    return;
                _lastTotalX = e.TotalX;
                _lastTotalY = e.TotalY;
                UpdateDrag (e.TotalX, e.TotalY);
                DragUpdateCommand?.Execute (new { e.TotalX, e.TotalY });
                break;

            case GestureStatus.Completed:
            case GestureStatus.Canceled:
                EndDrag (_lastTotalX, _lastTotalY);
                break;
        }
    }

    private void StartDrag()
    {
        if (_pendingContent == null)
            return;

        _isDragging = true;
        _dragDirectionDetermined = false;

        // 스테이징 프레젠터 준비
        if (_pendingContent is View content)
        {
            _stagingPresenter.Content = content;
        }

        ResetInitTransforms (_stagingPresenter);
        _stagingPresenter.IsVisible = true;
        _stagingPresenter.ZIndex = 1;
        _currentPresenter.ZIndex = 0;
        _stagingPresenter.Opacity = 0;
    }

    private void UpdateDrag(double totalX, double totalY)
    {
        if (!_isDragging)
            return;

        // 첫 Running 이벤트에서 드래그 방향 결정
        if (!_dragDirectionDetermined)
        {
            if (DragDirection == DragDirection.Horizontal)
            {
                // totalX가 음수면 왼쪽 드래그 → 다음 화면은 오른쪽에서
                // totalX가 양수면 오른쪽 드래그 → 다음 화면은 왼쪽에서
                _stagingPresenter.TranslationX = totalX < 0 ? Width : -Width;
            }
            else
            {
                _stagingPresenter.TranslationY = totalY < 0 ? Height : -Height;
            }
            _dragDirectionDetermined = true;
        }

        double progress = 0;

        if (DragDirection == DragDirection.Horizontal)
        {
            progress = Math.Abs (totalX) / Width;
            _stagingPresenter.TranslationX = (totalX < 0 ? Width : -Width) + totalX;
            _currentPresenter.TranslationX = totalX;
        }
        else
        {
            progress = Math.Abs (totalY) / Height;
            _stagingPresenter.TranslationY = (totalY < 0 ? Height : -Height) + totalY;
            _currentPresenter.TranslationY = totalY;
        }

        progress = Math.Clamp (progress, 0, 1);
        _currentPresenter.Opacity = Math.Max (0, 1 - progress);
        _stagingPresenter.Opacity = Math.Min (1, progress);
    }

    private async void EndDrag(double totalX, double totalY)
    {
        if (!_isDragging)
            return;

        _isDragging = false;

        double progress = DragDirection == DragDirection.Horizontal
            ? Math.Abs (totalX) / Width
            : Math.Abs (totalY) / Height;

        progress = Math.Clamp (progress, 0, 1);
        bool shouldComplete = progress >= DragThreshold;

        var durationMs = (uint)(TransitionDuration.TotalMilliseconds * (1 - progress));

        DragCompletedCommand?.Execute (new { shouldComplete, progress, totalX, totalY });


        _isAnimating = true;

        await CompleteDragTransition (durationMs);
    }

    private async Task CompleteDragTransition(uint durationMs)
    {
        _animationCts = new CancellationTokenSource ();
        var token = _animationCts.Token;
        var easing = Easing.CubicOut;

        Task incomingTask = _stagingPresenter.TranslateTo (0, 0, durationMs, easing);
        Task outgoingTask;

        if (DragDirection == DragDirection.Horizontal)
        {
            double targetX = _lastTotalX < 0 ? -Width : Width;
            outgoingTask = _currentPresenter.TranslateTo (targetX, 0, durationMs, easing);
        }
        else
        {
            double targetY = _lastTotalY < 0 ? -Height : Height;
            outgoingTask = _currentPresenter.TranslateTo (0, targetY, durationMs, easing);
        }

        var fadeIn = _stagingPresenter.FadeTo (1, durationMs, easing);
        var fadeOut = _currentPresenter.FadeTo (0, durationMs, easing);

        // 애니메이션 중간에 취소 시도
        while (!token.IsCancellationRequested &&
                (!incomingTask.IsCompleted || !outgoingTask.IsCompleted || !fadeIn.IsCompleted || !fadeOut.IsCompleted))
        {
            await Task.Delay (16); // 프레임 대기 (약 60fps)
        }

        if (token.IsCancellationRequested)
        {
            //_currentPresenter.AbortAnimation ("TranslationTo");
            //_stagingPresenter.AbortAnimation ("TranslationTo");
            //_currentPresenter.AbortAnimation ("FadeTo");
            //_stagingPresenter.AbortAnimation ("FadeTo");

            return;
        }

        CompleteTransition ();
        if (_animationCts != null && _animationCts.Token.IsCancellationRequested == false)
        {
            _pendingContent = null;
            _isAnimating = false;
            if (_animationCts != null)
            {
                _animationCts?.Dispose ();
                _animationCts = null;
            }
        }
    }

    private static void OnRegionNameChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is InteractiveLazyRegion region && !string.IsNullOrEmpty (region.RegionName))
        {
            LazyRegionRegistry.RegisterRegion (region.RegionName, region);
        }
    }

    private static void OnRegionContentChanged(BindableObject bindable, object oldValue, object newContent)
    {
        if (bindable is InteractiveLazyRegion region)
        {
            region.HandleRegionContentChanged (oldValue, newContent);
        }
    }

    private void HandleRegionContentChanged(object oldContent, object newContent)
    {
        if (EnableDragNavigation)
        {
            if (_isDragging == false|| _isAnimating == false)
                _pendingContent = newContent;
        }
        else
        {
            // 기존 애니메이션 모드: 즉시 전환
            MainThread.BeginInvokeOnMainThread (() => {
                _ = NavigationInternal (newContent);
            });
        }
    }

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

        //presenter.AbortAnimation ("TranslationTo");
        //presenter.AbortAnimation ("FadeTo");
        //presenter.CancelAnimations (); // 모든 애니메이션 취소
        presenter.TranslationX = 0;
        presenter.TranslationY = 0;
        presenter.Scale = 1;
        presenter.Opacity = 1;
    }

    private void ForceResetState()
    {
        _isAnimating = false;
        _isDragging = false;
        _dragDirectionDetermined = false;

        // 모든 애니메이션 즉시 중단
        _currentPresenter?.CancelAnimations ();
        _stagingPresenter?.CancelAnimations ();

        // 스테이징 숨기고 리셋
        if (_stagingPresenter != null)
        {
            _stagingPresenter.IsVisible = false;
            _stagingPresenter.Content = null;
            ResetInitTransforms (_stagingPresenter);
        }

        // 현재 프레젠터 정상 위치로
        if (_currentPresenter != null)
        {
            ResetInitTransforms (_currentPresenter);
            _currentPresenter.IsVisible = true;
        }
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

        ResetInitTransforms (_currentPresenter);
        ResetInitTransforms (_stagingPresenter);

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
                break;
        }

        try
        {
            await Task.WhenAll (incomingTask, outgoingTask);
        }
        catch
        {
            // ignore
        }

        CompleteTransition ();
    }

    private void CompleteTransition()
    {
        _currentPresenter.Content = null;
        _currentPresenter.IsVisible = false;

        (_currentPresenter, _stagingPresenter) = (_stagingPresenter, _currentPresenter);

        _currentPresenter.ZIndex = 1;
        _stagingPresenter.ZIndex = 0;
        _currentPresenter.IsVisible = true;
        _stagingPresenter.IsVisible = false;

        ResetInitTransforms (_currentPresenter);
        ResetInitTransforms (_stagingPresenter);

        _isNavigating = false;
    }
}

public enum DragDirection
{
    Horizontal,
    Vertical
}