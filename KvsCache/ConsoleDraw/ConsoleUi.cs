using KvsCache.Browse;
using KvsCache.Models.Geometry;

namespace KvsCache.ConsoleDraw;

public class ConsoleUi
{
    public BrowseGeometry Geometry => _uiBuffer.Geometry;

    private ConsoleUiBuffer _uiBuffer;
    private readonly Stack<ConsoleUiBuffer> _snapshots = new();
    private Point _cursor = new(0, 0);
    
    public ConsoleUi(BrowseGeometry geometry)
    {
        _uiBuffer = new ConsoleUiBuffer(geometry, new ConsoleColors(Console.ForegroundColor, Console.BackgroundColor));
    }

    public void DrawDoubleRectangle(int left, int top, int right, int bottom)
    {
        _uiBuffer.DrawDoubleRectangle(left, top, right, bottom);
        _uiBuffer.Flush();
    }

    public void DrawDoubleRectangle(Rectangle rectangle)
    {
        DrawDoubleRectangle(rectangle.Left, rectangle.Top, rectangle.Right, rectangle.Bottom);
    }

    public void DrawHorizontalLine(HorizontalLine line, bool verticalTerminators)
    {
        _uiBuffer.DrawHorizontalLine(line.Left, line.Top, line.Width, verticalTerminators);
        _uiBuffer.Flush();
    }

    public void WriteAt(int x, int y, string text)
    {
        _uiBuffer.WriteAt(x, y, text);
        _cursor.X = x + text.Length;
        _cursor.Y = y;
        _uiBuffer.Flush();
    }

    public void WriteAt(Point at, string text)
    {
        WriteAt(at.X, at.Y, text);
    }

    public void Write(string text)
    {
        _uiBuffer.WriteAt(_cursor.X, _cursor.Y, text);
        _cursor.X += text.Length;
        _uiBuffer.Flush();
    }

    public ConsoleColors Highlighted => new(ConsoleColor.Black, ConsoleColor.Cyan);
    public ConsoleColors Red => new(ConsoleColor.Red, _uiBuffer.GetDefaultColors().BackgroundColor);
    public ConsoleColors RedMessage => new(ConsoleColor.White, ConsoleColor.Red);
    public ConsoleColors GreenMessage => new(ConsoleColor.Black, ConsoleColor.DarkGreen);
    
    public void SetColors(ConsoleColors colors)
    {
        _uiBuffer.SetColor(colors);
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

    public void DisplayBoxedText(string message, ConsoleColors colors)
    {
        SetColors(colors);
        //TODO: implement method "center"
        var x = Geometry.Full.Left + (Geometry.Full.Width - message.Length) / 2;
        var y = Geometry.Full.Top + Geometry.Full.Height / 2;
        WriteAt(x, y, message);
        DrawDoubleRectangle(x - 1, y - 1, x + message.Length, y + 1);
        SetDefaultColors();
    }

    public void Message(string message, ConsoleColors colors)
    {
        PushSnapshot();
        DisplayBoxedText(message, colors);
        Console.ReadKey(true);
        PopSnapshot();
    }

    public void FillRectangle(Rectangle rectangle, char ch)
    {
        var emptyLine = new string(ch, rectangle.Width);
        for (var y = rectangle.Top; y <= rectangle.Bottom; ++y)
        {
            WriteAt(rectangle.Left, y, emptyLine);    
        }
        _uiBuffer.Flush();
    }

    public static ConsoleKeyInfo ReadKeyNonBlocking(bool intercept, CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            if (Console.KeyAvailable)
            {
                return Console.ReadKey(intercept);
            }
            Thread.Sleep(100);
        }
        return new ConsoleKeyInfo();
    }
}
