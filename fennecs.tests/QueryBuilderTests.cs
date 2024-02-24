namespace fennecs.tests;

public class QueryBuilderTests
{
    [Fact]
    private void All_QueryBuilders_Available()
    {
        using var world = new World();
        var q1 = world.Query();
        var q2 = world.Query<int>();
        var q3 = world.Query<int, string>();
        var q4 = world.Query<int, string, double>();
        var q5 = world.Query<int, string, double, float>();
        var q6 = world.Query<int, string, double, float, long>();
        Assert.NotNull(q1);
        Assert.NotNull(q2);
        Assert.NotNull(q3);
        Assert.NotNull(q4);
        Assert.NotNull(q5);
        Assert.NotNull(q6);
    }

    [Fact]
    private void All_QueryBuilders_Available_with_MatchExpressions()
    {
        using var world = new World();
        var q1 = world.Query();
        var q2 = world.Query<int>(Match.Identity);
        var q3 = world.Query<int, string>(Match.Any, Match.Plain);
        var q4 = world.Query<int, string, double>(Match.Object, Match.Relation, Match.Plain);
        var q5 = world.Query<int, string, double, float>(Match.Object, Match.Relation, Match.Plain, Match.Any);
        var q6 = world.Query<int, string, double, float, long>(Match.Object, Match.Relation, Match.Plain, Match.Any, Match.Object);
        Assert.NotNull(q1);
        Assert.NotNull(q2);
        Assert.NotNull(q3);
        Assert.NotNull(q4);
        Assert.NotNull(q5);
        Assert.NotNull(q6);
    }

    [Fact]
    private void Can_Create_Query()
    {
        using var world = new World();
        using var builder = world.Query();
        Assert.NotNull(builder);
        builder
            .Has<float>()
            .Has<string>("123")
            .Not<double>()
            .Not(new List<int>())
            .Any<long>()
            .Any(new List<float>());
    }
    
    [Fact]
    private void Can_Create_C1_Query()
    {
        using var world = new World();
        using var builder = world.Query<int>();
        Assert.NotNull(builder);
        builder
            .Has<float>()
            .Has<string>("123")
            .Not<double>()
            .Not(new List<int>())
            .Any<long>()
            .Any(new List<float>());
    }
    
    [Fact]
    private void Can_Create_C1_C2_Query()
    {
        using var world = new World();
        using var builder = world.Query<int, string>();
        Assert.NotNull(builder);
        builder
            .Has<float>()
            .Has<string>("123")
            .Not<double>()
            .Not(new List<int>())
            .Any<long>()
            .Any(new List<float>());
    }
    
    [Fact]
    private void Can_Create_C1_C2_C3_Query()
    {
        using var world = new World();
        using var builder = world.Query<int, string, double>();
        Assert.NotNull(builder);
        builder
            .Has<float>()
            .Has<string>("123")
            .Not<double>()
            .Not(new List<int>())
            .Any<long>()
            .Any(new List<float>());
    }
    
    [Fact]
    private void Can_Create_C1_C2_C3_C4_Query()
    {
        using var world = new World();
        var builder = world.Query<int, string, double, float>();
        Assert.NotNull(builder);
        builder
            .Has<float>()
            .Has<string>("123")
            .Not<double>()
            .Not(new List<int>())
            .Any<long>()
            .Any(new List<float>());
    }
    
    [Fact]
    private void Can_Create_C1_C2_C3_C4_C5_Query()
    {
        using var world = new World();
        using var builder = world.Query<int, string, double, float, long>();
        Assert.NotNull(builder);
        builder
            .Has<float>()
            .Has<string>("123")
            .Not<double>()
            .Not(new List<int>())
            .Any<long>()
            .Any(new List<float>());
    }
}