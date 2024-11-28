// SPDX-License-Identifier: MIT

using System.Collections.Immutable;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace fennecs;

/// <summary>
/// Represents a union structure that encapsulates type expressions, including Components,
/// Entity-Entity relations, Entity-object relations, and Wildcard expressions matching multiple.
/// The Target of this <see cref="TypeExpression"/>, determining whether it acts as a plain Component,
/// an Object Link, an Entity Relation, or a Wildcard Match Expression.
/// </summary>
[StructLayout(LayoutKind.Explicit)]
public readonly record struct TypeExpression : IComparable<TypeExpression>
{
    [FieldOffset(0)]
    private readonly ulong _value;
    
    [field: FieldOffset(0)] 
    internal Key Key { get; }
    
    [field: FieldOffset(6)] 
    internal short TypeId { get; }
    
    
    public TypeExpression(Key key, short typeId)
    {
        Debug.Assert(typeId != 0, "TypeId must be non-zero");
        Key = key;
        TypeId = typeId;
    }

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
    public static TypeExpression Of<T>(Match match) => new(match, LanguageType<T>.Id);


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
    public static TypeExpression Of(Type type, Match match) => new(match, LanguageType.Identify(type));


    /// <inheritdoc cref="object.ToString"/>
    public override string ToString()
    {
        if (isWildcard || isRelation) return $"<{LanguageType.Resolve(TypeId)}> >> {Key}";
        return $"<{LanguageType.Resolve(TypeId)}>";
    }


    /// <inheritdoc />
    public int CompareTo(TypeExpression other) => _value.CompareTo(other._value);
}

