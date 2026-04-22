using System.Diagnostics.CodeAnalysis;

namespace fennecs.CRUD;

/// <summary>
/// Implementors can read and write boxedComponent values in boxed form.
/// </summary>
public interface IAddRemoveBoxed<out SELF>
{
    /// <summary>
    /// Typeless API: Check if the Entity/Entities has a Component of a specific backing type, with optional match expression for relations.
    /// </summary>
    /// <remarks>
    /// ⚠️ To differentiate from its overloads for Object Links, use default or Match.Plain for match, or Wildcards like Match.Any, Match.Plain, etc.
    /// </remarks>
    public bool Has(Type type, Match match);
    
    /// <summary>
    /// Boxes the value of a Component of a specific type, with optional match expression for relations.
    /// </summary>
    /// <param name="type">backing type of the Component</param>
    /// <param name="value">boxed Component value, or null if the Entity does not have a Component of that type</param>
    /// <param name="match">optional match expression for relations</param>
    /// <returns>true if the Entity has a Component of that type</returns>
    /// <remarks>Semantically does not support Wildcards! (must identify a single specific Component)</remarks>
    public bool Get([MaybeNullWhen(false)] out object value, Type type, Match match = default);
    
    /// <summary>
    /// Boxes the value of a Component of a specific type, with optional match expression for relations.
    /// </summary>
    /// <returns>boxed Component value, or null if the Entity does not have a component of that type</returns>
    public object? Get(Type type, Match match = default) => Get(out var value, type, match) ? value : null;
    
    /// <summary>
    /// 'Typelessly' sets the value of a Component of a specific type, with optional match expression for relations.
    /// The component type will be the type that value.GetType() returns!
    /// </summary>
    /// <param name="value">reference type or boxed component value</param>
    /// <param name="match">optional match expression for relations</param>
    /// <remarks>Semantically does not support Wildcards! (must identify a single specific component)</remarks>
    /// <throws><see cref="InvalidOperationException"/>if trying to add an already existing component or
    /// <see cref="ArgumentException"/>if match is a Wildcard</throws>
    public void Set(object value, Match match = default);
    
    /// <summary>
    /// Removes the given component by type and optional match expression.
    /// </summary>
    /// <param name="type">backing type of the component</param>
    /// <param name="match">optional match expression for relations</param>
    /// <throws><see cref="InvalidOperationException"/>if trying to clear a non-existing component</throws>
    public SELF Clear(Type type, Match match = default);
}
