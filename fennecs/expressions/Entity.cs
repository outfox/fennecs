// SPDX-License-Identifier: MIT

using System.Runtime.InteropServices;
using fennecs.CRUD;

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
    [FieldOffset(6)] internal readonly ushort Generation;

    //Constituents for GetHashCode()
    [FieldOffset(0)] internal readonly uint DWordLow;
    [FieldOffset(8)] internal readonly uint DWordHigh;


    /// <summary>
    /// <c>null</c> equivalent for Entity.
    /// </summary>
    public static readonly Entity None = default;
    

    /// <summary>
    /// The World this Entity belongs to.
    /// </summary>
    public World World => World.Get(WorldIndex);


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


    internal Entity(ulong key, ushort generation)
    {
        Value = key | (ulong) generation << 48;
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
    /// Implicitly convert an Entity to a Key, for use in relations and matching.
    /// </summary>
    public static implicit operator Key(Entity entity) => new(entity);

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
    public static readonly Key Any = new((ulong) Key.Kind.Entity);


    /// <inheritdoc />
    public override string ToString()
    {
        var gen = Alive ? $"gen{Generation:D5}" : "DEAD";
        return $"E-{WorldIndex:d3}-{Index:x8} {gen}";
    }

    /// <inheritdoc />
    public Entity Add<C>(C component, Key key = default) where C : notnull
    {
        World.AddComponent(this, TypeExpression.Of<C>(key), component);
        return this;
    }

    /// <inheritdoc />
    public Entity Link<L>(L link) where L : class
    {
        World.AddComponent(this, TypeExpression.Of<L>(Key.Of(link)), link);
        return this;
    }

    /// <inheritdoc />
    public Entity Remove<C>(Key key = default) where C : notnull
    {
        World.RemoveComponent(this, TypeExpression.Of<C>(key));
        return this;
    }

    /// <inheritdoc />
    public bool Has<C>(Key key = default) where C : notnull
    {
        return World.HasComponent(this, TypeExpression.Of<C>(key));
    }

    /// <inheritdoc />
    public void Despawn()
    {
        World.Despawn(this);
    }
}