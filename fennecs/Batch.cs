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
        if (_world.Submit(this)) Dispose();
    }


    internal Batch(IReadOnlyList<Archetype> archetypes, World world, Mask mask, AddConflict addMode, RemoveConflict remove)
    {
        Archetypes.AddRange(archetypes);
        _world = world;
        _mask = mask;
        AddMode = addMode;
        RemoveMode = remove;
    }


    public Batch Add<T>(T data) => AddComponent(data, target: default);
    public Batch Add<T>() where T : new() => AddComponent(new T(), target: default);
    public Batch AddLink<T>(T target) where T : class => AddComponent(target, Identity.Of(target));
    public Batch AddRelation<T>(Entity target) where T : new() => AddComponent<T>(new T(), target.Id);
    public Batch AddRelation<T>(T data, Entity target) where T : notnull => AddComponent(data, target.Id);

    public Batch Remove<T>() => RemoveComponent<T>();
    public Batch RemoveLink<T>(T target) where T : class => RemoveComponent<T>(Identity.Of(target));
    public Batch RemoveRelation<T>(Entity target) => RemoveComponent<T>(target.Id);


    private Batch AddComponent<T>(T data, Identity target)
    {
        var typeExpression = TypeExpression.Of<T>(target);

        if (AddMode == AddConflict.Disallow && !_mask.SafeForAddition(typeExpression))
            throw new InvalidOperationException($"TypeExpression {typeExpression} is not filtered out via Not<T> by this Query/Mask, additions could cause unintended runtime state. See QueryBuilder.Not<T>(). See AddConflict.Disallow, AddConflict.Skip, AddConflict.Replace.");

        if (Additions.Contains(typeExpression))
            throw new InvalidOperationException($"Duplicate addition {typeExpression} : {data}  in same batch!");

        if (Removals.Contains(typeExpression))
            throw new InvalidOperationException($"Addition {typeExpression} conflicts with removal  in same batch!");

        Additions.Add(typeExpression);
        BackFill.Add(data!);
        return this;
    }


    private Batch RemoveComponent<T>(Identity target = default)
    {
        var typeExpression = TypeExpression.Of<T>(target);

        if (RemoveMode == RemoveConflict.Disallow && !_mask.SafeForRemoval(typeExpression))
            throw new InvalidOperationException($"TypeExpression {typeExpression} is not included via Has<T> or Any<T> by this Query/Mask, removals could cause unintended runtime state. See QueryBuilder.Has<T>(). See RemoveConflict.Disallow, RemoveConflict.Skip.");

        if (Additions.Contains(typeExpression))
            throw new InvalidOperationException($"Removal {typeExpression} conflicts with addition in same batch!");

        if (Removals.Contains(typeExpression))
            throw new InvalidOperationException($"Duplicate removal {typeExpression} in same batch!");

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
    /// Batch Addition conflict resolution mode.
    /// </summary>
    public enum AddConflict
    {
        /// <summary>
        /// Disallow add operation if the Component to be added is not guaranteed to be absent
        /// on ALL matched Archetypes see <see cref="QueryBuilder.Not{T}(fennecs.Identity)"/>.
        /// </summary>
        Disallow = default,

        /// <summary>
        /// Skip Archetypes where the Component to be added is already present, preserving its original data.
        /// </summary>
        Skip,

        /// <summary>
        /// Replace (overwrite) the Component to be added if it is already present.
        /// </summary>
        Replace,
    }


    /// <summary>
    /// Batch Removal conflict resolution mode.
    /// </summary>
    public enum RemoveConflict
    {
        /// <summary>
        /// Disallow remove operation if the Component to be removed is not guaranteed to be present
        /// on ALL matched Archetypes, see <see cref="QueryBuilder.Has{T}(fennecs.Identity)"/>.
        /// </summary>
        Disallow = default,

        /// <summary>
        /// Skip Archetypes where the Component to be removed is not present.
        /// </summary>
        Skip,
    }
}