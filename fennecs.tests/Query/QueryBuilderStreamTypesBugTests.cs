namespace fennecs.tests.Query;

/// <summary>
/// Tests to ensure that QueryBuilder&lt;C1, ..., Cn&gt; correctly requires ALL stream type
/// components in the query mask. These tests verify that entities missing any of the
/// stream type components are correctly excluded from query results.
/// 
/// A past bug in QueryBuilder&lt;C1,C2,C3,C4,C5&gt; had C1 missing from the streamTypes array,
/// causing entities without C1 to incorrectly match the query.
/// </summary>
public class QueryBuilderStreamTypesBugTests
{
    #region QueryBuilder<C1> Tests (Arity 1)

    [Fact]
    public void QueryBuilder1_Should_Require_C1_Component()
    {
        using var world = new World();
        
        // Entity missing C1
        var entityMissingC1 = world.Spawn()
            .Add("decoy");
        
        // Entity with C1
        var entityWithAll = world.Spawn()
            .Add(42);  // C1 - int
        
        using var builder = world.Query<int>();
        var query = builder.Compile();
        
        Assert.Equal(1, query.Count);
        Assert.Contains(entityWithAll, query);
        Assert.DoesNotContain(entityMissingC1, query);
    }

    [Fact]
    public void Stream1_Count_Should_Only_Include_Entities_With_All_Components()
    {
        using var world = new World();
        
        // Create 3 entities missing C1
        for (var i = 0; i < 3; i++)
            world.Spawn().Add($"decoy-{i}");
        
        // Create 2 entities with C1
        for (var i = 0; i < 2; i++)
            world.Spawn().Add(i);  // C1 - int
        
        var stream = world.Query<int>().Stream();
        
        Assert.Equal(2, stream.Count);
    }

    [Fact]
    public void Stream1_For_Should_Only_Iterate_Entities_With_All_Components()
    {
        using var world = new World();
        
        world.Spawn().Add("decoy");
        world.Spawn().Add(42);
        
        var stream = world.Query<int>().Stream();
        var iterationCount = 0;
        var foundValue = 0;
        
        stream.For((ref int c1) =>
        {
            iterationCount++;
            foundValue = c1;
        });
        
        Assert.Equal(1, iterationCount);
        Assert.Equal(42, foundValue);
    }

    #endregion

    #region QueryBuilder<C1, C2> Tests (Arity 2)

    [Fact]
    public void QueryBuilder2_Should_Require_C1_Component()
    {
        using var world = new World();
        
        // Entity with only C2 (missing C1)
        var entityMissingC1 = world.Spawn()
            .Add("C2-only");
        
        // Entity with only C1 (missing C2)
        var entityMissingC2 = world.Spawn()
            .Add(99);
        
        // Entity with both C1 and C2
        var entityWithAll = world.Spawn()
            .Add(42)           // C1 - int
            .Add("has-both");  // C2 - string
        
        using var builder = world.Query<int, string>();
        var query = builder.Compile();
        
        Assert.Equal(1, query.Count);
        Assert.Contains(entityWithAll, query);
        Assert.DoesNotContain(entityMissingC1, query);
        Assert.DoesNotContain(entityMissingC2, query);
    }

    [Fact]
    public void Stream2_Count_Should_Only_Include_Entities_With_All_Components()
    {
        using var world = new World();
        
        // Entities missing C1
        for (var i = 0; i < 2; i++)
            world.Spawn().Add($"missing-c1-{i}");
        
        // Entities missing C2
        for (var i = 0; i < 2; i++)
            world.Spawn().Add(i);
        
        // Entities with all components
        for (var i = 0; i < 3; i++)
            world.Spawn().Add(i).Add($"has-all-{i}");
        
        var stream = world.Query<int, string>().Stream();
        
        Assert.Equal(3, stream.Count);
    }

    [Fact]
    public void Stream2_For_Should_Only_Iterate_Entities_With_All_Components()
    {
        using var world = new World();
        
        world.Spawn().Add("missing-c1");
        world.Spawn().Add(99);
        world.Spawn().Add(42).Add("has-both");
        
        var stream = world.Query<int, string>().Stream();
        var iterationCount = 0;
        var foundC1 = 0;
        var foundC2 = "";
        
        stream.For((ref int c1, ref string c2) =>
        {
            iterationCount++;
            foundC1 = c1;
            foundC2 = c2;
        });
        
        Assert.Equal(1, iterationCount);
        Assert.Equal(42, foundC1);
        Assert.Equal("has-both", foundC2);
    }

    #endregion

    #region QueryBuilder<C1, C2, C3> Tests (Arity 3)

    [Fact]
    public void QueryBuilder3_Should_Require_All_Components()
    {
        using var world = new World();
        
        // Entity missing C1
        var entityMissingC1 = world.Spawn()
            .Add("C2")
            .Add(3.0f);
        
        // Entity missing C2
        var entityMissingC2 = world.Spawn()
            .Add(1)
            .Add(3.0f);
        
        // Entity missing C3
        var entityMissingC3 = world.Spawn()
            .Add(1)
            .Add("C2");
        
        // Entity with all 3 components
        var entityWithAll = world.Spawn()
            .Add(42)        // C1 - int
            .Add("C2")      // C2 - string
            .Add(3.14f);    // C3 - float
        
        using var builder = world.Query<int, string, float>();
        var query = builder.Compile();
        
        Assert.Equal(1, query.Count);
        Assert.Contains(entityWithAll, query);
        Assert.DoesNotContain(entityMissingC1, query);
        Assert.DoesNotContain(entityMissingC2, query);
        Assert.DoesNotContain(entityMissingC3, query);
    }

    [Fact]
    public void Stream3_Count_Should_Only_Include_Entities_With_All_Components()
    {
        using var world = new World();
        
        // Create entities missing various components
        world.Spawn().Add("c2").Add(3.0f);           // missing C1
        world.Spawn().Add(1).Add(3.0f);              // missing C2
        world.Spawn().Add(1).Add("c2");              // missing C3
        
        // Create 2 entities with all components
        for (var i = 0; i < 2; i++)
            world.Spawn().Add(i).Add($"c2-{i}").Add((float)i);
        
        var stream = world.Query<int, string, float>().Stream();
        
        Assert.Equal(2, stream.Count);
    }

    [Fact]
    public void Stream3_For_Should_Only_Iterate_Entities_With_All_Components()
    {
        using var world = new World();
        
        world.Spawn().Add("c2").Add(3.0f);
        world.Spawn().Add(1).Add(3.0f);
        world.Spawn().Add(42).Add("found").Add(3.14f);
        
        var stream = world.Query<int, string, float>().Stream();
        var iterationCount = 0;
        var foundC1 = 0;
        var foundC2 = "";
        var foundC3 = 0f;
        
        stream.For((ref int c1, ref string c2, ref float c3) =>
        {
            iterationCount++;
            foundC1 = c1;
            foundC2 = c2;
            foundC3 = c3;
        });
        
        Assert.Equal(1, iterationCount);
        Assert.Equal(42, foundC1);
        Assert.Equal("found", foundC2);
        Assert.Equal(3.14f, foundC3);
    }

    #endregion

    #region QueryBuilder<C1, C2, C3, C4> Tests (Arity 4)

    [Fact]
    public void QueryBuilder4_Should_Require_All_Components()
    {
        using var world = new World();
        
        // Entity missing C1
        var entityMissingC1 = world.Spawn()
            .Add("C2").Add(3.0f).Add(4.0);
        
        // Entity missing C2
        var entityMissingC2 = world.Spawn()
            .Add(1).Add(3.0f).Add(4.0);
        
        // Entity missing C3
        var entityMissingC3 = world.Spawn()
            .Add(1).Add("C2").Add(4.0);
        
        // Entity missing C4
        var entityMissingC4 = world.Spawn()
            .Add(1).Add("C2").Add(3.0f);
        
        // Entity with all 4 components
        var entityWithAll = world.Spawn()
            .Add(42)        // C1 - int
            .Add("C2")      // C2 - string
            .Add(3.14f)     // C3 - float
            .Add(4.0);      // C4 - double
        
        using var builder = world.Query<int, string, float, double>();
        var query = builder.Compile();
        
        Assert.Equal(1, query.Count);
        Assert.Contains(entityWithAll, query);
        Assert.DoesNotContain(entityMissingC1, query);
        Assert.DoesNotContain(entityMissingC2, query);
        Assert.DoesNotContain(entityMissingC3, query);
        Assert.DoesNotContain(entityMissingC4, query);
    }

    [Fact]
    public void Stream4_Count_Should_Only_Include_Entities_With_All_Components()
    {
        using var world = new World();
        
        // Create entities missing various components
        world.Spawn().Add("c2").Add(3.0f).Add(4.0);    // missing C1
        world.Spawn().Add(1).Add(3.0f).Add(4.0);       // missing C2
        world.Spawn().Add(1).Add("c2").Add(4.0);       // missing C3
        world.Spawn().Add(1).Add("c2").Add(3.0f);      // missing C4
        
        // Create 2 entities with all components
        for (var i = 0; i < 2; i++)
            world.Spawn().Add(i).Add($"c2-{i}").Add((float)i).Add((double)i);
        
        var stream = world.Query<int, string, float, double>().Stream();
        
        Assert.Equal(2, stream.Count);
    }

    [Fact]
    public void Stream4_For_Should_Only_Iterate_Entities_With_All_Components()
    {
        using var world = new World();
        
        world.Spawn().Add("c2").Add(3.0f).Add(4.0);
        world.Spawn().Add(1).Add(3.0f).Add(4.0);
        world.Spawn().Add(42).Add("found").Add(3.14f).Add(4.2);
        
        var stream = world.Query<int, string, float, double>().Stream();
        var iterationCount = 0;
        var foundC1 = 0;
        var foundC2 = "";
        var foundC3 = 0f;
        var foundC4 = 0.0;
        
        stream.For((ref int c1, ref string c2, ref float c3, ref double c4) =>
        {
            iterationCount++;
            foundC1 = c1;
            foundC2 = c2;
            foundC3 = c3;
            foundC4 = c4;
        });
        
        Assert.Equal(1, iterationCount);
        Assert.Equal(42, foundC1);
        Assert.Equal("found", foundC2);
        Assert.Equal(3.14f, foundC3);
        Assert.Equal(4.2, foundC4);
    }

    #endregion

    #region QueryBuilder<C1, C2, C3, C4, C5> Tests (Arity 5)

    [Fact]
    public void QueryBuilder5_Should_Require_All_Components()
    {
        using var world = new World();
        
        // Entity missing C1
        var entityMissingC1 = world.Spawn()
            .Add("C2").Add(3.0f).Add(4.0).Add(5L);
        
        // Entity missing C2
        var entityMissingC2 = world.Spawn()
            .Add(1).Add(3.0f).Add(4.0).Add(5L);
        
        // Entity missing C3
        var entityMissingC3 = world.Spawn()
            .Add(1).Add("C2").Add(4.0).Add(5L);
        
        // Entity missing C4
        var entityMissingC4 = world.Spawn()
            .Add(1).Add("C2").Add(3.0f).Add(5L);
        
        // Entity missing C5
        var entityMissingC5 = world.Spawn()
            .Add(1).Add("C2").Add(3.0f).Add(4.0);
        
        // Entity with all 5 components
        var entityWithAll = world.Spawn()
            .Add(42)        // C1 - int
            .Add("C2")      // C2 - string
            .Add(3.14f)     // C3 - float
            .Add(4.0)       // C4 - double
            .Add(5L);       // C5 - long
        
        using var builder = world.Query<int, string, float, double, long>();
        var query = builder.Compile();
        
        Assert.Equal(1, query.Count);
        Assert.Contains(entityWithAll, query);
        Assert.DoesNotContain(entityMissingC1, query);
        Assert.DoesNotContain(entityMissingC2, query);
        Assert.DoesNotContain(entityMissingC3, query);
        Assert.DoesNotContain(entityMissingC4, query);
        Assert.DoesNotContain(entityMissingC5, query);
    }

    [Fact]
    public void Stream5_Count_Should_Only_Include_Entities_With_All_Components()
    {
        using var world = new World();
        
        // Create entities missing various components
        world.Spawn().Add("c2").Add(3.0f).Add(4.0).Add(5L);     // missing C1
        world.Spawn().Add(1).Add(3.0f).Add(4.0).Add(5L);        // missing C2
        world.Spawn().Add(1).Add("c2").Add(4.0).Add(5L);        // missing C3
        world.Spawn().Add(1).Add("c2").Add(3.0f).Add(5L);       // missing C4
        world.Spawn().Add(1).Add("c2").Add(3.0f).Add(4.0);      // missing C5
        
        // Create 2 entities with all components
        for (var i = 0; i < 2; i++)
            world.Spawn().Add(i).Add($"c2-{i}").Add((float)i).Add((double)i).Add((long)i);
        
        var stream = world.Query<int, string, float, double, long>().Stream();
        
        Assert.Equal(2, stream.Count);
    }

    [Fact]
    public void Stream5_For_Should_Only_Iterate_Entities_With_All_Components()
    {
        using var world = new World();
        
        world.Spawn().Add("c2").Add(3.0f).Add(4.0).Add(5L);
        world.Spawn().Add(1).Add(3.0f).Add(4.0).Add(5L);
        world.Spawn().Add(42).Add("found").Add(3.14f).Add(4.2).Add(5L);
        
        var stream = world.Query<int, string, float, double, long>().Stream();
        var iterationCount = 0;
        var foundC1 = 0;
        var foundC2 = "";
        var foundC3 = 0f;
        var foundC4 = 0.0;
        var foundC5 = 0L;
        
        stream.For((ref int c1, ref string c2, ref float c3, ref double c4, ref long c5) =>
        {
            iterationCount++;
            foundC1 = c1;
            foundC2 = c2;
            foundC3 = c3;
            foundC4 = c4;
            foundC5 = c5;
        });
        
        Assert.Equal(1, iterationCount);
        Assert.Equal(42, foundC1);
        Assert.Equal("found", foundC2);
        Assert.Equal(3.14f, foundC3);
        Assert.Equal(4.2, foundC4);
        Assert.Equal(5L, foundC5);
    }

    #endregion
}
