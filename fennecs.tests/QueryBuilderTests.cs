namespace fennecs.tests;

public class QueryBuilderTests
{
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