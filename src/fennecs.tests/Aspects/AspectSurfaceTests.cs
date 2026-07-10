namespace fennecs.tests.Aspects;

/// <summary>
/// Exercises the full IAspect query/stream surface (all arities and overloads)
/// plus the small utility members of Aspect and World.
/// </summary>
public class AspectSurfaceTests(ITestOutputHelper output)
{
    private record struct S1(int V);
    private record struct S2(int V);
    private record struct S3(int V);
    private record struct S4(int V);
    private record struct S5(int V);


    private static (World world, Aspect aspect) CreateWorld()
    {
        var world = new World();
        var aspect = world.AddAspect("surface")
            .Owns<S1>().Owns<S2>().Owns<S3>().Owns<S4>().Owns<S5>();

        world.Spawn().Add(new S1(1)).Add(new S2(2)).Add(new S3(3)).Add(new S4(4)).Add(new S5(5));
        return (world, aspect);
    }


    [Fact]
    public void Aspect_Query_All_Arities_Compile_And_Match()
    {
        var (world, aspect) = CreateWorld();
        using var _1 = world;

        Assert.Equal(1, aspect.Query().Has<S1>().Compile().Count);

        Assert.Equal(1, aspect.Query<S1>().Compile().Count);
        Assert.Equal(1, aspect.Query<S1>(Match.Plain).Compile().Count);

        Assert.Equal(1, aspect.Query<S1, S2>().Compile().Count);
        Assert.Equal(1, aspect.Query<S1, S2>(Match.Plain, Match.Plain).Compile().Count);

        Assert.Equal(1, aspect.Query<S1, S2, S3>().Compile().Count);
        Assert.Equal(1, aspect.Query<S1, S2, S3>(Match.Plain, Match.Plain, Match.Plain).Compile().Count);

        Assert.Equal(1, aspect.Query<S1, S2, S3, S4>().Compile().Count);
        Assert.Equal(1, aspect.Query<S1, S2, S3, S4>(Match.Plain, Match.Plain, Match.Plain, Match.Plain).Compile().Count);

        Assert.Equal(1, aspect.Query<S1, S2, S3, S4, S5>().Compile().Count);
        Assert.Equal(1, aspect.Query<S1, S2, S3, S4, S5>(Match.Plain, Match.Plain, Match.Plain, Match.Plain, Match.Plain).Compile().Count);
    }


    [Fact]
    public void Aspect_Stream_All_Arities_Iterate()
    {
        var (world, aspect) = CreateWorld();
        using var _1 = world;

        var sum = 0;
        aspect.Stream<S1>().For((ref a) => sum += a.V);
        Assert.Equal(1, sum);

        sum = 0;
        aspect.Stream<S1, S2>().For((ref a, ref b) => sum += a.V + b.V);
        Assert.Equal(3, sum);

        sum = 0;
        aspect.Stream<S1, S2>(Match.Plain, Match.Plain).For((ref a, ref b) => sum += a.V + b.V);
        Assert.Equal(3, sum);

        sum = 0;
        aspect.Stream<S1, S2, S3>().For((ref a, ref b, ref c) => sum += a.V + b.V + c.V);
        Assert.Equal(6, sum);

        sum = 0;
        aspect.Stream<S1, S2, S3>(Match.Plain, Match.Plain, Match.Plain).For((ref a, ref b, ref c) => sum += a.V + b.V + c.V);
        Assert.Equal(6, sum);

        sum = 0;
        aspect.Stream<S1, S2, S3, S4>().For((ref a, ref b, ref c, ref d) => sum += a.V + b.V + c.V + d.V);
        Assert.Equal(10, sum);

        sum = 0;
        aspect.Stream<S1, S2, S3, S4>(Match.Plain, Match.Plain, Match.Plain, Match.Plain).For((ref a, ref b, ref c, ref d) => sum += a.V + b.V + c.V + d.V);
        Assert.Equal(10, sum);

        sum = 0;
        aspect.Stream<S1, S2, S3, S4, S5>().For((ref a, ref b, ref c, ref d, ref e) => sum += a.V + b.V + c.V + d.V + e.V);
        Assert.Equal(15, sum);

        sum = 0;
        aspect.Stream<S1, S2, S3, S4, S5>(Match.Plain, Match.Plain, Match.Plain, Match.Plain, Match.Plain).For((ref a, ref b, ref c, ref d, ref e) => sum += a.V + b.V + c.V + d.V + e.V);
        Assert.Equal(15, sum);
    }


    [Fact]
    public void Aspect_Has_ToString()
    {
        var (world, aspect) = CreateWorld();
        using var _1 = world;

        var str = aspect.ToString();
        output.WriteLine(str);
        Assert.Contains("surface", str);
    }


    [Fact]
    public void Aspect_Enumerates_Non_Generic()
    {
        var (world, aspect) = CreateWorld();
        using var _1 = world;

        var count = 0;
        var enumerator = ((System.Collections.IEnumerable)aspect).GetEnumerator();
        while (enumerator.MoveNext()) count++;
        Assert.Equal(1, count);
    }


    [Fact]
    public void World_Explicit_IAspect_World_Is_Itself()
    {
        using var world = new World();
        IAspect surface = world;
        Assert.Same(world, surface.World);
    }


    [Fact]
    public void World_Queries_Aggregates_Across_Aspects()
    {
        var (world, aspect) = CreateWorld();
        using var _1 = world;

        var mainQuery = world.Query<string>().Compile();     // unregistered -> Main
        var aspectQuery = aspect.Query<S1>().Compile();

        var queries = world.Queries;
        Assert.Contains(mainQuery, queries);
        Assert.Contains(aspectQuery, queries);
    }
}
