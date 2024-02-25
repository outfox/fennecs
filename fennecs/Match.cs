namespace fennecs;

/// <summary>
/// Match Expressions for Query Matching.
/// Differentiates, in Query Matching, between Plain Components, Entity-Entity Relations, and Entity-Object Relations.
/// Offers a set of Wildcards for matching any combinations of the above.
/// </summary>
public static class Match
{
    /// <summary>
    /// In Query Matching; matches ONLY Plain Components, i.e. those without a Relation Target.
    /// </summary>
    /// <remarks>
    /// Formerly known as "None"
    /// </remarks>
    public static readonly Identity Plain = default; // == 0-bit == new(0,0)

    /// <summary>
    /// In Query Matching; matches ONLY Entity-Entity relations.
    /// </summary>
    public static readonly Identity Identity = new(-3, 0);

    /// <summary>
    /// In Query Matching; matches ONLY Entity-Object links.
    /// </summary>
    public static readonly Identity Object = new(-4, 0);

    /// <summary>
    /// <para>
    /// <b>Wildcard!</b>
    /// <br/>In Query Matching; matches ALL Components: Plain, Entity, and Object.
    /// </para>
    /// <para>
    /// This Match Expression is free when applied to a Filter expression, see <see cref="QueryBuilder"/>.
    /// </para>
    /// <para>
    /// When applied to a Query's Stream Types (see <see cref="QueryBuilder{C0}"/> to <see cref="QueryBuilder{C0,C1,C2,C3,C4}"/>),
    /// the Match Expression may cause multiple iteration of Entities if the Archetype <em>has multiple</em> matching Components.
    /// </para>
    /// <para>
    /// <b>Cardinality 3:</b> up to three iterations per Wildcard per Archetype matching all three Component Stream Types
    /// </para>
    /// <ul>
    /// <li>(plain Components)</li>
    /// <li>(entity-entity relations)</li>
    /// <li>(entity-object relations)</li>
    /// </ul>
    /// </summary>
    /// <remarks>
    /// <para>
    /// Wildcards cause CROSS JOIN type Query iteration.
    /// </para>
    /// <para>
    /// This doesn't have a negative performance impact in and of itself (querying is fast), but it multiplies the number
    /// of times an entity is enumerated, which for large archetypes may multiply an already substantial workload by a factor
    /// between 2^n and 3^n (with n being the number of Wildcards and 2-4 being the cardinality).
    /// </para>
    /// <para>
    /// For small archetypes with simple workloads, repeat iterations are negligible compared to the overhead of starting the
    /// operation, especially when working with Jobs, see <see cref="Query{C0}.Job"/> to <see cref="Query{C0,C1,C2,C3,C4}.Job(fennecs.RefAction{C0,C1,C2,C3,C4},int)"/> 
    /// </para>
    /// <ul>
    /// <li>Confusion Risk: Query Delegates (<see cref="RefAction{C0}"/>, <see cref="SpanAction{C0}"/>, etc.) interacting with Entities matching a Wildcard multiple times will see the Entity repeatedly, once for each variant.</li>
    /// <li>Higher Workloads: In Archetypes where multiple matches exist, Entities will get enumerated once for each matched Component in an Archetype that fits the Stream Type this match
    /// applies to.</li>
    /// <li>Cartesian Product: queries with multiple Wildcard Stream Type Match Expressions create a cartesian product when iterating an Archetype
    /// that has multiple matching Components, complexity can be o(w^n), with w being the cardinality of n the number Wildcards (not Entities!).</li>
    /// <li>(not a real use case) Avoid enumerating the same Stream Type multiple times with Wildcards (it's redundant even with exact matches, and 4x or 9x per type depending on Wildcard).</li>
    /// </ul> 
    /// </remarks>
    public static readonly Identity Any = new(-1, 0);

    /// <summary>
    /// <para>
    /// <b>Wildcard!</b>
    /// <br/> In Query Matching; matches ALL Relations with a Target: Entity-Entity and Entity-Object.
    /// </para>
    /// <para>This expression is free when applied to a Filter expression, see <see cref="Query"/>.
    /// </para>
    /// <para>
    /// When this Match Expression is applied to a Query's Stream Types <see cref="Query{C0}"/> to <see cref="Query{C0,C1,C2,C3,C4}"/>, this will cause multiple iteration of Entities.
    /// </para>
    /// <para>
    /// <b>Cardinality 2:</b> up to two iterations per Wildcard per Archetype matching Component  Stream Types of Components
    /// </para>
    /// <ul>
    /// <li>(entity-entity relations)</li>
    /// <li>(entity-object relations)</li>
    /// </ul>
    /// </summary>
    /// <inheritdoc cref="Any"/>
    public static readonly Identity Relation = new(-2, 0);

    internal static bool CrossJoin(Span<int> counter, Span<int> limiter)
    {
        // Loop through all counters, counting up to goal and wrapping until saturated
        // Example: 0-0-0 to 1-3-2:
        // 000 -> 010 -> 020 -> 001 -> 011 -> 021 -> 002 -> 012 -> 022 -> 032

        for (var i = 0; i < counter.Length; i++)
        {
            // Increment the current counter
            counter[i]++;

            // Successful increment?
            if (counter[i] < limiter[i]) return true;

            // Current counter reached its goal, reset it and move to the next
            counter[i] = 0;

            //Continue until last counter fills up
            if (i == counter.Length - 1) break;
        }

        return false;
    }
}