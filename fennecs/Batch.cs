using System.Runtime.CompilerServices;
using fennecs.CRUD;
using fennecs.pools;

namespace fennecs;

/// <summary>
/// Wraps a set of operations to be executed atomically on a set of Archetypes (usually those matching a Query).
/// </summary>
public readonly struct Batch : IDisposable, IAddRemove<Batch>
{
    private readonly World _world;
    private readonly Mask _mask;
    
    internal readonly PooledList<Archetype> Archetypes = PooledList<Archetype>.Rent();
    internal readonly PooledList<TypeExpression> Additions = PooledList<TypeExpression>.Rent();
    internal readonly PooledList<MatchExpression> Removals = PooledList<MatchExpression>.Rent();
    internal readonly PooledList<object> BackFill = PooledList<object>.Rent();

    internal readonly AddConflict AddMode;

    // ReSharper disable once MemberCanBePrivate.Global
    internal readonly RemoveConflict RemoveMode;


    /// <summary>
    /// Submit this Batch to its World, taking ownership of the IDisposable.
    /// The world wil defer the operation if it is not in immediate mode.
    /// The Batch is disposed afterwards.
    /// </summary>
    public void Submit([CallerFilePath] string callerFile = "", [CallerLineNumber] int callerLine = 0)
    {
        if (_world.Submit(this, callerFile, callerLine))
        {
            Dispose();
        }
    }


    internal Batch(
        HashSet<Archetype> archetypes, 
        World world, Mask mask, 
        AddConflict addMode, 
        RemoveConflict removeMode
)
    {
        _world = world;
        _mask = mask;
        
        Archetypes.AddRange(archetypes);
        AddMode = addMode == default ? World.DefaultAddConflict : addMode;
        RemoveMode = removeMode == default ? World.DefaultRemoveConflict : removeMode;
    }


    #region Internals

    private Batch AddComponent<T>(T data, Key key) where T : notnull
    {
        var typeExpression = TypeExpression.Of<T>(key);
        
        if (Removals.Any(removal => removal.Matches(typeExpression)))
            throw new InvalidOperationException($"Addition of {typeExpression} conflicts with removal in same batch! Because all Removals are applied before any additions, this leads to undefined behaviour.");

        Additions.Add(typeExpression);
        BackFill.Add(data);
        return this;
    }

    /// <inheritdoc />
    public Batch Remove<C>(Match match = default) where C : notnull => Remove(MatchExpression.Of<C>(match));

    /// <inheritdoc />
    public Batch Remove(MatchExpression expression)
    {
        if (Additions.Any(expression.Matches))
            throw new InvalidOperationException($"Removal of {expression} conflicts with addition in same batch! Because any Additions are applied after all Removals, this leads to undefined behaviour.");

        Removals.Add(expression);
        return this;
    }
    
    /// <inheritdoc />
    public Batch Remove<C>(C target) where C : class => Remove<C>(Key.Of(target));

    #endregion


    #region IAddRemoveComponent

    /// <inheritdoc />
    public Batch Add<C>(Key key = default) where C : notnull, new() => AddComponent(new C(), key);

    /// <inheritdoc />
    public Batch Add<C>(C component, Key key = default) where C : notnull => AddComponent(component, key);

    
    /// <inheritdoc />
    public Batch Link<T>(T link) where T : class => AddComponent(link, Key.Of(link));


    /// <summary>
    /// Specifies behavior when adding a component to an archetype that already has the same type of component. 
    /// </summary>
    public enum AddConflict
    {
        /// <summary>
        /// Disallows the addition of components that could already be present in a query.
        /// </summary>
        /// <remarks>
        /// Exclude the component from the query via <see cref="QueryBuilderBase{QB}.Not{T}(Match)"/> or similar
        /// means. If you want to allow the addition of components that are already present, use <see cref="Preserve"/>
        /// to keep any values already present, or use <see cref="Replace"/> if you'd like to overwrite the component
        /// value everywhere it is already encountered in the query.
        /// </remarks>
        Strict = 1,

        /// <summary>
        /// Keeps the existing component data whenever trying to add a duplicate.
        /// </summary>
        Preserve = 2,

        /// <summary>
        /// Overwrites existing component data with the addded component if it is already present.
        /// </summary>
        /// <remarks>
        /// Alternatively, you can use the faster <see cref="Stream{C0}.Blit"/> if you
        /// can ensure that the component is present on all entities in the query.
        /// </remarks>
        Replace = 3,
    }


    /// <summary>
    /// Batch Removal conflict resolution mode.
    /// </summary>
    public enum RemoveConflict
    {
        /// <summary>
        /// Disallow remove operation if the Component to be removed is not guaranteed to be present
        /// on ALL matched Archetypes, see <see cref="QueryBuilderBase{QB}.Has{T}(Match)"/>.
        /// </summary>
        Strict = 1,

        /// <summary>
        /// Allow operating on Archetypes where the Component to be removed is not present.
        /// Removal operations are Idempotent on these archetypes, i.e. they don't change them
        /// on their own.
        /// </summary>
        Allow = 2,
    }

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
