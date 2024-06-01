using System.Buffers;
using System.Collections.Immutable;
using fennecs.pools;

namespace fennecs;

/// <summary>
/// External Enumerator / Permutate for Archetype Storages.
/// </summary>
public static class Cross
{
    #region Cross Join
    internal static bool FullPermutation(Span<int> counter, Span<int> limiter)
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

        private readonly PooledList<Storage<C0>> _storages0;

        private readonly bool _allocated;
        private readonly bool _populated;


        /// <summary>
        /// Cross-Joins the Archetype with a list of StreamTypes.
        /// </summary>
        internal Join(Archetype archetype, ReadOnlySpan<TypeExpression> streamTypes)
        {
            _allocated = true;

            _counter = ArrayPool<int>.Shared.Rent(streamTypes.Length);
            _limiter = ArrayPool<int>.Shared.Rent(streamTypes.Length);
            _storages0 = archetype.Match<C0>(streamTypes[0]);

            Array.Fill(_counter, 0);
            _limiter[0] = _storages0.Count;

            _populated = _storages0.Count > 0;
        }

        /// <summary>
        /// Returns the Storage in the current internal Cross Join counter configuration.
        /// Call <see cref="Iterate"/> to select the next permutation.
        /// </summary>
        internal Storage<C0> Select => _storages0[_counter[0]];


        /// <summary>
        /// Ticks the internal counter of the Join operation, readying the next permutation to use in <see cref="Select"/>.
        /// </summary>
        /// <returns>
        /// <c>true</c> if permutation exists,<br/><c>false</c> if the Cross Join has exhausted all permutations.
        /// </returns>
        internal bool Iterate() => FullPermutation(_counter, _limiter);


        /// <summary>
        /// Returns <c>true</c> if the Join is empty, i.e. no permutations are available.
        /// </summary>
        internal bool Empty => !_populated;
        
        /// <summary>
        /// Returns <c>true</c> if the Join has been populated with at least one permutation.
        /// </summary>
        internal bool Populated => _populated;


        public void Dispose()
        {
            if (!_allocated) return;

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

        private readonly PooledList<Storage<C0>> _storages0;
        private readonly PooledList<Storage<C1>> _storages1;

        private readonly bool _allocated;
        private readonly bool _populated;


        internal Join(Archetype archetype, ImmutableArray<TypeExpression> streamTypes)
        {
            _allocated = true;

            _counter = ArrayPool<int>.Shared.Rent(streamTypes.Length);
            _limiter = ArrayPool<int>.Shared.Rent(streamTypes.Length);
            _storages0 = archetype.Match<C0>(streamTypes[0]);
            _storages1 = archetype.Match<C1>(streamTypes[1]);

            Array.Fill(_counter, 0);
            _limiter[0] = _storages0.Count;
            _limiter[1] = _storages1.Count;

            _populated = _storages0.Count > 0 && _storages1.Count > 0;
        }


        internal (Storage<C0>, Storage<C1>) Select => (_storages0[_counter[0]], _storages1[_counter[1]]);

        internal bool Iterate() => FullPermutation(_counter, _limiter);

        internal bool Empty => !_populated;


        public void Dispose()
        {
            if (!_allocated) return;

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

        private readonly PooledList<Storage<C0>> _storages0;
        private readonly PooledList<Storage<C1>> _storages1;
        private readonly PooledList<Storage<C2>> _storages2;

        private readonly bool _allocated;
        private readonly bool _populated;


        internal Join(Archetype archetype, ImmutableArray<TypeExpression> streamTypes)
        {
            _allocated = true;

            _counter = ArrayPool<int>.Shared.Rent(streamTypes.Length);
            _limiter = ArrayPool<int>.Shared.Rent(streamTypes.Length);
            _storages0 = archetype.Match<C0>(streamTypes[0]);
            _storages1 = archetype.Match<C1>(streamTypes[1]);
            _storages2 = archetype.Match<C2>(streamTypes[2]);

            Array.Fill(_counter, 0);
            _limiter[0] = _storages0.Count;
            _limiter[1] = _storages1.Count;
            _limiter[2] = _storages2.Count;

            _populated = _storages0.Count > 0 && _storages1.Count > 0 && _storages2.Count > 0;
        }


        internal (Storage<C0>, Storage<C1>, Storage<C2>) Select => (_storages0[_counter[0]], _storages1[_counter[1]], _storages2[_counter[2]]);

        internal bool Iterate() => FullPermutation(_counter, _limiter);

        internal bool Empty => !_populated;


        public void Dispose()
        {
            if (!_allocated) return;

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

        private readonly PooledList<Storage<C0>> _storages0;
        private readonly PooledList<Storage<C1>> _storages1;
        private readonly PooledList<Storage<C2>> _storages2;
        private readonly PooledList<Storage<C3>> _storages3;

        private readonly bool _allocated;
        private readonly bool _populated;


        internal Join(Archetype archetype, ImmutableArray<TypeExpression> streamTypes)
        {
            _allocated = true;

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

            _populated = _storages0.Count > 0 && _storages1.Count > 0 && _storages2.Count > 0 && _storages3.Count > 0;
        }


        internal (Storage<C0>, Storage<C1>, Storage<C2>, Storage<C3>) Select => (_storages0[_counter[0]], _storages1[_counter[1]], _storages2[_counter[2]], _storages3[_counter[3]]);

        internal bool Iterate() => FullPermutation(_counter, _limiter);

        internal bool Empty => !_populated;


        public void Dispose()
        {
            if (!_allocated) return;

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

        private readonly PooledList<Storage<C0>> _storages0;
        private readonly PooledList<Storage<C1>> _storages1;
        private readonly PooledList<Storage<C2>> _storages2;
        private readonly PooledList<Storage<C3>> _storages3;
        private readonly PooledList<Storage<C4>> _storages4;

        private readonly bool _allocated;
        private readonly bool _populated;


        internal Join(Archetype archetype, ImmutableArray<TypeExpression> streamTypes)
        {
            _allocated = true;

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

            _populated = _storages0.Count > 0 && _storages1.Count > 0 && _storages2.Count > 0 && _storages3.Count > 0 && _storages4.Count > 0;
        }


        internal (Storage<C0>, Storage<C1>, Storage<C2>, Storage<C3>, Storage<C4>) Select => (_storages0[_counter[0]], _storages1[_counter[1]], _storages2[_counter[2]], _storages3[_counter[3]], _storages4[_counter[4]]);

        internal bool Iterate() => FullPermutation(_counter, _limiter);

        internal bool Empty => !_populated;


        public void Dispose()
        {
            if (!_allocated) return;

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