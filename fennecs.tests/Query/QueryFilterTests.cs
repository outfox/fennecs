namespace fennecs.tests.Query;

using fennecs;

public class QueryFilterTests
{
    private readonly World _world;
    private readonly Stream<ComponentA> _query;

    public QueryFilterTests()
    {
        _world = new World();
        // Assuming that the World class has a method to create queries.
        // Replace with the actual method to create a Query instance.
        _query = _world.Query<ComponentA>().Stream();
    }

    
    [Fact]
    public void Subset_ShouldNarrowDownResults()
    {
        // Arrange
        var entity1 = _world.Spawn().Add(new ComponentA());
        var entity2 = _world.Spawn().Add(new ComponentB());
        var entity3 = _world.Spawn().Add(new ComponentA()).Add(new ComponentB());

        // Act
        var query2 = _query with { Subset = [Comp<ComponentA>.Plain] }; 

        var results = query2.ToList().Select(r => r.Item1).ToArray();
        
        // Assert
        Assert.Contains(entity1, results);
        Assert.DoesNotContain(entity2, results);
        Assert.Contains(entity3, results);

        //Ensure count is reduced
        Assert.Equal(2, results.Length);
    }

    [Fact]
    public void Exclude_ShouldNarrowDownResults()
    {
        // Arrange
        var entity1 = _world.Spawn().Add(new ComponentA());
        var entity2 = _world.Spawn().Add(new ComponentB());
        var entity3 = _world.Spawn().Add(new ComponentA()).Add(new ComponentB());

        // Act
        var query2 = _query with { Exclude = [Comp<ComponentB>.Plain] }; 

        var results = query2.Select(r => r.Item1).ToArray();
        
        // Assert
        Assert.Contains(entity1, results);
        Assert.DoesNotContain(entity2, results);
        Assert.DoesNotContain(entity3, results);
        
        //Ensure count is reduced
        Assert.Single(results);
    }
    
    [Fact]
    public void Exclude_ShouldNarrowDownResults_EntityAny()
    {
        using var world = new World();
        
        // Arrange
        var target = world.Spawn();
        var entity1 = world.Spawn().Add(new ComponentA());
        var entity2 = world.Spawn().Add(new ComponentA()).Add(new ComponentB(), target);

        var stream = world.Query<ComponentA>().Stream();

        // Act
        var filtered = stream with
        {
            Exclude = [Comp<ComponentB>.Matching(Match.Entity)]
        };

        var results = new List<Entity>();
        filtered.For((in entity, ref _) => results.Add(entity));
        
        // Assert
        Assert.Contains(entity1, results);
        Assert.DoesNotContain(entity2, results);
        
        //Ensure count is reduced
        Assert.Single(results);
    }

    [Fact]
    public void Exclude_ShouldNarrowDownResults_MatchAny()
    {
        using var world = new World();
        
        // Arrange
        var target = world.Spawn();
        var entity1 = world.Spawn().Add(new ComponentA());
        var entity2 = world.Spawn().Add(new ComponentA()).Add(new ComponentB(), target);

        var stream = world.Query<ComponentA>().Stream();

        // Act
        var filtered = stream with
        {
            Exclude = [Comp<ComponentB>.Matching(Match.Any)]
        };

        var results = new List<Entity>();
        filtered.For((in entity, ref _) => results.Add(entity));
        
        // Assert
        Assert.Contains(entity1, results);
        Assert.DoesNotContain(entity2, results);
        
        //Ensure count is reduced
        Assert.Single(results);
    }    
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