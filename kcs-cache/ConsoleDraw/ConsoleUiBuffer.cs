namespace kcs_cache.ConsoleDraw;

public record ConsoleColors(ConsoleColor ForegroundColor, ConsoleColor BackgroundColor);

public record Coordinates(int X, int Y)
{
    public int X { get; set; } = X;
    public int Y { get; set; } = Y;
}

public record Section(int X, int Y, int Color, string Text);

public class ConsoleUiBuffer
{
    public int Top { get; init; }
    public int Width { get; init; }
    public int Height { get; init; }

    private List<string> _textBuffer;
    private int[,] _colorsBuffer;
    private List<ConsoleColors> _colors = new List<ConsoleColors>();
    private Coordinates _cursor;
    private int _currentColor;
    private Coordinates _invalidatedTopLeft = new Coordinates(-1, -1);
    private Coordinates _invalidatedBottomRight = new Coordinates(-1, -1);

    public ConsoleUiBuffer(ConsoleUiBuffer originalBuffer)
    {
        Top = originalBuffer.Top;
        Width = originalBuffer.Width;
        Height = originalBuffer.Height;
        _textBuffer = new List<string>(originalBuffer._textBuffer);
        _colorsBuffer = (int[,]) originalBuffer._colorsBuffer.Clone();
        _colors = new List<ConsoleColors>(originalBuffer._colors);
        _cursor = originalBuffer._cursor;
        _currentColor = originalBuffer._currentColor;
        
        InvalidateAll();
    }

    public ConsoleUiBuffer(int top, int width, int height, ConsoleColors defaultColors)
    {
        Top = top;
        Width = width;
        Height = height;

        _textBuffer = new List<string>();
        var emptyLine = new string(' ', Width);
        for (var y = 0; y < Height; ++y)
        {
            _textBuffer.Add(emptyLine);
        }

        _colorsBuffer = new int[Width, Height];
        _colors.Add(defaultColors);
        _currentColor = 0;
        _cursor = new Coordinates(0, 0);
    }

    private void InvalidateAll()
    {
        _invalidatedTopLeft = new Coordinates(0, 0);
        _invalidatedBottomRight = new Coordinates(Width - 1, Height - 1);
    }

    private void ValidateAll()
    {
        _invalidatedTopLeft = new Coordinates(-1, -1);
        _invalidatedBottomRight = new Coordinates(-1, -1);
    }

    public void SetCursorPosition(int left, int top) => _cursor = new Coordinates(left, top);

    public void Write(char chr)
    {
        Write(new string(chr, 1));
    }

    public void Write(string text)
    {
        _textBuffer[_cursor.Y] = _textBuffer[_cursor.Y].Remove(_cursor.X, text.Length).Insert(_cursor.X, text);
        for (int x = 0; x < text.Length; ++x)
        {
            _colorsBuffer[_cursor.X + x, _cursor.Y] = _currentColor;
        }

        Invalidate(_cursor, text.Length);
        
        _cursor.X += text.Length;
    }

    private void Invalidate(Coordinates cursor, int width)
    {
        if (_invalidatedTopLeft.X < 0)
        {
            _invalidatedTopLeft = new Coordinates(cursor.X, cursor.Y);
            _invalidatedBottomRight = new Coordinates(cursor.X + width - 1, cursor.Y);
            return;
        }
        if (_invalidatedTopLeft.X > cursor.X)
        {
            _invalidatedTopLeft.X = cursor.X;
        }
        if (_invalidatedBottomRight.X < cursor.X + width - 1)
        {
            _invalidatedBottomRight.X = cursor.X + width - 1;
        }

        if (_invalidatedTopLeft.Y > cursor.Y)
        {
            _invalidatedTopLeft.Y = cursor.Y;
        }
        else if (_invalidatedBottomRight.Y < cursor.Y)
        {
            _invalidatedBottomRight.Y = cursor.Y;
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

    public void DrawHorizontalLine(int startX, int startY, int width)
    {
        SetCursorPosition(startX, startY);
        Write(new string('\u2500', width));
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
        if (_invalidatedTopLeft.X < 0)
        {
            return;
        }

        var paintingColor = -1;
        foreach (var section in EnumerateSections(_invalidatedTopLeft, _invalidatedBottomRight))
        {
            Console.SetCursorPosition(section.X, Top + section.Y);
            if (section.Color != paintingColor)
            {
                paintingColor = section.Color;
                Console.BackgroundColor = _colors[paintingColor].BackgroundColor;
                Console.ForegroundColor = _colors[paintingColor].ForegroundColor;
            }
            Console.Write(section.Text);
        }
        SetDefaultColor();
        Console.BackgroundColor = _colors[_currentColor].BackgroundColor;
        Console.ForegroundColor = _colors[_currentColor].ForegroundColor;
        ValidateAll();
    }
    
    private IEnumerable<Section> EnumerateSections(Coordinates topLeft, Coordinates bottomRight)
    {
        for (var y = topLeft.Y; y <= bottomRight.Y; ++y)
        {
            var startX = topLeft.X;
            var currentColor = _colorsBuffer[startX, y];
            for (var x = topLeft.X + 1; x <= bottomRight.X; ++x)
            {
                if (_colorsBuffer[x, y] != currentColor)
                {
                    yield return new Section(startX, y, currentColor, _textBuffer[y].Substring(startX, x - startX));
                    startX = x;
                    currentColor = _colorsBuffer[x, y];
                }
            }
            yield return new Section(startX, y, currentColor, _textBuffer[y].Substring(startX, bottomRight.X - startX + 1));
        }
    }
}
