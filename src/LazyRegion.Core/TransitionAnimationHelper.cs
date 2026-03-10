namespace LazyRegion.Core;

public static class TransitionAnimationHelper
{
    public static TransitionAnimation GetReverse(TransitionAnimation animation)
        => animation switch
        {
            TransitionAnimation.SlideLeft    => TransitionAnimation.SlideRight,
            TransitionAnimation.SlideRight   => TransitionAnimation.SlideLeft,
            TransitionAnimation.SlideUp      => TransitionAnimation.SlideDown,
            TransitionAnimation.SlideDown    => TransitionAnimation.SlideUp,
            TransitionAnimation.NewFromLeft  => TransitionAnimation.NewFromRight,
            TransitionAnimation.NewFromRight => TransitionAnimation.NewFromLeft,
            TransitionAnimation.NewFromUp    => TransitionAnimation.NewFromDown,
            TransitionAnimation.NewFromDown  => TransitionAnimation.NewFromUp,
            TransitionAnimation.ZoomIn       => TransitionAnimation.ZoomOut,
            TransitionAnimation.ZoomOut      => TransitionAnimation.ZoomIn,
            _ => animation  // Fade, Scale, None → 대칭이므로 그대로
        };
}
