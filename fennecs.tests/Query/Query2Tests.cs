namespace fennecs.tests.Query;

// ReSharper disable ParameterOnlyUsedForPreconditionCheck.Local
// ReSharper disable once ClassNeverInstantiated.Global
public class Query2Tests
{
    [Theory]
    [ClassData(typeof(QueryCountGenerator))]
    private void All_Runners_Applicable(int count, bool createEmptyTable)
    {
        using var world = new World();

        var query = world.Query<int, string>().Build();

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

        query.For((ref int _, ref string str) =>
        {
            Assert.Equal("one", str);
            str = "two";
        });

        query.Raw((integers, strings) =>
        {
            for (var i = 0; i < count; i++)
            {
                Assert.Equal(i, integers.Span[i]);
                Assert.Equal("two", strings.Span[i]);
                strings.Span[i] = "three";
            }
        });

        query.Job((ref int _, ref string str) =>
        {
            Assert.Equal("three", str);
            str = "four";
        });

        query.Job((ref int index, ref string str) =>
        {
            Assert.Equal(index, index);
            Assert.Equal("four", str);
            str = "five";
        }, 4096);

        query.Job((ref int index, ref string str, int uniform) =>
        {
            Assert.Equal(index, index);
            Assert.Equal("five", str);
            str = uniform.ToString();
        }, 6, 4096);


        query.For((ref int _, ref string str, int uniform) =>
        {
            Assert.Equal(6.ToString(), str);
            str = uniform.ToString();
        }, 7);

        query.Raw((_, strings, uniform) =>
        {
            for (var i = 0; i < count; i++)
            {
                Assert.Equal(7.ToString(), strings.Span[i]);
                strings.Span[i] = uniform.ToString();
            }
        }, 8);

        query.Raw((_, c1, uniform) =>
        {
            for (var i = 0; i < count; i++)
            {
                Assert.Equal(8.ToString(), c1.Span[i]);
                c1.Span[i] = uniform.ToString();
            }
        }, 9);

        query.For((ref int _, ref string str) => { Assert.Equal(9.ToString(), str); });
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

        var query = world.Query<int, string>().Build();
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

        var query = world.Query<int, string>().Build();

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

        var query = world.Query<int, string>().Build();

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

        var query = world.Query<int, string>().Build();

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
    [ClassData(typeof(QueryChunkGenerator))]
    private void Job_Visits_All_Entities_Chunked(int count, int chunk, bool createEmptyTable)
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

        var query = world.Query<int, string>().Build();

        var processed = 0;
        query.Job((ref int index, ref string str) =>
        {
            Interlocked.Increment(ref processed);
            index = 123;
            Assert.Equal("I'll stay", str);
            str = "fools";
        }, chunk);

        Assert.Equal(count, processed);

        query.Job((ref int index, ref string str) =>
        {
            ArgumentOutOfRangeException.ThrowIfNegative(index);
            Assert.Equal(123, index);
            Assert.Equal("fools", str);
        }, chunk);
    }


    [Theory]
    [ClassData(typeof(QueryChunkGenerator))]
    private void Job_Uniform_Visits_All_Entities_Chunked(int count, int chunk, bool createEmptyTable)
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

        var query = world.Query<int, string>().Build();

        var processed = 0;
        query.Job((ref int index, ref string str, float _) =>
        {
            Interlocked.Increment(ref processed);
            index = 123;
            Assert.Equal("I'll stay", str);
            str = "fools";
        }, 0, chunk);

        Assert.Equal(count, processed);

        query.Job((ref int index, ref string str, float _) =>
        {
            ArgumentOutOfRangeException.ThrowIfNegative(index);
            Assert.Equal(123, index);
            Assert.Equal("fools", str);
        }, 0, chunk);
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

        var query = world.Query<int, string>().Build();

        var processed = 0;
        query.Job((ref int index, ref string str) =>
        {
            Interlocked.Increment(ref processed);
            index = 123;
            Assert.Equal("I'll stay", str);
            str = "fools";
        });

        Assert.Equal(count, processed);

        query.Job((ref int index, ref string str) =>
        {
            ArgumentOutOfRangeException.ThrowIfNegative(index);
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

        var query = world.Query<int, string>().Build();

        var processed = 0;
        query.Job((ref int index, ref string str) =>
        {
            Interlocked.Increment(ref processed);
            index = 123;
            Assert.Equal("I'll stay", str);
            str = "fools";
        });

        Assert.Equal(count, processed);

        query.Job((ref int index, ref string str) =>
        {
            ArgumentOutOfRangeException.ThrowIfNegative(index);
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

        var query = world.Query<int, float>().Build();

        query.Raw((integers, floats) =>
        {
            for (var i = 0; i < count; i++)
            {
                Assert.Equal(i, integers.Span[i]);
                Assert.Equal(0.1f, floats.Span[i]);
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
    
    [Fact]
    private void Can_Loop_With_Entity()
    {
        using var world = new World();

        var e1 = world.Spawn().Add(123).Add<string>("123");
        var e2 = world.Spawn().Add(555).Add<string>("ralf");
        
        var query = world.Query<int, string>().Build();

        var found = new List<Entity>();
        
        query.For((Entity e, ref int _, ref string _) =>
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
        
        var query = world.Query<int, string>().Build();

        var found = new List<Entity>();
        
        query.For((Entity e, ref int _, ref string _, float uniform) =>
        {
            found.Add(e);
            Assert.Equal(3.1415f, uniform);
        }, 3.1415f);

        Assert.Equal(2, found.Count);
        Assert.Contains(e1, found);
        Assert.Contains(e2, found);
    }
}