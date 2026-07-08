namespace fennecs.tests.Stream;

// The shared Stream test battery (enumeration, runners, filters, counts, etc.)
// is generated for all arities and component type sets by generators/Stream.Tests.tt.
// This file keeps only the tests that are unique to arity 1 / not arity-shaped.
public class Stream1Tests
{
    [Fact]
    public void Can_Create_Stream_From_Query()
    {
        using var world = new World();
        var query = world.Query<string>().Compile();

        var stream = query.Stream<string>();

        world.Spawn().Add("Arnold");
        world.Spawn().Add("Dolph");

        List<string> list = ["Arnold", "Dolph"];

        stream.For((ref c0) =>
        {
            list.Remove(c0);
        });

        Assert.Empty(list);
    }

    [Fact]
    public void Can_Create_Reference_Stream_From_World()
    {
        using var world = new World();

        var stream = world.Stream<string>();

        world.Spawn().Add("Arnold");
        world.Spawn().Add("Dolph");

        List<string> list = ["Arnold", "Dolph"];

        stream.For((ref c0) =>
        {
            Assert.True(list.Remove(c0));
        });

        Assert.Empty(list);
    }

    [Fact]
    public void Can_Create_Value_Stream_From_World()
    {
        using var world = new World();

        var stream1 = world.Stream<string>();
        var stream2 = world.Stream<int>();

        world.Spawn().Add("Arnold").Add(123);
        world.Spawn().Add("Dolph").Add(678);

        List<string> list = ["Arnold", "Dolph"];
        stream1.For((ref c0) =>
        {
            Assert.True(list.Remove(c0));
        });
        Assert.Empty(list);

        List<int> list2 = [123, 678];
        stream2.For((ref c0) =>
        {
            Assert.True(list2.Remove(c0));
        });
        Assert.Empty(list2);
    }

    [Fact]
    public void World_Stream_Backed_by_Cached_Query()
    {
        using var world = new World();
        var query = world.Query<string>().Compile();

        var stream1 = world.Stream<string>();
        var stream2 = world.Stream<string>();
        var stream3 = query.Stream<string>();

        Assert.Equal(stream1.Query, stream2.Query);
        Assert.Equal(stream1.Query, stream3.Query);
    }

    [Fact]
    public void Streams_Can_Batch()
    {
        using var world = new World();
        var entity = world.Spawn().Add("jason");

        var stream = world.Query<string>(Match.Any).Stream();

        Assert.True(entity.Has<string>());
        stream.Batch().Remove<string>().Submit();
        Assert.False(entity.Has<string>());

        Assert.Empty(world.Query<string>().Compile());
    }

    [Fact]
    public void Streams_Can_Batch_With_params()
    {
        using var world = new World();
        var entity = world.Spawn().Add("jason");

        var stream = world.Query<string>(Match.Any).Stream();

        Assert.True(entity.Has<string>());
        stream.Batch(Batch.AddConflict.Replace, Batch.RemoveConflict.Allow).Remove<string>().Submit();
        Assert.False(entity.Has<string>());

        Assert.Empty(world.Query<string>().Compile());
    }

    [Fact]
    public void Streams_Can_Batch_With_params1()
    {
        using var world = new World();
        var entity = world.Spawn().Add("jason");

        var stream = world.Query<string>(Match.Any).Stream();

        Assert.True(entity.Has<string>());
        stream.Batch(Batch.AddConflict.Replace).Remove<string>().Submit();
        Assert.False(entity.Has<string>());

        Assert.Empty(world.Query<string>().Compile());
    }

    [Fact]
    public void Streams_Can_Batch_With_params2()
    {
        using var world = new World();
        var entity = world.Spawn().Add("jason");

        var stream = world.Query<string>(Match.Any).Stream();

        Assert.True(entity.Has<string>());
        stream.Batch(Batch.RemoveConflict.Allow).Remove<string>().Submit();
        Assert.False(entity.Has<string>());

        Assert.Empty(world.Query<string>().Compile());
    }

    [Fact]
    public void Streams_Can_Despawn()
    {
        using var world = new World();
        var entity = world.Spawn().Add("jason");

        var stream = world.Query<string>(Match.Any).Stream();

        Assert.True(entity.Has<string>());
        stream.Despawn();
        Assert.False(entity.Has<string>());

        Assert.Empty(world.Query<string>().Compile());
    }

    [Fact]
    public void MatchAll_Overloads()
    {
        using var world = new World();
        world.Spawn().Add("69").Add(420).Add(1.0f).Add(new object()).Add('a');

        var query = world.All;
        var stream1 = query.Stream<string>(Match.Any);
        var stream2 = query.Stream<string, int>(Match.Any);
        var stream3 = query.Stream<string, int, float>(Match.Any);
        var stream4 = query.Stream<string, int, float, object>(Match.Any);
        var stream5 = query.Stream<string, int, float, object, char>(Match.Any);

        Assert.Single(stream1);
        Assert.Single(stream2);
        Assert.Single(stream3);
        Assert.Single(stream4);
        Assert.Single(stream5);
    }
}
