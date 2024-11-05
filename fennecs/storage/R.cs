namespace fennecs.storage;

internal readonly ref struct R<T>(ref T val) where T : notnull
{
    private readonly ref T _value = ref val;
    public T read => _value;

    public static implicit operator T(R<T> self) => self._value;
}