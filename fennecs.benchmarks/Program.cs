﻿using System.Runtime.Intrinsics.Arm;
using System.Runtime.Intrinsics.X86;
using Benchmark;
using Benchmark.Conceptual;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Environments;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Running;


var config = ManualConfig
    .Create(DefaultConfig.Instance)
    .WithOptions(ConfigOptions.JoinSummary)
    .HideColumns("Job", "Error", "Median", "RatioSD");

var jobs = new List<Job>([
    Job.ShortRun.WithId("Default").WithRuntime(CoreRuntime.Core90), 
    //Job.ShortRun.WithId("Native").WithRuntime(NativeAotRuntime.Net90),
]);


foreach (var job in jobs) config.AddJob(job);


// Most relevant vectorization instruction sets, add other intrinsics as needed.
// These are exclusions you can use to TURN OFF specific benchmarks based on the
// supported feature of the system.
if (!Avx2.IsSupported) config.AddFilter(new CategoryExclusion(nameof(Avx2)));
if (!Avx.IsSupported) config.AddFilter(new CategoryExclusion(nameof(Avx)));
if (!Sse3.IsSupported) config.AddFilter(new CategoryExclusion(nameof(Sse3)));
if (!Sse2.IsSupported) config.AddFilter(new CategoryExclusion(nameof(Sse2)));
if (!AdvSimd.IsSupported) config.AddFilter(new CategoryExclusion(nameof(AdvSimd)));


BenchmarkRunner.Run<QueryWideVsTall>(config);
