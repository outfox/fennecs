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
    /// Add a newable component of type C to the entity/entities, with an optional Key.
    /// </summary>
    /// <remarks>
    /// This will call the default parameterless constructor of the backing component type.
    /// </remarks>
    /// <returns>itself (fluent pattern)</returns>
    public SELF Add<C>(Key key = default) where C : notnull, new() => Add(new C(), key);

    /// <summary>
    /// Remove a Plain component of type C from the entity/entities.
    /// </summary>
    /// <returns>itself (fluent pattern)</returns>
    public SELF Remove<C>(Key key = default) where C : notnull;

    /// <summary>
    /// Add a Object Link component with an Object of type L to the entity/entities.
    /// </summary>
    /// <remarks>
    /// Remove the link by calling <c>Remove&lt;L&gt;(Key.Of(link))</c> or <c>Remove&lt;L&gt;(link.Key())</c>.
    /// Object Links are not tracked by the World, so their backing component can be changed.
    /// </remarks>
    /// <returns>itself (fluent pattern)</returns>
    public SELF Link<L>(L link) where L : class => Add(link, Key.Of(link));
    
}