namespace fennecs.storage;

/// <summary>
/// Read-only access to a component.
/// </summary>
public readonly ref struct R<T>(ref readonly T val) where T : notnull
{
    private readonly ref readonly T _value = ref val;

    /// <summary>
    /// Read access to the component's value.
    /// </summary>
    // ReSharper disable once InconsistentNaming
    public T read => _value;

    /// <summary>
    /// Implicitly casts a <see cref="R{T}"/> to its underlying value.
    /// </summary>
    public static implicit operator T(R<T> self) => self._value;

    /// <summary>
    /// Implicitly casts a <see cref="R{T}"/> to a string for output, calling ToString() on its value.
    /// </summary>
    public static implicit operator string(R<T> self) => self.ToString();

    /// <inheritdoc />
    public override string ToString() => _value.ToString() ?? "null";
}