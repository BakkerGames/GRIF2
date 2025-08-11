namespace GROD2;

public class Data
{
    private readonly Dictionary<string, string> _data = new(StringComparer.OrdinalIgnoreCase);

    public Data()
    {
    }

    public string? Get(string key)
    {
        ValidateKey(key);
        if (_data.TryGetValue(key, out var item))
        {
            return item;
        }
        return null;
    }

    public void Set(string key, string? text)
    {
        ValidateKey(key);
        if (text == null)
        {
            _data.Remove(key);
            return;
        }
        if (!_data.TryAdd(key, text))
        {
            _data[key] = text;
        }
    }

    public void Remove(string key)
    {
        ValidateKey(key);
        _data.Remove(key);
    }

    public void Clear()
    {
        _data.Clear();
    }

    public List<string> Keys
    {
        get
        {
            return [.. _data.Keys];
        }
    }

    public List<Item> Items
    {
        get
        {
            return [.. _data.Select(kvp => new Item { Key = kvp.Key, Text = kvp.Value })];
        }
    }

    public void AddRange(IEnumerable<Item> items)
    {
        if (items == null)
        {
            throw new ArgumentNullException(nameof(items), "Items cannot be null.");
        }
        foreach (var item in items)
        {
            Set(item.Key, item.Text);
        }
    }

    #region private methods

    private static void ValidateKey(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            throw new ArgumentNullException(nameof(key), "Key cannot be null or whitespace.");
        }
    }

    #endregion
}
