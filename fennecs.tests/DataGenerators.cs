using System.Collections;

namespace fennecs.tests;

public class QueryCountGenerator : IEnumerable<object[]>
{
    public IEnumerator<object[]> GetEnumerator()
    {
        // base induction
        for (var i = 0; i <= 8; i++) yield return [i, i % 2 == 0];

        // common powers of 2
        for (var i = 4; i <= 12; i++) yield return [(int) Math.Pow(2, i), i % 2 == 0];

        yield return [151, true]; // prime number
        yield return [6_197, false]; // prime number
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}

public class QueryChunkGenerator : IEnumerable<object[]>
{
    // There were issues with confusing storage.Length and table.Count.
    // This generator helps to call me out when that happens again.
    public IEnumerator<object[]> GetEnumerator()
    {
        // base induction / interleaving / degenerate cases
        for (var i = 0; i <= 10; i++)
        {
            for (var j = 1; j <= 10; j++)
            {
                yield return [i, j, i % 2 == 0];
            }
        }
        
        yield return [100, 10, true]; //fits
        yield return [100, 1_000, false]; //undersized
        yield return [1_000, 1_000, true]; //exact

        yield return [15_383, 1024, true]; //typical
        yield return [69_420, 4096, false]; //typical
        //yield return [214_363, 4096, true]; //typical

        yield return [433, 149, false]; // prime numbers
        yield return [149, 433, true]; // prime numbers
        //yield return [151_189, 13_441, true]; // prime numbers
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}