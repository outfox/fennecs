namespace fennecs.CRUD;

/// <summary>
/// Entity-Like objects and references that allow adding, removing, cloning, and despawning.
/// </summary>
public interface IEntity : IAddRemoveDeferrable<Entity>, IEquatable<Entity>, IHasComponent
{
    /// <summary>
    /// Despawn the Entity.
    /// </summary>
    public void Despawn();
    
    /// <summary>
    /// All Components on the Entity, in a reflection-friendly typeless format. (for example to be used for serialization)
    /// </summary>
    public IReadOnlyList<Component> Components { get; }
    
    /* TODO: Implement these :)
    /// <summary>
    /// Clones the Entity, and all its components.
    /// </summary>
    /// <remarks>
    /// If a relation on the Entity targets itself, the cloned Entity will also target the original Entity.
    /// This can be used to find all the clones of an Entity.
    /// </remarks>
    /// <param name="amount">amount of clones to create</param>
    /// <returns>the cloned Entities</returns>
    public IEnumerable<Entity> Clone(int amount = 1) => throw new NotImplementedException();
    */
}
