namespace fennecs.tests.Query;

// ReSharper disable ParameterOnlyUsedForPreconditionCheck.Local
// ReSharper disable once ClassNeverInstantiated.Global
public class Query1Tests
{
#if REMOVEME

    [Theory]
    [InlineData(1, 10, false)]
    [InlineData(2, 20, true)]
    [InlineData(3, 30, false)]
    // Add more test cases as needed
    public void Query_Tests(int componentCount, int entityCount, bool createEmptyTable)
    {
        using var world = new World();

        if (createEmptyTable)
        {
            var entity = world.Spawn();
            for (int i = 0; i < componentCount; i++)
            {
                entity.Add(GetComponentValue(i));
            }
            world.Despawn(entity);
        }

        var queryMethod = typeof(World).GetMethod("Query", Type.EmptyTypes);
        var genericQueryMethod = queryMethod!.MakeGenericMethod(GetComponentTypes(componentCount));
        var query = genericQueryMethod!.Invoke(world, null);

        var streamMethod = query!.GetType().GetMethod("Stream", Type.EmptyTypes);
        var stream = streamMethod!.Invoke(query, null);

        for (var i = 0; i < entityCount; i++)
        {
            var entity = world.Spawn();
            for (var j = 0; j < componentCount; j++)
            {
                entity.Add(GetComponentValue(j));
            }
        }

        Assert.Equal(entityCount, GetQueryCount(stream!));

        // Perform additional assertions and operations on the query stream
        // using reflection to invoke methods like For, Raw, Job, etc.
        // You can create helper methods to encapsulate the reflection logic
        // and make the test code more readable.
    }

    private Type[] GetComponentTypes(int count)
    {
        // Return an array of component types based on the count
        // You can use your own logic to determine the types
        // For example, you can use a dictionary or switch statement
        // to map the count to specific component types
        return [];
    }

    private object GetComponentValue(int index)
    {
        // Return a value for the component based on the index
        // You can use your own logic to generate test data
        return null!;
    }

    private int GetQueryCount(object stream)
    {
        // Use reflection to invoke the Count property on the stream
        var countProperty = stream.GetType().GetProperty("Count");
        return (int)(countProperty!.GetValue(stream) ?? throw new InvalidOperationException());
    }
    // Add more helper methods as needed for assertions and operations
}

#endif


    [Theory]
    [ClassData(typeof(QueryCountGenerator))]
    private void All_Runners_Applicable(int count, bool createEmptyTable)
    {
        using var world = new World();

        var query = world.Query<string>().Stream();

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

        query.For((ref string str) =>
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

        query.Job((ref string str) =>
        {
            Assert.Equal("three", str);
            str = "four";
        });

        query.Job((ref string str) =>
        {
            Assert.Equal("four", str);
            str = "five";
        });

        query.Job(6, (ref string str, int uniform) =>
        {
            Assert.Equal("five", str);
            str = uniform.ToString();
        });


        query.For(7, (ref string str, int uniform) =>
        {
            Assert.Equal(6.ToString(), str);
            str = uniform.ToString();
        });

        query.Raw(8, (strings, uniform) =>
        {
            for (var i = 0; i < count; i++)
            {
                Assert.Equal(7.ToString(), strings.Span[i]);
                strings.Span[i] = uniform.ToString();
            }
        });

        query.Raw(9, (c1, uniform) =>
        {
            for (var i = 0; i < count; i++)
            {
                Assert.Equal(8.ToString(), c1.Span[i]);
                c1.Span[i] = uniform.ToString();
            }
        });

        query.For((ref string str) => { Assert.Equal(9.ToString(), str); });
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

        var query = world.Query<int>().Stream();
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
    private void Query_Run_Count_Accurate(int count, bool createEmptyTable)
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
                .Add(index);

        var query = world.Query<int>().Stream();

        var processed = 0;
        query.Job((ref int index) =>
        {
            Interlocked.Increment(ref processed);
            index = 123;
        });

        Assert.Equal(count, processed);

        query.Job((ref int index) =>
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
            world.Spawn()
                .Add(index);

        var query = world.Query<int>().Stream();

        var processed = 0;
        query.Job((ref int index) =>
        {
            Interlocked.Increment(ref processed);
            index = 123;
        });

        Assert.Equal(count, processed);

        query.Job((ref int index) =>
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
            world.Spawn()
                .Add(c)
                .Add(0.0f);

        var query = world.Query<int, float>().Stream();

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
            world.Spawn()
                .Add<long>();

        var query = world.Query<long>().Stream();

        var processed = 0;

        query.Raw(longs =>
        {
            foreach (ref var i in longs.Span) i = processed++;
        });

        Assert.Equal(count, processed);

        query.Raw(longs =>
        {
            Assert.Equal(count, longs.Length);
            for (var i = 0; i < count; i++) Assert.Equal(i, longs.Span[i]);
        });

        var index = 0;
        query.For((ref long value) => { Assert.Equal(index++, value); });

        var index2 = 0;
        query.For(1337, (ref long value, int _) => { Assert.Equal(index2++, value); });
    }

    [Fact]
    private void Can_Loop_With_Entity()
    {
        using var world = new World();

        var e1 = world.Spawn().Add(123);
        var e2 = world.Spawn().Add(555);

        var query = world.Query<int>().Stream();

        var found = new List<Entity>();

        query.For((Entity e, ref int _) =>
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

        var e1 = world.Spawn().Add(123);
        var e2 = world.Spawn().Add(555);

        var query = world.Query<int>().Stream();

        var found = new List<Entity>();

        query.For(3.1415f, (Entity e, ref int _, float uniform) =>
        {
            found.Add(e);
            Assert.Equal(3.1415f, uniform);
        });

        Assert.Equal(2, found.Count);
        Assert.Contains(e1, found);
        Assert.Contains(e2, found);
    }

    [Fact]
    private void Can_Warmup()
    {
        using var world = new World();
        var stream = world.Query<int>().Stream();
        stream.Query.Warmup();
    }
}