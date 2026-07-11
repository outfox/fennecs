// SPDX-License-Identifier: MIT

using System.Numerics;

namespace fennecs.tests;

public class EntityEncodingTests(ITestOutputHelper output)
{
    [Fact]
    public void Wildcards_have_no_Payload()
    {
        Assert.True(Key.Any.IsWildcard);
        Assert.True(Key.Target.IsWildcard);
        Assert.True(Key.AnyEntity.IsWildcard);
        Assert.True(Key.AnyObject.IsWildcard);
        Assert.False(Key.Plain.IsWildcard);

        Assert.Equal(0ul, Key.Any.Payload);
        Assert.Equal(0ul, Key.Target.Payload);
        Assert.Equal(0ul, Key.AnyEntity.Payload);
        Assert.Equal(0ul, Key.AnyObject.Payload);
    }


    [Fact]
    public void Object_Key_Resolves_as_Type()
    {
        var objKey = Key.Of("hello world");
        Assert.Equal(typeof(string), objKey.Type);
        Assert.True(objKey.IsObject);
        Assert.False(objKey.IsEntity);
        Assert.False(objKey.IsWildcard);
    }


    [Fact]
    public void Key_Plain_is_default()
    {
        var none = default(Key);
        Assert.Equal(Key.Plain, none);
        output.WriteLine(none.ToString());
        Assert.Equal(default, none);
    }


    [Fact]
    public void Key_ToString()
    {
        _ = Match.Any.ToString();
        _ = Match.Entity.ToString();
        _ = Match.Target.ToString();
        _ = Match.Object.ToString();
        _ = Match.Plain.ToString();
        _ = Key.Of("hello world").ToString();

        output.WriteLine(Match.Any.ToString());
        output.WriteLine(Match.Entity.ToString());
        output.WriteLine(Match.Target.ToString());
        output.WriteLine(Match.Object.ToString());
        output.WriteLine(Match.Plain.ToString());
        output.WriteLine(Key.Of("hello world").ToString());

        using var world = new World();
        var entity = world.Spawn();
        output.WriteLine(entity.Key.ToString());
        output.WriteLine(entity.ToString());
    }


    [Fact]
    public void Entity_Key_does_not_Match_Plain()
    {
        using var world = new World();
        var entity = world.Spawn();
        Assert.NotEqual(Match.Plain, new Match(entity.Key));
    }


    [Fact]
    public void Entity_Encoding_Roundtrip()
    {
        using var world = new World();
        var entity = world.Spawn();

        Assert.NotEqual(0u, entity.Index);
        Assert.Equal(world.Tag, entity.WorldTag);
        Assert.Equal((ushort) 1, entity.Generation);

        // The Key drops the generation but keeps kind, world, and index.
        var key = entity.Key;
        Assert.True(key.IsEntity);
        Assert.Equal(entity.Index, key.Index);
        Assert.Equal(entity.WorldTag, key.WorldTag);

        // Reconstructing the live handle from the Key yields the same Entity.
        Assert.Equal(entity, world.EntityFor(key));
    }


    [Fact]
    public void Default_Entity_is_never_Alive()
    {
        Assert.False(default(Entity).Alive);

        using var world = new World();
        Assert.False(world.IsAlive(default));
    }


    [Fact]
    public void Stale_Generation_is_not_Alive()
    {
        using var world = new World(0);
        var entity1 = world.Spawn();
        world.Despawn(entity1);

        var entity2 = world.Spawn();

        // Index gets recycled with a bumped generation.
        Assert.Equal(entity1.Index, entity2.Index);
        Assert.NotEqual(entity1.Generation, entity2.Generation);

        Assert.False(entity1.Alive);
        Assert.True(entity2.Alive);
    }


    [Fact]
    public void Foreign_Entity_is_not_Alive_in_other_World()
    {
        using var world1 = new World();
        using var world2 = new World();

        var entity = world1.Spawn();
        Assert.True(world1.IsAlive(entity));
        Assert.False(world2.IsAlive(entity));
    }


    [Theory]
    [InlineData(1500, 64)]
    public void Entity_HashCodes_are_Unique(int idCount, int genCount)
    {
        var ids = new Dictionary<int, Entity>(idCount * genCount * 4);

        //Indices
        for (var i = 1; i < idCount; i++)
        //Generations
        for (ushort g = 1; g < genCount; g++)
        {
            var entity = new Entity(1, (uint) i, g);

            Assert.NotEqual<Match>(new(entity.Key), Match.Any);
            Assert.NotEqual<Match>(new(entity.Key), Match.Plain);

            if (ids.ContainsKey(entity.GetHashCode()))
                Assert.Fail($"Collision of {entity} with {ids[entity.GetHashCode()]}, {entity.GetHashCode()}#==#{ids[entity.GetHashCode()].GetHashCode()}");
            else
                ids.Add(entity.GetHashCode(), entity);
        }
    }

    [Fact]
    public void Any_and_None_are_Distinct()
    {
        Assert.NotEqual(Match.Any, Match.Plain);
        Assert.NotEqual(Match.Any.GetHashCode(), Match.Plain.GetHashCode());
    }


    [Fact]
    public void Entity_Matches_Self_if_Same()
    {
        var random = new Random(420960);
        for (var i = 0; i < 1_000; i++)
        {
            var index = (uint) random.Next();
            var gen = (ushort) random.Next(1, ushort.MaxValue);

            var self = new Entity(1, index, gen);
            var other = new Entity(1, index, gen);

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
        var entity1 = new Entity(1, 123, 999);
        var entity2 = new Entity(1, 123, 999);
        Assert.Equal(entity1, entity2);
        Assert.True(entity1 == entity2);
    }


    [Fact]
    private void Different_Entity_is_Not_Equal()
    {
        var entity1 = new Entity(1, 69, 420);
        var entity2 = new Entity(1, 420, 69);

        var entity3 = new Entity(1, 69, 69);
        var entity4 = new Entity(2, 69, 69);

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
        Assert.True(components.Count() == 2);
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
