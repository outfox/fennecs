using Schedulers;

namespace fennecs;

public partial class World
{
    /// <summary>
    /// Job Scheduler for the World.
    /// </summary>
    public readonly JobScheduler Scheduler = new(new JobScheduler.Config
    {
        ThreadPrefixName = "fennecs",
        ThreadCount = 0,
        MaxExpectedConcurrentJobs = 64,
        StrictAllocationMode = false,    
    });
}