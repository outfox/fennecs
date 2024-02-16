namespace fennecs.tests;

public class TableTests(ITestOutputHelper output)
{
    [Fact]
    public void Table_String_Contains_Types()
    {
        var world = new World();
        var entity = world.Spawn().Add("foo").Add(123).Add(17.0f).Id();

        var table = world.Archetypes.GetTable(world.Archetypes.GetEntityMeta(entity).TableId);

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
        var entity = world.Spawn().Add("foo").Add(123).Add(17.0f).Id();

        var table = world.Archetypes.GetTable(world.Archetypes.GetEntityMeta(entity).TableId);

        Assert.Throws<ArgumentOutOfRangeException>(() => table.Resize(-1));
        Assert.Throws<ArgumentOutOfRangeException>(() => table.Resize(0));
    }

    [Fact]
    public void Table_Resizing_Matches_Length()
    {
        var world = new World();
        var entity = world.Spawn().Add("foo").Add(123).Add(17.0f).Id();

        var table = world.Archetypes.GetTable(world.Archetypes.GetEntityMeta(entity).TableId);

        table.Resize(10);
        Assert.Equal(10, table.Capacity);

        table.Resize(5);
        Assert.Equal(5, table.Capacity);
    }
    
    
    [Fact]
    public void Table_GetStorage_Returns_Array()
    {
        var world = new World();
        var entity = world.Spawn().Add("foo").Add(123).Add(17.0f).Id();
        var table = world.Archetypes.GetTable(world.Archetypes.GetEntityMeta(entity).TableId);
        var storage = table.GetStorage(TypeExpression.Create<string>(Identity.None));
        Assert.IsAssignableFrom<Array>(storage);
    }
    
    [Fact]
    public void Table_Matches_TypeExpression()
    {
        var world = new World();
        var entity = world.Spawn().Add("foo").Add(123).Add(17.0f).Id();
        var table = world.Archetypes.GetTable(world.Archetypes.GetEntityMeta(entity).TableId);

        var typeExpression = TypeExpression.Create<string>(Identity.None);
        Assert.True(table.Matches(typeExpression));

        var typeExpressionAny = TypeExpression.Create<string>(Identity.Any);
        Assert.False(table.Matches(typeExpressionAny));

        var typeExpressionTarget = TypeExpression.Create<string>(new Identity(99999));
        Assert.False(table.Matches(typeExpressionTarget));
    }
}