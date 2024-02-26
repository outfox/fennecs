namespace fennecs.tests.Query;

// ReSharper disable ParameterOnlyUsedForPreconditionCheck.Local
// ReSharper disable once ClassNeverInstantiated.Global
public class Query1Tests
{
    [Theory]
    [ClassData(typeof(QueryCountGenerator))]
    private void All_Runners_Applicable(int count, bool createEmptyTable)
    {
        using var world = new World();

        var query = world.Query<string>().Build();

        //Create an empty table by spawning and despawning a single Entity
        //that matches our test Query (but is a larger Archetype)
        if (createEmptyTable)
        {
            var dead = world.Spawn().Add<int>().Add("will be removed");
            world.Despawn(dead);
        }

        for (var index = 0; index < count; index++)
        {
            Assert.Equal(index, query.Count);

            world.Spawn()
                .Add("one");
        }

        query.ForEach((ref string str) =>
        {
            Assert.Equal("one", str);
            str = "two";
        });

        query.Raw(strings =>
        {
            for (var i = 0; i < count; i++)
            {
                Assert.Equal("two", strings.Span[i]);
                strings.Span[i] = "three";
            }
        });

        query.Parallel((ref string str) =>
        {
            Assert.Equal("three", str);
            str = "four";
        });

        query.Parallel((ref string str) =>
        {
            Assert.Equal("four", str);
            str = "five";
        }, 4096);

        query.Parallel((ref string str, int uniform) =>
        {
            Assert.Equal("five", str);
            str = uniform.ToString();
        }, 6, 4096);


        query.ForEach((ref string str, int uniform) =>
        {
            Assert.Equal(6.ToString(), str);
            str = uniform.ToString();
        }, 7);

        query.Raw((strings, uniform) =>
        {
            for (var i = 0; i < count; i++)
            {
                Assert.Equal(7.ToString(), strings.Span[i]);
                strings.Span[i] = uniform.ToString();
            }
        }, 8);

        query.Raw((c1, uniform) =>
        {
            for (var i = 0; i < count; i++)
            {
                Assert.Equal(8.ToString(), c1.Span[i]);
                c1.Span[i] = uniform.ToString();
            }
        }, 9);

        query.ForEach((ref string str) => { Assert.Equal(9.ToString(), str); });
    }


    [Theory]
    [ClassData(typeof(QueryCountGenerator))]
    private void Query_Count_Accurate(int count, bool createEmptyTable)
    {
        using var world = new World();

        if (createEmptyTable)
        {
            var dead = world.Spawn().Add<int>().Add("will be removed");
            world.Despawn(dead);
        }

        List<Entity> entities = new(count);

        var query = world.Query<int>().Build();
        Assert.Equal(0, query.Count);

        for (var index = 0; index < count; index++)
        {
            Assert.Equal(index, query.Count);

            entities.Add(
                world.Spawn()
                    .Add(index)
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
            var dead = world.Spawn().Add<int>().Add("will be removed");
            world.Despawn(dead);
        }

        List<Entity> entities = new(count);

        var query = world.Query<int>().Build();

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
    private void Query_Run_Count_Accurate(int count, bool createEmptyTable)
    {
        using var world = new World();

        if (createEmptyTable)
        {
            var dead = world.Spawn().Add<int>().Add("will be removed");
            world.Despawn(dead);
        }

        List<Entity> entities = new(count);

        var query = world.Query<int>().Build();

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
    private void Query_Job_Count_Accurate(int count, bool createEmptyTable)
    {
        using var world = new World();

        if (createEmptyTable)
        {
            var dead = world.Spawn().Add<int>().Add("will be removed");
            world.Despawn(dead);
        }

        List<Entity> entities = new(count);

        var query = world.Query<int>().Build();

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
    [ClassData(typeof(QueryChunkGenerator))]
    private void Job_Visits_All_Entities_Chunked(int count, int chunk, bool createEmptyTable)
    {
        using var world = new World();

        if (createEmptyTable)
        {
            var dead = world.Spawn().Add<int>().Add("will be removed");
            world.Despawn(dead);
        }

        for (var index = 0; index < count; index++)
        {
            world.Spawn()
                .Add(index);
        }

        var query = world.Query<int>().Build();

        var processed = 0;
        query.Parallel((ref int index) =>
        {
            Interlocked.Increment(ref processed);
            index = 123;
        }, chunkSize: chunk);

        Assert.Equal(count, processed);

        query.Parallel((ref int index) =>
        {
            ArgumentOutOfRangeException.ThrowIfNegative(index);
            Assert.Equal(123, index);
        }, chunkSize: chunk);
    }


    [Theory]
    [ClassData(typeof(QueryChunkGenerator))]
    private void Job_Uniform_Visits_All_Entities_Chunked(int count, int chunk, bool createEmptyTable)
    {
        using var world = new World();

        if (createEmptyTable)
        {
            var dead = world.Spawn().Add<int>().Add("will be removed");
            world.Despawn(dead);
        }

        for (var index = 0; index < count; index++)
        {
            world.Spawn()
                .Add(index);
        }

        var query = world.Query<int>().Build();

        var processed = 0;
        query.Parallel((ref int index, float _) =>
        {
            Interlocked.Increment(ref processed);
            index = 123;
        }, 0, chunkSize: chunk);

        Assert.Equal(count, processed);

        query.Parallel((ref int index, float _) =>
        {
            ArgumentOutOfRangeException.ThrowIfNegative(index);
            Assert.Equal(123, index);
        }, 0, chunkSize: chunk);
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
        {
            world.Spawn()
                .Add(index);
        }

        var query = world.Query<int>().Build();

        var processed = 0;
        query.Parallel((ref int index) =>
        {
            Interlocked.Increment(ref processed);
            index = 123;
        });

        Assert.Equal(count, processed);

        query.Parallel((ref int index) =>
        {
            ArgumentOutOfRangeException.ThrowIfNegative(index);
            Assert.Equal(123, index);
        });
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
        {
            world.Spawn()
                .Add(index);
        }

        var query = world.Query<int>().Build();

        var processed = 0;
        query.Parallel((ref int index) =>
        {
            Interlocked.Increment(ref processed);
            index = 123;
        });

        Assert.Equal(count, processed);

        query.Parallel((ref int index) =>
        {
            ArgumentOutOfRangeException.ThrowIfNegative(index);
            Assert.Equal(123, index);
        });
    }


    [Theory]
    [ClassData(typeof(QueryCountGenerator))]
    private static void Raw_Visits_All_Entities(int count, bool createEmptyTable)
    {
        using var world = new World();

        if (createEmptyTable)
        {
            var dead = world.Spawn().Add<int>().Add("will be removed");
            world.Despawn(dead);
        }

        for (var c = 0; c < count; c++)
        {
            world.Spawn()
                .Add(c)
                .Add(0.0f);
        }

        var query = world.Query<int, float>().Build();

        query.Raw((integers, floats) =>
        {
            for (var i = 0; i < count; i++)
            {
                Assert.Equal(i, integers.Span[i]);
                Assert.Equal(0, floats.Span[i]);
                floats.Span[i] = integers.Span[i];
            }
        });

        query.Raw((integers, floats) =>
        {
            for (var i = 0; i < count; i++)
            {
                Assert.Equal(i, integers.Span[i]);
                Assert.Equal(i, floats.Span[i]);
            }
        });
    }


    [Theory]
    [ClassData(typeof(QueryCountGenerator))]
    private void Run_Visits_All_Entities_in_Order(int count, bool createEmptyTable)
    {
        using var world = new World();

        if (createEmptyTable)
        {
            var dead = world.Spawn().Add<long>().Add("will be removed");
            world.Despawn(dead);
        }

        for (var c = 0; c < count; c++)
        {
            world.Spawn()
                .Add<long>();
        }

        var query = world.Query<long>().Build();

        var processed = 0;

        query.Raw(longs =>
        {
            foreach (ref var i in longs.Span) i = processed++;
        });

        Assert.Equal(count, processed);

        query.Raw(longs =>
        {
            Assert.Equal(count, longs.Length);
            for (var i = 0; i < count; i++)
            {
                Assert.Equal(i, longs.Span[i]);
            }
        });

        var index = 0;
        query.ForEach((ref long value) => { Assert.Equal(index++, value); });

        var index2 = 0;
        query.ForEach((ref long value, int _) => { Assert.Equal(index2++, value); },
            1337);
    }
}