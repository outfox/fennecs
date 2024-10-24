using System.Diagnostics;

namespace fennecs;

/// <summary>
/// A Key is a secondary identifier for components, relations, object links, etc.
/// </summary>
/// <remarks>
/// The Key is a 64-bit value, but only the lower 48 bits are used.
/// Key can contain an Entity, without its Generation (this is the reason why Relations may only target living Entities).
/// <see cref="Key"/> to get the Key of an Entity.
/// </remarks>
public readonly record struct Key
{
    internal readonly ulong Value;
    internal Key(ulong value)
    {
        Debug.Assert((value & Bit.HeaderMask) == 0, "Key must not have header bits set.");
        Value = value; 
    }

    /// <summary>
    /// A Key that matches any Key (including none).
    /// </summary>
    /// <remarks>
    /// ⚠️ Not to be confused with the mythical <c>Any Key</c>. Sorry if you got your hopes up - keep looking?
    /// </remarks>
    public static Key Any = new(Bit.KeyAny);

    /*
    /// <summary>
    /// A Key that matches components with a Target of any kind (e.g. Relations, Object Links, etc.)
    /// </summary>
    public static Key AnyTarget = new(Bit.KeyTarget);
    
    /// <summary>
    /// A Key that matches components with an Object Link.
    /// </summary>
    public static Key AnyLink = new(Bit.KeyObject);
    
    /// <summary>
    /// A Key that matches components with an Object Link.
    /// </summary>
    public static Key AnyEntity = new(Bit.KeyEntity);
    */
    
    /// <summary>
    /// A Key that matches Plain Components only.
    /// </summary>
    public static Key None = default;
    
    
    public bool IsEntity => (Value & Bit.KeyEntity & Bit.EntityFlagWild) == Bit.KeyEntity;                          // If any non-0 flag is set, entity is not real. 
    public bool IsWildcard => (Value & Bit.KeyEntity & Bit.EntityFlagWild) == (Bit.KeyEntity | Bit.EntityFlagWild);
}


public readonly record struct Link2<T> where T : class
{
    public static Key Any = new(LTypeHelper.Sub<T>());
    public static Key With(T value) => new(Bit.KeyObject | LTypeHelper.Sub<T>() | (uint) value.GetHashCode());
}

public readonly record struct Relate2
{
    public static Key Any = new(Bit.KeyEntity);
    public static Key To(Entity entity) => entity;
}


internal static class LTypeHelper
{
    public static ulong Id<T>() => (ulong)LanguageType<T>.Id << 48;
    public static ulong Sub<T>() => (ulong)LanguageType<T>.Id << 32;

    public static Type Resolve(ulong type) => LanguageType.Resolve((TypeID)((type & Bit.TypeMask) >> 48));
    public static Type SubResolve(ulong type) => LanguageType.Resolve((TypeID)((type & Bit.SubTypeMask) >> 32));
}
