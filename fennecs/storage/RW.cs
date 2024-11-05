using fennecs.events;
using fennecs.pools;

namespace fennecs.storage;

/// <summary>
/// Read-write access to a component.
/// </summary>
public ref struct RW<T> where T : notnull
{
    internal RW(ref T val,
        ref Entity entity,
        ref PooledList<Entity> writtenEntities /*, //TODO: remove defaults
        PooledList<T>? writtenOriginals = null,
        PooledList<T>? writtenUpdates = null*/)
    {
        _writtenEntities = ref writtenEntities;
        //_writtenUpdates = writtenUpdates;
        //_writtenOriginals = writtenOriginals;
        
        _value = ref val;
        _entity = ref entity;
    }

    internal ref Entity _entity;
    internal ref T _value;

    private readonly ref PooledList<Entity> _writtenEntities;
    //private readonly PooledList<T>? _writtenUpdates;
    //private readonly PooledList<T>? _writtenOriginals;

    /// <summary>
    /// Read access to the component's value.
    /// </summary>
    public T read => _value;
    
    /// <summary>
    /// Write access to the component's value.
    /// </summary>
    public T write
    {
        get => _value;
        set
        {
            // Optimizes away the write and null checks if it's not modifiable.
            if (typeof(T).IsAssignableFrom(typeof(Modified<T>)))
            {
                var original = _value;
                _value = value;

                // TODO: Collect changes up into the Runner's outer scope instead, and process all at once there.
                _writtenEntities?.Add(_entity);
                //_writtenOriginals?.Add(original);
                //_writtenUpdates?.Add(value);
                
                // TODO: Handle this in the outer scope, where the lists come from.
                Modified<T>.Invoke([_entity], [original], [value]);
            }
            else
            {
                _value = value;
            }
        }
    }
    
    /// <summary>
    /// Reads the value (creating a shallow copy) and removes the component from the entity.
    /// </summary>
    /// <remarks>
    /// <para>When executing this in a Stream's Runner, the component will be removed later, in a deferred operation.</para>
    /// <para>This means it could be "consumed" repeatedly, and a runtime error will occur during World Catchup (usually at the end of the Runner's scope).</para>
    /// <para>Even though this is a structural change, it is still considered a "read" operation!</para>
    /// </remarks>
    /// <returns>the component value</returns>
    public T consume
    {
        get        
        {
            // ReSharper disable once SuggestVarOrType_SimpleTypes
            T copy = _value;
            // Remove<T> usually moves another entity into the slot of the removed one
            _entity.Remove<T>(); 
            return copy;
        }
    }
}