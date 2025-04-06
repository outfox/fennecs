namespace fennecs.Language;

internal readonly record struct TypeId(ushort Value)
{
    public static TypeId None => new(0);
    public static TypeId Any => new(ushort.MaxValue);

    public TypeId Next => Value < ushort.MaxValue - 1 
        ? new((ushort) (Value + 1)) 
        : throw new OverflowException("TypeIds exhausted. (ushort.MaxValue reached)");

    public bool Matches(TypeId that)
    {
        // None matches nothing.
        if (this == None || that == None) return false;

        // Any matches anything except none.
        if (this == Any || that == Any) return true;

        // Direct equality comparison.
        if (this == that) return true;

        // Inheritance check.
        // (to avoid this repeat memory access, we use bloom filters in o(n) or worse cases like Signatures)
        var thisType = Type.Types[Value];
        return thisType.IsAssignableFrom(Type.Types[that.Value]);
    }
}