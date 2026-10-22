using System.Buffers;
using System.Collections;
using System.Diagnostics;
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
        Debug.Assert(counter.Length == limiter.Length, "The counter and limiter spans should have the same sizes.");

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
        private readonly Storage<C0> _direct0;

        private readonly bool _allocated;
        private readonly bool _direct;
        private readonly bool _populated;


        /// <summary>
        /// Cross-Joins the Archetype with a list of StreamTypes.
        /// </summary>
        internal Join(Archetype archetype, ReadOnlySpan<TypeExpression> streamTypes)
        {
            Debug.Assert(streamTypes.Length == 1, "Not the right amount of stream types.");

            if (!streamTypes[0].isWildcard)
            {
                _counter = null!;
                _limiter = null!;
                _storages0 = null!;
                _allocated = false;
                _direct = true;
                _populated = archetype.TryGetStorage(streamTypes[0], out var storage0);
                _direct0 = _populated ? (Storage<C0>)storage0 : null!;
                return;
            }

            _allocated = true;
            _direct = false;
            _direct0 = null!;

            _counter = ArrayPool.Rent(1);
            _limiter = ArrayPool.Rent(1);
            _storages0 = archetype.Match<C0>(streamTypes[0]);

            Array.Fill(_counter, 0, 0, 1);
            _limiter[0] = _storages0.Count;

            _populated = _storages0.Count > 0;
        }

        /// <summary>
        /// Returns the Storage in the current internal Cross Join counter configuration.
        /// Call <see cref="Iterate"/> to select the next permutation.
        /// </summary>
        internal Storage<C0> Select => _direct ? _direct0 : _storages0[_counter[0]];

        // Test seam: bookkeeping internals for disposal / pool round-trip verification.
        internal (int[] Counter, int[] Limiter, IEnumerable[] Storages) TestState =>
            (_counter, _limiter, [_storages0]);


        /// <summary>
        /// Ticks the internal counter of the Join operation, readying the next permutation to use in <see cref="Select"/>.
        /// </summary>
        /// <returns>
        /// <c>true</c> if permutation exists,<br/><c>false</c> if the Cross Join has exhausted all permutations.
        /// </returns>
        internal bool Iterate()
        {
            if (_direct) return false;
            Debug.Assert(_counter is { Length: >= 1 });
            Debug.Assert(_limiter is { Length: >= 1 });
            return FullPermutation(_counter.AsSpan(0, 1), _limiter.AsSpan(0, 1));
        }


        /// <summary>
        /// Returns <c>true</c> if the Join is empty, i.e. no permutations are available.
        /// </summary>
        internal bool Empty => !_populated;


        public void Dispose()
        {
            if (_direct || !_allocated) return;

            _storages0.Dispose();
            ArrayPool.Return(_counter);
            ArrayPool.Return(_limiter);
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
        private readonly Storage<C0> _direct0;
        private readonly Storage<C1> _direct1;

        private readonly bool _allocated;
        private readonly bool _direct;
        private readonly bool _populated;

        internal Join(Archetype archetype, ReadOnlySpan<TypeExpression> streamTypes)
        {
            Debug.Assert(streamTypes.Length == 2, "Not the right amount of stream types.");

            if (!streamTypes[0].isWildcard && !streamTypes[1].isWildcard)
            {
                _counter = null!;
                _limiter = null!;
                _storages0 = null!;
                _storages1 = null!;
                _allocated = false;
                _direct = true;
                var has0 = archetype.TryGetStorage(streamTypes[0], out var storage0);
                var has1 = archetype.TryGetStorage(streamTypes[1], out var storage1);
                _populated = has0 && has1;
                _direct0 = has0 ? (Storage<C0>)storage0 : null!;
                _direct1 = has1 ? (Storage<C1>)storage1 : null!;
                return;
            }

            _allocated = true;
            _direct = false;
            _direct0 = null!;
            _direct1 = null!;

            _counter = ArrayPool.Rent(2);
            _limiter = ArrayPool.Rent(2);
            _storages0 = archetype.Match<C0>(streamTypes[0]);
            _storages1 = archetype.Match<C1>(streamTypes[1]);

            Array.Fill(_counter, 0, 0, 2);
            _limiter[0] = _storages0.Count;
            _limiter[1] = _storages1.Count;

            _populated = _storages0.Count > 0 && _storages1.Count > 0;
        }


        internal (Storage<C0>, Storage<C1>) Select => _direct
            ? (_direct0, _direct1)
            : (_storages0[_counter[0]], _storages1[_counter[1]]);

        // Test seam: bookkeeping internals for disposal / pool round-trip verification.
        internal (int[] Counter, int[] Limiter, IEnumerable[] Storages) TestState =>
            (_counter, _limiter, [_storages0, _storages1]);

        internal bool Iterate()
        {
            if (_direct) return false;
            Debug.Assert(_counter is { Length: >= 2 });
            Debug.Assert(_limiter is { Length: >= 2 });
            return FullPermutation(_counter.AsSpan(0, 2), _limiter.AsSpan(0, 2));
        }

        internal bool Empty => !_populated;


        public void Dispose()
        {
            if (_direct || !_allocated) return;

            _storages0.Dispose();
            _storages1.Dispose();
            ArrayPool.Return(_counter);
            ArrayPool.Return(_limiter);
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
        private readonly Storage<C0> _direct0;
        private readonly Storage<C1> _direct1;
        private readonly Storage<C2> _direct2;

        private readonly bool _allocated;
        private readonly bool _direct;
        private readonly bool _populated;


        internal Join(Archetype archetype, ReadOnlySpan<TypeExpression> streamTypes)
        {
            Debug.Assert(streamTypes.Length == 3, "Not the right amount of stream types.");

            if (!streamTypes[0].isWildcard && !streamTypes[1].isWildcard && !streamTypes[2].isWildcard)
            {
                _counter = null!;
                _limiter = null!;
                _storages0 = null!;
                _storages1 = null!;
                _storages2 = null!;
                _allocated = false;
                _direct = true;
                var has0 = archetype.TryGetStorage(streamTypes[0], out var storage0);
                var has1 = archetype.TryGetStorage(streamTypes[1], out var storage1);
                var has2 = archetype.TryGetStorage(streamTypes[2], out var storage2);
                _populated = has0 && has1 && has2;
                _direct0 = has0 ? (Storage<C0>)storage0 : null!;
                _direct1 = has1 ? (Storage<C1>)storage1 : null!;
                _direct2 = has2 ? (Storage<C2>)storage2 : null!;
                return;
            }

            _allocated = true;
            _direct = false;
            _direct0 = null!;
            _direct1 = null!;
            _direct2 = null!;

            _counter = ArrayPool.Rent(3);
            _limiter = ArrayPool.Rent(3);
            _storages0 = archetype.Match<C0>(streamTypes[0]);
            _storages1 = archetype.Match<C1>(streamTypes[1]);
            _storages2 = archetype.Match<C2>(streamTypes[2]);

            Array.Fill(_counter, 0, 0, 3);
            _limiter[0] = _storages0.Count;
            _limiter[1] = _storages1.Count;
            _limiter[2] = _storages2.Count;

            _populated = _storages0.Count > 0 && _storages1.Count > 0 && _storages2.Count > 0;
        }


        internal (Storage<C0>, Storage<C1>, Storage<C2>) Select => _direct
            ? (_direct0, _direct1, _direct2)
            : (_storages0[_counter[0]], _storages1[_counter[1]], _storages2[_counter[2]]);

        // Test seam: bookkeeping internals for disposal / pool round-trip verification.
        internal (int[] Counter, int[] Limiter, IEnumerable[] Storages) TestState =>
            (_counter, _limiter, [_storages0, _storages1, _storages2]);

        internal bool Iterate()
        {
            if (_direct) return false;
            Debug.Assert(_counter is { Length: >= 3 });
            Debug.Assert(_limiter is { Length: >= 3 });
            return FullPermutation(_counter.AsSpan(0, 3), _limiter.AsSpan(0, 3));
        }

        internal bool Empty => !_populated;


        public void Dispose()
        {
            if (_direct || !_allocated) return;

            _storages0.Dispose();
            _storages1.Dispose();
            _storages2.Dispose();
            ArrayPool.Return(_counter);
            ArrayPool.Return(_limiter);
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
        private readonly Storage<C0> _direct0;
        private readonly Storage<C1> _direct1;
        private readonly Storage<C2> _direct2;
        private readonly Storage<C3> _direct3;

        private readonly bool _allocated;
        private readonly bool _direct;
        private readonly bool _populated;


        internal Join(Archetype archetype, ReadOnlySpan<TypeExpression> streamTypes)
        {
            Debug.Assert(streamTypes.Length == 4, "Not the right amount of stream types.");

            if (!streamTypes[0].isWildcard && !streamTypes[1].isWildcard && !streamTypes[2].isWildcard &&
                !streamTypes[3].isWildcard)
            {
                _counter = null!;
                _limiter = null!;
                _storages0 = null!;
                _storages1 = null!;
                _storages2 = null!;
                _storages3 = null!;
                _allocated = false;
                _direct = true;
                var has0 = archetype.TryGetStorage(streamTypes[0], out var storage0);
                var has1 = archetype.TryGetStorage(streamTypes[1], out var storage1);
                var has2 = archetype.TryGetStorage(streamTypes[2], out var storage2);
                var has3 = archetype.TryGetStorage(streamTypes[3], out var storage3);
                _populated = has0 && has1 && has2 && has3;
                _direct0 = has0 ? (Storage<C0>)storage0 : null!;
                _direct1 = has1 ? (Storage<C1>)storage1 : null!;
                _direct2 = has2 ? (Storage<C2>)storage2 : null!;
                _direct3 = has3 ? (Storage<C3>)storage3 : null!;
                return;
            }

            _allocated = true;
            _direct = false;
            _direct0 = null!;
            _direct1 = null!;
            _direct2 = null!;
            _direct3 = null!;

            _counter = ArrayPool.Rent(4);
            _limiter = ArrayPool.Rent(4);
            _storages0 = archetype.Match<C0>(streamTypes[0]);
            _storages1 = archetype.Match<C1>(streamTypes[1]);
            _storages2 = archetype.Match<C2>(streamTypes[2]);
            _storages3 = archetype.Match<C3>(streamTypes[3]);

            Array.Fill(_counter, 0, 0, 4);
            _limiter[0] = _storages0.Count;
            _limiter[1] = _storages1.Count;
            _limiter[2] = _storages2.Count;
            _limiter[3] = _storages3.Count;

            _populated = _storages0.Count > 0 && _storages1.Count > 0 && _storages2.Count > 0 && _storages3.Count > 0;
        }


        internal (Storage<C0>, Storage<C1>, Storage<C2>, Storage<C3>) Select => _direct
            ? (_direct0, _direct1, _direct2, _direct3)
            : (_storages0[_counter[0]], _storages1[_counter[1]], _storages2[_counter[2]], _storages3[_counter[3]]);

        // Test seam: bookkeeping internals for disposal / pool round-trip verification.
        internal (int[] Counter, int[] Limiter, IEnumerable[] Storages) TestState =>
            (_counter, _limiter, [_storages0, _storages1, _storages2, _storages3]);

        internal bool Iterate()
        {
            if (_direct) return false;
            Debug.Assert(_counter is { Length: >= 4 });
            Debug.Assert(_limiter is { Length: >= 4 });
            return FullPermutation(_counter.AsSpan(0, 4), _limiter.AsSpan(0, 4));
        }

        internal bool Empty => !_populated;


        public void Dispose()
        {
            if (_direct || !_allocated) return;

            _storages0.Dispose();
            _storages1.Dispose();
            _storages2.Dispose();
            _storages3.Dispose();
            ArrayPool.Return(_counter);
            ArrayPool.Return(_limiter);
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
        private readonly Storage<C0> _direct0;
        private readonly Storage<C1> _direct1;
        private readonly Storage<C2> _direct2;
        private readonly Storage<C3> _direct3;
        private readonly Storage<C4> _direct4;

        private readonly bool _allocated;
        private readonly bool _direct;
        private readonly bool _populated;


        internal Join(Archetype archetype, ReadOnlySpan<TypeExpression> streamTypes)
        {
            Debug.Assert(streamTypes.Length == 5, "Not the right amount of stream types.");

            if (!streamTypes[0].isWildcard && !streamTypes[1].isWildcard && !streamTypes[2].isWildcard &&
                !streamTypes[3].isWildcard && !streamTypes[4].isWildcard)
            {
                _counter = null!;
                _limiter = null!;
                _storages0 = null!;
                _storages1 = null!;
                _storages2 = null!;
                _storages3 = null!;
                _storages4 = null!;
                _allocated = false;
                _direct = true;
                var has0 = archetype.TryGetStorage(streamTypes[0], out var storage0);
                var has1 = archetype.TryGetStorage(streamTypes[1], out var storage1);
                var has2 = archetype.TryGetStorage(streamTypes[2], out var storage2);
                var has3 = archetype.TryGetStorage(streamTypes[3], out var storage3);
                var has4 = archetype.TryGetStorage(streamTypes[4], out var storage4);
                _populated = has0 && has1 && has2 && has3 && has4;
                _direct0 = has0 ? (Storage<C0>)storage0 : null!;
                _direct1 = has1 ? (Storage<C1>)storage1 : null!;
                _direct2 = has2 ? (Storage<C2>)storage2 : null!;
                _direct3 = has3 ? (Storage<C3>)storage3 : null!;
                _direct4 = has4 ? (Storage<C4>)storage4 : null!;
                return;
            }

            _allocated = true;
            _direct = false;
            _direct0 = null!;
            _direct1 = null!;
            _direct2 = null!;
            _direct3 = null!;
            _direct4 = null!;

            _counter = ArrayPool.Rent(5);
            _limiter = ArrayPool.Rent(5);
            _storages0 = archetype.Match<C0>(streamTypes[0]);
            _storages1 = archetype.Match<C1>(streamTypes[1]);
            _storages2 = archetype.Match<C2>(streamTypes[2]);
            _storages3 = archetype.Match<C3>(streamTypes[3]);
            _storages4 = archetype.Match<C4>(streamTypes[4]);

            Array.Fill(_counter, 0, 0, 5);
            _limiter[0] = _storages0.Count;
            _limiter[1] = _storages1.Count;
            _limiter[2] = _storages2.Count;
            _limiter[3] = _storages3.Count;
            _limiter[4] = _storages4.Count;

            _populated = _storages0.Count > 0 && _storages1.Count > 0 && _storages2.Count > 0 && _storages3.Count > 0 &&
                         _storages4.Count > 0;
        }


        internal (Storage<C0>, Storage<C1>, Storage<C2>, Storage<C3>, Storage<C4>) Select => _direct
            ? (_direct0, _direct1, _direct2, _direct3, _direct4)
            : (_storages0[_counter[0]], _storages1[_counter[1]], _storages2[_counter[2]], _storages3[_counter[3]],
                _storages4[_counter[4]]);

        // Test seam: bookkeeping internals for disposal / pool round-trip verification.
        internal (int[] Counter, int[] Limiter, IEnumerable[] Storages) TestState =>
            (_counter, _limiter, [_storages0, _storages1, _storages2, _storages3, _storages4]);

        internal bool Iterate()
        {
            if (_direct) return false;
            Debug.Assert(_counter is { Length: >= 5 });
            Debug.Assert(_limiter is { Length: >= 5 });
            return FullPermutation(_counter.AsSpan(0, 5), _limiter.AsSpan(0, 5));
        }

        internal bool Empty => !_populated;


        public void Dispose()
        {
            if (_direct || !_allocated) return;

            _storages0.Dispose();
            _storages1.Dispose();
            _storages2.Dispose();
            _storages3.Dispose();
            _storages4.Dispose();
            ArrayPool.Return(_counter);
            ArrayPool.Return(_limiter);
        }
    }

    #endregion


    #region Read Join

    /// <summary>
    /// Read-only Cross-Join over untyped storages: serves every stream type form, including
    /// <see cref="fennecs.Match.Family"/>, whose derived storages are viewed covariantly as their base type.
    /// </summary>
    internal readonly struct ReadJoin : IDisposable
    {
        private readonly int[] _counter;
        private readonly int[] _limiter;

        private readonly PooledList<IStorage> _storages0;
        private readonly PooledList<IStorage>? _storages1;
        private readonly PooledList<IStorage>? _storages2;
        private readonly PooledList<IStorage>? _storages3;
        private readonly PooledList<IStorage>? _storages4;

        private readonly int _arity;
        private readonly bool _populated;


        internal ReadJoin(Archetype archetype, ReadOnlySpan<TypeExpression> streamTypes)
        {
            Debug.Assert(streamTypes.Length is >= 1 and <= 5, "Unsupported stream type arity.");

            _arity = streamTypes.Length;
            _counter = ArrayPool.Rent(_arity);
            _limiter = ArrayPool.Rent(_arity);
            _populated = true;

            _storages0 = archetype.MatchReadable(streamTypes[0]);
            if (_arity > 1) _storages1 = archetype.MatchReadable(streamTypes[1]);
            if (_arity > 2) _storages2 = archetype.MatchReadable(streamTypes[2]);
            if (_arity > 3) _storages3 = archetype.MatchReadable(streamTypes[3]);
            if (_arity > 4) _storages4 = archetype.MatchReadable(streamTypes[4]);

            for (var slot = 0; slot < _arity; slot++)
            {
                _counter[slot] = 0;
                _limiter[slot] = Slot(slot).Count;
                _populated &= _limiter[slot] > 0;
            }
        }


        private PooledList<IStorage> Slot(int slot) => slot switch
        {
            0 => _storages0,
            1 => _storages1!,
            2 => _storages2!,
            3 => _storages3!,
            _ => _storages4!,
        };


        /// <summary>Read-only view of the current permutation's storage for the given slot, as C.</summary>
        internal ReadOnlySpan<C> Span<C>(int slot) => Slot(slot)[_counter[slot]].ReadAs<C>();

        /// <summary>Single element of the current permutation's storage for the given slot, as C.</summary>
        internal C Get<C>(int slot, int row) => Slot(slot)[_counter[slot]].GetAs<C>(row);


        internal bool Iterate()
        {
            Debug.Assert(_counter.Length >= _arity && _limiter.Length >= _arity);
            return FullPermutation(_counter.AsSpan(0, _arity), _limiter.AsSpan(0, _arity));
        }


        internal bool Empty => !_populated;


        public void Dispose()
        {
            if (_counter is null) return;

            _storages0.Dispose();
            _storages1?.Dispose();
            _storages2?.Dispose();
            _storages3?.Dispose();
            _storages4?.Dispose();
            ArrayPool.Return(_counter);
            ArrayPool.Return(_limiter);
        }
    }

    #endregion

    private static readonly ArrayPool<int> ArrayPool = ArrayPool<int>.Create();
}
