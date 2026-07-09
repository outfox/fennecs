namespace fennecs.tests.Aspects;

public class AspectMembershipTests
{
    private record struct Position(float X, float Y);
    private record struct Matrix(int M);
    private record struct CrewData(int Count);


    private static (World world, Aspect visuals, Aspect game) CreateWorld()
    {
        var world = new World();
        var visuals = world.AddAspect("visuals").Owns<Position>().Owns<Matrix>();
        var game = world.AddAspect("game").Owns<CrewData>();
        return (world, visuals, game);
    }


    [Fact]
    public void Membership_Is_Lazy()
    {
        var (world, visuals, game) = CreateWorld();
        using var _ = world;

        var entity = world.Spawn();

        // Freshly spawned Entities are only members of Main.
        Assert.Equal(1, world.Count);
        Assert.Equal(0, visuals.Count);
        Assert.Equal(0, game.Count);

        entity.Add(new Position(1, 2));
        Assert.Equal(1, visuals.Count);
        Assert.Equal(0, game.Count);

        entity.Add(new CrewData(5));
        Assert.Equal(1, game.Count);
    }


    [Fact]
    public void Components_Live_In_Their_Aspects()
    {
        var (world, _, _) = CreateWorld();
        using var _1 = world;

        var entity = world.Spawn()
            .Add(new Position(1, 2))
            .Add(new CrewData(5));

        Assert.True(entity.Has<Position>());
        Assert.True(entity.Has<CrewData>());

        Assert.Equal(new(1, 2), entity.Ref<Position>());
        Assert.Equal(new(5), entity.Ref<CrewData>());

        entity.Ref<Position>() = new(3, 4);
        Assert.Equal(new(3, 4), entity.Ref<Position>());
    }


    [Fact]
    public void Removing_Last_Owned_Component_Evicts()
    {
        var (world, visuals, _) = CreateWorld();
        using var _1 = world;

        var entity = world.Spawn().Add(new Position(1, 2)).Add(new Matrix(7));
        Assert.Equal(1, visuals.Count);

        entity.Remove<Position>();
        Assert.Equal(1, visuals.Count); // still has Matrix
        Assert.True(entity.Has<Matrix>());

        entity.Remove<Matrix>();
        Assert.Equal(0, visuals.Count); // evicted
        Assert.False(entity.Has<Matrix>());

        // Entity remains alive and a member of Main.
        Assert.True(world.IsAlive(entity));
        Assert.Contains(entity, world);
    }


    [Fact]
    public void Entity_Can_Rejoin_After_Eviction()
    {
        var (world, visuals, _) = CreateWorld();
        using var _1 = world;

        var entity = world.Spawn().Add(new Position(1, 2));
        entity.Remove<Position>();
        Assert.Equal(0, visuals.Count);

        entity.Add(new Position(5, 6));
        Assert.Equal(1, visuals.Count);
        Assert.Equal(new(5, 6), entity.Ref<Position>());
    }


    [Fact]
    public void Despawn_Removes_From_All_Aspects()
    {
        var (world, visuals, game) = CreateWorld();
        using var _1 = world;

        var entity = world.Spawn()
            .Add(new Position(1, 2))
            .Add(new CrewData(5));

        var bystander = world.Spawn()
            .Add(new Position(9, 9))
            .Add(new CrewData(1));

        entity.Despawn();

        Assert.False(world.IsAlive(entity));
        Assert.Equal(1, world.Count);
        Assert.Equal(1, visuals.Count);
        Assert.Equal(1, game.Count);
        Assert.Equal(new(9, 9), bystander.Ref<Position>());
        Assert.Equal(new(1), bystander.Ref<CrewData>());
    }


    [Fact]
    public void Main_Contains_All_Living_Entities()
    {
        var (world, _, game) = CreateWorld();
        using var _1 = world;

        // Entity with components ONLY in the game aspect is still in Main (and thus the World).
        var entity = world.Spawn().Add(new CrewData(1));

        Assert.Equal(1, world.Count);
        Assert.Equal(1, world.Main.Count);
        Assert.Contains(entity, world);
        Assert.Equal(1, game.Count);
    }


    [Fact]
    public void Aspect_Enumerates_Its_Members()
    {
        var (world, visuals, _) = CreateWorld();
        using var _1 = world;

        var member = world.Spawn().Add(new Position(1, 2));
        world.Spawn().Add(new CrewData(1)); // not a visuals member

        Assert.Equal([member], visuals.ToArray());
    }


    [Fact]
    public void Components_Property_Spans_Aspects()
    {
        var (world, _, _) = CreateWorld();
        using var _1 = world;

        var entity = world.Spawn()
            .Add(new Position(1, 2))
            .Add(new CrewData(5));

        var components = entity.Components;
        Assert.Contains(components, c => c.Type == typeof(Position));
        Assert.Contains(components, c => c.Type == typeof(CrewData));
    }


    [Fact]
    public void Dump_Spans_Aspects()
    {
        var (world, _, _) = CreateWorld();
        using var _1 = world;

        var entity = world.Spawn()
            .Add(new Position(1, 2))
            .Add(new CrewData(5));

        var dump = entity.Dump();
        Assert.Contains(nameof(Position), dump);
        Assert.Contains(nameof(CrewData), dump);
    }


    [Fact]
    public void Truncate_Despawns_Across_Aspects()
    {
        var (world, visuals, game) = CreateWorld();
        using var _1 = world;

        for (var i = 0; i < 10; i++)
        {
            world.Spawn().Add(new Position(i, i)).Add(new CrewData(i));
        }

        var query = game.Query<CrewData>().Compile();
        query.Truncate(4);

        Assert.Equal(4, game.Count);
        Assert.Equal(4, visuals.Count);
        Assert.Equal(4, world.Count);
    }


    [Fact]
    public void GC_Preserves_Aspect_Roots()
    {
        var (world, visuals, _) = CreateWorld();
        using var _1 = world;

        var entity = world.Spawn().Add(new Position(1, 2));
        entity.Remove<Position>();

        world.GC();

        // The aspect still works after GC removed its empty archetypes.
        entity.Add(new Position(3, 4));
        Assert.Equal(1, visuals.Count);
    }
}
