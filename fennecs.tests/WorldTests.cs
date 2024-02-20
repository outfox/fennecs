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
    public Entity World_Spawns_valid_Entities()
    {
        using var world = new World();
        var entity = world.Spawn().Id();
        Assert.True(entity.Identity.IsEntity);
        Assert.False(entity.Identity.IsVirtual);
        Assert.False(entity.Identity.IsObject);
        return entity;
    }

    [Fact]
    public void World_Count_Accurate()
    {
        using var world = new World();
        Assert.Equal(0, world.Count);

        var e1 = world.Spawn().Id();
        Assert.Equal(1, world.Count);

        world.On(e1).Add(new { });
        Assert.Equal(1, world.Count);

        var e2 = world.Spawn().Id();
        world.On(e2).Add(new { });
        Assert.Equal(2, world.Count);
    }

    [Fact]
    public void Can_Find_Targets_of_Relation()
    {
        using var world = new World();
        var target1 = world.Spawn().Id();
        var target2 = world.Spawn().Add("hallo dieter").Id();

        world.Spawn().Link(target1, 666).Id();
        world.Spawn().Link(target2, 1.0f).Id();
        world.Spawn().Link<string>("123").Id();

        var targets = new List<Entity>();
        world.CollectTargets<int>(targets);
        Assert.Single(targets);
        Assert.Contains(target1, targets);
        targets.Clear();

        world.CollectTargets<float>(targets);
        Assert.Single(targets);
        Assert.Contains(target2, targets);
    }


    [Fact]
    public void Despawn_Target_Removes_Relation_From_Origins()
    {
        using var world = new World();
        var target1 = world.Spawn().Id();
        var target2 = world.Spawn().Id();

        for (var i = 0; i < 1000; i++)
        {
            world.Spawn().Link(target1, 666).Id();
            world.Spawn().Link(target2, 444).Id();
        }

        var query1 = world.Query<Entity>().Has<int>(target1).Build();
        var query2 = world.Query<Entity>().Has<int>(target2).Build();

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
        var entity = world.Spawn().Add<NewableClass>().Id();
        Assert.True(world.HasComponent<NewableClass>(entity));
        Assert.NotNull(world.GetComponent<NewableClass>(entity));
    }

    [Fact]
    public void Added_Newable_Struct_is_default()
    {
        using var world = new World();
        var entity = world.Spawn().Add<NewableStruct>().Id();
        Assert.True(world.HasComponent<NewableStruct>(entity));
        Assert.Equal(default, world.GetComponent<NewableStruct>(entity));
    }

    [Fact]
    public void Can_add_Non_Newable()
    {
        using var world = new World();
        var entity = world.Spawn().Add<string>("12").Id();
        Assert.True(world.HasComponent<string>(entity));
        Assert.NotNull(world.GetComponent<string>(entity));
    }



    [Fact]
    public void Adding_Component_in_Deferred_Mode_Is_Deferred()
    {
        using var world = new World();
        var entity = world.Spawn().Id();
        world.Lock();
        world.On(entity).Add(666);
        Assert.False(world.HasComponent<int>(entity));
        Assert.Throws<KeyNotFoundException>(() => world.GetComponent<int>(entity));
        world.Unlock();
        Assert.True(world.HasComponent<int>(entity));
        Assert.Equal(666, world.GetComponent<int>(entity));
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
        var entity = world.Spawn().Id();
        world.Unlock();
        Assert.True(world.IsAlive(entity));
    }

    [Fact]
    public void Can_apply_deferred_Add()
    {
        using var world = new World();
        var entity = world.Spawn().Id();
        world.Lock();
        world.On(entity).Add(666);
        Assert.False(world.HasComponent<int>(entity));
        world.Unlock();
        Assert.True(world.HasComponent<int>(entity));
        Assert.Equal(666, world.GetComponent<int>(entity));
    }

    [Fact]
    public void Can_apply_deferred_Remove()
    {
        using var world = new World();
        var entity = world.Spawn().Add(666).Id();
        world.Lock();
        world.On(entity).Remove<int>();
        world.Unlock();
        Assert.False(world.HasComponent<int>(entity));
    }

    [Fact]
    public void Can_apply_deferred_Despawn()
    {
        using var world = new World();
        var entity = world.Spawn().Add(666).Add("hallo").Id();
        world.Lock();
        world.Despawn(entity);
        Assert.True(world.IsAlive(entity));
        world.Unlock();
        Assert.False(world.IsAlive(entity));
    }

    [Fact]
    public void Can_apply_deferred_Relation()
    {
        using var world = new World();
        var entity = world.Spawn().Id();
        var target = world.Spawn().Id();
        world.Lock();
        world.On(entity).Link(target, 666);
        Assert.False(world.HasLink<int>(entity, target));
        world.Unlock();
        Assert.True(world.HasLink<int>(entity, target));
    }

    [Fact]
    public void Can_apply_deferred_Relation_Remove()
    {
        using var world = new World();
        var entity = world.Spawn().Id();
        var target = world.Spawn().Id();
        world.Lock();
        world.On(entity).Link(target, 666);
        world.On(entity).Remove<int>(target);
        Assert.False(world.HasComponent<int>(entity));
        Assert.False(world.HasComponent<int>(target));
        world.Unlock();
        Assert.False(world.HasComponent<int>(entity));
        Assert.False(world.HasComponent<int>(target));
    }

    [Fact]
    private void Can_Remove_Components_in_Reverse_Order()
    {
        using var world = new World();
        var entity = world.Spawn().Add(666).Add("hallo").Id();
        world.On(entity).Remove<int>();
        Assert.False(world.HasComponent<int>(entity));
        world.On(entity).Remove<string>();
        Assert.False(world.HasComponent<string>(entity));
    }

    [Fact]
    private void Can_Test_for_Entity_Relation_Component_Presence()
    {
        using var world = new World();
        var entity = world.Spawn().Id();
        var target = world.Spawn().Id();
        world.On(entity).Link(target, 666);
        Assert.True(world.HasLink<int>(entity, target));
    }

    [Fact]
    private void Can_Test_for_Type_Relation_Component_Presence()
    {
        using var world = new World();
        var entity = world.Spawn().Id();
        object target = new { };
        world.On(entity).Link(target);
        Assert.True(world.HasLink(entity, target));
    }

    [Fact]
    private void Can_Add_Component_with_T_new()
    {
        using var world = new World();
        var entity = world.Spawn().Id();
        world.AddComponent<NewableStruct>(entity);
        Assert.True(world.HasComponent<NewableStruct>(entity));
    }

    [Fact]
    private void Can_Remove_Component_with_Object_and_Entity_Target()
    {
        using var world = new World();
        var entity = world.Spawn().Id();
        object target = new { };
        world.On(entity).Link(target);
        world.Unlink(entity, target);
        Assert.False(world.HasLink(entity, target));
    }

    [Fact]
    private void Can_Relate_Over_Entity()
    {
        using var world = new World();
        var entity = world.Spawn().Id();
        var other = world.Spawn().Id();
        var data = new Entity(new Identity(123));
        world.On(entity).Link(other, data);
        Assert.True(world.HasLink<Entity>(entity, other));
    }

    [Fact]
    private void Cannot_Add_null_Component_Data()
    {
        using var world = new World();
        var entity = world.Spawn().Id();
        Assert.Throws<ArgumentNullException>(() => world.On(entity).Add<string>(null!));
    }

    
    
/*
    This API was retired, but might come back

    [Fact]
    private void Can_Try_Get_Component()
    {
        using var world = new World();
        var entity = world.Spawn().Add(666).Id();
        Assert.True(world.TryGetComponent<int>(entity, out var value));
        Assert.Equal(666, value);
    }

    [Fact]
    private void Can_Fail_Try_Get_Component()
    {
        using var world = new World();
        var entity = world.Spawn().Add(666.0).Id();
        Assert.Throws<NullReferenceException>(() =>
        {
            Assert.False(world.TryGetComponent<int>(entity, out var reference));
            output.WriteLine(reference.Value.ToString());
        });
    }
    [Fact]
    private void Can_Try_Get_Component_With_Target_Entity()
    {
        using var world = new World();
        var entity = world.Spawn().Id();
        var target = world.Spawn().Id();
        world.On(entity).Link(target, 666);
        Assert.True(world.TryGetComponent<int>(entity, target, out var value));
        Assert.Equal(666, value);
    }

    [Fact]
    private void Can_Fail_Try_Get_Component_With_Target_Entity()
    {
        using var world = new World();
        var entity = world.Spawn().Id();
        var target = world.Spawn().Id();
        world.On(entity).Link(target, 666.0);
        Assert.Throws<NullReferenceException>(() =>
        {
            Assert.False(world.TryGetComponent<int>(entity, target, out var reference));
            output.WriteLine(reference.Value.ToString());
        });
    }
*/
}