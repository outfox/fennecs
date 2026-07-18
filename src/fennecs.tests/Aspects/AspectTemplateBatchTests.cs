namespace fennecs.tests.Aspects;

public class AspectTemplateBatchTests
{
    private record struct Position(float X, float Y);
    private record struct CrewData(int Count);
    private record struct Cargo(int Tons);


    private static (World world, Aspect visuals, Aspect game) CreateWorld()
    {
        var world = new World();
        var visuals = world.AddAspect("visuals").Owns<Position>();
        var game = world.AddAspect("game").Owns<CrewData>().Owns<Cargo>();
        return (world, visuals, game);
    }


    [Fact]
    public void Template_Splits_Components_Across_Aspects()
    {
        var (world, visuals, game) = CreateWorld();
        using var _1 = world;

        world.Template()
            .Add(new Position(1, 2))
            .Add(new CrewData(5))
            .Spawn(100)
            .Dispose();

        Assert.Equal(100, world.Count);
        Assert.Equal(100, visuals.Count);
        Assert.Equal(100, game.Count);

        foreach (var entity in world)
        {
            Assert.Equal(new(1, 2), entity.Ref<Position>());
            Assert.Equal(new(5), entity.Ref<CrewData>());
        }
    }


    [Fact]
    public void Template_With_Only_Foreign_Components_Still_Populates_Main()
    {
        var (world, _, game) = CreateWorld();
        using var _1 = world;

        world.Template()
            .Add(new CrewData(5))
            .Spawn(10)
            .Dispose();

        Assert.Equal(10, world.Count);
        Assert.Equal(10, world.Main.Count);
        Assert.Equal(10, game.Count);
        Assert.Equal(10, world.All.Count);
    }


    [Fact]
    public void Spawned_Entities_Can_Be_Despawned_Across_Aspects()
    {
        var (world, visuals, game) = CreateWorld();
        using var _1 = world;

        world.Template()
            .Add(new Position(1, 2))
            .Add(new CrewData(5))
            .Spawn(10)
            .Dispose();

        foreach (var entity in world.ToArray()) entity.Despawn();

        Assert.Equal(0, world.Count);
        Assert.Equal(0, visuals.Count);
        Assert.Equal(0, game.Count);
    }


    [Fact]
    public void Template_Splits_Three_Ways_Including_Main()
    {
        var (world, visuals, game) = CreateWorld();
        using var _1 = world;

        world.Template()
            .Add(42)                  // int is unregistered -> Main
            .Add(new Position(1, 2))  // -> visuals
            .Add(new CrewData(5))     // -> game
            .Spawn(100)
            .Dispose();

        Assert.Equal(100, world.Count);
        Assert.Equal(100, visuals.Count);
        Assert.Equal(100, game.Count);

        foreach (var entity in world)
        {
            Assert.Equal(42, entity.Ref<int>());
            Assert.Equal(new(1, 2), entity.Ref<Position>());
            Assert.Equal(new(5), entity.Ref<CrewData>());
        }
    }


    [Fact]
    public void Template_Relations_And_Links_Route_To_Owning_Aspects()
    {
        var (world, visuals, game) = CreateWorld();
        using var _1 = world;

        var station = world.Spawn();

        world.Template()
            .Add(new CrewData(3), station)  // relation: backing type owned by game
            .Add(Link.With("depot"))        // object link: string unregistered -> Main
            .Spawn(10)
            .Dispose();

        Assert.Equal(11, world.Count); // 10 spawned + station
        Assert.Equal(10, game.Count);  // relation storage followed its backing type
        Assert.Equal(0, visuals.Count);

        foreach (var entity in world)
        {
            if (entity == station) continue;
            Assert.True(entity.Has<CrewData>(station));
            Assert.Equal(new(3), entity.Ref<CrewData>(Match.Relation(station)));
            Assert.True(entity.Has(Link.With("depot")));
        }
    }


    [Fact]
    public void Typed_Template_Blits_Per_Entity_Values_Across_Aspects()
    {
        var (world, visuals, game) = CreateWorld();
        using var _1 = world;

        var station = world.Spawn();

        using var template = world.Template()
            .Add(new Cargo(50))          // baked, game-owned
            .Needs<Position>()           // required, visuals-owned
            .Needs<CrewData>(station);   // required relation, game-owned

        var wave = new Entity[25];
        template.Spawn(wave, i => (new Position(i, -i), new CrewData(i)));

        Assert.Equal(25, visuals.Count);
        Assert.Equal(25, game.Count);

        for (var i = 0; i < wave.Length; i++)
        {
            Assert.Equal(new Position(i, -i), wave[i].Ref<Position>());
            Assert.Equal(new CrewData(i), wave[i].Ref<CrewData>(Match.Relation(station)));
            Assert.Equal(new Cargo(50), wave[i].Ref<Cargo>());
        }
    }


    [Fact]
    public void Large_Wave_Grows_Aspect_Capacities()
    {
        var (world, visuals, game) = CreateWorld();
        using var _1 = world;

        world.Template()
            .Add(42)
            .Add(new Position(7, 7))
            .Add(new CrewData(9))
            .Spawn(10_000)
            .Dispose();

        Assert.Equal(10_000, world.Count);
        Assert.Equal(10_000, visuals.Count);
        Assert.Equal(10_000, game.Count);

        // spot-check across the wave (start, middle, end of storage)
        var entities = world.ToArray();
        foreach (var entity in new[] { entities[0], entities[4_999], entities[^1] })
        {
            Assert.Equal(42, entity.Ref<int>());
            Assert.Equal(new(7, 7), entity.Ref<Position>());
            Assert.Equal(new(9), entity.Ref<CrewData>());
        }
    }


    [Fact]
    public void Batch_With_Foreign_Aspect_Type_Throws()
    {
        var (world, _, game) = CreateWorld();
        using var _1 = world;

        world.Spawn().Add(new CrewData(5));

        var query = game.Query<CrewData>().Compile();
        Assert.Throws<InvalidOperationException>(() => query.Batch().Add(new Position(1, 2)));
        Assert.Throws<InvalidOperationException>(() => query.Batch().Remove<Position>());
    }


    [Fact]
    public void Batch_Add_Within_Aspect_Works()
    {
        var (world, _, game) = CreateWorld();
        using var _1 = world;

        world.Spawn().Add(new CrewData(5));
        world.Spawn().Add(new CrewData(6));

        world.Query<CrewData>().Not<Cargo>().Compile()
            .Batch()
            .Add(new Cargo(50))
            .Submit();

        var loaded = game.Query<Cargo>().Compile();
        Assert.Equal(2, loaded.Count);
    }


    [Fact]
    public void Batch_Removing_All_Owned_Components_Bulk_Evicts()
    {
        var (world, _, game) = CreateWorld();
        using var _1 = world;

        for (var i = 0; i < 10; i++) world.Spawn().Add(new CrewData(i));

        world.Query<CrewData>().Compile()
            .Batch()
            .Remove<CrewData>()
            .Submit();

        Assert.Equal(0, game.Count);
        Assert.Equal(10, world.Count); // all still alive in Main

        foreach (var entity in world)
        {
            Assert.False(entity.Has<CrewData>());
        }
    }


    [Fact]
    public void Deferred_Operations_Route_To_Aspects()
    {
        var (world, visuals, game) = CreateWorld();
        using var _1 = world;

        var entity = world.Spawn();
        var member = world.Spawn().Add(new CrewData(1));

        using (var _lock = world.Lock())
        {
            entity.Add(new Position(1, 2)); // deferred lazy join
            member.Remove<CrewData>();      // deferred eviction

            // Not applied yet.
            Assert.Equal(0, visuals.Count);
            Assert.Equal(1, game.Count);
        }

        // Applied on catch-up.
        Assert.Equal(1, visuals.Count);
        Assert.Equal(0, game.Count);
        Assert.Equal(new(1, 2), entity.Ref<Position>());
    }


    [Fact]
    public void Deferred_Despawn_Cleans_All_Aspects()
    {
        var (world, visuals, game) = CreateWorld();
        using var _1 = world;

        var entity = world.Spawn().Add(new Position(1, 2)).Add(new CrewData(5));

        using (var _lock = world.Lock())
        {
            entity.Despawn();
            Assert.True(world.IsAlive(entity));
        }

        Assert.False(world.IsAlive(entity));
        Assert.Equal(0, visuals.Count);
        Assert.Equal(0, game.Count);
    }
}
