using BenchmarkDotNet.Attributes;

namespace Benchmark.Conceptual;

[ShortRunJob]
public class Fibonacci
{
    [Params(1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 20, 30, 40)]
    // ReSharper disable once UnusedAutoPropertyAccessor.Global
    public long sequence { get; set; }

    [Benchmark]
    public long Recursive()
    {
        return FibonacciRecursive(sequence);
    }

    private static long FibonacciRecursive(long n)
    {
        if (n <= 1)
            return n;
        return FibonacciRecursive(n - 1) + FibonacciRecursive(n - 2);
    }

    [Benchmark]
    public long Iterative()
    {
        return FibonacciIterative(sequence);
    }

    private static long FibonacciIterative(long n)
    {
        var a = 0;
        var b = 1;
        for (var i = 0; i < n; i++)
        {
            var temp = a;
            a = b;
            b = temp + b;
        }
        return a;
    }
    
    [Benchmark]
    public long RecursiveMemoized()
    {
        var memo = new long[sequence + 1];
        return FibonacciRecursiveMemoized(sequence, memo);
    }

    private static long FibonacciRecursiveMemoized(long n, long[] memo)
    {
        if (n <= 1)
            return n;
        if (memo[n] == 0)
            memo[n] = FibonacciRecursiveMemoized(n - 1, memo) + FibonacciRecursiveMemoized(n - 2, memo);
        return memo[n];
    }
    
    [Benchmark]
    public long IterativeMemoized()
    {
        return FibonacciIterativeArray(sequence);
    }
    
    private static long FibonacciIterativeArray(long n)
    {
        var memo = new long[n + 1];
        memo[0] = 0;
        memo[1] = 1;
        for (var i = 2; i <= n; i++)
        {
            memo[i] = memo[i - 1] + memo[i - 2];
        }
        return memo[n];
    }
}