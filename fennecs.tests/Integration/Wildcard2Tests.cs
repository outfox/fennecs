namespace fennecs.tests.Integration;

public class Wildcard2Tests
{
    private readonly World _world;
    private readonly Entity _entity;
    
    // string may be interned or not
    const string OBJECT1 = "hello world";
    const string OBJECT2 = "fly, you fools";
    const string NONE1 = "can't touch this";
    const string RELATION1 = "IOU";
    
    public Wildcard2Tests()
    {
        _world = new World();
        
        var bob = _world.Spawn().Id();
        _entity = _world.Spawn()
            .Add<float>()
            .AddLink(OBJECT1)
            .AddLink(OBJECT2)
            .Add(NONE1)
            .AddRelation(bob, RELATION1)
            .Id();
    }
    
    [Fact]
    public void Wildcard_Any_Enumerates_all_Components_Once()
    {
        using var query = _world.Query<string, float>(Entity.Any, Entity.None).Build();
        
        HashSet<string> seen = [];
        query.ForEach((ref string str, ref float _) =>
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
    public void Wildcard_None_Enumerates_Only_Plain_Components()
    {
        using var query = _world.Query<string, float>(Entity.None, Entity.None).Build();

        HashSet<string> seen = [];
        query.ForEach((ref string str, ref float _) =>
        {
            Assert.DoesNotContain(str, seen);
            seen.Add(str);
        });
        Assert.Contains(NONE1, seen);
        Assert.Single(seen);
    }


    [Fact]
    public void Wildcard_Target_Enumerates_all_Relations()
    {
        using var query = _world.Query<string, float>(Entity.Target, Entity.None).Build();

        HashSet<string> seen = [];

        query.ForEach((ref string str, ref float _) =>
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
    public void Wildcard_Relation_Enumerates_all_Relations()
    {
        using var query = _world.Query<string, float>(Entity.Relation, Entity.None).Build();
        
        HashSet<string> seen = [];
        
        query.ForEach((ref string str, ref float _) =>
        {
            Assert.DoesNotContain(str, seen);
            seen.Add(str);
        });
        Assert.Contains(RELATION1, seen);
        Assert.Single(seen);
    }



    [Fact]
    public void Wildcard_Object_Enumerates_all_Object_Links()
    {
        using var query = _world.Query<string, float>(Entity.Object, Entity.None).Build();

        HashSet<string> seen = [];

        query.ForEach((ref string str, ref float _) =>
        {
            Assert.DoesNotContain(str, seen);
            seen.Add(str);
        });
        
        Assert.Contains(OBJECT1, seen);
        Assert.Contains(OBJECT2, seen);
        Assert.Equal(2, seen.Count);
    }
}