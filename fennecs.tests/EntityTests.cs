// SPDX-License-Identifier: MIT

using System.Numerics;

namespace fennecs.tests;

public class EntityTests(ITestOutputHelper output)
{
    [Fact]
    public void Virtual_Entities_have_no_Successors()
    {
        Assert.Throws<InvalidCastException>(() => Entity.Any.Successor);
        Assert.Throws<InvalidCastException>(() => Entity.None.Successor);
    }

    [Fact]
    public void Entity_Resolves_as_Type()
    {
        var entity = new Entity(123);
        Assert.Equal(typeof(Entity), entity.Type);

        var objEntity = Entity.Of("hello world");
        Assert.Equal(typeof(string), objEntity.Type);
    }
    [Fact]
    
    public void Identity_None_is_default()
    {
        var none = Entity.None;
        Assert.Equal(default, none.Generation);
        output.WriteLine(none.Generation.ToString());
        output.WriteLine(none.ToString());
        Assert.Equal(default, none.Id);
    }

    [Fact]
    public void Identity_ToString()
    {
        _ = Entity.None.ToString();
        _ = Entity.Any.ToString();
        _ = Entity.Of("hello world").ToString();
        _ = new Entity(123, 456).ToString();

        output.WriteLine(Entity.None.ToString());
        output.WriteLine(Entity.Any.ToString());
        output.WriteLine(Entity.Of("hello world").ToString());
        output.WriteLine(new Entity(123, 456).ToString());
    }

    [Fact]
    public void Identity_None_cannot_Match_One()
    {
        var zero = new Entity(0);
        Assert.NotEqual(Entity.None, zero);

        var one = new Entity(1);
        Assert.NotEqual(Entity.None, one);
    }

    [Fact]
    public void Identity_Matches_Only_Self()
    {
        var self = new Entity(12345);
        Assert.Equal(self, self);

        var successor = new Entity(12345, 3);
        Assert.NotEqual(self, successor);

        var other = new Entity(9000, 3);
        Assert.NotEqual(self, other);

    }

    [Theory]
    [InlineData(1500, 1500)]
    public void Identity_HashCodes_are_Unique(TypeID idCount, TypeID genCount)
    {
        var ids = new Dictionary<int, Entity>((int) (idCount * genCount * 4f));

        //Identities
        for (var i = 0; i < idCount ; i++)
        {
            //Generations
            for (TypeID g = 1; g < genCount; g++)
            {
                var identity = new Entity(i, g);

                Assert.NotEqual(identity, Entity.Any);
                Assert.NotEqual(identity, Entity.None);

                if (ids.ContainsKey(identity.GetHashCode()))
                {
                    Assert.Fail($"Collision of {identity} with {ids[identity.GetHashCode()]}, {identity.GetHashCode()}#==#{ids[identity.GetHashCode()].GetHashCode()}");
                }
                else
                {
                    ids.Add(identity.GetHashCode(), identity);
                }
            }
        }
    }

    [Fact]
    public void Equals_Prevents_Boxing_as_InvalidCastException()
    {
        object o = "don't @ me";
        var id = new Entity(69, 420);
        Assert.Throws<InvalidCastException>(() => id.Equals(o));
    }

    [Fact]
    public void Any_and_None_are_Distinct()
    {
        Assert.NotEqual(Entity.Any, Entity.None);
        Assert.NotEqual(Entity.Any.GetHashCode(), Entity.None.GetHashCode());
    }

    [Fact]
    public void Identity_Matches_Self_if_Same()
    {
        var random = new Random(420960);
        for (var i = 0; i < 1_000; i++)
        {
            var id = random.Next();
            var gen = (TypeID) random.Next();
            
            var self = new Entity(id, gen);
            var other = new Entity(id, gen);

            Assert.Equal(self, other);
        }
    }
    
    #region Input Data

    private struct CompoundComponent
    {
        // ReSharper disable once NotAccessedField.Local
        public required bool B1;

        // ReSharper disable once NotAccessedField.Local
        public required int I1;
    }

    private class ComponentDataSource : List<object[]>
    {
        public ComponentDataSource()
        {
            Add([123]);
            Add([1.23f]);
            Add([float.NegativeInfinity]);
            Add([float.NaN]);
            Add([new Vector2(1, 2)]);
            Add([new Vector3(1, 2, 3)]);
            Add([new Vector4(1, 2, 3, 4)]);
            Add([new Matrix4x4()]);
            Add([new CompoundComponent {B1 = true, I1 = 5}]);
            Add([new CompoundComponent {B1 = default, I1 = default}]);
        }
    }

    #endregion

    /*
    [Fact]
    private void Entity_ToString_Facades_Identity_ToString()
    {
        var identity = new Identity(123, 456);
        var identity = new Identity(identity);
        output.WriteLine(identity.ToString());
        Assert.Equal(identity.ToString(), identity.ToString());
    }
    */
    
    [Fact]
    public void Entity_HashCode_is_Stable()
    {
        using var world = new World();
        var entity1 = world.Spawn().Id();
        var entity2 = world.Spawn().Id();
        var hash1 = entity1.GetHashCode();
        var hash2 = entity2.GetHashCode();
        Assert.NotEqual(hash1, hash2);
        Assert.Equal(hash1, entity1.GetHashCode());
        Assert.Equal(hash2, entity2.GetHashCode());
    }

    [Fact] 
    private void Entity_is_Equal_to_Itself()
    {
        using var world = new World();
        var identity = world.Spawn().Id();
        Assert.Equal(identity, identity);
    }
    
    [Fact]
    private void Same_Entity_is_Equal()
    {
        var entity1 = new Entity(123, 999);
        var entity2 = new Entity(123, 999);
        Assert.Equal(entity1, entity2);
        Assert.True(entity1 == entity2);
    }



    [Fact]
    private void Different_Entity_is_Not_Equal()
    {
        var entity1 = new Entity(69, 420);
        var entity2 = new Entity(420, 69);

        var entity3 = new Entity(69, 69);
        var entity4 = new Entity(420, 420);
        
        Assert.NotEqual(entity1, entity2);
        Assert.True(entity1 != entity2);
        
        Assert.NotEqual(entity3, entity4);
        Assert.True(entity3 != entity4);
        
        Assert.NotEqual(entity1, entity3);
        Assert.True(entity1 != entity3);
        
        Assert.NotEqual(entity2, entity4);
        Assert.True(entity2 != entity4);
    }


    [Fact]
    public Entity Entity_is_Alive_after_Spawn()
    {
        using var world = new World();
        var identity = world.Spawn().Id();
        Assert.True(world.IsAlive(identity));
        return identity;
    }

    [Fact]
    private void Entity_is_Not_Alive_after_Despawn()
    {
        using var world = new World();
        var identity = world.Spawn().Id();
        world.Despawn(identity);
        Assert.False(world.IsAlive(identity));
    }

    [Fact]
    private void Entity_has_no_Components_after_Spawn()
    {
        using var world = new World();
        var identity = world.Spawn().Id();
        var components = world.GetComponents(identity);
        Assert.False(world.HasComponent<int>(identity));
        Assert.True(components.Count() == 1);
    }

    [Theory]
    [ClassData(typeof(ComponentDataSource))]
    private void Entity_can_Add_Component<T>(T t1) where T : struct
    {
        using var world = new World();
        var identity = world.Spawn().Id();
        world.On(identity).Add(t1);
        Assert.True(world.HasComponent<T>(identity));
        var components = world.GetComponents(identity);
        Assert.True(components.Count() == 2);
    }

    [Theory]
    [ClassData(typeof(ComponentDataSource))]
    private void Entity_cannot_Get_Component_from_Dead<T>(T t1) where T : struct
    {
        using var world = new World();
        var identity = world.Spawn().Add(t1).Id();
        world.Despawn(identity);

        Assert.Throws<ObjectDisposedException>(() => world.GetComponent<T>(identity));
    }

    [Theory]
    [ClassData(typeof(ComponentDataSource))]
    private void Entity_cannot_Get_Component_from_Successor<T>(T t1) where T : struct
    {
        using var world = new World();
        var entity1 = world.Spawn().Add(t1).Id();
        world.Despawn(entity1);
        var entity2 = world.Spawn().Add(t1).Id();

        Assert.Equal(entity1.Id, entity2.Id);
        Assert.Throws<ObjectDisposedException>(() => world.GetComponent<T>(entity1));
    }

    [Theory]
    [ClassData(typeof(ComponentDataSource))]
    private void Entity_can_Get_Component<T>(T t1) where T : struct
    {
        using var world = new World();
        var identity = world.Spawn().Add(t1).Id();
        var x = world.GetComponent<T>(identity);
        Assert.Equal(t1, x);
    }

    [Theory]
    [ClassData(typeof(ComponentDataSource))]
    private void Entity_can_Remove_Component<T>(T t1) where T : struct
    {
        using var world = new World();
        var identity = world.Spawn().Id();
        world.On(identity).Add(t1);
        world.On(identity).Remove<T>();
        Assert.False(world.HasComponent<T>(identity));
    }

    [Theory]
    [ClassData(typeof(ComponentDataSource))]
    private void Entity_can_ReAdd_Component<T>(T t1) where T : struct
    {
        using var world = new World();
        var identity = world.Spawn().Id();
        world.On(identity).Add(t1);
        world.On(identity).Remove<T>();
        world.On(identity).Add(t1);
        Assert.True(world.HasComponent<T>(identity));
    }

    [Theory]
    [ClassData(typeof(ComponentDataSource))]
    private void Entity_cannot_Add_Component_twice<T>(T t1) where T : struct 
    {
        using var world = new World();
        var identity = world.Spawn().Id();
        world.On(identity).Add(t1);
        Assert.Throws<ArgumentException>(() => world.On(identity).Add(t1));
    }

    [Theory]
    [ClassData(typeof(ComponentDataSource))]
    private void Entity_cannot_Remove_Component_twice<T>(T t1) where T : struct
    {
        using var world = new World();
        var identity = world.Spawn().Id();
        world.On(identity).Add(t1);
        world.On(identity).Remove<T>();
        Assert.Throws<ArgumentException>(() => world.On(identity).Remove<T>());
    }

    [Theory]
    [ClassData(typeof(ComponentDataSource))]
#pragma warning disable xUnit1026
    private void Entity_cannot_Remove_Component_without_Adding<T>(T _) where T : struct
    {
        using var world = new World();
        var identity = world.Spawn().Id();
        Assert.Throws<ArgumentException>(() => world.On(identity).Remove<T>());
    }
#pragma warning restore xUnit1026    
}