using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace fennecs.storage;

/// <summary>
/// Read-only access to a component.
/// </summary>
public readonly ref struct R<T>(ref readonly T value) : IEquatable<R<T>>, IEquatable<T> where T : notnull
{
    internal readonly ref readonly T Value = ref value;

    /// <summary>
    /// Read access to the component's value.
    /// </summary>
    // ReSharper disable once InconsistentNaming
    public ref readonly T read => ref Value;

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
    [OverloadResolutionPriority(9001)]
    public static bool operator ==(R<T> self, R<T> other) => self.Equals(other);

    /// <inheritdoc cref="Equals(T)"/>
    [OverloadResolutionPriority(9001)]
    public static bool operator !=(R<T> self, R<T> other) => !(self == other);

    
    /// <inheritdoc cref="Equals(T)"/>
    public static bool operator ==(R<T> other, RW<T> self) => self.Value.Equals(other.Value);

    /// <inheritdoc cref="Equals(T)"/>
    public static bool operator !=(R<T> self, RW<T> other) => !self.Equals(other);

    
    #endregion

    #region IEquatable

    /// <inheritdoc />
    public bool Equals(R<T> other) => Value.Equals(other.Value);

    /// <inheritdoc />
    public bool Equals(T? other) => other != null && Value.Equals(other);

    /// <inheritdoc />
    public override bool Equals([NotNullWhen(true)] object? obj)
    {
        return obj != null && obj.Equals(Value);
    }

    #endregion
    
    /// <inheritdoc />
    public override int GetHashCode() => Value.GetHashCode();
    /// <inheritdoc />
    public override string ToString() => $"R<{typeof(T)}>({Value.ToString()})";
    
}
