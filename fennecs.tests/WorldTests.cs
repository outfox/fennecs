namespace fennecs.tests;

public class WorldTests
{
    [Fact]
    public World World_Creates()
    {
        var world = new World();
        Assert.NotNull(world);
        return world;
    }

    [Fact]
    public void World_Disposes()
    {
        using var world = World_Creates();
    }

    [Fact]
    public void World_Spawns_valid_Entities()
    {
        using var world = new World();
        var entity = world.Spawn();
        Assert.True(entity.Id.IsEntity);
        Assert.False(entity.Id.IsVirtual);
        Assert.False(entity.Id.IsObject);
    }

    [Fact]
    public void World_Count_Accurate()
    {
        using var world = new World();
        Assert.Equal(0, world.Count);

        var e1 = world.Spawn();
        Assert.Equal(1, world.Count);

        world.On(e1.Id).Add(new { });
        Assert.Equal(1, world.Count);

        var e2 = world.Spawn();
        world.On(e2.Id).Add(new { });
        Assert.Equal(2, world.Count);
    }

    [Fact]
    public void Can_Find_Targets_of_Relation()
    {
        using var world = new World();
        var target1 = world.Spawn();
        var target2 = world.Spawn().Add("hallo dieter");

        world.Spawn().AddRelation(target1, 666);
        world.Spawn().AddRelation(target2, 1.0f);
        world.Spawn().AddLink<string>("123");

        var targets = new List<Identity>();
        world.CollectTargets<int>(targets);
        Assert.Single(targets);
        Assert.Contains(target1.Id, targets);
        targets.Clear();

        world.CollectTargets<float>(targets);
        Assert.Single(targets);
        Assert.Contains(target2, targets);
    }


    [Fact]
    public void Despawn_Target_Removes_Relation_From_Origins()
    {
        using var world = new World();
        var target1 = world.Spawn();
        var target2 = world.Spawn();

        for (var i = 0; i < 1000; i++)
        {
            world.Spawn().AddRelation(target1, 666);
            world.Spawn().AddRelation(target2, 444);
        }

        var query1 = world.Query<Identity>().Has<int>(target1.Id).Build();
        var query2 = world.Query<Identity>().Has<int>(target2.Id).Build();

        Assert.Equal(1000, query1.Count);
        Assert.Equal(1000, query2.Count);
        world.Despawn(target1);
        Assert.Equal(0, query1.Count);
        Assert.Equal(1000, query2.Count);
    }


    private class NewableClass;

    private struct NewableStruct;

    [Fact]
    public void Added_Newable_Class_is_not_Null()
    {
        using var world = new World();
        var identity = world.Spawn().Add<NewableClass>().Id;
        Assert.True(world.HasComponent<NewableClass>(identity));
        Assert.NotNull(world.GetComponent<NewableClass>(identity));
    }

    [Fact]
    public void Added_Newable_Struct_is_default()
    {
        using var world = new World();
        var identity = world.Spawn().Add<NewableStruct>().Id;
        Assert.True(world.HasComponent<NewableStruct>(identity));
        Assert.Equal(default, world.GetComponent<NewableStruct>(identity));
    }

    [Fact]
    public void Can_add_Non_Newable()
    {
        using var world = new World();
        var identity = world.Spawn().Add<string>("12").Id;
        Assert.True(world.HasComponent<string>(identity));
        Assert.NotNull(world.GetComponent<string>(identity));
    }



    [Fact]
    public void Adding_Component_in_Deferred_Mode_Is_Deferred()
    {
        using var world = new World();
        var identity = world.Spawn().Id;
        world.Lock();
        world.On(identity).Add(666);
        Assert.False(world.HasComponent<int>(identity));
        Assert.Throws<KeyNotFoundException>(() => world.GetComponent<int>(identity));
        world.Unlock();
        Assert.True(world.HasComponent<int>(identity));
        Assert.Equal(666, world.GetComponent<int>(identity));
    }


    [Fact]
    public void Can_Lock_and_Unlock_World()
    {
        using var world = new World();
        world.Lock();
        world.Unlock();
    }

    [Fact]
    public void Cannot_Lock_Locked_World()
    {
        using var world = new World();
        world.Lock();
        Assert.Throws<InvalidOperationException>(() => world.Lock());
    }

    [Fact]
    public void Cannot_Unlock_Unlocked_World()
    {
        using var world = new World();
        Assert.Throws<InvalidOperationException>(() => world.Unlock());
    }

    [Fact]
    public void Can_apply_deferred_Spawn()
    {
        using var world = new World();
        world.Lock();
        var identity = world.Spawn().Id;
        world.Unlock();
        Assert.True(world.IsAlive(identity));
    }

    [Fact]
    public void Can_apply_deferred_Add()
    {
        using var world = new World();
        var identity = world.Spawn().Id;
        world.Lock();
        world.On(identity).Add(666);
        Assert.False(world.HasComponent<int>(identity));
        world.Unlock();
        Assert.True(world.HasComponent<int>(identity));
        Assert.Equal(666, world.GetComponent<int>(identity));
    }

    [Fact]
    public void Can_apply_deferred_Remove()
    {
        using var world = new World();
        var identity = world.Spawn().Add(666).Id;
        world.Lock();
        world.On(identity).Remove<int>();
        world.Unlock();
        Assert.False(world.HasComponent<int>(identity));
    }

    [Fact]
    public void Can_apply_deferred_Despawn()
    {
        using var world = new World();
        var identity = world.Spawn().Add(666).Add("hallo").Id;
        world.Lock();
        world.Despawn(identity);
        Assert.True(world.IsAlive(identity));
        world.Unlock();
        Assert.False(world.IsAlive(identity));
    }

    [Fact]
    public void Can_apply_deferred_Relation()
    {
        using var world = new World();
        var identity = world.Spawn();
        var target = world.Spawn();
        world.Lock();
        world.On(identity).AddRelation(target, 666);
        Assert.False(world.HasRelation<int>(identity, target));
        world.Unlock();
        Assert.True(world.HasRelation<int>(identity, target));
    }

    [Fact]
    public void Can_apply_deferred_Relation_Remove()
    {
        using var world = new World();
        var identity = world.Spawn();
        var target = world.Spawn();
        world.Lock();
        world.On(identity).AddRelation(target, 666);
        world.On(identity).RemoveRelation<int>(target);
        Assert.False(world.HasComponent<int>(identity));
        Assert.False(world.HasComponent<int>(target));
        world.Unlock();
        Assert.False(world.HasComponent<int>(identity));
        Assert.False(world.HasComponent<int>(target));
    }

    [Fact]
    private void Can_Remove_Components_in_Reverse_Order()
    {
        using var world = new World();
        var identity = world.Spawn().Add(666).Add("hallo");
        world.On(identity).Remove<int>();
        Assert.False(world.HasComponent<int>(identity));
        world.On(identity).Remove<string>();
        Assert.False(world.HasComponent<string>(identity));
    }

    [Fact]
    private void Can_Test_for_Entity_Relation_Component_Presence()
    {
        using var world = new World();
        var identity = world.Spawn();
        var target = world.Spawn();
        world.On(identity).AddRelation(target, 666);
        Assert.True(world.HasRelation<int>(identity, target));
    }

    [Fact]
    private void Can_Test_for_Type_Relation_Component_Presence()
    {
        using var world = new World();
        var identity = world.Spawn().Id;
        object target = new { };
        world.On(identity).AddLink(target);
        Assert.True(world.HasLink(identity, target));
    }

    [Fact]
    private void Can_Add_Component_with_T_new()
    {
        using var world = new World();
        var identity = world.Spawn().Id;
        world.AddComponent<NewableStruct>(identity);
        Assert.True(world.HasComponent<NewableStruct>(identity));
    }

    [Fact]
    private void Can_Remove_Component_with_Object_and_Entity_Target()
    {
        using var world = new World();
        var identity = world.Spawn().Id;
        object target = new { };
        world.On(identity).AddLink(target);
        world.RemoveLink(identity, target);
        Assert.False(world.HasLink(identity, target));
    }

    [Fact]
    private void Can_Relate_Over_Entity()
    {
        using var world = new World();
        var identity = world.Spawn();
        var other = world.Spawn();
        var data = new Identity(123);
        world.On(identity).AddRelation(other, data);
        Assert.True(world.HasRelation<Identity>(identity, other));
    }

    [Fact]
    private void Cannot_Add_null_Component_Data()
    {
        using var world = new World();
        var identity = world.Spawn();
        Assert.Throws<ArgumentNullException>(() => world.On(identity).Add<string>(null!));
    }

    [Fact]
    private void GetEntity_and_On_return_same_Identity()
    {
        using var world = new World();
        var entity = world.Spawn();
        Assert.Equal(entity, world.GetEntity(entity.Id));
        Assert.Equal(entity, world.On(entity.Id));
    }
}