using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace fennecs.CRUD;

/// <summary>
/// Objects of this type can perform Batch operations on entities or sets of entities.
/// </summary>
public interface IBatchBegin
{
    /// <summary>
    /// Provide a fluent Batch struct that allows to enqueue multiple operations on the Entities matched by this Query.
    /// </summary>
    /// <remarks>
    /// (Add, Remove, etc.) If they were applied one by one, they would cause the Entities in many cases to no longer
    /// be matched after the first operation, and thus lead to undesired no-ops.
    /// </remarks>
    /// <param name="add">Conflict Resolution for Addition of Components already on Entities encountered</param> 
    /// <param name="remove">Conflict Resolution for Removal of Components not on Entities encountered</param>
    /// <returns>a BatchOperation that needs to be executed by calling <see cref="Batch.Submit"/></returns>
    public Batch Batch(Batch.AddConflict add = default, Batch.RemoveConflict remove = default);

    /// <inheritdoc cref="Batch(fennecs.Batch.AddConflict,fennecs.Batch.RemoveConflict)"/>
    [SuppressMessage("ReSharper", "ExplicitCallerInfoArgument")]
    public Batch Batch(Batch.RemoveConflict remove) => Batch(default, remove);
}
