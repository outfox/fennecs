namespace fennecs.tests.Query;

using fennecs;

public class QueryFilterTests
{
    private readonly World _world;
    private readonly Query _query;

    public QueryFilterTests()
    {
        _world = new World();
        // Assuming that the World class has a method to create queries.
        // Replace with the actual method to create a Query instance.
        _query = _world.Query().Compile();
    }

    /*
    [Fact]
    public void AddFilter_ShouldNarrowDownResults()
    {
        // Arrange
        var entity1 = _world.Spawn().Add(new ComponentA());
        var entity2 = _world.Spawn().Add(new ComponentB());
        var entity3 = _world.Spawn().Add(new ComponentA()).Add(new ComponentB());

        // Act
        _query.Subset<ComponentA>(Match.Plain);
        var results = _query.ToList();

        // Assert
        Assert.Contains(entity1, results);
        Assert.DoesNotContain(entity2, results);
        Assert.Contains(entity3, results);
    }

    [Fact]
    public void ClearStreamFilter_ShouldResetQuery()
    {
        // Arrange
        var entity1 = _world.Spawn().Add(new ComponentA());
        var entity2 = _world.Spawn().Add(new ComponentB());
        var entity3 = _world.Spawn().Add(new ComponentA()).Add(new ComponentB());

        _query.Subset<ComponentA>(Match.Plain);

        // Act
        _query.ClearFilters();
        var results = _query.ToList();

        // Assert
        Assert.Contains(entity1, results);
        Assert.Contains(entity2, results);
        Assert.Contains(entity3, results);
    }

    [Fact]
    public void Subset_WithInheritors_ShouldWorkCorrectly()
    {
        // Arrange
        var query1 = _world.Query<ComponentA>().Stream();
        var query2 = _world.Query<ComponentA, ComponentB>().Stream();
        // ... up to query5 for Query<ComponentA, ComponentB, ComponentC, ComponentD, ComponentE>

        var entity = _world.Spawn().Add(new ComponentA()).Add(new ComponentB());
        // ... add more components as needed for the test

        // Act
        query1.Query.Subset<ComponentA>(Match.Plain);
        query2.Query.Subset<ComponentB>(Match.Plain);
        // ... apply filters to other queries

        // Assert
        Assert.Single(query1.Query.ToList());
        Assert.Single(query2.Query.ToList());
        // ... assert other queries
    }

    [Fact]
    public void Exclude_WithInheritors_ShouldWorkCorrectly()
    {
        // Arrange
        var query1 = _world.Query<ComponentA>().Stream();
        var query2 = _world.Query<ComponentA, ComponentB>().Stream();
        // ... up to query5 for Query<ComponentA, ComponentB, ComponentC, ComponentD, ComponentE>

        var entity = _world.Spawn().Add(new ComponentA()).Add(new ComponentB());
        // ... add more components as needed for the test

        // Act
        query1.Query.Exclude<ComponentA>(Match.Plain);
        query2.Query.Exclude<ComponentB>(Match.Plain);
        // ... apply filters to other queries

        // Assert
        Assert.Empty(query1.Query.ToList());
        Assert.Empty(query2.Query.ToList());
        // ... assert other queries
    }
*/
}


public struct ComponentA
{
    // Add properties or fields relevant to the component here
    // For testing purposes, it can be left empty
}


public struct ComponentB
{
    // Add properties or fields relevant to the component here
    // For testing purposes, it can be left empty
}


public struct ComponentC
{
    // Add properties or fields relevant to the component here
    // For testing purposes, it can be left empty
}


public struct ComponentD
{
    // Add properties or fields relevant to the component here
    // For testing purposes, it can be left empty
}


public struct ComponentE
{
    // Add properties or fields relevant to the component here
    // For testing purposes, it can be left empty
}