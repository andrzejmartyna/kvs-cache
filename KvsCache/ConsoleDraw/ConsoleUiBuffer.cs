using KvsCache.Browse;
using KvsCache.Models.Geometry;

namespace KvsCache.ConsoleDraw;

public record ConsoleColors(ConsoleColor ForegroundColor, ConsoleColor BackgroundColor);

public record Section(int X, int Y, int Color, string Text);

public class ConsoleUiBuffer
{
    public BrowseGeometry Geometry { get; }

    private readonly List<string> _textBuffer;
    private readonly int[,] _colorsBuffer;
    private readonly List<ConsoleColors> _colors = new();
    private Point _cursor;
    private int _currentColor;
    private Rectangle _invalidatedArea;
    private readonly object _lock = new();

    public ConsoleUiBuffer(ConsoleUiBuffer originalBuffer)
    {
        Geometry = new BrowseGeometry(originalBuffer.Geometry);
        _textBuffer = new List<string>(originalBuffer._textBuffer);
        _colorsBuffer = (int[,]) originalBuffer._colorsBuffer.Clone();
        _colors = new List<ConsoleColors>(originalBuffer._colors);
        _cursor = originalBuffer._cursor;
        _currentColor = originalBuffer._currentColor;
        
        InvalidateAll();
    }

    public ConsoleUiBuffer(BrowseGeometry geometry, ConsoleColors defaultColors)
    {
        Geometry = geometry;

        _textBuffer = new List<string>();
        var emptyLine = new string(' ', Geometry.Full.Width);
        for (var y = 0; y < Geometry.Full.Height; ++y)
        {
            _textBuffer.Add(emptyLine);
        }

        _colorsBuffer = new int[Geometry.Full.Width, Geometry.Full.Height];
        _colors.Add(defaultColors);
        _currentColor = 0;
        _cursor = new Point(0, 0);
    }

    private void InvalidateAll()
    {
        _invalidatedArea = new Rectangle(0, 0, Geometry.Full.Width, Geometry.Full.Height);
    }

    private void ValidateAll()
    {
        _invalidatedArea = new Rectangle(0, 0, 0, 0);
    }

    //TODO: better handle translation from absolute coordinates to relative coordinates
    public void SetCursorPosition(int left, int top) => _cursor = new Point(left - Geometry.Full.Left, top - Geometry.Full.Top);

    public void Write(char chr)
    {
        Write(new string(chr, 1));
    }

    public void Write(string text)
    {
        _textBuffer[_cursor.Y] = _textBuffer[_cursor.Y].Remove(_cursor.X, text.Length).Insert(_cursor.X, text);
        for (var x = 0; x < text.Length; ++x)
        {
            _colorsBuffer[_cursor.X + x, _cursor.Y] = _currentColor;
        }

        Invalidate(_cursor, text.Length);

        _cursor.MoveRight(text.Length);
    }

    private void Invalidate(Point cursor, int width)
    {
        if (_invalidatedArea.Height <= 0)
        {
            _invalidatedArea = new Rectangle(cursor.X, cursor.Y, width, 1);
            return;
        }
        
        if (_invalidatedArea.Left > cursor.X)
        {
            _invalidatedArea.SetLeft(cursor.X);
        }
        if (_invalidatedArea.Right < cursor.X + width - 1)
        {
            _invalidatedArea.SetRight(cursor.X + width - 1);
        }

        if (_invalidatedArea.Top > cursor.Y)
        {
            _invalidatedArea.SetTop(cursor.Y);
        }
        else if (_invalidatedArea.Bottom < cursor.Y)
        {
            _invalidatedArea.SetBottom(cursor.Y);
        }
    }

    public void SetDefaultColor() => _currentColor = 0;

    public ConsoleColors GetDefaultColors() => _colors[0];
    
    public void SetColor(ConsoleColors colors)
    {
        var index = _colors.FindIndex(a => a == colors);
        if (index >= 0)
        {
            _currentColor = index;
        }
        else
        {
            _colors.Add(colors);
            _currentColor = _colors.Count - 1;
        }
    }
    
    //INFO: [Box drawing ASCII characters](https://en.wikipedia.org/wiki/Box-drawing_character)
    public void DrawDoubleRectangle(int left, int top, int right, int bottom)
    {
        SetCursorPosition(left, top);
        Write('\u2554');
        Write(new string('\u2550', right - left - 1));
        Write('\u2557');
        for (var y = top + 1; y < bottom; ++y)
        {
            SetCursorPosition(left, y);
            Write('\u2551');
            SetCursorPosition(right, y);
            Write('\u2551');
        }
        SetCursorPosition(left, bottom);
        Write('\u255A');
        Write(new string('\u2550', right - left - 1));
        Write('\u255D');
    }

    public void DrawHorizontalLine(int startX, int startY, int width, bool verticalTerminators)
    {
        SetCursorPosition(startX, startY);
        Write(verticalTerminators ? "\u255F" : "\u2500");
        if (width > 2)
        {
            Write(new string('\u2500', width - 2));
        }
        if (width > 1)
        {
            Write(verticalTerminators ? "\u2562" : "\u2500");
        }
    }

    public void WriteAt(int x, int y, string text)
    {
        SetCursorPosition(x, y);
        Write(text);
    }

    public void Redraw()
    {
        InvalidateAll();
        Flush();
    }

    public void Flush()
    {
        if (_invalidatedArea.Height <= 0)
        {
            return;
        }

        lock (_lock)
        {
            var paintingColor = -1;
            foreach (var section in EnumerateSections(_invalidatedArea))
            {
                //TODO: better handle translation from relative coordinates to absolute coordinates
                Console.SetCursorPosition(Geometry.Full.Left + section.X, Geometry.Full.Top + section.Y);
                if (section.Color != paintingColor)
                {
                    paintingColor = section.Color;
                    Console.BackgroundColor = _colors[paintingColor].BackgroundColor;
                    Console.ForegroundColor = _colors[paintingColor].ForegroundColor;
                }
                Console.Write(section.Text);
            }

            Console.BackgroundColor = _colors[_currentColor].BackgroundColor;
            Console.ForegroundColor = _colors[_currentColor].ForegroundColor;
            ValidateAll();
        }
    }
    
    private IEnumerable<Section> EnumerateSections(Rectangle rectangle)
    {
        for (var y = rectangle.Top; y <= rectangle.Bottom; ++y)
        {
            var startX = rectangle.Left;
            var currentColor = _colorsBuffer[startX, y];
            for (var x = rectangle.Left + 1; x <= rectangle.Right; ++x)
            {
                if (_colorsBuffer[x, y] != currentColor)
                {
                    yield return new Section(startX, y, currentColor, _textBuffer[y].Substring(startX, x - startX));
                    startX = x;
                    currentColor = _colorsBuffer[x, y];
                }
            }
            yield return new Section(startX, y, currentColor, _textBuffer[y].Substring(startX, rectangle.Right - startX + 1));
        }
    }
}
