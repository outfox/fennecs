using fennecs.CRUD;

namespace fennecs;

/// <summary>
/// Represents a living Entity. Can be cast to Entity to get a snapshot annotated with a generation.
/// </summary>
public readonly ref partial struct Entity : IComparable<Entity>, IEntity
{
    private readonly ref readonly Id _id;
    private Entity entity => new(in _id);

    
    internal uint Raw => _id.Value;

    /// <summary>
    /// Represents a living Entity. Can be cast to Entity to get a snapshot annotated with a generation.
    /// </summary>
    internal Entity(ref readonly Id id)
    {
        _id = ref id;
    }

    /// <summary>
    /// The Index of this Entity.
    /// </summary>
    public uint Index => _id.Index;

    /// <summary>
    /// The World this Entity lives in.
    /// </summary>
    public World World => World.Get(WorldIndex);

    private uint WorldIndex => _id.Value & World.Mask >> World.Shift;
    private ref Meta Meta => ref World.GetEntityMeta(this);
    internal uint Gen => World.GetGeneration(this);
    internal int Row => Meta.Row;


    /// <summary>
    /// Convert this Identity to an Entity, annotating it with its current generation.
    /// </summary>
    /// <returns></returns>
    public static implicit operator Snapshot(Entity self) => new(self._id, self.Gen);

    /// <inheritdoc />
    public int CompareTo(Entity other) => _id.CompareTo(other._id);

    public bool Equals(Entity other)
    {
        return entity.Equals(other);
    }

    /// <inheritdoc />
    public override string ToString() => $"i{Index:x8}(w{WorldIndex}g{Gen})";

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

