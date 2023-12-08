namespace KvsCache.Models.Geometry;

public readonly struct HorizontalLine
{
    public int Left { get; }
    public int Top { get; }
    public int Width { get; }

    public HorizontalLine(int left, int top, int width)
    {
        Left = left;
        Top = top;
        Width = width;
    }

    public Rectangle Rectangle => new(Left, Top, Width, 1);
}
