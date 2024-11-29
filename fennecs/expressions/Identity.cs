// SPDX-License-Identifier: MIT

using System.Diagnostics;
using System.Runtime.InteropServices;

namespace fennecs;

/// <summary>
/// Refers to an identity:
/// real Entity, tracked object, or virtual concept (e.g. any/none Match Expression).
/// </summary>
[StructLayout(LayoutKind.Explicit)]
public readonly record struct Identity : IComparable<Identity>
{
    [FieldOffset(0)] internal readonly ulong Value;

    //Identity Components
    [FieldOffset(0)] internal readonly int Index; //IDEA: Can use top 1~2 bits for special state, e.g. disabled, hidden, etc.
    [FieldOffset(4)] internal readonly byte WorldIndex;
    
    [FieldOffset(5)] internal readonly byte Flags;
    [FieldOffset(6)] internal readonly ushort Generation;

    //Constituents for GetHashCode()
    [FieldOffset(0)] internal readonly uint DWordLow;
    [FieldOffset(8)] internal readonly uint DWordHigh;


    /// <summary>
    /// <c>null</c> equivalent for Entity.
    /// </summary>
    public static readonly Identity None = default;

    
    /// <summary>
    /// The World this Entity belongs to.
    /// </summary>
    public World World => World.Get(WorldIndex);


    // Entity Reference.
    /// <summary>
    /// Truthy if the Identity represents an actual Entity.
    /// Falsy if it is a virtual concept or a tracked object.
    /// Falsy if it is the <c>default</c> Identity.
    /// </summary>
    public bool IsEntity => Index > 0 && Generation > 0;

    // Tracked Object Reference.
    /// <summary>
    /// Truthy if the Identity represents a tracked object.
    /// Falsy if it is a virtual concept or an actual Entity.
    /// Falsy if it is the <c>default</c> Identity.
    /// </summary>
    public bool IsObject => Generation < 0;

    // Wildcard Entities, such as Any, Object, Entity, or Relation.
    /// <summary>
    /// Truthy if the Identity represents a virtual concept (see <see cref="Cross"/>).
    /// Falsy if it is an actual Entity or a tracked object.
    /// Falsy if it is the <c>default</c> Identity.
    /// </summary>
    public bool IsWildcard => Generation == 0 && Index < 0;


    #region IComparable/IEquatable Implementation

    /// <inheritdoc cref="IEquatable{T}"/>
    public bool Equals(Identity other) => Value == other.Value;

    /// <inheritdoc cref="IComparable{T}"/>
    public int CompareTo(Identity other) => Value.CompareTo(other.Value);

    /// <summary>
    /// Casts an Entity to its Identity. (extracting the appropriatefield)
    /// </summary>
    /// <param name="entity">an Entity</param>
    /// <returns>the Identity</returns>
    public static implicit operator Identity(Entity entity) => entity.Id;
    

    /// <inheritdoc />
    public override int GetHashCode()
    {
        unchecked
        {
            return (int) (0x811C9DC5u * DWordLow + 0x1000193u * DWordHigh + 0xc4ceb9fe1a85ec53u);
        }
    }
    #endregion


    internal Type Type => typeof(Identity);


    #region Constructors / Creators
    /// <summary>
    /// Create a new Identity, Generation 1, in the given World. Called by IdentityPool.
    /// </summary>
    internal Identity(World.Id worldId, int index, short generation = 1)
    {
        // 0xgggg_E0ww_iiii_iiii
        Value = (ulong) generation << 48 | BaseFlag | worldId.Bits | (uint) index;   
    }
    
    internal const ulong BaseFlag = 0x0000_E000_0000_0000u;
    
    
    internal Identity(ulong key, ushort generation)
    {
        Value = key | (ulong) generation << 48;
    }

    internal Identity Successor
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

    public Key Key => Key.Of(this);

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
    public override string ToString() => $"E-{WorldIndex:d3}-{Index:x8} gen{Generation:D5}";
}
