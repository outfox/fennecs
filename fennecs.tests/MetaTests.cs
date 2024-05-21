namespace fennecs.tests;

public class MetaTests
{
    [Fact]
    private void Meta_Integrity_Preserved_On_Add()
    {
        using var world = new World();

        var entity0 = world.Spawn();
        var entity1 = world.Spawn();

        ref var meta0 = ref world.GetEntityMeta(entity0);
        ref var meta1 = ref world.GetEntityMeta(entity1);

        Assert.Equal(0, meta0.Row);
        Assert.Equal(1, meta1.Row);

        entity1.Add(8.0f);
        Assert.Equal(0, meta1.Row);

        entity0.Add(18.0f);
        Assert.Equal(1, meta0.Row);
    }


    [Fact]
    private void Meta_Integrity_Preserved_On_Remove()
    {
        using var world = new World();

        var entity0 = world.Spawn();
        var entity1 = world.Spawn();

        ref var meta0 = ref world.GetEntityMeta(entity0);
        ref var meta1 = ref world.GetEntityMeta(entity1);

        entity1.Add(8.0f);
        entity0.Add(18.0f);

        entity1.Remove<float>();
        entity0.Remove<float>();

        Assert.Equal(1, meta0.Row);
        Assert.Equal(0, meta1.Row);
    }


    [Fact]
    private void Meta_Integrity_Preserved_On_Migrate_with()
    {
        using var world = new World();

        var entity0 = world.Spawn();
        var entity1 = world.Spawn();
        var entity2 = world.Spawn().Add("already there, too");

        var queryStr = world.Query<string>().Build();

        ref var meta0 = ref world.GetEntityMeta(entity0);
        ref var meta1 = ref world.GetEntityMeta(entity1);
        ref var meta2 = ref world.GetEntityMeta(entity2);

        Assert.Equal(0, meta0.Row);
        Assert.Equal(1, meta1.Row);
        Assert.Equal(0, meta2.Row);

        queryStr.Remove<string>();

        Assert.Equal(0, meta0.Row);
        Assert.Equal(1, meta1.Row);
        Assert.Equal(2, meta2.Row);
    }

    
    [Fact]
    private void Meta_Integrity_Preserved_On_Migrate_with_Swap()
    {
        using var world = new World();

        var entity0 = world.Spawn();
        var entity1 = world.Spawn();
        var entity2 = world.Spawn().Add("already there");
        var entity3 = world.Spawn().Add("already there, too");

        var query = world.Query().Not<string>().Build();
        var queryStr = world.Query<string>().Build();

        ref var meta0 = ref world.GetEntityMeta(entity0);
        ref var meta1 = ref world.GetEntityMeta(entity1);
        ref var meta2 = ref world.GetEntityMeta(entity2);
        ref var meta3 = ref world.GetEntityMeta(entity3);

        Assert.Equal(0, meta0.Row);
        Assert.Equal(1, meta1.Row);
        Assert.Equal(0, meta2.Row);
        Assert.Equal(1, meta3.Row);

        query.Add<string>("Hello, World!");

        Assert.Equal(2, meta0.Row);
        Assert.Equal(3, meta1.Row);
        Assert.Equal(0, meta2.Row);
        Assert.Equal(1, meta3.Row);

        queryStr.Remove<string>();

        Assert.Equal(2, meta0.Row);
        Assert.Equal(3, meta1.Row);
        Assert.Equal(0, meta2.Row);
        Assert.Equal(1, meta3.Row);
    }
}
