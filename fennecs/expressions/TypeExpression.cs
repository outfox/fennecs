// SPDX-License-Identifier: MIT

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
    [FieldOffset(0)] internal readonly ulong _value;

    /// <summary>
    /// The secondary Key of this TypeExpression (expressing its relation or link target).
    /// </summary>
    [field: FieldOffset(0)]
    public readonly Key Key => new(_value);

    [field: FieldOffset(6)] 
    internal readonly short TypeId;
    
    internal TypeExpression(Type type, Key key) : this(LanguageType.Identify(type), key) { }
    
    internal TypeExpression(short typeId, Key key)
    {
        Debug.Assert(typeId != 0, "TypeId must be non-zero");
        _value = key.Value | (ulong) typeId << 48;
    }

    internal TypeExpression(ulong value)
    {
        _value = value;
    }

    /// <summary>
    /// Get the backing Component type that this <see cref="TypeExpression"/> represents.
    /// </summary>
    public Type Type => LanguageType.Resolve(TypeId);
    

    /// <summary>
    /// Creates a new <see cref="TypeExpression"/> for a given Component type and key.
    /// This may express a plain Component if <paramref name="key"/> is <c>default</c>/>,
    /// </summary> 
    public static TypeExpression Of<T>(Key key = default) => new(LanguageType<T>.Id, key);
    
    

    /// <summary>
    /// Creates a new <see cref="TypeExpression"/> for a given Component type and key.
    /// This may express a plain Component if <paramref name="key"/> is <c>default</c>/>,
    /// </summary> 
    public static TypeExpression Of(Type type, Key key = default) => new(type, key);


    /// <summary>
    /// Creates a new <see cref="TypeExpression"/> for a given object link.
    /// An object link is is a relation to the <see cref="Key.Of(object)"/> backed by the object itself..
    /// </summary> 
    public static TypeExpression Of<L>(L link) where L: class => new(typeof(L), Key.Of(link));


    /// <inheritdoc cref="object.ToString"/>
    public override string ToString()
    {
        return Key != default ? $"<{LanguageType.Resolve(TypeId)}> >> {Key}" : $"<{LanguageType.Resolve(TypeId)}>";
    }

    /// <summary>
    /// Implicitly converts a <see cref="TypeExpression"/> to a <see cref="MatchExpression"/>.
    /// </summary>
    public static implicit operator MatchExpression(TypeExpression self) => new(self);
    
    /// <summary>
    /// Implicitly converts a (Type, Key) tuple to a TypeExpression.
    /// </summary>
    public static implicit operator TypeExpression((Type type, Key key) tuple) => new(tuple.type, tuple.key);

    /// <summary>
    /// Implicitly converts a (object, Key) tuple to a TypeExpression.
    /// </summary>
    public static implicit operator TypeExpression((object obj, Key key) tuple) => new(tuple.obj.GetType(), tuple.key);
    
    
    /// <inheritdoc />
    public int CompareTo(TypeExpression other) => _value.CompareTo(other._value);

    /// <summary>
    /// Does the secondary key target an Entity?
    /// </summary>
    public bool IsRelation => Key.IsEntity;
    
    /// <summary>
    /// Does the secondary key target an Object?
    /// </summary>
    public bool IsLink => Key.IsLink;

    /// <summary>
    /// Returns the Entity targeted by this TypeExpression, if it is a relation.
    /// </summary>
    public Entity Target => IsRelation ? Key : Entity.None;
}


