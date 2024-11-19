using System.Collections;
using System.Diagnostics.CodeAnalysis;

namespace fennecs.future;

/// <summary>
/// A dictionary backed by a pair of lists, for fast enumeration.
/// </summary>
/// <remarks>
/// o(1) for addition, lookup, indexing, and removal.
/// Linear keys and values enumeration.
/// </remarks>
public sealed class HashList<K, V>(int capacity = 4) : IReadOnlyList<V>, IDictionary<K, V>
    where K : notnull
{
    private readonly Dictionary<K, int> _indices = new(capacity);
    private readonly List<K> _keys = new(capacity);
    private readonly List<V> _values = new(capacity);

    /// <inheritdoc />
    public void Add(K key, V value)
    {
        _indices.Add(key, _values.Count);
        _keys.Add(key);
        _values.Add(value);
    }

    /// <inheritdoc cref="Dictionary{TKey,TValue}.Add" />
    public void Add(KeyValuePair<K, V> item)
    {
        _indices.Add(item.Key, _values.Count);
        _keys.Add(item.Key);
        _values.Add(item.Value);
    }

    /// <inheritdoc cref="Dictionary{TKey,TValue}.Clear" />
    public void Clear()
    {
        _indices.Clear();
        _values.Clear();
    }

    /// <inheritdoc cref="ICollection{T}.Contains" />
    public bool Contains(KeyValuePair<K, V> item) => _indices.ContainsKey(item.Key);

    /// <inheritdoc />
    public void CopyTo(KeyValuePair<K, V>[] array, int arrayIndex)
    {
        if (arrayIndex < 0 || arrayIndex > array.Length) throw new ArgumentOutOfRangeException(nameof(arrayIndex));
        if (array.Length - arrayIndex < _values.Count) throw new ArgumentException("Not enough space in the array");

        for (var i = 0; i < _values.Count; i++)
        {
            array[arrayIndex + i] = new KeyValuePair<K, V>(_keys[i], _values[i]);
        }
    }

    /// <inheritdoc />
    public bool Remove(KeyValuePair<K, V> item)
    {
        return Remove(item.Key);
    }

    /// <inheritdoc />
    public bool ContainsKey(K key) => _indices.ContainsKey(key);

    /// <inheritdoc />
    public bool Remove(K key)
    {
        if (!_indices.Remove(key, out var removed)) return false;

        var last = _values.Count - 1;
        if (last != removed)
        {
            // Copy the last value into the removed slot
            _values[removed] = _values[last];
            _indices[_keys[last]] = removed;
            _keys[removed] = _keys[last];
        }

        _values.RemoveAt(last);
        _keys.RemoveAt(last);
        return true;
    }

    /// <inheritdoc />
    public bool TryGetValue(K key, [MaybeNullWhen(false)] out V value)
    {
        if (_indices.TryGetValue(key, out var index))
        {
            value = _values[index];
            return true;
        }

        value = default;
        return false;
    }

    /// <inheritdoc />
    public V this[K key]
    {
        get => _values[_indices[key]];
        set
        {
            if (_indices.TryGetValue(key, out var index))
            {
                _values[index] = value;
            }
            else
            {
                Add(key, value);
            }
        }
    }

    #region Indexers and Values

    /// <inheritdoc />
    public V this[int index] => _values[index];

    /// <inheritdoc cref="Dictionary{TKey,TValue}.Keys" />
    public ICollection<K> Keys => _keys;

    /// <inheritdoc cref="Dictionary{TKey,TValue}.Values" />
    public ICollection<V> Values => _values;

    /// <inheritdoc cref="ICollection{T}.Count" />
    public int Count => _values.Count;

    /// <inheritdoc />
    public bool IsReadOnly => false;

    #endregion

    #region IEnumerable

    IEnumerator<KeyValuePair<K, V>> IEnumerable<KeyValuePair<K, V>>.GetEnumerator()
    {
        // ReSharper disable once LoopCanBeConvertedToQuery
        for (var i = 0; i < _values.Count; i++) yield return new KeyValuePair<K, V>(_keys[i], _values[i]);
    }

    IEnumerator<V> IEnumerable<V>.GetEnumerator() => _values.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => _values.GetEnumerator();

    #endregion
}