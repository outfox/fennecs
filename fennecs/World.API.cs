using fennecs.pools;

namespace fennecs;

public partial class World : IDisposable
{
    #region Entity Spawn, Liveness, and Desapwn

    /// <summary>
    /// Creates a new Identity in this World, and returns its Entity builder struct.
    /// Reuses previously despawned Entities, whose Identities will differ in Generation after respawn. 
    /// </summary>
    /// <returns>an EntityBuilder to operate on</returns>
    public Entity Spawn() => new(this, NewEntity()); //TODO: Check if semantically legal to spawn in Deferred mode.


    /// <summary>
    /// Despawn (destroy) an Entity from this World.
    /// </summary>
    /// <param name="entity">the entity to despawn.</param>
    public void Despawn(Entity entity) => DespawnImpl(entity.Id);


    /// <summary>
    /// Despawn (destroy) an Entity from this World by its Identity.
    /// </summary>
    /// <param name="identity">the entity to despawn.</param>
    public void Despawn(Identity identity) => DespawnImpl(identity);


    /// <summary>
    /// Interact with an Identity as an Entity.
    /// Perform operations on the given identity in this world, via fluid API.
    /// </summary>
    /// <example>
    /// <code>world.On(identity).Add(123).Add("string").Remove&lt;int&gt;();</code>
    /// </example>
    /// <returns>an Entity builder struct whose methods return itself, to provide a fluid syntax. </returns>
    public Entity On(Identity identity)
    {
        AssertAlive(identity);
        return new Entity(this, identity);
    }


    /// <summary>
    /// Alias for <see cref="On(Identity)"/>, returning an Entity builder struct to operate on. Included to
    /// provide a more intuitive verb to "get" an Entity to assign to a variable.
    /// </summary>
    /// <example>
    /// <code>var bob = world.GetEntity(bobsIdentity);</code>
    /// </example>
    /// <returns>an Entity builder struct whose methods return itself, to provide a fluid syntax. </returns>
    public Entity GetEntity(Identity identity) => On(identity);


    /// <summary>
    /// Checks if the entity is alive (was not despawned).
    /// </summary>
    /// <param name="identity">an Entity</param>
    /// <returns>true if the Entity is Alive, false if it was previously Despawned</returns>
    public bool IsAlive(Identity identity) => identity.IsEntity && identity == _meta[identity.Index].Identity;


    /// <summary>
    /// The number of living entities in the World.
    /// </summary>
    public int Count => _identityPool.Count;

    #endregion


    #region Bulk Operations

    /// <summary>
    /// Despawn (destroy) all Entities matching a given Type and Match Expression.
    /// </summary>
    /// <typeparam name="T">any component type</typeparam>
    /// <param name="match">default <see cref="Match.Plain"/>.<br/>Can alternatively be one
    /// of <see cref="Match.Any"/>, <see cref="Match.Object"/>, <see cref="Match.Target"/>
    /// </param>
    public void DespawnAllWith<T>(Identity match = default)
    {
        using var query = Query<Identity>().Has<T>(match).Build();
        query.Raw(delegate(Memory<Identity> entities)
        {
            foreach (var identity in entities.Span) DespawnImpl(identity);
        });
    }
    

    /// <summary>
    /// Bulk Despawn Entities from a World.
    /// </summary>
    /// <param name="toDelete">the entities to despawn (remove)</param>
    public void Despawn(ReadOnlySpan<Identity> toDelete)
    {
        foreach (var identity in toDelete)
        {
            DespawnImpl(identity);
        }
    }

    #endregion


    #region Lifecycle & Locking

    /// <summary>
    /// Create a new World.
    /// </summary>
    /// <param name="capacity">initial Entity capacity to reserve. The world will grow automatically.</param>
    public World(int capacity = 4096)
    {
        _identityPool = new IdentityPool(capacity);

        _meta = new Meta[capacity];

        //Create the "Entity" Archetype, which is also the root of the Archetype Graph.
        _root = AddTable(new Signature<TypeExpression>(TypeExpression.Of<Identity>(Match.Plain)));
    }


    /// <summary>
    /// Disposes of the World. Currently a no-op.
    /// </summary>
    public void Dispose()
    {
        //TODO: Release all Object Links?
    }


    /// <summary>
    /// Locks the World (setting into a Deferred mode) for the scope of the returned WorldLock.
    /// Multiple Locks can be taken out, and all structural Operations on Entities will be queued,
    /// and executed once the last Lock is released. 
    /// </summary>
    /// <exception cref="InvalidOperationException"></exception>
    public WorldLock Lock => new(this);

    #endregion
}