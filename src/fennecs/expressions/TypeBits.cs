// SPDX-License-Identifier: MIT

using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;

namespace fennecs;

/// <summary>
/// Dense bitset over <see cref="TypeID"/>s, allocated in 256-bit blocks.
/// </summary>
/// <remarks>
/// TypeIds are assigned monotonically, so bitsets created at different times differ in length;
/// all operations treat words beyond an operand's length as zero.
/// </remarks>
internal readonly struct TypeBits
{
    private readonly ulong[] _words;

    /// <summary>True if no bit is set; callers use this to skip whole-plane tests.</summary>
    internal readonly bool IsEmpty;

    /// <summary>256 bits per block: one <see cref="Vector256{T}"/> of ulongs.</summary>
    internal const int WordsPerBlock = 4;

    internal static readonly TypeBits Empty = new([]);


    internal TypeBits(ulong[] words)
    {
        _words = words;
        var empty = true;
        foreach (var word in words) empty &= word == 0;
        IsEmpty = empty;
    }


    /// <summary>Allocates a word array covering TypeIds 0..<paramref name="maxTypeId"/> in whole 256-bit blocks.</summary>
    internal static ulong[] AllocateFor(int maxTypeId) => new ulong[(maxTypeId / 256 + 1) * WordsPerBlock];


    // Shift counts on ulong are masked to 63 by the runtime, so 1ul << typeId addresses the bit within the word.
    internal static void Set(ulong[] words, TypeID typeId) => words[typeId >> 6] |= 1ul << typeId;


    private static Vector256<ulong> LoadOrZero(ulong[] words, int offset) =>
        offset + WordsPerBlock <= words.Length
            ? Vector256.LoadUnsafe(ref MemoryMarshal.GetArrayDataReference(words), (nuint)offset)
            : Vector256<ulong>.Zero;


    /// <summary>Is the bit for this TypeId set? (bits beyond this bitset's length are unset)</summary>
    internal bool Get(TypeID typeId)
    {
        var index = typeId >> 6;
        return index < _words.Length && (_words[index] & (1ul << typeId)) != 0;
    }


    /// <summary>Are all bits of <paramref name="required"/> also set in this bitset?</summary>
    internal bool ContainsAll(in TypeBits required)
    {
        if (required.IsEmpty) return true;

        var req = required._words;
        var have = _words;
        var shared = Math.Min(req.Length, have.Length);
        var i = 0;

        if (Vector256.IsHardwareAccelerated && shared >= WordsPerBlock)
        {
            ref var reqRef = ref MemoryMarshal.GetArrayDataReference(req);
            ref var haveRef = ref MemoryMarshal.GetArrayDataReference(have);
            for (; i + WordsPerBlock <= shared; i += WordsPerBlock)
            {
                var r = Vector256.LoadUnsafe(ref reqRef, (nuint)i);
                var h = Vector256.LoadUnsafe(ref haveRef, (nuint)i);
                if (Vector256.AndNot(r, h) != Vector256<ulong>.Zero) return false;
            }
        }

        for (; i < shared; i++)
            if ((req[i] & ~have[i]) != 0) return false;

        // Required bits beyond our length are misses.
        for (; i < req.Length; i++)
            if (req[i] != 0) return false;

        return true;
    }


    /// <summary>Do this bitset and <paramref name="other"/> share any set bit?</summary>
    internal bool Intersects(in TypeBits other)
    {
        if (IsEmpty || other.IsEmpty) return false;

        var a = _words;
        var b = other._words;
        var shared = Math.Min(a.Length, b.Length);
        var i = 0;

        if (Vector256.IsHardwareAccelerated && shared >= WordsPerBlock)
        {
            ref var aRef = ref MemoryMarshal.GetArrayDataReference(a);
            ref var bRef = ref MemoryMarshal.GetArrayDataReference(b);
            for (; i + WordsPerBlock <= shared; i += WordsPerBlock)
            {
                if ((Vector256.LoadUnsafe(ref aRef, (nuint)i) & Vector256.LoadUnsafe(ref bRef, (nuint)i))
                    != Vector256<ulong>.Zero) return true;
            }
        }

        for (; i < shared; i++)
            if ((a[i] & b[i]) != 0) return true;

        return false;
    }


    /// <summary>Is <paramref name="required"/> fully covered by the union <paramref name="a"/> | <paramref name="b"/>?</summary>
    internal static bool ContainsAll2(in TypeBits required, in TypeBits a, in TypeBits b)
    {
        if (required.IsEmpty) return true;

        var req = required._words;
        var aw = a._words;
        var bw = b._words;
        var i = 0;

        if (Vector256.IsHardwareAccelerated)
        {
            ref var reqRef = ref MemoryMarshal.GetArrayDataReference(req);
            for (; i + WordsPerBlock <= req.Length; i += WordsPerBlock)
            {
                var requiredBlock = Vector256.LoadUnsafe(ref reqRef, (nuint)i);
                if (Vector256.AndNot(requiredBlock, LoadOrZero(aw, i) | LoadOrZero(bw, i))
                    != Vector256<ulong>.Zero) return false;
            }
        }

        for (; i < req.Length; i++)
        {
            var have = (i < aw.Length ? aw[i] : 0) | (i < bw.Length ? bw[i] : 0);
            if ((req[i] & ~have) != 0) return false;
        }

        return true;
    }


    /// <summary>Is <paramref name="required"/> fully covered by the union of <paramref name="a"/>, <paramref name="b"/>, <paramref name="c"/>?</summary>
    internal static bool ContainsAll3(in TypeBits required, in TypeBits a, in TypeBits b, in TypeBits c)
    {
        if (required.IsEmpty) return true;

        var req = required._words;
        var aw = a._words;
        var bw = b._words;
        var cw = c._words;
        var i = 0;

        if (Vector256.IsHardwareAccelerated)
        {
            ref var reqRef = ref MemoryMarshal.GetArrayDataReference(req);
            for (; i + WordsPerBlock <= req.Length; i += WordsPerBlock)
            {
                var requiredBlock = Vector256.LoadUnsafe(ref reqRef, (nuint)i);
                var have = LoadOrZero(aw, i) | LoadOrZero(bw, i) | LoadOrZero(cw, i);
                if (Vector256.AndNot(requiredBlock, have) != Vector256<ulong>.Zero) return false;
            }
        }

        for (; i < req.Length; i++)
        {
            var have = (i < aw.Length ? aw[i] : 0) | (i < bw.Length ? bw[i] : 0) | (i < cw.Length ? cw[i] : 0);
            if ((req[i] & ~have) != 0) return false;
        }

        return true;
    }


    /// <summary>Does <paramref name="probe"/> intersect the union <paramref name="a"/> | <paramref name="b"/>?</summary>
    internal static bool Intersects2(in TypeBits probe, in TypeBits a, in TypeBits b)
    {
        if (probe.IsEmpty) return false;

        var pw = probe._words;
        var aw = a._words;
        var bw = b._words;
        var i = 0;

        if (Vector256.IsHardwareAccelerated)
        {
            ref var probeRef = ref MemoryMarshal.GetArrayDataReference(pw);
            for (; i + WordsPerBlock <= pw.Length; i += WordsPerBlock)
                if ((Vector256.LoadUnsafe(ref probeRef, (nuint)i) & (LoadOrZero(aw, i) | LoadOrZero(bw, i)))
                    != Vector256<ulong>.Zero) return true;
        }

        for (; i < pw.Length; i++)
        {
            var have = (i < aw.Length ? aw[i] : 0) | (i < bw.Length ? bw[i] : 0);
            if ((pw[i] & have) != 0) return true;
        }

        return false;
    }


    /// <summary>Does <paramref name="probe"/> intersect the union of <paramref name="a"/>, <paramref name="b"/>, <paramref name="c"/>?</summary>
    internal static bool Intersects3(in TypeBits probe, in TypeBits a, in TypeBits b, in TypeBits c)
    {
        if (probe.IsEmpty) return false;

        var pw = probe._words;
        var aw = a._words;
        var bw = b._words;
        var cw = c._words;
        var i = 0;

        if (Vector256.IsHardwareAccelerated)
        {
            ref var probeRef = ref MemoryMarshal.GetArrayDataReference(pw);
            for (; i + WordsPerBlock <= pw.Length; i += WordsPerBlock)
            {
                var have = LoadOrZero(aw, i) | LoadOrZero(bw, i) | LoadOrZero(cw, i);
                if ((Vector256.LoadUnsafe(ref probeRef, (nuint)i) & have) != Vector256<ulong>.Zero) return true;
            }
        }

        for (; i < pw.Length; i++)
        {
            var have = (i < aw.Length ? aw[i] : 0) | (i < bw.Length ? bw[i] : 0) | (i < cw.Length ? cw[i] : 0);
            if ((pw[i] & have) != 0) return true;
        }

        return false;
    }
}


/// <summary>
/// 256-bit bloom filter over packed <see cref="TypeExpression"/> values: a miss is certain absence,
/// a hit means "maybe present" and requires a precise confirm.
/// </summary>
/// <remarks>
/// Patterns compose by OR (<see cref="Union"/>); membership tests are superset tests. This is the extension
/// point for Family/inheritance matching: a filter that also ORs in patterns of ancestor expressions answers
/// "this-or-derived" queries with the same test.
/// </remarks>
internal readonly struct KeyBloom
{
    private readonly Vector256<ulong> _bits;

    internal static readonly KeyBloom Empty = default;

    private KeyBloom(Vector256<ulong> bits) => _bits = bits;


    internal bool IsEmpty => _bits == Vector256<ulong>.Zero;


    /// <summary>The k=2 bloom pattern of a raw 64-bit expression value.</summary>
    internal static KeyBloom Of(ulong raw)
    {
        var hash = Mix(raw);
        var bit1 = (int)(hash & 255);
        var bit2 = (int)((hash >> 8) & 255);

        Span<ulong> lanes = stackalloc ulong[4];
        lanes.Clear();
        lanes[bit1 >> 6] |= 1ul << bit1;
        lanes[bit2 >> 6] |= 1ul << bit2;

        return new(Vector256.Create(lanes[0], lanes[1], lanes[2], lanes[3]));
    }


    /// <summary>The combined filter containing everything in this filter and <paramref name="other"/>.</summary>
    internal KeyBloom Union(in KeyBloom other) => new(_bits | other._bits);


    /// <summary>May this filter contain all entries of <paramref name="pattern"/>? False is certain absence.</summary>
    internal bool MayContain(in KeyBloom pattern) => (pattern._bits & ~_bits) == Vector256<ulong>.Zero;


    /// <summary>
    /// Do the two filters share any bit? False certifies that NO entry of <paramref name="other"/> is present
    /// in this filter — the one-op group rejection for Not clauses (true still requires per-entry confirms).
    /// </summary>
    internal bool Intersects(in KeyBloom other) => (_bits & other._bits) != Vector256<ulong>.Zero;


    // splitmix64 finalizer: cheap, well-mixed bits from the structured expression value.
    private static ulong Mix(ulong z)
    {
        z ^= z >> 30;
        z *= 0xBF58476D1CE4E5B9ul;
        z ^= z >> 27;
        z *= 0x94D049BB133111EBul;
        z ^= z >> 31;
        return z;
    }
}
