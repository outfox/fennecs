namespace fennecs.tests.Stream;

public class Stream1FilterTests
{
    private readonly record struct Test2(float Value) : IComparable<float>
    {
        public int CompareTo(float other)
        {
            return Value.CompareTo(other);
        }
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(10)]
    [InlineData(100)]
    [InlineData(1312)]
    public void ForFilterWithLambda(int count)
    {
        using var world = new World();

        for (var i = 0; i < count; i++)
        {
            world.Spawn()
                .Add(new Test2(Random.Shared.NextSingle() * 100));
        }
        
        var all = world.Stream<Test2>();

        var topHalfFloat = all.Where((in t) => t.Value > 50.0f);
        var botHalfFloat = all.Where((in t) => t.Value <= 50.0f);
        
        var top = 0;
        var bot = 0;
        topHalfFloat.For((ref t2) =>
        {
            top++;
            Assert.True(t2.Value > 50);
        });

        botHalfFloat.For((ref t2) =>
        {
            bot++;
            Assert.True(t2.Value <= 50);
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
    public void JobFilterWithLambda(int count)
    {
        using var world = new World();

        for (var i = 0; i < count; i++)
        {
            world.Spawn()
                .Add(new Test2(Random.Shared.NextSingle() * 100));
        }
        
        var all = world.Stream<Test2>();

        var topHalfFloat = all.Where((in t) => t.Value > 50.0f);
        var botHalfFloat = all.Where((in t) => t.Value <= 50.0f);
        
        var top = 0;
        var bot = 0;
        topHalfFloat.Job((ref t2) =>
        {
            Interlocked.Increment(ref top);
            Assert.True(t2.Value > 50);
        });

        botHalfFloat.Job((ref t2) =>
        {
            Interlocked.Increment(ref bot);
            Assert.True(t2.Value <= 50);
        });
        
        Assert.Equal(count, top + bot);
    }    
}
