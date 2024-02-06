using System.Numerics;

namespace fennecs.tests.Integration;

public static class QueryTests
{
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
        var bob = world.Spawn().Add(p2).Add(alice, 111).Id();
        /*var charlie = */world.Spawn().Add(p3).Add(bob, 222).Id();

        var query = world.Query<Entity, Vector3>()
            .Any<int>(Identity.None)
            .Build();

        var count = 0;
        query.Raw((me, mp) =>
        {
            count++;
            Assert.Equal(1, mp.Length);
            var entity = me.Span[0];
            Assert.Equal(alice, entity);
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
        var eve = world.Spawn().Add(p2).Add(alice, 111).Id();
        var charlie = world.Spawn().Add(p3).Add(eve, 222).Id();

        var query = world.Query<Entity, Vector3>().Any<int>(eve).Build();

        var count = 0;
        query.Raw((me, mp) =>
        {
            count++;
            Assert.Equal(1, mp.Length);
            var entity = me.Span[0];
            Assert.Equal(charlie, entity);
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
        var eve = world.Spawn().Add(p2).Add(alice, 111).Id();
        var charlie = world.Spawn().Add(p3).Add(eve, 222).Id();

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
                var entity = me.Span[index];
                count++;
                if (entity == charlie)
                {
                    var pos = mp.Span[index];
                    Assert.Equal(pos, p3);
                }
                else if (entity == eve)
                {
                    var pos = mp.Span[index];
                    Assert.Equal(pos, p2);
                }
                else
                {
                    Assert.Fail("Unexpected entity");
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
        var bob = world.Spawn().Add(p2).Add(alice, 111).Id();
        var eve = world.Spawn().Add(p1).Add(888).Id();

        /*var charlie = */
        world.Spawn().Add(p3).Add(bob, 222).Id();
        /*var charlie = */
        world.Spawn().Add(p3).Add(eve, 222).Id();

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
                var entity = me.Span[index];
                count++;
                if (entity == bob)
                {
                    var pos = mp.Span[index];
                    Assert.Equal(pos, p2);
                }
                else
                {
                    Assert.Fail("Unexpected entity");
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

        var bob = world.Spawn().Add(p2).Add(alice, 111).Id();

        world.Spawn().Add(p3).Add(bob, 555).Id();
        world.Spawn().Add(p3).Add(eve, 666).Id();

        var query = world.Query<Entity, Vector3, int>()
            .Not<int>(bob)
            .Build();

        var count = 0;
        query.Raw((me, mp, mi) =>
        {
            Assert.Equal(2, mp.Length);
            for (var index = 0; index < me.Length; index++)
            {
                var entity = me.Span[index];
                count++;
                
                if (entity == alice)
                {
                    var pos = mp.Span[0];
                    Assert.Equal(pos, p1);
                    var integer = mi.Span[index];
                    Assert.Equal(0, integer);
                }
                else if (entity == eve)
                {
                    var pos = mp.Span[index];
                    Assert.Equal(pos, p1);
                    var i = mi.Span[index];
                    Assert.Equal(888, i);
                }
                else
                {
                    Assert.Fail($"Unexpected entity {entity}");
                }
            }
        });
        Assert.Equal(2, count);
    }
}