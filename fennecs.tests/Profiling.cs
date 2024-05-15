using System.Collections;

namespace fennecs.tests;

using System.Numerics;
using fennecs;
/*
public class Profiling(ITestOutputHelper output)
{
    [Theory]
    [ClassData(typeof(ProfilingWorld))]
    public void Blit1K(Query<Vector3> query, Random rnd)
    {
        var uniformConstantVector = new Vector3(rnd.NextSingle(), rnd.NextSingle(), rnd.NextSingle());
        BlitTest(query, rnd, uniformConstantVector);
    }

    private void BlitTest(Query<Vector3> query, Random rnd, Vector3 uniformConstantVector)
    {
        for (var i = 0; i < 100; i++) query.Blit(uniformConstantVector);
    }
}

public class ProfilingWorld : IEnumerable<object[]>
{
    private const int EntityCount = 1000;
    public IEnumerator<object[]> GetEnumerator()
    {
        World world = new(EntityCount * 3);
        Random rnd = new(1337);

        for (var i = 0; i < EntityCount; i++)
        {
            world.Spawn().Add<Vector3>(new(rnd.NextSingle(), rnd.NextSingle(), rnd.NextSingle()));
        }

        var query = world.Query<Vector3>().Build();
        query.Warmup();

        yield return [query, rnd];
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
*/