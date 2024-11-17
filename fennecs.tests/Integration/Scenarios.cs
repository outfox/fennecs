using System.Buffers;

namespace fennecs.tests.Integration;

public class Scenarios
{
    [Theory]
    [InlineData(1_000, 2, 3, 5, 7)]
    [InlineData(1_000, 23, 17, 19, 7)]
    [InlineData(1_000, 19, 31, 37, 43)]
    [InlineData(10_000, 23, 17, 19, 7)]
    [InlineData(10_000, 19, 31, 37, 43)]
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
            var builder = world.Spawn().Add(count);
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
                builder.Add<ushort>(new Entity(world, entities[random.Next(entities.Count)]));
            }

            entities.Add(builder);
        }


        var floatsActual = world.Query<float>().Stream().Count;
        Assert.Equal(floats, floatsActual);

        var doublesActual = world.Query<double>().Stream().Count;
        Assert.Equal(doubles, doublesActual);

        var stringsActual = world.Query<string>().Stream().Count;
        Assert.Equal(strings, stringsActual);

        var stringsAndDoublesActual = world.Query<string, double>().Stream().Count;
        Assert.Equal(count / (stringRate * doubleRate), stringsAndDoublesActual);

        var floatsAndShortsActual = world.Query().Any<float>().Has<ushort>(Match.Any).Compile().Count;
        Assert.Equal(count / (floatRate * shortRate), floatsAndShortsActual);

        var shortsActual = world.Query().Has<ushort>(Match.Any).Compile().Count;
        Assert.Equal(shorts, shortsActual);
    }

    [Fact]
    public void Not_influenced_by_junk_in_shared_ArrayPool()
    {
        // Store some junk in the shared array pool here, so it will get picked up later for a _limiter.
        var rentedArrays = Enumerable.Range(0, 2)
            .Select(_ =>
            {
                var rent = ArrayPool<int>.Shared.Rent(2);
                rent[1] = 2;
                return rent;
            })
            .ToList();

        // Return out arrays, so fennecs can play with them later.
        foreach (var rentedArray in rentedArrays) ArrayPool<int>.Shared.Return(rentedArray);

        using var world = new World();
        world.Spawn().Add<int>();

        var count = 0;
        world.Stream<int>().For((_) => count++);
        Assert.Equal(count, world.Count);
    }
}