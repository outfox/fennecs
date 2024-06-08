namespace fennecs.tests;

public static class ExpressionTests
{
    [Fact]
    public static void Any_Matches_All()
    {
        using var world = new World();

        var matchAny = Match.AnyAny<string>();

        var compPlain = Match.PlainComponent<string>();
        var compEntity = Match.SpecificEntity<string>(world.Spawn());
        var compObject = Match.SpecificLink<string>("erwin");

        Assert.True(matchAny.Matches(compPlain));
        Assert.True(matchAny.Matches(compEntity));
        Assert.True(matchAny.Matches(compObject));
    }

    
    [Fact]
    public static void Target_Matches_only_Targeted()
    {
        using var world = new World();

        var matchTarget = Match.AnyRelation<string>();

        var compPlain = Match.PlainComponent<string>();
        var compEntity = Match.SpecificEntity<string>(world.Spawn());
        var compObject = Match.SpecificLink<string>("erwin");

        Assert.False(matchTarget.Matches(compPlain));
        Assert.True(matchTarget.Matches(compEntity));
        Assert.True(matchTarget.Matches(compObject));
    }
    
    
    [Fact]
    public static void Object_Matches_only_Object()
    {
        using var world = new World();

        var matchObject = Match.AnyObject<string>();

        var compPlain = Match.PlainComponent<string>();
        var compEntity = Match.SpecificEntity<string>(world.Spawn());
        var compObject = Match.SpecificLink<string>("erwin");

        Assert.False(matchObject.Matches(compPlain));
        Assert.False(matchObject.Matches(compEntity));
        Assert.True(matchObject.Matches(compObject));
    }
    
    
    [Fact]
    public static void Entity_Matches_only_Entity()
    {
        using var world = new World();

        var matchObject = Match.AnyEntity<string>();

        var compPlain = Match.PlainComponent<string>();
        var compEntity = Match.SpecificEntity<string>(world.Spawn());
        var compObject = Match.SpecificLink<string>("erwin");

        Assert.False(matchObject.Matches(compPlain));
        Assert.True(matchObject.Matches(compEntity));
        Assert.False(matchObject.Matches(compObject));
    }
    
    
    [Fact]
    public static void Plain_Matches_only_Plain()
    {
        using var world = new World();

        var matchPlain = Match.PlainComponent<string>();

        var compPlain = Match.PlainComponent<string>();
        var compEntity = Match.SpecificEntity<string>(world.Spawn());
        var compObject = Match.SpecificLink<string>("erwin");

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
        
        var matchObject = Match.SpecificEntity<string>(right);

        var compEntityRight = Match.SpecificEntity<string>(right);
        var compEntityWrong = Match.SpecificEntity<string>(wrong);
        
        Assert.True(matchObject.Matches(compEntityRight));
        Assert.False(matchObject.Matches(compEntityWrong));
    }

    [Fact]
    public static void Specific_Object_Matches_only_Specific_interned()
    {
        using var world = new World();

        // CAUTION - string interning might cause flukes / weird artifacts
        const string right = "erwin";
        const string wrong = "different";
        
        var matchObject = Match.SpecificLink(right);

        var compObjectRight = Match.SpecificLink(right);
        var compObjectWrong = Match.SpecificLink(wrong);
        
        Assert.True(matchObject.Matches(compObjectRight));
        Assert.False(matchObject.Matches(compObjectWrong));
    }

    
    [Fact]
    public static void Specific_Object_Matches_only_Specific()
    {
        using var world = new World();

        List<string> right = ["erwin"];
        List<string> wrong = ["different"];
        
        var matchObject = Match.SpecificLink(right);

        var compObjectRight = Match.SpecificLink(right);
        var compObjectWrong = Match.SpecificLink(wrong);
        
        Assert.True(matchObject.Matches(compObjectRight));
        Assert.False(matchObject.Matches(compObjectWrong));
    }

    [Fact]
    public static void Can_Expand_Plain()
    {
        var type = TypeExpression.Of<int>(Match.Plain);
        
        var expanded = type.Expand();
        var anyInt = TypeExpression.Of<int>(Match.Any);
        Assert.Contains(type, expanded);
        Assert.Contains(anyInt, expanded);
        Assert.Equal(2, expanded.Count);
    }

    [Fact]
    public static void Can_Expand_Entity()
    {
        var world = new World();
        var entity = world.Spawn();
        var type = TypeExpression.Of<int>(entity);
        
        var expanded = type.Expand();
        var anyInt = TypeExpression.Of<int>(Match.Any);
        var targetInt = TypeExpression.Of<int>(Match.Target);
        var entityInt = TypeExpression.Of<int>(Match.Entity);
        Assert.Contains(type, expanded);
        Assert.Contains(anyInt, expanded);
        Assert.Contains(targetInt, expanded);
        Assert.Contains(entityInt, expanded);
        Assert.Equal(4, expanded.Count);
    }

    [Fact]
    public static void Can_Expand_Object()
    {
        var world = new World();
        var entity = world.Spawn();
        var type = TypeExpression.Of<string>(Link.With("dieter"));
        
        var expanded = type.Expand();
        var wildAny = TypeExpression.Of<string>(Match.Any);
        var wildTarget = TypeExpression.Of<string>(Match.Target);
        var wildObject = TypeExpression.Of<string>(Match.Object);
        Assert.Contains(type, expanded);
        Assert.Contains(wildAny, expanded);
        Assert.Contains(wildTarget, expanded);
        Assert.Contains(wildObject, expanded);
        Assert.Equal(4, expanded.Count);
    }

    [Fact]
    public static void Can_Expand_Target()
    {
        var type = TypeExpression.Of<string>(Match.Target);
        
        var expanded = type.Expand();
        var wildAny = TypeExpression.Of<string>(Match.Any);
        var wildEntity = TypeExpression.Of<string>(Match.Entity);
        var wildObject = TypeExpression.Of<string>(Match.Object);
        Assert.Contains(type, expanded);
        Assert.Contains(wildAny, expanded);
        Assert.Contains(wildEntity, expanded);
        Assert.Contains(wildObject, expanded);
        Assert.Equal(4, expanded.Count);
    }
}
