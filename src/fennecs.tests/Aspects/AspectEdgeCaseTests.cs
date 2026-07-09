namespace fennecs.tests.Aspects;

public class AspectEdgeCaseTests
{
    private record struct CrewData(int Count);
    private record struct Cargo(int Tons);


    private static (World world, Aspect game) CreateWorld(int initialCapacity = 4096)
    {
        var world = new World(initialCapacity);
        var game = world.AddAspect("game").Owns<CrewData>().Owns<Cargo>();
        return (world, game);
    }


    [Fact]
    public void Remove_From_Non_Member_Throws()
    {
        var (world, _) = CreateWorld();
        using var _1 = world;

        var entity = world.Spawn();
        Assert.Throws<InvalidOperationException>(() => entity.Remove<CrewData>());
    }


    [Fact]
    public void Remove_Missing_Component_From_Member_Throws()
    {
        var (world, _) = CreateWorld();
        using var _1 = world;

        // Member of the aspect via CrewData, but has no Cargo.
        var entity = world.Spawn().Add(new CrewData(5));
        Assert.Throws<InvalidOperationException>(() => entity.Remove<Cargo>());

        // Unaffected by the failed removal.
        Assert.True(entity.Has<CrewData>());
    }


    [Fact]
    public void Get_Returns_Empty_For_Non_Member()
    {
        var (world, _) = CreateWorld();
        using var _1 = world;

        var entity = world.Spawn();
        Assert.Empty(entity.Get<CrewData>(Match.Any));
    }


    [Fact]
    public void Get_Returns_Values_For_Member()
    {
        var (world, _) = CreateWorld();
        using var _1 = world;

        var entity = world.Spawn().Add(new CrewData(5));
        Assert.Equal([new(5)], entity.Get<CrewData>(Match.Any));
        Assert.Equal([new(5)], entity.Get<CrewData>(Match.Plain));
    }


    [Fact]
    public void Has_Is_False_Beyond_Aspect_Capacity()
    {
        // A tiny World makes later Entity indices exceed the aspect's (lazily grown) meta table.
        var (world, _) = CreateWorld(initialCapacity: 1);
        using var _1 = world;

        Entity last = default;
        for (var i = 0; i < 8; i++) last = world.Spawn();

        Assert.False(last.Has<CrewData>());
        Assert.Empty(last.Get<CrewData>(Match.Any));
    }


    [Fact]
    public void Components_Skips_Non_Member_Aspects()
    {
        var (world, _) = CreateWorld();
        using var _1 = world;

        // Entity has data only in Main; the game aspect contributes nothing.
        var entity = world.Spawn().Add("hello");

        var components = entity.Components;
        Assert.Contains(components, c => c.Type == typeof(string));
        Assert.DoesNotContain(components, c => c.Type == typeof(CrewData));
    }


    private record struct R01(int V); private record struct R02(int V); private record struct R03(int V);
    private record struct R04(int V); private record struct R05(int V); private record struct R06(int V);
    private record struct R07(int V); private record struct R08(int V); private record struct R09(int V);
    private record struct R10(int V); private record struct R11(int V); private record struct R12(int V);
    private record struct R13(int V); private record struct R14(int V); private record struct R15(int V);
    private record struct R16(int V); private record struct R17(int V); private record struct R18(int V);
    private record struct R19(int V); private record struct R20(int V);


    [Fact]
    public void Registry_Grows_Beyond_Initial_Size()
    {
        using var world = new World();

        // Registering more types than the routing table's initial size forces it to grow.
        // (TypeIDs are assigned from a global counter, so 20 fresh types guarantee IDs > 16)
        var aspect = world.AddAspect("wide").Owns(
            typeof(R01), typeof(R02), typeof(R03), typeof(R04), typeof(R05),
            typeof(R06), typeof(R07), typeof(R08), typeof(R09), typeof(R10),
            typeof(R11), typeof(R12), typeof(R13), typeof(R14), typeof(R15),
            typeof(R16), typeof(R17), typeof(R18), typeof(R19), typeof(R20));

        var entity = world.Spawn().Add(new R20(42));
        Assert.Equal(1, aspect.Count);
        Assert.Equal(42, entity.Ref<R20>().V);
    }


    [Fact]
    public void Strict_Mode_Permits_Identity_Expressions()
    {
        using var world = new World { StrictAspects = true };
        world.AddAspect("game").Owns<CrewData>();

        // Identity is exempt from ownership registration, even in strict mode.
        var entity = world.Spawn();
        Assert.True(entity.Has<Identity>());
    }
}
