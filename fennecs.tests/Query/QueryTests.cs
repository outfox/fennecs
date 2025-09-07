﻿using System.Collections;
using System.Numerics;

// ReSharper disable ParameterOnlyUsedForPreconditionCheck.Local

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

        var query = world.Query<object>().Compile();
        var plain = query as IEnumerable;

        foreach (var current in plain)
        {
            Assert.IsType<Entity>(current);

            var entity = (Entity)current;
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

        var query = world.Query<int>().Compile();

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
            .Stream();

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
            .Stream();

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
        var bob = world.Spawn().Add(p2).Add(111, alice);
        /*var charlie = */
        world.Spawn().Add(p3).Add(222, bob);

        var query = world.Query<Identity, Vector3>(Match.Plain, default)
            .Any<int>(Match.Plain)
            .Stream();

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
        var eve = world.Spawn().Add(p2).Add(111, alice);
        var charlie = world.Spawn().Add(p3).Add(222, eve);

        var query = world.Query<Identity, Vector3>(Match.Plain, Match.Plain).Any<int>(eve).Stream();

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
        var eve = world.Spawn().Add(p2).Add(111, alice);
        var charlie = world.Spawn().Add(p3).Add(222, eve);

        var query = world.Query<Identity, Vector3>(Match.Plain, Match.Plain)
            .Any<int>(eve)
            .Any<int>(alice)
            .Stream();

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
        var bob = world.Spawn().Add(p2).Add(111, alice);
        var eve = world.Spawn().Add(p1).Add(888);

        /*var charlie = */
        world.Spawn().Add(p3).Add(222, bob);
        /*var charlie = */
        world.Spawn().Add(p3).Add(222, eve);

        var query = world.Query<Identity, Vector3>(Match.Plain, Match.Plain)
            .Not<int>(bob)
            .Any<int>(alice)
            .Stream();

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

        var bob = world.Spawn().Add(p2).Add(111, alice);

        world.Spawn().Add(p3).Add(555, bob);
        world.Spawn().Add(p3).Add(666, eve);

        var query = world.Query<Identity, Vector3, int>(Match.Plain, Match.Plain, Match.Plain)
            .Not<int>(bob)
            .Stream();

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

        var query1A = world.Query().Compile();
        var query1B = world.Query().Compile();

        var query2A = world.Query<Identity>(Match.Plain).Compile();
        var query2B = world.Query<Identity>(Match.Plain).Compile();

        var query3A = world.Query().Has<int>().Compile();
        var query3B = world.Query().Has<int>().Compile();

        var query4A = world.Query<Identity>(Match.Plain).Not<int>().Compile();
        var query4B = world.Query<Identity>(Match.Plain).Not<int>().Compile();

        var query5A = world.Query<Identity>(Match.Plain).Any<int>().Any<float>().Compile();
        var query5B = world.Query<Identity>(Match.Plain).Any<int>().Any<float>().Compile();

        Assert.Equal(query1A, query1B);
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

        var query = world.Query().Compile();
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
    private void Query_Double_Dispose()
    {
        using var world = new World();

        var query = world.Query().Compile();
        query.Dispose();
        Assert.Throws<ObjectDisposedException>(query.Dispose);
    }



    [Fact]
    private void Contains_Like_Enumerable()
    {
        using var world = new World();
        var entity23 = world.Spawn().Add(23);
        var entity42 = world.Spawn().Add(42);

        var query = world.Query<int>().Compile();
        Assert.Contains(entity23, query);
        Assert.Contains(entity42, query);
    }


    [Fact]
    private void Indexer()
    {
        using var world = new World();
        var entity23 = world.Spawn().Add(23);
        var entity42 = world.Spawn().Add(42);

        var query = world.Query<int>().Compile();
        Assert.Equal(entity23, query[0]);
        Assert.Equal(entity42, query[1]);
    }


    [Fact]
    private void Indexer_Multi_Table_Join()
    {
        using var world = new World();
        var entity23 = world.Spawn().Add(23);
        var entity42 = world.Spawn().Add(42).Add<string>("I'm in another table");

        var query = world.Query<int>().Compile();
        Assert.Contains(entity23, query);
        Assert.Contains(entity42, query);
        Assert.Equal(2, query.Count);

        //After switching to sorted sets, this became a tad less predictable (opposite should be true?)
        Assert.True (entity23 == query[0] || entity23 == query[1]);
        Assert.True (entity42 == query[0] || entity42 == query[1]);
    }


    [Fact]
    private void Indexer_Throws_When_Out_Of_Range()
    {
        using var world = new World();
        var query = world.Query<int>().Compile();
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
        var query = world.Query<int>().Compile();
        var entity23 = world.Spawn().Add(23);
        var entity42 = world.Spawn().Add(42);
        Assert.Contains(query.Random(), new[] { entity23, entity42 });
    }


    [Fact]
    private void Random_Access_with_One_Entity()
    {
        using var world = new World();
        var query = world.Query<int>().Compile();
        var entity = world.Spawn().Add(23);
        Assert.Equal(entity, query.Random());
    }


    [Fact]
    private void Random_Access_Throws_with_Empty_Query()
    {
        using var world = new World();
        var query = world.Query<int>().Compile();
        Assert.True(query.IsEmpty);
        Assert.Throws<IndexOutOfRangeException>(() => query.Random());
    }


    [Fact]
    private void Query_Contains_Type()
    {
        using var world = new World();
        var query = world.Query<int>().Compile();
        world.Spawn().Add<int>();
        Assert.True(query.Contains<int>());
        Assert.False(query.Contains<float>());
    }


    [Fact]
    private void Query_Contains_Type_Subset()
    {
        using var world = new World();
        var query = world.Query<int>(Match.Entity).Compile();
        var entity = world.Spawn();
        world.Spawn().Add<int>(entity);
        Assert.True(query.Contains<int>(Match.Any));
        Assert.False(query.Contains<float>(Match.Any));
    }


    [Fact]
    private void Query_Containss_Type_Superset()
    {
        using var world = new World();
        var query = world.Query<int>(Match.Any).Compile();
        world.Spawn().Add<int>();
        Assert.True(query.Contains<int>(Match.Plain));
        Assert.False(query.Contains<float>(Match.Object));
    }


    [Fact]
    private void Query_Contains_Entity()
    {
        using var world = new World();
        var entity = world.Spawn().Add(23);
        var query = world.Query<int>().Compile();
        Assert.True(query.Contains(entity));
    }


    [Fact]
    public void Filtered_Enumerator_Filters()
    {
        using var world = new World();
        var query = world.Query<Identity, int>(Match.Plain, Match.Any).Compile();

        var entity1 = world.Spawn().Add(444);
        var entity2 = world.Spawn().Add(555, entity1);

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

        var query = world.Query<int>(Match.Any).Compile();
        var entity1 = world.Spawn().Add(444);
        world.Spawn().Add(555, entity1);

        Assert.Equal(2, query.Archetypes.Count);
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
        var query = world.Query<int>(Match.Any).Stream();

        for (var i = 0; i < entityCount; i++) world.Spawn().Add(i);

        query.Truncate(targetSize);
        Assert.True(query.Count <= targetSize);
    }

/*
    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(69)]
    [InlineData(1_000)]
    public void Truncate_Honors_Filter_Exclude(int entityCount)
    {
        using var world = new World(entityCount * 2 + 2);
        world.Entity().Add<int>().Spawn(entityCount).Dispose();
        world.Entity().Add<int>().Add<string>("don't truncate me, senpai").Spawn(entityCount).Dispose();

        var query = world.Query<int>().Compile();
        Assert.Equal(entityCount * 2, query.Count);

        query.Exclude<string>(Match.Any);
        Assert.Equal(entityCount, query.Count);

        query.Truncate(0);
        Assert.Equal(0, query.Count);

        query.ClearFilters();
        Assert.Equal(entityCount, query.Count);

        Assert.All(query, e => Assert.True(e.Has<string>()));
    }
    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(69)]
    [InlineData(1_000)]
    public void Truncate_Honors_Filter_Subset(int entityCount)
    {
        using var world = new World();
        world.Entity().Add<int>().Spawn(entityCount).Dispose();
        world.Entity().Add<int>().Add<string>("PLEASE TRUNCATE ME!").Spawn(entityCount).Dispose();

        var query = world.Query<int>().Compile();
        Assert.Equal(entityCount * 2, query.Count);

        query.Subset<string>(Match.Any);
        Assert.Equal(entityCount, query.Count);

        query.Truncate(0);
        Assert.Equal(0, query.Count);

        query.ClearFilters();
        Assert.Equal(entityCount, query.Count);

        Assert.All(query, e => Assert.False(e.Has<string>()));
    }
*/


    [Fact]
    public void Can_Clear()
    {
        using var world = new World();
        var query = world.Query<int>(Match.Any).Compile();

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
        var query = world.Query<int>(Match.Any).Compile();

        for (var i = 0; i < 420; i++) world.Spawn().Add(i);

        query.Despawn();
        Assert.Equal(0, query.Count);
    }


    [Fact]
    public void Can_Enumerate()
    {
        using var world = new World();
        var query = world.Query().Compile();

        var entity1 = world.Spawn().Add(444);
        var entity2 = world.Spawn().Add(555, entity1);

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

    [Fact]
    public void Can_Blit_Empty()
    {
        using var world = new World();
        var query = world.Query<int, string, Vector2, Vector3, Vector4>().Stream();
        query.Blit(123);
        query.Blit("test");
        query.Blit(Vector2.One);
        query.Blit(Vector3.One);
        query.Blit(Vector4.One);
    }

    [Fact]
    public void Can_Blit_All_Components()
    {
        using var world = new World();
        var query = world.Query<int, string, Vector2, Vector3, Vector4>().Stream();

        world.Entity()
            .Add(42)
            .Add("replaceme")
            .Add(Vector2.Zero)
            .Add(Vector3.Zero)
            .Add(Vector4.Zero)
            .Spawn(100);

        query.Blit(69);
        query.Blit("test");
        query.Blit(Vector2.One);
        query.Blit(Vector3.One);
        query.Blit(Vector4.One);

        query.For((ref i, ref s, ref v2, ref v3, ref v4) =>
        {
            Assert.Equal(69, i);
            Assert.Equal("test", s);
            Assert.Equal(Vector2.One, v2);
            Assert.Equal(Vector3.One, v3);
            Assert.Equal(Vector4.One, v4);
        });

        Assert.Equal(100, query.Count);
    }


    [Fact]
    public void For_On_Empty_Query()
    {
        using var world = new World();
        var e = world.Spawn();
        e.Add(Vector4.One);
        e.Add(Vector3.One);
        e.Add(Vector2.One);
        e.Add(42);
        e.Add("test");
        e.Add(0.5f);
        e.Despawn();

        var query1 = world.Query<Vector4>().Stream();
        query1.For((ref _) =>
        {
            Assert.Fail("Should not be called");
        });

        var query2 = world.Query<Vector3, Vector4>().Stream();
        query2.For((ref _, ref _) =>
        {
            Assert.Fail("Should not be called");
        });

        var query3 = world.Query<Vector2, Vector3, Vector4>().Stream();
        query3.For((ref _, ref _, ref _) =>
        {
            Assert.Fail("Should not be called");
        });

        var query4 = world.Query<string, Vector2, Vector3, Vector4>().Stream();
        query4.For((ref _, ref _, ref _, ref _) =>
        {
            Assert.Fail("Should not be called");
        });

        var query5 = world.Query<int, string, Vector2, Vector3, Vector4>().Stream();
        query5.For((ref _, ref _, ref _, ref _, ref _) =>
        {
            Assert.Fail("Should not be called");
        });
    }

    [Fact]
    public void ForE_On_Empty_Query()
    {
        using var world = new World();
        var e = world.Spawn();
        e.Add(Vector4.One);
        e.Add(Vector3.One);
        e.Add(Vector2.One);
        e.Add(42);
        e.Add("test");
        e.Add(0.5f);
        e.Despawn();

        var query1 = world.Query<Vector4>().Stream();
        query1.For((in _, ref _) =>
        {
            Assert.Fail("Should not be called");
        });

        var query2 = world.Query<Vector3, Vector4>().Stream();
        query2.For((in _, ref _, ref _) =>
        {
            Assert.Fail("Should not be called");
        });

        var query3 = world.Query<Vector2, Vector3, Vector4>().Stream();
        query3.For((in _, ref _, ref _, ref _) =>
        {
            Assert.Fail("Should not be called");
        });

        var query4 = world.Query<string, Vector2, Vector3, Vector4>().Stream();
        query4.For((in _, ref _, ref _, ref _, ref _) =>
        {
            Assert.Fail("Should not be called");
        });

        var query5 = world.Query<int, string, Vector2, Vector3, Vector4>().Stream();
        query5.For((in _, ref _, ref _, ref _, ref _, ref _) =>
        {
            Assert.Fail("Should not be called");
        });
    }

    [Fact]
    public void ForU_On_Empty_Query()
    {
        using var world = new World();
        var e = world.Spawn();
        e.Add(Vector4.One);
        e.Add(Vector3.One);
        e.Add(Vector2.One);
        e.Add(42);
        e.Add("test");
        e.Add(0.5f);
        e.Despawn();


        var query1 = world.Query<Vector4>().Stream();
        query1.For(0.0f, (_, ref _) =>
        {
            Assert.Fail("Should not be called");
        });

        var query2 = world.Query<Vector3, Vector4>().Stream();
        query2.For(0.0f,
            static (_, ref _, ref _) =>
            {
                Assert.Fail("Should not be called");
            }
        );

        var query3 = world.Query<Vector2, Vector3, Vector4>().Stream();
        query3.For(0.0f, (_, ref _, ref _, ref _) =>
        {
            Assert.Fail("Should not be called");
        });

        var query4 = world.Query<string, Vector2, Vector3, Vector4>().Stream();
        query4.For(0.0f, (_, ref _, ref _, ref _, ref _) =>
        {
            Assert.Fail("Should not be called");
        });

        var query5 = world.Query<int, string, Vector2, Vector3, Vector4>().Stream();
        query5.For(0.0f, (_, ref _, ref _, ref _, ref _, ref _) =>
        {
            Assert.Fail("Should not be called");
        });
    }

    [Fact]
    public void ForEU_On_Empty_Query()
    {
        using var world = new World();
        var e = world.Spawn();
        e.Add(Vector4.One);
        e.Add(Vector3.One);
        e.Add(Vector2.One);
        e.Add(42);
        e.Add("test");
        e.Add(0.5f);
        e.Despawn();

        var query1 = world.Query<Vector4>().Stream();
        query1.For(0.0f, (_, in _, ref _) =>
        {
            Assert.Fail("Should not be called");
        });

        var query2 = world.Query<Vector3, Vector4>().Stream();
        query2.For(0.0f, (_, in _, ref _, ref _) =>
        {
            Assert.Fail("Should not be called");
        });

        var query3 = world.Query<Vector2, Vector3, Vector4>().Stream();
        query3.For(0.0f, (_, in _, ref _, ref _, ref _) =>
        {
            Assert.Fail("Should not be called");
        });

        var query4 = world.Query<string, Vector2, Vector3, Vector4>().Stream();
        query4.For(0.0f, (_, in _, ref _, ref _, ref _, ref _) =>
        {
            Assert.Fail("Should not be called");
        });

        var query5 = world.Query<int, string, Vector2, Vector3, Vector4>().Stream();
        query5.For(0.0f, (_, in _, ref _, ref _, ref _, ref _, ref _) =>
        {
            Assert.Fail("Should not be called");
        });
    }


    [Fact]
    public void Obsolete_Coverage_Build()
    {
        using var world = new World();
        world.Query<Vector4>().Compile();
        world.Query<Vector3, Vector4>().Compile();
        world.Query<Vector2, Vector3, Vector4>().Compile();
        world.Query<string, Vector2, Vector3, Vector4>().Compile();
        world.Query<int, string, Vector2, Vector3, Vector4>().Compile();
    }


    [Fact]
    public void Queries_Are_In_World()
    {
        using var world = new World();
        var query = world.Query<int>().Compile();

        Assert.Contains(query, world.Queries);
    }

    [Fact]
    public void Dispose_Removes_Query_From_World()
    {
        using var world = new World();
        var query = world.Query<int>().Compile();
        query.Dispose();
        Assert.DoesNotContain(query, world.Queries);
    }

    [Fact]
    public void Cannot_Repeatedly_Dispose_Query()
    {
        using var world = new World();

        var query = world.Query<int>().Compile();
        query.Dispose();
        Assert.Throws<ObjectDisposedException>(query.Dispose);
    }
}
