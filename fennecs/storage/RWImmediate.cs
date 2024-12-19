using fennecs.events;

namespace fennecs.storage;

/// <summary>
/// Read-write access to a component.
/// </summary>
/// <remarks>
/// This is a specialized version of <see cref="RW{T}"/> to meet the needs of obtaining a reference to a component, <see cref="Entity.Ref{C}(Key)"/>
/// </remarks>
public readonly ref struct RWImmediate<T>(ref T value, Entity entity, Key key) where T : notnull
{
    private readonly ref T _value = ref value;

    /// <summary>
    /// Read access to the component's value.
    /// </summary>
    public T Read => _value;
    
    /// <summary>
    /// Write access to the component's value.
    /// </summary>
    public T Write
    {
        get => _value; // Included for +=
        set
        {
            // Optimizes away the write and null checks if it's not modifiable.
            if (typeof(IModified<T>).IsAssignableFrom(typeof(T)))
            {
                var original = _value;
                _value = value;

                // TODO: Collect changes up into the Runner's outer scope instead, and process all at once there.
                //_writtenEntities?.Add(_entity);
                //_writtenOriginals?.Add(original);
                //_writtenUpdates?.Add(value);
                
                // TODO: Handle this in the outer scope, where the lists come from.
                IModified<T>.Invoke([entity], [original], [value]);
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
    public T Consume
    {
        get        
        {
            // ReSharper disable once SuggestVarOrType_SimpleTypes
            T copy = _value;
            // Remove<T> usually moves another entity into the slot of the removed one in immediate mode
            // The structural change is so expensive that it's not worth optimizing this getter further.
            entity.Remove<T>(key); 
            return copy;
        }
    }

    /// <summary>
    /// Removes the component from the entity.
    /// </summary>
    /// <inheritdoc cref="Entity.Remove{C}(fennecs.Key)"/>
    public void Remove() => entity.Remove<T>(key);
}