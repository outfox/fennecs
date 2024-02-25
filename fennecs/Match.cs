using System.Buffers;
using fennecs.pools;

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


    #region Cross Join

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


    /// <summary>
    /// Cross-Joins the Archetype with a list of StreamTypes.
    /// </summary>
    internal readonly struct Join<C0> : IDisposable
    {
        private readonly int[] _counter;
        private readonly int[] _limiter;

        private readonly PooledList<C0[]> _storages0;


        /// <summary>
        /// Cross-Joins the Archetype with a list of StreamTypes.
        /// </summary>
        internal Join(Archetype archetype, TypeExpression[] streamTypes)
        {
            _counter = ArrayPool<int>.Shared.Rent(streamTypes.Length);
            _limiter = ArrayPool<int>.Shared.Rent(streamTypes.Length);
            _storages0 = archetype.Match<C0>(streamTypes[0]);

            Array.Fill(_counter, 0);
            _limiter[0] = _storages0.Count;
        }


        /// <summary>
        /// Returns the Storage in the current internal Cross Join counter configuration.
        /// Call <see cref="Permutate"/> to select the next permutation.
        /// </summary>
        internal C0[] Select => _storages0[_counter[0]];

        /// <summary>
        /// Ticks the internal counter of the Join operation, readying the next permutation to use in <see cref="Select"/>.
        /// </summary>
        /// <returns>
        /// <c>true</c> if permutation exists<br/><c>false</c> if the Cross Join has exhausted all permutations.
        /// </returns>
        internal bool Permutate => CrossJoin(_counter, _limiter);


        public void Dispose()
        {
            _storages0.Dispose();
            ArrayPool<int>.Shared.Return(_counter);
            ArrayPool<int>.Shared.Return(_limiter);
        }
    }


    /// <summary>
    /// Cross-Joins the Archetype with a list of StreamTypes.
    /// </summary>
    internal readonly struct Join<C0, C1> : IDisposable
    {
        private readonly int[] _counter;
        private readonly int[] _limiter;

        private readonly PooledList<C0[]> _storages0;
        private readonly PooledList<C1[]> _storages1;


        internal Join(Archetype archetype, TypeExpression[] streamTypes)
        {
            _counter = ArrayPool<int>.Shared.Rent(streamTypes.Length);
            _limiter = ArrayPool<int>.Shared.Rent(streamTypes.Length);
            _storages0 = archetype.Match<C0>(streamTypes[0]);
            _storages1 = archetype.Match<C1>(streamTypes[1]);

            Array.Fill(_counter, 0);
            _limiter[0] = _storages0.Count;
            _limiter[1] = _storages1.Count;
        }


        internal (C0[], C1[]) Select => (_storages0[_counter[0]], _storages1[_counter[1]]);

        internal bool Permutate => CrossJoin(_counter, _limiter);


        public void Dispose()
        {
            _storages0.Dispose();
            _storages1.Dispose();
            ArrayPool<int>.Shared.Return(_counter);
            ArrayPool<int>.Shared.Return(_limiter);
        }
    }


    /// <summary>
    /// Cross-Joins the Archetype with a list of StreamTypes.
    /// </summary>
    internal readonly struct Join<C0, C1, C2> : IDisposable
    {
        private readonly int[] _counter;
        private readonly int[] _limiter;

        private readonly PooledList<C0[]> _storages0;
        private readonly PooledList<C1[]> _storages1;
        private readonly PooledList<C2[]> _storages2;


        internal Join(Archetype archetype, TypeExpression[] streamTypes)
        {
            _counter = ArrayPool<int>.Shared.Rent(streamTypes.Length);
            _limiter = ArrayPool<int>.Shared.Rent(streamTypes.Length);
            _storages0 = archetype.Match<C0>(streamTypes[0]);
            _storages1 = archetype.Match<C1>(streamTypes[1]);
            _storages2 = archetype.Match<C2>(streamTypes[2]);

            Array.Fill(_counter, 0);
            _limiter[0] = _storages0.Count;
            _limiter[1] = _storages1.Count;
            _limiter[2] = _storages2.Count;
        }


        internal (C0[], C1[], C2[]) Select => (_storages0[_counter[0]], _storages1[_counter[1]], _storages2[_counter[2]]);

        internal bool Permutate => CrossJoin(_counter, _limiter);


        public void Dispose()
        {
            _storages0.Dispose();
            _storages1.Dispose();
            _storages2.Dispose();
            ArrayPool<int>.Shared.Return(_counter);
            ArrayPool<int>.Shared.Return(_limiter);
        }
    }


    /// <summary>
    /// Cross-Joins the Archetype with a list of StreamTypes.
    /// </summary>
    internal readonly struct Join<C0, C1, C2, C3> : IDisposable
    {
        private readonly int[] _counter;
        private readonly int[] _limiter;

        private readonly PooledList<C0[]> _storages0;
        private readonly PooledList<C1[]> _storages1;
        private readonly PooledList<C2[]> _storages2;
        private readonly PooledList<C3[]> _storages3;


        internal Join(Archetype archetype, TypeExpression[] streamTypes)
        {
            _counter = ArrayPool<int>.Shared.Rent(streamTypes.Length);
            _limiter = ArrayPool<int>.Shared.Rent(streamTypes.Length);
            _storages0 = archetype.Match<C0>(streamTypes[0]);
            _storages1 = archetype.Match<C1>(streamTypes[1]);
            _storages2 = archetype.Match<C2>(streamTypes[2]);
            _storages3 = archetype.Match<C3>(streamTypes[3]);

            Array.Fill(_counter, 0);
            _limiter[0] = _storages0.Count;
            _limiter[1] = _storages1.Count;
            _limiter[2] = _storages2.Count;
            _limiter[3] = _storages3.Count;
        }


        internal (C0[], C1[], C2[], C3[]) Select => (_storages0[_counter[0]], _storages1[_counter[1]], _storages2[_counter[2]], _storages3[_counter[3]]);

        internal bool Permutate => CrossJoin(_counter, _limiter);


        public void Dispose()
        {
            _storages0.Dispose();
            _storages1.Dispose();
            _storages2.Dispose();
            _storages3.Dispose();
            ArrayPool<int>.Shared.Return(_counter);
            ArrayPool<int>.Shared.Return(_limiter);
        }
    }


    /// <summary>
    /// Cross-Joins the Archetype with a list of StreamTypes.
    /// </summary>
    internal readonly struct Join<C0, C1, C2, C3, C4> : IDisposable
    {
        private readonly int[] _counter;
        private readonly int[] _limiter;

        private readonly PooledList<C0[]> _storages0;
        private readonly PooledList<C1[]> _storages1;
        private readonly PooledList<C2[]> _storages2;
        private readonly PooledList<C3[]> _storages3;
        private readonly PooledList<C4[]> _storages4;


        internal Join(Archetype archetype, TypeExpression[] streamTypes)
        {
            _counter = ArrayPool<int>.Shared.Rent(streamTypes.Length);
            _limiter = ArrayPool<int>.Shared.Rent(streamTypes.Length);
            _storages0 = archetype.Match<C0>(streamTypes[0]);
            _storages1 = archetype.Match<C1>(streamTypes[1]);
            _storages2 = archetype.Match<C2>(streamTypes[2]);
            _storages3 = archetype.Match<C3>(streamTypes[3]);
            _storages4 = archetype.Match<C4>(streamTypes[4]);

            Array.Fill(_counter, 0);
            _limiter[0] = _storages0.Count;
            _limiter[1] = _storages1.Count;
            _limiter[2] = _storages2.Count;
            _limiter[3] = _storages3.Count;
            _limiter[4] = _storages4.Count;
        }


        internal (C0[], C1[], C2[], C3[], C4[]) Select => (_storages0[_counter[0]], _storages1[_counter[1]], _storages2[_counter[2]], _storages3[_counter[3]], _storages4[_counter[4]]);

        internal bool Permutate => CrossJoin(_counter, _limiter);


        public void Dispose()
        {
            _storages0.Dispose();
            _storages1.Dispose();
            _storages2.Dispose();
            _storages3.Dispose();
            _storages4.Dispose();
            ArrayPool<int>.Shared.Return(_counter);
            ArrayPool<int>.Shared.Return(_limiter);
        }
    }

    #endregion
}