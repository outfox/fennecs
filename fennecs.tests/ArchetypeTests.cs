using System.Collections;

namespace fennecs.tests;

public class ArchetypeTests(ITestOutputHelper output)
{
    [Fact]
    public void Table_String_Contains_Types()
    {
        using var world = new World();
        var identity = world.Spawn().Add("foo").Add(123).Add(17.0f);

        var table = world.GetEntityMeta(identity).Archetype;

        output.WriteLine(table.ToString());
        Assert.Contains(typeof(Identity).ToString(), table.ToString());
        Assert.Contains(typeof(string).ToString(), table.ToString());
        Assert.Contains(typeof(int).ToString(), table.ToString());
        Assert.Contains(typeof(float).ToString(), table.ToString());
    }
    

    [Fact]
    public void GetStorage_Returns_IStorage_Backed_By_Specific_Type()
    {
        using var world = new World();
        var identity = world.Spawn().Add("foo").Add(123).Add(17.0f);
        var table = world.GetEntityMeta(identity).Archetype;
        var storage = table.GetStorage(TypeExpression.Of<string>(Match.Plain));
        Assert.IsAssignableFrom<IStorage>(storage);
        Assert.IsAssignableFrom<Storage<string>>(storage);
    }
    
    [Fact]
    public void Table_Matches_TypeExpression()
    {
        using var world = new World();
        var identity = world.Spawn().Add("foo").Add(123).Add(17.0f).Id;
        var table = world.GetEntityMeta(identity).Archetype;

        var typeExpression = TypeExpression.Of<string>(Match.Plain);
        Assert.True(table.Matches(typeExpression));

        var typeExpressionAny = TypeExpression.Of<string>(Match.Any);
        Assert.True(table.Matches(typeExpressionAny));

        var typeExpressionTarget = TypeExpression.Of<string>(new Identity(99999));
        Assert.False(table.Matches(typeExpressionTarget));
    }


    [Fact]
    public void Table_Can_be_Generically_Enumerated()
    {
        using var world = new World();
        var other = world.Spawn().Add("foo").Add(123).Add(17.0f).Id;
        var table = world.GetEntityMeta(other).Archetype;

        var count = 0;
        foreach (var entity in (IEnumerable) table)
        {
            count++;
            Assert.Equal(entity, entity);
        }

        Assert.Equal(1, count);
    }


    [Fact]
    public void Can_Truncate_Nothing()
    {
        using var world = new World();
        var identity = world.Spawn().Add("foo").Add(123).Add(17.0f).Id;
        var table = world.GetEntityMeta(identity).Archetype;

        table.Truncate(2000);
        Assert.Equal(1, table.Count);
        table.Truncate(1);
        Assert.Equal(1, table.Count);
    }


    [Fact]
    public void Can_Truncate_Negative()
    {
        using var world = new World();
        var identity = world.Spawn().Add("foo").Add(123).Add(17.0f).Id;
        var table = world.GetEntityMeta(identity).Archetype;

        table.Truncate(-2);
        Assert.Equal(0, table.Count);
    }

    [Fact]
    public void Moved_Entity_Leaves_Archetype()
    {
        using var world = new World();

        var entity = world.Spawn();
        var entityInt = world.Spawn().Add(123);
        
        var queryAll = world.Query().Build();
        var queryInt = world.Query().Has<int>().Build();
        
        Assert.Equal(2, queryAll.Count);
        Assert.Equal(1, queryInt.Count);
    }

    [Fact]
    public void IsComparable_Same_As_Signature()
    {
        using var world = new World();
        var entity1 = world.Spawn().Add("foo").Add(123).Add(17.0f).Id;
        var entity2 = world.Spawn().Add(123).Add(17.0f).Id;
        
        var table1 = world.GetEntityMeta(entity1).Archetype;
        var table2 = world.GetEntityMeta(entity1).Archetype;

        Assert.True(table1.CompareTo(table2) == table1.Signature.CompareTo(table2.Signature));

        Assert.True(table1.CompareTo(null) == table1.Signature.CompareTo(default));
    }

}