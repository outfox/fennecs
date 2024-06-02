using fennecs.pools;

namespace fennecs;

/// <summary>
/// Wraps a set of operations to be executed atomically on a set of Archetypes (usually those matching a Query).
/// </summary>
public readonly struct Batch : IDisposable
{
    private readonly World _world;
    private readonly Mask _mask;

    internal readonly PooledList<Archetype> Archetypes = PooledList<Archetype>.Rent();
    internal readonly PooledList<TypeExpression> Additions = PooledList<TypeExpression>.Rent();
    internal readonly PooledList<TypeExpression> Removals = PooledList<TypeExpression>.Rent();
    internal readonly PooledList<object> BackFill = PooledList<object>.Rent();

    internal readonly AddConflict AddMode;

    // ReSharper disable once MemberCanBePrivate.Global
    internal readonly RemoveConflict RemoveMode;


    /// <summary>
    /// Submit this Batch to its World, which will take ownership of the IDisposable.
    /// The world wil defer the operation if it is not in immediate mode and dispose afterwards.
    /// </summary>
    public void Submit()
    {
        if (_world.Submit(this))
        {
            Dispose();
        }
    }


    internal Batch(List<Archetype> archetypes, World world, Mask mask, AddConflict addMode, RemoveConflict removeMode)
    {
        _world = world;
        _mask = mask;

        Archetypes.AddRange(archetypes);
        AddMode = addMode;
        RemoveMode = removeMode;
    }


    /// <summary>
    /// Append an AddComponent operation to the batch.
    /// </summary>
    /// <typeparam name="T">component type</typeparam>
    /// <param name="data">component data</param>
    /// <param name="target">relation target (default = no relation, plain component)</param>
    /// <returns>the Batch itself (fluent syntax)</returns>
    public Batch Add<T>(T data, Relate target = default) => AddComponent(data, target);
    
    /// <summary>
    /// Append an AddComponent operation to the batch.
    /// </summary>
    /// <typeparam name="T">component type</typeparam>
    /// <param name="link">an object link</param>
    /// <returns>the Batch itself (fluent syntax)</returns>
    public Batch Add<T>(Link<T> link) where T : class => AddComponent(link.Target, link);

    /// <summary>
    /// Append an AddComponent operation to the batch.
    /// </summary>
    /// <typeparam name="T">component type (newable)</typeparam>
    /// <returns>the Batch itself (fluent syntax)</returns>
    public Batch Add<T>() where T : new() => AddComponent(new T(), target: Match.Plain);

    /// <summary>
    /// Append an Add operation to the batch.
    /// </summary>
    /// <param name="target">target of the relation</param>
    /// <typeparam name="T">component type (newable)</typeparam>
    /// <returns>the Batch itself (fluent syntax)</returns>
    public Batch Add<T>(Entity target) where T : new() => AddComponent<T>(new(), Relate.To(target));
    
    
    /// <summary>
    /// Append an RemoveComponent operation to the batch.
    /// </summary>
    /// <typeparam name="T">component type</typeparam>
    /// <returns>the Batch itself (fluent syntax)</returns>
    public Batch Remove<T>() => RemoveComponent<T>(Match.Plain);

    /// <summary>
    /// Append an Remove operation to the batch.
    /// </summary>
    /// <typeparam name="T">component type</typeparam>
    /// <param name="link">target of the link</param>
    /// <returns>the Batch itself (fluent syntax)</returns>
    public Batch Remove<T>(Link<T> link) where T : class => RemoveComponent<T>(link);

    /// <summary>
    /// Append a RemoveRelation operation to the batch.
    /// </summary>
    /// <typeparam name="T">component type</typeparam>
    /// <param name="target">target of the relation</param>
    /// <returns>the Batch itself (fluent syntax)</returns>
    public Batch Remove<T>(Relate target) => RemoveComponent<T>(target);


    private Batch AddComponent<T>(T data, Match target)
    {
        var typeExpression = TypeExpression.Of<T>(target);

        if (AddMode == AddConflict.Strict && !_mask.SafeForAddition(typeExpression))
            throw new InvalidOperationException(
                $"TypeExpression {typeExpression} is not filtered out via Not<T> by this Query/Mask, additions could cause unintended runtime state. See QueryBuilder.Not<T>(). See AddConflict.Disallow, AddConflict.Skip, AddConflict.Replace.");

        if (Additions.Contains(typeExpression))
            throw new InvalidOperationException($"Duplicate addition {typeExpression} : {data}  in same batch!");

        if (Removals.Contains(typeExpression))
            throw new InvalidOperationException($"Addition {typeExpression} conflicts with removal  in same batch!");

        Additions.Add(typeExpression);
        BackFill.Add(data!);
        return this;
    }

    private Batch RemoveComponent<T>(Match target = default)
    {
        var typeExpression = TypeExpression.Of<T>(target);

        if (RemoveMode == RemoveConflict.Strict && !_mask.SafeForRemoval(typeExpression))
            throw new InvalidOperationException(
                $"TypeExpression {typeExpression} is not included via Has<T> or Any<T> by this Query/Mask, removals could cause unintended runtime state. See QueryBuilder.Has<T>(). See RemoveConflict.Disallow, RemoveConflict.Skip.");

        if (Additions.Contains(typeExpression))
            throw new InvalidOperationException($"Removal of {typeExpression} conflicts with addition in same batch!");

        if (Removals.Contains(typeExpression))
            throw new InvalidOperationException($"Duplicate removal of {typeExpression} in same batch!");

        Removals.Add(typeExpression);
        return this;
    }


    /// <summary>
    /// Disposes the Batch Operation, freeing internals resources.
    /// Automatically called by Submit().
    /// </summary>
    public void Dispose()
    {
        Archetypes.Dispose();
        Additions.Dispose();
        Removals.Dispose();
        BackFill.Dispose();
        _mask.Dispose();
    }


    /// <summary>
    /// Specifies behavior when adding a component to an archetype that already has the same type of component. 
    /// </summary>
    public enum AddConflict
    {
        /// <summary>
        /// Disallows the addition of components that could already be present in a query.
        /// </summary>
        /// <remarks>
        /// Exclude the component from the query via <see cref="QueryBuilder{C1}.Not{T}(fennecs.Match)"/> or similar
        /// means. If you want to allow the addition of components that are already present, use <see cref="Preserve"/>
        /// to keep any values already present, or use <see cref="Replace"/> if you'd like to overwrite the component
        /// value everywhere it is already encountered in the query.
        /// </remarks>
        Strict = default,

        /// <summary>
        /// Ignores archetypes that already contain the component, leaving their data and state unchanged.
        /// ⚠️ This affects all operations to be submitted with the Batch, even retroactively, when a conflicting
        /// Add operation is added.
        /// </summary>
        /// <remarks>
        /// If an archetype already has the component that a batch tries to add, no entities of that archetype are affected. This is true regardless of whether or not they match the batch's EntityQuery.
        /// </remarks>
        [Obsolete("Use Preserve instead.", true)]
        SkipEntirely,

        /// <summary>
        /// Keeps the existing component data whenever trying to add a duplicate.
        /// </summary>
        Preserve,

        /// <summary>
        /// Overwrites existing component data with the addded component if it is already present.
        /// </summary>
        /// <remarks>
        /// Alternatively, you can use the faster <see cref="Query{C0}.Blit"/> if you
        /// can ensure that the component is present on all entities in the query.
        /// </remarks>
        Replace,
    }


    /// <summary>
    /// Batch Removal conflict resolution mode.
    /// </summary>
    public enum RemoveConflict
    {
        /// <summary>
        /// Disallow remove operation if the Component to be removed is not guaranteed to be present
        /// on ALL matched Archetypes, see <see cref="QueryBuilder.Has{T}(fennecs.Match)"/>.
        /// </summary>
        Strict = default,

        /// <summary>
        /// Allow operating on Archetypes where the Component to be removed is not present.
        /// Removal operations are Idempotent on these archetypes, i.e. they don't change them
        /// on their own.
        /// </summary>
        Allow,
    }
}
