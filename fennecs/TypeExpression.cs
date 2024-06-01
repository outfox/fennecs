// SPDX-License-Identifier: MIT

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

    #endregion


    /// <summary>
    /// The Target of this <see cref="TypeExpression"/>, determining whether it acts as a plain Component,
    /// an Object Link, an Entity Relation, or a Wildcard Match Expression.
    /// </summary>
    /// <remarks>
    /// <para>If <see cref="Match.Plain"/>, the type expression matches a plain Component of its <see cref="Type"/>.</para>
    /// <para>If a specific <see cref="Identity"/> (e.g. <see cref="Identity.IsEntity"/> or <see cref="Identity.IsObject"/> are true), the type expression represents a relation targeting that Entity.</para>
    /// <para>If <see cref="Match.Any"/>, the type expression acts as a Wildcard 
    ///   expression that matches any target, INCLUDING <see cref="Match.Plain"/>.</para>
    /// <para> If <see cref="Match.Target"/>, the type expression acts as a Wildcard 
    ///   expression that matches relations and their targets, EXCEPT <see cref="Match.Plain"/>.</para>
    /// <para> If <see cref="Match.Entity"/>, the type expression acts as a Wildcard 
    ///   expression that matches ONLY Entity-entity relations.</para>
    /// <para> If <see cref="Match.Object"/>, the type expression acts as a Wildcard 
    ///   expression that matches ONLY entity-object relations.</para>
    /// </remarks>
    public Match Target => new(new(Id, Decoration));
    
    [Obsolete("Needs refactoring out...")]
    internal Identity Identity => new(Id, Decoration);

    /// <summary>
    /// The <see cref="TypeExpression"/> is a relation, meaning it has a target other than None.
    /// </summary>
    public bool isRelation => TypeId != 0 && Target != Match.Plain && !Target.IsWildcard;


    /// <summary>
    ///  Is this TypeExpression a Wildcard expression? See <see cref="Cross"/>.
    /// </summary>
    public bool isWildcard => Target.IsWildcard;


    /// <summary>
    /// Get the backing Component type that this <see cref="TypeExpression"/> represents.
    /// </summary>
    public Type Type => LanguageType.Resolve(TypeId);


    /// <summary>
    /// Does this <see cref="TypeExpression"/> match any of the given type expressions?
    /// </summary>
    /// <param name="other">a collection of type expressions</param>
    /// <returns>true if matched</returns>
    public bool Matches(IEnumerable<TypeExpression> other)
    {
        var self = this;

        foreach (var type in other)
        {
            if (self.Matches(type)) return true;
            if (type.Matches(self)) return true;
        }

        return false;
    }


    /// <summary>
    /// Match against another TypeExpression; used for Query Matching.
    /// Examines the Type and Target fields of either and decides whether the other TypeExpression is a match.
    /// <para>
    /// See also: <see cref="Match.Plain"/>, <see cref="Match.Target"/>, <see cref="Match.Entity"/>, <see cref="Match.Object"/>, <see cref="Match.Any"/>
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
    /// <seealso cref="Match.Plain"/>
    /// <seealso cref="Match.Target"/>
    /// <seealso cref="Match.Entity"/>
    /// <seealso cref="Match.Object"/>
    /// <seealso cref="Match.Any"/>
    /// <seealso cref="Match.Relation"/>
    /// <seealso cref="Match.Link{T}"/>
    /// <returns>true if the other expression is matched by this expression</returns>
    public bool Matches(TypeExpression other)
    {
        // Reject if Types are incompatible. 
        if (TypeId != other.TypeId) return false;

        // Match.None matches only None. (plain Components)
        if (Target == Match.Plain) return other.Target == Match.Plain;

        // Match.Any matches everything; relations and pure Components (target == none).
        if (Target == Match.Any) return true;

        // Match.Target matches all Entity-Target Relations.
        if (Target == Match.Target) return other.Target != Match.Plain;

        // Match.Relation matches only Entity-Entity relations.
        if (Target == Match.Entity) return other.Target.IsEntity;

        // Match.Object matches only Entity-Object relations.
        if (Target == Match.Object) return other.Target.IsObject;

        // Direct match?
        return Target == other.Target;
    }


    /// <inheritdoc cref="System.IEquatable{T}"/>
    public bool Equals(TypeExpression other) => Value == other.Value;


    /// <inheritdoc cref="System.IComparable{T}"/>
    public int CompareTo(TypeExpression other) => -Value.CompareTo(other.Value);


    ///<summary>
    /// Implements <see cref="System.IEquatable{T}"/>.Equals(object? obj)
    /// </summary>
    /// <remarks>
    /// ⚠️This method ALWAYS throws InvalidCastException, as boxing of this type is disallowed.
    /// </remarks>
    public override bool Equals(object? obj) => throw new InvalidCastException("fennecs.TypeExpression: Boxing Disallowed; use Equals(TypeExpression) instead.");


    /// <summary>
    /// Creates a new <see cref="TypeExpression"/> for a given Component type and target entity.
    /// This may express a plain Component if <paramref name="target"/> is <see cref="Match.Plain"/>, 
    /// or a relation if <paramref name="target"/> is a normal Entity or an object Entity obtained 
    /// from <c>Entity.Of&lt;T&gt;(T target)</c>.
    /// Providing any of the special virtual Entities <see cref="Match.Any"/>, <see cref="Match.Target"/>,
    /// <see cref="Match.Entity"/>, or <see cref="Match.Object"/> will create a Wildcard expression.
    /// </summary>
    /// <remarks>
    /// <para>If <paramref name="target"/> is <see cref="Match.Plain"/>, the type expression matches a plain Component of its <see cref="Type"/>.</para>
    /// <para>If <paramref name="target"/> is <see cref="Match.Any"/>, the type expression acts as a Wildcard 
    ///   expression that matches any target, INCLUDING <see cref="Match.Plain"/>.</para>
    /// <para> If <paramref name="target"/> is <see cref="Match.Target"/>, the type expression acts as a Wildcard 
    ///   expression that matches relations and their targets, EXCEPT <see cref="Match.Plain"/>.</para>
    /// <para> If <paramref name="target"/> is <see cref="Match.Entity"/>, the type expression acts as a Wildcard 
    ///   expression that matches ONLY entity-entity relations.</para>
    /// <para> If <paramref name="target"/> is <see cref="Match.Object"/>, the type expression acts as a Wildcard 
    ///   expression that matches ONLY entity-object relations.</para>
    /// </remarks>
    /// <typeparam name="T">The backing type for which to generate the expression.</typeparam>
    /// <param name="target">The target entity, with a default of <see cref="Match.Plain"/>, specifically NO target.</param>
    /// <returns>A new <see cref="TypeExpression"/> struct instance, configured according to the specified type and target.</returns>
    public static TypeExpression Of<T>(Match target) => new(target, LanguageType<T>.Id);

    //public static TypeExpression Of<T>(T data) where T : class => new(Link.With<T>(data), LanguageType<T>.Id);

    /// <inheritdoc cref="Of{T}(fennecs.Match)"/>
    //public static TypeExpression Of<T>(Entity entity) => new(entity, LanguageType<T>.Id);    

    /// <inheritdoc cref="Of{T}(fennecs.Match)"/>
    public static TypeExpression Of<T>() => new(Match.Plain, LanguageType<T>.Id);


    /// <summary>
    /// Creates a new <see cref="TypeExpression"/> for a given Component type and target entity.
    /// This may express a plain Component if <paramref name="target"/> is <see cref="Match.Plain"/>, 
    /// or a relation if <paramref name="target"/> is a normal Entity or an object Entity obtained 
    /// from <c>Entity.Of&lt;T&gt;(T target)</c>.
    /// Providing any of the special virtual Entities <see cref="Match.Any"/>, <see cref="Match.Target"/>,
    /// <see cref="Match.Entity"/>, or <see cref="Match.Object"/> will create a Wildcard expression.
    /// </summary>
    /// <remarks>
    /// <para>If <paramref name="target"/> is <see cref="Match.Plain"/>, the type expression matches a plain Component of its <see cref="Type"/>.</para>
    /// <para>If <paramref name="target"/> is <see cref="Match.Any"/>, the type expression acts as a Wildcard 
    ///   expression that matches any Component or relation, INCLUDING <see cref="Match.Plain"/>.</para>
    /// <para> If <paramref name="target"/> is <see cref="Match.Target"/>, the type expression acts as a Wildcard 
    ///   expression that matches relations and their targets, EXCEPT <see cref="Match.Plain"/>.</para>
    /// <para> If <paramref name="target"/> is <see cref="Match.Entity"/>, the type expression acts as a Wildcard 
    ///   expression that matches ONLY entity-entity relations.</para>
    /// <para> If <paramref name="target"/> is <see cref="Match.Object"/>, the type expression acts as a Wildcard 
    ///   expression that matches ONLY entity-object relations.</para>
    /// </remarks>
    /// <param name="type">The Component type.</param>
    /// <param name="target">The target entity, with a default of <see cref="Match.Plain"/>, specifically NO target.</param>
    /// <returns>A new <see cref="TypeExpression"/> struct instance, configured according to the specified type and target.</returns>
    public static TypeExpression Of(Type type, Match target)
    {
        return new TypeExpression(target, LanguageType.Identify(type));
    }

    /// <inheritdoc cref="Of(System.Type,fennecs.Identity)"/>
    public static TypeExpression Of(Type type)
    {
        return new(Match.Plain, LanguageType.Identify(type));
    }


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
    internal TypeExpression(Match target, TypeID typeId)
    {
        target.Deconstruct(out var id);
        Value = id.Value;
        TypeId = typeId;
    }
}
