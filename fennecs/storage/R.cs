namespace fennecs.storage;

/// <summary>
/// Read-only access to a component.
/// </summary>
public readonly ref struct R<T>(ref readonly T val) where T : notnull
{
    private readonly ref readonly T _value = ref val;

    //public Match Match => default;
    
    /// <summary>
    /// Read access to the component's value.
    /// </summary>
    public T read => _value;

    /// <summary>
    /// Implicitly casts a <see cref="R{T}"/> to its underlying value.
    /// </summary>
    public static implicit operator T(R<T> self) => self._value;
}