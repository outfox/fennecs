using fennecs.expressions;

namespace fennecs.tests;

public static class ExpressionTests
{
    [Fact]
    public static void Any_Matches_All()
    {
        using var world = new World();

        var matchAny = Match.Any<string>();

        var compPlain = Component.Plain<string>();
        var compEntity = Component.Entity<string>(world.Spawn());
        var compObject = Component.Object<string>("dieter");

        Assert.True(matchAny.Matches(compPlain));
        Assert.True(matchAny.Matches(compEntity));
        Assert.True(matchAny.Matches(compObject));
    }

    
    [Fact]
    public static void Target_Matches_only_Targeted()
    {
        using var world = new World();

        var matchTarget = Match.AnyTarget<string>();

        var compPlain = Component.Plain<string>();
        var compEntity = Component.Entity<string>(world.Spawn());
        var compObject = Component.Object<string>("dieter");

        Assert.False(matchTarget.Matches(compPlain));
        Assert.True(matchTarget.Matches(compEntity));
        Assert.True(matchTarget.Matches(compObject));
    }
    
    
    [Fact]
    public static void Object_Matches_only_Object()
    {
        using var world = new World();

        var matchObject = Match.AnyObject<string>();

        var compPlain = Component.Plain<string>();
        var compEntity = Component.Entity<string>(world.Spawn());
        var compObject = Component.Object<string>("dieter");

        Assert.False(matchObject.Matches(compPlain));
        Assert.False(matchObject.Matches(compEntity));
        Assert.True(matchObject.Matches(compObject));
    }
    
    
    [Fact]
    public static void Entity_Matches_only_Entity()
    {
        using var world = new World();

        var matchObject = Match.AnyEntity<string>();

        var compPlain = Component.Plain<string>();
        var compEntity = Component.Entity<string>(world.Spawn());
        var compObject = Component.Object<string>("dieter");

        Assert.False(matchObject.Matches(compPlain));
        Assert.True(matchObject.Matches(compEntity));
        Assert.False(matchObject.Matches(compObject));
    }
    
    
    [Fact]
    public static void Plain_Matches_only_Plain()
    {
        using var world = new World();

        var matchPlain = Match.Plain<string>();

        var compPlain = Component.Plain<string>();
        var compEntity = Component.Entity<string>(world.Spawn());
        var compObject = Component.Object<string>("dieter");

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
        
        var matchObject = Match.Entity<string>(right);

        var compEntityRight = Component.Entity<string>(right);
        var compEntityWrong = Component.Entity<string>(wrong);
        
        Assert.True(matchObject.Matches(compEntityRight));
        Assert.False(matchObject.Matches(compEntityWrong));
    }

    [Fact]
    public static void Specific_Object_Matches_only_Specific_interned()
    {
        using var world = new World();

        // CAUTION - string interning might cause flukes / weird artifacts
        const string right = "interned";
        const string wrong = "different";
        
        var matchObject = Match.Object(right);

        var compObjectRight = Component.Object(right);
        var compObjectWrong = Component.Object(wrong);
        
        Assert.True(matchObject.Matches(compObjectRight));
        Assert.False(matchObject.Matches(compObjectWrong));
    }

    
    [Fact]
    public static void Specific_Object_Matches_only_Specific()
    {
        using var world = new World();

        List<string> right = ["erwin"];
        List<string> wrong = ["fumpel"];
        
        var matchObject = Match.Object(right);

        var compObjectRight = Component.Object(right);
        var compObjectWrong = Component.Object(wrong);
        
        Assert.True(matchObject.Matches(compObjectRight));
        Assert.False(matchObject.Matches(compObjectWrong));
    }
}
