namespace fennecs;

public readonly record struct Match(Identity Value)
{
    public static Match Any => new(MatchOld.Any);
    public static Match AnyRelation => new(MatchOld.Target);
    public static Match AnyObject => new(MatchOld.Object);
    public static Match AnyEntity => new(MatchOld.Entity);
    public static Match Entity(Entity other) => new(other.Id);
    public static Match Plain => new(MatchOld.Plain);
    
    public bool Matches(Match other) => Value == other.Value;
    
    public static implicit operator Match(Identity value) => new(value);
    
    //TODO Maybe not even needed...
    public bool IsWildcard => Value.IsWildcard;
    public bool IsEntity => Value.IsEntity;
    public bool IsObject => Value.IsObject;
    public bool IsTarget => Value == MatchOld.Target;
    public bool IsPlain => Value == MatchOld.Plain;
    
    [Obsolete("Find a way to remove me.")]
    internal ulong Raw => Value.Value;
}
