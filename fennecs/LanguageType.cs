global using TypeID = short;

namespace fennecs;

internal class LanguageType
{
    protected internal static Type Resolve(TypeID typeId) => Types[typeId];

    // Shared ID counter
    protected static TypeID Counter;

    protected static readonly Dictionary<TypeID, Type> Types = new();
    protected static readonly Dictionary<Type, TypeID> Ids = new();

    protected static readonly object RegistryLock = new();


    protected internal static TypeID Identify(Type type)
    {
        lock (RegistryLock)
        {
            // Query the registry directly for a fast response.
            if (Ids.TryGetValue(type, out var id)) return id;

            // ^^^ Optional Pattern: double-checked locking (DCL); move lock down here.
            // Query the registry again, this time synchronized.
            //if (Ids.TryGetValue(type, out id)) return id;

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

        // Register the last (MaxValue) ID as Any type, reserved used for future Wildcards and as a
        // simple stopgap for when all TypeIDs are exhausted, raising an Exception the type initializer
        // of LanguageType<T> (the same way as any other type collision)
        Types[TypeID.MaxValue] = typeof(Any);
        Ids[typeof(Any)] = TypeID.MaxValue;
    }


    private struct Any;

    private struct None;
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
            Types.Add(Id, typeof(T));
            Ids.Add(typeof(T), Id);
        }
    }


    //FIXME: This collides with certain Entity types and generations.
    public static TypeID TargetId => (TypeID) (-Id);
}