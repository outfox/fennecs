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
    [FieldOffset(0)]
    private readonly ulong _value;

    [field: FieldOffset(0)]
    internal readonly Key Key;

    [field: FieldOffset(6)] 
    internal readonly short TypeId;
    
    
    internal TypeExpression(Key key, short typeId)
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
    /// Creates a new <see cref="TypeExpression"/> for a given Component type and key.
    /// This may express a plain Component if <paramref name="key"/> is <c>default</c>/>,
    /// </summary> 
    public static TypeExpression Of<T>(Key key) => new(key, LanguageType<T>.Id);


    /// <summary>
    /// Creates a new <see cref="TypeExpression"/> for a given Component type and key.
    /// This may express a plain Component if <paramref name="key"/> is <c>default</c>/>,
    /// </summary> 
    public static TypeExpression Of(Type type, Key key) => new(key, LanguageType.Identify(type));


    /// <inheritdoc cref="object.ToString"/>
    public override string ToString()
    {
        return Key != default ? $"<{LanguageType.Resolve(TypeId)}> >> {Key}" : $"<{LanguageType.Resolve(TypeId)}>";
    }

    /// <inheritdoc />
    public int CompareTo(TypeExpression other) => _value.CompareTo(other._value);
}

