using fennecs.CRUD;
using fennecs.pools;

namespace fennecs;

/// <summary>
/// Wraps a set of operations to be executed atomically on a set of Archetypes (usually those matching a Query).
/// </summary>
public readonly struct Batch : IDisposable, IAddRemove<Batch>
{
    internal readonly Aspect Aspect;
    private World World => Aspect.World;
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
        if (World.Submit(this))
        {
            Dispose();
        }
    }


    internal Batch(SortedSet<Archetype> archetypes, Aspect aspect, Mask mask, AddConflict addMode, RemoveConflict removeMode)
    {
        Aspect = aspect;
        _mask = mask;

        Archetypes.AddRange(archetypes);
        AddMode = addMode;
        RemoveMode = removeMode;
    }


    #region Internals

    private Batch AddComponent<T>(T data, Match match)
    {
        var typeExpression = TypeExpression.Of<T>(match);

        AssertSameAspect(typeExpression);
        World.AssertSameWorld(typeExpression);

        if (AddMode == AddConflict.Strict && !_mask.SafeForAddition(typeExpression))
            throw new InvalidOperationException(
                $"TypeExpression {typeExpression} is not filtered out via Not<T> by this Query/Mask, additions could cause unintended runtime state. See QueryBuilder.Not<T>(). See AddConflict.Disallow, AddConflict.Skip, AddConflict.Replace.");

        if (Additions.Contains(typeExpression))
            throw new InvalidOperationException($"Duplicate addition {typeExpression} : {data}  in same batch!");

        // Matches is checked both ways so Wildcard removals conflict with concrete additions of the same type.
        if (ConflictsWith(typeExpression, Removals))
            throw new InvalidOperationException($"Addition {typeExpression} conflicts with removal  in same batch!");

        Additions.Add(typeExpression);
        BackFill.Add(data!);
        return this;
    }

    private Batch RemoveComponent<T>(Match match = default)
    {
        var typeExpression = TypeExpression.Of<T>(match);

        AssertSameAspect(typeExpression);
        World.AssertSameWorld(typeExpression);

        if (RemoveMode == RemoveConflict.Strict && !_mask.SafeForRemoval(typeExpression))
            throw new InvalidOperationException(
                $"TypeExpression {typeExpression} is not included via Has<T> or Any<T> by this Query/Mask, removals could cause unintended runtime state. See QueryBuilder.Has<T>(). See RemoveConflict.Disallow, RemoveConflict.Skip.");

        // Matches is checked both ways so Wildcard removals conflict with concrete additions of the same type.
        if (ConflictsWith(typeExpression, Additions))
            throw new InvalidOperationException($"Removal of {typeExpression} conflicts with addition in same batch!");

        if (Removals.Contains(typeExpression))
            throw new InvalidOperationException($"Duplicate removal of {typeExpression} in same batch!");

        Removals.Add(typeExpression);
        return this;
    }


    // Pairwise, both directions: optimal for the handful of operations a single Batch accumulates.
    private static bool ConflictsWith(TypeExpression expression, PooledList<TypeExpression> others)
    {
        foreach (var other in others)
        {
            if (expression.Equals(other) || expression.Matches(other) || other.Matches(expression)) return true;
            if (expression.Key == Key.Family && other.Key == default
                && LanguageType.IsInFamily(expression.TypeId, other.TypeId)) return true;
            if (other.Key == Key.Family && expression.Key == default
                && LanguageType.IsInFamily(other.TypeId, expression.TypeId)) return true;
        }

        return false;
    }


    private void AssertSameAspect(TypeExpression typeExpression)
    {
        var owner = World.AspectOf(typeExpression);
        if (owner == Aspect) return;

        throw new InvalidOperationException(
            $"Batch on Aspect \"{Aspect.Name}\" cannot operate on {typeExpression}, which is stored in Aspect \"{owner.Name}\". " +
            "Batches migrate Archetypes within a single Aspect.");
    }

    #endregion


    #region IAddRemoveComponent

    /// <inheritdoc />
    public Batch Add<R>(R value, Entity relation) where R : notnull => AddComponent(value, relation);

    /// <inheritdoc />
    public Batch Add<T>(Link<T> link) where T : class => AddComponent(link.Target, link);

    /// <inheritdoc />
    public Batch Add<T>() where T : notnull, new() => AddComponent(new T(), Match.Plain);

    /// <inheritdoc />
    public Batch Add<C>(C value) where C : notnull => AddComponent(value, Match.Plain);

    /// <inheritdoc />
    public Batch Add<T>(Entity target) where T : notnull, new() => AddComponent<T>(new(), target);

    /// <inheritdoc />
    public Batch Remove<T>() where T : notnull => RemoveComponent<T>(Match.Plain);

    /// <inheritdoc />
    public Batch Remove<C>(Match match) where C : notnull => RemoveComponent<C>(match);

    /// <inheritdoc />
    public Batch Remove<R>(Entity relation) where R : notnull => RemoveComponent<R>(relation);

    /// <inheritdoc />
    public Batch Remove<L>(L linkedObject) where L : class => RemoveComponent<L>(Link<L>.With(linkedObject));

    /// <inheritdoc />
    public Batch Remove<T>(Link<T> link) where T : class => RemoveComponent<T>(link);

    #endregion


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
}
