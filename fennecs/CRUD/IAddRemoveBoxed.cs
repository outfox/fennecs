using System.Diagnostics.CodeAnalysis;

namespace fennecs.CRUD;

/// <summary>
/// Implementors can read and write boxedComponent values in boxed form.
/// </summary>
public interface IAddRemoveBoxed<out SELF>
{
    /// <summary>
    /// Typeless API: Check if the entity/entities has a Component of a specific backing type, with optional match expression for relations.
    /// </summary>
    /// <remarks>
    /// ⚠️ To differentiate from its overloads for Object Links, use default or Match.Plain for match, or Wildcards like Match.Any, Match.Plain, etc.
    /// </remarks>
    public bool Has(Type type, Key key = default);

    /// <summary>
    /// Boxes the value of a Component of a specific type, with optional match expression for relations.
    /// </summary>
    /// <param name="type">backing type of the component</param>
    /// <param name="value">boxed component value, or null if the entity does not have a component of that type</param>
    /// <param name="key">optional secondary key</param>
    /// <returns>true if the entity has a component of that type</returns>
    /// <remarks>Semantically does not support wildcards! (must identify a single specific component)</remarks>
    public bool Get([MaybeNullWhen(false)] out object value, Type type, Key key = default);
    
    /// <summary>
    /// Boxes the value of a Component of a specific type, with optional match expression for relations.
    /// </summary>
    /// <returns>boxed component value, or null if the entity does not have a component of that type</returns>
    public object? Get(Type type, Key key = default) => Get(out var value, type, key) ? value : null;
    
    /// <summary>
    /// 'Typelessly' sets the value of a Component of a specific type, with optional match expression for relations.
    /// The component type will be the type that value.GetType() returns!
    /// </summary>
    /// <param name="value">reference type or boxed component value</param>
    /// <param name="key">optional match expression for relations</param>
    /// <remarks>Semantically does not support wildcards! (must identify a single specific component)</remarks>
    /// <throws><see cref="InvalidOperationException"/>if trying to add an already existing component or
    /// <see cref="ArgumentException"/>if match is a wildcard</throws>
    public void Set(object value, Key key = default);
    
    /// <summary>
    /// Removes the given component by type and optional match expression.
    /// </summary>
    /// <param name="type">backing type of the component</param>
    /// <param name="key">optional match expression for relations</param>
    /// <throws><see cref="InvalidOperationException"/>if trying to clear a non-existing component</throws>
    public SELF Clear(Type type, Key key = default);
}
