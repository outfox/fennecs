using System.Collections;
using System.Numerics;

namespace fennecs.tests.Query;

public class QueryTests
{
    [Fact]
    private void Can_Enumerate_PlainEnumerator()
    {
        using var world = new World();

        var entities = new List<Entity>();
        for (var i = 0; i < 5; i++)
        {
            var entity = world.Spawn().Add(new object());
            entities.Add(entity);
        }

        var query = world.Query<object>().Build();
        var plain = query as IEnumerable;

        foreach (var current in plain)
        {
            Assert.IsType<Entity>(current);

            var entity = (Entity) current;
            Assert.Contains(entity, entities);
            entities.Remove(entity);
        }

        Assert.Empty(entities);
    }


    [Fact]
    private void Contains_Finds_Entity()
    {
        using var world = new World();

        var random = new Random(1234);
        var entities = new List<Entity>();
        for (var i = 0; i < 2345; i++)
        {
            var identity = world.Spawn().Add(i);
            entities.Add(identity);
        }

        var query = world.Query<int>().Build();

        Assert.True(entities.All(e => query.Contains(e)));

        var former = entities.ToArray();
        while (entities.Count > 0)
        {
            var index = random.Next(entities.Count);
            var identity = entities[index];
            world.Despawn(identity);
            Assert.False(query.Contains(identity));
            entities.RemoveAt(index);
        }

        Assert.True(!former.Any(e => query.Contains(e)));
    }


    [Fact]
    private void Has_Matches()
    {
        var p1 = new Vector3(6, 6, 6);
        var p2 = new Vector3(1, 2, 3);

        using var world = new World();
        world.Spawn().Add(p1);
        world.Spawn().Add(p2).Add<int>();
        world.Spawn().Add(p2).Add<int>();

        var query = world.Query<Vector3>()
            .Has<int>()
            .Build();

        query.Raw(memory =>
        {
            Assert.True(memory.Length == 2);
            foreach (var pos in memory.Span) Assert.Equal(p2, pos);
        });
    }


    [Fact]
    private void Not_prevents_Match()
    {
        var p1 = new Vector3(6, 6, 6);
        var p2 = new Vector3(1, 2, 3);

        using var world = new World();
        world.Spawn().Add(p1);
        world.Spawn().Add(p2).Add<int>();
        world.Spawn().Add(p2).Add<int>();

        var query = world.Query<Vector3>()
            .Not<int>()
            .Build();

        query.Raw(memory =>
        {
            Assert.True(memory.Length == 1);
            foreach (var pos in memory.Span) Assert.Equal(p1, pos);
        });
    }


    [Fact]
    private void Any_Target_None_Matches_Only_None()
    {
        var p1 = new Vector3(6, 6, 6);
        var p2 = new Vector3(1, 2, 3);
        var p3 = new Vector3(4, 4, 4);

        using var world = new World();
        var alice = world.Spawn().Add(p1).Add(0);
        var bob = world.Spawn().Add(p2).AddRelation(alice, 111);
        /*var charlie = */
        world.Spawn().Add(p3).AddRelation(bob, 222);

        var query = world.Query<Identity, Vector3>()
            .Any<int>(Match.Plain)
            .Build();

        var count = 0;
        query.Raw((me, mp) =>
        {
            count++;
            Assert.Equal(1, mp.Length);
            var identity = me.Span[0];
            Assert.Equal(alice, identity);
        });
        Assert.Equal(1, count);
    }


    [Fact]
    private void Any_Target_Single_Matches()
    {
        var p1 = new Vector3(6, 6, 6);
        var p2 = new Vector3(1, 2, 3);
        var p3 = new Vector3(4, 4, 4);

        using var world = new World();
        var alice = world.Spawn().Add(p1).Add(0);
        var eve = world.Spawn().Add(p2).AddRelation(alice, 111);
        var charlie = world.Spawn().Add(p3).AddRelation(eve, 222);

        var query = world.Query<Identity, Vector3>().Any<int>(eve).Build();

        var count = 0;
        query.Raw((me, mp) =>
        {
            count++;
            Assert.Equal(1, mp.Length);
            var identity = me.Span[0];
            Assert.Equal(charlie, identity);
            var pos = mp.Span[0];
            Assert.Equal(pos, p3);
        });
        Assert.Equal(1, count);
    }


    [Fact]
    private void Any_Target_Multiple_Matches()
    {
        var p1 = new Vector3(6, 6, 6);
        var p2 = new Vector3(1, 2, 3);
        var p3 = new Vector3(4, 4, 4);

        using var world = new World();
        var alice = world.Spawn().Add(p1).Add(0);
        var eve = world.Spawn().Add(p2).AddRelation(alice, 111);
        var charlie = world.Spawn().Add(p3).AddRelation(eve, 222);

        var query = world.Query<Identity, Vector3>()
            .Any<int>(eve)
            .Any<int>(alice)
            .Build();

        var count = 0;
        query.Raw((me, mp) =>
        {
            Assert.Equal(1, mp.Length);
            for (var index = 0; index < me.Length; index++)
            {
                var identity = me.Span[index];
                count++;
                if (identity == charlie)
                {
                    var pos = mp.Span[index];
                    Assert.Equal(pos, p3);
                }
                else if (identity == eve)
                {
                    var pos = mp.Span[index];
                    Assert.Equal(pos, p2);
                }
                else
                {
                    Assert.Fail("Unexpected identity");
                }
            }
        });
        Assert.Equal(2, count);
    }


    [Fact]
    private void Any_Not_does_not_Match_Specific()
    {
        var p1 = new Vector3(6, 6, 6);
        var p2 = new Vector3(1, 2, 3);
        var p3 = new Vector3(4, 4, 4);

        using var world = new World();
        var alice = world.Spawn().Add(p1).Add(0);
        var bob = world.Spawn().Add(p2).AddRelation(alice, 111);
        var eve = world.Spawn().Add(p1).Add(888);

        /*var charlie = */
        world.Spawn().Add(p3).AddRelation(bob, 222);
        /*var charlie = */
        world.Spawn().Add(p3).AddRelation(eve, 222);

        var query = world.Query<Identity, Vector3>()
            .Not<int>(bob)
            .Any<int>(alice)
            .Build();

        var count = 0;
        query.Raw((me, mp) =>
        {
            Assert.Equal(1, mp.Length);
            for (var index = 0; index < me.Length; index++)
            {
                var identity = me.Span[index];
                count++;
                if (identity == bob)
                {
                    var pos = mp.Span[index];
                    Assert.Equal(pos, p2);
                }
                else
                {
                    Assert.Fail("Unexpected identity");
                }
            }
        });
        Assert.Equal(1, count);
    }


    [Fact]
    private void Query_provided_Has_works_with_Target()
    {
        var p1 = new Vector3(6, 6, 6);
        var p2 = new Vector3(1, 2, 3);
        var p3 = new Vector3(4, 4, 4);

        using var world = new World();

        var alice = world.Spawn().Add(p1).Add(0);
        var eve = world.Spawn().Add(p1).Add(888);

        var bob = world.Spawn().Add(p2).AddRelation(alice, 111);

        world.Spawn().Add(p3).AddRelation(bob, 555);
        world.Spawn().Add(p3).AddRelation(eve, 666);

        var query = world.Query<Identity, Vector3, int>(Match.Plain, Match.Plain, Match.Plain)
            .Not<int>(bob)
            .Build();

        var count = 0;
        query.Raw((me, mp, mi) =>
        {
            Assert.Equal(2, mp.Length);
            for (var index = 0; index < me.Length; index++)
            {
                var identity = me.Span[index];
                count++;

                if (identity == alice)
                {
                    var pos = mp.Span[0];
                    Assert.Equal(pos, p1);
                    var integer = mi.Span[index];
                    Assert.Equal(0, integer);
                }
                else if (identity == eve)
                {
                    var pos = mp.Span[index];
                    Assert.Equal(pos, p1);
                    var i = mi.Span[index];
                    Assert.Equal(888, i);
                }
                else
                {
                    Assert.Fail($"Unexpected identity {identity}");
                }
            }
        });
        Assert.Equal(2, count);
    }


    [Fact]
    private void Queries_are_Cached()
    {
        using var world = new World();

        world.Spawn().Add(123);

        var query1A = world.Query().Build();
        var query1B = world.Query().Build();

        var query2A = world.Query<Identity>().Build();
        var query2B = world.Query<Identity>().Build();

        var query3A = world.Query().Has<int>().Build();
        var query3B = world.Query().Has<int>().Build();

        var query4A = world.Query<Identity>().Not<int>().Build();
        var query4B = world.Query<Identity>().Not<int>().Build();

        var query5A = world.Query<Identity>().Any<int>().Any<float>().Build();
        var query5B = world.Query<Identity>().Any<int>().Any<float>().Build();

        Assert.True(ReferenceEquals(query1A, query1B));
        Assert.True(ReferenceEquals(query2A, query2B));
        Assert.True(ReferenceEquals(query3A, query3B));
        Assert.True(ReferenceEquals(query4A, query4B));
        Assert.True(ReferenceEquals(query5A, query5B));
    }


    [Fact]
    private void Queries_are_Disposable()
    {
        using var world = new World();

        var query = world.Query().Build();
        query.Dispose();
        /*
         TODO: Re-enable this test when Query disposal repercussions redesigned :D
        Assert.Throws<ObjectDisposedException>(() => query.Raw(_ => { }));
        Assert.Throws<ObjectDisposedException>(() =>
        {
            foreach (var _ in query) Assert.Fail("Should not enumerate disposed Query.");
        });
        */
    }


    [Fact]
    private void Ref_disallows_Component_Type_Entity()
    {
        using var world = new World();
        var identity = world.Spawn();
        var query = world.Query<Identity>().Build();

        Assert.Throws<TypeAccessException>(() => query.Ref<Identity>(identity));
    }


    [Fact]
    private void Ref_disallows_Dead_Entity()
    {
        using var world = new World();
        var entity = world.Spawn().Add<int>();
        world.Despawn(entity);
        Assert.False(world.IsAlive(entity));

        var query = world.Query<int>().Build();
        Assert.Throws<ObjectDisposedException>(() => query.Ref<int>(entity));
    }


    [Fact]
    private void Ref_disallows_Nonexistent_Component()
    {
        using var world = new World();
        var identity = world.Spawn().Add<int>();
        var query = world.Query<int>().Build();

        Assert.Throws<TypeAccessException>(() => query.Ref<float>(identity));
    }


    [Fact]
    private void Ref_gets_Mutable_Component()
    {
        using var world = new World();
        var identity = world.Spawn().Add(23);
        var query = world.Query<int>().Build();

        ref var gotten = ref query.Ref<int>(identity);
        Assert.Equal(23, gotten);

        // Identity can't be a ref (is readonly - make sure!)
        gotten = 42;
        Assert.Equal(42, query.Ref<int>(identity));
    }


    [Fact]
    private void Contains_Like_Enumerable()
    {
        using var world = new World();
        var entity23 = world.Spawn().Add(23);
        var entity42 = world.Spawn().Add(42);

        var query = world.Query<int>().Build();
        Assert.Contains(entity23, query);
        Assert.Contains(entity42, query);
    }


    [Fact]
    private void Indexer()
    {
        using var world = new World();
        var entity23 = world.Spawn().Add(23);
        var entity42 = world.Spawn().Add(42);

        var query = world.Query<int>().Build();
        Assert.Equal(entity23, query[0]);
        Assert.Equal(entity42, query[1]);
    }


    [Fact]
    private void Indexer_Multi_Table_Join()
    {
        using var world = new World();
        var entity23 = world.Spawn().Add(23);
        var entity42 = world.Spawn().Add(42).Add<string>("I'm in another table");

        var query = world.Query<int>().Build();
        Assert.Equal(entity23, query[0]);
        Assert.Equal(entity42, query[1]);
    }


    [Fact]
    private void Indexer_Throws_When_Out_Of_Range()
    {
        using var world = new World();
        var query = world.Query<int>().Build();
        Assert.Throws<IndexOutOfRangeException>(() => query[0]);
        Assert.Throws<IndexOutOfRangeException>(() => query[-1]);
        Assert.Throws<IndexOutOfRangeException>(() => query[1]);

        var entity = world.Spawn().Add(23);
        Assert.Equal(entity, query[0]);
        Assert.Throws<IndexOutOfRangeException>(() => query[-1]);
        Assert.Throws<IndexOutOfRangeException>(() => query[1]);
    }


    [Fact]
    private void Random_Access_Is_Possible()
    {
        using var world = new World();
        var query = world.Query<int>().Build();
        var entity23 = world.Spawn().Add(23);
        var entity42 = world.Spawn().Add(42);
        Assert.Contains(query.Random(), [entity23, entity42]);
    }


    [Fact]
    private void Random_Access_with_One_Entity()
    {
        using var world = new World();
        var query = world.Query<int>().Build();
        var entity = world.Spawn().Add(23);
        Assert.Equal(entity, query.Random());
    }


    [Fact]
    private void Random_Access_Throws_with_Empty_Query()
    {
        using var world = new World();
        var query = world.Query<int>().Build();
        Assert.True(query.IsEmpty);
        Assert.Throws<IndexOutOfRangeException>(() => query.Random());
    }


    [Fact]
    private void Query_Contains_Type()
    {
        using var world = new World();
        var query = world.Query<int>().Build();
        Assert.True(query.Contains<int>());
        Assert.False(query.Contains<float>());
    }


    [Fact]
    private void Query_Contains_Type_Subset()
    {
        using var world = new World();
        var query = world.Query<int>(Match.Entity).Build();
        Assert.True(query.Contains<int>(Match.Any));
        Assert.False(query.Contains<float>(Match.Any));
    }


    [Fact]
    private void Query_Containss_Type_Superset()
    {
        using var world = new World();
        var query = world.Query<int>(Match.Any).Build();
        Assert.True(query.Contains<int>(Match.Plain));
        Assert.False(query.Contains<float>(Match.Object));
    }


    [Fact]
    private void Query_Contains_Entity()
    {
        using var world = new World();
        var entity = world.Spawn().Add(23);
        var query = world.Query<int>().Build();
        Assert.True(query.Contains(entity));
    }


    [Fact]
    private void Ref_throws_for_non_contained_Entity()
    {
        using var world = new World();
        var entity = world.Spawn().Add(23);
        var query = world.Query<int>().Build();
        world.Despawn(entity);
        Assert.Throws<ObjectDisposedException>(() => query.Ref<int>(entity));

        var foreignQueryEntity = world.Spawn().Add("not in query");
        Assert.Throws<KeyNotFoundException>(() => query.Ref<int>(foreignQueryEntity));

        using var anotherWorld = new World();
        var foreignWorldEntity = anotherWorld.Spawn().Add(23);
        Assert.Throws<InvalidOperationException>(() => query.Ref<int>(foreignWorldEntity));
    }


    [Fact]
    public void Filtered_Enumerator_Filters()
    {
        using var world = new World();
        var query = world.Query<Identity, int>(Match.Plain, Match.Any).Build();

        var entity1 = world.Spawn().Add(444);
        var entity2 = world.Spawn().AddRelation(entity1, 555);

        //Partial miss
        var tx = TypeExpression.Of<int>(Match.Plain);
        Assert.Contains(entity1, query.Filtered(tx));
        Assert.DoesNotContain(entity2, query.Filtered(tx));

        //Complete miss
        tx = TypeExpression.Of<string>(Match.Any);
        Assert.DoesNotContain(entity1, query.Filtered(tx));
        Assert.DoesNotContain(entity2, query.Filtered(tx));

        //No-op filter
        tx = TypeExpression.Of<int>(Match.Any);
        Assert.Contains(entity1, query.Filtered(tx));
        Assert.Contains(entity2, query.Filtered(tx));
    }


    [Fact]
    public void Can_Iterate_ArchetypesReadonly()
    {
        using var world = new World();

        var query = world.Query<int>(Match.Any).Build();
        var entity1 = world.Spawn().Add(444);
        world.Spawn().AddRelation(entity1, 555);

        Assert.Equal(2, query.TrackedArchetypes.Count);
    }


    [Theory]
    [InlineData(0, 0)]
    [InlineData(0, 1)]
    [InlineData(0, 2)]
    [InlineData(0, 10_000)]
    [InlineData(1, 0)]
    [InlineData(1, 1)]
    [InlineData(1, 2)]
    [InlineData(1, 10_000)]
    [InlineData(10_000, 0)]
    [InlineData(10_000, 1)]
    [InlineData(10_000, 2)]
    [InlineData(10_000, 10_000)]
    [InlineData(10_000, 10_001)]
    public void Can_Truncate(int entityCount, int targetSize)
    {
        using var world = new World();
        var query = world.Query<int>(Match.Any).Build();

        for (var i = 0; i < entityCount; i++) world.Spawn().Add(i);

        query.Truncate(targetSize);
        Assert.True(query.Count <= targetSize);
    }


    [Fact]
    public void Can_Clear()
    {
        using var world = new World();
        var query = world.Query<int>(Match.Any).Build();

        for (var i = 0; i < 420; i++) world.Spawn().Add(i);

#pragma warning disable CS0618 // Type or member is obsolete
        query.Clear();
#pragma warning restore CS0618 // Type or member is obsolete
        Assert.Equal(0, query.Count);
    }


    [Fact]
    public void Can_Despawn()
    {
        using var world = new World();
        var query = world.Query<int>(Match.Any).Build();

        for (var i = 0; i < 420; i++) world.Spawn().Add(i);

        query.Despawn();
        Assert.Equal(0, query.Count);
    }


    [Fact]
    public void Can_Enumerate()
    {
        using var world = new World();
        var query = world.Query().Build();
        
        var entity1 = world.Spawn().Add(444);
        var entity2 = world.Spawn().AddRelation(entity1, 555);

        var spawnedEntities = new List<Entity>
        {
            entity1,
            entity2,
        };
        Assert.Contains(entity1, query);
        Assert.Contains(entity2, query);

        foreach (var entity in query)
        {
            Assert.Contains(entity, spawnedEntities);
            spawnedEntities.Remove(entity);
        }
        Assert.Empty(spawnedEntities);
    }
}