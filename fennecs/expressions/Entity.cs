using fennecs.CRUD;

namespace fennecs;

/// <summary>
/// An object in the fennecs World, that can have any number of Components.
/// This <c>ref struct</c> represents a living Entity, and while in scope is
/// guaranteed to be alive.
/// </summary>
/// <remarks>
/// Use its <see cref="snapshot"/> property to store it in an <see cref="Entity.Snapshot">Entity.Snapshot</see>
/// variable, annotated with a generation, for long term storage and later referencing.
/// </remarks>
public readonly ref partial struct Entity : IComparable<Entity>, IEntity
{
    private readonly ref readonly Id _id;
    private Entity entity => new(in _id);

    
    internal uint Raw => _id.Value;
    
    public Snapshot snapshot => _id.Snapshot;

    /// <summary>
    /// Represents a living Entity. Can be cast to Entity to get a snapshot annotated with a generation.
    /// </summary>
    internal Entity(ref readonly Id id)
    {
        _id = ref id;
    }

    /// <summary>
    /// The World this Entity lives in.
    /// </summary>
    public World World => World.Get(WorldIndex);
    
    internal uint Index => _id.Index;
    private uint WorldIndex => _id.Value & World.Mask >> World.Shift;


    /// <summary>
    /// Convert this Identity to an Entity, annotating it with its current generation.
    /// </summary>
    /// <returns></returns>
    public static implicit operator Snapshot(Entity self) => self.snapshot;

    /// <inheritdoc />
    public int CompareTo(Entity other) => _id.CompareTo(other._id);

    public bool Equals(Entity other)
    {
        return entity.Equals(other);
    }

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

