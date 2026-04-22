namespace fennecs.CRUD;

/// <summary>
/// Objects of this type can perform Add and Remove operations on Entities or sets of Entities.
/// </summary>
public interface IAddRemove<out SELF>
{
    /// <summary>
    /// Add a default, Plain newable Component of type C to the Entity/Entities.
    /// </summary>
    /// <returns>itself (fluent pattern)</returns>
    public SELF Add<C>() where C : notnull, new();

    /// <summary>
    /// Add a Plain Component with value of type C to the Entity/Entities.
    /// </summary>
    /// <returns>itself (fluent pattern)</returns>
    public SELF Add<C>(C value) where C : notnull;

    /// <summary>
    /// Add a newable Relation Component backed by a value of type R to the Entity/Entities. (default value)
    /// </summary>
    /// <returns>itself (fluent pattern)</returns>
    public SELF Add<T>(Entity target) where T : notnull, new();
    
    /// <summary>
    /// Add a Relation Component backed by a value of type R to the Entity/Entities.
    /// </summary>
    /// <returns>itself (fluent pattern)</returns>
    public SELF Add<R>(R value, Entity relation) where R : notnull;


    /// <summary>
    /// Add a Object Link Component with an Object of type L to the Entity/Entities.
    /// </summary>
    /// <returns>itself (fluent pattern)</returns>
    public SELF Add<L>(Link<L> link) where L : class;

    /// <summary>
    /// Remove a Plain Component of type C from the Entity/Entities.
    /// </summary>
    /// <returns>itself (fluent pattern)</returns>
    public SELF Remove<C>() where C : notnull;

    /// <summary>
    /// Remove a Relation Component of type R with the specified relation from the Entity/Entities.
    /// </summary>
    /// <returns>itself (fluent pattern)</returns>
    public SELF Remove<R>(Entity relation) where R : notnull;

    /// <summary>
    /// Remove an Object Link Component with the specified linked object from the Entity/Entities.
    /// </summary>
    /// <returns>itself (fluent pattern)</returns>
    public SELF Remove<L>(L linkedObject) where L : class;

    /// <summary>
    /// Remove an Object Link component with the specified link from the Entity/Entities.
    /// </summary>
    /// <returns>itself (fluent pattern)</returns>
    public SELF Remove<L>(Link<L> link) where L : class;
}
