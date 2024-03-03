using System.Diagnostics.CodeAnalysis;

namespace fennecs.tests.Query;

[Experimental("StatefulFiltering")]
public class ExperimentalStreamFilterTests
{
    [Fact]
    public void Queries_Can_be_Filtered()
    {
        using var world = new World();
        var query = world.Query<Identity, int>(Match.Plain, Match.Any).Build();

        var entity1 = world.Spawn().Add(444);
        var entity2 = world.Spawn().AddRelation(entity1, 555);

        Assert.Contains(entity1, query);
        Assert.Contains(entity2, query);

        query.AddStreamFilter<int>(Match.Plain);

        Assert.Contains(entity1, query);
        Assert.DoesNotContain(entity2, query);
    }


    [Fact]
    public void Query_Filters_can_be_Cleared()
    {
        using var world = new World();
        var query = world.Query<int>(Match.Any).Build();

        var entity1 = world.Spawn().Add(444);
        var entity2 = world.Spawn().AddRelation(entity1, 555);

        query.AddStreamFilter<int>(Match.Plain);
        Assert.Contains(entity1, query);
        Assert.DoesNotContain(entity2, query);

        query.ClearStreamFilter();
        Assert.Contains(entity1, query);
        Assert.Contains(entity2, query);
    }


    [Fact(Skip = "Experimental Stream Filters need rework")]
    public void Can_Set_on_Specific_StreamType_index()
    {
        using var world = new World();
        //Bad practice, never build a query like this in a real world scenario. :)
        var query = world.Query<int, int>(Match.Plain, Match.Target).Build();

        var entity1 = world.Spawn().Add(444);
        var entity2 = world.Spawn().AddRelation(entity1, 666);
        var entity3 = world.Spawn().AddRelation(entity1, 888).Add(123);

        Assert.DoesNotContain(entity1, query);
        Assert.DoesNotContain(entity2, query);
        Assert.Contains(entity3, query);

        query.AddStreamFilter<int>(Match.Plain, 0);
        query.AddStreamFilter<int>(Match.Plain, 1);

        Assert.Contains(entity1, query);
        Assert.DoesNotContain(entity2, query);
    }


    [Fact(Skip = "Experimental Stream Filters need rework")]
    public void Cannot_Set_on_Invalid_StreamType_index()
    {
        using var world = new World();
        //Bad practice, never build a query like this in a real world scenario. :)
        var query = world.Query<int, int, int>(Match.Any, Match.Any, Match.Any).Build();
        Assert.Throws<IndexOutOfRangeException>(() => query.AddStreamFilter<int>(Match.Plain, 3));
    }


    [Fact]
    public void Cannot_Filter_foreign_Component()
    {
        using var world = new World();
        var query = world.Query<float>().Build();
        Assert.Throws<InvalidOperationException>(() => query.AddStreamFilter<int>(Match.Plain));
    }


    [Fact]
    public void Cannot_Widen_Filter()
    {
        using var world = new World();
        var query = world.Query<int>(Match.Plain).Build();
        Assert.Throws<InvalidOperationException>(() => query.AddStreamFilter<int>(Match.Any));
        Assert.Throws<InvalidOperationException>(() => query.AddStreamFilter<int>(Match.Target));
    }


    [Fact]
    public void Cannot_Mismatch_Filter()
    {
        using var world = new World();
        var query = world.Query<int>(Match.Plain).Build();
        Assert.Throws<InvalidOperationException>(() => query.AddStreamFilter<int>(Match.Target));
        Assert.Throws<InvalidOperationException>(() => query.AddStreamFilter<int>(Match.Entity));
        Assert.Throws<InvalidOperationException>(() => query.AddStreamFilter<int>(Match.Object));
    }
}