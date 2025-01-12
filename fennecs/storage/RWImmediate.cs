using fennecs.events;

namespace fennecs.storage;

/// <summary>
/// Read-write access to a component.
/// </summary>
/// <remarks>
/// This is a specialized version of <see cref="RW{T}"/> to meet the needs of obtaining a reference to a component, <see cref="Entity.Ref{C}(Key)"/>
/// </remarks>
public readonly ref struct RWImmediate<T>(ref T value, Entity entity, Key key) : IEquatable<T> where T : notnull
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
            if (value is IModified<T> modifiable)
            {
                var original = _value;
                
                // TODO: This can't compile prior to restructuring the events system.
                //modifiable.Notify([entity], [value], [original]);
            }
            _value = value;
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
    /// <inheritdoc cref="Entity.Remove{C}(Match,string,int)"/>
    public void Remove() => entity.Remove<T>(key);

    /// <inheritdoc cref="IEquatable{T}" />
    public static bool operator ==(T other, RWImmediate<T> self) => self._value.Equals(other);

    /// <inheritdoc cref="IEquatable{T}" />
    public static bool operator !=(T other, RWImmediate<T> self) => !(other == self);

    /// <inheritdoc cref="IEquatable{T}" />
    public static bool operator ==(RWImmediate<T> self, T other) => self._value.Equals(other);

    /// <inheritdoc cref="IEquatable{T}" />
    public static bool operator !=(RWImmediate<T> self, T other) => !(self == other);

    /// <inheritdoc />
    public bool Equals(T? other) => other is not null && _value.Equals(other);
    
    /// <inheritdoc />
    public override bool Equals(object? obj) => _value.Equals(obj);
    
    /// <inheritdoc />
    public override int GetHashCode() => _value.GetHashCode();
}