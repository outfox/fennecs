namespace fennecs.tests;

public class Match1Tests
{
    private readonly World _world;
    
    // string may be interned or not
    private const string OBJECT1 = "hello world";
    private const string OBJECT2 = "fly, you fools";
    private const string NONE1 = "can't touch this";
    private const string RELATION1 = "IOU";
    
    public Match1Tests()
    {
        _world = new World();
        
        var bob = _world.Spawn();

        _world.Spawn()
            .AddLink(OBJECT1)
            .AddLink(OBJECT2)
            .Add(NONE1)
            .AddRelation(bob, RELATION1);
    }
    
    [Fact]
    public void Any_Enumerates_all_Components_Once()
    {
        using var query = _world.Query<string>(Match.Any).Build();
        
        HashSet<string> seen = [];
        query.ForEach((ref string str) =>
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
        using var query = _world.Query<string>(Match.Plain).Build();

        HashSet<string> seen = [];
        query.ForEach((ref string str) =>
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
        using var query = _world.Query<string>(Match.Relation).Build();

        HashSet<string> seen = [];

        query.ForEach((ref string str) =>
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
    public void Relation_Enumerates_all_Relations()
    {
        using var query = _world.Query<string>(Match.Identity).Build();
        
        HashSet<string> seen = [];

        query.ForEach((ref string str) =>
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
        using var query = _world.Query<string>(Match.Object).Build();

        HashSet<string> seen = [];

        query.ForEach((ref string str) =>
        {
            Assert.DoesNotContain(str, seen);
            seen.Add(str);
        });
        
        Assert.Contains(OBJECT1, seen);
        Assert.Contains(OBJECT2, seen);
        Assert.Equal(2, seen.Count);
    }
}