namespace fennecs.CRUD;

/// <summary>
/// Objects of this type can perform Batch operations on Entities or sets of Entities.
/// </summary>
public interface IBatchBegin
{
    /// <summary>
    /// Provide a Builder Struct that allows to enqueue multiple operations on the Entities matched by this Query.
    /// </summary>
    /// <remarks>
    /// (Add, Remove, etc.) If they were applied one by one, they would cause the Entities in many cases to no longer
    /// be matched after the first operation, and thus lead to undesired no-ops.
    /// </remarks>
    /// <param name="add">Conflict Resolution for Addition of Components already on Entities encountered</param> 
    /// <param name="remove">Conflict Resolution for Removal of Components not on Entities encountered</param> 
    /// <returns>a BatchOperation that needs to be executed by calling <see cref="Batch.Submit"/></returns>
    public Batch Batch(Batch.AddConflict add, Batch.RemoveConflict remove);

    /// <inheritdoc cref="Batch(fennecs.Batch.AddConflict,fennecs.Batch.RemoveConflict)"/>
    public Batch Batch();
    
    /// <inheritdoc cref="Batch(fennecs.Batch.AddConflict,fennecs.Batch.RemoveConflict)"/>
    public Batch Batch(Batch.AddConflict add);

    /// <inheritdoc cref="Batch(fennecs.Batch.AddConflict,fennecs.Batch.RemoveConflict)"/>
    public Batch Batch(Batch.RemoveConflict remove);
    
}
