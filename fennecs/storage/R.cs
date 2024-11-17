using System.Runtime.CompilerServices;

namespace fennecs.storage;

/// <summary>
/// Read-only access to a component.
/// </summary>
public readonly ref struct R<T>(ref readonly T value) : IEquatable<R<T>>, IEquatable<T> where T : notnull
{
    private readonly ref readonly T _value = ref value;

    /// <summary>
    /// Read access to the component's value.
    /// </summary>
    // ReSharper disable once InconsistentNaming
    public ref readonly T read => ref _value;

    /// <summary>
    /// Implicitly casts a <see cref="R{T}"/> to its underlying value.
    /// </summary>
    public static implicit operator T(R<T> self) => self._value;

    /// <summary>
    /// Implicitly casts a <see cref="R{T}"/> to a string for output, calling ToString() on its value.
    /// </summary>
    public static implicit operator FormattableString(R<T> self) => FormattableStringFactory.Create(self.ToString());

    /// <inheritdoc />
    public override string ToString() => _value.ToString() ?? "null";

    /// <inheritdoc />
    public bool Equals(R<T> other) => _value.Equals(other._value);

    /// <inheritdoc />
    public bool Equals(T? other) => _value.Equals(other);
}