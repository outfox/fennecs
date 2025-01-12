using System.Collections.Concurrent;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace fennecs;

[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicMethods)]
internal class LanguageType
{
    protected internal static Type Resolve(short typeId) => Types[typeId];

    // Shared ID counter
    protected static short Counter;

    protected static readonly ConcurrentDictionary<short, Type> Types = new();
    protected static readonly ConcurrentDictionary<Type, short> Ids = new();

    protected static readonly Lock RegistryLock = new();


    protected internal static short Identify(Type type)
    {
        // Query the registry directly for a fast response.
        if (Ids.TryGetValue(type, out var id)) return id;

        lock (RegistryLock)
        {
            // Construct LanguageType<T>, invoking its static constructor.
            Type[] typeArgs = [type];
            var constructed = typeof(LanguageType<>).MakeGenericType(typeArgs);
            constructed.TypeInitializer!.Invoke(null, null);

            // Constructor should have added the type to the registry.
            return Ids[type];
        }
    }


    static LanguageType()
    {
        // Block off the first (0th) ID and treat as a None type.
        Types[0] = typeof(None);
        Ids[typeof(None)] = 0;

        // Entity is the first type ID.
        Identify(typeof(Entity));

        // Register the last (MaxValue) ID as Any type, reserved used for future Wildcards and as a
        // simple stopgap for when all TypeIDs are exhausted, raising an Exception the type initializer
        // of LanguageType<T> (the same way as any other type collision)
        Types[short.MaxValue] = typeof(Any);
        Ids[typeof(Any)] = short.MaxValue;
    }


    private readonly struct Any;

    private readonly struct None;

    private static readonly ConcurrentDictionary<Type, TypeFlags> CachedFlags = new();
    public static TypeFlags Flags(Type type)
    {
        if (CachedFlags.TryGetValue(type, out var flags)) return flags;

        // Call generic method for T
        var method = typeof(LanguageType).GetMethod(nameof(FlagsOf), BindingFlags.Public | BindingFlags.Static);
        var generic = method!.MakeGenericMethod(type);
        return (TypeFlags) generic.Invoke(null, null)!;
    }

    public static TypeFlags FlagsOf<T>()
    {
        if (CachedFlags.TryGetValue(typeof(T), out var flags)) return flags;

        if (typeof(T).IsUnmanaged())
        {
            var size = Unsafe.SizeOf<T>();

            // Arbitrary: 2048 bytes is the maximum size of a SIMD-able type.
            // It is recommended to keep this much lower - 64 bytes or less.
            if (size <= 0x1000) flags |= (TypeFlags)size;

            flags |= TypeFlags.Unmanaged;
        }

        CachedFlags.TryAdd(typeof(T), flags);
        return flags;
    }

    public static ulong LinkId(object link) => LinkMask & (ulong) Identify(link.GetType()) << 32;

    protected const ulong LinkMask = 0x0000_0FFF_0000_0000u;
}

internal class LanguageType<T> : LanguageType
{
    // ReSharper disable once StaticMemberInGenericType (we indeed want this unique for each T)
    public static readonly short Id;
    
    static LanguageType()
    {
        lock (RegistryLock)
        {
            Id = ++Counter;
            Debug.Assert(Id < 4096, "Too many componenttypes registered.");
            Types.TryAdd(Id, typeof(T));
            Ids.TryAdd(typeof(T), Id);
        }
    }
    
    //FIXME: This constrains the number of legal component types quite a bit. (to max 4096)
    public new static ulong LinkId => LinkMask & (ulong) Id << 32;
}


internal static class TypeFlagExtensions
{
    public static bool IsUnmanaged(this Type t)
    {
        if (t.IsPrimitive || t.IsPointer || t.IsEnum) return true;
        
        if (!t.IsValueType || t.IsGenericType || t.IsByRef || t.IsByRefLike) return false;

        var fields = t.GetFields(BindingFlags.Public
                                 | BindingFlags.NonPublic
                                 | BindingFlags.Instance);
        
        // Recursively check all fields.
        return fields.All(x => x.FieldType.IsUnmanaged());
    }
    
    // Probably no use for this comptime optimization?
    // public static bool IsUnmanaged<T>() where T: unmanaged => true;
}

[Flags]
public enum TypeFlags : ushort
{ 
    SIMDSize  = 0x1fff, // bottom 12 bits.
    Unmanaged = 0x8000, // top bit.
}
