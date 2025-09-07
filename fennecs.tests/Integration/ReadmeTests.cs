using System.Numerics;
// ReSharper disable UnusedVariable

namespace fennecs.tests.Integration;

public class ReadmeTests
{
    // Declare a component record. (we can also use most existing value & reference types)
    private record struct Velocity(Vector3 Value);
    private const float DeltaTime = 0.01f;

    [Fact]
    public void QuickStart_Example_Works()
    {
        // Create a world. (fyi, World implements IDisposable)
        var world = new World();

        // Spawn an entity into the world with a choice of components. (or add/remove them later)
        var entity = world.Spawn().Add<Velocity>();

        // Queries are cached, just build them right where you want to use them.
        var stream = world.Query<Velocity>().Stream();

        // Run code on all entities in the query. (exchange 'For' with 'Job' for parallel processing)
        stream.For(
            uniform: DeltaTime * 9.81f * Vector3.UnitZ,
            static (uniform, ref velocity) =>
            {
                velocity.Value -= uniform;
            }
        );
        
        Assert.Equal(-1 * DeltaTime * 9.81f * Vector3.UnitZ, entity.Ref<Velocity>().Value);
    }
}
