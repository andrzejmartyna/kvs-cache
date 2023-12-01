namespace KvsCache.Models.Geometry;

public struct Rectangle
{
    public int Left { get; private set; }
    public int Top { get; private set; }
    public int Width { get; private set; }
    public int Height { get; private set; }
    public int Right => Left + Width - 1;
    public int Bottom => Top + Height - 1;

    public Rectangle(int left, int top, int width, int height)
    {
        Left = left;
        Top = top;
        Width = width;
        Height = height;
    }

    public void SetLeft(int x)
    {
        Width += Left - x;
        Left = x;
    }

    public void SetRight(int x)
    {
        Width += x - Right;
    }

    public void SetTop(int y)
    {
        Height += Top - y;
        Top = y;
    }

    public void SetBottom(int y)
    {
        Height += y - Bottom;
    }
}
