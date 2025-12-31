global using TypeID = short;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace fennecs;

[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicMethods)]
internal class LanguageType
{
    protected internal static Type Resolve(TypeID typeId) => Types[typeId];

    // Shared ID counter
    protected static TypeID Counter;

    protected static readonly ConcurrentDictionary<TypeID, Type> Types = new();
    protected static readonly ConcurrentDictionary<Type, TypeID> Ids = new();

    protected static readonly object RegistryLock = new();


    protected internal static TypeID Identify(Type type)
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
        lock (RegistryLock)
        {
            // Block off the first (0th) ID and treat as a None type.
            Types[0] = typeof(None);
            Ids[typeof(None)] = 0;

            // Register the 1st ID as Identity's type, used for Entity identities.
            Types[1] = typeof(Identity);
            Ids[typeof(Identity)] = 1;
            Counter = 1;

            // Register the last (MaxValue) ID as Any type, reserved used for future Wildcards and as
            // a simple stopgap for when all TypeIDs are exhausted. This will raise an Exception in the
            // type initializer of LanguageType<T> (the same way as any other type collision)
            Types[TypeID.MaxValue] = typeof(Any);
            Ids[typeof(Any)] = TypeID.MaxValue;
        }
    }


    private struct Any;

    private struct None;

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

        if (IsBlittable<T>())
        {
            var size = Unsafe.SizeOf<T>();

            // Arbitrary: 2048 bytes is the maximum size of a SIMD-able type.
            // It is recommended to keep this much lower - 64 bytes or fewer.
            if (size <= 0x1000) flags |= (TypeFlags) size;

            flags |= TypeFlags.Unmanaged;
        }

        CachedFlags.TryAdd(typeof(T), flags);
        return flags;
    }

    private static bool IsBlittable<U>()
    {
        if (RuntimeHelpers.IsReferenceOrContainsReferences<U>()) return false;
        
        try
        {
            _ = Marshal.SizeOf<U>();
            return true;
        }
        catch
        {
            return false;
        }
    }
}

internal class LanguageType<T> : LanguageType
{
    // ReSharper disable once StaticMemberInGenericType (we indeed want this unique for each T)
    public static readonly TypeID Id;

    static LanguageType()
    {
        lock (RegistryLock)
        {
            Id = ++Counter;
            Types.TryAdd(Id, typeof(T));
            Ids.TryAdd(typeof(T), Id);
        }
    }


    //FIXME: This collides with certain Entity types and generations.
    public static TypeID TargetId => (TypeID) (-Id);
}

[Flags]
internal enum TypeFlags : ushort
{
    SIMDSize = 0x1fff, // bottom 12 bits.
    Unmanaged = 0x8000, // top bit.
}