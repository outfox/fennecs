namespace fennecs.tests.Stream.Job;

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
            lock(list) Assert.True(list.Remove((e.Entity, str.read)));
        });
        
        Assert.Empty(list);
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

        stream = world.Query<string>(Match.Link).Stream();
        Assert.Throws<InvalidOperationException>(() => stream.Job((str) => { output.WriteLine(str); }));

        stream = world.Query<string>(default(Key)).Stream();
        var ran = false;
        stream.Job((str) =>
        {
            output.WriteLine(str);
            ran = true;
        });
        Assert.True(ran);
    }


    [Theory]
    [ClassData(typeof(QueryCountGenerator))]
    private void Query_Job_Count_Accurate(int count, bool createEmptyTable)
    {
        using var world = new World();

        if (createEmptyTable)
        {
            var dead = world.Spawn().Add<int>().Add("will be removed");
            world.Despawn(dead);
        }

        List<Entity> entities = new(count);

        var query = world.Query<int>().Stream();

        query.Raw(integers => { Assert.Equal(0, integers.Length); });

        for (var index = 0; index < count; index++)
        {
            var captured = index;
            query.Raw(integers => { Assert.Equal(captured, integers.Length); });

            entities.Add(
                world.Spawn()
                    .Add(index)
            );
        }

        query.Raw(integers => { Assert.Equal(count, integers.Length); });

        var random = new Random(69 + count);

        for (var i = count; i > 0; i--)
        {
            var captured = i;
            query.Raw(integers => { Assert.Equal(captured, integers.Length); });

            var removalIndex = random.Next(entities.Count);
            var removalEntity = entities[removalIndex];
            entities.RemoveAt(removalIndex);
            world.Despawn(removalEntity);
        }

        query.Raw(integers => { Assert.Equal(0, integers.Length); });
    }
    
    [Theory]
    [ClassData(typeof(QueryCountGenerator))]
    private void Job_Visits_All_Entities(int count, bool createEmptyTable)
    {
        using var world = new World();

        if (createEmptyTable)
        {
            var dead = world.Spawn().Add<int>().Add("will be removed");
            world.Despawn(dead);
        }

        for (var index = 0; index < count; index++)
            world.Spawn()
                .Add(index);

        var query = world.Query<int>(default(Key)).Stream();

        var processed = 0;
        query.Job((index) =>
        {
            Interlocked.Increment(ref processed);
            index.write = 123;
        });

        Assert.Equal(count, processed);

        query.Job((index) =>
        {
            ArgumentOutOfRangeException.ThrowIfNegative(index.read);
            Assert.Equal(123, index);
        });
    }
}