namespace fennecs.pools;

/// <summary>
/// Storage for the targets of ObjectLinks
/// </summary>
/// <param name="capacity">initial capacity (count) of references dictionary</param>
internal class ReferenceStore(int capacity = 4096)
{
    private readonly Dictionary<Key, StoredReference<object>> _storage = new(capacity);

    public Key Request<T>(T item) where T : class
    {
        var key = Key.Of(item);

        lock (_storage)
        {
            // Already tracking this item.
            if (_storage.TryGetValue(key, out var reference))
            {
                //TODO: Consider replacing exception with assert.
                if (reference.Item != item)
                {
                    //TODO: Maybe disable the exception handling here, gives better inlining performance.
                    throw new InvalidOperationException($"GetHashCode() collision in {typeof(T)}, causing Key collision between {item} and {reference.Item} in {reference}.");
                }

                reference.Count++;
                _storage[key] = reference;
                return key;
            }

            // First time tracking this item.
            reference = new StoredReference<object>
            {
                Item = item,
                Count = 1,
            };

            _storage[key] = reference;
            return key;
        }
    }


    public T Get<T>(Key key) where T : class
    {
        lock (_storage)
        {
            if (!_storage.TryGetValue(key, out var reference))
            {
                throw new KeyNotFoundException($"Key is not tracking an instance of {typeof(T)}.");
            }

            return (T) reference.Item;
        }
    }


    public void Release(Key key)
    {
        lock (_storage)
        {
            if (_storage.TryGetValue(key, out var reference))
            {
                reference.Count--;
                if (reference.Count == 0)
                {
                    _storage.Remove(key);
                }
                else
                {
                    _storage[key] = reference;
                }
            }
            else
            {
                throw new KeyNotFoundException($"Key {key} is not tracked.");
            }
        }
    }


    internal struct StoredReference<T>
    {
        public required T Item;
        public required int Count;
        public override string ToString() => $"{Item} x{Count}";
    }
}
