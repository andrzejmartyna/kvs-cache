using kcs_cache.ConsoleDraw;

namespace kcs_cache.Browse;

public class Browser
{
    private readonly ConsoleUi _console;
    private readonly List<BrowserItem> _items = new List<BrowserItem>();
    private readonly Action<BrowserItem, bool> _onEnter;

    private readonly int _left;
    private readonly int _top;
    private readonly int _width;
    private readonly int _height;

    private readonly string _itemsName;

    private BrowserState _currentState = new BrowserState(0, 0);
    private HashSet<string> _filteredStates = new HashSet<string>();
    
    public Browser(ConsoleUi console, IEnumerable<(string, object)> items, BrowserItem? parentItem, Action<BrowserItem, bool> onEnter, string itemsName)
    {
        _console = console;
        _onEnter = onEnter;
        _left = 1;
        _top = 6;
        _width = _console.Width - 2;
        _height = _console.Height - 7;
        _itemsName = itemsName;

        var itemSorted = new SortedDictionary<string, object>(items.ToDictionary(a => a.Item1, a => a.Item2)); 

        foreach (var item in itemSorted)
        {
            _items.Add(new BrowserItem(BrowserItemType.Single, item.Key, new object[] {item.Value}, parentItem));
        }
    }

    public void Browse()
    {
        _console.PushSnapshot();
        
        if (_items.Count <= 0)
        {
            _console.WriteAt(_left, _top - 1, new string(' ', _width));
            _console.WriteAt(_left, _top - 1, $"No {_itemsName} found");
            var key = Console.ReadKey(true);
            while (key.Key != ConsoleKey.Escape)
            {
                key = Console.ReadKey(true);
            }
        }
        else
        {
            _console.WriteAt(_left, _top - 1, new string(' ', _width));
            _console.WriteAt(_left, _top - 1, $"{_itemsName} found:");
            DisplayAndSelect(new BrowserState(0, 0));
            var key = Console.ReadKey();
            while (true)
            {
                if (key.Key == ConsoleKey.Escape)
                {
                    break;
                }

                switch (key.Key)
                {
                    case ConsoleKey.Enter:
                        _onEnter(_items[_currentState.Selected], key.Modifiers.HasFlag(ConsoleModifiers.Alt));
                        break;
                    case ConsoleKey.UpArrow:
                        if (_currentState.Selected > 0)
                        {
                            if (_currentState.Selected > _currentState.FirstDisplayed)
                            {
                                ChangeSelection(_currentState.Selected, _currentState.Selected - 1);
                            }
                            else
                            {
                                DisplayAndSelect(new BrowserState(_currentState.FirstDisplayed - 1, _currentState.Selected - 1));
                            }
                        }
                        break;
                    case ConsoleKey.DownArrow:
                        if (_currentState.Selected < _items.Count - 1)
                        {
                            if (_currentState.Selected < _currentState.FirstDisplayed + _height - 1)
                            {
                                ChangeSelection(_currentState.Selected, _currentState.Selected + 1);
                            }
                            else
                            {
                                DisplayAndSelect(new BrowserState(_currentState.FirstDisplayed + 1, _currentState.Selected + 1));
                            }
                        }
                        break;
                    case ConsoleKey.Home:
                        if (_currentState.Selected > 0)
                        {
                            DisplayAndSelect(new BrowserState(0, 0));
                        }
                        break;
                    case ConsoleKey.End:
                        if (_currentState.Selected < _items.Count - 1)
                        {
                            DisplayAndSelect(new BrowserState(int.Max(0, _items.Count - _height), _items.Count - 1));
                        }
                        break;
                }
                
                key = Console.ReadKey(true);
            }
        }

        _console.PopSnapshot();
    }

    private void ChangeSelection(int oldSelection, int newSelection)
    {
        _console.WriteAt(_left, _top + oldSelection - _currentState.FirstDisplayed, _items[oldSelection].DisplayName);
        _console.SetHighlightedColors();
        _console.WriteAt(_left, _top + newSelection - _currentState.FirstDisplayed, _items[newSelection].DisplayName);
        _console.SetDefaultColors();
        
        _currentState = new BrowserState(_currentState.FirstDisplayed, newSelection);
    }

    private void DisplayAndSelect(BrowserState newState)
    {
        var emptyLine = new string(' ', _width);
        for (var y = _top; y < _top + _height; ++y)
        {
            _console.WriteAt(_left, y, emptyLine);    
        }
        
        var idx = 0;
        for (var y = newState.FirstDisplayed; y < _items.Count && idx < _height; ++y, ++idx)
        {
            if (y == newState.Selected)
            {
                _console.SetHighlightedColors();
            }
            _console.WriteAt(_left, _top + idx, _items[idx].DisplayName);
            if (y == newState.Selected)
            {
                _console.SetDefaultColors();
            }
        }

        _currentState = newState;
    }
}
