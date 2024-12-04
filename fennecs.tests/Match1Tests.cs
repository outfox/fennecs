using fennecs.storage;

namespace fennecs.tests;

public class Match1Tests
{
    // string may be interned or not
    private const string OBJECT1 = "hello world";
    private const string OBJECT2 = "fly, you fools";
    private const string NONE1 = "can't touch this";
    private const string RELATION1 = "IOU";
    private readonly World _world;


    public Match1Tests()
    {
        _world = new World();

        var bob = _world.Spawn();

        _world.Spawn()
            .Add(Link.With(OBJECT1))
            .Add(Link.With(OBJECT2))
            .Add(NONE1)
            .Add(RELATION1, bob);
    }


    [Fact]
    public void Match_Any_Enumerates_all_Components_Once()
    {
        var query = _world.Query<string>(Match.Any).Stream();

        HashSet<string> seen = [];
        query.For((str) =>
        {
            Assert.DoesNotContain(str, seen);
            seen.Add(str);
        });

        Assert.Contains(OBJECT1, seen);
        Assert.Contains(OBJECT2, seen);
        Assert.Contains(NONE1, seen);
        Assert.Contains(RELATION1, seen);
        Assert.Equal(4, seen.Count);
    }

    [Fact]
    public void Plain_Enumerates_Only_Plain_Components()
    {
        var query = _world.Query<string>(Match.Plain).Stream();

        HashSet<string> seen = [];
        query.For((str) =>
        {
            Assert.DoesNotContain(str, seen);
            seen.Add(str);
        });
        Assert.Contains(NONE1, seen);
        Assert.Single(seen);
    }


    [Fact]
    public void Target_Enumerates_all_Relations()
    {
        var query = _world.Query<string>(Match.Target).Stream();

        HashSet<string> seen = [];

        query.For((str) =>
        {
            Assert.DoesNotContain(str, seen);
            seen.Add(str);
        });
        Assert.Contains(OBJECT1, seen);
        Assert.Contains(OBJECT2, seen);
        Assert.Contains(RELATION1, seen);
        Assert.Equal(3, seen.Count);
    }


    [Fact]
    public void EntityAny_Relation_Enumerates_all_Relations()
    {
        var query = _world.Query<string>(Entity.Any).Stream();

        HashSet<string> seen = [];

        query.For((str) =>
        {
            Assert.DoesNotContain(str, seen);
            seen.Add(str);
        });
        Assert.Contains(RELATION1, seen);
        Assert.Single(seen);
    }


    [Fact]
    public void MatchEntity_Relation_Enumerates_all_Relations()
    {
        var query = _world.Query<string>(Match.Entity).Stream();

        HashSet<string> seen = [];

        query.For((str) =>
        {
            Assert.DoesNotContain(str, seen);
            seen.Add(str);
        });
        Assert.Contains(RELATION1, seen);
        Assert.Single(seen);
    }


    [Fact]
    public void Object_Enumerates_all_Object_Links()
    {
        var query = _world.Query<string>(Match.Link).Stream();

        HashSet<string> seen = [];

        query.For((str) =>
        {
            Assert.DoesNotContain(str, seen);
            seen.Add(str);
        });

        Assert.Contains(OBJECT1, seen);
        Assert.Contains(OBJECT2, seen);
        Assert.Equal(2, seen.Count);
    }
}