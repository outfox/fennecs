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

    //[Fact]
    // Test for https://github.com/outfox/fennecs/issues/41
    private void DespawnWithRelationDespawnsAllEntities()
    {
        var world = new World();
        var entities = new List<Entity>();
        
        var parent = world.Spawn();
        parent.Add<TagForDespawn>();
        parent.Add(-1);
        entities.Add(parent);

        for (var i = 0; i < 100; i++) {
            var entity = world.Spawn();
            entity.Add(i);
            
            entities.Add(entity);
            
            if (i % 5 == 2) {
                entity.Add<RelationComponent>(parent);
            }

            entity.Add<ComponentA>();

            if (i % 3 == 0) {
                entity.Add<TagForDespawn>();
            }

            parent = entity;
        }

        world.Query<int>().Compile().Despawn();
        
        foreach (var entity in entities) Assert.False(entity.Alive, $"Entity {entity} should be despawned");
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
