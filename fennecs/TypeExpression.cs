// SPDX-License-Identifier: MIT

using System.Runtime.InteropServices;

namespace fennecs;

/// <summary>
/// Represents a union structure that encapsulates type expressions, including components,
/// entity-entity relations, entity-object relations, and wildcard expressions matching multiple.
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
    /// The target of this <see cref="TypeExpression"/>, determining whether it is a plain component,
    /// a relation, or a wildcard expression.
    /// </summary>
    /// <remarks>
    /// <para>If <see cref="Entity.None"/>, the type expression matches a plain component of its <see cref="Type"/>.</para>
    /// <para>If a specific <see cref="Entity"/> (e.g. <see cref="Entity.IsEntity"/> or <see cref="Entity.IsObject"/> are true), the type expression represents a relation targeting that Entity.</para>
    /// <para>If <see cref="Entity.Any"/>, the type expression acts as a wildcard 
    ///   expression that matches any target, INCLUDING <see cref="Entity.None"/>.</para>
    /// <para> If <see cref="Entity.Target"/>, the type expression acts as a wildcard 
    ///   expression that matches relations and their targets, EXCEPT <see cref="Entity.None"/>.</para>
    /// <para> If <see cref="Entity.Relation"/>, the type expression acts as a wildcard 
    ///   expression that matches ONLY entity-entity relations.</para>
    /// <para> If <see cref="Entity.Object"/>, the type expression acts as a wildcard 
    ///   expression that matches ONLY entity-object relations.</para>
    /// </remarks>
    public Entity Target => new(Id, Decoration);
    
    /// <summary>
    /// The <see cref="TypeExpression"/> is a relation, meaning it has a target other than None.
    /// </summary>
    public bool isRelation => TypeId != 0 && Target != Entity.None;

    /// <summary>
    /// Get the backing component type that this <see cref="TypeExpression"/> represents.
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
        return other.Any(type => self.Matches(type));
    }
    
    /// <summary>
    /// Match against another TypeExpression; used for Query Matching, and is Non-Commutative.
    /// Examines the Target field and decides whether the other TypeExpression is a match.
    /// </summary>
    /// <param name="other">another type expression</param>
    /// <returns>true if matched</returns>
    public bool Matches(TypeExpression other)
    {
        // Reject if Types are incompatible. 
        if (TypeId != other.TypeId) return false;

        // Entity.None matches only None. (plain components)
        if (Target == Entity.None) return other.Target == Entity.None;

        // Entity.Any matches everything; relations and pure components (target == none).
        if (Target == Entity.Any) return true;
        
        // Entity.Target matches all Entity-Target Relations.
        if (Target == Entity.Target) return other.Target != Entity.None;
        
        // Entity.Relation matches only Entity-Entity relations.
        if (Target == Entity.Relation) return other.Target.IsEntity;
        
        // Entity.Object matches only Entity-Object relations.
        if (Target == Entity.Object) return other.Target.IsObject;

        // Direct match?
        return Target == other.Target;
    }

    /// <inheritdoc cref="IEquatable{T}.Equals(T?)"/>
    public bool Equals(TypeExpression other) => Value == other.Value;

    /// <inheritdoc cref="IComparable{T}.CompareTo"/>
    public int CompareTo(TypeExpression other) => Value.CompareTo(other.Value);

    ///<summary>
    /// Implements <see cref="IEquatable{T}.Equals(object?)"/>
    /// </summary>
    /// <remarks>
    /// ⚠️This method ALWAYS throws InvalidCastException, as boxing of this type is disallowed.
    /// </remarks>
    public override bool Equals(object? obj) => throw new InvalidCastException("Boxing Disallowed; use TypeId.Equals(TypeId) instead.");

    /// <summary>
    /// Creates a new <see cref="TypeExpression"/> for a given component type and target entity.
    /// This may express a plain component if <paramref name="target"/> is <see cref="Entity.None"/>, 
    /// or a relation if <paramref name="target"/> is a normal Entity or an object Entity obtained 
    /// from <c>Entity.Of&lt;T&gt;(T target)</c>.
    /// Providing any of the special virtual Entities <see cref="Entity.Any"/>, <see cref="Entity.Target"/>,
    /// <see cref="Entity.Relation"/>, or <see cref="Entity.Object"/> will create a wildcard expression.
    /// </summary>
    /// <remarks>
    /// <para>If <paramref name="target"/> is <see cref="Entity.None"/>, the type expression matches a plain component of its <see cref="Type"/>.</para>
    /// <para>If <paramref name="target"/> is <see cref="Entity.Any"/>, the type expression acts as a wildcard 
    ///   expression that matches any target, INCLUDING <see cref="Entity.None"/>.</para>
    /// <para> If <paramref name="target"/> is <see cref="Entity.Target"/>, the type expression acts as a wildcard 
    ///   expression that matches relations and their targets, EXCEPT <see cref="Entity.None"/>.</para>
    /// <para> If <paramref name="target"/> is <see cref="Entity.Relation"/>, the type expression acts as a wildcard 
    ///   expression that matches ONLY entity-entity relations.</para>
    /// <para> If <paramref name="target"/> is <see cref="Entity.Object"/>, the type expression acts as a wildcard 
    ///   expression that matches ONLY entity-object relations.</para>
    /// </remarks>
    /// <typeparam name="T">The backing type for which to generate the expression.</typeparam>
    /// <param name="target">The target entity, with a default of <see cref="Entity.None"/>, specifically NO target.</param>
    /// <returns>A new <see cref="TypeExpression"/> struct instance, configured according to the specified type and target.</returns>
    public static TypeExpression Create<T>(Entity target = default)
    {
        return new TypeExpression(target, LanguageType<T>.Id);
    }

    /// <summary>
    /// Creates a new <see cref="TypeExpression"/> for a given component type and target entity.
    /// This may express a plain component if <paramref name="target"/> is <see cref="Entity.None"/>, 
    /// or a relation if <paramref name="target"/> is a normal Entity or an object Entity obtained 
    /// from <c>Entity.Of&lt;T&gt;(T target)</c>.
    /// Providing any of the special virtual Entities <see cref="Entity.Any"/>, <see cref="Entity.Target"/>,
    /// <see cref="Entity.Relation"/>, or <see cref="Entity.Object"/> will create a wildcard expression.
    /// </summary>
    /// <remarks>
    /// <para>If <paramref name="target"/> is <see cref="Entity.None"/>, the type expression matches a plain component of its <see cref="Type"/>.</para>
    /// <para>If <paramref name="target"/> is <see cref="Entity.Any"/>, the type expression acts as a wildcard 
    ///   expression that matches any component or relation, INCLUDING <see cref="Entity.None"/>.</para>
    /// <para> If <paramref name="target"/> is <see cref="Entity.Target"/>, the type expression acts as a wildcard 
    ///   expression that matches relations and their targets, EXCEPT <see cref="Entity.None"/>.</para>
    /// <para> If <paramref name="target"/> is <see cref="Entity.Relation"/>, the type expression acts as a wildcard 
    ///   expression that matches ONLY entity-entity relations.</para>
    /// <para> If <paramref name="target"/> is <see cref="Entity.Object"/>, the type expression acts as a wildcard 
    ///   expression that matches ONLY entity-object relations.</para>
    /// </remarks>
    /// <param name="type">The component type.</param>
    /// <param name="target">The target entity, with a default of <see cref="Entity.None"/>, specifically NO target.</param>
    /// <returns>A new <see cref="TypeExpression"/> struct instance, configured according to the specified type and target.</returns>
    public static TypeExpression Create(Type type, Entity target = default)
    {
        return new TypeExpression(target, LanguageType.Identify(type));
    }
    
    /// <summary>
    /// Implements a hash function that aims for a low collision rate.
    /// </summary>
    public override int GetHashCode()
    {
        unchecked
        {
            return (int) (0x811C9DC5u * DWordLow + 0x1000193u * DWordHigh + 0xc4ceb9fe1a85ec53u);
        }
    }

    /// <inheritdoc cref="object.ToString"/>
    public override string ToString()
    {
        return $"<{LanguageType.Resolve(TypeId)}>\u2192{Target}>";
    }

    public static bool operator ==(TypeExpression left, TypeExpression right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(TypeExpression left, TypeExpression right)
    {
        return !(left == right);
    }

    /// <summary>
    /// Internal constructor, used by <see cref="Create{T}"/> and by unit tests.
    /// </summary>
    /// <param name="target">literal target Entity value</param>
    /// <param name="typeId">literal TypeID value</param>
    internal TypeExpression(Entity target, TypeID typeId)
    {
        Value = target.Value;
        TypeId = typeId;
    }
}