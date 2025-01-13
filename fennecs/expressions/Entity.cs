// SPDX-License-Identifier: MIT

using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using fennecs.CRUD;
using fennecs.pools;
using fennecs.storage;

namespace fennecs;

/// <summary>
/// Entity: An object in the fennecs World, that can have any number of Components.
/// </summary>
[StructLayout(LayoutKind.Explicit)]
public readonly record struct Entity : IComparable<Entity>, IEntity
{
    [FieldOffset(0)] internal readonly ulong Value;

    //Entity Components
    [FieldOffset(0)]
    internal readonly int Index; //IDEA: Can use top 1~2 bits for special state, e.g. disabled, hidden, etc.

    [FieldOffset(4)] internal readonly byte WorldIndex;

    [FieldOffset(5)] internal readonly byte Flags;
    [FieldOffset(6)] internal readonly short Generation;

    //Constituents for GetHashCode()
    [FieldOffset(0)] internal readonly uint DWordLow;
    [FieldOffset(4)] internal readonly uint DWordHigh;


    [Obsolete("Just use this / the Entity itself")]
    internal Entity Id => this;
    
    private ref Meta Meta => ref World[this];
    
    internal int Row => Meta.Row;

    /// <summary>
    /// <c>null</c> equivalent for Entity.
    /// </summary>
    public static readonly Entity None = default;

    /// <summary>
    /// The World this Entity belongs to.
    /// </summary>
    public World World => World.Get(WorldIndex);
    
    /// <summary>
    /// The Archetype this Entity belongs to.
    /// </summary>
    public Archetype Archetype => Meta.Archetype;


    #region IComparable/IEquatable Implementation

    /// <inheritdoc cref="IEquatable{T}"/>
    public bool Equals(Entity other) => Value == other.Value;

    /// <inheritdoc cref="IComparable{T}"/>
    public int CompareTo(Entity other) => Value.CompareTo(other.Value);


    /// <inheritdoc />
    public override int GetHashCode()
    {
        unchecked
        {
            return (int) (0x811C9DC5u * DWordLow + 0x1000193u * DWordHigh + 0xc4ceb9fe1a85ec53u);
        }
    }

    #endregion


    #region Constructors / Creators
    
    /// <summary>
    /// Create a new Entity, Generation 1, in the given World. Called by EntityPool.
    /// </summary>
    internal Entity(World.Id worldId, int index, short generation = 1)
    {
        // 0xgggg_E0ww_iiii_iiii
        Value = (ulong) generation << 48 | Key.BaseFlag | worldId.Bits | (uint) index;
    }


    internal Entity(ulong raw, ushort generation)
    {
        Value = raw | (ulong) generation << 48;
    }

    internal Entity(ulong raw)
    {
        Value = raw;
    }

    /// <summary>
    /// Implicitly convert a Key to an Entity.
    /// </summary>
    /// <throws><see cref="ArgumentException"/> if the Key is not an Entity.</throws>
    /// <throws><see cref="InvalidOperationException"/> if the Entity is not alive.</throws>
    /// <throws><see cref="NullReferenceException"/> if the decoded Entity has an invalid World.</throws>
    /// <returns>the entity, if the Key is a living Entity</returns>
    public static implicit operator Entity(Key key) => new(key);


    /// <summary>
    /// Construct an Entity from a Key.
    /// </summary>
    /// <remarks>
    /// The entity may technically not be alive, but relation keys are usually guaranteed to be alive if the world hasn't changed since it was obtained.
    /// </remarks>
    /// <throws><see cref="ArgumentException"/> if the Key is not an Entity relation key.</throws>
    /// <throws><see cref="NullReferenceException"/> if the decoded World is invalid.</throws>
    public Entity(Key key)
    {
        if (!key.IsEntity) throw new ArgumentException("Key must be an Entity.");
        Value = key.Value;
        Generation = World[Key.Index].Generation;
    }

    internal Entity Successor
    {
        get
        {
            var generationWrappedStartingAtOne = (ushort) (Generation % (ushort.MaxValue - 1) + 1);
            return new(Value, generationWrappedStartingAtOne);
        }
    }

    /// <summary>
    /// Is this Entity alive in its World?
    /// </summary>
    public bool Alive => World.IsAlive(this);

    /// <summary>
    /// The Key of this Entity (for use in relations).
    /// </summary>
    internal Key Key => new(this);

    /// <inheritdoc />
    public IReadOnlyList<Component> Components => World.GetComponents(this);

    #endregion

    /// <summary>
    /// <para><b>Wildcard match expression for Entity iteration.</b><br/>This matches only <b>Entity-Entity</b> Relations of the given Stream Type.
    /// </para>
    /// <para>This expression is free when applied to a Filter expression, see <see cref="Query"/>.
    /// </para>
    /// <para>Applying this to a Query's Stream Type can result in multiple iterations over entities if they match multiple component types. This is due to the wildcard's nature of matching all components.</para>
    /// </summary>
    /// <inheritdoc cref="Any"/>
    public static readonly Match Any = Match.Entity;


    /// <inheritdoc />
    public override string ToString()
    {
        var gen = Alive ? $"gen{Generation:D5}" : "DEAD";
        return $"E-{WorldIndex:d3}-{Index:x8} {gen}";
    }

    /// <inheritdoc />
    public Entity Add<C>(C component, Key key = default, [CallerFilePath] string callerFile = "", [CallerLineNumber] int callerLine = 0) where C : notnull
    {
        World.AddComponent(this, TypeExpression.Of<C>(key), component, callerFile, callerLine);
        return this;
    }
    
    /// <inheritdoc />
    public Entity Add<C>(Key key = default, [CallerFilePath] string callerFile = "", [CallerLineNumber] int callerLine = 0) where C : notnull, new() => Add(new C(), key);

    /// <inheritdoc />
    public Entity Link<L>(L link, [CallerFilePath] string callerFile = "", [CallerLineNumber] int callerLine = 0) where L : class
    {
        if (typeof(L) == typeof(object))
        {
            World.AddComponent(this, TypeExpression.Of(link.GetType(), Key.Of(link)), link);
            return this;
        }
        
        World.AddComponent(this, TypeExpression.Of<L>(Key.Of(link)), link);
        return this;
    }

    /// <inheritdoc />
    public Entity Remove<C>(Match match = default, [CallerFilePath] string callerFile = "", [CallerLineNumber] int callerLine = 0) where C : notnull => Remove(MatchExpression.Of<C>(match));

    /// <inheritdoc />
    public Entity Remove(MatchExpression expression, [CallerFilePath] string callerFile = "", [CallerLineNumber] int callerLine = 0)
    {
        World.RemoveComponent(this, expression);
        return this;
    }

    /// <inheritdoc />
    public bool Has<C>(Match match = default) where C : notnull => World.HasComponent<C>(this, match);

    /// <inheritdoc />
    public bool Has(Type type, Match match = default) => World.HasComponent(this, type, match);

    /// <inheritdoc />
    public void Despawn() => World.Despawn(this);


    /// <summary>
    /// Returns a <c>ref readonly</c> to a component of the given type, matching the given Key.
    /// </summary>
    public ref readonly C Get<C>(Key key = default) where C : notnull => ref World.GetComponent<C>(this, key);
    
    /// <summary>
    /// Gets the component from the Entity (boxed)
    /// </summary>
    public object Get(Type type, Key key = default)
    {
        if (World.TryGetComponent(this, TypeExpression.Of(type, key), out var component)) return component;
        throw new InvalidOperationException($"Entity {this} does not have a component of {type} for key {key}");
    }
    

    /// <summary>
    /// Returns a <c>ref readonly</c> to a component of the given type, matching the given Key.
    /// </summary>
    public bool Get(Type type, Key key, [MaybeNullWhen(false)] out object component)
    {
        return World.TryGetComponent(this, TypeExpression.Of(type, key), out component);
    }

    /// <summary>
    /// Returns a <c>ref readonly</c> to a component of the given type, matching the given Key.
    /// </summary>
    public bool Get(Type type, [MaybeNullWhen(false)] out object component)
    {
        return World.TryGetComponent(this, TypeExpression.Of(type), out component);
    }

    /// <summary>
    /// Returns a List of all component type expressions accompanied by their values matching the provided term and backing type <see cref="T"/>.
    /// </summary>
    /// <remarks>
    /// The values are copies, but if the components are reference types, these values will reference the same objects.
    /// <see cref="PooledList{T}"/> should be Disposed if possible, either by declaring them in a using statement, or by calling their <see cref="IDisposable.Dispose"/> method.
    /// </remarks>
    /// <returns><c>PooledList&lt;(TypeExpression type, T value)&gt;</c></returns>
    public PooledList<(TypeExpression expression, T value)> Get<T>(Match match = default) where T : notnull
    {
        using var storages = Archetype.Match<T>(match);
        var list = PooledList<(TypeExpression type, T value)>.Rent();
        var index = Index;
        list.AddRange(storages.Select(storage => (storage.Expression, storage[index])));
        return list;
    }

    /// <summary>
    /// Returns a <c>ref</c> to a component of the given type, matching the given Key.
    /// </summary>
    public ref C Write<C>(Key key = default) where C : notnull => ref Archetype.GetStorage<C>(key)[Row];

    /// <summary>
    /// Returns a <c>ref readonly</c> to a component of the given type, matching the given Key.
    /// </summary>
    public ref readonly C Read<C>(Key key = default) where C : notnull => ref Archetype.GetStorage<C>(key)[Row];

    /// <summary>
    /// Sets all components of the given backing type on the Entity, matching the given Match term.
    /// </summary>
    /// <remarks>
    /// This (as all functions taking a Match term) supports wildcards.
    /// </remarks> 
    public Entity Set<C>(in C value, Match match = default) where C : notnull
    {
        using var storages = Archetype.Match<C>(match);
        foreach (var storage in storages) storage.Store(Index, value);
        return this;
    }

    /// <summary>
    /// Returns a reference to a component of the given type, matching the given Key.
    /// </summary>
    /// <remarks>
    /// Only use this if you need to work with the component directly, otherwise it is recommended to use <see cref="Entity.Get{C}(fennecs.Key)"/> and <see cref="Set{C}(in C, fennecs.Key)"/>.
    /// </remarks>
    public RWImmediate<C> Ref<C>(Key key = default) where C : notnull => new(ref World.GetComponent<C>(this, key), this, key);

    /// <inheritdoc />
    public bool Has(MatchExpression expression) => World.HasComponent(this, expression);

    /// <inheritdoc />
    public bool Has<L>(L link) where L : class => World.HasComponent(this, MatchExpression.Of<L>(link.Key()));
    
    /// <summary>Truthy if the Entity is alive.</summary>
    public static implicit operator bool(Entity entity) => entity.Alive;
}