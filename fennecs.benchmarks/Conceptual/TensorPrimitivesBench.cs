// Add a <PackageReference Include="System.Numerics.Tensors" Version="9.0.0" /> to the csproj.
// dotnet run -c Release -f net9.0 --filter "*"

using System.Numerics.Tensors;
using BenchmarkDotNet.Attributes;

namespace Benchmark.Conceptual;

public class TensorPrimitivesBench
{
    private float[] _source = null!, _destination = null!;

    [GlobalSetup]
    public void Setup()
    {
        var r = new Random(42);
        _source = Enumerable.Range(0, 1024).Select(_ => (float)r.NextSingle()).ToArray();
        _destination = new float[1024];
    }

    [Benchmark(Baseline = true)]
    public void ManualLoop()
    {
        ReadOnlySpan<float> source = _source;
        Span<float> destination = _destination;
        for (int i = 0; i < source.Length; i++)
        {
            destination[i] = MathF.PI + source[i];
        }
    }

    [Benchmark]
    public void UnrolledLoop()
    {
        Span<float> source = _source.AsSpan();
        Span<float> destination = _destination.AsSpan();
        for (int i = 0; i < source.Length; i+=8)
        {
            destination[i] = MathF.PI + source[i];
            destination[i+1] = MathF.PI + source[i+1];
            destination[i+2] = MathF.PI + source[i+2];
            destination[i+3] = MathF.PI + source[i+3];
            destination[i+4] = MathF.PI + source[i+4];
            destination[i+5] = MathF.PI + source[i+5];
            destination[i+6] = MathF.PI + source[i+6];
            destination[i+7] = MathF.PI + source[i+7];
        }
    }

    [Benchmark]
    public void BuiltIn()
    {
        //TensorPrimitives.Cosh(_source, _destination);
        TensorPrimitives.Multiply(_source, MathF.PI, _destination);
    }
}