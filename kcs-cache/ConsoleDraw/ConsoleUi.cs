namespace kcs_cache.ConsoleDraw;

public class ConsoleUi
{
    public int Width => _uiBuffer.Width;
    public int Height => _uiBuffer.Height;
    
    
    private ConsoleUiBuffer _uiBuffer;
    private Stack<ConsoleUiBuffer> _snapshots = new Stack<ConsoleUiBuffer>();
    private (int, int) _cursor = new(0, 0);
    
    public ConsoleUi(int width, int height)
    {
        for (var i = 0; i < height; ++i)
        {
            Console.WriteLine();
        }

        var cursor = Console.GetCursorPosition();
        var top = height - cursor.Top;
        _uiBuffer = new ConsoleUiBuffer(0, width, height, new ConsoleColors(Console.ForegroundColor, Console.BackgroundColor));
    }

    public void DrawDoubleRectangle(int left, int top, int right, int bottom)
    {
        _uiBuffer.DrawDoubleRectangle(left, top, right, bottom);
        _uiBuffer.Flush();
    }

    public void DrawHorizontalLine(int startX, int startY, int width)
    {
        _uiBuffer.DrawHorizontalLine(startX, startY, width);
        _uiBuffer.Flush();
    }

    public void WriteAt(int x, int y, string text)
    {
        _uiBuffer.WriteAt(x, y, text);
        _cursor.Item1 = x + text.Length;
        _cursor.Item2 = y;
        _uiBuffer.Flush();
    }

    public void Write(string text)
    {
        _uiBuffer.WriteAt(_cursor.Item1, _cursor.Item2, text);
        _cursor.Item1 += text.Length;
        _uiBuffer.Flush();
    }

    public void SetCursorPosition(int x, int y)
    {
        _uiBuffer.SetCursorPosition(x, y);
    }

    public void SetHighlightedColors()
    {
        _uiBuffer.SetColor(new ConsoleColors(ConsoleColor.Black, ConsoleColor.Cyan));
    }
    
    public void SetMessageColors()
    {
        _uiBuffer.SetColor(new ConsoleColors(ConsoleColor.White, ConsoleColor.Red));
    }

    public void SetRedColors()
    {
        _uiBuffer.SetColor(new ConsoleColors(ConsoleColor.Red, _uiBuffer.GetDefaultColors().BackgroundColor));
    }
    
    public void SetDefaultColors()
    {
        _uiBuffer.SetDefaultColor();
    }

    public void PushSnapshot()
    {
        _snapshots.Push(new ConsoleUiBuffer(_uiBuffer));
    }

    public void PopSnapshot()
    {
        _uiBuffer = _snapshots.Pop();
        _uiBuffer.Redraw();
    }
    
    public void Message(string message)
    {
        PushSnapshot();
        
        SetMessageColors();
        WriteAt((_uiBuffer.Width - message.Length) / 2, _uiBuffer.Height / 2, message);
        SetDefaultColors();
        Console.ReadKey(true);
        
        PopSnapshot();
    }
}
