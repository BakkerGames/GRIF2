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

    public Item? GetItem(string key)
    {
        ValidateKey(key);
        if (_data.TryGetValue(key, out var text))
        {
            return new Item { Key = key, Text = text };
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

    public void Set(Item item)
    {
        if (item == null)
        {
            throw new ArgumentNullException(nameof(item), "Item cannot be null.");
        }
        Set(item.Key, item.Text);
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

    public IEnumerable<string> Keys => [.. _data.Keys];

    public List<string> SortedKeys
    {
        get
        {
            var keys = _data.Keys.ToList();
            keys.Sort(StringComparer.OrdinalIgnoreCase);
            return keys;
        }
    }

    public List<Item> Items
    {
        get
        {
            return [.. _data.Select(kvp => new Item { Key = kvp.Key, Text = kvp.Value })];
        }
    }

    private static void ValidateKey(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            throw new ArgumentNullException(nameof(key), "Key cannot be null or whitespace.");
        }
    }
}
