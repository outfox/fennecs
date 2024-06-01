namespace fennecs.tests;

public class ObjectLinkTests(ITestOutputHelper output)
{
    [Fact]
    public void Can_Link_Objects_via_Builder()
    {
        using var world = new World();
        using var query = world.Query<string>(Match.Any).Compile();

        // string may be interned or not
        const string TARGET = "hello world";
        world.Spawn().Add(Link.With(TARGET));

        var runs = 0;
        query.For((ref string str) =>
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
        using var query = world.Query<string>(Match.Any).Compile();

        // string may be interned or not
        const string TARGET = "hello world";

        var entity = world.Spawn();
        world.On(entity).Add(Link.With(TARGET));

        var runs = 0;
        query.For((ref string str) =>
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
        //TODO: This test intermittently fails! May be due to string interning or concurrent test runners.
        
        using var world = new World();
        using var query = world.Query<string>(Match.Any).Compile();

        // string may be interned or not
        const string TARGET = "hello world";

        var entity = world.Spawn().Add(Link.With(TARGET));
        entity.Remove<string>(TARGET);

        var runs = 0;
        query.For((ref string _) => { runs++; });
        Assert.Equal(0, runs);
    }


    [Fact]
    public void Can_Unlink_Objects_via_World()
    {
        using var world = new World();
        using var query = world.Query<string>(Match.Any).Compile();

        // string may be interned or not
        const string TARGET = "hello world";

        var entity = world.Spawn().Add(Link.With(TARGET));
        var typeExpression = TypeExpression.Of<string>(Link.With("hello world"));
        world.RemoveComponent(entity, typeExpression);

        var runs = 0;
        query.For((ref string _) => { runs++; });
        Assert.Equal(0, runs);
    }
}