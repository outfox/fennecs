using System.Collections;

namespace fennecs.tests;

public class ArchetypeTests(ITestOutputHelper output)
{
    [Fact]
    public void Table_String_Contains_Types()
    {
        using var world = new World();
        var identity = world.Spawn().Add("foo").Add(123).Add(17.0f);

        var table = world.GetEntityMeta(identity).Archetype;

        output.WriteLine(table.ToString());
        Assert.Contains(typeof(Entity).ToString(), table.ToString());
        Assert.Contains(typeof(string).ToString(), table.ToString());
        Assert.Contains(typeof(int).ToString(), table.ToString());
        Assert.Contains(typeof(float).ToString(), table.ToString());
    }


    [Fact]
    public void Matches_Requires_At_Least_One_Any_Type()
    {
        using var world = new World();
        var plain = world.Spawn().Add(1);
        var tagged = world.Spawn().Add(2).Add("tag");

        using var mask = fennecs.pools.MaskPool.Rent();
        mask.Has(TypeExpression.Of<int>(Match.Plain));
        mask.Any(TypeExpression.Of<string>(Match.Plain));
        mask.Any(TypeExpression.Of<double>(Match.Plain));

        Assert.False(world.GetEntityMeta(plain).Archetype.Matches(MaskBits.Of(mask)));
        Assert.True(world.GetEntityMeta(tagged).Archetype.Matches(MaskBits.Of(mask)));
    }


    [Fact]
    public void Remove_Component_Shifts_Remaining_Values()
    {
        using var world = new World();
        var a = world.Spawn().Add(1).Add("a-str");
        var b = world.Spawn().Add(2).Add("b-str");

        a.Remove<string>();

        Assert.Equal("b-str", b.Ref<string>());
        Assert.Equal(2, b.Ref<int>());
        Assert.Equal(1, a.Ref<int>());
        Assert.False(a.Has<string>());
    }


    [Fact]
    public void Truncate_Preserves_Remaining_Component_Values()
    {
        using var world = new World();
        var entities = new List<Entity>();
        for (var i = 0; i < 5; i++) entities.Add(world.Spawn().Add(i).Add($"str{i}"));

        var table = world.GetEntityMeta(entities[0]).Archetype;
        table.Truncate(2);

        Assert.Equal(2, table.Count);
        Assert.Equal(0, entities[0].Ref<int>());
        Assert.Equal(1, entities[1].Ref<int>());
        Assert.Equal("str0", entities[0].Ref<string>());
        Assert.Equal("str1", entities[1].Ref<string>());
        Assert.False(world.IsAlive(entities[2]));
        Assert.False(world.IsAlive(entities[3]));
        Assert.False(world.IsAlive(entities[4]));
    }


    [Fact]
    public void Migrate_Invalidates_Destination_Enumerators()
    {
        using var world = new World();
        world.Spawn().Add(1);
        var resident = world.Spawn().Add(2).Add(1.0);
        var destination = world.GetEntityMeta(resident).Archetype;

        using var enumerator = destination.GetEnumerator();
        Assert.True(enumerator.MoveNext());

        // Migrates the (int) archetype's entity into the destination (int, double) archetype.
        world.Query<int>().Not<double>().Compile().Batch().Add(3.0).Submit();

        Assert.Throws<InvalidOperationException>(() => enumerator.MoveNext());
    }


    [Fact]
    public void GetStorage_Returns_IStorage_Backed_By_Specific_Type()
    {
        using var world = new World();
        var identity = world.Spawn().Add("foo").Add(123).Add(17.0f);
        var table = world.GetEntityMeta(identity).Archetype;
        var storage = table.GetStorage(TypeExpression.Of<string>(Match.Plain));
        Assert.IsAssignableFrom<IStorage>(storage);
        Assert.IsAssignableFrom<Storage<string>>(storage);
    }

    [Fact]
    public void Table_Matches_TypeExpression()
    {
        using var world = new World();
        var identity = world.Spawn().Add("foo").Add(123).Add(17.0f);
        var table = world.GetEntityMeta(identity).Archetype;

        var typeExpression = TypeExpression.Of<string>(Match.Plain);
        Assert.True(table.Matches(typeExpression));

        var typeExpressionAny = TypeExpression.Of<string>(Match.Any);
        Assert.True(table.Matches(typeExpressionAny));
    }


    [Fact]
    public void Table_Can_be_Generically_Enumerated()
    {
        using var world = new World();
        var other = world.Spawn().Add("foo").Add(123).Add(17.0f);
        var table = world.GetEntityMeta(other).Archetype;

        var count = 0;
        foreach (var entity in (IEnumerable)table)
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
        var identity = world.Spawn().Add("foo").Add(123).Add(17.0f);
        var table = world.GetEntityMeta(identity).Archetype;

        table.Truncate(2000);
        Assert.Equal(1, table.Count);
        table.Truncate(1);
        Assert.Equal(1, table.Count);
    }


    [Fact]
    public void Can_Truncate_Negative()
    {
        using var world = new World();
        var identity = world.Spawn().Add("foo").Add(123).Add(17.0f);
        var table = world.GetEntityMeta(identity).Archetype;

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
        Assert.Single(queryInt);
    }

    // Verifies fix to https://github.com/outfox/fennecs/issues/23
    [Fact]
    public void Remaining_Entity_Metas_Updated_Upon_Delete()
    {
        using var world = new World();
        var e1 = world.Spawn().Add(1);
        var e2 = world.Spawn().Add(2);
        e1.Despawn();
        Assert.Equal(2, e2.Ref<int>());

        var e3 = world.Spawn().Add(3);
        e2.Despawn();
        var e3_seen_in_query_alive_and_with_val_3 = false;
        var dead_entity_in_query = false;
        world.Query<int>().Stream().For((in entity, ref val) =>
        {
            if (entity.Alive && val == 3)
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

        var e3_seen_in_world_iteration_alive_and_with_val_3 = false;
        var dead_entity_in_world_iteration = false;
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

        var e1 = world.Spawn().Add(1);

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
            Assert.Equal(entity, world.GetEntityMeta(entity).Archetype[world.GetEntityMeta(entity).Row]);
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
            entity.Add((short)i);
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
            entity.Add((short)i);
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
        var entity1 = world.Spawn().Add("foo").Add(123).Add(17.0f);
        _ = world.Spawn().Add(123).Add(17.0f);

        var table1 = world.GetEntityMeta(entity1).Archetype;
        var table2 = world.GetEntityMeta(entity1).Archetype;

        Assert.True(table1.CompareTo(table2) == table1.Signature.CompareTo(table2.Signature));

        Assert.True(table1.CompareTo(null) == table1.Signature.CompareTo(default));
    }

    [Fact]
    public void Has_Signature_HashCode()
    {
        using var world = new World();
        var entity1 = world.Spawn().Add("foo").Add(123).Add(17.0f);
        var entity2 = world.Spawn().Add(123).Add(17.0f);

        var table1 = world.GetEntityMeta(entity1).Archetype;
        var table2 = world.GetEntityMeta(entity2).Archetype;

        Assert.True(table1.GetHashCode() == table1.Signature.GetHashCode());
        Assert.True(table2.GetHashCode() == table2.Signature.GetHashCode());
        Assert.NotEqual(table1.GetHashCode(), table2.GetHashCode());
    }
}
