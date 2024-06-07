// SPDX-License-Identifier: MIT

using System.Collections.Immutable;
using System.Runtime.InteropServices;

namespace fennecs;

/// <summary>
/// Represents a union structure that encapsulates type expressions, including Components,
/// Entity-Entity relations, Entity-object relations, and Wildcard expressions matching multiple.
/// </summary>
[StructLayout(LayoutKind.Explicit)]
public readonly struct TypeExpression : IEquatable<TypeExpression>, IComparable<TypeExpression>
{
    #region Struct Data Layout

    //             This is a 64 bit union struct.
    //                 Layout: (little endian)
    //   | LSB                                   MSB |
    //   |-------------------------------------------|
    //   | Value                                     |
    //   | 64 bits                                   |
    //   |-------------------------------------------|
    //   | Id              | Generation | TypeNumber |
    //   | 32 bits         |  16 bits   |  16 bits   |
    //   |-------------------------------------------|
    //   | Entity (Identity)            | TypeNumber |
    //   | 48 bits                      |  16 bits   |

    //Union Backing Store
    [FieldOffset(0)] internal readonly ulong Value;

    //Identity Components
    [FieldOffset(0)] internal readonly int Id;
    [FieldOffset(4)] internal readonly ushort Generation;
    [FieldOffset(4)] internal readonly TypeID Decoration;

    // Type Header
    [FieldOffset(6)] internal readonly TypeID TypeId;

    //Constituents for GetHashCode()
    [FieldOffset(0)] internal readonly uint DWordLow;
    [FieldOffset(4)] internal readonly uint DWordHigh;

    //Target interpretation
    [FieldOffset(0)] internal readonly Identity Identity;
    #endregion


    /// <summary>
    /// The Target of this <see cref="TypeExpression"/>, determining whether it acts as a plain Component,
    /// an Object Link, an Entity Relation, or a Wildcard Match Expression.
    /// </summary>
    /// <remarks>
    /// <para>If <see cref="fennecs.Identity.Plain"/>, the type expression matches a plain Component of its <see cref="Type"/>.</para>
    /// <para>If a specific <see cref="Identity"/> (e.g. <see cref="Identity.IsEntity"/> or <see cref="Identity.IsObject"/> are true), the type expression represents a relation targeting that Entity.</para>
    /// <para>If <see cref="fennecs.Identity.Any"/>, the type expression acts as a Wildcard 
    ///   expression that matches any target, INCLUDING <see cref="fennecs.Identity.Plain"/>.</para>
    /// <para> If <see cref="fennecs.Identity.Target"/>, the type expression acts as a Wildcard 
    ///   expression that matches relations and their targets, EXCEPT <see cref="fennecs.Identity.Plain"/>.</para>
    /// <para> If <see cref="fennecs.Identity.Entity"/>, the type expression acts as a Wildcard 
    ///   expression that matches ONLY Entity-entity relations.</para>
    /// <para> If <see cref="fennecs.Identity.Object"/>, the type expression acts as a Wildcard 
    ///   expression that matches ONLY entity-object relations.</para>
    /// </remarks>
    //internal Target Target => new(new(Id, Decoration));

    internal Relate Relation => new(new(Id, Decoration));

    internal Identity Target
    {
        get => Identity;
        init
        {
            var type = TypeId;
            Identity = value;
            TypeId = type;
        }
    }

    /// <summary>
    /// The <see cref="TypeExpression"/> is a relation, meaning it has a target other than None.
    /// </summary>
    public bool isRelation => TypeId != 0 && Target != Wildcard.Plain && !Target.IsWildcard;


    /// <summary>
    ///  Is this TypeExpression a Wildcard expression? See <see cref="Cross"/>.
    /// </summary>
    public bool isWildcard => Target.IsWildcard;


    /// <summary>
    /// Get the backing Component type that this <see cref="TypeExpression"/> represents.
    /// </summary>
    public Type Type => LanguageType.Resolve(TypeId);


    // A method to check if a TypeExpression matches any of the given type expressions in a collection.
    /// <summary>
    /// Does this <see cref="TypeExpression"/> match any of the given type expressions?
    /// </summary>
    /// <param name="other">a collection of type expressions</param>
    /// <returns>true if matched</returns>
    public bool Matches(ImmutableHashSet<TypeExpression> other)
    {
        var self = this;

        //TODO: HUGE OPTIMIZATION POTENTIAL! (set comparison is way faster than linear search, etc.) FIXME!!
        foreach (var type in other)
        {
            if (self.Matches(type)) return true;
            if (type.Matches(self)) return true;
        }

        return false;
    }

    /// <inheritdoc cref="Matches(System.Collections.Immutable.ImmutableHashSet{fennecs.TypeExpression})"/>
    [Obsolete("Try to use Matches(Signature) or ImmutableSortedSets directly.")]
    public bool Matches(IEnumerable<TypeExpression> other)
    {
        return Matches(other.ToImmutableHashSet());
    }

    /// <summary>
    /// Fast O(1) Matching against Signatures.
    /// </summary>
    public bool Matches(Signature other)
    {
        return other.Contains(this);
    }


    /// <summary>
    /// Match against another TypeExpression; used for Query Matching.
    /// Examines the Type and Target fields of either and decides whether the other TypeExpression is a match.
    /// <para>
    /// See also: <see cref="fennecs.Identity.Plain"/>, <see cref="fennecs.Identity.Target"/>, <see cref="fennecs.Identity.Entity"/>, <see cref="fennecs.Identity.Object"/>, <see cref="fennecs.Identity.Any"/>
    /// </para>
    /// </summary>
    /// <remarks>
    /// <para>
    /// ⚠️ This comparison is non-commutative; the order of the operands matters!
    /// </para>
    /// <para>
    /// You must handle matching the commuted case(s) in your code if needed.
    /// </para>
    /// </remarks>
    /// <example>
    /// <para>
    /// Non-Commutative: <br/><c>Match.Plain</c> doesn't match wildcard <c>Match.Any</c>, but <c>Match.Any</c> <i><b>does</b> match</i> <c>Match.Plain</c>.
    /// </para>
    /// <para>
    /// Pseudo-Commutative: <br/><see cref="Identity"/> <c>E-0000007b:00456</c> matches itself, as well as the three wildcards <c>Match.Target</c>, <c>Match.Entity</c>, and <c>Match.Any</c>. Vice versa, it is also matched by all of them! 
    /// </para>
    /// </example>
    /// <param name="other">another type expression</param>
    /// <seealso cref="fennecs.Identity.Plain"/>
    /// <seealso cref="fennecs.Identity.Target"/>
    /// <seealso cref="fennecs.Identity.Entity"/>
    /// <seealso cref="fennecs.Identity.Object"/>
    /// <seealso cref="fennecs.Identity.Any"/>
    /// <seealso cref="fennecs.Target.Relation"/>
    /// <seealso cref="fennecs.Target.Link{T}"/>
    /// <returns>true if the other expression is matched by this expression</returns>
    public bool Matches(TypeExpression other)
    {
        // Default matches nothing.
        if (this == default || other == default) return false;

        // Reject if Types are incompatible. 
        if (TypeId != other.TypeId) return false;

        // Match.None matches only None. (plain Components)
        if (Target == Wildcard.Plain) return other.Target == Wildcard.Plain;

        // Match.Any matches everything; relations and pure Components (target == none).
        if (Target == Wildcard.Any) return true;

        // Match.Target matches all Entity-Target Relations.
        if (Target == Wildcard.Target) return other.Target != Wildcard.Plain;

        // Match.Relation matches only Entity-Entity relations.
        if (Target == Wildcard.Entity) return other.Target.IsEntity;

        // Match.Object matches only Entity-Object relations.
        if (Target == Wildcard.Object) return other.Target.IsObject;

        // Direct match?
        return Target == other.Target;
    }


    /// <inheritdoc cref="System.IEquatable{T}"/>
    public bool Equals(TypeExpression other) => Value == other.Value;


    /// <inheritdoc cref="System.IComparable{T}"/>
    public int CompareTo(TypeExpression other) => Value.CompareTo(other.Value);


    ///<summary>
    /// Implements <see cref="System.IEquatable{T}"/>.Equals(object? obj)
    /// </summary>
    /// <remarks>
    /// ⚠️This method ALWAYS throws InvalidCastException, as boxing of this type is disallowed.
    /// </remarks>
    public override bool Equals(object? obj) => throw new InvalidCastException("fennecs.TypeExpression: Boxing Disallowed; use Equals(TypeExpression) instead.");


    /// <summary>
    /// Creates a new <see cref="TypeExpression"/> for a given Component type and target entity.
    /// This may express a plain Component if <paramref name="target"/> is <see cref="fennecs.Identity.Plain"/>, 
    /// or a relation if <paramref name="target"/> is a normal Entity or an object Entity obtained 
    /// from <c>Entity.Of&lt;T&gt;(T target)</c>.
    /// Providing any of the special virtual Entities <see cref="Wildcard.Any"/>, <see cref="fennecs.Identity.Target"/>,
    /// <see cref="fennecs.Identity.Entity"/>, or <see cref="fennecs.Identity.Object"/> will create a Wildcard expression.
    /// </summary>
    /// <remarks>
    /// <para>If <paramref name="target"/> is <see cref="fennecs.Identity.Plain"/>, the type expression matches a plain Component of its <see cref="Type"/>.</para>
    /// <para>If <paramref name="target"/> is <see cref="Wildcard.Any"/>, the type expression acts as a Wildcard 
    ///   expression that matches any target, INCLUDING <see cref="fennecs.Identity.Plain"/>.</para>
    /// <para> If <paramref name="target"/> is <see cref="fennecs.Identity.Target"/>, the type expression acts as a Wildcard 
    ///   expression that matches relations and their targets, EXCEPT <see cref="fennecs.Identity.Plain"/>.</para>
    /// <para> If <paramref name="target"/> is <see cref="fennecs.Identity.Entity"/>, the type expression acts as a Wildcard 
    ///   expression that matches ONLY entity-entity relations.</para>
    /// <para> If <paramref name="target"/> is <see cref="fennecs.Identity.Object"/>, the type expression acts as a Wildcard 
    ///   expression that matches ONLY entity-object relations.</para>
    /// </remarks>
    /// <typeparam name="T">The backing type for which to generate the expression.</typeparam>
    /// <param name="target">The target entity, with a default of <see cref="fennecs.Identity.Plain"/>, specifically NO target.</param>
    /// <returns>A new <see cref="TypeExpression"/> struct instance, configured according to the specified type and target.</returns>
    public static TypeExpression Of<T>(Target target) => new(target, LanguageType<T>.Id);


    /// <summary>
    /// Creates a new <see cref="TypeExpression"/> for a given Component type and target entity.
    /// This may express a plain Component if <paramref name="target"/> is <see cref="fennecs.Identity.Plain"/>, 
    /// or a relation if <paramref name="target"/> is a normal Entity or an object Entity obtained 
    /// from <c>Entity.Of&lt;T&gt;(T target)</c>.
    /// Providing any of the special virtual Entities <see cref="Wildcard.Any"/>, <see cref="fennecs.Identity.Target"/>,
    /// <see cref="fennecs.Identity.Entity"/>, or <see cref="fennecs.Identity.Object"/> will create a Wildcard expression.
    /// </summary>
    /// <remarks>
    /// <para>If <paramref name="target"/> is <see cref="fennecs.Identity.Plain"/>, the type expression matches a plain Component of its <see cref="Type"/>.</para>
    /// <para>If <paramref name="target"/> is <see cref="Wildcard.Any"/>, the type expression acts as a Wildcard 
    ///   expression that matches any Component or relation, INCLUDING <see cref="fennecs.Identity.Plain"/>.</para>
    /// <para> If <paramref name="target"/> is <see cref="fennecs.Identity.Target"/>, the type expression acts as a Wildcard 
    ///   expression that matches relations and their targets, EXCEPT <see cref="fennecs.Identity.Plain"/>.</para>
    /// <para> If <paramref name="target"/> is <see cref="fennecs.Identity.Entity"/>, the type expression acts as a Wildcard 
    ///   expression that matches ONLY entity-entity relations.</para>
    /// <para> If <paramref name="target"/> is <see cref="fennecs.Identity.Object"/>, the type expression acts as a Wildcard 
    ///   expression that matches ONLY entity-object relations.</para>
    /// </remarks>
    /// <param name="type">The Component type.</param>
    /// <param name="target">The target entity, with a default of <see cref="fennecs.Identity.Plain"/>, specifically NO target.</param>
    /// <returns>A new <see cref="TypeExpression"/> struct instance, configured according to the specified type and target.</returns>
    public static TypeExpression Of(Type type, Target target) => new(target, LanguageType.Identify(type));


    /// <summary>
    /// Implements a hash function that aims for a low collision rate.
    /// </summary>
    public override int GetHashCode()
    {
        unchecked
        {
            return (int)(0x811C9DC5u * DWordLow + 0x1000193u * DWordHigh + 0xc4ceb9fe1a85ec53u);
        }
    }


    /// <inheritdoc cref="object.ToString"/>
    public override string ToString()
    {
        if (isWildcard || isRelation) return $"<{LanguageType.Resolve(TypeId)}> >> {Target}";
        return $"<{LanguageType.Resolve(TypeId)}>";
    }


    /// <inheritdoc cref="Equals(fennecs.TypeExpression)"/>
    public static bool operator ==(TypeExpression left, TypeExpression right)
    {
        return left.Equals(right);
    }


    /// <inheritdoc cref="Equals(fennecs.TypeExpression)"/>
    public static bool operator !=(TypeExpression left, TypeExpression right)
    {
        return !(left == right);
    }


    /// <summary>
    /// Internal constructor, used by <see cref="Of{T}"/> and by unit tests.
    /// </summary>
    /// <param name="target">literal target Entity value</param>
    /// <param name="typeId">literal TypeID value</param>
    internal TypeExpression(Target target, TypeID typeId)
    {
        target.Deconstruct(out var id);
        Value = id.Value;
        TypeId = typeId;
    }

    /// <summary>
    /// Expands this TypeExpression into a set of TypeExpressions that that are Equivalent but unique.
    /// </summary>
    /// <remarks>
    /// <ul>
    /// <li>wild Any -> [ wild Plain, wild Entity, wild Object ]</li>
    /// <li>wild Target -> [ wild Entity, wild Object ]</li>
    /// <li>specific Object -> [ wild Object ] </li>
    /// <li>specific Entity -> [ wild Entity ]</li>
    /// </ul>
    /// </remarks>
    /// <returns></returns>
    public ImmutableHashSet<TypeExpression> Expand()
    {
        if (Target == Wildcard.Any) return [ this, this with { Target = Wildcard.Plain }, this with { Target = Wildcard.Entity }, this with { Target = Wildcard.Object } ];
        
        if (Target == Wildcard.Target) return [ this, this with { Target = Wildcard.Any }, this with { Target = Wildcard.Entity }, this with { Target = Wildcard.Object } ];
        
        if (Target == Wildcard.Entity) return [ this, this with { Target = Wildcard.Any }, this with { Target = Wildcard.Target }];
        
        if (Target == Wildcard.Object) return [ this, this with { Target = Wildcard.Any }, this with { Target = Wildcard.Target } ];
        
        if (Target.IsObject) return [ this, this with { Target = Wildcard.Any }, this with { Target = Wildcard.Target }, this with { Target = Wildcard.Object } ];
        
        if (Target.IsEntity) return [ this, this with { Target = Wildcard.Any }, this with { Target = Wildcard.Target }, this with { Target = Wildcard.Entity } ];
        
        return [ this, this with { Target = Wildcard.Any } ];
    }
}
