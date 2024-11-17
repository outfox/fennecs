namespace fennecs.tests;

public class ObjectLinkTests(ITestOutputHelper output)
{
    [Fact]
    public void Can_Link_Objects_via_Builder()
    {
        using var world = new World();
        var query = world.Query<string>(Match.Any).Stream();

        // string may be interned or not
        const string target = "hello world";
        world.Spawn().Add(Link.With(target));

        var runs = 0;
        query.For((str) =>
        {
            runs++;
            output.WriteLine(str);
            Assert.Equal(target, str);
            Assert.True(ReferenceEquals(target, str.read));
        });
        Assert.Equal(1, runs);
    }


    [Fact]
    public void Can_Link_Objects_via_World()
    {
        using var world = new World();
        var query = world.Query<string>(Match.Any).Stream();

        // string may be interned or not
        const string link = "hello world";

        var entity = world.Spawn();
        entity.Add(Link.With(link));

        var runs = 0;
        query.For((str) =>
        {
            runs++;
            output.WriteLine(str);
            Assert.Equal(link, str);
            Assert.True(ReferenceEquals(link, str.read));
        });
        Assert.Equal(1, runs);
    }


    [Fact]
    public void Can_Unlink_Objects_via_Builder()
    {
        //TODO: This test intermittently fails! May be due to string interning or concurrent test runners.
        
        using var world = new World();
        var query = world.Query<string>(Match.Any).Stream();

        // string may be interned or not
        const string target = "hello world";

        var entity = world.Spawn().Add(Link.With(target));
        entity.Remove<string>(target);

        var runs = 0;
        query.For((_) => { runs++; });
        Assert.Equal(0, runs);
    }


    [Fact]
    public void Can_Unlink_Objects_via_World()
    {
        using var world = new World();
        var query = world.Query<string>(Match.Any).Stream();

        // string may be interned or not
        const string target = "hello world";

        var entity = world.Spawn().Add(Link.With(target));
        var typeExpression = TypeExpression.Of<string>(Link.With("hello world"));
        world.RemoveComponent(entity, typeExpression);

        var runs = 0;
        query.For((_) => { runs++; });
        Assert.Equal(0, runs);
    }
}