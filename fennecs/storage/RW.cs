﻿using fennecs.events;

namespace fennecs.storage;

/// <summary>
/// Read-write access to a component.
/// </summary>
public readonly ref struct RW<T> where T : notnull
{
    private readonly ref readonly Entity _entity;
    private readonly ref readonly TypeExpression _expression;

    private readonly ref T _value;

    /// <summary>
    /// Read-write access to a component.
    /// </summary>
    internal RW(ref T value, ref readonly Entity entity, ref readonly TypeExpression expression)
    {
        _entity = ref entity;
        _expression = ref expression;
        _value = ref value;
    }

    // TODO: Expose it publicly once TypeExpression is exposed.
    internal TypeExpression Expression => _expression;

    /// <summary>
    /// The <see cref="Match"/> expression of this component.
    /// </summary>
    public Match Match => _expression.Match;

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
            if (typeof(Modified<T>).IsAssignableFrom(typeof(T)))
            {
                var original = _value;
                _value = value;

                // TODO: Collect changes up into the Runner's outer scope instead, and process all at once there.
                //_writtenEntities?.Add(_entity);
                //_writtenOriginals?.Add(original);
                //_writtenUpdates?.Add(value);

                Modified<T>.Clear();
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
            // Remove<T> usually moves another entity into the slot of the removed one in immediate mode
            // The structural change is so expensive that it's not worth optimizing this getter further.
            _entity.Remove<T>(_expression.Match); 
            return copy;
        }
    }
    
    /// <summary>
    /// Removes the component from the entity.
    /// </summary>
    public void Remove()
    {
        _entity.Remove<T>(_expression.Match);
    }
    
    /// <summary>
    /// Implicitly casts a <see cref="RW{T}"/> to its underlying value.
    /// </summary>
    public static implicit operator T(RW<T> self) => self._value;
}