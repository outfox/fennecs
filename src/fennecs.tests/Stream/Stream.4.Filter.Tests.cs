namespace fennecs.tests.Stream;

public class Stream4FilterTests
{
    private readonly record struct Test1(int Value) : IComparable<int>, IComparable<Test1>
    {
        public int CompareTo(Test1 other)
        {
            return Value.CompareTo(other.Value);
        }

        public int CompareTo(int other)
        {
            return Value.CompareTo(other);
        }
    }

    private readonly record struct Test2(float Value) : IComparable<float>
    {
        public int CompareTo(float other)
        {
            return Value.CompareTo(other);
        }
    }

    private readonly record struct Test3(string Name);

    private readonly record struct Test4(bool Flag);

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(10)]
    [InlineData(100)]
    [InlineData(1312)]
    public void ForFilterWithLambda_4Types(int count)
    {
        using var world = new World();

        for (var i = 0; i < count; i++)
        {
            world.Spawn()
                .Add(new Test1(Random.Shared.Next(i)))
                .Add(new Test2(Random.Shared.NextSingle() * 100))
                .Add(new Test3("Entity_" + i))
                .Add(new Test4(i % 2 == 0));
        }
        
        var all = world.Stream<Test1, Test2, Test3, Test4>();

        var topHalfFloat = all.Where((in Test2 t) => t.Value > 50.0f);
        var botHalfFloat = all.Where((in Test2 t) => t.Value <= 50.0f);
        
        var top = 0;
        var bot = 0;
        topHalfFloat.For((ref _, ref t2, ref t3, ref t4) =>
        {
            top++;
            Assert.True(t2.Value > 50);
            Assert.NotNull(t3.Name);
            Assert.IsType<bool>(t4.Flag);
        });

        botHalfFloat.For((ref _, ref t2, ref t3, ref t4) =>
        {
            bot++;
            Assert.True(t2.Value <= 50);
            Assert.StartsWith("Entity_", t3.Name);
            Assert.IsType<bool>(t4.Flag);
        });
        
        Assert.Equal(count, top + bot);
    }
    
    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(10)]
    [InlineData(100)]
    [InlineData(1312)]
    public void JobFilterWithLambda_4Types(int count)
    {
        using var world = new World();

        for (var i = 0; i < count; i++)
        {
            world.Spawn()
                .Add(new Test1(Random.Shared.Next(i)))
                .Add(new Test2(Random.Shared.NextSingle() * 100))
                .Add(new Test3("Entity_" + i))
                .Add(new Test4(i % 2 == 0));
        }
        
        var all = world.Stream<Test1, Test2, Test3, Test4>();

        var topHalfFloat = all.Where((in Test2 t) => t.Value > 50.0f);
        var botHalfFloat = all.Where((in Test2 t) => t.Value <= 50.0f);
        
        var top = 0;
        var bot = 0;
        topHalfFloat.Job((ref _, ref t2, ref t3, ref t4) =>
        {
            Interlocked.Increment(ref top);
            Assert.True(t2.Value > 50);
            Assert.NotNull(t3.Name);
            Assert.IsType<bool>(t4.Flag);
        });

        botHalfFloat.Job((ref _, ref t2, ref t3, ref t4) =>
        {
            Interlocked.Increment(ref bot);
            Assert.True(t2.Value <= 50);
            Assert.StartsWith("Entity_", t3.Name);
            Assert.IsType<bool>(t4.Flag);
        });
        
        Assert.Equal(count, top + bot);
    }    
}