using kcs_cache.ConsoleDraw;

namespace kcs_cache.Browse;

public class Browser
{
    private readonly ConsoleUi _console;
    private readonly Action<BrowserItem, bool> _onEnter;

    private readonly int _left;
    private readonly int _top;
    private readonly int _width;
    private readonly int _height;

    private readonly string _itemsName;

    private BrowserItem? _parent;
    private readonly Dictionary<string, BrowseState> _filteredStates = new Dictionary<string, BrowseState>();
    private BrowseState _state;
    private readonly List<(string, object)> _allItems;
    
    public Browser(ConsoleUi console, IEnumerable<(string, object)> items, BrowserItem? parentItem, Action<BrowserItem, bool> onEnter, string itemsName)
    {
        _console = console;
        _onEnter = onEnter;
        _left = 1;
        _top = 6;
        _width = _console.Width - 2;
        _height = _console.Height - 7;
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
        var key = Console.ReadKey();
        while (true)
        {
            if (key.Key is ConsoleKey.Escape or ConsoleKey.LeftArrow)
            {
                break;
            }

            switch (key.Key)
            {
                case ConsoleKey.Enter:
                case ConsoleKey.RightArrow:
                    if (_state.Count > 0)
                    {
                        _onEnter(_state[_state.Selection.Selected], key.Modifiers.HasFlag(ConsoleModifiers.Alt));
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
                        if (_state.Selection.Selected < _state.Selection.FirstDisplayed + _height - 1)
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
                        var targetFirstDisplayed = int.Max(0, _state.Count - _height);
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

    private void ApplyFilter(string newFilter)
    {
        if (!_filteredStates.TryGetValue(newFilter, out var newState))
        {
            var filtered = new List<(string, object)>();
            foreach (var item in _allItems)
            {
                foreach (var word in newFilter.Trim().Split(' '))
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

        _filteredStates[_state.Filter] = _state;
        _state = newState;
        
        _console.WriteAt(_left, _top - 1, new string(' ', _width));
        if (_state.Count <= 0)
        {
            _console.WriteAt(_left, _top - 1, $"No {_itemsName} found");
        }
        else
        {
            _console.WriteAt(_left, _top - 1, $"{_itemsName} found:");
        }

        if (!string.IsNullOrWhiteSpace(_state.Filter))
        {
            _console.Write(" (filtered by: ");
            _console.SetRedColors();
            _console.Write(_state.Filter);
            _console.Write(" )");
        }

        RedrawConsole();
    }

    private void ChangeSelectionTo(int newSelection)
    {
        _console.WriteAt(_left, _top + _state.Selection.Selected - _state.Selection.FirstDisplayed, _state[_state.Selection.Selected].DisplayName);
        _console.SetHighlightedColors();
        _console.WriteAt(_left, _top + newSelection - _state.Selection.FirstDisplayed, _state[newSelection].DisplayName);
        _console.SetDefaultColors();
        
        _state.SetSelection(_state.Selection.FirstDisplayed, newSelection);
    }

    private void RedrawConsole()
    {
        var emptyLine = new string(' ', _width);
        for (var y = _top; y < _top + _height; ++y)
        {
            _console.WriteAt(_left, y, emptyLine);    
        }
        
        var idx = 0;
        for (var y = _state.Selection.FirstDisplayed; y < _state.Count && idx < _height; ++y, ++idx)
        {
            if (y == _state.Selection.Selected)
            {
                _console.SetHighlightedColors();
            }
            _console.WriteAt(_left, _top + idx, _state[idx].DisplayName);
            if (y == _state.Selection.Selected)
            {
                _console.SetDefaultColors();
            }
        }
    }
}
