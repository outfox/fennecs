using System.Numerics;
using System.Threading.Channels;
using BenchmarkDotNet.Attributes;
using fennecs;

namespace Benchmark.Conceptual;

struct WorkLoad : IAsyncEnumerable<bool>
{
    public async IAsyncEnumerator<bool> GetAsyncEnumerator(CancellationToken cancellationToken = new CancellationToken())
    {
        await Task.Yield();
        yield return true;
    }
    
}

[ShortRunJob]
public class AsyncStreams
{
    private Channel<Work<Vector3, Vector3>> _channel = null!;

    [Params(1_000, 1_000_000)]
    public int entityCount { get; set; }
    
    [GlobalSetup]
    public void Setup()
    {
        _channel = Channel.CreateUnbounded<Work<Vector3, Vector3>>();
    }
    
    [Benchmark]
    public async Task ProduceAndConsume()
    {
        for (var i = 0; i < entityCount; i++)
        {
            await _channel.Writer.WriteAsync(new Work<Vector3, Vector3>());
        }
        //_channel.Writer.Complete();

        await foreach (var work in _channel.Reader.ReadAllAsync())
        {
            work.Execute();
        }
    }
}