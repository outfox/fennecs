namespace fennecs;

/// <summary>
/// Objects of this type can perform Add and Remove operations on entities or sets of entities.
/// </summary>
public interface IAddRemoveComponent<out SELF>
{
    /// <summary>
    /// Add a default, Plain newable component of type C to the entity/entities.
    /// </summary>
    /// <returns>itself (fluent pattern)</returns>
    public SELF Add<C>() where C : notnull, new();

    /// <summary>
    /// Add a Plain component with value of type C to the entity/entities.
    /// </summary>
    /// <returns>itself (fluent pattern)</returns>
    public SELF Add<C>(C value) where C : notnull;

    /// <summary>
    /// Add a newable Relation component backed by a value of type R to the entity/entities. (default value)
    /// </summary>
    /// <returns>itself (fluent pattern)</returns>
    public SELF Add<T>(Entity target) where T : notnull, new();
    
    /// <summary>
    /// Add a Relation component backed by a value of type R to the entity/entities.
    /// </summary>
    /// <returns>itself (fluent pattern)</returns>
    public SELF Add<R>(R value, Entity relation) where R : notnull;


    /// <summary>
    /// Add a Object Link component with an Object of type L to the entity/entities.
    /// </summary>
    /// <returns>itself (fluent pattern)</returns>
    public SELF Add<L>(Link<L> link) where L : class;

    /// <summary>
    /// Remove a Plain component of type C from the entity/entities.
    /// </summary>
    /// <returns>itself (fluent pattern)</returns>
    public SELF Remove<C>() where C : notnull;

    /// <summary>
    /// Remove a Relation component of type R with the specified relation from the entity/entities.
    /// </summary>
    /// <returns>itself (fluent pattern)</returns>
    public SELF Remove<R>(Entity relation) where R : notnull;

    /// <summary>
    /// Remove an Object Link component with the specified linked object from the entity/entities.
    /// </summary>
    /// <returns>itself (fluent pattern)</returns>
    public SELF Remove<L>(L linkedObject) where L : class;

    /// <summary>
    /// Remove an Object Link component with the specified link from the entity/entities.
    /// </summary>
    /// <returns>itself (fluent pattern)</returns>
    public SELF Remove<L>(Link<L> link) where L : class;
}


/// <summary>
/// Objects of this type can express the presence of components on an entity or set of entities.
/// </summary>
public interface IHasComponent
{
    /// <summary>
    /// Check if the entity/entities has a Plain component of type C.
    /// </summary>
    /// <returns>true if the entity/entities has the component; otherwise, false.</returns>
    public bool Has<C>() where C : notnull;

    /// <summary>
    /// Check if the entity/entities has a Relation component of type R with the specified relation.
    /// </summary>
    /// <returns>true if the entity/entities has the component with the specified relation; otherwise, false.</returns>
    public bool Has<R>(Entity relation) where R : notnull;

    /// <summary>
    /// Check if the entity/entities has an Object Link component with the specified linked object.
    /// </summary>
    /// <returns>true if the entity/entities has the component with the specified linked object; otherwise, false.</returns>
    public bool Has<L>(L linkedObject) where L : class;

    /// <summary>
    /// Check if the entity/entities has an Object Link component with the specified link.
    /// </summary>
    /// <returns>true if the entity/entities has the component with the specified link; otherwise, false.</returns>
    public bool Has<L>(Link<L> link) where L : class;
}


/// <summary>
/// Objects of this type can perform Batch operations on entities or sets of entities.
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