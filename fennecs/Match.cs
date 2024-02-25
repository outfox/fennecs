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
    public static readonly Identity Entity = new(-3, 0);

    /// <summary>
    /// In Query Matching; matches ONLY Entity-Object links.
    /// </summary>
    public static readonly Identity Object = new(-4, 0);

    /// <summary>
    /// Represents a wildcard match expression for query filtering. This matches all types of components: Plain, Entity, and Object.
    /// <para>Use it freely in filter expressions to match any component type. See <see cref="QueryBuilder"/> for how to apply it in queries.</para>
    /// <para>This expression is free when applied to a Filter expression, see <see cref="Query"/>.
    /// </para>
    /// <para>Applying this to a Query's Stream Type can result in multiple iterations over entities if they match multiple component types. This is due to the wildcard's nature of matching all components.</para>
    /// </summary>
    /// <remarks>
    /// <para>⚠️ Using wildcards can lead to a CROSS JOIN effect, iterating over entities multiple times for each matching component. While querying is efficient, this increases the number of operations per entity.</para>
    /// <para>This effect is more pronounced in large archetypes with many matching components, potentially multiplying the workload significantly. However, for smaller archetypes or simpler tasks, the impact is minimal.</para>
    /// <para>Risks and considerations include:</para>
    /// <ul>
    /// <li>Repeated enumeration: Entities matching a wildcard are processed multiple times, for each matching component type combination.</li>
    /// <li>Increased workload: Especially in cases where entities match multiple components, leading to higher processing times.</li>
    /// <li>Complex queries: Multiple wildcards can create a cartesian product effect, significantly increasing complexity and workload.</li>
    /// <li>Avoid redundant enumeration with wildcards, as it's unnecessary and can inflate processing times.</li>
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
        /// Call <see cref="Iterate"/> to select the next permutation.
        /// </summary>
        internal C0[] Select => _storages0[_counter[0]];


        /// <summary>
        /// Ticks the internal counter of the Join operation, readying the next permutation to use in <see cref="Select"/>.
        /// </summary>
        /// <returns>
        /// <c>true</c> if permutation exists<br/><c>false</c> if the Cross Join has exhausted all permutations.
        /// </returns>
        internal bool Iterate() => CrossJoin(_counter, _limiter);


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

        internal bool Iterate() => CrossJoin(_counter, _limiter);


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

        internal bool Iterate() => CrossJoin(_counter, _limiter);


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

        internal bool Iterate() => CrossJoin(_counter, _limiter);


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

        internal bool Iterate() => CrossJoin(_counter, _limiter);


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