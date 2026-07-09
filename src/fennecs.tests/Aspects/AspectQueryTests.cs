namespace fennecs.tests.Aspects;

public class AspectQueryTests
{
    private record struct Position(float X, float Y);
    private record struct Matrix(int M);
    private record struct CrewData(int Count);
    private record struct Cargo(int Tons);


    private static (World world, Aspect visuals, Aspect game) CreateWorld()
    {
        var world = new World();
        var visuals = world.AddAspect("visuals").Owns<Position>().Owns<Matrix>();
        var game = world.AddAspect("game").Owns<CrewData>().Owns<Cargo>();
        return (world, visuals, game);
    }


    [Fact]
    public void World_Query_Resolves_Owning_Aspect()
    {
        var (world, _, _) = CreateWorld();
        using var _1 = world;

        world.Spawn().Add(new Position(1, 2));
        world.Spawn().Add(new Position(3, 4));
        world.Spawn().Add(new CrewData(5));

        var positions = world.Query<Position>().Compile();
        Assert.Equal(2, positions.Count);

        var crews = world.Query<CrewData>().Compile();
        Assert.Equal(1, crews.Count);
    }


    [Fact]
    public void World_And_Aspect_Queries_Share_Cache()
    {
        var (world, visuals, _) = CreateWorld();
        using var _1 = world;

        var viaWorld = world.Query<Position>().Compile();
        var viaAspect = visuals.Query<Position>().Compile();

        Assert.Same(viaWorld, viaAspect);
    }


    [Fact]
    public void Mixed_Aspect_Query_Throws_With_Listing()
    {
        var (world, _, _) = CreateWorld();
        using var _1 = world;

        var exception = Assert.Throws<InvalidOperationException>(() => world.Query<Position, CrewData>().Compile());

        Assert.Contains(nameof(Position), exception.Message);
        Assert.Contains(nameof(CrewData), exception.Message);
        Assert.Contains("visuals", exception.Message);
        Assert.Contains("game", exception.Message);
    }


    [Fact]
    public void Mixed_Aspect_Filter_Throws()
    {
        var (world, _, _) = CreateWorld();
        using var _1 = world;

        // Stream types and filters must all resolve to the same Aspect.
        Assert.Throws<InvalidOperationException>(() => world.Query<Position>().Has<CrewData>().Compile());
        Assert.Throws<InvalidOperationException>(() => world.Query<Position>().Not<CrewData>().Compile());
        Assert.Throws<InvalidOperationException>(() => world.Query<Position>().Any<CrewData>().Compile());
    }


    [Fact]
    public void Aspect_Query_With_Foreign_Type_Throws()
    {
        var (world, visuals, _) = CreateWorld();
        using var _1 = world;

        var exception = Assert.Throws<InvalidOperationException>(() => visuals.Query<CrewData>().Compile());
        Assert.Contains("game", exception.Message);
    }


    [Fact]
    public void Same_Aspect_Query_With_Filters_Works()
    {
        var (world, _, game) = CreateWorld();
        using var _1 = world;

        world.Spawn().Add(new CrewData(5)).Add(new Cargo(10));
        world.Spawn().Add(new CrewData(3));

        var loaded = world.Query<CrewData>().Has<Cargo>().Compile();
        Assert.Equal(1, loaded.Count);

        var empty = game.Query<CrewData>().Not<Cargo>().Compile();
        Assert.Equal(1, empty.Count);
    }


    [Fact]
    public void World_All_Is_Main_All()
    {
        var (world, _, game) = CreateWorld();
        using var _1 = world;

        world.Spawn().Add(new CrewData(5));
        world.Spawn();

        Assert.Equal(2, world.All.Count);
        Assert.Same(world.All, world.Main.All);
        Assert.Equal(1, game.All.Count);
    }


    [Fact]
    public void Streams_Iterate_Aspect_Data()
    {
        var (world, visuals, game) = CreateWorld();
        using var _1 = world;

        world.Spawn().Add(new Position(1, 0)).Add(new CrewData(10));
        world.Spawn().Add(new Position(2, 0)).Add(new CrewData(20));

        var xSum = 0f;
        visuals.Stream<Position>().For((ref position) => xSum += position.X);
        Assert.Equal(3f, xSum);

        var crewSum = 0;
        world.Stream<CrewData>().For((ref crew) => crewSum += crew.Count);
        Assert.Equal(30, crewSum);

        _ = game; // silence unused
    }


    [Fact]
    public void Query_Contains_Is_Aspect_Scoped()
    {
        var (world, visuals, game) = CreateWorld();
        using var _1 = world;

        var visual = world.Spawn().Add(new Position(1, 2));
        var crewed = world.Spawn().Add(new CrewData(5));

        var positions = visuals.Query<Position>().Compile();
        Assert.True(positions.Contains(visual));

        // Alive, but not a member of this query's Aspect.
        Assert.False(positions.Contains(crewed));

        _ = game;
    }


    [Fact]
    public void New_Archetypes_Notify_Aspect_Queries()
    {
        var (world, _, game) = CreateWorld();
        using var _1 = world;

        var crews = game.Query<CrewData>().Compile();
        Assert.Equal(0, crews.Count);

        // Creates a brand-new archetype in the game aspect after the query was compiled.
        world.Spawn().Add(new CrewData(5)).Add(new Cargo(10));
        Assert.Equal(1, crews.Count);
    }


    [Fact]
    public void DespawnAllWith_Routes_To_Aspect()
    {
        var (world, visuals, game) = CreateWorld();
        using var _1 = world;

        world.Spawn().Add(new Position(1, 1)).Add(new CrewData(5));
        world.Spawn().Add(new Position(2, 2));

        world.DespawnAllWith<CrewData>();

        Assert.Equal(1, world.Count);
        Assert.Equal(1, visuals.Count);
        Assert.Equal(0, game.Count);
    }
}
