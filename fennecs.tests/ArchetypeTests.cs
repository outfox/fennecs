using System.Collections;
using fennecs.storage;

namespace fennecs.tests;

public class ArchetypeTests(ITestOutputHelper output)
{
    [Fact]
    public void Table_String_Contains_Types()
    {
        using var world = new World();
        var entity = world.Spawn().Add("foo").Add(123).Add(17.0f);

        var table = world.GetEntityMeta(entity).Archetype;

        output.WriteLine(table.ToString());
        Assert.Contains(typeof(Entity).ToString(), table.ToString());
        Assert.Contains(typeof(string).ToString(), table.ToString());
        Assert.Contains(typeof(int).ToString(), table.ToString());
        Assert.Contains(typeof(float).ToString(), table.ToString());
    }
    

    [Fact]
    public void GetStorage_Returns_IStorage_Backed_By_Specific_Type()
    {
        using var world = new World();
        var entity = world.Spawn().Add("foo").Add(123).Add(17.0f);
        var table = world.GetEntityMeta(entity).Archetype;
        var storage = table.GetStorage(TypeExpression.Of<string>((Key) default));
        Assert.IsAssignableFrom<IStorage>(storage);
        Assert.IsAssignableFrom<Storage<string>>(storage);
    }
    
    [Fact]
    public void Table_Matches_TypeExpression()
    {
        using var world = new World();
        var entity = world.Spawn().Add("foo").Add(123).Add(17.0f);
        var table = world.GetEntityMeta(entity).Archetype;

        var typeExpression = TypeExpression.Of<string>((Key) default);
        Assert.True(table.Has(typeExpression));

        var matchExpressionAny = MatchExpression.Of<string>(Match.Any);
        Assert.True(table.Has(matchExpressionAny));
    }


    [Fact]
    public void Table_Can_be_Generically_Enumerated()
    {
        using var world = new World();
        var other = world.Spawn().Add("foo").Add(123).Add(17.0f);
        var table = world.GetEntityMeta(other).Archetype;

        var count = 0;
        foreach (var entity in (IEnumerable) table)
        {
            count++;
            Assert.Equal(entity, entity);
        }

        Assert.Equal(1, count);
    }


    [Fact]
    public void Can_Truncate_Nothing()
    {
        using var world = new World();
        var entity = world.Spawn().Add("foo").Add(123).Add(17.0f).Id;
        var table = world.GetEntityMeta(entity).Archetype;

        table.Truncate(2000);
        Assert.Equal(1, table.Count);
        table.Truncate(1);
        Assert.Equal(1, table.Count);
    }


    [Fact]
    public void Can_Truncate_Negative()
    {
        using var world = new World();
        var entity = world.Spawn().Add("foo").Add(123).Add(17.0f).Id;
        var table = world.GetEntityMeta(entity).Archetype;

        table.Truncate(-2);
        Assert.Equal(0, table.Count);
    }

    [Fact]
    public void Moved_Entity_Leaves_Archetype()
    {
        using var world = new World();

        world.Spawn();
        world.Spawn().Add(123);
        
        var queryAll = world.Query().Compile();
        var queryInt = world.Query().Has<int>().Compile();
        
        Assert.Equal(2, queryAll.Count);
        Assert.Equal(1, queryInt.Count);
    }
    
    // Verifies fix to https://github.com/outfox/fennecs/issues/23
    [Fact]
    public void Remaining_Entity_Metas_Updated_Upon_Delete()
    {
        using var world = new World();
        Entity e1 = world.Spawn().Add(1);
        Entity e2 = world.Spawn().Add(2);
        e1.Despawn();
        Assert.Equal(2, e2.Ref<int>());

        Entity e3 = world.Spawn().Add(3);
        e2.Despawn();
        bool e3_seen_in_query_alive_and_with_val_3 = false;
        bool dead_entity_in_query = false;
        world.Query<int>().Stream().For((entity, val) =>
        {
            if (entity.Alive && val.read == 3)
            {
                e3_seen_in_query_alive_and_with_val_3 = true;
            }

            if (!entity.Alive)
            {
                dead_entity_in_query = true;
            }
        });
        Assert.True(e3_seen_in_query_alive_and_with_val_3);
        Assert.False(dead_entity_in_query);

        bool e3_seen_in_world_iteration_alive_and_with_val_3 = false;
        bool dead_entity_in_world_iteration = false;
        foreach (var entity in world)
        {
            if (entity.Alive && entity.Ref<int>() == 3)
            {
                e3_seen_in_world_iteration_alive_and_with_val_3 = true;
            }

            if (!entity.Alive)
            {
                dead_entity_in_world_iteration = true;
            }
        }
        Assert.True(e3_seen_in_world_iteration_alive_and_with_val_3);
        Assert.False(dead_entity_in_world_iteration);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(10)]
    [InlineData(69)]
    [InlineData(420)]
    [InlineData(10_000)]
    public void Meta_Integrity_After_Despawn(int count)
    {
        using var world = new World();
        
        Entity e1 = world.Spawn().Add(1);
        
        var entities = new Entity[count];
        for (var i = 0; i < entities.Length; i++)
        {
            entities[i] = world.Spawn().Add(i);
        }
        
        world.Despawn(e1);

        for (var i = 0; i < entities.Length; i++)
        {
            var entity = entities[i];
            Assert.True(world.IsAlive(entity));

            // Metas patched?
            Assert.Equal(entity, world.GetEntityMeta(entity).Entity);
        }
    }
    
    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(10)]
    [InlineData(69)]
    [InlineData(420)]
    [InlineData(10_000)]
    public void Components_Integrity_After_Despawn(int count)
    {
        using var world = new World();
        
        var e1 = world.Spawn().Add(-1);
        var e2 = world.Spawn().Add(-2);
        
        var entities = new List<Entity>(count);
        for (var i = 0; i < count; i++)
        {
            entities.Add(world.Spawn().Add(i));
        }

        world.Despawn(e1);

        for (var i = 0; i < count; i++)
        {
            var entity = entities[i];
            entity.Add((short) i);
        }

        world.Despawn(e2);

        for (var i = 0; i < count; i++)
        {
            var entity = entities[i];
            Assert.True(world.IsAlive(entity));

            // Components correct?
            Assert.Equal(i, entity.Ref<int>());    
            Assert.Equal(i, entity.Ref<short>());    
        }
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(10)]
    [InlineData(69)]
    [InlineData(420)]
    [InlineData(10_000)]
    public void Components_Integrity_After_Truncate(int count)
    {
        using var world = new World();
        
        var entities = new List<Entity>(count);
        for (var i = 0; i < count; i++)
        {
            entities.Add(world.Spawn().Add(i));
        }
        
        world.GetEntityMeta(entities[0]).Archetype.Truncate(10);
        entities = entities.Take(10).ToList();
        

        for (var i = 0; i < entities.Count; i++)
        {
            var entity = entities[i];
            entity.Add((short) i);
        }

        for (var i = 0; i < entities.Count; i++)
        {
            var entity = entities[i];
            Assert.True(world.IsAlive(entity));

            // Components correct?
            Assert.Equal(i, entity.Ref<int>());    
            Assert.Equal(i, entity.Ref<short>());    
        }
    }

    [Fact]
    public void IsComparable_Same_As_Signature()
    {
        using var world = new World();
        var entity1 = world.Spawn().Add("foo").Add(123).Add(17.0f).Id;
        _ = world.Spawn().Add(123).Add(17.0f).Id;
        
        var table1 = world.GetEntityMeta(entity1).Archetype;
        var table2 = world.GetEntityMeta(entity1).Archetype;

        Assert.True(table1.CompareTo(table2) == table1.Signature.CompareTo(table2.Signature));

        Assert.True(table1.CompareTo(null) == table1.Signature.CompareTo(default));
    }

    [Fact]
    public void Has_Signature_HashCode()
    {
        using var world = new World();
        var entity1 = world.Spawn().Add("foo").Add(123).Add(17.0f).Id;
        var entity2 = world.Spawn().Add(123).Add(17.0f).Id;
        
        var table1 = world.GetEntityMeta(entity1).Archetype;
        var table2 = world.GetEntityMeta(entity2).Archetype;

        Assert.True(table1.GetHashCode() == table1.Signature.GetHashCode());
        Assert.True(table2.GetHashCode() == table2.Signature.GetHashCode());
        Assert.NotEqual(table1.GetHashCode(), table2.GetHashCode());
    }
}