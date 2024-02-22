using System.Collections;
using System.Numerics;

namespace fennecs.tests.Integration;

public class QueryTests
{
    [Fact]
    private void CrossJoin_Counts_All()
    {
        int[] counter = [0, 0, 0];
        int[] limiter = [9, 5, 3];

        var count = 0;
        do
        {
            count++;
        } while (Query.CrossJoin(counter, limiter));
        
        Assert.Equal(9*5*3, count);
    }
    
    [Fact]
    private static void Can_Enumerate_PlainEnumerator()
    {
        using var world = new World();

        var entities = new List<Entity>();
        for (var i = 0; i < 234; i++)
        {
            var identity = world.Spawn().Add(new object()).Id();
            entities.Add(identity);
        }
        
        var query = world.Query<object>().Build();
        var plain = query as IEnumerable;
        
        var enumerator = plain.GetEnumerator();
        using var disposable = enumerator as IDisposable;
        while (enumerator.MoveNext())
        {
            Assert.IsType<Entity>(enumerator.Current);
            
            var identity = (Entity) enumerator.Current;
            Assert.Contains(identity, entities);
            entities.Remove(identity);            
        }
        Assert.Empty(entities);
    }

    [Fact]
    private static void Contains_Finds_Entity()
    {
        using var world = new World();

        var random = new Random(1234);
        var entities = new List<Entity>();
        for (var i = 0; i < 2345; i++)
        {
            var identity = world.Spawn().Add(i).Id();
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
    private static void Has_Matches()
    {
        var p1 = new Vector3(6, 6, 6);
        var p2 = new Vector3(1, 2, 3);

        using var world = new World();
        world.Spawn().Add(p1).Id();
        world.Spawn().Add(p2).Add<int>().Id();
        world.Spawn().Add(p2).Add<int>().Id();

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
    private static void Not_prevents_Match()
    {
        var p1 = new Vector3(6, 6, 6);
        var p2 = new Vector3(1, 2, 3);

        using var world = new World();
        world.Spawn().Add(p1).Id();
        world.Spawn().Add(p2).Add<int>().Id();
        world.Spawn().Add(p2).Add<int>().Id();

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
    private static void Any_Target_None_Matches_Only_None()
    {
        var p1 = new Vector3(6, 6, 6);
        var p2 = new Vector3(1, 2, 3);
        var p3 = new Vector3(4, 4, 4);

        using var world = new World();
        var alice = world.Spawn().Add(p1).Add(0).Id();
        var bob = world.Spawn().Add(p2).Link(alice, 111).Id();
        /*var charlie = */world.Spawn().Add(p3).Link(bob, 222).Id();

        var query = world.Query<Entity, Vector3>()
            .Any<int>(Entity.None)
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
    private static void Any_Target_Single_Matches()
    {
        var p1 = new Vector3(6, 6, 6);
        var p2 = new Vector3(1, 2, 3);
        var p3 = new Vector3(4, 4, 4);

        using var world = new World();
        var alice = world.Spawn().Add(p1).Add(0).Id();
        var eve = world.Spawn().Add(p2).Link(alice, 111).Id();
        var charlie = world.Spawn().Add(p3).Link(eve, 222).Id();

        var query = world.Query<Entity, Vector3>().Any<int>(eve).Build();

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
    private static void Any_Target_Multiple_Matches()
    {
        var p1 = new Vector3(6, 6, 6);
        var p2 = new Vector3(1, 2, 3);
        var p3 = new Vector3(4, 4, 4);

        using var world = new World();
        var alice = world.Spawn().Add(p1).Add(0).Id();
        var eve = world.Spawn().Add(p2).Link(alice, 111).Id();
        var charlie = world.Spawn().Add(p3).Link(eve, 222).Id();

        var query = world.Query<Entity, Vector3>()
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
    private static void Any_Not_does_not_Match_Specific()
    {
        var p1 = new Vector3(6, 6, 6);
        var p2 = new Vector3(1, 2, 3);
        var p3 = new Vector3(4, 4, 4);

        using var world = new World();
        var alice = world.Spawn().Add(p1).Add(0).Id();
        var bob = world.Spawn().Add(p2).Link(alice, 111).Id();
        var eve = world.Spawn().Add(p1).Add(888).Id();

        /*var charlie = */
        world.Spawn().Add(p3).Link(bob, 222).Id();
        /*var charlie = */
        world.Spawn().Add(p3).Link(eve, 222).Id();

        var query = world.Query<Entity, Vector3>()
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
    private static void Query_provided_Has_works_with_Target()
    {
        var p1 = new Vector3(6, 6, 6);
        var p2 = new Vector3(1, 2, 3);
        var p3 = new Vector3(4, 4, 4);

        using var world = new World();

        var alice = world.Spawn().Add(p1).Add(0).Id();
        var eve = world.Spawn().Add(p1).Add(888).Id();

        var bob = world.Spawn().Add(p2).Link(alice, 111).Id();

        world.Spawn().Add(p3).Link(bob, 555).Id();
        world.Spawn().Add(p3).Link(eve, 666).Id();

        var query = world.Query<Entity, Vector3, int>()
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
    private static void Queries_are_Cached()
    {
        using var world = new World();

        world.Spawn().Add(123);

        var query1A = world.Query().Build();
        var query1B = world.Query().Build();

        var query2A = world.Query<Entity>().Build();
        var query2B = world.Query<Entity>().Build();

        var query3A = world.Query().Has<int>().Build();
        var query3B = world.Query().Has<int>().Build();

        var query4A = world.Query<Entity>().Not<int>().Build();
        var query4B = world.Query<Entity>().Not<int>().Build();

        var query5A = world.Query<Entity>().Any<int>().Any<float>().Build();
        var query5B = world.Query<Entity>().Any<int>().Any<float>().Build();

        Assert.True(ReferenceEquals(query1A, query1B));
        Assert.True(ReferenceEquals(query2A, query2B));
        Assert.True(ReferenceEquals(query3A, query3B));
        Assert.True(ReferenceEquals(query4A, query4B));
        Assert.True(ReferenceEquals(query5A, query5B));
    }

    
    [Fact]
    private static void Queries_are_Disposable()
    {
        using var world = new World();

        var query = world.Query().Build();
        query.Dispose();
        Assert.Throws<ObjectDisposedException>(() => query.Raw(memory => { }));
        Assert.Throws<ObjectDisposedException>(() =>
        {
            foreach (var _ in query)
            {
                Assert.Fail("Should not enumerate disposed Query.");
            }
        });
    }


    [Fact]
    private void Ref_disallows_Component_Type_Entity()
    {
        using var world = new World();
        var identity = world.Spawn().Id();
        var query = world.Query<Entity>().Build();

        Assert.Throws<TypeAccessException>(() => query.Ref<Entity>(identity));
    }

    
    [Fact]
    private void Ref_disallows_Dead_Entity()
    {
        using var world = new World();
        var identity = world.Spawn().Add<int>().Id();
        world.Despawn(identity);
        Assert.False(world.IsAlive(identity));

        var query = world.Query<int>().Build();
        Assert.Throws<ObjectDisposedException>(() => query.Ref<int>(identity));
    }

    [Fact]
    private void Ref_disallows_Nonexistent_Component()
    {
        using var world = new World();
        var identity = world.Spawn().Add<int>().Id();

        var query = world.Query<int>().Build();
        Assert.Throws<KeyNotFoundException>(() => query.Ref<float>(identity));
    }

    [Fact]
    private void Ref_gets_Mutable_Component()
    {
        using var world = new World();
        var identity = world.Spawn().Add(23).Id();
        var query = world.Query<int>().Build();

        ref var gotten = ref query.Ref<int>(identity);
        Assert.Equal(23, gotten);

        // Identity can't be a ref (is readonly - make sure!)
        gotten = 42;
        Assert.Equal(42, query.Ref<int>(identity));
    }
}