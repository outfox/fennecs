namespace fennecs.tests.Aspects;

public class AspectRelationTests
{
    private record struct Position(float X, float Y);
    private record struct Follows(int Priority);
    private record struct CrewAssignment(int Role);


    private static (World world, Aspect visuals, Aspect game) CreateWorld()
    {
        var world = new World();
        var visuals = world.AddAspect("visuals").Owns<Position>().Owns<Follows>();
        var game = world.AddAspect("game").Owns<CrewAssignment>();
        return (world, visuals, game);
    }


    [Fact]
    public void Relations_Work_Within_Aspects()
    {
        var (world, visuals, _) = CreateWorld();
        using var _1 = world;

        var target = world.Spawn();
        var follower = world.Spawn().Add(new Follows(1), target);

        Assert.True(follower.Has<Follows>(target));
        Assert.Equal(1, visuals.Count); // only the follower has visuals data
    }


    [Fact]
    public void Relation_Can_Target_Entity_Outside_Its_Aspect()
    {
        var (world, _, game) = CreateWorld();
        using var _1 = world;

        // The target has no components in the game aspect at all.
        var ship = world.Spawn().Add(new Position(1, 2));
        var crewMember = world.Spawn().Add(new CrewAssignment(7), ship);

        Assert.True(crewMember.Has<CrewAssignment>(ship));
        Assert.Equal(1, game.Count);
    }


    [Fact]
    public void Despawning_Target_Cleans_Relations_In_All_Aspects()
    {
        var (world, visuals, game) = CreateWorld();
        using var _1 = world;

        var target = world.Spawn();
        var follower = world.Spawn().Add(new Follows(1), target);
        var crewMember = world.Spawn().Add(new CrewAssignment(7), target);

        target.Despawn();

        // Both relation components (living in different Aspects) are removed.
        Assert.False(follower.Has<Follows>(Match.Any));
        Assert.False(crewMember.Has<CrewAssignment>(Match.Any));

        Assert.True(follower.Alive);
        Assert.True(crewMember.Alive);

        // The relation holders were evicted from their aspects along with their last components.
        Assert.Equal(0, visuals.Count);
        Assert.Equal(0, game.Count);
    }


    [Fact]
    public void Despawning_Target_Preserves_Other_Components()
    {
        var (world, visuals, _) = CreateWorld();
        using var _1 = world;

        var target = world.Spawn();
        var follower = world.Spawn().Add(new Position(1, 2)).Add(new Follows(1), target);

        target.Despawn();

        Assert.False(follower.Has<Follows>(Match.Any));
        Assert.True(follower.Has<Position>());
        Assert.Equal(new(1, 2), follower.Ref<Position>());
        Assert.Equal(1, visuals.Count);
    }


    [Fact]
    public void Relation_Streams_Match_Per_Aspect()
    {
        var (world, _, game) = CreateWorld();
        using var _1 = world;

        var shipA = world.Spawn();
        var shipB = world.Spawn();

        world.Spawn().Add(new CrewAssignment(1), shipA);
        world.Spawn().Add(new CrewAssignment(2), shipA);
        world.Spawn().Add(new CrewAssignment(3), shipB);

        var crewOfA = game.Query<CrewAssignment>(shipA).Compile();
        Assert.Equal(2, crewOfA.Count);

        var allCrew = world.Query<CrewAssignment>(Match.Entity).Compile();
        Assert.Equal(3, allCrew.Count);
    }
}
