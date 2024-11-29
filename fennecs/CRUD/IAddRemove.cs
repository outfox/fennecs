namespace fennecs.CRUD;

/// <summary>
/// Objects of this type can perform Add and Remove operations on entities or sets of entities.
/// </summary>
public interface IAddRemove<out SELF>
{
    /// <summary>
    /// Add a Plain component with value of type C to the entity/entities.
    /// </summary>
    /// <returns>itself (fluent pattern)</returns>
    public SELF Add<C>(C component, Key key = default) where C : notnull;

    /// <summary>
    /// Remove a Plain component of type C from the entity/entities.
    /// </summary>
    /// <returns>itself (fluent pattern)</returns>
    public SELF Remove<C>(Key key = default) where C : notnull;


    #region Convenience Defaults
    
    /// <summary>
    /// Add a default, Plain newable component of type C to the entity/entities.
    /// </summary>
    /// <returns>itself (fluent pattern)</returns>
    public SELF Add<C>(Key key = default) where C : notnull, new() => Add(new C(), key);
    
    /// <summary>
    /// Add a newable Relation component backed by a value of type R to the entity/entities. (default value)
    /// </summary>
    /// <returns>itself (fluent pattern)</returns>
    public SELF Relate<R>(Entity target) where R : notnull, new() => Add(new R(), target.Key);

    /// <summary>
    /// Add a Relation component backed by a value of type R to the entity/entities.
    /// </summary>
    /// <returns>itself (fluent pattern)</returns>
    public SELF Relate<R>(R component, Entity target) where R : notnull => Add(component, target.Key);

    /// <summary>
    /// Add a newable Relation component backed by a value of type R to the entity/entities. (default value)
    /// </summary>
    /// <returns>itself (fluent pattern)</returns>
    public SELF Unrelate<R>(Entity target) where R : notnull => Remove<R>(target.Key);

    /// <summary>
    /// Add a Object Link component with an Object of type L to the entity/entities.
    /// </summary>
    /// <returns>itself (fluent pattern)</returns>
    public SELF Link<L>(L link) where L : class => Add(link, Key.Of(link));
    
    /// <summary>
    /// Remove a Object Link component with an Object of type L from the entity/entities.
    /// </summary>  
    public SELF Unlink<L>(L link) where L : class => Remove<L>(Key.Of(link));

    #endregion
}
