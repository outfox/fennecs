using fennecs.CRUD;

namespace fennecs;

/// <summary>
/// An object in a fennecs.<see cref="fennecs.World"/>. Entities can have any number of Components.
/// This <c>ref struct</c> represents an Entity, which while in scope is
/// guaranteed to be alive.
/// </summary>
/// <remarks>
/// Implicitly casts to <see cref="Entity"/> to store it on the heap, annotated with a generation
/// for later disambiguation / liveness checks.
/// </remarks>
public readonly ref struct EntityRef : IComparable<Entity>, IEntity
{
    private readonly ref readonly Id _id;

    /// <summary>
    /// Take the Entity's snapshot, annotated with its current generation.
    /// </summary>
    public static implicit operator Entity(EntityRef self) => self._id.Snapshot;

    /// <summary>
    /// Represents a living Entity. Can be cast to Entity to get a snapshot annotated with a generation.
    /// </summary>
    internal EntityRef(ref readonly Id id)
    {
        _id = ref id;
    }

    /// <summary>
    /// The World this Entity lives in.
    /// </summary>
    public World World => this.World.Get(WorldIndex);
    
    internal uint Index => _id.Index;
    private uint WorldIndex => _id.Value & this.World.Mask >> this.World.Shift;

    
    /// <inheritdoc />
    public int CompareTo(Entity other) => _id.CompareTo(other._id);
    
    /// <inheritdoc />
    public bool Equals(Entity other) => other.Equals(this);

    /// <inheritdoc />
    public override string ToString() => _id.ToString();

    #region IEntity

    /// <inheritdoc />
    public Entity Add<C>(C component, Key key = default, string callerFile = "", int callerLine = 0) where C : notnull => entity.Add(component, key, callerFile, callerLine);

    /// <inheritdoc />
    public Entity Add<C>(Key key = default, string callerFile = "", int callerLine = 0) where C : notnull, new() => entity.Add<C>(key, callerFile, callerLine);

    /// <inheritdoc />
    public Entity Remove<C>(Match match = default, string callerFile = "", int callerLine = 0) where C : notnull => entity.Remove<C>(match, callerFile, callerLine);

    /// <inheritdoc />
    public Entity Remove(MatchExpression expression, string callerFile = "", int callerLine = 0) => entity.Remove(expression, callerFile, callerLine);

    /// <inheritdoc />
    public Entity Link<L>(L link, string callerFile = "", int callerLine = 0) where L : class => entity.Link(link, callerFile, callerLine);

    /// <inheritdoc />
    public bool Has<C>(Match match = default) where C : notnull => entity.Has<C>(match);

    /// <inheritdoc />
    public bool Has<L>(L link) where L : class => entity.Has(link);

    /// <inheritdoc />
    public bool Has(Type type, Match match) => entity.Has(type, match);

    /// <inheritdoc />
    public bool Has(MatchExpression expression) => entity.Has(expression);

    /// <inheritdoc />
    public void Despawn() => entity.Despawn();

    /// <inheritdoc />
    public IReadOnlyList<Component> Components => entity.Components;

    #endregion
}