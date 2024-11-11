using BenchmarkDotNet.Filters;
using BenchmarkDotNet.Running;

namespace Benchmark;

/// <summary>
/// Excludes a given category from benchmarks
/// </summary>
public class CategoryExclusion(string category) : IFilter
{
    public bool Predicate(BenchmarkCase benchmarkCase)
    {
        return !benchmarkCase.Descriptor.Categories.Contains(category);
    }
}