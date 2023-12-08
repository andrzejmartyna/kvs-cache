namespace KvsCache.Models.Geometry;

public struct Point
{
    public int X { get; set; }
    public int Y { get; set; }

    public Point(int x, int y)
    {
        X = x;
        Y = y;
    }

    public void MoveRight(int by)
    {
        X += by;
    }
}
