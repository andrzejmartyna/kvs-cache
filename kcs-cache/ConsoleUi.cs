namespace kcs_cache;

public class ConsoleUi
{
    public int Width { get; private set; }
    public int Height { get; private set; }
    
    public ConsoleUi(int width, int height)
    {
        Width = width;
        Height = height;
        for (var i = 0; i < height; ++i)
        {
            Console.WriteLine();
        }
    }

    public void DrawDoubleRectangle(int left, int top, int right, int bottom)
    {
        Console.SetCursorPosition(left, top);
        Console.Write('\u2554');
        Console.Write(new string('\u2550', right - left - 1));
        Console.Write('\u2557');
        for (var y = top + 1; y < bottom; ++y)
        {
            Console.SetCursorPosition(left, y);
            Console.Write('\u2551');
            Console.SetCursorPosition(right, y);
            Console.Write('\u2551');
        }
        Console.SetCursorPosition(left, bottom);
        Console.Write('\u255A');
        Console.Write(new string('\u2550', right - left - 1));
        Console.Write('\u255D');
    }

    public void DrawHorizontalLine(int startX, int startY, int width)
    {
        Console.SetCursorPosition(startX, startY);
        Console.Write(new string('\u2500', width));
    }

    public void WriteAt(int x, int y, string text)
    {
        Console.SetCursorPosition(x, y);
        Console.Write(text);
    }

    public void MoveTo(int x, int y)
    {
        Console.SetCursorPosition(x, y);
    }
}
