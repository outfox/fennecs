namespace fennecs.storage;

/// <summary>
/// Read-only access to a component.
/// </summary>
public readonly ref struct R<T>(ref T val) where T : notnull
{
    private readonly ref T _value = ref val;
    
    /// <summary>
    /// Read access to the component's value.
    /// </summary>
    public T read => _value;

    /// <summary>
    /// Implicitly casts a <see cref="R{T}"/> to its underlying value.
    /// </summary>
   public static implicit operator T(R<T> self) => self._value;
}