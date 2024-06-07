namespace fennecs.tests;

public class EntityTests(ITestOutputHelper output)
{
    [Fact]
    public void Can_Relate_to_Entity()
    {
        using var world = new World();
        var entity = world.Spawn();
        var target = world.Spawn();
        var builder = new Entity(world, entity);
        builder.Add<int>(target);
        Assert.True(entity.Has<int>(target));
        Assert.False(entity.Has<int>(new Entity(world, new Identity(9001))));
    }


    [Fact]
    public void Can_Relate_to_Entity_with_Data()
    {
        using var world = new World();
        var entity = world.Spawn();
        var target = world.Spawn();
        var builder = new Entity(world, entity);
        builder.Add(123, target);
        Assert.True(entity.Has<int>(target));
        Assert.False(entity.Has<int>(new Entity(world, new Identity(9001))));
    }


    [Fact]
    public void Entity_has_ToString()
    {
        using var world = new World();
        var entity = world.Spawn();
        var builder = new Entity(world, entity.Id);
        Assert.Equal(entity.ToString(), builder.ToString());

        entity.Add(123);
        entity.Add(7.0f, Relate.To(world.Spawn()));
        entity.Add(Link.With("hello"));
        output.WriteLine(entity.ToString());
        
        world.Despawn(entity);
        output.WriteLine(entity.ToString());
    }


    [Fact]
    public void Entity_Can_Despawn_Itself()
    {
        using var world = new World();
        var entity = world.Spawn();
        entity.Add(123);
        entity.Add(7.0f, world.Spawn());
        entity.Add(Link.With("hello"));
        entity.Despawn();
        Assert.False(world.IsAlive(entity));
    }

    [Fact]
    public void Entity_Is_Comparable()
    {
        using var world = new World();
        var entity1 = new Entity(null!, new Identity(1));
        var entity2 = new Entity(null!, new Identity(2));
        var entity3 = new Entity(null!, new Identity(3));

        Assert.True(entity1.CompareTo(entity2) < 0);
        Assert.True(entity2.CompareTo(entity3) < 0);
        Assert.True(entity1.CompareTo(entity3) < 0);
    }


    [Fact]
    public void Entity_Is_Equal_Same_Id_Same_World()
    {
        using var world = new World();
        var entity1 = world.Spawn();
        var entity2 = new Entity(world, entity1.Id);
        Assert.Equal(entity1, entity2);
        Assert.True(entity1 == entity2);
        Assert.True(entity2 == entity1);
    }


    [Fact]
    public void Entity_Is_Distinct_Same_Id_Different_World()
    {
        using var world = new World();
        var entity1 = world.Spawn();
        var entity3 = new Entity(null!, entity1.Id);
        Assert.NotEqual(entity1, entity3);
        Assert.True(entity1 != entity3);
        Assert.True(entity3 != entity1);
    }


    [Fact]
    public void Entity_Is_Distinct_Different_Id_Same_World()
    {
        using var world = new World();
        var entity1 = world.Spawn();
        var entity2 = world.Spawn();
        Assert.NotEqual(entity1, entity2);
        Assert.True(entity1 != entity2);
        Assert.True(entity2 != entity1);
    }


    [Fact]
    public void Entity_Is_Distinct_Different_Id_Different_World()
    {
        using var world1 = new World();
        using var world2 = new World();
        var entity1 = world2.Spawn();
        var entity2 = new Entity(null!, new Identity(2));
        Assert.NotEqual(entity1, entity2);
        Assert.True(entity1 != entity2);
        Assert.True(entity2 != entity1);
    }


    [Fact]
    public void Entity_is_Equatable_to_Object()
    {
        using var world = new World();
        var entity1 = world.Spawn();
        var entity2 = new Entity(world, entity1.Id);
        Assert.True(entity1.Equals(entity2));
        Assert.True(entity1.Equals((object) entity2));
        // ReSharper disable once SuspiciousTypeConversion.Global
        Assert.False(entity1.Equals("can't touch this"));
    }


    [Fact]
    public void Entity_Is_Hashable()
    {
        using var world = new World();
        var entity1 = world.Spawn();
        var entity2 = world.Spawn();
        var entity3 = new Entity(world, entity1.Id);
        var entity4 = new Entity(world, entity2.Id);
        var set = new HashSet<Entity> {entity1, entity2, entity3, entity4};
        Assert.Equal(2, set.Count);
    }


    [Fact]
    public void Entity_Decays_to_Identity()
    {
        using var world = new World();
        var entity = world.Spawn();
        Identity identity = entity;
        Assert.Equal(entity.Id, identity);
    }


    [Fact]
    public void Entity_is_Disposable()
    {
        using var world = new World();
        var builder = world.Spawn();
        Assert.IsAssignableFrom<IDisposable>(builder);
        builder.Dispose();
    }


    [Fact]
    public void Entity_provides_Has()
    {
        using var world = new World();
        var entity = world.Spawn();
        entity.Add(123);

        Assert.True(entity.Has<int>());
        Assert.True(entity.Has<int>(Identity.Plain));
        Assert.True(entity.Has<int>(Identity.Any));

        Assert.False(entity.Has<int>(Identity.Entity));
        Assert.False(entity.Has<int>(Identity.Object));
        Assert.False(entity.Has<int>(Identity.Match));

        Assert.False(entity.Has<float>(Identity.Any));
    }


    [Fact]
    public void Entity_provides_HasLink()
    {
        using var world = new World();
        var entity = world.Spawn();
        world.Spawn();
        entity.Add(Link.With("hello world"));

        Assert.True(entity.Has<string>("hello world"));
        Assert.True(entity.Has<string>(Identity.Any));
        Assert.True(entity.Has<string>(Identity.Object));
        Assert.True(entity.Has<string>(Identity.Match));

        Assert.False(entity.Has<string>("goodbye world"));
        Assert.False(entity.Has<int>(Identity.Entity));
    }


    [Fact]
    public void Entity_provides_Has_overload_With_Plain_MatchExpression()
    {
        using var world = new World();
        var entity = world.Spawn();
        world.Spawn();
        entity.Add(Link.With("hello world"));
        entity.Add("bellum gallicum");

        Assert.True(entity.Has<string>("hello world"));
        Assert.True(entity.Has<string>());
        Assert.False(entity.Has<EntityTests>());
    }


    [Fact]
    public void Entity_provides_HasRelation()
    {
        using var world = new World();
        var entity = world.Spawn();
        var target = world.Spawn();
        entity.Add<int>(target);

        Assert.True(entity.Has<int>(target));
        Assert.True(entity.Has<int>(Identity.Match));
        Assert.True(entity.Has<int>(Identity.Any));

        Assert.False(entity.Has<int>(new Entity(world, new Identity(9001))));
        Assert.False(entity.Has<int>(Identity.Object));
    }


    [Fact]
    public void Entity_provides_HasRelation_overload_With_Plain_MatchExpression()
    {
        using var world = new World();
        var entity = world.Spawn();
        var target = world.Spawn();
        entity.Add<int>(target);

        Assert.False(entity.Has<int>());
        Assert.True(entity.Has<int>(Identity.Entity));
        Assert.False(entity.Has<float>(Identity.Entity));
    }


    [Fact]
    public void Can_Get_Component_as_Ref()
    {
        using var world = new World();
        var entity = world.Spawn();
        entity.Add(123);
        ref var component = ref entity.Ref<int>();
        Assert.Equal(123, component);
        component = 456;
        Assert.Equal(456, entity.Ref<int>());
    }


    [Fact]
    public void Can_Get_Link_Object_as_Ref()
    {
        using var world = new World();
        var entity = world.Spawn();
        const string helloWorld = "hello world";
        entity.Add(Link.With(helloWorld));
        ref var component = ref entity.Ref<string>(Link.With(helloWorld));
        Assert.Equal(helloWorld, component);
    }


    [Fact]
    public void Can_Get_Relation_Backing_as_Ref()
    {
        using var world = new World();
        var entity = world.Spawn();
        var target = world.Spawn();
        entity.Add<int>(target);
        ref var component = ref entity.Ref<int>(target);
        Assert.Equal(0, component);
        component = 123;
        Assert.Equal(123, entity.Ref<int>(target));
    }
}