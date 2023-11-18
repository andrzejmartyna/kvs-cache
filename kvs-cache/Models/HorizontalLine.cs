namespace kcs_cache.Models;

public struct HorizontalLine
{
    public int Left { get; init; }
    public int Top { get; init; }
    public int Width { get; init; }

    public HorizontalLine(int left, int top, int width)
    {
        Left = left;
        Top = top;
        Width = width;
    }

    public Rectangle Rectangle => new Rectangle(Left, Top, Width, 1);
}
