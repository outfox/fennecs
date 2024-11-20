using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace fennecs.storage;

/// <summary>
/// Read-only access to a component.
/// </summary>

public readonly ref struct R<T> : IEquatable<R<T>>, IEquatable<T> where T : notnull
{
    /// <summary>
    /// Read-only access to component's value.
    /// </summary>
    public readonly ref readonly T read;

    //private readonly ref readonly Entity _entity;
    //private readonly ref readonly TypeExpression _expression;

    /// <summary>
    /// Read-only access to a component.
    /// </summary>
    internal R(ref readonly T read)
    {
        this.read = ref read;
        //_entity = ref entity;
        //_expression = ref expression;
    }

    //internal TypeExpression Expression => _expression;

    /// <summary>
    /// Implicitly casts a <see cref="R{T}"/> to its underlying value.
    /// </summary>
    public static implicit operator T(R<T> self) => self.read;

    
    #region Equality Operators

    /// <inheritdoc cref="Equals(T)"/>
    public static bool operator ==(R<T> self, T other) => self.read.Equals(other);

    /// <inheritdoc cref="Equals(T)"/>
    public static bool operator !=(R<T> self, T other) => !(self == other);

    /// <inheritdoc cref="Equals(T)"/>
    public static bool operator ==(R<T> self, R<T> other) => self.Equals(other);

    /// <inheritdoc cref="Equals(T)"/>
    public static bool operator !=(R<T> self, R<T> other) => !(self == other);

    
    /// <inheritdoc cref="Equals(T)"/>
    public static bool operator ==(R<T> other, RW<T> self) => self.Value.Equals(other.read);

    /// <inheritdoc cref="Equals(T)"/>
    public static bool operator !=(R<T> self, RW<T> other) => self.read.Equals(other.Value);

    
    #endregion

    #region IEquatable

    /// <inheritdoc />
    public bool Equals(R<T> other) => read.Equals(other.read);

    /// <inheritdoc />
    public bool Equals(T? other) => other != null && read.Equals(other);

    /// <inheritdoc />
    public override bool Equals([NotNullWhen(true)] object? obj)
    {
        return obj != null && obj.Equals(read);
    }

    #endregion
    
    /// <inheritdoc />
    public override int GetHashCode() => read.GetHashCode();
    /// <inheritdoc />
    public override string ToString() => $"R<{typeof(T)}>({read.ToString()})";
    
}
