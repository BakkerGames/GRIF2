namespace Grif;

public class Grod
{
    public const string Version = "2.2025.1024";

    private const string NULL = "null";
    private const string TRUE = "true";
    private const string FALSE = "false";

    private readonly Dictionary<string, string?> _data = new(StringComparer.OrdinalIgnoreCase);

    public Grod()
    {
    }

    public Grod(string name)
    {
        Name = name;
    }

    public Grod(string name, Grod? parent)
    {
        Name = name;
        Parent = parent;
    }

    public string Name { get; set; } = string.Empty;

    public Grod? Parent { get; set; }

    public int Count(bool recursive)
    {
        var keys = Keys(recursive, false);
        return keys.Count;
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

    public int? GetInt(string key, bool recursive)
    {
        var value = Get(key, recursive);
        if (value == null)
        {
            return null;
        }
        if (int.TryParse(value, out int intValue))
        {
            return intValue;
        }
        throw new FormatException($"Value for key '{key}' is not a valid integer.");
    }

    public bool? GetBool(string key, bool recursive)
    {
        var value = Get(key, recursive);
        if (value == null)
        {
            return null;
        }
        if (bool.TryParse(value, out bool boolValue))
        {
            return boolValue;
        }
        switch (value.ToLowerInvariant())
        {
            case "y":
            case "yes":
            case "t":
            case "1":
            case "-1":
                return true;
            case "n":
            case "no":
            case "f":
            case "0":
            case "":
                return false;
            default:
                break;
        }
        throw new FormatException($"Value for key '{key}' is not a valid boolean.");
    }

    public void Set(string key, string? value)
    {
        ValidateKey(ref key);
        if (value != null && value.Equals(NULL, StringComparison.OrdinalIgnoreCase))
        {
            value = null;
        }
        if (!_data.TryAdd(key, value))
        {
            _data[key] = value;
        }
    }

    public void Set(string key, int value)
    {
        Set(key, value.ToString());
    }

    public void Set(string key, bool value)
    {
        Set(key, value ? TRUE : FALSE);
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
            var parentKeys = Parent.Keys(recursive, false);
            keys = [.. keys.Union(parentKeys, StringComparer.OrdinalIgnoreCase)];
        }
        if (sorted)
        {
            keys.Sort(CompareKeys);
        }
        return keys;
    }

    public List<string> Keys(string prefix, bool recursive, bool sorted)
    {
        var keys = new List<string>(_data.Keys)
            .Where(x => x.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            .ToList();
        if (recursive && Parent != null)
        {
            var parentKeys = Parent.Keys(recursive, false)
                .Where(x => x.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                .ToList();
            keys = [.. keys.Union(parentKeys, StringComparer.OrdinalIgnoreCase)];
        }
        if (sorted)
        {
            keys.Sort(CompareKeys);
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

    public List<GrodItem> Items(string prefix, bool recursive, bool sorted)
    {
        var keys = Keys(prefix, recursive, sorted);
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

    private static int CompareKeys(string x, string y)
    {
        if (x == null)
        {
            if (y == null) return 0;
            return -1;
        }
        if (y == null)
        {
            return 1;
        }
        if (x.Equals(y, StringComparison.OrdinalIgnoreCase)) return 0;
        var xTokens = x.Split('.');
        var yTokens = y.Split('.');
        for (int i = 0; i < Math.Max(xTokens.Length, yTokens.Length); i++)
        {
            if (i >= xTokens.Length) return -1; // x is shorter and earlier
            if (i >= yTokens.Length) return 1; // y is shorter and earlier
            if (xTokens[i].Equals(yTokens[i], StringComparison.OrdinalIgnoreCase)) continue;
            if (xTokens[i] == "*") return -1; // "*" comes first so x is earlier
            if (yTokens[i] == "*") return 1; // "*" comes first so y is earlier
            if (xTokens[i] == "?") return -1; // "?" comes next so x is earlier
            if (yTokens[i] == "?") return 1; // "?" comes next so y is earlier
            if (xTokens[i] == "#") return -1; // "#" comes next so x is earlier
            if (yTokens[i] == "#") return 1; // "#" comes next so y is earlier
            if (int.TryParse(xTokens[i], out int xVal) && int.TryParse(yTokens[i], out int yVal))
            {
                if (xVal == yVal) continue;
                return xVal < yVal ? -1 : 1;
            }
            return string.Compare(xTokens[i], yTokens[i], StringComparison.OrdinalIgnoreCase);
        }
        return 0;
    }

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
