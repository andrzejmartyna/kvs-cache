using System.Text.RegularExpressions;
using kcs_cache.ConsoleDraw;
using kcs_cache.Models;

namespace kcs_cache.Browse;

public class Browser
{
    public BrowseGeometry Geometry => _console.Geometry;
    
    private readonly ConsoleUi _console;
    private readonly Action<BrowserItem> _onEnter;
    private readonly Action<BrowserItem>? _onInfo;

    private readonly Rectangle _rectangle;

    private readonly string _itemsName;

    private BrowserItem? _parent;
    private readonly Dictionary<string, BrowseState> _filteredStates = new Dictionary<string, BrowseState>();
    private BrowseState _state;
    private readonly List<(string, object)> _allItems;
    
    public Browser(ConsoleUi console, IEnumerable<(string, object)> items, BrowserItem? parentItem, Action<BrowserItem> onEnter, Action<BrowserItem>? onInfo, string itemsName)
    {
        _console = console;
        _onEnter = onEnter;
        _onInfo = onInfo;
        _rectangle = _console.Geometry.BrowsingRectangle;
        _itemsName = itemsName;

        _parent = parentItem;
        _allItems = items.ToList();
        _state = new BrowseState(_allItems, _parent, string.Empty);
    }

    public void Browse()
    {
        _console.PushSnapshot();
        
        ApplyFilter(string.Empty);

        _state.SetSelection(0, 0);
        RedrawConsole();
        var key = Console.ReadKey(true);
        while (true)
        {
            if (key.Key is ConsoleKey.Escape or ConsoleKey.LeftArrow)
            {
                break;
            }

            switch (key.Key)
            {
                case ConsoleKey.D:
                    if (_onInfo != null && key.Modifiers.HasFlag(ConsoleModifiers.Control))
                    {
                        _onInfo(_state[_state.Selection.Selected]);
                    }
                    break;
                case ConsoleKey.Enter:
                case ConsoleKey.RightArrow:
                    if (_state.Count > 0)
                    {
                        _onEnter(_state[_state.Selection.Selected]);
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
            
            key = Console.ReadKey(true);
        }

        _console.PopSnapshot();
    }

    private static string CleanFilter(string filter) => Regex.Replace(filter, @"\s+", " ");
    
    private static string[] SplitFilter(string filter) => CleanFilter(filter).Split(' ', StringSplitOptions.RemoveEmptyEntries); 
    
    private void ApplyFilter(string newFilter)
    {
        newFilter = CleanFilter(newFilter);
        if (!_filteredStates.TryGetValue(newFilter, out var newState))
        {
            if (string.IsNullOrWhiteSpace(newFilter))
            {
                newState = new BrowseState(_allItems, _parent, newFilter);
            }
            else
            {
                var filtered = new List<(string, object)>();
                var words = SplitFilter(newFilter);
                foreach (var item in _allItems)
                {
                    foreach (var word in words)
                    {
                        if (item.Item1.Contains(word, StringComparison.InvariantCultureIgnoreCase))
                        {
                            filtered.Add(item);
                            break;
                        }
                    }
                }
                newState = new BrowseState(filtered, _parent, newFilter);
            }
        }

        _filteredStates[_state.Filter] = _state;
        _state = newState;

        var header = Geometry.SelectionHeaderLine;
        _console.DrawHorizontalLine(header, false);
        if (_state.Count <= 0)
        {
            _console.WriteAt(header.Left, header.Top, $"No {_itemsName} found");
        }
        else
        {
            _console.WriteAt(header.Left, header.Top, $"{_itemsName} found:");
        }

        if (!string.IsNullOrWhiteSpace(_state.Filter))
        {
            _console.Write(" (filtered by");
            if (SplitFilter(_state.Filter).Length > 1)
            {
                _console.Write(" any of");
            }
            _console.Write(": ");
            _console.SetColors(_console.Red);
            _console.Write(CleanFilter(_state.Filter));
            _console.SetDefaultColors();
            _console.Write(")");
        }

        RedrawConsole();
    }

    private void ChangeSelectionTo(int newSelection)
    {
        _console.WriteAt(_rectangle.Left, _rectangle.Top + _state.Selection.Selected - _state.Selection.FirstDisplayed, _state[_state.Selection.Selected].DisplayName);
        _console.SetColors(_console.Highlighted);
        _console.WriteAt(_rectangle.Left, _rectangle.Top + newSelection - _state.Selection.FirstDisplayed, _state[newSelection].DisplayName);
        _console.SetDefaultColors();
        
        _state.SetSelection(_state.Selection.FirstDisplayed, newSelection);
    }

    private void RedrawConsole()
    {
        _console.FillRectangle(_rectangle, ' ');
        
        var idx = 0;
        for (var y = _state.Selection.FirstDisplayed; y < _state.Count && idx < _rectangle.Height; ++y, ++idx)
        {
            if (y == _state.Selection.Selected)
            {
                _console.SetColors(_console.Highlighted);
            }
            _console.WriteAt(_rectangle.Left, _rectangle.Top + idx, _state[idx].DisplayName);
            if (y == _state.Selection.Selected)
            {
                _console.SetDefaultColors();
            }
        }
    }
}
