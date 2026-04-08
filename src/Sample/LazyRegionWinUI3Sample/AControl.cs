using Microsoft.UI;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LazyRegionWinUI3Sample
{
    public class AControl : UserControl
    {
        public AControl()
        {
            this.Content = new Border ()
            {
                CornerRadius = new Microsoft.UI.Xaml.CornerRadius (10),
                Background = new Microsoft.UI.Xaml.Media.SolidColorBrush (Colors.AliceBlue),
                Child = new TextBlock ()
                {
                    Text = "Screen A",
                    HorizontalAlignment = Microsoft.UI.Xaml.HorizontalAlignment.Stretch,
                    VerticalAlignment = Microsoft.UI.Xaml.VerticalAlignment.Stretch,
                    FontSize = 28,
                    Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush (Microsoft.UI.Colors.Black),
                },
            };
            this.HorizontalAlignment = Microsoft.UI.Xaml.HorizontalAlignment.Stretch;
            this.VerticalAlignment = Microsoft.UI.Xaml.VerticalAlignment.Stretch;
            this.Background = new Microsoft.UI.Xaml.Media.SolidColorBrush (Colors.AliceBlue);
        }
    }
    public class BControl : UserControl
    {
        public BControl()
        {
            this.Content = new Border ()
            {
                CornerRadius = new Microsoft.UI.Xaml.CornerRadius (10),
                Background = new Microsoft.UI.Xaml.Media.SolidColorBrush (Colors.IndianRed),
                Child = new TextBlock ()
                {
                    Text = "Screen B",
                    HorizontalAlignment = Microsoft.UI.Xaml.HorizontalAlignment.Stretch,
                    VerticalAlignment = Microsoft.UI.Xaml.VerticalAlignment.Stretch,
                    FontSize = 28,
                    Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush (Microsoft.UI.Colors.Black),
                },
            };

            this.Background = new Microsoft.UI.Xaml.Media.SolidColorBrush (Colors.IndianRed);
        }
    }
    public class CControl : UserControl
    {
        public CControl()
        {
            this.Content = new Border ()
            {
                CornerRadius = new Microsoft.UI.Xaml.CornerRadius (10),
                Background = new Microsoft.UI.Xaml.Media.SolidColorBrush (Colors.Indigo),
                Child = new TextBlock ()
                {
                    Text = "Screen C",
                    HorizontalAlignment = Microsoft.UI.Xaml.HorizontalAlignment.Stretch,
                    VerticalAlignment = Microsoft.UI.Xaml.VerticalAlignment.Stretch,
                    FontSize = 28,
                    Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush (Microsoft.UI.Colors.Black),
                },
            };
            this.Background = new Microsoft.UI.Xaml.Media.SolidColorBrush (Colors.Indigo);
        }
    }
}
