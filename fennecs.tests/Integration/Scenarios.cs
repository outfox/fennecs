namespace fennecs.tests.Integration;

public class Scenarios
{
    [Theory]
    [InlineData(1_000, 2, 3, 5, 7)]
    [InlineData(10_000, 2, 3, 5, 7)]
    [InlineData(100_000, 19, 31, 37, 43)]
    [InlineData(1_000_000, 23, 17, 19, 7)]
    public void Can_Iterate_many_Entities(int count, int floatRate, int doubleRate, int stringRate, int shortRate)
    {
        using var world = new World();
        
        var random = new Random(9001);
        var entities = new List<Entity>();


        var floats = 0;
        var doubles = 0;
        var strings = 0;
        var shorts = 0;
        
        for (var i = 1; i < count; i++)
        {
            var builder = world.Spawn().Add<int>(count);
            if (i % floatRate == 0)
            {
                floats++;
                builder.Add<float>(i);
            }

            if (i % doubleRate == 0)
            {
                doubles++;
                builder.Add<double>(i);
            }

            if (i % stringRate == 0)
            {
                strings++;
                builder.Add(i.ToString());
            }

            if (i % shortRate == 0)
            {
                shorts++;
                builder.Add<short>(entities[random.Next(0, entities.Count)]);
            }

            entities.Add(builder.Id());
        }

        
        var floatsActual = world.Query<float>().Build().Count;
        Assert.Equal(floats, floatsActual);
        
        var doublesActual = world.Query<double>().Build().Count;
        Assert.Equal(doubles, doublesActual);

        var stringsActual = world.Query<string>().Build().Count;
        Assert.Equal(strings, stringsActual);

        var stringsAndDoublesActual = world.Query<string, double>().Build().Count;
        Assert.Equal(count / (stringRate * doubleRate), stringsAndDoublesActual);

        var floatsAndShortsActual = world.Query().Any<float>().Has<short>(Identity.Any).Build().Count;
        Assert.Equal(count / (floatRate * shortRate), floatsAndShortsActual);

        var shortsActual = world.Query().Has<short>(Identity.Any).Build().Count;
        Assert.Equal(shorts, shortsActual);
    }
}