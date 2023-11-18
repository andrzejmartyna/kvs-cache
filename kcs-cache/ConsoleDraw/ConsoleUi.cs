using kcs_cache.Browse;
using kcs_cache.Models;

namespace kcs_cache.ConsoleDraw;

public class ConsoleUi
{
    public BrowseGeometry Geometry => _uiBuffer.Geometry;

    private ConsoleUiBuffer _uiBuffer;
    private Stack<ConsoleUiBuffer> _snapshots = new Stack<ConsoleUiBuffer>();
    private (int, int) _cursor = new(0, 0);
    
    public ConsoleUi(BrowseGeometry _geometry)
    {
        _uiBuffer = new ConsoleUiBuffer(_geometry, new ConsoleColors(Console.ForegroundColor, Console.BackgroundColor));
    }

    public void DrawDoubleRectangle(int left, int top, int right, int bottom)
    {
        _uiBuffer.DrawDoubleRectangle(left, top, right, bottom);
        _uiBuffer.Flush();
    }

    public void DrawDoubleRectangle(Rectangle rectangle)
    {
        _uiBuffer.DrawDoubleRectangle(rectangle.Left, rectangle.Top, rectangle.Right, rectangle.Bottom);
    }

    public void DrawHorizontalLine(HorizontalLine line, bool verticalTerminators)
    {
        _uiBuffer.DrawHorizontalLine(line.Left, line.Top, line.Width, verticalTerminators);
    }

    public void WriteAt(int x, int y, string text)
    {
        _uiBuffer.WriteAt(x, y, text);
        _cursor.Item1 = x + text.Length;
        _cursor.Item2 = y;
        _uiBuffer.Flush();
    }

    public void WriteAt(Point at, string text)
    {
        WriteAt(at.X, at.Y, text);
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
        //TODO: implement method "center"
        WriteAt(Geometry.Full.Left + (Geometry.Full.Width - message.Length) / 2, Geometry.Full.Top + Geometry.Full.Height / 2, message);
        SetDefaultColors();
        Console.ReadKey(true);
        
        PopSnapshot();
    }
}
