using OneVOneTimer.Overlay;
using static OneVOneTimer.Overlay.OverlayLayout;

namespace OneVOneTimer.Tests;

public static class OverlayLayoutTests
{
    const double Shape = 4.0; // width over height

    static PixelRect R(int left, int top, int width, int height) => new(left, top, left + width, top + height);

    [Test]
    public static void Dragging_a_side_sets_the_width_and_the_height_follows()
    {
        var current = R(100, 100, 200, 50);
        var right = Sizing(R(100, 100, 400, 50), current, WmszRight, Shape);
        Assert.Equal(R(100, 100, 400, 100), right, "right edge: twice as wide, twice as high, top-left stays");
        var left = Sizing(R(0, 100, 300, 50), current, WmszLeft, Shape);
        Assert.Equal(R(0, 100, 300, 75), left, "left edge: the right side stays");
    }

    [Test]
    public static void Dragging_the_top_or_bottom_sets_the_height_and_the_width_follows()
    {
        var current = R(100, 100, 200, 50);
        Assert.Equal(R(100, 100, 400, 100), Sizing(R(100, 100, 200, 100), current, WmszBottom, Shape), "bottom: the top stays");
        Assert.Equal(R(100, 50, 400, 100), Sizing(R(100, 50, 200, 100), current, WmszTop, Shape), "top: the bottom stays");
    }

    [Test]
    public static void A_corner_follows_whichever_way_was_pulled_further()
    {
        var current = R(100, 100, 200, 50);
        Assert.Equal(R(100, 100, 320, 80), Sizing(R(100, 100, 320, 55), current, WmszBottomRight, Shape), "pulled out sideways");
        Assert.Equal(R(100, 100, 160, 40), Sizing(R(100, 100, 195, 40), current, WmszBottomRight, Shape), "pushed in from below");
        var topLeft = Sizing(R(60, 90, 240, 60), current, WmszTopLeft, Shape);
        Assert.Equal(300, topLeft.Right, "top-left: the right side stays");
        Assert.Equal(150, topLeft.Bottom, "top-left: the bottom stays");
    }

    [Test]
    public static void Sizes_stay_within_the_limits()
    {
        var current = R(0, 0, 200, 50);
        Assert.Equal(MinHeight, Sizing(R(0, 0, 8, 2), current, WmszBottomRight, Shape).Height, "never smaller than the smallest");
        Assert.Equal(MaxHeight, Sizing(R(0, 0, 90000, 50), current, WmszRight, Shape).Height, "never larger than the largest");
        Assert.Equal(MinHeight, ClampHeight(1), "clamped up");
        Assert.Equal(MaxHeight, ClampHeight(int.MaxValue), "clamped down");
    }

    [Test]
    public static void The_shape_comes_from_the_numbers_room()
    {
        double box = BoxEm(3.0);
        Assert.Equal(3.0 + 2 * SideEm, box, "the text and its sides");
        Assert.Equal(2 * box / HeightEm, Aspect(box), "two boxes side by side");
        Assert.Equal(300, WidthFor(100, 3.0), "width from height");
    }

    [Test]
    public static void The_edges_resize_and_the_middle_moves()
    {
        const int w = 200, h = 50, band = 6;
        Assert.Equal(HtCaption, HitTest(100, 25, w, h, band), "middle: move");
        Assert.Equal(HtLeft, HitTest(2, 25, w, h, band), "left edge");
        Assert.Equal(HtRight, HitTest(198, 25, w, h, band), "right edge");
        Assert.Equal(HtTop, HitTest(100, 1, w, h, band), "top edge");
        Assert.Equal(HtBottom, HitTest(100, 48, w, h, band), "bottom edge");
        Assert.Equal(HtTopLeft, HitTest(1, 1, w, h, band), "top-left corner");
        Assert.Equal(HtBottomRight, HitTest(197, 49, w, h, band), "bottom-right corner");
        Assert.Equal(HtBottomRight, HitTest(190, 49, w, h, band), "the corner reaches a little along the edge");
        Assert.Equal(HtTopRight, HitTest(199, 8, w, h, band), "and down the side");
    }

    [Test]
    public static void A_small_window_keeps_a_middle_to_drag()
    {
        // 24 px high with a 12 px band asked: the band shrinks so the middle stays.
        Assert.Equal(HtCaption, HitTest(50, 12, 100, 24, 12), "middle of a small window");
    }
}
