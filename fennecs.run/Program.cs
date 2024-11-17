// ReSharper disable RedundantUsingDirective
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Environments;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Running;

using fennecs;
using fennecs.run;

using var world = new World();

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