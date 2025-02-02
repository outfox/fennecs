namespace fennecs;

/// <summary>
/// Provides a weak reference storage mechanism for an Entity, by pairing it with a 32-bit Generation value.
/// This type ensures safe entity tracking by using the Generation counter as a validity check.
/// When an Entity is despawned, its Generation is incremented, automatically invalidating any existing
/// EntityWithGeneration references to that Entity.
/// </summary>
/// <remarks>
/// Best suited for scenarios where:
/// <list type="bullet">
/// <item>You need to track Entity lifecycle changes and validate weak references (e.g. for networking, or AI targets)</item>
/// <item>You require direct access to the Entity's components from anywhere not operating on a <see cref="Stream"/> that contains the Entity</item>
/// <item>You perform frequent component access operations and need performance</item>
/// </list>
/// 
/// While robust object lifecycle management could make this type unnecessary,
/// it provides a reliable safety mechanism for scenarios where Entity lifetime
/// cannot be guaranteed at access time.
/// </remarks>
public readonly record struct EntityWithGeneration(Entity Entity, uint Generation) : IEquatable<Entity>
{
    /// <inheritdoc />
    public bool Equals(Entity other) => this == other;

    /// <inheritdoc />
    public override int GetHashCode() => HashCode.Combine(Entity, Generation);
    
    /// <summary>
    /// Fast alive check, compares generation values.
    /// </summary>
    public bool Alive => Entity.Generation == Generation;
}