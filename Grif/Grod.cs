namespace Grif;

public record GrodItem(string Key, string? Value);

public class Grod
{
    private const string _version = "2.2025.0824";

    private readonly Dictionary<string, string?> _data = new(StringComparer.OrdinalIgnoreCase);

    public Grod(string name)
    {
        Name = name;
    }

    public Grod(string name, Grod? parent)
    {
        Name = name;
        Parent = parent;
    }

    public static string Version => _version;

    public string Name { get; set; }

    public Grod? Parent { get; set; }

    public int Count(bool recursive)
    {
        if (recursive && Parent != null)
        {
            return _data.Count + Parent.Count(recursive);
        }
        return _data.Count;
    }

    public string? Get(string key, bool recursive)
    {
        ValidateKey(ref key);
        if (_data.TryGetValue(key, out var value))
        {
            return value;
        }
        if (recursive && Parent != null)
        {
            return Parent.Get(key, recursive);
        }
        return null;
    }

    public void Set(string key, string? value)
    {
        ValidateKey(ref key);
        if (!_data.TryAdd(key, value))
        {
            _data[key] = value;
        }
    }

    public void Set(GrodItem item)
    {
        ArgumentNullException.ThrowIfNull(item);
        Set(item.Key, item.Value);
    }

    public void Remove(string key, bool recursive)
    {
        ValidateKey(ref key);
        _data.Remove(key);
        if (recursive && Parent != null)
        {
            Parent.Remove(key, recursive);
        }
    }

    public void Clear(bool recursive)
    {
        _data.Clear();
        if (recursive && Parent != null)
        {
            Parent.Clear(recursive);
        }
    }

    public bool ContainsKey(string key, bool recursive)
    {
        ValidateKey(ref key);
        if (_data.ContainsKey(key))
        {
            return true;
        }
        if (recursive && Parent != null)
        {
            return Parent.ContainsKey(key, recursive);
        }
        return false;
    }

    public List<string> Keys(bool recursive, bool sorted)
    {
        var keys = new List<string>(_data.Keys);
        if (recursive && Parent != null)
        {
            var parentKeys = Parent.Keys(recursive, sorted);
            foreach (var parentKey in parentKeys)
            {
                if (!keys.Contains(parentKey, StringComparer.OrdinalIgnoreCase))
                {
                    keys.Add(parentKey);
                }
            }
        }
        if (sorted)
        {
            keys.Sort(StringComparer.OrdinalIgnoreCase);
        }
        return keys;
    }

    public List<string> Keys(string prefix, bool recursive, bool sorted)
    {
        var keys = new List<string>(_data.Keys).Where(x => x.StartsWith(prefix)).ToList();
        if (recursive && Parent != null)
        {
            var parentKeys = Parent.Keys(recursive, sorted).Where(x => x.StartsWith(prefix)).ToList();
            foreach (var parentKey in parentKeys)
            {
                if (!keys.Contains(parentKey, StringComparer.OrdinalIgnoreCase))
                {
                    keys.Add(parentKey);
                }
            }
        }
        return keys;
    }

    public List<GrodItem> Items(bool recursive, bool sorted)
    {
        var keys = Keys(recursive, sorted);
        List<GrodItem> items = [];
        foreach (var key in keys)
        {
            var value = Get(key, recursive);
            items.Add(new GrodItem(key, value));
        }
        return items;
    }

    public void AddItems(IEnumerable<GrodItem> items)
    {
        if (items != null)
        {
            foreach (var item in items)
            {
                Set(item);
            }
        }
    }

    #region private methods

    private static void ValidateKey(ref string key)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            throw new ArgumentException("Key cannot be null or whitespace.", nameof(key));
        }
        key = key.Trim();
    }

    #endregion
}
