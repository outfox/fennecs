using BenchmarkDotNet.Running;

/*
var summary = new ChunkingBenchmarks();
summary.Setup();
summary.CrossProduct_Callback();
summary.Cleanup();
*/

BenchmarkSwitcher.FromAssembly(typeof(Benchmark.Base).Assembly).Run(args);


