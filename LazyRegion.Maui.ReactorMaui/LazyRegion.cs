using LazyRegion.Core;
using MauiReactor;

namespace LazyRegion.Maui.ReactorMaui;

public class LazyRegionState
{
    // 현재 어느 Presenter를 보여줄지 (true: Presenter1, false: Presenter2)
    public bool ShowPresenter1 { get; set; } = true;

    // 각 Presenter의 컨텐츠
    public Component Presenter1Content { get; set; }
    public Component Presenter2Content { get; set; }
    public bool IsTransitioning { get; set; }
}

public partial class LazyRegionView : Component<LazyRegionState>, ILazyRegion
{
    [Prop] TransitionAnimation _animation = TransitionAnimation.None;
    [Prop] TimeSpan _transitionDuration = TimeSpan.FromMilliseconds (300);
    [Prop] Component _regionContent;
    public LazyRegionView(string RegionName)
    {
        LazyRegionRegistry.RegisterRegion (RegionName, this);
    }
    private Microsoft.Maui.Controls.Grid _containerRef;

    public void Set(object content, object dataContext = null)
    {
        StartTransition ((Component)content);
    }

    private void StartTransition(Component newContent)
    {
        if (State.IsTransitioning)
            return;

        SetState (s =>
        {
            // 다음에 사용할 Presenter에 새 컨텐츠 설정
            if (s.ShowPresenter1)
                s.Presenter2Content = newContent;
            else
                s.Presenter1Content = newContent;

            s.IsTransitioning = true;
        });

        if(_animation == TransitionAnimation.None)
        {
            CompleteTransition();
            return;
        }
        // 애니메이션 완료 후 상태 전환
        Task.Delay (_transitionDuration).ContinueWith (_ =>
        {
            MainThread.BeginInvokeOnMainThread (CompleteTransition);
        });
    }

    private void CompleteTransition()
    {
        SetState (s =>
        {
            // Presenter 토글
            s.ShowPresenter1 = !s.ShowPresenter1;
            s.IsTransitioning = false;

            // 이전 Presenter의 컨텐츠 클리어 (메모리 절약)
            if (s.ShowPresenter1)
                s.Presenter2Content = null;
            else
                s.Presenter1Content = null;
        });
    }

    private double GetContainerWidth() => _containerRef?.Width ?? 0;
    private double GetContainerHeight() => _containerRef?.Height ?? 0;

    private VisualNode RenderPresenter(object content, bool isActive)
    {
        if (content == null || content is not Component visualContent)
            return null;

        var container = ContentView (visualContent);

        if (!State.IsTransitioning || _animation == TransitionAnimation.None)
            return container;

        var duration = (uint)_transitionDuration.TotalMilliseconds;
        var easing = Easing.CubicInOut;
        var width = GetContainerWidth ();
        var height = GetContainerHeight ();
        // 들어오는 Presenter (inactive → active)
        if (!isActive)
        {
            switch (_animation)
            {
                case TransitionAnimation.Fade:
                    return container
                        .Opacity (0)
                        .Opacity (1)
                        .WithAnimation (duration: duration, easing: easing);

                case TransitionAnimation.NewFromLeft:
                    return container
                        .TranslationX (-width)
                        .TranslationX (0)
                        .WithAnimation (duration: duration, easing: easing);

                case TransitionAnimation.NewFromRight:
                    return container
                        .TranslationX (width)
                        .TranslationX (0)
                        .WithAnimation (duration: duration, easing: easing);

                case TransitionAnimation.NewFromUp:
                    return container
                        .TranslationY (-height)
                        .TranslationY (0)
                        .WithAnimation (duration: duration, easing: easing);

                case TransitionAnimation.NewFromDown:
                    return container
                        .TranslationY (height)
                        .TranslationY (0)
                        .WithAnimation (duration: duration, easing: easing);

                //case TransitionAnimation.SlideLeft:
                //    return container
                //        .TranslationX (isReversed ? -width : width)
                //        .TranslationX (0)
                //        .WithAnimation (duration: duration, easing: easing);

                //case TransitionAnimation.SlideRight:
                //    return container
                //        .TranslationX (isReversed ? width : -width)
                //        .TranslationX (0)
                //        .WithAnimation (duration: duration, easing: easing);

                case TransitionAnimation.SlideUp:
                    return container
                        .TranslationY (height)
                        .TranslationY (0)
                        .WithAnimation (duration: duration, easing: easing);

                case TransitionAnimation.SlideDown:
                    return container
                        .TranslationY (-height)
                        .TranslationY (0)
                        .WithAnimation (duration: duration, easing: easing);

                case TransitionAnimation.ZoomIn:
                    return container
                        .Scale (0.5)
                        .Opacity (0)
                        .Scale (1)
                        .Opacity (1)
                        .WithAnimation (duration: duration, easing: easing);

                case TransitionAnimation.Scale:
                    return container
                        .Scale (0)
                        .Scale (1)
                        .WithAnimation (duration: duration, easing: easing);

                default:
                    return container;
            }
        }
        // 나가는 Presenter (active → inactive)
        else
        {
            switch (_animation)
            {
                case TransitionAnimation.Fade:
                    return container
                        .Opacity (0)
                        .WithAnimation (duration: duration, easing: easing);

                //case TransitionAnimation.SlideLeft:
                //    return container
                //        .TranslationX (isReversed ? width : -width)
                //        .WithAnimation (duration: duration, easing: easing);

                //case TransitionAnimation.SlideRight:
                //    return container
                //        .TranslationX (isReversed ? -width : width)
                //        .WithAnimation (duration: duration, easing: easing);

                case TransitionAnimation.SlideUp:
                    return container
                        .TranslationY (-height)
                        .WithAnimation (duration: duration, easing: easing);

                case TransitionAnimation.SlideDown:
                    return container
                        .TranslationY (height)
                        .WithAnimation (duration: duration, easing: easing);

                case TransitionAnimation.Scale:
                    return container
                        .Scale (0)
                        .WithAnimation (duration: duration, easing: easing);

                case TransitionAnimation.ZoomOut:
                    return container
                        .Scale (0.8)
                        .Opacity (0)
                        .WithAnimation (duration: duration, easing: easing);

                default:
                    // NewFrom 애니메이션은 나가는 애니메이션 없음
                    return container;
            }
        }
    }

    public override VisualNode Render()
        => Grid (r => _containerRef = r,
            // Presenter 1
            ContentView (
                RenderPresenter (State.Presenter1Content, State.ShowPresenter1))
                .ZIndex (!State.ShowPresenter1 ? 1 : 0),

            // Presenter 2
            ContentView (
                RenderPresenter (State.Presenter2Content, !State.ShowPresenter1))
                .ZIndex (State.ShowPresenter1 ? 1 : 0)
        )
        .OnSizeChanged (OnSizeChanged);

    private void OnSizeChanged(object sender, EventArgs e)
    {
        if (sender is Microsoft.Maui.Controls.Grid grid && grid.Width > 0 && grid.Height > 0)
        {
            grid.Clip = new Microsoft.Maui.Controls.Shapes.RectangleGeometry (new Rect (0, 0, grid.Width, grid.Height));
        }
    }
}