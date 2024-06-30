using System.Runtime.CompilerServices;

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
        entity.Add(7.0f, world.Spawn());
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
    public void Entity_provides_Has()
    {
        using var world = new World();
        var entity = world.Spawn();
        entity.Add(123);

        Assert.True(entity.Has<int>());
        Assert.True(entity.Has<int>(Match.Plain));
        Assert.True(entity.Has<int>(Match.Any));

        Assert.False(entity.Has<int>(Entity.Any));
        Assert.False(entity.Has<int>(Link.Any));
        Assert.False(entity.Has<int>(Match.Target));

        Assert.False(entity.Has<float>(Match.Any));
    }


    [Fact]
    public void Entity_provides_HasLink()
    {
        using var world = new World();
        var entity = world.Spawn();
        world.Spawn();
        entity.Add(Link.With("hello world"));

        Assert.True(entity.Has<string>("hello world"));
        Assert.True(entity.Has<string>(Match.Any));
        Assert.True(entity.Has<string>(Match.Object));
        Assert.True(entity.Has<string>(Link.Any));
        Assert.True(entity.Has<string>(Match.Target));

        Assert.False(entity.Has<string>("goodbye world"));
        Assert.False(entity.Has<int>(Match.Entity));
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
        Assert.True(entity.Has<int>(Match.Target));
        Assert.True(entity.Has<int>(Match.Any));

        Assert.False(entity.Has<int>(new Entity(world, new(9001))));
        Assert.False(entity.Has<int>(Match.Object));
    }


    [Fact]
    public void Entity_provides_HasRelation_overload_With_Plain_MatchExpression()
    {
        using var world = new World();
        var entity = world.Spawn();
        var target = world.Spawn();
        entity.Add<int>(target);

        Assert.False(entity.Has<int>());
        Assert.True(entity.Has<int>(Match.Entity));
        Assert.False(entity.Has<float>(Match.Entity));
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
    public void Cannot_Get_Missing_Component_as_Ref()
    {
        using var world = new World();
        var entity = world.Spawn();
        Assert.Throws<InvalidOperationException>(() => entity.Ref<int>());
    }

    // ReSharper disable once NotAccessedPositionalProperty.Local
    private record Name(string _);

    [Fact]
    public void Can_Get_Link_Object_as_Ref()
    {
        using var world = new World();
        var entity = world.Spawn();
        Name helloWorld = new("hello world");
        entity.Add(Link.With(helloWorld));
        ref var component = ref entity.Ref(Link.With(helloWorld));
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


    [Fact]
    public void Implements_IHasComponent()
    {
        using var world = new World();
        var other = world.Spawn();
        
        var entity = world.Spawn().Add(123);
        var interfaceEntity = (IHasComponent) entity;
        Assert.True(entity.Has<int>());
        
        entity.Add("123");
        Assert.True(interfaceEntity.Has<string>());
        
        entity.Add(Link.With("666"));
        Assert.True(interfaceEntity.Has(Link.With("666")));
        
        Assert.False(interfaceEntity.Has<int>(other));
        Assert.False(interfaceEntity.Has<string>(other));

        entity.Add(123, other);
        Assert.True(interfaceEntity.Has<int>(other));
        Assert.False(interfaceEntity.Has<string>(other));
        
        entity.Add("123", other);
        Assert.True(interfaceEntity.Has<int>(other));
        Assert.True(interfaceEntity.Has<string>(other));
    }


    private struct TypeA;
    
    [Fact]
    public void CanGetComponents()
    {
        using var world = new World();
        var entity = world.Spawn();
        entity.Add(123);
        entity.Add(69.420);
        entity.Add(new TypeA());
        entity.Add(Link.With("hello"));
        
        
        var components = entity.Components;
        Assert.Equal(4, components.Length);

        List<IStrongBox> expected  = [new StrongBox<int>(123), new StrongBox<double>(69.420), new StrongBox<TypeA>(new()), new StrongBox<string>("hello")];
        foreach (var component in components)
        {
            var found = expected.Aggregate(false, (current, box) => current | box.Value!.Equals(component.Box.Value));
            Assert.True(found, $"Component {component.Type} = {component.Box.Value} not found in expected list.");
        }
    }
}