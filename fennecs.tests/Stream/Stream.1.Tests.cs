using System.Collections;
using System.Runtime.CompilerServices;
using fennecs.storage;

namespace fennecs.tests.Stream;

public class Stream1Tests(ITestOutputHelper output)
{

    [Fact]
    public void Can_Run_New_Job()
    {
        using var world = new World();
        var arnold = world.Spawn().Add("Arnold");
        var dolph = world.Spawn().Add("Dolph");

        List<(Entity, string)> list = [(arnold, "Arnold"), (dolph, "Dolph")];

        var stream = world.Stream<string>();
        stream.Job((e, str) =>
        {
            output.WriteLine(str.read);
            output.WriteLine(e.ToString());
            
            list.Remove(((Entity) e, str.read));
        });
        
        Assert.Empty(list);
    }


    [Fact]
    public void Can_Enumerate_Stream()
    {
        using var world = new World();
        var arnold = world.Spawn().Add("Arnold");
        var dolph = world.Spawn().Add("Dolph");

        List<(Entity, string)> list = [(arnold, "Arnold"), (dolph, "Dolph")];

        var stream = world.Stream<string>();
        foreach (var row in stream)
        {
            Assert.True(list.Remove(row));
        }

        Assert.Empty(list);
    }


    [Fact]
    public void Can_Enumerate_Boxed()
    {
        using var world = new World();
        var arnold = world.Spawn().Add("Arnold");
        var dolph = world.Spawn().Add("Dolph");

        List<object> list = [(arnold, "Arnold"), (dolph, "Dolph")];

        IEnumerable stream = world.Stream<string>();
        foreach (var row in stream)
        {
            Assert.True(list.Remove(row));
        }

        Assert.Empty(list);
    }

    [Fact]
    public void Cannot_Structural_Chane_While_Enumerating()
    {
        using var world = new World();
        world.Spawn().Add("Arnold");
        world.Spawn().Add("Dolph");

        var stream = world.Stream<string>();
        Assert.Throws<InvalidOperationException>(() =>
        {
            foreach (var row in stream)
            {
                world.Spawn().Add("Sylvester");
            }
        });
    }


    [Fact]
    public void Can_Create_Stream_From_Query()
    {
        using var world = new World();
        var query = world.Query<string>().Compile();

        var stream = query.Stream<string>();

        world.Spawn().Add("Arnold");
        world.Spawn().Add("Dolph");

        List<string> list = ["Arnold", "Dolph"];

        stream.For((c0) =>
        {
            list.Remove(c0.read);
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

        stream.For((c0) =>
        {
            Assert.True(list.Remove(c0.read));
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
        stream1.For((c0) =>
        {
            Assert.True(list.Remove(c0.read));
        });
        Assert.Empty(list);

        List<int> list2 = [123, 678];
        stream2.For((c0) =>
        {
            Assert.True(list2.Remove(c0.read));
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
    public void Cannot_Run_Job_on_Wildcard_Query()
    {
        using var world = new World();
        world.Spawn().Add("jason");

        var stream = world.Query<string>(Match.Any).Stream();
        Assert.Throws<InvalidOperationException>(() => stream.Job(str => { output.WriteLine(str); }));

        stream = world.Query<string>(Match.Entity).Stream();
        Assert.Throws<InvalidOperationException>(() => stream.Job((str) => { output.WriteLine(str); }));

        stream = world.Query<string>(Match.Target).Stream();
        Assert.Throws<InvalidOperationException>(() => stream.Job((str) => { output.WriteLine(str); }));

        stream = world.Query<string>(Match.Object).Stream();
        Assert.Throws<InvalidOperationException>(() => stream.Job((str) => { output.WriteLine(str); }));

        stream = world.Query<string>(Match.Plain).Stream();
        var ran = false;
        stream.Job((str) =>
        {
            output.WriteLine(str);
            ran = true;
        });
        Assert.True(ran);
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
