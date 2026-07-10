namespace fennecs.tests.Aspects;

public class AspectRegistrationTests
{
    private record struct Position(float X, float Y);
    private record struct Velocity(float X, float Y);
    private record struct CrewData(int Count);


    [Fact]
    public void World_Has_Main_Aspect()
    {
        using var world = new World();
        Assert.NotNull(world.Main);
        Assert.True(world.Main.IsMain);
        Assert.Equal("main", world.Main.Name);
        Assert.Same(world.Main, world.Aspects[0]);
        Assert.Single(world.Aspects);
    }


    [Fact]
    public void Can_Add_Aspect()
    {
        using var world = new World();
        var visuals = world.AddAspect("visuals");

        Assert.Equal("visuals", visuals.Name);
        Assert.False(visuals.IsMain);
        Assert.Same(world, visuals.World);
        Assert.Equal(2, world.Aspects.Count);
        Assert.Contains(visuals, world.Aspects);
    }


    [Fact]
    public void Duplicate_Aspect_Name_Throws()
    {
        using var world = new World();
        world.AddAspect("visuals");
        Assert.Throws<ArgumentException>(() => world.AddAspect("visuals"));
        Assert.Throws<ArgumentException>(() => world.AddAspect("main"));
    }


    [Fact]
    public void Owns_Is_Fluent_And_Idempotent()
    {
        using var world = new World();
        var visuals = world.AddAspect("visuals").Owns<Position>().Owns<Velocity>();

        // Re-registering to the same Aspect is a no-op.
        Assert.Same(visuals, visuals.Owns<Position>());
    }


    [Fact]
    public void Owns_Type_Params_Overload()
    {
        using var world = new World();
        var visuals = world.AddAspect("visuals", typeof(Position), typeof(Velocity));

        var entity = world.Spawn().Add(new Position(1, 2));
        Assert.Equal(1, visuals.Count);
    }


    [Fact]
    public void Owns_By_Other_Aspect_Throws()
    {
        using var world = new World();
        world.AddAspect("visuals").Owns<Position>();
        var game = world.AddAspect("game");

        var exception = Assert.Throws<InvalidOperationException>(() => game.Owns<Position>());
        Assert.Contains("visuals", exception.Message);
    }


    [Fact]
    public void Ownership_Frozen_After_Materialization()
    {
        using var world = new World();
        var visuals = world.AddAspect("visuals");

        // Using the type routes it to Main and freezes its ownership.
        world.Spawn().Add(new Position(1, 2));

        var exception = Assert.Throws<InvalidOperationException>(() => visuals.Owns<Position>());
        Assert.Contains("freezes", exception.Message);
    }


    [Fact]
    public void Owns_Identity_Throws()
    {
        using var world = new World();
        var visuals = world.AddAspect("visuals");
        Assert.Throws<InvalidOperationException>(() => visuals.Owns<Identity>());
        Assert.Throws<InvalidOperationException>(() => visuals.Owns(typeof(Identity)));
    }


    [Fact]
    public void Strict_Mode_Requires_Registration()
    {
        using var world = new World { StrictAspects = true };
        world.AddAspect("game").Owns<CrewData>();

        // Spawning works (Identity is exempt from registration).
        var entity = world.Spawn();

        // Registered type works.
        entity.Add(new CrewData(5));

        // Unregistered type throws.
        Assert.Throws<InvalidOperationException>(() => entity.Add(new Position(1, 2)));
    }


    [Fact]
    public void Strict_Mode_Off_Routes_To_Main()
    {
        using var world = new World();
        world.AddAspect("game").Owns<CrewData>();

        var entity = world.Spawn().Add(new Position(1, 2));
        Assert.True(entity.Has<Position>());
    }


    private class GameWorld : World
    {
        public readonly Aspect Visuals;
        public readonly Aspect Game;

        public GameWorld()
        {
            Visuals = AddAspect("visuals").Owns<Position>().Owns<Velocity>();
            Game = AddAspect("game").Owns<CrewData>();
        }
    }


    [Fact]
    public void World_Subclass_Ctor_Registration_Pattern()
    {
        using var world = new GameWorld();

        var entity = world.Spawn()
            .Add(new Position(1, 2))
            .Add(new CrewData(3));

        Assert.Equal(1, world.Visuals.Count);
        Assert.Equal(1, world.Game.Count);
        Assert.True(entity.Has<Position>());
        Assert.True(entity.Has<CrewData>());
        Assert.Equal(3, world.Aspects.Count);
    }


    [Fact]
    public void GC_On_Empty_World_Keeps_Root_Alive()
    {
        // Regression: World.GC() used to dispose the Root archetype of an empty World,
        // leaving NewEntity() with a stale reference.
        using var world = new World();
        world.GC();

        var entity = world.Spawn();
        Assert.True(world.IsAlive(entity));
        Assert.Contains(entity, world);
    }
}
