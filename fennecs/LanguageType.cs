namespace fennecs;

internal class LanguageType
{
    protected internal static Type Resolve(ushort id) => Types[id];

    // ReSharper disable once StaticMemberInGenericType
    protected static ushort Counter;
    protected static readonly Dictionary<ushort, Type> Types = new();
    protected static readonly Dictionary<Type, ushort> Ids = new();
    
    protected static readonly object RegistryLock = new();

    protected internal static ushort Identify(Type type)
    {
        lock (RegistryLock) // Maybe there's a nicer/safer way for this?
        {
            // Query the registry directly.
            if (Ids.TryGetValue(type, out var id)) return id;

            // Construct LanguageType<T> and invoke static constructor.
            Type[] typeArgs = [type];
            var constructed = typeof(LanguageType<>).MakeGenericType(typeArgs);
            constructed.TypeInitializer?.Invoke(null, null);

            // Constructor should have added the type to the registry.
            return Ids[type];
        }
    }
    
    static LanguageType()
    {
        Types[0] = typeof(None);
        Ids[typeof(None)] = 0;
        
        Types[ushort.MaxValue] = typeof(Any);
        Ids[typeof(Any)] = ushort.MaxValue;
    }

    private struct Any;
    private struct None;
}


// ReSharper disable once UnusedTypeParameter
// ReSharper disable once ClassNeverInstantiated.Global
internal class LanguageType<T> : LanguageType
{
    // ReSharper disable once StaticMemberInGenericType
    public static readonly ushort Id;
    
    static LanguageType()
    {
        lock (RegistryLock)
        {
            if (++Counter >= ushort.MaxValue) throw new InvalidOperationException("Language Level TypeIds exhausted.");
            Id = Counter;
            Types.Add(Id, typeof(T));
            Ids.Add(typeof(T), Id);
        }
    }
}

