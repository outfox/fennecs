using fennecs.events;

namespace fennecs.storage;

internal readonly ref struct RW<T>(ref T val, ref Entity entity) where T : notnull
{
    private readonly ref Entity _entity = ref entity;
    private readonly ref T _value = ref val;
    
    public T read => _value;
    public T write
    {
        get => _value;
        set
        {
            if (typeof(T).IsAssignableFrom(typeof(Modified<T>)))
            {
                var original = _value;
                _value = value;
                
                // TODO: Pass this up into the Runner's outer scope instead, and process all at once there.
                Modified<T>.Invoke([_entity], [original], [value]);
            }
            else
            {
                _value = value;
            }
        }
    }
    
    public T Consume()
    {
        _entity.Remove<T>();
        return _value;
    }
}