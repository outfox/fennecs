namespace fennecs.tests;

public class ArchetypeTests(ITestOutputHelper output)
{
    [Fact]
    public void Table_String_Contains_Types()
    {
        var world = new World();
        var identity = world.Spawn().Add("foo").Add(123).Add(17.0f).Id();

        var table = world.GetTable(world.GetEntityMeta(identity).TableId);

        output.WriteLine(table.ToString());
        Assert.Contains(typeof(Entity).ToString(), table.ToString());
        Assert.Contains(typeof(string).ToString(), table.ToString());
        Assert.Contains(typeof(int).ToString(), table.ToString());
        Assert.Contains(typeof(float).ToString(), table.ToString());
    }

    [Fact]
    public void Table_Resizing_Fails_On_Wrong_Size()
    {
        var world = new World();
        var identity = world.Spawn().Add("foo").Add(123).Add(17.0f).Id();

        var table = world.GetTable(world.GetEntityMeta(identity).TableId);

        Assert.Throws<ArgumentOutOfRangeException>(() => table.Resize(-1));
        Assert.Throws<ArgumentOutOfRangeException>(() => table.Resize(0));
    }

    [Fact]
    public void Table_Resizing_Matches_Length()
    {
        var world = new World();
        var identity = world.Spawn().Add("foo").Add(123).Add(17.0f).Id();

        var table = world.GetTable(world.GetEntityMeta(identity).TableId);

        table.Resize(10);
        Assert.Equal(10, table.Capacity);

        table.Resize(5);
        Assert.Equal(5, table.Capacity);
    }
    
    
    [Fact]
    public void Table_GetStorage_Returns_System_Array()
    {
        var world = new World();
        var identity = world.Spawn().Add("foo").Add(123).Add(17.0f).Id();
        var table = world.GetTable(world.GetEntityMeta(identity).TableId);
        var storage = table.GetStorage(TypeExpression.Create<string>(Entity.None));
        Assert.IsAssignableFrom<Array>(storage);
    }
    
    [Fact]
    public void Table_Matches_TypeExpression()
    {
        var world = new World();
        var identity = world.Spawn().Add("foo").Add(123).Add(17.0f).Id();
        var table = world.GetTable(world.GetEntityMeta(identity).TableId);

        var typeExpression = TypeExpression.Create<string>(Entity.None);
        Assert.True(table.Matches(typeExpression));

        var typeExpressionAny = TypeExpression.Create<string>(Entity.Any);
        Assert.False(table.Matches(typeExpressionAny));

        var typeExpressionTarget = TypeExpression.Create<string>(new Entity(99999));
        Assert.False(table.Matches(typeExpressionTarget));
    }
}