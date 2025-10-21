using MauiReactor.Shapes;

namespace MauiReactorSample.Components;

public class ScreenAComponent : Component
{
    public override VisualNode Render()
        => Border (
                Label ("Screen A")
                    .FontSize (28)
                    .HStart ()
                    .VStart ()
                    .TextColor (Colors.White)
           )
           .StrokeShape(RoundRectangle()
                            .CornerRadius(10)
            )
           .Background (Color.Parse("#2d2f33"))
           .Margin (50)
           .Padding (10);
}

public class ScreenBComponent : Component
{
    public override VisualNode Render()
        => Border (
                Label ("Screen B")
                    .FontSize(28)
                    .HStart()
                    .VStart()
                    .TextColor(Colors.White)
           )
           .StrokeShape (RoundRectangle ()
                            .CornerRadius (10)
            )
           .Background (Color.Parse ("#4a4a50"))
           .Margin (50)
           .Padding (10);
}
