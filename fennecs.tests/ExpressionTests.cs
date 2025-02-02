namespace fennecs.tests;

/*
public static class ExpressionTests
{
    [Fact]
    public static void Any_Matches_All()
    {
        using var world = new World();

        var matchAny = Comp<string>.Matching(Match.Any);

        var entity = world.Spawn();
        
        var compPlain = Comp<string>.Plain;
        var compEntity = Comp<string>.Matching(entity);
        var compObject = Comp<string>.Matching(Key.Of("Erwin"));

        Assert.True(matchAny.Matches(compPlain));
        Assert.True(matchAny.Matches(compEntity));
        Assert.True(matchAny.Matches(compObject));
    }

    
    [Fact]
    public static void Target_Matches_only_Targeted()
    {
        using var world = new World();

        var matchTarget = Comp<string>.Matching(Match.Target);

        var compPlain = Comp<string>.Plain;
        var compEntity = Comp<string>.Matching(world.Spawn());
        var compObject = Comp<string>.Matching("Erwin");

        Assert.False(matchTarget.Matches(compPlain));
        Assert.True(matchTarget.Matches(compEntity));
        Assert.True(matchTarget.Matches(compObject));
    }
    
    
    [Fact]
    public static void Object_Matches_only_Object()
    {
        using var world = new World();

        var matchObject = Comp<string>.Matching(Link.Any);
        
        var compPlain = Comp<string>.Plain;
        var compEntity = Comp<string>.Matching(world.Spawn());
        var compObject = Comp<string>.Matching("Erwin");

        Assert.False(matchObject.Matches(compPlain));
        Assert.False(matchObject.Matches(compEntity));
        Assert.True(matchObject.Matches(compObject));
    }
    
    
    [Fact]
    public static void Entity_Matches_only_Entity()
    {
        using var world = new World();

        var matchObject = Comp<string>.Matching(Entity.Any);

        var compPlain = Comp<string>.Plain;
        var compEntity = Comp<string>.Matching(world.Spawn());
        var compObject = Comp<string>.Matching("Erwin");

        Assert.False(matchObject.Matches(compPlain));
        Assert.True(matchObject.Matches(compEntity));
        Assert.False(matchObject.Matches(compObject));
    }
    
    
    [Fact]
    public static void Plain_Matches_only_Plain()
    {
        using var world = new World();

        var matchPlain = Comp<string>.Plain;

        var compPlain = Comp<string>.Plain;
        var compEntity = Comp<string>.Matching(world.Spawn());
        var compObject = Comp<string>.Matching("Erwin");

        Assert.True(matchPlain.Matches(compPlain));
        Assert.False(matchPlain.Matches(compEntity));
        Assert.False(matchPlain.Matches(compObject));
    }
    
    
    [Fact]
    public static void Specific_Entity_Matches_only_Specific()
    {
        using var world = new World();

        var right = world.Spawn();
        var wrong = world.Spawn();
        
        var matchObject = Comp<string>.Matching(right);

        var compEntityRight = Comp<string>.Matching(right);
        var compEntityWrong = Comp<string>.Matching(wrong);
        
        Assert.True(matchObject.Matches(compEntityRight));
        Assert.False(matchObject.Matches(compEntityWrong));
    }

    [Fact]
    public static void Specific_Object_Matches_only_Specific_interned()
    {
        using var world = new World();

        // CAUTION - string interning might cause flukes / weird artifacts
        const string right = "Erwin";
        const string wrong = "different";
        
        var matchObject = Comp<string>.Matching(right);

        var compObjectRight = Comp<string>.Matching(right);
        var compObjectWrong = Comp<string>.Matching(wrong);
        
        Assert.True(matchObject.Matches(compObjectRight));
        Assert.False(matchObject.Matches(compObjectWrong));
    }

    
    [Fact]
    public static void Specific_Object_Matches_only_Specific()
    {
        using var world = new World();

        List<string> right = ["Erwin"];
        List<string> wrong = ["different"];
        
        var matchObject = Comp<string>.Matching(right);

        var compObjectRight = Comp<string>.Matching(right);
        var compObjectWrong = Comp<string>.Matching(wrong);
        
        Assert.True(matchObject.Matches(compObjectRight));
        Assert.False(matchObject.Matches(compObjectWrong));
    }
}
*/