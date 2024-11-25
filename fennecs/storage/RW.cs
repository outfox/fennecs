using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using fennecs.events;

namespace fennecs.storage;

/// <summary>
/// Read-write access to a component.
/// </summary>
// ReSharper disable once InconsistentNaming
public readonly ref struct RW<T> : IEquatable<RW<T>>, IEquatable<T> where T : notnull 
{
    internal readonly ref T Value;
    
    private readonly ref readonly Entity _entity;
    private readonly ref readonly TypeExpression _expression;

    /// <summary>
    /// Read-write access to a component.
    /// </summary>
    internal RW(ref T value, ref readonly TypeExpression expression, ref readonly Entity entity)
    {
        _entity = ref entity;
        _expression = ref expression;
        Value = ref value;
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
    public ref readonly T read
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => ref Value;
    }


    /// <summary>
    /// Write access to the component's value.
    /// </summary>
    // ReSharper disable once InconsistentNaming
    public ref T write
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => ref Value;
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
            T copy = Value;
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

    #region Equality Operators

    /// <summary>
    /// Equality comparison (Value)
    /// </summary>
    public static bool operator ==(RW<T> self, T otherValue) => self.Value.Equals(otherValue);

    /// <summary>
    /// Inequality comparison (Value)
    /// </summary>
    public static bool operator !=(RW<T> self, T otherValue) => !(self == otherValue);
    
    /// <summary>
    /// Equality comparison (Component)
    /// </summary>
    public static bool operator ==(RW<T> self, RW<T> other) => self.Equals(other);

    /// <inheritdoc cref="Equals(T)"/>
    public static bool operator !=(RW<T> self, RW<T> other) => !(self == other);

    /// <inheritdoc cref="Equals(T)"/>
    public static bool operator ==(RW<T> self, R<T> other) => self.Value.Equals(other.read);

    /// <inheritdoc cref="Equals(T)"/>
    public static bool operator !=(RW<T> self, R<T> other) => !other.Equals(self);

    #endregion

    #region IEquatable
    /// <inheritdoc />
    public bool Equals(RW<T> other) => Expression == other.Expression;

    /// <summary>
    /// Equality comparison comparing the value of the <see cref="RW{T}"/> to the given value.
    /// </summary>
    public bool Equals(T? otherValue) => Value.Equals(otherValue);

    /// <inheritdoc />
    public override bool Equals([NotNullWhen(true)] object? obj) 
    {
        return obj != null && Value.Equals(obj);
    }
    #endregion
    
    /// <inheritdoc />
    public override int GetHashCode() => Value.GetHashCode();
    
    /// <inheritdoc />
    public override string ToString() => $"RW<{typeof(T)}>({Value.ToString()})";
    
    /// <summary>
    /// You found the cursed operator! It's a secret, between you, Bjarne Stroustrup, and the gods.
    /// </summary>
    public static RW<T> operator <<(RW<T> self, T otherValue)
    {
        self.Value = otherValue;
        return self;
    }
}