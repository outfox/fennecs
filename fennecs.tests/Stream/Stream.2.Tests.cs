using System.Collections;

namespace fennecs.tests.Stream;

// ReSharper disable ParameterOnlyUsedForPreconditionCheck.Local
// ReSharper disable once ClassNeverInstantiated.Global
public class Stream2Tests(ITestOutputHelper output)
{
    [Fact] public void Can_Use_RW_Inferred()
    {
        using var world = new World();
        var entity = world.Spawn();
        entity.Add(123).Add(890f);

        var stream = world.Stream<int, float>();

        stream.For(static (a, b) =>
        {
            Assert.Equal(123, a.read);
            Assert.Equal(890f, b.read);
            b.write = 456f;
        });
    }

    [Fact] public void Can_Use_WR_Inferred()
    {
        using var world = new World();
        var entity = world.Spawn();
        entity.Add(123).Add(890f);

        var stream = world.Stream<int, float>();

        stream.For(static (a, b) =>
        {
            Assert.Equal(123, a.read);
            Assert.Equal(890f, b.read);
        });
        
        stream.For(static (a, b) =>
        {
            Assert.Equal(123, a.read);
            Assert.Equal(890f, b.write);
        });
    }

    [Fact] public void Can_Use_WW_Inferred()
    {
        using var world = new World();
        var entity = world.Spawn();
        entity.Add(123).Add(890f);

        var stream = world.Stream<int, float>();

        stream.For(static (a, b) =>
        {
            Assert.Equal(123, a.read);
            Assert.Equal(890f, b.read);
            a.write = 456;
            b.write = 789f;
        });
    }

    [Fact] public void Can_Use_RR_Inferred()
    {
        using var world = new World();
        var entity = world.Spawn();
        entity.Add(123).Add(890f);

        var stream = world.Stream<int, float>();

        stream.For(static (a, b) =>
        {
            Assert.Equal(123, a.read);
            Assert.Equal(890f, b.read);
        });
    }

    [Fact] public void Can_Use_WW_With_Different_Targets()
    {
        using var world = new World();
        var entity = world.Spawn();
        var target = world.Spawn();
        entity.Add(123, target).Add(456);

        var stream = world.Stream<int, int>(target, default);

        stream.For((a, b) =>
        {
            Assert.Equal(123, a.write);
            Assert.Equal(456, b.write);
            Assert.Equal(a.Match, target);
            Assert.Equal(b.Match, default);
        });

        var swapped = world.Stream<int, int>(default,target);

        swapped.For((b, a) =>
        {
            Assert.Equal(123, a.write);
            Assert.Equal(456, b.write);
            Assert.Equal(a.Match, target);
            Assert.Equal(b.Match, default);
        });
    }

    [Fact]
    public void Can_Use_EWW()
    {
        using var world = new World();
        var entity = world.Spawn();
        var target = world.Spawn();
        entity.Add(123, target).Add(456);

        var stream = world.Stream<int, int>(target, default);

        stream.For((e, a, b) =>
        {
            Assert.Equal(123, a.write);
            Assert.Equal(456, b.write);
            Assert.Equal(a.Match, target);
            Assert.Equal(b.Match, default);

            Assert.True(e.Equals(entity));
            Assert.True(e == entity);
            Assert.True(entity == e);
            Assert.Equal(e, entity);
        });

        var swapped = world.Stream<int, int>(default,target);

        swapped.For((e, b, a) =>
        {
            Assert.Equal(123, a.write);
            Assert.Equal(456, b.write);
            Assert.Equal(a.Match, target);
            Assert.Equal(b.Match, default);

            Assert.True(e.Equals(entity));
            Assert.True(e == entity);
            Assert.True(entity == e);
            Assert.Equal(e, entity);
        });
    }

    [Fact]
    public void Can_Use_ERR()
    {
        using var world = new World();
        var entity = world.Spawn();
        var target = world.Spawn();
        entity.Add(123, target).Add(456);

        var stream = world.Stream<int, int>(target, default);

        stream.For((e, a, b) =>
        {
            Assert.Equal(123, a.read);
            Assert.Equal(456, b.read);

            Assert.True(e.Equals(entity));
            Assert.True(e == entity);
            Assert.True(entity == e);
            Assert.Equal(e, entity);
        });

        var swapped = world.Stream<int, int>(default,target);

        swapped.For((e, b, a) =>
        {
            Assert.Equal(123, a.read);
            Assert.Equal(456, b.read);

            Assert.True(e.Equals(entity));
            Assert.True(e == entity);
            Assert.True(entity == e);
            Assert.Equal(e, entity);
        });
    }

    [Fact]
    public void Can_Enumerate_Stream()
    {
        using var world = new World();
        var arnold = world.Spawn().Add("Arnold").Add(1);
        var dolph = world.Spawn().Add("Dolph").Add(2);

        List<(Entity, string, int)> list = [(arnold, "Arnold", 1), (dolph, "Dolph", 2)];

        var stream = world.Stream<string, int>();
        foreach (var row in stream)
        {
            Assert.True(list.Remove(row));
        }

        Assert.Empty(list);
    }

    [Fact]
    public void Can_Enumerate_Boxed_Inherited()
    {
        using var world = new World();
        var arnold = world.Spawn().Add("Arnold").Add(1);
        var dolph = world.Spawn().Add("Dolph").Add(2);

        List<object> list = [(arnold, "Arnold", 1), (dolph, "Dolph", 2)];

        IEnumerable stream = world.Stream<string, int>();
        foreach (var row in stream)
        {
            Assert.True(list.Remove(row));
        }

        Assert.Empty(list);
    }


    [Fact]
    public void Cannot_Structural_Change_While_Enumerating()
    {
        using var world = new World();
        world.Spawn().Add("Arnold").Add(1);
        world.Spawn().Add("Dolph").Add(2);

        var stream = world.Stream<string, int>();

        Assert.Throws<InvalidOperationException>(() =>
        {
            foreach (var row in stream)
            {
                row.Item1.Remove<int>();
            }
        });
    }

    [Theory]
    [ClassData(typeof(QueryCountGenerator))]
    private void All_Runners_Applicable(int count, bool createEmptyTable)
    {
        using var world = new World();

        var query = world.Query<int, string>().Stream();

        //Create an empty table by spawning and despawning a single entity
        //that matches our test Query (but is a larger Archetype)
        if (createEmptyTable)
        {
            var dead = world.Spawn().Add<int>().Add(0.25f).Add("will be removed");
            world.Despawn(dead);
        }

        for (var index = 0; index < count; index++)
        {
            Assert.Equal(index, query.Count);

            world.Spawn()
                .Add(index)
                .Add("one");
        }

        query.For((_, str) =>
        {
            Assert.Equal("one", str);
            str.write = "two";
        });

        query.Raw((ReadOnlySpan<int> integers, Span<string> strings) =>
        {
            for (var i = 0; i < count; i++)
            {
                Assert.Equal(i, integers[i]);
                Assert.Equal("two", strings[i]);
                strings[i] = "three";
            }
        });

        query.Job((_, str) =>
        {
            Assert.Equal("three", str);
            str.write = "four";
        });

        query.Job((_, str) =>
        {
            Assert.Equal("four", str); 
            str.write = "five";
        });

        query.Job(6,
            (uniform, _, str) =>
            {
                Assert.Equal("five", str);
                str.write = uniform.ToString();
            });


        query.For(7,
            (uniform,_, str) =>
            {
                Assert.Equal(6.ToString(), str);
                str.write = uniform.ToString();
            });

        query.Raw(8, (int uniform, Span<int> _, Span<string> strings) =>
        {
            for (var i = 0; i < count; i++)
            {
                Assert.Equal(7.ToString(), strings[i]);
                strings[i] = uniform.ToString();
            }
        });

        query.Raw(9, (int uniform, Span<int> _, Span<string> strings) =>
        {
            for (var i = 0; i < count; i++)
            {
                Assert.Equal(8.ToString(), strings[i]);
                strings[i] = uniform.ToString();
            }
        });

        query.For((_, str) => { Assert.Equal(9.ToString(), str); });
    }


    [Theory]
    [ClassData(typeof(QueryCountGenerator))]
    private void Query_Count_Accurate(int count, bool createEmptyTable)
    {
        using var world = new World();

        if (createEmptyTable)
        {
            var dead = world.Spawn().Add<int>().Add(0.25f).Add("will be removed");
            world.Despawn(dead);
        }

        List<Entity> entities = new(count);

        var query = world.Query<int, string>().Stream();
        Assert.Equal(0, query.Count);

        for (var index = 0; index < count; index++)
        {
            Assert.Equal(index, query.Count);

            entities.Add(
                world.Spawn()
                    .Add(index)
                    .Add("I'll stay")
            );
        }

        Assert.Equal(count, query.Count);

        var random = new Random(69 + count);

        for (var i = count; i > 0; i--)
        {
            Assert.Equal(i, query.Count);
            var removalIndex = random.Next(entities.Count);
            var removalEntity = entities[removalIndex];
            entities.RemoveAt(removalIndex);
            world.Despawn(removalEntity);
        }

        Assert.Equal(0, query.Count);
    }


    [Theory]
    [ClassData(typeof(QueryCountGenerator))]
    private void Query_Raw_Count_Accurate(int count, bool createEmptyTable)
    {
        using var world = new World();

        if (createEmptyTable)
        {
            var dead = world.Spawn().Add<int>().Add(0.25f).Add("will be removed");
            world.Despawn(dead);
        }

        List<Entity> entities = new(count);

        var query = world.Query<int, string>().Stream();

        query.Raw((integers, strings) =>
        {
            Assert.Equal(0, integers.Length);
            Assert.Equal(0, strings.Length);
        });

        for (var index = 0; index < count; index++)
        {
            var captured = index;
            query.Raw((integers, strings) =>
            {
                Assert.Equal(captured, integers.Length);
                Assert.Equal(captured, strings.Length);
            });

            entities.Add(
                world.Spawn()
                    .Add(index)
            );
        }

        query.Raw((integers, strings) =>
        {
            Assert.Equal(count, integers.Length);
            Assert.Equal(count, strings.Length);
        });

        var random = new Random(69 + count);

        for (var i = count; i > 0; i--)
        {
            var captured = i;
            query.Raw((integers, _) => { Assert.Equal(captured, integers.Length); });

            var removalIndex = random.Next(entities.Count);
            var removalEntity = entities[removalIndex];
            entities.RemoveAt(removalIndex);
            world.Despawn(removalEntity);
        }

        query.Raw((integers, _) => { Assert.Equal(0, integers.Length); });
    }


    [Theory]
    [ClassData(typeof(QueryCountGenerator))]
    private void Query_Run_Count_Accurate(int count, bool createEmptyTable)
    {
        using var world = new World();

        if (createEmptyTable)
        {
            var dead = world.Spawn().Add<int>().Add(0.25f).Add("will be removed");
            world.Despawn(dead);
        }

        List<Entity> entities = new(count);

        var query = world.Query<int, string>().Stream();

        query.Raw((integers, _) => { Assert.Equal(0, integers.Length); });

        for (var index = 0; index < count; index++)
        {
            var captured = index;
            query.Raw((integers, _) => { Assert.Equal(captured, integers.Length); });

            entities.Add(
                world.Spawn()
                    .Add(index)
                    .Add("I'll stay")
            );
        }

        query.Raw((integers, _) => { Assert.Equal(count, integers.Length); });

        var random = new Random(69 + count);

        for (var i = count; i > 0; i--)
        {
            var captured = i;
            query.Raw((integers, strings) =>
            {
                Assert.Equal(captured, integers.Length);
                Assert.Equal(captured, strings.Length);
            });

            var removalIndex = random.Next(entities.Count);
            var removalEntity = entities[removalIndex];
            entities.RemoveAt(removalIndex);
            world.Despawn(removalEntity);
        }

        query.Raw((integers, strings) =>
        {
            Assert.Equal(0, integers.Length);
            Assert.Equal(0, strings.Length);
        });
    }


    [Theory]
    [ClassData(typeof(QueryCountGenerator))]
    private void Query_Job_Count_Accurate(int count, bool createEmptyTable)
    {
        using var world = new World();

        if (createEmptyTable)
        {
            var dead = world.Spawn().Add<int>().Add(0.25f).Add("will be removed");
            world.Despawn(dead);
        }

        List<Entity> entities = new(count);

        var query = world.Query<int, string>().Stream();

        query.Raw((integers, strings) =>
        {
            Assert.Equal(0, integers.Length);
            Assert.Equal(0, strings.Length);
        });

        for (var index = 0; index < count; index++)
        {
            var captured = index;
            query.Raw((integers, strings) =>
            {
                Assert.Equal(captured, integers.Length);
                Assert.Equal(captured, strings.Length);
            });

            entities.Add(
                world.Spawn()
                    .Add(index)
                    .Add("I'll stay")
            );
        }

        query.Raw((integers, strings) =>
        {
            Assert.Equal(count, integers.Length);
            Assert.Equal(count, strings.Length);
        });

        var random = new Random(69 + count);

        for (var i = count; i > 0; i--)
        {
            var captured = i;
            query.Raw((integers, strings) =>
            {
                Assert.Equal(captured, integers.Length);
                Assert.Equal(captured, strings.Length);
            });

            var removalIndex = random.Next(entities.Count);
            var removalEntity = entities[removalIndex];
            entities.RemoveAt(removalIndex);
            world.Despawn(removalEntity);
        }

        query.Raw((integers, strings) =>
        {
            Assert.Equal(0, integers.Length);
            Assert.Equal(0, strings.Length);
        });
    }


    [Theory]
    [ClassData(typeof(QueryCountGenerator))]
    private void Parallel_Visits_All_Entities(int count, bool createEmptyTable)
    {
        using var world = new World();

        if (createEmptyTable)
        {
            var dead = world.Spawn().Add<int>().Add("will be removed");
            world.Despawn(dead);
        }

        for (var index = 0; index < count; index++)
            world.Spawn()
                .Add(index)
                .Add("I'll stay");

        var query = world.Query<int, string>().Stream();

        var processed = 0;
        query.Job((index, str) =>
        {
            Interlocked.Increment(ref processed);
            index.write = 123;
            Assert.Equal("I'll stay", str);
            str.write = "fools";
        });

        Assert.Equal(count, processed);

        query.Job((index, str) =>
        {
            ArgumentOutOfRangeException.ThrowIfNegative(index.read);
            Assert.Equal(123, index);
            Assert.Equal("fools", str);
        });
    }


    [Theory]
    [ClassData(typeof(QueryCountGenerator))]
    private void Job_Visits_All_Entities(int count, bool createEmptyTable)
    {
        using var world = new World();

        if (createEmptyTable)
        {
            var dead = world.Spawn().Add<int>().Add(0.25f).Add("will be removed");
            world.Despawn(dead);
        }

        for (var index = 0; index < count; index++)
            world.Spawn()
                .Add(index)
                .Add("I'll stay");

        var query = world.Query<int, string>().Stream();

        var processed = 0;
        query.Job((index, str) =>
        {
            Interlocked.Increment(ref processed);
            index.write = 123;
            Assert.Equal("I'll stay", str);
            str.write = "fools";
        });

        Assert.Equal(count, processed);

        query.Job((index, str) =>
        {
            ArgumentOutOfRangeException.ThrowIfNegative(index.read);
            Assert.Equal(123, index);
            Assert.Equal("fools", str);
        });
    }


    [Theory]
    [ClassData(typeof(QueryCountGenerator))]
    private static void Raw_Visits_All_Entities(int count, bool createEmptyTable)
    {
        using var world = new World();

        if (createEmptyTable)
        {
            var dead = world.Spawn().Add<int>().Add(0.25f).Add("will be removed");
            world.Despawn(dead);
        }

        for (var c = 0; c < count; c++)
            world.Spawn()
                .Add(c)
                .Add(0.1f);

        var query = world.Query<int, float>().Stream();

        query.Raw((ReadOnlySpan<int> integers, Span<float> floats) =>
        {
            for (var i = 0; i < count; i++)
            {
                Assert.Equal(i, integers[i]);
                Assert.Equal(0.1f, floats[i]);
                floats[i] = integers[i];
            }
        });

        query.Raw((integers, floats) =>
        {
            for (var i = 0; i < count; i++)
            {
                Assert.Equal(i, integers[i]);
                Assert.Equal(i, floats[i]);
            }
        });
    }

    [Fact]
    private void Can_Loop_With_Entity()
    {
        using var world = new World();

        var e1 = world.Spawn().Add(123).Add<string>("123");
        var e2 = world.Spawn().Add(555).Add<string>("ralf");

        var query = world.Query<int, string>().Stream();

        var found = new List<Entity>();

        query.For((e, _, _) =>
        {
            found.Add(e);
        });

        Assert.Equal(2, found.Count);
        Assert.Contains(e1, found);
        Assert.Contains(e2, found);
    }


    [Fact]
    private void Can_Loop_With_Entity_and_Uniform()
    {
        using var world = new World();

        var e1 = world.Spawn().Add(123).Add<string>("123");
        var e2 = world.Spawn().Add(555).Add<string>("ralf");

        var query = world.Query<int, string>().Stream();

        var found = new List<Entity>();

        query.For(3.1415f, (e, uniform, _,  _) =>
        {
            found.Add(e);
            Assert.Equal(3.1415f, uniform);
        });

        Assert.Equal(2, found.Count);
        Assert.Contains(e1, found);
        Assert.Contains(e2, found);
    }

    [Fact]
    public void Cannot_Run_Job_on_Wildcard_Query()
    {
        using var world = new World();
        world.Spawn().Add("jason").Add(123);

        var stream = world.Query<string, int>(Match.Any).Stream();
        Assert.Throws<InvalidOperationException>(() => stream.Job((str, _) => { output.WriteLine(str); }));

        stream = world.Query<string, int>(Match.Entity).Stream();
        Assert.Throws<InvalidOperationException>(() => stream.Job((str, _) => { output.WriteLine(str); }));

        stream = world.Query<string, int>(Match.Target).Stream();
        Assert.Throws<InvalidOperationException>(() => stream.Job((str, _) => { output.WriteLine(str); }));

        stream = world.Query<string, int>(Match.Object).Stream();
        Assert.Throws<InvalidOperationException>(() => stream.Job((str, _) => { output.WriteLine(str); }));

        stream = world.Query<string, int>(Match.Plain).Stream();
        var ran = false;
        stream.Job((str, _) =>
        {
            output.WriteLine(str);
            ran = true;
        });
        Assert.True(ran);
    }
}
