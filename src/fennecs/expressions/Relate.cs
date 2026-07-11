namespace fennecs;

/// <summary>
/// Target Expression to build a relation.
/// </summary>
internal readonly record struct Relate
{
    private Key Value { get; }

    internal Relate(Key key) => Value = key;

    internal Relate(Entity entity) => Value = entity.Key;

    /// <summary>
    /// Create a Relation expression to the Target Entity.
    /// </summary>
    public static Relate To(Entity entity) => new(entity.Key);

    /// <summary>
    /// Implicit conversion from Entity to Relation Target.
    /// </summary>
    public static implicit operator Relate(Entity entity) => new(entity.Key);

    /// <summary>
    /// Implicit conversion from Relation Target to Generic Match term.
    /// </summary>
    public static implicit operator Match(Relate self)
    {
        // Unfortunately, plain is not default here, but it helps a lot with the other match
        // Expressions. But actually this. That we have strong ID types.
        return self.Value == default ? Match.Plain : new(self.Value);
    }

    /// <inheritdoc />
    public override string ToString() => Value.ToString();
}
