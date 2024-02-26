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

        query.SetFilter<int>(Match.Plain);

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

        query.SetFilter<int>(Match.Plain);
        Assert.Contains(entity1, query);
        Assert.DoesNotContain(entity2, query);

        query.ClearFilters();
        Assert.Contains(entity1, query);
        Assert.Contains(entity2, query);
    }
}