namespace fennecs.tests.Query;

/// <summary>
/// Tests to ensure that QueryBuilder&lt;C1, ..., Cn&gt; correctly requires ALL stream type
/// components in the query mask. These tests verify that entities missing any combination
/// of the stream type components are correctly excluded from query results.
/// 
/// A past bug in QueryBuilder&lt;C1,C2,C3,C4,C5&gt; had C1 missing from the streamTypes array,
/// causing entities without C1 to incorrectly match the query.
/// 
/// These tests exhaustively check all 2^n combinations of component presence for each arity,
/// ensuring only entities with ALL required components match.
/// </summary>
public class QueryBuilderStreamTypesBugTests
{
    #region QueryBuilder<C1> Tests (Arity 1) - 2 combinations

    [Theory]
    [InlineData(false, false)]  // No components - should not match
    [InlineData(true, true)]    // C1 only - should match
    public void QueryBuilder1_Matches_Only_Entities_With_All_Components(bool hasC1, bool shouldMatch)
    {
        using var world = new World();
        
        var entity = world.Spawn();
        if (hasC1) entity.Add(42);  // C1 - int
        
        // Add a decoy component to ensure entity exists in some archetype
        if (!hasC1) entity.Add('x');  // char as decoy
        
        using var builder = world.Query<int>();
        var query = builder.Compile();
        
        if (shouldMatch)
        {
            Assert.Single(query);
            Assert.Contains(entity, query);
        }
        else
        {
            Assert.DoesNotContain(entity, query);
        }
    }

    [Fact]
    public void QueryBuilder1_Matches_Entities_With_Extra_Components()
    {
        using var world = new World();
        
        // Entity with C1 plus extra components should still match
        var entity = world.Spawn()
            .Add(42)       // C1 - int (required)
            .Add("extra")  // Extra component
            .Add(3.14f);   // Another extra component
        
        using var builder = world.Query<int>();
        var query = builder.Compile();
        
        Assert.Single(query);
        Assert.Contains(entity, query);
    }

    [Fact]
    public void Stream1_For_Only_Iterates_Entities_With_All_Components()
    {
        using var world = new World();
        
        // Create entity without C1
        world.Spawn().Add('x');
        
        // Create entity with C1
        world.Spawn().Add(42);
        
        var stream = world.Query<int>().Stream();
        var count = 0;
        var foundValue = 0;
        
        stream.For((ref int c1) =>
        {
            count++;
            foundValue = c1;
        });
        
        Assert.Equal(1, count);
        Assert.Equal(42, foundValue);
    }

    #endregion

    #region QueryBuilder<C1, C2> Tests (Arity 2) - 4 combinations

    [Theory]
    [InlineData(false, false, false)]  // None - should not match
    [InlineData(true, false, false)]   // C1 only - should not match
    [InlineData(false, true, false)]   // C2 only - should not match
    [InlineData(true, true, true)]     // C1+C2 - should match
    public void QueryBuilder2_Matches_Only_Entities_With_All_Components(bool hasC1, bool hasC2, bool shouldMatch)
    {
        using var world = new World();
        
        var entity = world.Spawn();
        if (hasC1) entity.Add(42);       // C1 - int
        if (hasC2) entity.Add("hello");  // C2 - string
        
        // Add a decoy component to ensure entity exists in some archetype
        if (!hasC1 && !hasC2) entity.Add('x');
        
        using var builder = world.Query<int, string>();
        var query = builder.Compile();
        
        if (shouldMatch)
        {
            Assert.Single(query);
            Assert.Contains(entity, query);
        }
        else
        {
            Assert.DoesNotContain(entity, query);
        }
    }

    [Fact]
    public void QueryBuilder2_Count_With_All_Combinations()
    {
        using var world = new World();
        
        // Create all 4 combinations
        world.Spawn().Add('x');                         // None
        world.Spawn().Add(1);                           // C1 only
        world.Spawn().Add("only-c2");                   // C2 only
        world.Spawn().Add(42).Add("both");              // C1+C2 - should match
        world.Spawn().Add(43).Add("both-2");            // C1+C2 - should match
        
        using var builder = world.Query<int, string>();
        var query = builder.Compile();
        
        Assert.Equal(2, query.Count);
    }

    [Fact]
    public void QueryBuilder2_Matches_Entities_With_Extra_Components()
    {
        using var world = new World();
        
        // Entity with C1+C2 plus extra components should still match
        var entity = world.Spawn()
            .Add(42)       // C1 - int (required)
            .Add("hello")  // C2 - string (required)
            .Add(3.14f)    // Extra component
            .Add('x');     // Another extra component
        
        using var builder = world.Query<int, string>();
        var query = builder.Compile();
        
        Assert.Single(query);
        Assert.Contains(entity, query);
    }

    [Fact]
    public void Stream2_For_Only_Iterates_Entities_With_All_Components()
    {
        using var world = new World();
        
        // Create all non-matching combinations
        world.Spawn().Add('x');
        world.Spawn().Add(1);
        world.Spawn().Add("only-c2");
        
        // Create matching entity
        world.Spawn().Add(42).Add("found");
        
        var stream = world.Query<int, string>().Stream();
        var count = 0;
        var foundC1 = 0;
        var foundC2 = "";
        
        stream.For((ref int c1, ref string c2) =>
        {
            count++;
            foundC1 = c1;
            foundC2 = c2;
        });
        
        Assert.Equal(1, count);
        Assert.Equal(42, foundC1);
        Assert.Equal("found", foundC2);
    }

    #endregion

    #region QueryBuilder<C1, C2, C3> Tests (Arity 3) - 8 combinations

    [Theory]
    [InlineData(false, false, false, false)]  // None
    [InlineData(true, false, false, false)]   // C1 only
    [InlineData(false, true, false, false)]   // C2 only
    [InlineData(false, false, true, false)]   // C3 only
    [InlineData(true, true, false, false)]    // C1+C2 (missing C3)
    [InlineData(true, false, true, false)]    // C1+C3 (missing C2)
    [InlineData(false, true, true, false)]    // C2+C3 (missing C1)
    [InlineData(true, true, true, true)]      // C1+C2+C3 - should match
    public void QueryBuilder3_Matches_Only_Entities_With_All_Components(bool hasC1, bool hasC2, bool hasC3, bool shouldMatch)
    {
        using var world = new World();
        
        var entity = world.Spawn();
        if (hasC1) entity.Add(42);       // C1 - int
        if (hasC2) entity.Add("hello");  // C2 - string
        if (hasC3) entity.Add(3.14f);    // C3 - float
        
        // Add a decoy component to ensure entity exists in some archetype
        if (!hasC1 && !hasC2 && !hasC3) entity.Add('x');
        
        using var builder = world.Query<int, string, float>();
        var query = builder.Compile();
        
        if (shouldMatch)
        {
            Assert.Single(query);
            Assert.Contains(entity, query);
        }
        else
        {
            Assert.DoesNotContain(entity, query);
        }
    }

    [Fact]
    public void QueryBuilder3_Count_With_All_Combinations()
    {
        using var world = new World();
        
        // Create all 8 combinations (7 non-matching + 1 matching)
        world.Spawn().Add('x');                                  // None
        world.Spawn().Add(1);                                    // C1 only
        world.Spawn().Add("c2");                                 // C2 only
        world.Spawn().Add(1.0f);                                 // C3 only
        world.Spawn().Add(1).Add("c2");                          // C1+C2
        world.Spawn().Add(1).Add(1.0f);                          // C1+C3
        world.Spawn().Add("c2").Add(1.0f);                       // C2+C3
        world.Spawn().Add(42).Add("found").Add(3.14f);           // C1+C2+C3 - should match
        world.Spawn().Add(43).Add("found-2").Add(2.71f);         // C1+C2+C3 - should match
        
        using var builder = world.Query<int, string, float>();
        var query = builder.Compile();
        
        Assert.Equal(2, query.Count);
    }

    [Fact]
    public void QueryBuilder3_Matches_Entities_With_Extra_Components()
    {
        using var world = new World();
        
        // Entity with C1+C2+C3 plus extra components should still match
        var entity = world.Spawn()
            .Add(42)       // C1 - int (required)
            .Add("hello")  // C2 - string (required)
            .Add(3.14f)    // C3 - float (required)
            .Add('x')      // Extra component
            .Add(999L);    // Another extra component
        
        using var builder = world.Query<int, string, float>();
        var query = builder.Compile();
        
        Assert.Single(query);
        Assert.Contains(entity, query);
    }

    [Fact]
    public void Stream3_For_Only_Iterates_Entities_With_All_Components()
    {
        using var world = new World();
        
        // Create all 7 non-matching combinations
        world.Spawn().Add('x');
        world.Spawn().Add(1);
        world.Spawn().Add("c2");
        world.Spawn().Add(1.0f);
        world.Spawn().Add(1).Add("c2");
        world.Spawn().Add(1).Add(1.0f);
        world.Spawn().Add("c2").Add(1.0f);
        
        // Create matching entity
        world.Spawn().Add(42).Add("found").Add(3.14f);
        
        var stream = world.Query<int, string, float>().Stream();
        var count = 0;
        var foundC1 = 0;
        var foundC2 = "";
        var foundC3 = 0f;
        
        stream.For((ref int c1, ref string c2, ref float c3) =>
        {
            count++;
            foundC1 = c1;
            foundC2 = c2;
            foundC3 = c3;
        });
        
        Assert.Equal(1, count);
        Assert.Equal(42, foundC1);
        Assert.Equal("found", foundC2);
        Assert.Equal(3.14f, foundC3);
    }

    #endregion

    #region QueryBuilder<C1, C2, C3, C4> Tests (Arity 4) - 16 combinations

    [Theory]
    // All 16 combinations (only the last one should match)
    [InlineData(false, false, false, false, false)]  // None
    [InlineData(true, false, false, false, false)]   // C1
    [InlineData(false, true, false, false, false)]   // C2
    [InlineData(false, false, true, false, false)]   // C3
    [InlineData(false, false, false, true, false)]   // C4
    [InlineData(true, true, false, false, false)]    // C1+C2
    [InlineData(true, false, true, false, false)]    // C1+C3
    [InlineData(true, false, false, true, false)]    // C1+C4
    [InlineData(false, true, true, false, false)]    // C2+C3
    [InlineData(false, true, false, true, false)]    // C2+C4
    [InlineData(false, false, true, true, false)]    // C3+C4
    [InlineData(true, true, true, false, false)]     // C1+C2+C3
    [InlineData(true, true, false, true, false)]     // C1+C2+C4
    [InlineData(true, false, true, true, false)]     // C1+C3+C4
    [InlineData(false, true, true, true, false)]     // C2+C3+C4
    [InlineData(true, true, true, true, true)]       // C1+C2+C3+C4 - should match
    public void QueryBuilder4_Matches_Only_Entities_With_All_Components(
        bool hasC1, bool hasC2, bool hasC3, bool hasC4, bool shouldMatch)
    {
        using var world = new World();
        
        var entity = world.Spawn();
        if (hasC1) entity.Add(42);       // C1 - int
        if (hasC2) entity.Add("hello");  // C2 - string
        if (hasC3) entity.Add(3.14f);    // C3 - float
        if (hasC4) entity.Add(2.71);     // C4 - double
        
        // Add a decoy component to ensure entity exists in some archetype
        if (!hasC1 && !hasC2 && !hasC3 && !hasC4) entity.Add('x');
        
        using var builder = world.Query<int, string, float, double>();
        var query = builder.Compile();
        
        if (shouldMatch)
        {
            Assert.Single(query);
            Assert.Contains(entity, query);
        }
        else
        {
            Assert.DoesNotContain(entity, query);
        }
    }

    [Fact]
    public void QueryBuilder4_Count_With_All_Combinations()
    {
        using var world = new World();
        
        // Create all 16 combinations (15 non-matching)
        world.Spawn().Add('x');                                           // None
        world.Spawn().Add(1);                                             // C1
        world.Spawn().Add("c2");                                          // C2
        world.Spawn().Add(1.0f);                                          // C3
        world.Spawn().Add(1.0);                                           // C4
        world.Spawn().Add(1).Add("c2");                                   // C1+C2
        world.Spawn().Add(1).Add(1.0f);                                   // C1+C3
        world.Spawn().Add(1).Add(1.0);                                    // C1+C4
        world.Spawn().Add("c2").Add(1.0f);                                // C2+C3
        world.Spawn().Add("c2").Add(1.0);                                 // C2+C4
        world.Spawn().Add(1.0f).Add(1.0);                                 // C3+C4
        world.Spawn().Add(1).Add("c2").Add(1.0f);                         // C1+C2+C3
        world.Spawn().Add(1).Add("c2").Add(1.0);                          // C1+C2+C4
        world.Spawn().Add(1).Add(1.0f).Add(1.0);                          // C1+C3+C4
        world.Spawn().Add("c2").Add(1.0f).Add(1.0);                       // C2+C3+C4
        world.Spawn().Add(42).Add("found").Add(3.14f).Add(2.71);          // C1+C2+C3+C4 - should match
        world.Spawn().Add(43).Add("found-2").Add(2.71f).Add(3.14);        // C1+C2+C3+C4 - should match
        
        using var builder = world.Query<int, string, float, double>();
        var query = builder.Compile();
        
        Assert.Equal(2, query.Count);
    }

    [Fact]
    public void QueryBuilder4_Matches_Entities_With_Extra_Components()
    {
        using var world = new World();
        
        // Entity with C1+C2+C3+C4 plus extra components should still match
        var entity = world.Spawn()
            .Add(42)       // C1 - int (required)
            .Add("hello")  // C2 - string (required)
            .Add(3.14f)    // C3 - float (required)
            .Add(2.71)     // C4 - double (required)
            .Add('x')      // Extra component
            .Add(999L);    // Another extra component
        
        using var builder = world.Query<int, string, float, double>();
        var query = builder.Compile();
        
        Assert.Single(query);
        Assert.Contains(entity, query);
    }

    [Fact]
    public void Stream4_For_Only_Iterates_Entities_With_All_Components()
    {
        using var world = new World();
        
        // Create all 15 non-matching combinations
        world.Spawn().Add('x');
        world.Spawn().Add(1);
        world.Spawn().Add("c2");
        world.Spawn().Add(1.0f);
        world.Spawn().Add(1.0);
        world.Spawn().Add(1).Add("c2");
        world.Spawn().Add(1).Add(1.0f);
        world.Spawn().Add(1).Add(1.0);
        world.Spawn().Add("c2").Add(1.0f);
        world.Spawn().Add("c2").Add(1.0);
        world.Spawn().Add(1.0f).Add(1.0);
        world.Spawn().Add(1).Add("c2").Add(1.0f);
        world.Spawn().Add(1).Add("c2").Add(1.0);
        world.Spawn().Add(1).Add(1.0f).Add(1.0);
        world.Spawn().Add("c2").Add(1.0f).Add(1.0);
        
        // Create matching entity
        world.Spawn().Add(42).Add("found").Add(3.14f).Add(2.71);
        
        var stream = world.Query<int, string, float, double>().Stream();
        var count = 0;
        var foundC1 = 0;
        var foundC2 = "";
        var foundC3 = 0f;
        var foundC4 = 0.0;
        
        stream.For((ref int c1, ref string c2, ref float c3, ref double c4) =>
        {
            count++;
            foundC1 = c1;
            foundC2 = c2;
            foundC3 = c3;
            foundC4 = c4;
        });
        
        Assert.Equal(1, count);
        Assert.Equal(42, foundC1);
        Assert.Equal("found", foundC2);
        Assert.Equal(3.14f, foundC3);
        Assert.Equal(2.71, foundC4);
    }

    #endregion

    #region QueryBuilder<C1, C2, C3, C4, C5> Tests (Arity 5) - 32 combinations

    [Theory]
    // All 32 combinations (only the last one should match)
    // 0 components
    [InlineData(false, false, false, false, false, false)]  // None
    // 1 component
    [InlineData(true, false, false, false, false, false)]   // C1
    [InlineData(false, true, false, false, false, false)]   // C2
    [InlineData(false, false, true, false, false, false)]   // C3
    [InlineData(false, false, false, true, false, false)]   // C4
    [InlineData(false, false, false, false, true, false)]   // C5
    // 2 components
    [InlineData(true, true, false, false, false, false)]    // C1+C2
    [InlineData(true, false, true, false, false, false)]    // C1+C3
    [InlineData(true, false, false, true, false, false)]    // C1+C4
    [InlineData(true, false, false, false, true, false)]    // C1+C5
    [InlineData(false, true, true, false, false, false)]    // C2+C3
    [InlineData(false, true, false, true, false, false)]    // C2+C4
    [InlineData(false, true, false, false, true, false)]    // C2+C5
    [InlineData(false, false, true, true, false, false)]    // C3+C4
    [InlineData(false, false, true, false, true, false)]    // C3+C5
    [InlineData(false, false, false, true, true, false)]    // C4+C5
    // 3 components
    [InlineData(true, true, true, false, false, false)]     // C1+C2+C3
    [InlineData(true, true, false, true, false, false)]     // C1+C2+C4
    [InlineData(true, true, false, false, true, false)]     // C1+C2+C5
    [InlineData(true, false, true, true, false, false)]     // C1+C3+C4
    [InlineData(true, false, true, false, true, false)]     // C1+C3+C5
    [InlineData(true, false, false, true, true, false)]     // C1+C4+C5
    [InlineData(false, true, true, true, false, false)]     // C2+C3+C4
    [InlineData(false, true, true, false, true, false)]     // C2+C3+C5
    [InlineData(false, true, false, true, true, false)]     // C2+C4+C5
    [InlineData(false, false, true, true, true, false)]     // C3+C4+C5
    // 4 components
    [InlineData(true, true, true, true, false, false)]      // C1+C2+C3+C4
    [InlineData(true, true, true, false, true, false)]      // C1+C2+C3+C5
    [InlineData(true, true, false, true, true, false)]      // C1+C2+C4+C5
    [InlineData(true, false, true, true, true, false)]      // C1+C3+C4+C5
    [InlineData(false, true, true, true, true, false)]      // C2+C3+C4+C5
    // 5 components - should match
    [InlineData(true, true, true, true, true, true)]        // C1+C2+C3+C4+C5
    public void QueryBuilder5_Matches_Only_Entities_With_All_Components(
        bool hasC1, bool hasC2, bool hasC3, bool hasC4, bool hasC5, bool shouldMatch)
    {
        using var world = new World();
        
        var entity = world.Spawn();
        if (hasC1) entity.Add(42);       // C1 - int
        if (hasC2) entity.Add("hello");  // C2 - string
        if (hasC3) entity.Add(3.14f);    // C3 - float
        if (hasC4) entity.Add(2.71);     // C4 - double
        if (hasC5) entity.Add(999L);     // C5 - long
        
        // Add a decoy component to ensure entity exists in some archetype
        if (!hasC1 && !hasC2 && !hasC3 && !hasC4 && !hasC5) entity.Add('x');
        
        using var builder = world.Query<int, string, float, double, long>();
        var query = builder.Compile();
        
        if (shouldMatch)
        {
            Assert.Single(query);
            Assert.Contains(entity, query);
        }
        else
        {
            Assert.DoesNotContain(entity, query);
        }
    }

    [Fact]
    public void QueryBuilder5_Count_With_All_Combinations()
    {
        using var world = new World();
        
        // Create all 32 combinations (31 non-matching)
        // 0 components
        world.Spawn().Add('x');
        // 1 component
        world.Spawn().Add(1);
        world.Spawn().Add("c2");
        world.Spawn().Add(1.0f);
        world.Spawn().Add(1.0);
        world.Spawn().Add(1L);
        // 2 components
        world.Spawn().Add(1).Add("c2");
        world.Spawn().Add(1).Add(1.0f);
        world.Spawn().Add(1).Add(1.0);
        world.Spawn().Add(1).Add(1L);
        world.Spawn().Add("c2").Add(1.0f);
        world.Spawn().Add("c2").Add(1.0);
        world.Spawn().Add("c2").Add(1L);
        world.Spawn().Add(1.0f).Add(1.0);
        world.Spawn().Add(1.0f).Add(1L);
        world.Spawn().Add(1.0).Add(1L);
        // 3 components
        world.Spawn().Add(1).Add("c2").Add(1.0f);
        world.Spawn().Add(1).Add("c2").Add(1.0);
        world.Spawn().Add(1).Add("c2").Add(1L);
        world.Spawn().Add(1).Add(1.0f).Add(1.0);
        world.Spawn().Add(1).Add(1.0f).Add(1L);
        world.Spawn().Add(1).Add(1.0).Add(1L);
        world.Spawn().Add("c2").Add(1.0f).Add(1.0);
        world.Spawn().Add("c2").Add(1.0f).Add(1L);
        world.Spawn().Add("c2").Add(1.0).Add(1L);
        world.Spawn().Add(1.0f).Add(1.0).Add(1L);
        // 4 components
        world.Spawn().Add(1).Add("c2").Add(1.0f).Add(1.0);
        world.Spawn().Add(1).Add("c2").Add(1.0f).Add(1L);
        world.Spawn().Add(1).Add("c2").Add(1.0).Add(1L);
        world.Spawn().Add(1).Add(1.0f).Add(1.0).Add(1L);
        world.Spawn().Add("c2").Add(1.0f).Add(1.0).Add(1L);
        // 5 components - should match
        world.Spawn().Add(42).Add("found").Add(3.14f).Add(2.71).Add(999L);
        world.Spawn().Add(43).Add("found-2").Add(2.71f).Add(3.14).Add(888L);
        
        using var builder = world.Query<int, string, float, double, long>();
        var query = builder.Compile();
        
        Assert.Equal(2, query.Count);
    }

    [Fact]
    public void QueryBuilder5_Matches_Entities_With_Extra_Components()
    {
        using var world = new World();
        
        // Entity with C1+C2+C3+C4+C5 plus extra components should still match
        var entity = world.Spawn()
            .Add(42)       // C1 - int (required)
            .Add("hello")  // C2 - string (required)
            .Add(3.14f)    // C3 - float (required)
            .Add(2.71)     // C4 - double (required)
            .Add(999L)     // C5 - long (required)
            .Add('x')      // Extra component
            .Add((short)1);// Another extra component
        
        using var builder = world.Query<int, string, float, double, long>();
        var query = builder.Compile();
        
        Assert.Single(query);
        Assert.Contains(entity, query);
    }

    [Fact]
    public void Stream5_For_Only_Iterates_Entities_With_All_Components()
    {
        using var world = new World();
        
        // Create all 31 non-matching combinations
        // 0 components
        world.Spawn().Add('x');
        // 1 component
        world.Spawn().Add(1);
        world.Spawn().Add("c2");
        world.Spawn().Add(1.0f);
        world.Spawn().Add(1.0);
        world.Spawn().Add(1L);
        // 2 components
        world.Spawn().Add(1).Add("c2");
        world.Spawn().Add(1).Add(1.0f);
        world.Spawn().Add(1).Add(1.0);
        world.Spawn().Add(1).Add(1L);
        world.Spawn().Add("c2").Add(1.0f);
        world.Spawn().Add("c2").Add(1.0);
        world.Spawn().Add("c2").Add(1L);
        world.Spawn().Add(1.0f).Add(1.0);
        world.Spawn().Add(1.0f).Add(1L);
        world.Spawn().Add(1.0).Add(1L);
        // 3 components
        world.Spawn().Add(1).Add("c2").Add(1.0f);
        world.Spawn().Add(1).Add("c2").Add(1.0);
        world.Spawn().Add(1).Add("c2").Add(1L);
        world.Spawn().Add(1).Add(1.0f).Add(1.0);
        world.Spawn().Add(1).Add(1.0f).Add(1L);
        world.Spawn().Add(1).Add(1.0).Add(1L);
        world.Spawn().Add("c2").Add(1.0f).Add(1.0);
        world.Spawn().Add("c2").Add(1.0f).Add(1L);
        world.Spawn().Add("c2").Add(1.0).Add(1L);
        world.Spawn().Add(1.0f).Add(1.0).Add(1L);
        // 4 components
        world.Spawn().Add(1).Add("c2").Add(1.0f).Add(1.0);
        world.Spawn().Add(1).Add("c2").Add(1.0f).Add(1L);
        world.Spawn().Add(1).Add("c2").Add(1.0).Add(1L);
        world.Spawn().Add(1).Add(1.0f).Add(1.0).Add(1L);
        world.Spawn().Add("c2").Add(1.0f).Add(1.0).Add(1L);
        
        // Create matching entity
        world.Spawn().Add(42).Add("found").Add(3.14f).Add(2.71).Add(999L);
        
        var stream = world.Query<int, string, float, double, long>().Stream();
        var count = 0;
        var foundC1 = 0;
        var foundC2 = "";
        var foundC3 = 0f;
        var foundC4 = 0.0;
        var foundC5 = 0L;
        
        stream.For((ref int c1, ref string c2, ref float c3, ref double c4, ref long c5) =>
        {
            count++;
            foundC1 = c1;
            foundC2 = c2;
            foundC3 = c3;
            foundC4 = c4;
            foundC5 = c5;
        });
        
        Assert.Equal(1, count);
        Assert.Equal(42, foundC1);
        Assert.Equal("found", foundC2);
        Assert.Equal(3.14f, foundC3);
        Assert.Equal(2.71, foundC4);
        Assert.Equal(999L, foundC5);
    }

    #endregion
}
