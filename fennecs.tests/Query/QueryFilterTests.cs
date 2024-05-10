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
        _query = _world.Query().Build();
    }

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
    public void AddFilter_WithInheritors_ShouldWorkCorrectly()
    {
        // Arrange
        var query1 = _world.Query<ComponentA>().Build();
        var query2 = _world.Query<ComponentA, ComponentB>().Build();
        // ... up to query5 for Query<ComponentA, ComponentB, ComponentC, ComponentD, ComponentE>

        var entity = _world.Spawn().Add(new ComponentA()).Add(new ComponentB());
        // ... add more components as needed for the test

        // Act
        query1.Subset<ComponentA>(Match.Plain);
        query2.Subset<ComponentB>(Match.Plain);
        // ... apply filters to other queries

        // Assert
        Assert.Single(query1.ToList());
        Assert.Single(query2.ToList());
        // ... assert other queries
    }

    // Additional tests for other inheritors of Query...
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