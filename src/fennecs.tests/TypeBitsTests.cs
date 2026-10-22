namespace fennecs.tests;

public class TypeBitsTests
{
    private static TypeBits BitsOf(int maxTypeId, params int[] typeIds)
    {
        var words = TypeBits.AllocateFor(maxTypeId);
        foreach (var id in typeIds) TypeBits.Set(words, (TypeID)id);
        return new(words);
    }


    [Theory]
    [InlineData(1)]
    [InlineData(63)]
    [InlineData(64)]
    [InlineData(255)]
    [InlineData(256)]
    [InlineData(4094)]
    public void Set_Then_Get_Roundtrips(int typeId)
    {
        var bits = BitsOf(typeId, typeId);
        Assert.True(bits.Get((TypeID)typeId));
        Assert.False(bits.IsEmpty);
    }


    [Fact]
    public void Get_Beyond_Length_Is_False()
    {
        var bits = BitsOf(10, 3);
        Assert.False(bits.Get(3000));
    }


    [Fact]
    public void Empty_Is_Empty()
    {
        Assert.True(TypeBits.Empty.IsEmpty);
        Assert.True(BitsOf(1000).IsEmpty);
        Assert.False(TypeBits.Empty.Get(1));
    }


    [Fact]
    public void AllocateFor_Rounds_To_Blocks()
    {
        Assert.Equal(4, TypeBits.AllocateFor(0).Length);
        Assert.Equal(4, TypeBits.AllocateFor(255).Length);
        Assert.Equal(8, TypeBits.AllocateFor(256).Length);
        Assert.Equal(64, TypeBits.AllocateFor(4094).Length);
    }


    [Fact]
    public void ContainsAll_Subset_And_Superset()
    {
        var super = BitsOf(300, 1, 64, 200, 299);
        var sub = BitsOf(300, 64, 299);
        var other = BitsOf(300, 64, 128);

        Assert.True(super.ContainsAll(sub));
        Assert.True(super.ContainsAll(super));
        Assert.False(sub.ContainsAll(super));
        Assert.False(super.ContainsAll(other));
        Assert.True(super.ContainsAll(TypeBits.Empty));
        Assert.True(TypeBits.Empty.ContainsAll(BitsOf(300)));
    }


    [Fact]
    public void ContainsAll_Zero_Extends_Both_Directions()
    {
        var shortBits = BitsOf(10, 3, 7);          // one block
        var longSame = BitsOf(3000, 3, 7);         // many blocks, same content
        var longHigh = BitsOf(3000, 3, 7, 2999);   // high bit beyond short's length

        Assert.True(shortBits.ContainsAll(longSame));
        Assert.True(longSame.ContainsAll(shortBits));
        Assert.False(shortBits.ContainsAll(longHigh));
        Assert.True(longHigh.ContainsAll(shortBits));
    }


    [Fact]
    public void Intersects_Basics_And_Lengths()
    {
        var a = BitsOf(10, 3, 7);
        var b = BitsOf(3000, 7, 2999);
        var c = BitsOf(3000, 2999);

        Assert.True(a.Intersects(b));
        Assert.True(b.Intersects(a));
        Assert.False(a.Intersects(c));
        Assert.False(c.Intersects(a));
        Assert.False(a.Intersects(TypeBits.Empty));
        Assert.False(TypeBits.Empty.Intersects(a));
    }


    [Fact]
    public void Fused_Union_Operations_Match_Manual_Union()
    {
        var a = BitsOf(100, 1, 2);
        var b = BitsOf(400, 300);
        var c = BitsOf(100, 50);

        Assert.True(TypeBits.ContainsAll2(BitsOf(400, 1, 300), a, b));
        Assert.False(TypeBits.ContainsAll2(BitsOf(400, 1, 300, 399), a, b));
        Assert.True(TypeBits.ContainsAll3(BitsOf(400, 2, 50, 300), a, b, c));
        Assert.False(TypeBits.ContainsAll3(BitsOf(400, 2, 50, 301), a, b, c));

        Assert.True(TypeBits.Intersects2(BitsOf(400, 300, 99), a, b));
        Assert.False(TypeBits.Intersects2(BitsOf(400, 99, 50), a, b));
        Assert.True(TypeBits.Intersects3(BitsOf(100, 50), a, b, c));
        Assert.False(TypeBits.Intersects3(BitsOf(4000, 3999), a, b, c));
    }


    [Fact]
    public void Fused_Union_Operations_Agree_Across_Blocks_And_Lengths()
    {
        var random = new Random(20260806);

        for (var round = 0; round < 500; round++)
        {
            var sets = Enumerable.Range(0, 4)
                .Select(_ => new HashSet<int>(Enumerable.Range(0, random.Next(20))
                    .Select(_ => random.Next(1, 4095))))
                .ToArray();
            var bits = sets.Select(set => BitsOf(set.Count == 0 ? 1 : set.Max(), set.ToArray())).ToArray();
            var union2 = new HashSet<int>(sets[1]);
            union2.UnionWith(sets[2]);
            var union3 = new HashSet<int>(union2);
            union3.UnionWith(sets[3]);

            Assert.Equal(union2.IsSupersetOf(sets[0]), TypeBits.ContainsAll2(bits[0], bits[1], bits[2]));
            Assert.Equal(union3.IsSupersetOf(sets[0]), TypeBits.ContainsAll3(bits[0], bits[1], bits[2], bits[3]));
            Assert.Equal(sets[0].Overlaps(union2), TypeBits.Intersects2(bits[0], bits[1], bits[2]));
            Assert.Equal(sets[0].Overlaps(union3), TypeBits.Intersects3(bits[0], bits[1], bits[2], bits[3]));
        }
    }


    [Fact]
    public void Randomized_Agreement_With_Set_Reference()
    {
        var random = new Random(20260805);

        for (var round = 0; round < 500; round++)
        {
            var maxA = random.Next(1, 4094);
            var maxB = random.Next(1, 4094);
            var setA = new HashSet<int>(Enumerable.Range(0, random.Next(0, 20)).Select(_ => random.Next(1, maxA + 1)));
            var setB = new HashSet<int>(Enumerable.Range(0, random.Next(0, 20)).Select(_ => random.Next(1, maxB + 1)));

            // Sometimes force subset/overlap relationships to hit the true branches.
            if (round % 3 == 0) setB.UnionWith(setA);
            if (round % 5 == 0 && setA.Count > 0) setB.Add(setA.First());

            var bitsA = BitsOf(maxA, setA.ToArray());
            var bitsB = BitsOf(Math.Max(maxB, setB.Count == 0 ? 1 : setB.Max()), setB.ToArray());

            Assert.Equal(setA.IsSupersetOf(setB), bitsA.ContainsAll(bitsB));
            Assert.Equal(setB.IsSupersetOf(setA), bitsB.ContainsAll(bitsA));
            Assert.Equal(setA.Overlaps(setB), bitsA.Intersects(bitsB));
        }
    }


    [Fact]
    public void Bloom_Pattern_Is_Deterministic()
    {
        var a = KeyBloom.Of(0xDEADBEEF12345678ul);
        var b = KeyBloom.Of(0xDEADBEEF12345678ul);

        Assert.True(a.MayContain(b));
        Assert.True(b.MayContain(a));
        Assert.False(a.IsEmpty);
    }


    [Fact]
    public void Bloom_Never_False_Negative()
    {
        var random = new Random(42);
        var values = Enumerable.Range(0, 64).Select(_ => (ulong)random.NextInt64()).ToArray();

        var filter = KeyBloom.Empty;
        foreach (var value in values) filter = filter.Union(KeyBloom.Of(value));

        foreach (var value in values) Assert.True(filter.MayContain(KeyBloom.Of(value)));
    }


    [Fact]
    public void Bloom_Empty_Rejects_Everything()
    {
        Assert.True(KeyBloom.Empty.IsEmpty);
        Assert.False(KeyBloom.Empty.MayContain(KeyBloom.Of(123456789ul)));
        Assert.True(KeyBloom.Empty.MayContain(KeyBloom.Empty));
    }


    [Fact]
    public void Bloom_Intersects_Certifies_Group_Absence()
    {
        var present = KeyBloom.Of(0x1111_2222_3333_4444ul).Union(KeyBloom.Of(0x5555_6666_7777_8888ul));
        var absentGroup = KeyBloom.Of(0x9999_AAAA_BBBB_CCCCul).Union(KeyBloom.Of(0xDDDD_EEEE_FFFF_0000ul));

        // Overlap with itself is guaranteed; disjointness certifies absence of every group entry.
        Assert.True(present.Intersects(present));
        Assert.False(present.Intersects(absentGroup));
        Assert.False(present.MayContain(KeyBloom.Of(0x9999_AAAA_BBBB_CCCCul)));
        Assert.False(present.MayContain(KeyBloom.Of(0xDDDD_EEEE_FFFF_0000ul)));
        Assert.False(present.Intersects(KeyBloom.Empty));
        Assert.False(KeyBloom.Empty.Intersects(present));
    }


    [Fact]
    public void Bloom_Rejects_Most_Absent_Values()
    {
        var random = new Random(7);
        var filter = KeyBloom.Empty;
        for (var i = 0; i < 8; i++) filter = filter.Union(KeyBloom.Of((ulong)random.NextInt64()));

        var falsePositives = 0;
        for (var i = 0; i < 10_000; i++)
            if (filter.MayContain(KeyBloom.Of((ulong)random.NextInt64()))) falsePositives++;

        // k=2, m=256, n=8: expected FP rate ≈ 0.4%; allow generous headroom against seed variance.
        Assert.True(falsePositives < 300, $"Bloom false-positive rate too high: {falsePositives}/10000");
    }
}
