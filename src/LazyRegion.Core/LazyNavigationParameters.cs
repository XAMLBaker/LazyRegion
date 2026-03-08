using System.Collections.Generic;

namespace LazyRegion.Core;

public sealed class LazyNavigationParameters
{
    private readonly Dictionary<string, object> _store = new();

    public void Add(string key, object value) => _store[key] = value;

    public T GetValue<T>(string key) => (T)_store[key];

    public T GetValueOrDefault<T>(string key, T defaultValue = default)
        => _store.TryGetValue(key, out var v) && v is T t ? t : defaultValue;

    public bool TryGetValue<T>(string key, out T value)
    {
        if (_store.TryGetValue(key, out var raw) && raw is T typed)
        {
            value = typed;
            return true;
        }
        value = default;
        return false;
    }

    public bool ContainsKey(string key) => _store.ContainsKey(key);

    public int Count => _store.Count;
}
