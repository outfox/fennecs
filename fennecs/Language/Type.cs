using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;

namespace fennecs.Language;

[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicMethods)]
internal class Type
{
    protected internal static System.Type Resolve(TypeId typeId) => Types[typeId.Value];

    // Shared ID counter
    protected static TypeId Counter;

    protected internal static readonly System.Type[] Types = new System.Type[ushort.MaxValue + 1];
    protected static readonly Traits[] Traits = new Traits[ushort.MaxValue + 1];
    
    /// <summary>
    /// Direct flags that can be used to identify types in bloom filters.
    /// </summary>
    protected static readonly Vector256<byte>[] Identified = new Vector256<byte>[ushort.MaxValue + 1];
    /// <summary>
    /// Bloom filter that encompasses the type's inheritance hierarchy, used to check if it is assignable to another.
    /// </summary>
    protected static readonly Vector256<byte>[] Assignable = new Vector256<byte>[ushort.MaxValue + 1];
    
    protected static readonly Dictionary<System.Type, TypeId> Ids = new();

    protected static readonly object RegistryLock = new();


    protected internal static TypeId Identify(System.Type type)
    {
        // Query the registry directly for a fast response.
        if (Ids.TryGetValue(type, out var id)) return id;

        lock (RegistryLock)
        {
            // Construct a fresh LanguageType<T>, invoking its static constructor.
            System.Type[] typeArgs = [type];
            var constructed = typeof(Type<>).MakeGenericType(typeArgs);
            constructed.TypeInitializer!.Invoke(null, null);

            // Constructor should have added the type to the registry.
            return Ids[type];
        }
    }

    /// <summary>
    /// Wildcard type, used for generic wildcards and as a stopgap for when all TypeIds are exhausted.
    /// </summary>
    private struct Any;
    
    /// <summary>
    /// No type, used to put something in the 0th id to make comparisons simpler.
    /// </summary>
    private struct None;
    


    static Type()
    {
        Counter = default;
        
        // Block off the first (0th) ID and treat as a None type.
        Types[TypeId.None.Value] = typeof(None);
        Ids[typeof(None)] = TypeId.None;

        // Register the last (MaxValue) ID as Any type, reserved used for future Wildcards and as a
        // simple stopgap for when all TypeIds are exhausted, raising an Exception the type initializer
        // of LanguageType<T> (the same way as any other type collision)
        Types[TypeId.Any.Value] = typeof(Any);
        Ids[typeof(Any)] = TypeId.Any;
    }


    private static readonly ConcurrentDictionary<System.Type, TypeFlags> CachedFlags = new();
    public static TypeFlags Flags(System.Type type)
    {
        if (CachedFlags.TryGetValue(type, out var flags)) return flags;

        // Call generic method for T
        var method = typeof(Type).GetMethod(nameof(FlagsOf), BindingFlags.Public | BindingFlags.Static);
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
            // It is recommended to keep this much lower - 64 bytes or fewer.
            if (size <= 0x1000) flags |= (TypeFlags)size;

            flags |= TypeFlags.Unmanaged;
        }

        CachedFlags.TryAdd(typeof(T), flags);
        return flags;
    }
}

internal class Type<T> : Type where T : unmanaged
{
    // ReSharper disable StaticMemberInGenericType (we indeed want this unique for each T)
    public static readonly TypeId Id;
    
    static Type()
    {
        lock (RegistryLock)
        {
            Counter = Counter.Next;
            Id = Counter;
            Ids[typeof(T)] = Id;
            
            Types[Id.Value] = typeof(T);
            
            Traits[Id.Value] = new()
            {
                Type = typeof(T),
                IsClass = typeof(T).IsClass,
                IsValueType = typeof(T).IsValueType,
                IsUnmanaged = typeof(T).IsUnmanaged(),
                IsInterface = typeof(T).IsInterface
            };

            // Single hash function Bloom filter https://hur.st/bloomfilter/?n=50&p=&m=256&k=1
            // This is perfect until 256 component types are known. Then it gradually degrades.
            // On positive (false or true), made 100% reliable by a fast set overlap check.
            Identified[Id.Value] = Bloom((byte) (Id.Value % 256));

            // Classes can be assignable to other classes or interfaces, value types are only assignable to interfaces.
            Assignable[Id.Value] = typeof(T).IsClass
                ? InheritanceBloom() | InterfaceBloom()
                : InterfaceBloom();
        }
    }


    /// <summary>
    /// Returns a specific bit set in a 256 bit bloom filter.
    /// </summary>
    /// <param name="bit">the bit to set</param>
    /// <returns></returns>
    private static Vector256<byte> Bloom(byte bit)
    {
        // The index of the byte to set.
        var index = bit / 8;
        
        // The byte to set.
        var mask = (byte) (1 << (bit % 8));
        
        return Vector256<byte>.Zero.WithElement(index, mask);
    }

    /// <summary>
    /// Calculate bloom filter value for a type. (using 3 independent hashes)<ul><li>
    /// 1 in 1000 false positives up to 10 inheritance/interface layers deep.</li><li>
    /// 1 in 100 false positives up to 20 inheritance/interface layers deep.</li></ul>
    /// On positive, is just n Type.isAssignableFrom checks.
    /// </summary>
    private static Vector256<byte> TypeBloom(System.Type type) => 
        Bloom((byte) (type.GetHashCode() % 256)) |
        Bloom((byte) (type.GUID.GetHashCode() % 256)) |
        Bloom((byte) (type.Name.GetHashCode() % 256));
    
    /// <summary>
    /// Bloom filter with bit set for a type and each base type it inherits from.
    /// </summary>
    /// <returns></returns>
    private static Vector256<byte> InheritanceBloom()
    {
        var type = typeof(T);
        if (type.IsValueType) return Vector256<byte>.Zero;
        
        var bloom = Vector256<byte>.Zero;
        while (type != null && type != typeof(object))
        {
            bloom |= TypeBloom(type);
            type = type.BaseType;
        }

        return bloom;
    }

    /// <summary>
    /// Bloom filter with bit set for each interface the type implements.
    /// </summary>
    private static Vector256<byte> InterfaceBloom() =>
        typeof(T).GetInterfaces() is { } types
            ? types.Aggregate(Vector256<byte>.Zero, (current, type) => current | TypeBloom(type))
            : Vector256<byte>.Zero;
}