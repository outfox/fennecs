// ReSharper disable RedundantUsingDirective
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Environments;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Running;

using fennecs;
using fennecs.run;

using var world = new World();

var stream = world.Query<Comp1, Comp2, Comp3>().Stream();

SpawnEntities(1_000_000);

WarmUp();

//BenchmarkJob();
BenchmarkRaw();
BenchmarkFuture();

return;


void BenchmarkRaw()
{
    stream.Mem(static (comp1, comp2, comp3) =>
    {
        var m1 = comp1.Memory.Span;
        var m2 = comp2.ReadOnlyMemory.Span;
        var m3 = comp3.ReadOnlyMemory.Span;
        for (var i = 0; i < m1.Length; i++)
        {
            m1[i].Value = m1[i].Value + m2[i].Value + m3[i].Value;
        }
    });
}

void BenchmarkFuture()
{
    stream.Raw( (Span<Comp1> m1, ReadOnlySpan<Comp2> m2, ReadOnlySpan<Comp3> m3) =>
    {
        for (var i = 0; i < m1.Length; i++)
        {
            m1[i].Value = m1[i].Value + m2[i].Value + m3[i].Value;
        }
    });
}


void BenchmarkJob()
{
    stream.Job(static (comp1, comp2, comp3) =>
    {
        comp1.write.Value += comp2.read.Value + comp3.read.Value;
    });
}



void SpawnEntities(int count)
{
    for (var i = 0; i < count; i++)
        world.Spawn().Add<Comp1>()
            .Add(new Comp2 {Value = i})
            .Add(new Comp3 {Value = i/3});
}

void WarmUp()
{
    stream.Raw(static (Span<Comp1> comp1, Span<Comp2> comp2, Span<Comp3> comp3) =>
    {
        var m1 = comp1;
        var m2 = comp2;
        var m3 = comp3;
        for (var i = 0; i < m1.Length; i++)
        {
            m1[i].Value = m1[i].Value + m2[i].Value + m3[i].Value;
        }
    });
}


record struct Comp1(int Value);
record struct Comp2(int Value);
record struct Comp3(int Value);
/*
var config = ManualConfig
    .Create(DefaultConfig.Instance)
    .WithOptions(ConfigOptions.JoinSummary)
    .HideColumns("Job", "Error", "Median", "RatioSD");

var jobs = new List<Job>([
    Job.ShortRun.WithId("RyuJIT").WithRuntime(CoreRuntime.Core90), 
    //Job.ShortRun.WithId("Native").WithRuntime(NativeAotRuntime.Net90), 
]);

foreach (var job in jobs) config.AddJob(job);

BenchmarkRunner.Run<DirectStorageMemoryAccess>(config);
*/