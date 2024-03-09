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
    public void Table_Resizing_Fails_On_Wrong_Size()
    {
        using var world = new World();
        var identity = world.Spawn().Add("foo").Add(123).Add(17.0f);

        var table = world.GetEntityMeta(identity).Archetype;

        Assert.Throws<ArgumentOutOfRangeException>(() => table.Resize(-1));
        Assert.Throws<ArgumentOutOfRangeException>(() => table.Resize(0));
    }


    [Fact]
    public void Table_Resizing_Matches_Length()
    {
        using var world = new World();
        var identity = world.Spawn().Add("foo").Add(123).Add(17.0f);

        var table = world.GetEntityMeta(identity).Archetype;

        table.Resize(10);
        Assert.Equal(10, table.Capacity);

        table.Resize(5);
        Assert.Equal(5, table.Capacity);
    }


    [Fact]
    public void Table_GetStorage_Returns_System_Array()
    {
        using var world = new World();
        var identity = world.Spawn().Add("foo").Add(123).Add(17.0f);
        var table = world.GetEntityMeta(identity).Archetype;
        var storage = table.GetStorage(TypeExpression.Of<string>(Match.Plain));
        Assert.IsAssignableFrom<Array>(storage);
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
}