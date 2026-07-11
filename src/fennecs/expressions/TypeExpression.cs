// SPDX-License-Identifier: MIT

using System.Collections.Immutable;
using System.Diagnostics;

namespace fennecs;

/// <summary>
/// A packed type expression that encapsulates a Component type together with its secondary
/// <see cref="Key"/>: plain Components, Entity-Entity relations, Entity-Object links, and
/// Wildcard expressions matching multiple of these.
/// </summary>
/// <remarks>
/// Layout (single 64-bit value):
/// <code>
/// [PrimaryKind:4 (63..60)] [TypeId:12 (59..48)] [Key:48 (47..0)]
/// </code>
/// <para>If the <see cref="Key"/> is <see cref="fennecs.Key.Plain"/> (default), the expression matches a plain Component of its <see cref="Type"/>.</para>
/// <para>If the Key is specific (<see cref="fennecs.Key.IsEntity"/> or <see cref="fennecs.Key.IsObject"/>), the expression is a relation targeting that Entity or Object.</para>
/// <para>If the Key is <see cref="fennecs.Key.Any"/>, the expression is a Wildcard matching any target, INCLUDING Plain.</para>
/// <para>If the Key is <see cref="fennecs.Key.Target"/>, the expression is a Wildcard matching relations and links, EXCEPT Plain.</para>
/// <para>If the Key is <see cref="fennecs.Key.AnyEntity"/> / <see cref="fennecs.Key.AnyObject"/>, the expression is a Wildcard matching ONLY Entity relations / Object links.</para>
/// </remarks>
internal readonly record struct TypeExpression : IComparable<TypeExpression>
{
    private readonly ulong _value;

    /// <summary>PrimaryKind + TypeId — the bits that must be equal for two expressions to describe the same Component type.</summary>
    private const ulong TypeHeaderMask = Key.HeaderMask;


    internal TypeExpression(ulong value) => _value = value;

    internal TypeExpression(PrimaryKind kind, TypeID typeId, Key key)
    {
        Debug.Assert(typeId > 0, "TypeExpression must have a TypeId.");
        Debug.Assert((typeId & ~0xFFF) == 0, "TypeId must fit in 12 bits.");
        _value = ((ulong) kind << 60) | ((ulong) (ushort) typeId << 48) | key.Value;
    }


    /// <summary>The storage kind of this expression. (always <see cref="PrimaryKind.Data"/> today)</summary>
    internal PrimaryKind Kind => (PrimaryKind) (_value >> 60);

    /// <summary>The TypeId of the backing Component type.</summary>
    public TypeID TypeId => (TypeID) ((_value >> 48) & 0xFFF);

    /// <summary>The secondary Key of this expression (its relation or link target, or a Wildcard).</summary>
    internal Key Key => new(_value & Key.Mask);

    /// <summary>The Key as a Match term.</summary>
    internal Match Match => new(Key);

    /// <summary>The relation target of this expression, for relation reverse-lookups.</summary>
    internal Key Relation => Key;


    /// <summary>
    /// The <see cref="TypeExpression"/> is a relation, meaning it has a specific target (an Entity or an Object).
    /// </summary>
    public bool isRelation => Key.IsRelation;

    /// <summary>
    /// Is this TypeExpression a Wildcard expression? See <see cref="Cross"/>.
    /// </summary>
    public bool isWildcard => Key.IsWildcard;

    internal bool isUnmanaged => LanguageType.FlagsById(TypeId).HasFlag(TypeFlags.Unmanaged);

    internal int SIMDsize => (int) (LanguageType.FlagsById(TypeId) & TypeFlags.SIMDSize);

    /// <summary>
    /// Get the backing Component type that this <see cref="TypeExpression"/> represents.
    /// </summary>
    public Type Type => LanguageType.Resolve(TypeId);


    /// <summary>
    /// TODO: Remove me.
    /// A method to check if a TypeExpression matches any of the given type expressions in an IEnumerable.
    /// Does this <see cref="TypeExpression"/> match any of the given type expressions?
    /// </summary>
    /// <param name="other">a collection of type expressions</param>
    /// <returns>true if matched</returns>
    public bool Matches(IEnumerable<TypeExpression> other)
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

    /// <summary>
    /// Fast O(1) Matching against (expanded) Signature.
    /// </summary>
    /// <remarks>
    /// The other signature must be a Wildcard-Expanded signature.
    /// </remarks>
    public bool Matches(Signature expandedSignature) => expandedSignature.Matches(this);


    /// <summary>
    /// Match against another TypeExpression; used for Query Matching.
    /// Examines the Type and Key fields of either and decides whether the other TypeExpression is a match.
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
    /// Non-Commutative: <br/><c>Match.Plain</c> doesn't match Wildcard <c>Match.Any</c>, but <c>Match.Any</c> <i><b>does</b> match</i> <c>Match.Plain</c>.
    /// </para>
    /// <para>
    /// Pseudo-Commutative: <br/>A specific Entity key matches itself, as well as the Wildcards <c>Match.Target</c>, <c>Match.Entity</c>, and <c>Match.Any</c>. Vice versa, it is also matched by all of them!
    /// </para>
    /// </example>
    /// <param name="other">another type expression</param>
    /// <returns>true if the other expression is matched by this expression</returns>
    public bool Matches(TypeExpression other)
    {
        // Reject if Types (or storage kinds) are incompatible.
        if ((_value & TypeHeaderMask) != (other._value & TypeHeaderMask)) return false;

        var key = Key;
        var otherKey = other.Key;

        // Plain matches only Plain.
        if (key == default) return otherKey == default;

        // A specific target matches only exactly itself.
        if (!key.IsWildcard) return key == otherKey;

        return key.Kind switch
        {
            // Any matches everything: relations, links, and Plain.
            SecondaryKind.Any => true,

            // Target matches all specific targets and non-plain Wildcards.
            SecondaryKind.Target => otherKey != default,

            // Entity/Object Wildcards match only specific keys of their category.
            SecondaryKind.Entity => otherKey.IsEntity,
            SecondaryKind.Object => otherKey.IsObject,

            _ => key == otherKey,
        };
    }

    /// <summary>
    /// Creates a new <see cref="TypeExpression"/> for a given Component type and Match term.
    /// This may express a plain Component if <paramref name="match"/> is <see cref="fennecs.Match.Plain"/>,
    /// a relation if it targets a specific Entity or Object, or a Wildcard expression for
    /// <see cref="fennecs.Match.Any"/>, <see cref="fennecs.Match.Target"/>, <see cref="fennecs.Match.Entity"/>,
    /// or <see cref="fennecs.Match.Object"/>.
    /// </summary>
    /// <typeparam name="T">The backing type for which to generate the expression.</typeparam>
    /// <param name="match">The Match term, with a default of <see cref="fennecs.Match.Plain"/>, specifically NO target.</param>
    /// <returns>A new <see cref="TypeExpression"/> struct instance, configured according to the specified type and target.</returns>
    public static TypeExpression Of<T>(Match match) => new(PrimaryKind.Data, LanguageType<T>.Id, match.Value);


    /// <inheritdoc cref="Of{T}(fennecs.Match)"/>
    /// <param name="type">The Component type.</param>
    /// <param name="match">The Match term, with a default of <see cref="fennecs.Match.Plain"/>, specifically NO target.</param>
    public static TypeExpression Of(Type type, Match match) => new(PrimaryKind.Data, LanguageType.Identify(type), match.Value);


    /// <inheritdoc cref="object.ToString"/>
    public override string ToString()
    {
        if (isWildcard || isRelation) return $"<{LanguageType.Resolve(TypeId)}> >> {Match}";
        return $"<{LanguageType.Resolve(TypeId)}>";
    }

    /// <summary>
    /// Replaces the Key of this expression, keeping its type header.
    /// </summary>
    internal TypeExpression WithKey(Key key) => new((_value & TypeHeaderMask) | key.Value);

    /// <summary>
    /// Expands this TypeExpression into a set of TypeExpressions that are Equivalent but unique.
    /// </summary>
    /// <remarks>
    /// <ul>
    /// <li>wild Any -> [ Plain, wild Entity, wild Object, wild Target ]</li>
    /// <li>wild Target -> [ wild Any, wild Entity, wild Object ]</li>
    /// <li>wild Entity / wild Object -> [ wild Any, wild Target ]</li>
    /// <li>specific Object -> [ wild Any, wild Target, wild Object ]</li>
    /// <li>specific Entity -> [ wild Any, wild Target, wild Entity ]</li>
    /// <li>Plain -> [ wild Any ]</li>
    /// </ul>
    /// </remarks>
    public ImmutableHashSet<TypeExpression> Expand()
    {
        var key = Key;

        if (key == Key.Any) return [WithKey(default), WithKey(Key.AnyEntity), WithKey(Key.AnyObject), WithKey(Key.Target)];

        if (key == Key.Target) return [WithKey(Key.Any), WithKey(Key.AnyEntity), WithKey(Key.AnyObject)];

        if (key == Key.AnyEntity) return [WithKey(Key.Any), WithKey(Key.Target)];

        if (key == Key.AnyObject) return [WithKey(Key.Any), WithKey(Key.Target)];

        if (key.IsObject) return [WithKey(Key.Any), WithKey(Key.Target), WithKey(Key.AnyObject)];

        if (key.IsEntity) return [WithKey(Key.Any), WithKey(Key.Target), WithKey(Key.AnyEntity)];

        return [WithKey(Key.Any)];
    }


    /// <inheritdoc />
    public int CompareTo(TypeExpression other) => _value.CompareTo(other._value);
}
