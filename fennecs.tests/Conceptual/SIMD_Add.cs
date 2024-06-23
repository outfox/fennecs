namespace fennecs.tests.Conceptual;

public class SIMD_Add
{
    private record struct TestInt(int Value);
    private record struct TestInt2(int Value);
    private record struct TestInt3(int Value);
    
    private record struct TestFloat(int Value);

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(10)]
    [InlineData(69)]
    [InlineData(420)]
    [InlineData(4096)]
    public void CanAddI32(int count)
    {
        using var world = new World();

        var query = world.Query<TestInt>().Compile();
        var simd = new SIMD(query);

        world.Entity().Add<TestInt>(new (69)).Spawn(count);
        
        simd.Add(Comp<TestInt>.Plain, 42);

        var stream = query.Stream<TestInt>();
        foreach (var (_, test) in stream)
        {
            Assert.Equal(111, test.Value);
        }
        Assert.Equal(count, stream.Count);
    }

    /*
    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    [InlineData(5)]
    [InlineData(6)]
    [InlineData(7)]
    [InlineData(8)]
    [InlineData(9)]
    [InlineData(10)]
    [InlineData(11)]
    [InlineData(12)]
    [InlineData(13)]
    [InlineData(14)]
    [InlineData(15)]
    [InlineData(16)]
    public void CanAddI32safe(int count)
    {
        using var world = new World();

        var query = world.Query<TestInt>().Compile();
        var simd = new SIMD(query);

        world.Entity().Add<TestInt>().Spawn(count);
        
        simd.AddImplScalar(69, default);
        simd.AddImplScalar(11, default);
        simd.AddImplScalar(1, default);
        simd.AddImplScalar(42, default);
        
        var stream = query.Stream<TestInt>();
        foreach (var (_, test) in stream)
        {
            Assert.Equal(123, test.Value);
        }
        Assert.Equal(count, stream.Count);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(10)]
    [InlineData(69)]
    [InlineData(420)]
    [InlineData(4096)]
    public void CanSumI32_2Operands(int count)
    {
        using var world = new World();

        var query = world.Query<TestInt>().Compile();
        var simd = new SIMD(query);

        world.Entity()
            .Add(new TestInt(5))
            .Add(new TestInt2(7))
            .Spawn(count);
        
        simd.AddI32<TestInt, TestInt, TestInt2>(new (default), new (default), new (default));
        
        var stream = query.Stream<TestInt>();
        foreach (var (_, test) in stream)
        {
            Assert.Equal(12, test.Value);
        }
        Assert.Equal(count, stream.Count);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(10)]
    [InlineData(69)]
    [InlineData(420)]
    [InlineData(4096)]
    public void CanSumI32_2Operands_Separate_Destination(int count)
    {
        using var world = new World();

        var query = world.Query<TestInt>().Compile();
        var simd = new SIMD(query);

        world.Entity()
            .Add(new TestInt(0))
            .Add(new TestInt2(5))
            .Add(new TestInt3(7))
            .Spawn(count);
        
        simd.AddI32<TestInt, TestInt2, TestInt3>(new (default), new (default), new (default));
        
        var stream = query.Stream<TestInt>();
        foreach (var (_, test) in stream)
        {
            Assert.Equal(12, test.Value);
        }
        Assert.Equal(count, stream.Count);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(10)]
    [InlineData(69)]
    [InlineData(420)]
    [InlineData(4096)]
    public void CanSumI32_3Operands(int count)
    {
        using var world = new World();

        var query = world.Query<TestInt>().Compile();
        var simd = new SIMD(query);

        world.Entity()
            .Add(new TestInt(5))
            .Add(new TestInt2(7))
            .Add(new TestInt3(30))
            .Spawn(count);
        
        simd.AddI32<TestInt, TestInt, TestInt2, TestInt3>(new (default), new (default), new (default), new (default));
        
        var stream = query.Stream<TestInt>();
        foreach (var (_, test) in stream)
        {
            Assert.Equal(42, test.Value);
        }
        Assert.Equal(count, stream.Count);
    }
    */
}
