using System.Text.RegularExpressions;
using KvsCache.ConsoleDraw;
using KvsCache.Models.Geometry;

namespace KvsCache.Browse;

public class Browser
{
    public BrowseGeometry Geometry => _context.Console.Geometry;

    private readonly BrowseContext _context;
    private BrowseState? _state;
    
    private readonly Action<BrowserItem, BrowseContext> _onEnter;
    private readonly Action<BrowserItem>? _onInfo;
    private readonly bool _exitOnLeft;

    private readonly Rectangle _rectangle;

    private readonly string _itemsName;

    private BrowserItem? _parent;
    private Dictionary<string, BrowseState> _filteredStates = new();
    private List<BrowserItem> _allItems = new();
    
    public Browser(BrowseContext context, Action<BrowserItem, BrowseContext> onEnter, Action<BrowserItem>? onInfo, string itemsName, bool exitOnLeft)
    {
        _context = context;
        _exitOnLeft = exitOnLeft;
        _onEnter = onEnter;
        _onInfo = onInfo;
        _rectangle = _context.Console.Geometry.BrowsingRectangle;
        _itemsName = itemsName;
    }

    public void Browse(IEnumerable<BrowserItem> items, BrowserItem? parentItem)
    {
        _context.Console.PushSnapshot();

        var enterPreviousSelection = _state is { Count: > 0, Entered: true } && _allItems.Any(a => 0 == string.Compare(a.DisplayName, _state.Selected.DisplayName, StringComparison.InvariantCultureIgnoreCase));
        _parent = parentItem;
        _allItems = items.ToList();
        
        if (_state == null)
        {
            _state = new BrowseState(_allItems, _parent, string.Empty);
            ApplyFilter(_state.Filter);
        }
        else
        {
            _state.ResetItems(_allItems, parentItem);
            _filteredStates = new Dictionary<string, BrowseState>();
            ApplyFilter(_state.Filter);
            _state.Entered = enterPreviousSelection;
        }

        RedrawConsole();
        var key = enterPreviousSelection ? new ConsoleKeyInfo() : ConsoleUi.ReadKeyNonBlocking(true, _context.CancellationToken);
        while (!_context.CancellationToken.IsCancellationRequested)
        {
            if (enterPreviousSelection)
            {
                enterPreviousSelection = false;
                _onEnter(_state.Selected, _context);
                if (_context.CancellationToken.IsCancellationRequested)
                {
                    break;
                }
                _state.Entered = false;
                key = ConsoleUi.ReadKeyNonBlocking(true, _context.CancellationToken);
                continue;
            }

            if (key.Key is ConsoleKey.Escape || (_exitOnLeft &&  key.Key is ConsoleKey.LeftArrow))
            {
                break;
            }

            if (key.Modifiers.HasFlag(ConsoleModifiers.Control))
            {
                switch (key.Key)
                {
                    case ConsoleKey.D:
                        _onInfo?.Invoke(_state.Selected);
                        break;
                    case ConsoleKey.R:
                        _context.RefreshEvent.Set();
                        break;
                }
                
                key = ConsoleUi.ReadKeyNonBlocking(true, _context.CancellationToken);
                continue;
            }

            var pageSize = _rectangle.Height;
            switch (key.Key)
            {
                case ConsoleKey.Enter:
                case ConsoleKey.RightArrow:
                    if (_state.Count > 0)
                    {
                        _state.Entered = true;
                        _onEnter(_state.Selected, _context);
                        if (!_context.CancellationToken.IsCancellationRequested)
                        {
                            _state.Entered = false;
                        }
                    }
                    break;
                case ConsoleKey.UpArrow:
                    if (_state.Selection.Selected > 0)
                    {
                        if (_state.Selection.Selected > _state.Selection.FirstDisplayed)
                        {
                            ChangeSelectionTo(_state.Selection.Selected - 1);
                        }
                        else
                        {
                            _state.SetSelection(_state.Selection.FirstDisplayed - 1, _state.Selection.Selected - 1);
                            RedrawConsole();
                        }
                    }
                    break;
                case ConsoleKey.DownArrow:
                    if (_state.Selection.Selected < _state.Count - 1)
                    {
                        if (_state.Selection.Selected < _state.Selection.FirstDisplayed + _rectangle.Height - 1)
                        {
                            ChangeSelectionTo(_state.Selection.Selected + 1);
                        }
                        else
                        {
                            _state.SetSelection(_state.Selection.FirstDisplayed + 1, _state.Selection.Selected + 1);
                            RedrawConsole();
                        }
                    }
                    break;
                case ConsoleKey.Home:
                    if (_state.Selection.Selected > 0)
                    {
                        if (_state.Selection.FirstDisplayed == 0)
                        {
                            ChangeSelectionTo(0);
                        }
                        else
                        {
                            _state.SetSelection(0, 0);
                            RedrawConsole();
                        }
                    }
                    break;
                case ConsoleKey.End:
                    if (_state.Selection.Selected < _state.Count - 1)
                    {
                        var targetFirstDisplayed = int.Max(0, _state.Count - _rectangle.Height);
                        if (_state.Selection.FirstDisplayed >= targetFirstDisplayed)
                        {
                            ChangeSelectionTo(_state.Count - 1);
                        }
                        else
                        {
                            _state.SetSelection(targetFirstDisplayed, _state.Count - 1);
                            RedrawConsole();
                        }
                    }
                    break;
                case ConsoleKey.PageDown:
                    if (_state.Selection.Selected + pageSize < _state.Count - 1)
                    {
                        _state.SetSelection(_state.Selection.FirstDisplayed + pageSize, _state.Selection.Selected + pageSize);
                        RedrawConsole();
                    }
                    break;
                case ConsoleKey.PageUp:
                    if (_state.Selection.FirstDisplayed - pageSize >= 0)
                    {
                        _state.SetSelection(_state.Selection.FirstDisplayed - pageSize, _state.Selection.Selected - pageSize);
                        RedrawConsole();
                    }
                    break;
                case ConsoleKey.Backspace:
                    if (!string.IsNullOrEmpty(_state.Filter))
                    {
                        ApplyFilter(_state.Filter.Substring(0, _state.Filter.Length - 1));
                    }
                    break;
                default:
                    if (char.IsLetterOrDigit(key.KeyChar) || ' ' == key.KeyChar || '-' == key.KeyChar || '_' == key.KeyChar)
                    {
                        ApplyFilter(_state.Filter + key.KeyChar);
                    }
                    break;
            }
            
            key = ConsoleUi.ReadKeyNonBlocking(true, _context.CancellationToken);
        }

        _context.Console.PopSnapshot();
    }

    private static string CleanFilter(string? filter) => string.IsNullOrWhiteSpace(filter) ? string.Empty : Regex.Replace(filter, @"\s+", " ");
    
    private static string[] SplitFilter(string filter) => CleanFilter(filter).Split(' ', StringSplitOptions.RemoveEmptyEntries);

    private static bool ItemShouldBeVisible(string itemName, IEnumerable<string> words)
    {
        return words.All(word => itemName.Contains(word, StringComparison.InvariantCultureIgnoreCase));
    }
    
    private void ApplyFilter(string? newFilter)
    {
        if (_state == null) return;
        
        newFilter = CleanFilter(newFilter);
        if (!_filteredStates.TryGetValue(newFilter, out var newState))
        {
            if (string.IsNullOrWhiteSpace(newFilter))
            {
                newState = new BrowseState(_allItems, _parent, newFilter);
            }
            else
            {
                var words = SplitFilter(newFilter);
                newState = new BrowseState(_allItems.Where(item => ItemShouldBeVisible(item.DisplayName, words)).ToList(), _parent, newFilter);
            }
        }

        _filteredStates[_state.Filter] = _state;
        _state = newState;

        var header = Geometry.SelectionHeaderLine;
        _context.Console.DrawHorizontalLine(header, false);
        _context.Console.WriteAt(header.Left, header.Top, _state.Count <= 0 ? $"No {_itemsName} found" : $"{_itemsName} found:");

        if (!string.IsNullOrWhiteSpace(_state.Filter))
        {
            _context.Console.Write(" (filtered by");
            if (SplitFilter(_state.Filter).Length > 1)
            {
                _context.Console.Write(" all of");
            }
            _context.Console.Write(": ");
            _context.Console.SetColors(_context.Console.Red);
            _context.Console.Write(CleanFilter(_state.Filter));
            _context.Console.SetDefaultColors();
            _context.Console.Write(")");
        }

        RedrawConsole();
    }

    private void ChangeSelectionTo(int newSelection)
    {
        if (_state == null) return;

        _context.Console.WriteAt(_rectangle.Left, _rectangle.Top + _state.Selection.Selected - _state.Selection.FirstDisplayed, _state.Selected.DisplayName);
        _context.Console.SetColors(_context.Console.Highlighted);
        _context.Console.WriteAt(_rectangle.Left, _rectangle.Top + newSelection - _state.Selection.FirstDisplayed, _state[newSelection].DisplayName);
        _context.Console.SetDefaultColors();
        
        _state.SetSelection(_state.Selection.FirstDisplayed, newSelection);
    }

    private void RedrawConsole()
    {
        if (_state == null) return;

        _context.Console.FillRectangle(_rectangle, ' ');
        
        var idx = 0;
        for (var y = _state.Selection.FirstDisplayed; y < _state.Count && idx < _rectangle.Height; ++y, ++idx)
        {
            if (y == _state.Selection.Selected)
            {
                _context.Console.SetColors(_context.Console.Highlighted);
            }
            _context.Console.WriteAt(_rectangle.Left, _rectangle.Top + idx, _state[y].DisplayName);
            if (y == _state.Selection.Selected)
            {
                _context.Console.SetDefaultColors();
            }
        }
    }
}
