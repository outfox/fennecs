namespace fennecs.tests.Integration;

// ReSharper disable once ClassNeverInstantiated.Global
public class Query1Tests
{
    [Theory]
    [InlineData(1_000, 100)] //fits
    [InlineData(10_000, 10_000)] //exact
    [InlineData(15_383, 1024)] //typical
    [InlineData(214_363, 4096)] //typical
    [InlineData(151_189, 13_441)] // prime numbers
    private void Job_Visits_All_Entities_Chunked(int count, int chunk)
    {
        using var world = new World();

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
    [InlineData(1_000, 100)] //fits
    [InlineData(10_000, 10_000)] //exact
    [InlineData(15_383, 1024)] //typical
    [InlineData(214_363, 4096)] //typical
    [InlineData(151_189, 13_441)] // prime numbers
    private void Parallel_Visits_All_Entities_Chunked(int count, int chunk)
    {
        using var world = new World();

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
    [InlineData(1_000, 100)] //fits
    [InlineData(10_000, 10_000)] //exact
    [InlineData(15_383, 1024)] //typical
    [InlineData(214_363, 4096)] //typical
    [InlineData(151_189, 13_441)] // prime numbers
    private void Parallel_Uniform_Visits_All_Entities_Chunked(int count, int chunk)
    {
        using var world = new World();

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

    [Fact]
    private void Parallel_Visits_All_Entities_Chunked_Switched()
    {
        // There were issues with confusing storage.Length and table.Count.
        // This test is to call me out when that happens again.
        for (var i = 0; i < 50; i++)
        {
            for (var j = 1; j < 15; j++)
            {
                Parallel_Visits_All_Entities_Chunked(i, j);
            }
        }
    }

    [Fact]
    private void Parallel_Uniform_Visits_All_Entities_Chunked_Switched()
    {
        // There were issues with confusing storage.Length and table.Count.
        // This test is to call me out when that happens again.
        for (var i = 0; i < 50; i++)
        {
            for (var j = 1; j < 15; j++)
            {
                Parallel_Uniform_Visits_All_Entities_Chunked(i, j);
            }
        }
    }

    [Fact]
    private void Job_Visits_All_Entities_Chunked_Switched()
    {
        // There were issues with confusing storage.Length and table.Count.
        // This test is to call me out when that happens again.
        for (var i = 0; i < 50; i++)
        {
            for (var j = 1; j < 15; j++)
            {
                Job_Visits_All_Entities_Chunked(i, j);
            }
        }
    }

    [Theory]
    [InlineData(134_41)]
    [InlineData(100_000)]
    [InlineData(151_189)]
    private void Parallel_Visits_All_Entities(int count)
    {
        using var world = new World();

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
    [InlineData(134_41)]
    [InlineData(100_000)]
    [InlineData(151_189)]
    private void Job_Visits_All_Entities(int count)
    {
        using var world = new World();

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
    [InlineData(134_41)]
    [InlineData(100_000)]
    [InlineData(151_189)]
    private static void Raw_Visits_All_Entities(int count)
    {
        using var world = new World();

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
    [InlineData(1_000)] //fits
    [InlineData(10_000)] //typical
    [InlineData(100_000)] //typical
    [InlineData(151189)] // Prime numbers
    private void Run_Visits_All_Entities_in_Order(int count)
    {
        using var world = new World();

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
    }
}