namespace kcs_cache.Models;

public struct Point
{
    public int X { get; private set; }
    public int Y { get; private set; }

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
