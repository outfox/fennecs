using System.Runtime.CompilerServices;
using fennecs.CRUD;

namespace fennecs.tests;

public class EntityTests(ITestOutputHelper output)
{
    [Fact]
    public void Can_Relate_to_Entity()
    {
        using var world = new World();
        var entity = world.Spawn();
        var target = world.Spawn();
        var wrong = world.Spawn();
        entity.Add<int>(target);
        
        Assert.True(entity.Has<int>(target));
        Assert.False(entity.Has<int>(wrong));
        Assert.False(entity.Has<int>());
        Assert.Equal(default, entity.Ref<int>(target).Read);
        Assert.Equal(default, entity.Ref<int>(target).Write);
   }


    [Fact]
    public void Can_Relate_to_Entity_with_Data()
    {
        using var world = new World();
        var entity = world.Spawn();
        var target = world.Spawn();
        var wrong = world.Spawn();
        entity.Add(123, target);
        
        Assert.True(entity.Has<int>(target));
        Assert.False(entity.Has<int>(wrong));
        Assert.False(entity.Has<int>());
        Assert.Equal(123, entity.Ref<int>(target).Read);
        Assert.Equal(123, entity.Ref<int>(target).Write);
    }

    
    [Fact]
    public void Entity_Can_Despawn_Itself()
    {
        using var world = new World();
        var entity = world.Spawn();
        entity.Add(123);
        entity.Add(7.0f, world.Spawn());
        entity.Link("hello");
        entity.Despawn();
        Assert.False(world.IsAlive(entity));
    }

    [Theory]
    [InlineData(69, 1, 2, 3)]
    [InlineData(2, 1, 20, 300)]
    [InlineData(3, 1000, 2000, 3000)]
    [InlineData(55, 31415, 123456, 345678)]
    [InlineData(1, 1, int.MaxValue/2, int.MaxValue)]
    public void Entity_Is_Comparable(byte world, int r1, int r2, int r3)
    {
        var entity1 = new Entity(world, r1);
        var entity2 = new Entity(world, r2);
        var entity3 = new Entity(world, r3);

        Assert.True(entity1.CompareTo(entity2) < 0);
        Assert.True(entity1.CompareTo(entity3) < 0);

        Assert.True(entity2.CompareTo(entity1) > 0);
        Assert.True(entity2.CompareTo(entity3) < 0);

        Assert.True(entity3.CompareTo(entity1) > 0);
        Assert.True(entity3.CompareTo(entity2) > 0);
        
        Assert.Equal(0, entity1.CompareTo(entity1));
        Assert.Equal(0, entity2.CompareTo(entity2));
        Assert.Equal(0, entity3.CompareTo(entity3));
    }


    [Fact]
    public void Entity_Is_Equal_Same_Id_Same_World()
    {
        var entity1 = new Entity(1, 1);
        var entity2 = new Entity(1, 1);
        var entity3 = new Entity(entity1.Value);
        
        Assert.Equal(entity1, entity2);
        Assert.Equal(entity1, entity3);

        Assert.Equal(entity2, entity1);
        Assert.Equal(entity2, entity3);
        
        Assert.Equal(entity3, entity1);
        Assert.Equal(entity3, entity2);
        
        Assert.True(entity1 == entity2);
        Assert.True(entity1 == entity3);
        
        Assert.True(entity2 == entity1);
        Assert.True(entity2 == entity3);
        
        Assert.True(entity3 == entity1);
        Assert.True(entity3 == entity2);
    }


    [Fact]
    public void Entity_Is_Distinct_Same_Id_Different_World()
    {
        var entity1 = new Entity(1, 1);
        var entity3 = new Entity(4, 1);
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

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(10)]
    [InlineData(69)]
    [InlineData(420)]
    [InlineData(10_000)]
    public void Entity_is_Equatable_to_Object(int index)
    {
        var entity1 = new Entity(1, index);
        var entity2 = new Entity(1, entity1.Index);
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
        var entity3 = new Entity(world._id, entity1.Index, entity1.Generation);
        var entity4 = new Entity(world._id, entity2.Index, entity2.Generation);
        var set = new HashSet<Entity> {entity1, entity2, entity3, entity4};
    
        Assert.Equal(2, set.Count);
        
        Assert.Contains(entity1, set);
        Assert.Contains(entity2, set);
        Assert.Contains(entity3, set);
        Assert.Contains(entity4, set);
    }

    
    [Fact]
    public void Entity_provides_Has()
    {
        using var world = new World();
        var entity = world.Spawn();
        entity.Add(123);

        Assert.True(entity.Has<int>());
        Assert.True(entity.Has<int>(default));
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
        entity.Link("hello world");

        Assert.True(entity.Has<string>("hello world"));
        Assert.True(entity.Has<string>(Match.Any));
        Assert.True(entity.Has<string>(Match.Link));
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
        entity.Link("hello world");
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

        var wrong = world.Spawn();
        Assert.False(entity.Has<int>(wrong));
        Assert.False(entity.Has<int>(Match.Link));
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
    public void Can_Read_Component_as_RW()
    {
        using var world = new World();
        var entity = world.Spawn();
        entity.Add(123);

        var component = entity.Ref<int>();
        
        Assert.Equal(123, component.Read);
    }

    
    [Fact]
    public void Can_Read_Relation_as_RW()
    {
        using var world = new World();
        var entity = world.Spawn();
        
        var other = world.Spawn();
        var third = world.Spawn();
        entity.Add(123, other);
        entity.Add(345, third);

        var component1 = entity.Ref<int>(other);
        Assert.Equal(123, component1.Read);

        var component2 = entity.Ref<int>(third);
        Assert.Equal(345, component2.Read);
    }

    
    [Fact]
    public void Can_Write_Value_Component_as_RW()
    {
        using var world = new World();
        var entity = world.Spawn();
        entity.Add(0);

        var component = entity.Ref<int>();

        component.Write = 321;
        Assert.Equal(321, entity.Ref<int>().Read);

        component.Write += 123;
        Assert.Equal(444, entity.Ref<int>().Read);
    }

    
    [Fact]
    public void Can_Write_Ref_Component_as_RW()
    {
        using var world = new World();
        var entity = world.Spawn();
        entity.Add(new List<int>());

        var component = entity.Ref<List<int>>();

        component.Write = [420];
        Assert.Equal([420], entity.Ref<List<int>>().Read);

        component.Write.Add(69);
        Assert.Equal([420, 69], entity.Ref<List<int>>().Read);
    }

    
    [Fact]
    public void Can_Write_Component_as_RW_Directly()
    {
        using var world = new World();
        var entity = world.Spawn();
        entity.Add(0);


        entity.Ref<int>().Write = 321;
        Assert.Equal(321, entity.Ref<int>().Read);

        entity.Ref<int>().Write += 123;
        Assert.Equal(444, entity.Read<int>());
    }
    
    
    [Fact]
    public void Can_Consume_Component_as_RW()
    {
        using var world = new World();
        var entity = world.Spawn();
        entity.Add(789);
        
        var component = entity.Ref<int>();
        Assert.Equal(789, component.Consume);
        Assert.False(entity.Has<int>());
    }

    
    [Fact]
    public void Can_Consume_Component_as_RW_Directly()
    {
        using var world = new World();
        var entity = world.Spawn();
        entity.Add(789);
        
        var value = entity.Ref<int>().Consume;
        Assert.Equal(789, value);
        Assert.False(entity.Has<int>());
    }

    
    [Fact]
    public void Can_Remove_Component_as_RW()
    {
        using var world = new World();
        var entity = world.Spawn();
        entity.Add(789);
        
        var component = entity.Ref<int>();
        component.Remove();
        Assert.False(entity.Has<int>());
    }

    
    
    [Fact]
    public void Can_Get_Component_as_Ref()
    {
        using var world = new World();
        var entity = world.Spawn();
        entity.Add(123);
        ref var component = ref entity.Write<int>();
        Assert.Equal(123, component);
        component = 456;
        Assert.Equal(456, entity.Read<int>());
    }

    [Fact]
    public void Cannot_Get_Missing_Component_as_RW()
    {
        using var world = new World();
        var entity = world.Spawn();
        Assert.Throws<InvalidOperationException>(() => entity.Ref<int>());
    }

    
    [Fact]
    public void Cannot_Get_Missing_Relation_as_RW()
    {
        using var world = new World();
        var entity = world.Spawn();
        entity.Add(123);

        var target = world.Spawn();
        Assert.Throws<InvalidOperationException>(() => entity.Ref<int>(target));
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
        entity.Link(helloWorld);
        var component = entity.Ref<Name>(Key.Of(helloWorld));
        Assert.Equal(helloWorld, component.Read);
    }

    [Fact]
    public void Can_Get_Relation_Backing_as_Ref()
    {
        using var world = new World();
        var entity = world.Spawn();
        var target = world.Spawn();
        entity.Add<int>(target);
        var component = entity.Ref<int>(target);
        Assert.Equal(0, component.Read);
        component.Write = 123;
        Assert.Equal(123, entity.Ref<int>(target).Read);
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
        
        entity.Link("666");
        Assert.True(interfaceEntity.Has("666"));
        
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
        entity.Link("hello");
        
        var components = entity.Components;
        Assert.Equal(4, components.Count);

        List<IStrongBox> expected  = [new StrongBox<int>(123), new StrongBox<double>(69.420), new StrongBox<TypeA>(new()), new StrongBox<string>("hello")];
        foreach (var component in components)
        {
            var found = expected.Aggregate(false, (current, box) => current | box.Value!.Equals(component.Box.Value));
            Assert.True(found, $"Component {component.Type} = {component.Box.Value} not found in expected list.");
        }
    }
    
    
    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(1337)]
    [InlineData(41697)]
    public void GottenComponentsCanBeRelations(int seed)
    {
        var random = new Random(seed);
        
        using var world = new World();
        var entity = world.Spawn();
        var other = world.Spawn();
        
        entity.Add(123, other);
        var literal = "hello" + random.Next();
        entity.Link(literal);
        
        var components = entity.Components;
        Assert.Equal(2, components.Count);
        
        Assert.True(components[0].IsRelation);
        Assert.False(components[1].IsRelation);
        Assert.True(components[1].Box.Value is string);
        Assert.Equal(literal, components[1].Box.Value);
        Assert.True(components[1].IsLink);
    }
    
    
    [Fact]
    public void GottenComponentRelationsHaveCorrectEntity()
    {
        using var world = new World();
        var entity = world.Spawn();
        var other = world.Spawn();
        
        entity.Add(123, other);
        
        var components = entity.Components;
        Assert.Single(components);
        Assert.True(components[0].IsRelation);
        Assert.Equal(other, components[0].TargetEntity);
    }

    [Fact]
    public void CannotGetEntityFromNonRelation()
    {
        using var world = new World();
        var entity = world.Spawn();
        entity.Add(123);
        
        var components = entity.Components;
        Assert.Single(components);
        Assert.False(components[0].IsRelation);
        Assert.Throws<InvalidOperationException>(() => components[0].TargetEntity);
    }

    
    [Fact]
    public void Can_Get_Link_Object_via_Get()
    {
        using var world = new World();
        var entity = world.Spawn();
        var literal = "hello world";
        Name helloWorld = new(literal);
        entity.Link(helloWorld);
        entity.Link(literal);
        var strings = entity.GetAll<Name>(Match.Any);
        Assert.Equal(helloWorld, strings[0].Item2);
        Assert.Single(strings);
    }
    
    
    [Fact]
    public void Can_Get_Link_Objects_via_Get()
    {
        using var world = new World();
        var entity = world.Spawn();
        const string literal1 = "hello world1";
        const string literal2 = "hello world2";
        Name helloWorld1 = new(literal1);
        Name helloWorld2 = new(literal2);
        entity.Link(helloWorld1);
        entity.Link(helloWorld2);
        var strings = entity.GetAll<Name>(Match.Any);
        Assert.Contains((TypeExpression.Of(helloWorld1), helloWorld1), strings);
        Assert.Contains((TypeExpression.Of(helloWorld2), helloWorld2), strings);
        Assert.Equal(2, strings.Count);
    }


    [Fact]
    public void Truthy()
    {
        using var world = new World();

        var entity = world.Spawn();
        Assert.True(entity);

        entity.Despawn();
        Assert.False(entity);
        
        entity = default;
        Assert.False(entity);
    }
    
}