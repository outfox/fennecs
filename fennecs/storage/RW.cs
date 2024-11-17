using System.Runtime.CompilerServices;
using fennecs.events;

namespace fennecs.storage;

/// <summary>
/// Read-write access to a component.
/// </summary>
// ReSharper disable once InconsistentNaming
public readonly ref struct RW<T> : IEquatable<RW<T>>, IEquatable<T> where T : notnull
{
    private readonly ref readonly Entity _entity;
    private readonly ref readonly TypeExpression _expression;

    private readonly ref T _value;

    private readonly ref bool _modified;
    
    /// <summary>
    /// Read-write access to a component.
    /// </summary>
    internal RW(ref T value, ref bool modified, ref readonly Entity entity, ref readonly TypeExpression expression)
    {
        _modified = ref modified;
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
    // ReSharper disable once InconsistentNaming
    public ref readonly T read => ref _value;
    
    
    /// <summary>
    /// Write access to the component's value.
    /// </summary>
    // ReSharper disable once InconsistentNaming
    public ref T write
    {
        get
        {
            // JIT Optimizes away the write and type checks if it's not a modifiable type.
            if (typeof(Modified<T>).IsAssignableFrom(typeof(T))) _modified = true;
            return ref _value;
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
    // ReSharper disable once InconsistentNaming
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
    public static implicit operator T(RW<T> self) => self.read;

    /// <summary>
    /// You found the cursed operator! It's a secret, and it's stroustrup.
    /// </summary>
    public static RW<T> operator <<(RW<T> self, T other)
    {
        self._value = other;
        return self;
    }
    
    /// <inheritdoc />
    public override string ToString() => $"RW<{typeof(T)}>({_value.ToString()})";
    
    /// <inheritdoc />
    public bool Equals(RW<T> other) => _value.Equals(other._value);

    /// <inheritdoc />
    public bool Equals(T? other) => _value.Equals(other);
}