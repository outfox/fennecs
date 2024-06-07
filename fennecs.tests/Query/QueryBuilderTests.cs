using System.Numerics;
using System.Text;

namespace fennecs.tests.Query;

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
        var q2 = world.Query<int>(Identity.Entity);
        var q3 = world.Query<int, string>(Identity.Any, Identity.Plain);
        var q4 = world.Query<int, string, double>(Identity.Object, Identity.Match, Identity.Plain);
        var q5 = world.Query<int, string, double, float>(Identity.Object, Identity.Match, Identity.Plain, Identity.Any);
        var q6 = world.Query<int, string, double, float, long>(Identity.Object, Identity.Match, Identity.Plain, Identity.Any, Identity.Object);
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
            .Has(Link.With("123"))
            .Not<double>()
            .Not(Link.With(new List<int>()))
            .Any<long>()
            .Any(Link.With(new List<float>()));
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
            .Not(Link.With(new List<int>()))
            .Any<long>()
            .Any(Link.With(new List<float>()));
    }


    [Fact]
    private void Can_Create_C1_C2_Query()
    {
        using var world = new World();
        using var builder = world.Query<int, string>();
        Assert.NotNull(builder);
        builder.Has(Link.With(new StringBuilder("123")));
        builder.Has<TypeA>(world.Spawn())
            .Has<Thread>()
            .Not<Half>()
            .Not(Link.With(new List<int>()))
            .Any<byte>()
            .Any(Link.With(new List<float>()));
        builder.Compile();
    }


    [Fact]
    private void Can_Create_C1_C2_C3_Query()
    {
        using var world = new World();
        using var builder = world.Query<int, string, double>();
        Assert.NotNull(builder);
        builder.Has(Link.With(new StringBuilder("123")));
        builder.Has<TypeA>(world.Spawn())
            .Has<Thread>()
            .Not<Half>()
            .Not(Link.With(new List<int>()))
            .Any<byte>()
            .Any(Link.With(new List<float>()));
        builder.Compile();
    }


    [Fact]
    private void Can_Create_C1_C2_C3_C4_Query()
    {
        using var world = new World();
        var builder = world.Query<int, string, double, float>();
        Assert.NotNull(builder);
        builder.Has(Link.With(new StringBuilder("123")));
        builder.Has<TypeA>(world.Spawn())
            .Has<Thread>()
            .Not<Half>()
            .Not(Link.With(new List<int>()))
            .Any<byte>()
            .Any(Link.With(new List<float>()));
        builder.Compile();
    }


    struct TypeA;

    [Fact]
    private void Can_Create_C1_C2_C3_C4_C5_Query()
    {
        using var world = new World();
        using var builder = world.Query<int, string, double, float, long>();
        Assert.NotNull(builder);
        builder.Has(Link.With(new StringBuilder("123")));
        builder.Has<TypeA>(world.Spawn())
            .Has<Thread>()
            .Not<Half>()
            .Not(Link.With(new List<int>()))
            .Any<byte>()
            .Any(Link.With(new List<float>()));
        builder.Compile();
    }

    [Fact]
    private void Builder_Allows_Conflicting_and_Repeat_Type()
    {
        using var world = new World();
        using var builder = world.Query();

        builder
            .Has<float>()
            .Has<string>("123")
            .Not<double>()
            .Not<int>()
            .Any<long>()
            .Any(Link.With(new List<float>()));

        //Conflict
        builder.Has<int>().Not<string>(Identity.Object).Any<double>();

        //Repeat
        builder.Has<int>();

        builder.Has<string>("I'm allowed");
        builder.Not<string>("Say that aloud?");

        builder.Compile();
    }

    [Fact]
    private void Can_Create_Cached_Queries_1()
    {
        using var world = new World();
        var builder = world.Query<int>();

        var query1 = builder.Compile();
        var query2 = builder.Compile();
        Assert.True(query1 == query2);
    }


    [Fact]
    private void Streams_are_Unique_2()
    {
        using var world = new World();
        var builder = world.Query<float, string>();

        var query1 = builder.Stream();
        var query2 = builder.Stream();
        Assert.False(query1 == query2);
    }


    [Fact]
    private void Can_Create_Cached_Queries_2()
    {
        using var world = new World();
        var builder = world.Query<int, string>();

        var query1 = builder.Compile();
        var query2 = builder.Compile();
        Assert.True(query1 == query2);
    }


    [Fact]
    private void Streams_are_Unique_3()
    {
        using var world = new World();
        var builder = world.Query<Random, float, string>();

        var query1 = builder.Stream();
        var query2 = builder.Stream();
        Assert.False(query1 == query2);
    }


    [Fact]
    private void Can_Create_Cached_Queries_3()
    {
        using var world = new World();
        var builder = world.Query<Vector3, object, string>();

        var query1 = builder.Compile();
        var query2 = builder.Compile();
        Assert.True(query1 == query2);
    }


    [Fact]
    private void Streams_are_Unique_4()
    {
        using var world = new World();
        var builder = world.Query<byte, Random, float, string>();

        var query1 = builder.Stream();
        var query2 = builder.Stream();
        Assert.False(query1 == query2);
    }


    [Fact]
    private void Can_Create_Cached_Queries_4()
    {
        using var world = new World();
        var builder = world.Query<int, Vector3, object, string>();

        var query1 = builder.Compile();
        var query2 = builder.Compile();
        Assert.True(query1 == query2);
    }


    [Fact]
    private void Streams_are_Unique_5()
    {
        using var world = new World();
        var builder = world.Query<object, int, double, float, string>();

        var query1 = builder.Stream();
        var query2 = builder.Stream();
        Assert.False(query1 == query2);
    }


    [Fact]
    private void Can_Create_Cached_Queries_5()
    {
        using var world = new World();
        var builder = world.Query<Vector4, Vector3, Vector2, object, string>();

        var query1 = builder.Compile();
        var query2 = builder.Compile();
        Assert.True(query1 == query2);
    }
}
