namespace fennecs.CRUD;

/// <summary>
/// Objects of this type can perform Add and Remove operations on entities or sets of entities.
/// </summary>
public interface IAddRemove<out SELF>
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
