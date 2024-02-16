namespace fennecs.tests.Integration;

// ReSharper disable ParameterOnlyUsedForPreconditionCheck.Local
// ReSharper disable once ClassNeverInstantiated.Global
public class Query1Tests
{
    [Fact]
    private void Indexer_disallows_Component_Type_Entity()
    {
        using var world = new World();
        var entity = world.Spawn().Id();
        var query = world.Query<Entity>().Build();

        Assert.Throws<TypeAccessException>(() => query[entity]);
    }

    [Fact]
    private void Indexer_disallows_Dead_Entity()
    {
        using var world = new World();
        var entity = world.Spawn().Add<int>().Id();
        world.Despawn(entity);
        Assert.False(world.IsAlive(entity));

        var query = world.Query<int>().Build();
        Assert.Throws<ObjectDisposedException>(() => query[entity]);
    }

    [Fact]
    private void Indexer_gets_Mutable_Component()
    {
        using var world = new World();
        var entity = world.Spawn().Add(23).Id();
        var query = world.Query<int>().Build();

        ref var gotten = ref query.Ref(entity);
        Assert.Equal(23, gotten);

        // Entity can't be a ref (is readonly - make sure!)
        gotten = 42;
        Assert.Equal(42, query.Ref(entity));
    }

    [Theory]
    [ClassData(typeof(QueryCountGenerator))]
    private void Query_Count_Accurate(int count, bool createEmptyTable)
    {
        using var world = new World();

        if (createEmptyTable)
        {
            var dead = world.Spawn().Add<int>().Add("will be removed").Id();
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
                    .Id()
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
            var dead = world.Spawn().Add<int>().Add("will be removed").Id();
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
                    .Id()
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
            var dead = world.Spawn().Add<int>().Add("will be removed").Id();
            world.Despawn(dead);
        }

        List<Entity> entities = new(count);

        var query = world.Query<int>().Build();

        query.Run(integers => { Assert.Equal(0, integers.Length); });

        for (var index = 0; index < count; index++)
        {
            var captured = index;
            query.Raw(integers => { Assert.Equal(captured, integers.Length); });

            entities.Add(
                world.Spawn()
                    .Add(index)
                    .Id()
            );
        }

        query.Run(integers => { Assert.Equal(count, integers.Length); });

        var random = new Random(69 + count);

        for (var i = count; i > 0; i--)
        {
            var captured = i;
            query.Run(integers => { Assert.Equal(captured, integers.Length); });

            var removalIndex = random.Next(entities.Count);
            var removalEntity = entities[removalIndex];
            entities.RemoveAt(removalIndex);
            world.Despawn(removalEntity);
        }

        query.Run(integers => { Assert.Equal(0, integers.Length); });
    }


    [Theory]
    [ClassData(typeof(QueryCountGenerator))]
    private void Query_Job_Count_Accurate(int count, bool createEmptyTable)
    {
        using var world = new World();

        if (createEmptyTable)
        {
            var dead = world.Spawn().Add<int>().Add("will be removed").Id();
            world.Despawn(dead);
        }

        List<Entity> entities = new(count);

        var query = world.Query<int>().Build();

        query.Run(integers => { Assert.Equal(0, integers.Length); });

        for (var index = 0; index < count; index++)
        {
            var captured = index;
            query.Raw(integers => { Assert.Equal(captured, integers.Length); });

            entities.Add(
                world.Spawn()
                    .Add(index)
                    .Id()
            );
        }

        query.Run(integers => { Assert.Equal(count, integers.Length); });

        var random = new Random(69 + count);

        for (var i = count; i > 0; i--)
        {
            var captured = i;
            query.Run(integers => { Assert.Equal(captured, integers.Length); });

            var removalIndex = random.Next(entities.Count);
            var removalEntity = entities[removalIndex];
            entities.RemoveAt(removalIndex);
            world.Despawn(removalEntity);
        }

        query.Run(integers => { Assert.Equal(0, integers.Length); });
    }


    [Theory]
    [ClassData(typeof(QueryChunkGenerator))]
    private void Job_Visits_All_Entities_Chunked(int count, int chunk, bool createEmptyTable)
    {
        using var world = new World();

        if (createEmptyTable)
        {
            var dead = world.Spawn().Add<int>().Add("will be removed").Id();
            world.Despawn(dead);
        }

        for (var index = 0; index < count; index++)
        {
            world.Spawn()
                .Add(index)
                .Id();
        }

        var query = world.Query<int>().Build();

        var processed = 0;
        query.Job((ref int index) =>
        {
            Interlocked.Increment(ref processed);
            index = 123;
        }, chunkSize: chunk);

        Assert.Equal(count, processed);

        query.Job((ref int index) =>
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
            var dead = world.Spawn().Add<int>().Add("will be removed").Id();
            world.Despawn(dead);
        }

        for (var index = 0; index < count; index++)
        {
            world.Spawn()
                .Add(index)
                .Id();
        }

        var query = world.Query<int>().Build();

        var processed = 0;
        query.Job((ref int index, float _) =>
        {
            Interlocked.Increment(ref processed);
            index = 123;
        }, 0, chunkSize: chunk);

        Assert.Equal(count, processed);

        query.Job((ref int index, float _) =>
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
            var dead = world.Spawn().Add<int>().Add("will be removed").Id();
            world.Despawn(dead);
        }

        for (var index = 0; index < count; index++)
        {
            world.Spawn()
                .Add(index)
                .Id();
        }

        var query = world.Query<int>().Build();

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
            var dead = world.Spawn().Add<int>().Add("will be removed").Id();
            world.Despawn(dead);
        }
        
        for (var index = 0; index < count; index++)
        {
            world.Spawn()
                .Add(index)
                .Id();
        }

        var query = world.Query<int>().Build();

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
            var dead = world.Spawn().Add<int>().Add("will be removed").Id();
            world.Despawn(dead);
        }
        
        for (var c = 0; c < count; c++)
        {
            world.Spawn()
                .Add(c)
                .Add(0.0f)
                .Id();
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
            var dead = world.Spawn().Add<long>().Add("will be removed").Id();
            world.Despawn(dead);
        }

        for (var c = 0; c < count; c++)
        {
            world.Spawn()
                .Add<long>()
                .Id();
        }

        var query = world.Query<long>().Build();

        var processed = 0;

        query.Run(longs =>
        {
            foreach (ref var i in longs) i = processed++;
        });

        Assert.Equal(count, processed);

        query.Run(longs =>
        {
            Assert.Equal(count, longs.Length);
            for (var i = 0; i < count; i++)
            {
                Assert.Equal(i, longs[i]);
            }
        });

        var index = 0;
        query.ForEach((ref long value) => { Assert.Equal(index++, value); });

        var index2 = 0;
        query.ForEach((ref long value, int _) => { Assert.Equal(index2++, value); },
            1337);
    }
}