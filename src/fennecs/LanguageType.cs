global using TypeID = short;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace fennecs;

[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicMethods)]
internal class LanguageType
{
    /// <summary>
    /// TypeId of the entity column type present in every Archetype. (reserved)
    /// </summary>
    internal const TypeID EntityId = 1;

    /// <summary>
    /// Highest assignable TypeId. TypeIds occupy 12 bits of a <see cref="TypeExpression"/>;
    /// 0 (None), 1 (<see cref="EntityId"/>), and 0xFFF (<see cref="AnyId"/>) are reserved.
    /// </summary>
    internal const TypeID MaxTypeId = 0xFFE;

    /// <summary>
    /// Reserved TypeId for future Wildcards / the exhaustion sentinel.
    /// </summary>
    internal const TypeID AnyId = 0xFFF;


    protected internal static Type Resolve(TypeID typeId) => Types[typeId];

    // Shared ID counter
    protected static TypeID Counter;

    protected static readonly Dictionary<TypeID, Type> Types = new();
    protected static readonly Dictionary<Type, TypeID> Ids = new();

    protected static readonly Lock RegistryLock = new();

    // Side-table of TypeFlags, indexed by TypeId. (flags are type-derived, so they
    // live here instead of inside the packed TypeExpression)
    private static readonly TypeFlags[] FlagTable = new TypeFlags[AnyId + 1];


    /// <summary>
    /// The <see cref="TypeFlags"/> of the type registered with the given TypeId.
    /// </summary>
    internal static TypeFlags FlagsById(TypeID typeId) => FlagTable[typeId];


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

            // Reserve the 1st ID for the entity column type present in every Archetype.
            // LanguageType<Entity> adopts this reservation when constructed.
            Types[EntityId] = typeof(Entity);
            Ids[typeof(Entity)] = EntityId;
            Counter = EntityId;

            // Register the last (0xFFF) ID as Any type, reserved for future Wildcards.
            Types[AnyId] = typeof(Any);
            Ids[typeof(Any)] = AnyId;
        }
    }


    private struct Any;

    private struct None;


    public static TypeFlags Flags(Type type) => FlagTable[Identify(type)];

    public static TypeFlags FlagsOf<T>() => FlagTable[LanguageType<T>.Id];

    protected static TypeFlags ComputeFlags<T>()
    {
        if (RuntimeHelpers.IsReferenceOrContainsReferences<T>()) return default;

        return TypeFlags.Unmanaged | ((TypeFlags) Unsafe.SizeOf<T>() & TypeFlags.SIMDSize);
    }

    protected static void StoreFlags(TypeID id, TypeFlags flags) => FlagTable[id] = flags;
}

internal class LanguageType<T> : LanguageType
{
    // ReSharper disable once StaticMemberInGenericType (we indeed want this unique for each T)
    public static readonly TypeID Id;

    public static readonly int Size = Unsafe.SizeOf<T>();

    static LanguageType()
    {
        lock (RegistryLock)
        {
            if (Ids.TryGetValue(typeof(T), out var reserved))
            {
                // Adopt a reserved registration (e.g. the entity column type).
                Id = reserved;
            }
            else
            {
                if (Counter >= MaxTypeId)
                {
                    throw new InvalidOperationException($"TypeID space exhausted: no more than {MaxTypeId - 1} distinct Component types can be registered per process.");
                }

                Id = ++Counter;
                Types.Add(Id, typeof(T));
                Ids.Add(typeof(T), Id);
            }

            StoreFlags(Id, ComputeFlags<T>());
        }
    }
}

[Flags]
internal enum TypeFlags : ushort
{
    SIMDSize = 0b0000111111111111,
    Unmanaged = 0b1000000000000000, // top bit.
}
