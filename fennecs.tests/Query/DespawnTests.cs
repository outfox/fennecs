namespace fennecs.tests.Query;

public class DespawnTests
{
    [Fact]
    private void CanDespawnWithRelation()
    {
        var world = new World();

        var parent = world.Spawn();
        parent.Add<TagForDespawn>();

        for (var i = 0; i < 10; i++) {
            var entity = world.Spawn();
            if (i % 3 == 2) {
                entity.Add<RelationComponent>(parent);
            }

            entity.Add<ComponentA>();

            if (i % 2 == 0) {
                entity.Add<TagForDespawn>();
            }

            parent = entity;
        }

        world.Query<TagForDespawn>().Compile().Despawn();
    }


    [Fact]
    private void CanDespawnPlain()
    {
        var world = new World();

        for (var i = 0; i < 10; i++) {
            var entity = world.Spawn();
            if (i % 3 == 2) {
                entity.Add(i);
            }
            entity.Add<ComponentA>();
            if (i % 2 == 0) {
                entity.Add<TagForDespawn>();
            }
        }

        world.Query<TagForDespawn>().Compile().Despawn();
    }


    private struct TagForDespawn;

    private record struct ComponentA(
        int Value
    );

    private record struct RelationComponent(
        int Value
    );
}
