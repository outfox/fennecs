namespace fennecs.tests.Integration;

public class ObjectLinkTests(ITestOutputHelper output)
{
    [Fact]
    public void Can_Link_Objects_via_Builder()
    {
        using var world = new World();
        using var query = world.Query<string>(Match.Any).Build();

        // string may be interned or not
        const string TARGET = "hello world";
        world.Spawn().AddLink(TARGET);

        var runs = 0;
        query.ForEach((ref string str) =>
        {
            runs++;
            output.WriteLine(str);
            Assert.Equal(TARGET, str);
            Assert.True(ReferenceEquals(TARGET, str));
        });
        Assert.Equal(1, runs);
    }

    [Fact]
    public void Can_Link_Objects_via_World()
    {
        using var world = new World();
        using var query = world.Query<string>(Match.Any).Build();

        // string may be interned or not
        const string TARGET = "hello world";
        
        var entity = world.Spawn().Id();
        world.On(entity).AddLink(TARGET);

        var runs = 0;
        query.ForEach((ref string str) =>
        {
            runs++;
            output.WriteLine(str);
            Assert.Equal(TARGET, str);
            Assert.True(ReferenceEquals(TARGET, str));
        });
        Assert.Equal(1, runs);
    }


    [Fact]
    public void Can_Unlink_Objects_via_Builder()
    {
        using var world = new World();
        using var query = world.Query<string>(Match.Any).Build();

        // string may be interned or not
        const string TARGET = "hello world";

        var entity = world.Spawn().AddLink(TARGET);
        entity.RemoveLink<string>(TARGET);

        var runs = 0;
        query.ForEach((ref string _) =>
        {
            runs++;
        });
        Assert.Equal(0, runs);
    }

    [Fact]
    public void Can_Unlink_Objects_via_World()
    {
        using var world = new World();
        using var query = world.Query<string>(Match.Any).Build();

        // string may be interned or not
        const string TARGET = "hello world";

        var entity = world.Spawn().AddLink(TARGET).Id();
        world.RemoveLink(entity, "hello world");

        var runs = 0;
        query.ForEach((ref string _) => { runs++; });
        Assert.Equal(0, runs);
    }
}    
