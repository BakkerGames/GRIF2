namespace GROD2;

public class Grod2
{
    private const string Version = "2.2025.0812";

    private readonly StringComparer OIC = StringComparer.OrdinalIgnoreCase;

    private readonly Dictionary<string, Level> _levels = new(StringComparer.OrdinalIgnoreCase);

    public Grod2()
    {
    }

    public static string GetVersion()
    {
        return Version;
    }

    public List<string> GetLevelKeys()
    {
        return [.. _levels.Keys];
    }

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

    public string? GetLevelParent(string levelKey)
    {
        ValidateLevelKey(levelKey);
        if (!_levels.TryGetValue(levelKey, out var level))
        {
            throw new KeyNotFoundException($"Level '{levelKey}' not found.");
        }
        return level.Parent;
    }

    public void SetLevelParent(string levelKey, string? parentLevelKey)
    {
        ValidateLevelKey(levelKey);
        if (string.IsNullOrWhiteSpace(parentLevelKey))
        {
            parentLevelKey = null; // Allow setting parent to null
        }
        else
        {
            ValidateLevelKey(parentLevelKey);
            if (!_levels.ContainsKey(parentLevelKey))
            {
                throw new KeyNotFoundException($"Parent level '{parentLevelKey}' not found.");
            }
        }
        if (!_levels.TryGetValue(levelKey, out var level))
        {
            throw new KeyNotFoundException($"Level '{levelKey}' not found.");
        }
        level.Parent = parentLevelKey;
    }

    public void RemoveLevel(string levelKey)
    {
        ValidateLevelKey(levelKey);
        foreach (var level in _levels.Values)
        {
            if (level.Parent == levelKey)
            {
                throw new InvalidOperationException($"Cannot remove level '{levelKey}' because it is a parent of another level.");
            }
        }
        if (!_levels.Remove(levelKey))
        {
            throw new KeyNotFoundException($"Level '{levelKey}' not found.");
        }
    }

    public void ClearLevel(string levelKey)
    {
        ValidateLevelKey(levelKey);
        if (!_levels.TryGetValue(levelKey, out var lvl))
        {
            throw new KeyNotFoundException($"Level '{levelKey}' not found.");
        }
        lvl.Clear();
    }

    public string? Get(string levelKey, string key)
    {
        ValidateLevelKey(levelKey);
        if (!_levels.TryGetValue(levelKey, out var lvl))
        {
            throw new KeyNotFoundException($"Level '{levelKey}' not found.");
        }
        return lvl.Get(key);
    }

    public string? GetRecursive(string levelKey, string key)
    {
        ValidateLevelKey(levelKey);
        if (!_levels.TryGetValue(levelKey, out var lvl))
        {
            throw new KeyNotFoundException($"Level '{levelKey}' not found.");
        }
        return GetRecursive(lvl, key);
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

    public void Remove(string levelKey, string key)
    {
        ValidateLevelKey(levelKey);
        if (!_levels.TryGetValue(levelKey, out var lvl))
        {
            throw new KeyNotFoundException($"Level '{levelKey}' not found.");
        }
        lvl.Remove(key);
    }

    public List<string> GetKeys(string levelKey)
    {
        ValidateLevelKey(levelKey);
        if (!_levels.TryGetValue(levelKey, out var lvl))
        {
            throw new KeyNotFoundException($"Level '{levelKey}' not found.");
        }
        var keys = lvl.Keys;
        keys.Sort(OIC);
        return keys;
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
        allKeys.Sort(OIC);
        return allKeys;
    }

    public List<Item> GetItems(string levelKey)
    {
        ValidateLevelKey(levelKey);
        if (!_levels.TryGetValue(levelKey, out var lvl))
        {
            throw new KeyNotFoundException($"Level '{levelKey}' not found.");
        }
        var keys = lvl.Keys;
        keys.Sort(OIC);
        List<Item> sortedItems = [];
        foreach (var key in keys)
        {
            var value = lvl.Get(key);
            if (value != null)
            {
                sortedItems.Add(new Item { Key = key, Text = value });
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
        keys.Sort(OIC);
        List<Item> allItems = [];
        foreach (var key in keys)
        {
            var value = GetRecursive(lvl, key);
            if (value != null)
            {
                allItems.Add(new Item { Key = key, Text = value });
            }
        }
        return allItems;
    }

    public void AddRange(string levelKey, List<Item> items)
    {
        ValidateLevelKey(levelKey);
        if (!_levels.TryGetValue(levelKey, out var lvl))
        {
            throw new KeyNotFoundException($"Level '{levelKey}' not found.");
        }
        lvl.AddRange(items);
    }

    #region private methods

    private static void ValidateLevelKey(string levelKey)
    {
        if (string.IsNullOrWhiteSpace(levelKey))
        {
            throw new ArgumentNullException(nameof(levelKey), "Level key cannot be null or whitespace.");
        }
    }

    private string? GetRecursive(Level lvl, string key)
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
}
