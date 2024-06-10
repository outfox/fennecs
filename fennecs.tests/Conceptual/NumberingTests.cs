namespace fennecs.tests.Conceptual;

public class NumberingTests
{
    private static Stream<Index> Setup(int count1, int count2 = 0)
    {
        using var world = new World();

        using var _ = world.Entity()
            .Add<Index>()
            .Spawn(count1)
            .Add(true)
            .Spawn(count2);

        // This is shorthand for a stream query.
        return world.Stream<Index>();
    }

    
    // ReSharper disable once NotAccessedPositionalProperty.Local
    private record struct Index(int Value)
    {
        public static IEnumerator<Index> Ascending(int from = 0, int to = int.MaxValue)
        {
            while (from < to) yield return new(from++);
        }
    }
    
    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(1000)]
    [InlineData(99_999)]
    public void NumberingEntitiesWithEnumerator(int count)
    {
        var stream = Setup(count);
        
        stream.For(
            uniform: Index.Ascending(from: 0),
            action: static (IEnumerator<Index> enumerator, ref Index index) =>
            {
                enumerator.MoveNext();
                index = enumerator.Current;
            }
        );
        
        VerifyCountAndOrder(stream, count);
    }


    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(1000)]
    [InlineData(99_999)]
    public void NumberingEntitiesWithLINQEnumerator(int count)
    {
        var stream = Setup(count);

        // Build a range enumerator to number the entities.
        // We actually create the Index structs directly (ints are kaka-poopoo)
        using var range = Enumerable.Range(0, count).Select(i => new Index(i)).GetEnumerator();

        stream.For(
            uniform: range,
            action: static (IEnumerator<Index> enumerator, ref Index index) =>
            {
                enumerator.MoveNext();
                index = enumerator.Current;
            }
        );


        VerifyCountAndOrder(stream, count);
    }


    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(1000)]
    [InlineData(99_999)]
    public void NumberingEntitiesWithRange(int count)
    {
        var stream = Setup(count);
        
        var lazyQueue = new Queue<int>(Enumerable.Range(0, count));
        stream.For(
            uniform: lazyQueue,
            action: static (Queue<int> queue, ref Index index) =>
            {
                index.Value = queue.Dequeue();
            }
        );
        
        VerifyCountAndOrder(stream, count);
    }


    [Theory]
    [InlineData(0, 0)]
    [InlineData(1, 0)]
    [InlineData(2, 20)]
    [InlineData(1000, 500)]
    [InlineData(99_999, 12_345)]
    public void NumberingEntitiesWithQueue(int count1, int count2)
    {
        var stream = Setup(count1, count2);

        var queue = new Queue<Index>(Enumerable.Range(0, stream.Count).Select(i => new Index(i)));
        stream.For((ref Index index) => index = queue.Dequeue());

        VerifyCountAndOrder(stream, count1+count2);
    }


    [Theory]
    [InlineData(0, 0)]
    [InlineData(1, 0)]
    [InlineData(2, 20)]
    [InlineData(1000, 500)]
    [InlineData(99_999, 12_345)]
    public void NumberingEntitiesWithRawClosure(int count1, int count2)
    {
        var stream = Setup(count1, count2);

        var i = 0;
        stream.For((ref Index index) => index = new(i++));

        VerifyCountAndOrder(stream, count1+count2);
    }


    [Theory]
    [InlineData(0, 0)]
    [InlineData(1, 0)]
    [InlineData(2, 20)]
    [InlineData(1000, 500)]
    [InlineData(99_999, 12_345)]
    public void NumberingEntitiesWithRawLoop(int count1, int count2)
    {
        var stream = Setup(count1, count2);

        var index = 0;
        stream.Raw(
            action: indices =>
            {
                foreach (ref var i in indices.Span) i = new Index(index++);
            }
        );

        VerifyCountAndOrder(stream, count1+count2);
    }

    private static void VerifyCountAndOrder(Stream<Index> stream, int count)
    {
        // Check that the entities are numbered correctly.
        var accumulator = new List<Index>();
        stream.Raw(
            action: indices =>
            {
                accumulator.AddRange(indices.Span);
            }
        );
        var testRange = Enumerable.Range(0, count).Select(i => new Index(i)).ToArray();
        Assert.Equal(testRange, accumulator);
    }
}
