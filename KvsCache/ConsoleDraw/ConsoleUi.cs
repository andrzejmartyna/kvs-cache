using KvsCache.Browse;
using KvsCache.Models.Geometry;
using TextCopy;

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

    public void WriteAt(int x, int y, string? text)
    {
        if (string.IsNullOrEmpty(text)) return;
        
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

    private void DisplayBoxedText(string title, string message, ConsoleColors colors)
    {
        SetColors(colors);

        var lines = message.Trim().Split(Environment.NewLine);
        var longestLine = lines.Max(a => a.Length);

        var width = Math.Min(Geometry.Full.Width, longestLine + 2);
        var height = Math.Min(Geometry.Full.Height, lines.Length + 2);
        var x = Geometry.Full.Left + (Geometry.Full.Width - width) / 2;
        var y = Geometry.Full.Top + (Geometry.Full.Height - height) / 2;

        DrawDoubleRectangle(x, y, x + width - 1, y + height - 1);
        
        var titleX = Geometry.Full.Left + Math.Max(1, (Geometry.Full.Width - title.Length) / 2);
        WriteAt(titleX, y, title[..Math.Min(title.Length, Geometry.Full.Width - 2)]);
        
        var anyTruncation = title.Length > width - 2;
        for (var lineIdx = 0; lineIdx < height - 2; ++lineIdx)
        {
            var line = lines[lineIdx];
            anyTruncation = anyTruncation || line.Length > width - 2;
            WriteAt(x + 1, y + lineIdx + 1, line[..Math.Min(line.Length, width - 2)]);
        }
        
        if (anyTruncation)
        {
            var footer = "see clipboard for more ...";
            var footerX = Geometry.Full.Left + Math.Max(1, (Geometry.Full.Width - footer.Length) / 2);
            WriteAt(footerX, y + height - 1, footer[..Math.Min(footer.Length, Geometry.Full.Width - 2)]);
            ClipboardService.SetText(title + Environment.NewLine + message);
        }

        SetDefaultColors();
    }

    public void Menu(string title, string[] items, ConsoleColors colors)
    {
        PushSnapshot();
        //TODO: make an interactive menu using arrow keys, Enter, Esc, etc.
        DisplayBoxedText(title, string.Join(Environment.NewLine, items), colors);
        Console.ReadKey(true);
        PopSnapshot();
    }
    
    public void Message(string title, string message, ConsoleColors colors)
    {
        PushSnapshot();
        DisplayBoxedText(title, message, colors);
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
