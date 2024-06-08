namespace fennecs;

/// <summary>
/// A specific set of match expressions to match specific component types where type parameters are not available.
/// </summary>
public readonly record struct Component
{
    internal Component(TypeExpression value)
    {
        this.value = value;
    }
    
    internal TypeExpression value { get; }

    internal bool Matches(Component other) => value.Matches(other.value);

}
