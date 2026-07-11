// SPDX-License-Identifier: MIT

namespace fennecs.tests;

// Runs serially: draining the process-wide World tag pool would starve
// any tests running in parallel with this one.
[CollectionDefinition("WorldTagPool", DisableParallelization = true)]
public class WorldTagPoolCollection;


[Collection("WorldTagPool")]
public class WorldTagExhaustionTests
{
    [Fact]
    public void Too_Many_Concurrent_Worlds_Throws()
    {
        var worlds = new List<World>();
        try
        {
            var ex = Assert.Throws<InvalidOperationException>(() =>
            {
                // 300 > the 255 tag slots; throws once the pool runs dry.
                for (var i = 0; i < 300; i++) worlds.Add(new World(0));
            });
            Assert.Contains("255", ex.Message);
        }
        finally
        {
            foreach (var world in worlds) world.Dispose();
        }

        // Tags were recycled; Worlds can be created again.
        using var revived = new World(0);
        Assert.True(revived.Spawn().Alive);
    }
}


public class TypeIdExhaustionTests
{
    [Fact]
    public void TypeId_Space_Exhaustion_Throws()
    {
        var ex = RegistryProbe.ProvokeExhaustion();
        var inner = Assert.IsType<InvalidOperationException>(ex.InnerException);
        Assert.Contains("TypeID space exhausted", inner.Message);
    }


    // Grants test access to the protected registry internals of LanguageType.
    private class RegistryProbe : LanguageType
    {
        internal static TypeInitializationException ProvokeExhaustion()
        {
            lock (RegistryLock)
            {
                var saved = Counter;
                try
                {
                    Counter = MaxTypeId;
                    // First-ever touch of the canary type runs its static
                    // constructor, which sees the exhausted counter and throws.
                    return Assert.Throws<TypeInitializationException>(() => _ = LanguageType<ExhaustionCanary>.Id);
                }
                finally
                {
                    Counter = saved;
                }
            }
        }
    }


    // Used ONLY by ProvokeExhaustion: its LanguageType registration is
    // permanently poisoned by the provoked throw.
    private struct ExhaustionCanary;
}
