namespace fennecs.tests;

public class QueryFilterTests
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

        query.AddFilter<int>(Match.Plain);

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

        query.AddFilter<int>(Match.Plain);
        Assert.Contains(entity1, query);
        Assert.DoesNotContain(entity2, query);

        query.ClearFilters();
        Assert.Contains(entity1, query);
        Assert.Contains(entity2, query);
    }


    [Fact]
    public void Cannot_Filter_foreign_Component()
    {
        using var world = new World();
        var query = world.Query<float>().Build();
        Assert.Throws<InvalidOperationException>(() => query.AddFilter<int>(Match.Plain));
    }


    [Fact]
    public void Cannot_Widen_Filter()
    {
        using var world = new World();
        var query = world.Query<int>(Match.Plain).Build();
        Assert.Throws<InvalidOperationException>(() => query.AddFilter<int>(Match.Any));
        Assert.Throws<InvalidOperationException>(() => query.AddFilter<int>(Match.Target));
    }


    [Fact]
    public void Cannot_Mismatch_Filter()
    {
        using var world = new World();
        var query = world.Query<int>(Match.Plain).Build();
        Assert.Throws<InvalidOperationException>(() => query.AddFilter<int>(Match.Target));
        Assert.Throws<InvalidOperationException>(() => query.AddFilter<int>(Match.Entity));
        Assert.Throws<InvalidOperationException>(() => query.AddFilter<int>(Match.Object));
    }


    [Fact]
    public void Filtered_Enumerator()
    {
        using var world = new World();
        var query = world.Query<Identity, int>(Match.Plain, Match.Any).Build();

        var entity1 = world.Spawn().Add(444);
        var entity2 = world.Spawn().AddRelation(entity1, 555);

        var tx = TypeExpression.Of<int>(Match.Plain);
        Assert.Contains(entity1, query.Filtered(tx));
        Assert.DoesNotContain(entity2, query.Filtered(tx));
    }
}