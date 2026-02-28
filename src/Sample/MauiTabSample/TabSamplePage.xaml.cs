using LazyRegion.Core;
using LazyRegion.Maui;

namespace MauiTabSample;

public partial class TabSamplePage : ContentPage
{
    private static readonly Color[] _tabColors =
    [
        Color.FromArgb ("#E74C3C"),
        Color.FromArgb ("#3498DB"),
        Color.FromArgb ("#2ECC71"),
        Color.FromArgb ("#9B59B6")
    ];

    public TabSamplePage()
    {
        InitializeComponent ();
        animPicker.SelectedIndex = 6; // SlideLeft default
        UpdateTabState (0);
    }

    private void OnSelectionChanged(object? sender, int index)
    {
        UpdateTabState (index);
    }

    private void UpdateTabState(int selectedIndex)
    {
        var labels = new[] { tabLabelA, tabLabelB, tabLabelC, tabLabelD };
        var bars   = new[] { tabBarA,   tabBarB,   tabBarC,   tabBarD   };

        for (int i = 0; i < labels.Length; i++)
        {
            bool isSelected = i == selectedIndex;
            labels[i].TextColor = isSelected ? _tabColors[i] : Color.FromArgb ("#8E8E93");
            bars[i].IsVisible   = isSelected;
        }
    }

    private void OnAnimationChanged(object? sender, EventArgs e)
    {
        if (animPicker.SelectedIndex < 0) return;

        var name = animPicker.Items[animPicker.SelectedIndex];
        if (Enum.TryParse<TransitionAnimation> (name, out var anim))
        {
            tabRegion.TransitionAnimation = anim;

            bool swipeEnabled = anim is TransitionAnimation.SlideLeft
                or TransitionAnimation.SlideRight
                or TransitionAnimation.SlideUp
                or TransitionAnimation.SlideDown;

            swipeBadgeLabel.Text     = swipeEnabled ? "Swipe: ON" : "Swipe: OFF";
            swipeBadgeLabel.TextColor = swipeEnabled
                ? Color.FromArgb ("#30D158")
                : Color.FromArgb ("#636366");
        }
    }
}
