// SPDX-License-Identifier: MIT

using System.Numerics;

namespace fennecs.tests;

public class IdentityTests(ITestOutputHelper output)
{
    [Fact]
    public void Entity_None_is_default()
    {
        Assert.Equal(default, Entity.None);
    }


    [Fact]
    public void Entity_ToString()
    {
        _ = Match.Any.ToString();
        _ = Match.Entity.ToString();
        _ = Match.Target.ToString();
        _ = Match.Link.ToString();
        _ = default(Key).ToString();
        _ = Key.Of("hello world").ToString();
        _ = new Entity(123, 456).ToString();

        output.WriteLine(Match.Any.ToString());
        output.WriteLine(Match.Entity.ToString());
        output.WriteLine(Match.Target.ToString());
        output.WriteLine(Match.Link.ToString());
        output.WriteLine(default(Key).ToString());
        output.WriteLine(Key.Of("hello world").ToString());
        output.WriteLine(new Entity(123, 456).ToString());
    }


    [Fact]
    public void Entity_None_cannot_Match_One()
    {
        var zero = Entity.None;
        Assert.Equal(default, new Key(zero));

        var one = new Entity(1, 1);
        Assert.NotEqual((Key) default, new(one));
    }


    [Fact]
    public void Entity_Matches_Only_Self()
    {
        var self = new Entity(12, 12);
        Assert.Equal(self, self);

        var successor = self.Successor;
        Assert.NotEqual(self, successor);

        var other1 = new Entity(12, 3);
        Assert.NotEqual(self, other1);
        
        var other2 = new Entity(13, 12);
        Assert.NotEqual(self, other2);
    }


    /// <summary>
    /// This isn't really a test, as we're using the internal hashcode for long.
    /// </summary>
    [Theory]
    [InlineData(10, short.MaxValue)]
    [InlineData(50, short.MaxValue)]
    [InlineData(100, short.MaxValue)]
    public void Entity_HashCodes(int idCount, short genCount)
    {
        var ids = new Dictionary<int, Entity>((int)(idCount * genCount * 4f));
        var rnd = new Random(idCount);
        
        //Identities
        for (var i = 0; i < idCount; i++)
        {
            
            var index = rnd.Next();
            
            for (short generation = 1; generation < genCount; generation++)
            {
                var entity = new Entity(1, index, generation);

                Assert.NotEqual(new(entity), Match.Any);
                Assert.NotEqual(new(entity), (Key) default);
                if (!ids.TryAdd(entity.GetHashCode(), entity))
                {
                    Assert.Fail($"Collision of {entity} with {ids[entity.GetHashCode()]}, {entity.GetHashCode()}#==#{ids[entity.GetHashCode()].GetHashCode()}");
                }
            }
        }
        //Generations
    }

    
    [Fact]
    public void Any_and_None_are_Distinct()
    {
        Assert.NotEqual(Match.Any, default(Key));
        Assert.NotEqual(Match.Any.GetHashCode(), default(Key).GetHashCode());
    }

    [Fact]
    public void Can_Create_In_All_Worlds()
    {
        for (var i = 0; i < byte.MaxValue; i++)
        {
            _ = new Entity(new(i), 1);
        }

        HashSet<Entity> entities = [];
        List<World> worlds = [];
        
        for (var i = 0; i < byte.MaxValue; i++)
        {
            var world = new World();
            worlds.Add(world);
            var entity = world.Spawn();
            Assert.True(entities.Add(entity));
        }
        
        foreach (var world in worlds) world.Dispose();
    }

    [Theory]
    [InlineData(9_999)]
    public void Entity_Matches_Self_if_Same(int count)
    {
        var random = new Random(420960);
        for (var i = 1; i <= count; i++)
        {
            var world = new World.Id(random.Next() & 255);

            var index = random.Next();
            var gen = (short)(random.Next() % short.MaxValue);
            
            var self = new Entity(world, index, gen);
            var other = new Entity(world, index, gen);

            Assert.Equal(self, other);
        }
    }


    [Fact]
    public void Entity_HashCode_is_Stable()
    {
        using var world = new World();
        var entity1 = world.Spawn();
        var entity2 = world.Spawn();
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
        var entity = world.Spawn();
        Assert.Equal(entity, entity);
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
        var entity1 = new Entity(69, 42);
        var entity2 = new Entity(42, 69);

        var entity3 = new Entity(69, 69);
        var entity4 = new Entity(42, 42);

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
    public void Entity_is_Alive_after_Spawn()
    {
        using var world = new World();
        var entity = world.Spawn();
        Assert.True(world.IsAlive(entity));
    }


    [Fact]
    private void Entity_is_Not_Alive_after_Despawn()
    {
        using var world = new World();
        var entity = world.Spawn();
        world.Despawn(entity);
        Assert.False(world.IsAlive(entity));
    }


    [Fact]
    private void Entity_has_no_Components_after_Spawn()
    {
        using var world = new World();
        var entity = world.Spawn();
        var components = world.GetSignature(entity);
        Assert.False(world.HasComponent<int>(entity, default));
        Assert.True(components.Count() == 1);
    }


    [Theory]
    [ClassData(typeof(ComponentDataSource))]
    private void Entity_can_Add_Component<T>(T t1) where T : struct
    {
        using var world = new World();
        var entity = world.Spawn();
        entity.Add(t1);
        Assert.True(world.HasComponent<T>(entity, default));
        var components = world.GetSignature(entity);
        Assert.Equal(2, components.Count);
    }


    [Theory]
    [ClassData(typeof(ComponentDataSource))]
    private void Entity_cannot_Get_Component_from_Dead<T>(T t1) where T : struct
    {
        using var world = new World();
        var entity = world.Spawn().Add(t1);
        world.Despawn(entity);

        Assert.Throws<ObjectDisposedException>(() => world.GetComponent<T>(entity, default));
    }


    [Theory]
    [ClassData(typeof(ComponentDataSource))]
    private void Entity_cannot_Get_Component_from_Successor<T>(T t1) where T : struct
    {
        using var world = new World(0);
        var entity1 = world.Spawn().Add(t1);
        world.Despawn(entity1);
        var entity2 = world.Spawn().Add(t1);

        Assert.Equal(entity1.Index, entity2.Index);
        Assert.Throws<ObjectDisposedException>(() => world.GetComponent<T>(entity1, default));
    }


    [Theory]
    [ClassData(typeof(ComponentDataSource))]
    private void Entity_can_Get_Component<T>(T t1) where T : struct
    {
        using var world = new World();
        var entity = world.Spawn().Add(t1);
        var x = world.GetComponent<T>(entity, default);
        Assert.Equal(t1, x);
    }


    [Theory]
    [ClassData(typeof(ComponentDataSource))]
    private void Entity_can_Remove_Component<T>(T t1) where T : struct
    {
        using var world = new World();
        var entity = world.Spawn();
        entity.Add(t1);
        entity.Remove<T>();
        Assert.False(world.HasComponent<T>(entity, default));
    }


    [Theory]
    [ClassData(typeof(ComponentDataSource))]
    private void Entity_can_ReAdd_Component<T>(T t1) where T : struct
    {
        using var world = new World();
        var entity = world.Spawn();
        entity.Add(t1);
        entity.Remove<T>();
        entity.Add(t1);
        Assert.True(world.HasComponent<T>(entity, default));
    }


    [Theory]
    [ClassData(typeof(ComponentDataSource))]
    private void Entity_cannot_Add_Component_twice<T>(T t1) where T : struct
    {
        using var world = new World();
        var entity = world.Spawn();
        entity.Add(t1);
        Assert.Throws<InvalidOperationException>(() => entity.Add(t1));
    }


    [Theory]
    [ClassData(typeof(ComponentDataSource))]
    private void Entity_cannot_Remove_Component_twice<T>(T t1) where T : struct
    {
        using var world = new World();
        var entity = world.Spawn();
        entity.Add(t1);
        entity.Remove<T>();
        Assert.Throws<InvalidOperationException>(() => entity.Remove<T>());
    }


    [Theory]
    [ClassData(typeof(ComponentDataSource))]
#pragma warning disable xUnit1026
    private void Entity_cannot_Remove_Component_without_Adding<T>(T _) where T : struct
    {
        using var world = new World();
        var entity = world.Spawn();
        Assert.Throws<InvalidOperationException>(() => entity.Remove<T>());
    }
#pragma warning restore xUnit1026


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
            Add([new CompoundComponent { B1 = true, I1 = 5 }]);
            Add([new CompoundComponent { B1 = default, I1 = default }]);
        }
    }

    #endregion
}
