namespace GROD2;

public class Grod2
{
    private const string Version = "2.0.0";

    private readonly StringComparer OIC = StringComparer.OrdinalIgnoreCase;

    private readonly Dictionary<string, Level> _levels = new(StringComparer.OrdinalIgnoreCase);

    private string? _currentLevelKey;
    private Level? _currentLevel;

    public Grod2()
    {
    }

    public static string GetVersion()
    {
        return Version;
    }

    #region Level management

    public List<string> LevelKeys => [.. _levels.Keys];

    public void AddLevel(string levelKey)
    {
        ValidateLevelKey(levelKey);
        if (_levels.ContainsKey(levelKey))
        {
            throw new InvalidOperationException($"Level '{levelKey}' already exists.");
        }
        _levels[levelKey] = new Level(levelKey);
    }

    public void AddLevel(string levelKey, string parentLevelKey)
    {
        ValidateLevelKey(levelKey);
        ValidateLevelKey(parentLevelKey);
        if (_levels.ContainsKey(levelKey))
        {
            throw new InvalidOperationException($"Level '{levelKey}' already exists.");
        }
        if (!_levels.TryGetValue(parentLevelKey, out var _))
        {
            throw new KeyNotFoundException($"Parent level '{parentLevelKey}' not found.");
        }
        _levels[levelKey] = new Level(levelKey, parentLevelKey);
    }

    public void RemoveLevel(string levelKey)
    {
        ValidateLevelKey(levelKey);
        if (!_levels.Remove(levelKey))
        {
            throw new KeyNotFoundException($"Level '{levelKey}' not found.");
        }
    }

    public void SetCurrentLevel(string levelKey)
    {
        ValidateLevelKey(levelKey);
        if (!_levels.TryGetValue(levelKey, out var lvl))
        {
            throw new KeyNotFoundException($"Level '{levelKey}' not found.");
        }
        _currentLevelKey = levelKey;
        _currentLevel = lvl;
    }

    public string? GetCurrentLevel()
    {
        return _currentLevelKey;
    }

    #endregion

    #region Get methods

    public string? Get(string key)
    {
        ValidateCurrentLevel();
        return Get(_currentLevel!, key);
    }

    public string? Get(string levelKey, string key)
    {
        ValidateLevelKey(levelKey);
        if (!_levels.TryGetValue(levelKey, out var lvl))
        {
            throw new KeyNotFoundException($"Level '{levelKey}' not found.");
        }
        return Get(lvl, key);
    }

    private string? Get(Level lvl, string key)
    {
        if (lvl == null)
        {
            throw new ArgumentNullException(nameof(lvl), "Level cannot be null.");
        }
        var value = lvl.Get(key);
        while (value == null && lvl != null && lvl.Parent != null)
        {
            if (!_levels.TryGetValue(lvl.Parent, out var lvl2))
            {
                throw new KeyNotFoundException($"Level '{lvl.Parent}' not found.");
            }
            lvl = lvl2;
            value = lvl?.Get(key);
        }
        return value;
    }

    #endregion

    #region GetItem methods

    public Item? GetItem(string key)
    {
        ValidateCurrentLevel();
        return _currentLevel!.GetItem(key);
    }

    public Item? GetItem(string levelKey, string key)
    {
        ValidateLevelKey(levelKey);
        if (!_levels.TryGetValue(levelKey, out var lvl))
        {
            throw new KeyNotFoundException($"Level '{levelKey}' not found.");
        }
        var item = lvl.GetItem(key);
        while (item == null && lvl != null && lvl.Parent != null)
        {
            if (!_levels.TryGetValue(lvl.Parent, out var lvl2))
            {
                throw new KeyNotFoundException($"Level '{lvl.Parent}' not found.");
            }
            lvl = lvl2;
            item = lvl?.GetItem(key);
        }
        return item;
    }

    #endregion

    #region Set methods

    public void Set(string key, string? text)
    {
        ValidateCurrentLevel();
        _currentLevel!.Set(key, text);
    }

    public void Set(string levelKey, string key, string? text)
    {
        ValidateLevelKey(levelKey);
        if (!_levels.TryGetValue(levelKey, out var lvl))
        {
            throw new KeyNotFoundException($"Level '{levelKey}' not found.");
        }
        lvl.Set(key, text);
    }

    public void Set(Item item)
    {
        if (item == null)
        {
            throw new ArgumentNullException(nameof(item), "Item cannot be null.");
        }
        ValidateCurrentLevel();
        _currentLevel!.Set(item);
    }

    public void Set(string levelKey, Item item)
    {
        ValidateLevelKey(levelKey);
        if (!_levels.TryGetValue(levelKey, out var lvl))
        {
            throw new KeyNotFoundException($"Level '{levelKey}' not found.");
        }
        lvl.Set(item);
    }

    #endregion

    #region Remove methods

    public void Remove(string key)
    {
        ValidateCurrentLevel();
        _currentLevel!.Remove(key);
    }

    public void Remove(string levelKey, string key)
    {
        ValidateLevelKey(levelKey);
        if (!_levels.TryGetValue(levelKey, out var lvl))
        {
            throw new KeyNotFoundException($"Level '{levelKey}' not found.");
        }
        lvl.Remove(key);
    }

    #endregion

    #region Clear methods

    public void Clear()
    {
        ValidateCurrentLevel();
        _currentLevel!.Clear();
    }

    public void Clear(string levelKey)
    {
        ValidateLevelKey(levelKey);
        if (!_levels.TryGetValue(levelKey, out var lvl))
        {
            throw new KeyNotFoundException($"Level '{levelKey}' not found.");
        }
        lvl.Clear();
    }

    #endregion

    #region GetKeys methods

    public IEnumerable<string> GetKeys()
    {
        ValidateCurrentLevel();
        return GetKeys(_currentLevelKey!);
    }

    public IEnumerable<string> GetKeysSorted()
    {
        ValidateCurrentLevel();
        return GetKeysSorted(_currentLevelKey!);
    }

    public IEnumerable<string> GetKeys(string levelKey)
    {
        ValidateLevelKey(levelKey);
        if (!_levels.TryGetValue(levelKey, out var lvl))
        {
            throw new KeyNotFoundException($"Level '{levelKey}' not found.");
        }
        return lvl.Keys;
    }

    public IEnumerable<string> GetKeysSorted(string levelKey)
    {
        ValidateLevelKey(levelKey);
        if (!_levels.TryGetValue(levelKey, out var lvl))
        {
            throw new KeyNotFoundException($"Level '{levelKey}' not found.");
        }
        return lvl.SortedKeys;
    }

    public List<string> GetKeysRecursive()
    {
        ValidateCurrentLevel();
        return GetKeysRecursive(_currentLevelKey!);
    }

    public List<string> GetKeysRecursive(string levelKey)
    {
        ValidateLevelKey(levelKey);
        if (!_levels.TryGetValue(levelKey, out var lvl))
        {
            throw new KeyNotFoundException($"Level '{levelKey}' not found.");
        }
        List<string> allKeys = [];
        while (lvl != null)
        {
            foreach (var key in lvl.Keys)
            {
                if (!allKeys.Contains(key))
                {
                    allKeys.Add(key);
                }
            }
            if (string.IsNullOrWhiteSpace(lvl.Parent))
            {
                break;
            }
            if (!_levels.TryGetValue(lvl.Parent, out var parentLevel))
            {
                throw new KeyNotFoundException($"Parent level '{lvl.Parent}' not found.");
            }
            lvl = parentLevel;
        }
        return allKeys;
    }

    public List<string> GetKeysRecursiveSorted()
    {
        ValidateCurrentLevel();
        return GetKeysRecursiveSorted(_currentLevelKey!);
    }

    public List<string> GetKeysRecursiveSorted(string levelKey)
    {
        var allKeys = GetKeysRecursive(levelKey);
        allKeys.Sort(OIC);
        return allKeys;
    }

    #endregion

    #region GetItems methods

    public List<Item> GetItems()
    {
        ValidateCurrentLevel();
        return _currentLevel!.Items;
    }

    public List<Item> GetItems(string levelKey)
    {
        ValidateLevelKey(levelKey);
        if (!_levels.TryGetValue(levelKey, out var lvl))
        {
            throw new KeyNotFoundException($"Level '{levelKey}' not found.");
        }
        return lvl.Items;
    }

    public List<Item> GetItemsSorted(string levelKey)
    {
        ValidateLevelKey(levelKey);
        if (!_levels.TryGetValue(levelKey, out var lvl))
        {
            throw new KeyNotFoundException($"Level '{levelKey}' not found.");
        }
        var keys = lvl.SortedKeys;
        keys.Sort(OIC);
        List<Item> sortedItems = [];
        foreach (var key in keys)
        {
            var item = lvl.GetItem(key);
            if (item != null)
            {
                sortedItems.Add(item);
            }
        }
        return sortedItems;
    }

    public List<Item> GetItemsRecursive(string levelKey)
    {
        ValidateLevelKey(levelKey);
        if (!_levels.TryGetValue(levelKey, out var lvl))
        {
            throw new KeyNotFoundException($"Level '{levelKey}' not found.");
        }
        var keys = GetKeysRecursive(levelKey);
        List<Item> allItems = [];
        foreach (var key in keys)
        {
            var item = lvl!.GetItem(key);
            if (item != null)
            {
                allItems.Add(item);
                continue;
            }
            // If item is not found in the current level, check parent levels
            while (lvl != null && lvl.Parent != null)
            {
                if (!_levels.TryGetValue(lvl.Parent, out var parentLevel))
                {
                    throw new KeyNotFoundException($"Parent level '{lvl.Parent}' not found.");
                }
                item = parentLevel.GetItem(key);
                if (item != null)
                {
                    allItems.Add(item);
                    break;
                }
                lvl = parentLevel;
            }
        }
        return allItems;
    }

    public List<Item> GetItemsRecursiveSorted(string levelKey)
    {
        ValidateLevelKey(levelKey);
        if (!_levels.TryGetValue(levelKey, out var lvl))
        {
            throw new KeyNotFoundException($"Level '{levelKey}' not found.");
        }
        var keys = GetKeysRecursiveSorted(levelKey);
        List<Item> allItems = [];
        foreach (var key in keys)
        {
            var item = lvl.GetItem(key);
            if (item != null)
            {
                allItems.Add(item);
            }
        }
        return allItems;
    }

    #endregion

    #region private methods

    private static void ValidateLevelKey(string levelKey)
    {
        if (string.IsNullOrWhiteSpace(levelKey))
        {
            throw new ArgumentNullException(nameof(levelKey), "Level key cannot be null or whitespace.");
        }
    }

    private void ValidateCurrentLevel()
    {
        if (string.IsNullOrWhiteSpace(_currentLevelKey) || _currentLevel == null)
        {
            throw new InvalidOperationException("Current level is not set.");
        }
    }

    #endregion
}
