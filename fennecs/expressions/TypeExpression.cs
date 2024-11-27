// SPDX-License-Identifier: MIT

using System.Collections.Immutable;

namespace fennecs;

/// <summary>
/// Represents a union structure that encapsulates type expressions, including Components,
/// Entity-Entity relations, Entity-object relations, and Wildcard expressions matching multiple.
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
public readonly record struct TypeExpression : IComparable<TypeExpression>
{
    //private TypeExpression(Match match, TypeID typeId) : this(match.Value, typeId, default)
    //{ }
    
    public TypeExpression(Identity Identity, short TypeId, TypeFlags Flags)
    {
        this.Identity = Identity;
        this.TypeId = TypeId;
        this.Flags = Flags;
    }

    internal bool isUnmanaged => Flags.HasFlag(TypeFlags.Unmanaged);
    internal int SIMDsize => (int)(Flags & TypeFlags.SIMDSize);
    
    internal Relate Relation => new(Identity);

    internal Match Match => new(Identity);


    /// <summary>
    /// The <see cref="TypeExpression"/> is a relation, meaning it has a target other than None.
    /// </summary>
    public bool isRelation => TypeId != 0 && Match != Match.Plain && !Match.IsWildcard;


    /// <summary>
    ///  Is this TypeExpression a Wildcard expression? See <see cref="Cross"/>.
    /// </summary>
    public bool isWildcard => Match.IsWildcard;


    /// <summary>
    /// Get the backing Component type that this <see cref="TypeExpression"/> represents.
    /// </summary>
    public Type Type => LanguageType.Resolve(TypeId);

    public Identity Identity { get; init; }
    public short TypeId { get; }
    public TypeFlags Flags { get; }
    


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
    /// <seealso cref="Match.Relation"/>
    /// <seealso cref="Match.Link{T}"/>
    /// <returns>true if the other expression is matched by this expression</returns>
    public bool Matches(TypeExpression other)
    {
        // Reject if Types are incompatible. 
        if (TypeId != other.TypeId) return false;

        // Match.None matches only None. (plain Components)
        if (Match == Match.Plain) return other.Match == Match.Plain;

        // Match.Any matches everything; relations and pure Components (target == none).
        if (Match == Match.Any) return true;

        // Match.Target matches all Entity-Target Relations.
        if (Match == Match.Target) return other.Match != Match.Plain;

        // Match.Relation matches only Entity-Entity relations.
        if (Match == Match.Entity) return other.Match.IsEntity;

        // Match.Object matches only Entity-Object relations.
        if (Match == Match.Object) return other.Match.IsObject;

        // Direct match?
        return Match == other.Match;
    }

    /// <summary>
    /// Creates a new <see cref="TypeExpression"/> for a given Component type and target entity.
    /// This may express a plain Component if <paramref name="match"/> is <see cref="fennecs.Identity.Plain"/>, 
    /// or a relation if <paramref name="match"/> is a normal Entity or an object Entity obtained 
    /// from <c>Entity.Of&lt;T&gt;(T target)</c>.
    /// Providing any of the special virtual Entities <see cref="fennecs.Match.Any"/>, <see cref="fennecs.Identity.Target"/>,
    /// <see cref="fennecs.Identity.Entity"/>, or <see cref="fennecs.Identity.Object"/> will create a Wildcard expression.
    /// </summary>
    /// <remarks>
    /// <para>If <paramref name="match"/> is <see cref="fennecs.Identity.Plain"/>, the type expression matches a plain Component of its <see cref="Type"/>.</para>
    /// <para>If <paramref name="match"/> is <see cref="fennecs.Match.Any"/>, the type expression acts as a Wildcard 
    ///   expression that matches any target, INCLUDING <see cref="fennecs.Identity.Plain"/>.</para>
    /// <para> If <paramref name="match"/> is <see cref="fennecs.Identity.Target"/>, the type expression acts as a Wildcard 
    ///   expression that matches relations and their targets, EXCEPT <see cref="fennecs.Identity.Plain"/>.</para>
    /// <para> If <paramref name="match"/> is <see cref="fennecs.Identity.Entity"/>, the type expression acts as a Wildcard 
    ///   expression that matches ONLY entity-entity relations.</para>
    /// <para> If <paramref name="match"/> is <see cref="fennecs.Identity.Object"/>, the type expression acts as a Wildcard 
    ///   expression that matches ONLY entity-object relations.</para>
    /// </remarks>
    /// <typeparam name="T">The backing type for which to generate the expression.</typeparam>
    /// <param name="match">The target entity, with a default of <see cref="fennecs.Identity.Plain"/>, specifically NO target.</param>
    /// <returns>A new <see cref="TypeExpression"/> struct instance, configured according to the specified type and target.</returns>
    public static TypeExpression Of<T>(Match match) => new(match.Value, LanguageType<T>.Id, LanguageType.FlagsOf<T>());


    /// <summary>
    /// Creates a new <see cref="TypeExpression"/> for a given Component type and target entity.
    /// This may express a plain Component if <paramref name="match"/> is <see cref="fennecs.Identity.Plain"/>, 
    /// or a relation if <paramref name="match"/> is a normal Entity or an object Entity obtained 
    /// from <c>Entity.Of&lt;T&gt;(T target)</c>.
    /// Providing any of the special virtual Entities <see cref="fennecs.Match.Any"/>, <see cref="fennecs.Identity.Target"/>,
    /// <see cref="fennecs.Identity.Entity"/>, or <see cref="fennecs.Identity.Object"/> will create a Wildcard expression.
    /// </summary>
    /// <remarks>
    /// <para>If <paramref name="match"/> is <see cref="fennecs.Identity.Plain"/>, the type expression matches a plain Component of its <see cref="Type"/>.</para>
    /// <para>If <paramref name="match"/> is <see cref="fennecs.Match.Any"/>, the type expression acts as a Wildcard 
    ///   expression that matches any Component or relation, INCLUDING <see cref="fennecs.Identity.Plain"/>.</para>
    /// <para> If <paramref name="match"/> is <see cref="fennecs.Identity.Target"/>, the type expression acts as a Wildcard 
    ///   expression that matches relations and their targets, EXCEPT <see cref="fennecs.Identity.Plain"/>.</para>
    /// <para> If <paramref name="match"/> is <see cref="fennecs.Identity.Entity"/>, the type expression acts as a Wildcard 
    ///   expression that matches ONLY entity-entity relations.</para>
    /// <para> If <paramref name="match"/> is <see cref="fennecs.Identity.Object"/>, the type expression acts as a Wildcard 
    ///   expression that matches ONLY entity-object relations.</para>
    /// </remarks>
    /// <param name="type">The Component type.</param>
    /// <param name="match">The target entity, with a default of <see cref="fennecs.Identity.Plain"/>, specifically NO target.</param>
    /// <returns>A new <see cref="TypeExpression"/> struct instance, configured according to the specified type and target.</returns>
    public static TypeExpression Of(Type type, Match match) => new(match.Value, LanguageType.Identify(type), LanguageType.Flags(type));


    /// <inheritdoc cref="object.ToString"/>
    public override string ToString()
    {
        if (isWildcard || isRelation) return $"<{LanguageType.Resolve(TypeId)}> >> {Match}";
        return $"<{LanguageType.Resolve(TypeId)}>";
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
    public ImmutableHashSet<TypeExpression> Expand()
    {
        if (Match == Match.Any) return [this with { Identity = default }, this with { Identity = Identity.Entity }, this with { Identity = Identity.Object }, this with { Identity = Identity.Target }];

        if (Match == Match.Target) return [this with { Identity = Identity.Any }, this with { Identity = Identity.Entity }, this with { Identity = Identity.Object }];

        if (Match == Match.Entity) return [this with { Identity = Identity.Any }, this with { Identity = Identity.Target }];

        if (Match == Match.Object) return [this with { Identity = Identity.Any }, this with { Identity = Identity.Target }];

        if (Match.IsObject) return [this with { Identity = Identity.Any }, this with { Identity = Identity.Target }, this with { Identity = Identity.Object }];

        if (Match.IsEntity) return [this with { Identity = Identity.Any }, this with { Identity = Identity.Target }, this with { Identity = Identity.Entity }];

        return [this with { Identity = Identity.Any }];
    }


    /// <inheritdoc />
    public int CompareTo(TypeExpression other)
    {
        var typeComparison = TypeId.CompareTo(other.TypeId);
        if (typeComparison != 0)
        {
            return typeComparison;
        }

        var identityComparison = Identity.CompareTo(other.Identity);
        return identityComparison;
    }
}

