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
        var q2 = world.Query<int>(Match.Entity);
        var q3 = world.Query<int, string>(Match.Any, Match.Plain);
        var q4 = world.Query<int, string, double>(Match.Object, Match.Target, Match.Plain);
        var q5 = world.Query<int, string, double, float>(Match.Object, Match.Target, Match.Plain, Match.Any);
        var q6 = world.Query<int, string, double, float, long>(Match.Object, Match.Target, Match.Plain, Match.Any, Match.Object);
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
    private void Disallows_Repeat_Type()
    {
        using var world = new World();
        using var builder = world.Query();
        Assert.NotNull(builder);
        builder
            .Has<float>()
            .Has<string>("123")
            .Not<double>()
            .Not(Link.With(new List<int>()))
            .Any<long>()
            .Any(Link.With(new List<float>()));

        Assert.Throws<InvalidOperationException>(() => builder.Has<float>());
        Assert.Throws<InvalidOperationException>(() => builder.Not<float>());
        Assert.Throws<InvalidOperationException>(() => builder.Any<float>());
    }

    [Fact]
    private void Disallows_Repeat_Type_With_Match()
    {
        using var world = new World();
        using var builder = world.Query();
        var entity = world.Spawn();
        Assert.NotNull(builder);
        builder
            .Has<float>(entity)
            .Has<string>("123")
            .Not<double>()
            .Not(Link.With(new List<int>()))
            .Any<long>()
            .Any(Link.With(new List<float>()));

        Assert.Throws<InvalidOperationException>(() => builder.Has<float>(Match.Any));
        Assert.Throws<InvalidOperationException>(() => builder.Not<List<int>>(Match.Object));
        Assert.Throws<InvalidOperationException>(() => builder.Any<float>(Match.Entity));
    }


    [Fact]
    private void Disallows_Conflicting_Type()
    {
        using var world = new World();
        using var builder = world.Query();
        Assert.NotNull(builder);
        builder
            .Has<float>()
            .Has<string>("123")
            .Not<double>()
            .Not<int>()
            .Any<long>()
            .Any(Link.With(new List<float>()));

        Assert.Throws<InvalidOperationException>(() => builder.Has<int>());
        Assert.Throws<InvalidOperationException>(() => builder.Not<string>(Match.Object));
        Assert.Throws<InvalidOperationException>(() => builder.Any<double>());

        builder.Has<string>("I'm different");
        builder.Not<string>("Think different");
    }


    [Fact]
    private void Unchecked_Allows_Conflicting_and_Repeat_Type()
    {
        using var world = new World();
        using var builder = world.Query().Unchecked();

        builder
            .Has<float>()
            .Has<string>("123")
            .Not<double>()
            .Not<int>()
            .Any<long>()
            .Any(Link.With(new List<float>()));

        //Conflict
        builder.Has<int>().Not<string>(Match.Object).Any<double>();

        //Repeat
        builder.Has<int>();

        builder.Has<string>("I'm allowed");
        builder.Not<string>("Say that aloud?");

        builder.Compile();
    }


    [Fact]
    private void Cannot_Add_Stream_Type_Twice()
    {
        using var world = new World();
        var builder = world.Query<int>();
        Assert.NotNull(builder);
        Assert.Throws<InvalidOperationException>(() => builder.Has<int>());
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
        var builder = world.Query<object, string>();

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


    [Fact]
    private void Can_Create_Unchecked_Queries()
    {
        using var world = new World();
        world.Query().Unchecked().Has<Vector4>().Has<Vector4>().Compile();
        world.Query<Vector4>().Unchecked().Has<Vector4>().Stream();
        world.Query<Vector3, Vector4>().Unchecked().Has<Vector4>().Stream();
        world.Query<Vector2, Vector3, Vector4>().Unchecked().Has<Vector4>().Stream();
        world.Query<float, Vector2, Vector3, Vector4>().Unchecked().Has<Vector4>().Stream();
        world.Query<int, float, Vector2, Vector3, Vector4>().Unchecked().Has<Vector4>().Stream();
    }

    
    [Fact]
    private void Cannot_Create_Duplicate_Has()
    {
        using var world = new World();
        
        Assert.Throws<InvalidOperationException>(() =>
        {
            world.Query().Has<Vector4>().Has<Vector4>().Compile();
        });

        Assert.Throws<InvalidOperationException>(() =>
        {
            world.Query<Vector4>().Has<Vector4>().Stream();
        });

        Assert.Throws<InvalidOperationException>(() =>
        {
            world.Query<Vector3, Vector4>().Has<Vector4>().Stream();
        });
        
        Assert.Throws<InvalidOperationException>(() =>
        {
            world.Query<Vector2, Vector3, Vector4>().Has<Vector4>().Stream();
        });
        
        Assert.Throws<InvalidOperationException>(() =>
        {
            world.Query<float, Vector2, Vector3, Vector4>().Has<Vector4>().Stream();
        });
        
        //TODO: This test failed once for "no" reason?.
        Assert.Throws<InvalidOperationException>(() =>
        {
            world.Query<int, float, Vector2, Vector3, Vector4>().Has<Vector4>().Stream();
        });
    }
}
