namespace fennecs.tests.Query;

/// <summary>
/// Tests for the IReadOnlySet&lt;Entity&gt; implementation on Query.
/// These tests verify the set operation methods: Contains, IsSubsetOf, IsSupersetOf,
/// IsProperSubsetOf, IsProperSupersetOf, Overlaps, and SetEquals.
/// </summary>
public class QueryReadOnlySetTests
{
    #region Contains Tests

    [Fact]
    public void Contains_Returns_True_For_Matching_Entity()
    {
        using var world = new World();
        var entity = world.Spawn().Add(42);
        var query = world.Query<int>().Compile();

        Assert.True(query.Contains(entity));
    }

    [Fact]
    public void Contains_Returns_False_For_Non_Matching_Entity()
    {
        using var world = new World();
        var matchingEntity = world.Spawn().Add(42);
        var nonMatchingEntity = world.Spawn().Add("string");
        var query = world.Query<int>().Compile();

        Assert.True(query.Contains(matchingEntity));
        Assert.False(query.Contains(nonMatchingEntity));
    }

    [Fact]
    public void Contains_Works_With_Assert_Contains()
    {
        using var world = new World();
        var entity = world.Spawn().Add(42);
        var query = world.Query<int>().Compile();

        Assert.Contains(entity, query);
    }

    [Fact]
    public void DoesNotContain_Works_With_Assert_DoesNotContain()
    {
        using var world = new World();
        var matchingEntity = world.Spawn().Add(42);
        var nonMatchingEntity = world.Spawn().Add("string");
        var query = world.Query<int>().Compile();

        Assert.Contains(matchingEntity, query);
        Assert.DoesNotContain(nonMatchingEntity, query);
    }

    #endregion

    #region IsSubsetOf Tests

    [Fact]
    public void IsSubsetOf_Empty_Query_Is_Subset_Of_Any_Collection()
    {
        using var world = new World();
        var entity = world.Spawn().Add("string");  // Won't match int query
        var query = world.Query<int>().Compile();

        Assert.True(query.IsEmpty);
        Assert.True(query.IsSubsetOf([entity]));
        Assert.True(query.IsSubsetOf([]));
    }

    [Fact]
    public void IsSubsetOf_Query_Is_Subset_Of_Itself()
    {
        using var world = new World();
        world.Spawn().Add(1);
        world.Spawn().Add(2);
        world.Spawn().Add(3);
        var query = world.Query<int>().Compile();

        Assert.True(query.IsSubsetOf(query));
    }

    [Fact]
    public void IsSubsetOf_Query_Is_Subset_Of_Larger_Collection()
    {
        using var world = new World();
        var e1 = world.Spawn().Add(1);
        var e2 = world.Spawn().Add(2);
        var e3 = world.Spawn().Add("extra");  // Won't match query
        var query = world.Query<int>().Compile();

        Assert.True(query.IsSubsetOf([e1, e2, e3]));
    }

    [Fact]
    public void IsSubsetOf_Returns_False_When_Not_Subset()
    {
        using var world = new World();
        var e1 = world.Spawn().Add(1);
        var e2 = world.Spawn().Add(2);
        var query = world.Query<int>().Compile();

        // Query has both e1 and e2, but we only provide e1
        Assert.False(query.IsSubsetOf([e1]));
    }

    [Fact]
    public void IsSubsetOf_Throws_On_Null()
    {
        using var world = new World();
        var query = world.Query<int>().Compile();

        Assert.Throws<ArgumentNullException>(() => query.IsSubsetOf(null!));
    }

    #endregion

    #region IsSupersetOf Tests

    [Fact]
    public void IsSupersetOf_Query_Is_Superset_Of_Empty_Collection()
    {
        using var world = new World();
        world.Spawn().Add(1);
        var query = world.Query<int>().Compile();

        Assert.True(query.IsSupersetOf([]));
    }

    [Fact]
    public void IsSupersetOf_Query_Is_Superset_Of_Itself()
    {
        using var world = new World();
        world.Spawn().Add(1);
        world.Spawn().Add(2);
        var query = world.Query<int>().Compile();

        Assert.True(query.IsSupersetOf(query));
    }

    [Fact]
    public void IsSupersetOf_Query_Is_Superset_Of_Smaller_Collection()
    {
        using var world = new World();
        var e1 = world.Spawn().Add(1);
        world.Spawn().Add(2);
        world.Spawn().Add(3);
        var query = world.Query<int>().Compile();

        Assert.True(query.IsSupersetOf([e1]));
    }

    [Fact]
    public void IsSupersetOf_Returns_False_When_Not_Superset()
    {
        using var world = new World();
        var e1 = world.Spawn().Add(1);
        var e2 = world.Spawn().Add("string");  // Won't match query
        var query = world.Query<int>().Compile();

        // Query only has e1, but collection has e2 which is not in query
        Assert.False(query.IsSupersetOf([e1, e2]));
    }

    [Fact]
    public void IsSupersetOf_Empty_Query_Is_Superset_Of_Empty_Collection()
    {
        using var world = new World();
        world.Spawn().Add("string");  // Won't match int query
        var query = world.Query<int>().Compile();

        Assert.True(query.IsEmpty);
        Assert.True(query.IsSupersetOf([]));
    }

    [Fact]
    public void IsSupersetOf_Throws_On_Null()
    {
        using var world = new World();
        var query = world.Query<int>().Compile();

        Assert.Throws<ArgumentNullException>(() => query.IsSupersetOf(null!));
    }

    #endregion

    #region IsProperSubsetOf Tests

    [Fact]
    public void IsProperSubsetOf_Returns_True_When_Strictly_Smaller()
    {
        using var world = new World();
        var e1 = world.Spawn().Add(1);
        var e2 = world.Spawn().Add(2);
        var e3 = world.Spawn().Add("extra");  // Won't match query
        var query = world.Query<int>().Compile();

        // Query has {e1, e2}, collection has {e1, e2, e3}
        Assert.True(query.IsProperSubsetOf([e1, e2, e3]));
    }

    [Fact]
    public void IsProperSubsetOf_Returns_False_When_Equal()
    {
        using var world = new World();
        var e1 = world.Spawn().Add(1);
        var e2 = world.Spawn().Add(2);
        var query = world.Query<int>().Compile();

        // Query has {e1, e2}, collection has {e1, e2} - same size, not proper subset
        Assert.False(query.IsProperSubsetOf([e1, e2]));
    }

    [Fact]
    public void IsProperSubsetOf_Empty_Query_Is_Proper_Subset_Of_NonEmpty()
    {
        using var world = new World();
        var entity = world.Spawn().Add("string");  // Won't match int query
        var query = world.Query<int>().Compile();

        Assert.True(query.IsEmpty);
        Assert.True(query.IsProperSubsetOf([entity]));
    }

    [Fact]
    public void IsProperSubsetOf_Empty_Query_Is_Not_Proper_Subset_Of_Empty()
    {
        using var world = new World();
        world.Spawn().Add("string");  // Won't match int query
        var query = world.Query<int>().Compile();

        Assert.True(query.IsEmpty);
        Assert.False(query.IsProperSubsetOf([]));
    }

    [Fact]
    public void IsProperSubsetOf_Throws_On_Null()
    {
        using var world = new World();
        var query = world.Query<int>().Compile();

        Assert.Throws<ArgumentNullException>(() => query.IsProperSubsetOf(null!));
    }

    #endregion

    #region IsProperSupersetOf Tests

    [Fact]
    public void IsProperSupersetOf_Returns_True_When_Strictly_Larger()
    {
        using var world = new World();
        var e1 = world.Spawn().Add(1);
        var e2 = world.Spawn().Add(2);
        var e3 = world.Spawn().Add(3);
        var query = world.Query<int>().Compile();

        // Query has {e1, e2, e3}, we provide only {e1}
        Assert.True(query.IsProperSupersetOf([e1]));
    }

    [Fact]
    public void IsProperSupersetOf_Returns_False_When_Equal()
    {
        using var world = new World();
        var e1 = world.Spawn().Add(1);
        var e2 = world.Spawn().Add(2);
        var query = world.Query<int>().Compile();

        // Query has {e1, e2}, collection has {e1, e2} - same size, not proper superset
        Assert.False(query.IsProperSupersetOf([e1, e2]));
    }

    [Fact]
    public void IsProperSupersetOf_NonEmpty_Query_Is_Proper_Superset_Of_Empty()
    {
        using var world = new World();
        world.Spawn().Add(1);
        var query = world.Query<int>().Compile();

        Assert.True(query.IsProperSupersetOf([]));
    }

    [Fact]
    public void IsProperSupersetOf_Empty_Query_Is_Not_Proper_Superset_Of_Empty()
    {
        using var world = new World();
        world.Spawn().Add("string");  // Won't match int query
        var query = world.Query<int>().Compile();

        Assert.True(query.IsEmpty);
        Assert.False(query.IsProperSupersetOf([]));
    }

    [Fact]
    public void IsProperSupersetOf_Handles_Duplicates_In_Input()
    {
        using var world = new World();
        var e1 = world.Spawn().Add(1);
        var e2 = world.Spawn().Add(2);
        var query = world.Query<int>().Compile();

        // Input has duplicates: {e1, e1, e1} but unique count is 1
        // Query has {e1, e2} which is proper superset of {e1}
        Assert.True(query.IsProperSupersetOf([e1, e1, e1]));
    }

    [Fact]
    public void IsProperSupersetOf_Throws_On_Null()
    {
        using var world = new World();
        var query = world.Query<int>().Compile();

        Assert.Throws<ArgumentNullException>(() => query.IsProperSupersetOf(null!));
    }

    #endregion

    #region Overlaps Tests

    [Fact]
    public void Overlaps_Returns_True_When_At_Least_One_Common_Element()
    {
        using var world = new World();
        var e1 = world.Spawn().Add(1);
        var e2 = world.Spawn().Add("string");  // Won't match int query
        var query = world.Query<int>().Compile();

        Assert.True(query.Overlaps([e1, e2]));
    }

    [Fact]
    public void Overlaps_Returns_False_When_No_Common_Elements()
    {
        using var world = new World();
        world.Spawn().Add(1);
        var e2 = world.Spawn().Add("string");  // Won't match int query
        var query = world.Query<int>().Compile();

        Assert.False(query.Overlaps([e2]));
    }

    [Fact]
    public void Overlaps_Returns_False_For_Empty_Collection()
    {
        using var world = new World();
        world.Spawn().Add(1);
        var query = world.Query<int>().Compile();

        Assert.False(query.Overlaps([]));
    }

    [Fact]
    public void Overlaps_Returns_False_For_Empty_Query()
    {
        using var world = new World();
        var entity = world.Spawn().Add("string");  // Won't match int query
        var query = world.Query<int>().Compile();

        Assert.True(query.IsEmpty);
        Assert.False(query.Overlaps([entity]));
    }

    [Fact]
    public void Overlaps_Returns_True_When_All_Elements_Match()
    {
        using var world = new World();
        var e1 = world.Spawn().Add(1);
        var e2 = world.Spawn().Add(2);
        var query = world.Query<int>().Compile();

        Assert.True(query.Overlaps([e1, e2]));
    }

    [Fact]
    public void Overlaps_Throws_On_Null()
    {
        using var world = new World();
        var query = world.Query<int>().Compile();

        Assert.Throws<ArgumentNullException>(() => query.Overlaps(null!));
    }

    #endregion

    #region SetEquals Tests

    [Fact]
    public void SetEquals_Returns_True_For_Identical_Sets()
    {
        using var world = new World();
        var e1 = world.Spawn().Add(1);
        var e2 = world.Spawn().Add(2);
        var query = world.Query<int>().Compile();

        Assert.True(query.SetEquals([e1, e2]));
    }

    [Fact]
    public void SetEquals_Returns_True_For_Same_Elements_Different_Order()
    {
        using var world = new World();
        var e1 = world.Spawn().Add(1);
        var e2 = world.Spawn().Add(2);
        var e3 = world.Spawn().Add(3);
        var query = world.Query<int>().Compile();

        Assert.True(query.SetEquals([e3, e1, e2]));
    }

    [Fact]
    public void SetEquals_Returns_True_With_Duplicates_In_Input()
    {
        using var world = new World();
        var e1 = world.Spawn().Add(1);
        var e2 = world.Spawn().Add(2);
        var query = world.Query<int>().Compile();

        // Duplicates should be ignored - unique set is {e1, e2}
        Assert.True(query.SetEquals([e1, e2, e1, e2]));
    }

    [Fact]
    public void SetEquals_Returns_False_When_Query_Has_More_Elements()
    {
        using var world = new World();
        var e1 = world.Spawn().Add(1);
        world.Spawn().Add(2);
        var query = world.Query<int>().Compile();

        Assert.False(query.SetEquals([e1]));
    }

    [Fact]
    public void SetEquals_Returns_False_When_Collection_Has_More_Elements()
    {
        using var world = new World();
        var e1 = world.Spawn().Add(1);
        var e2 = world.Spawn().Add("string");  // Won't match int query
        var query = world.Query<int>().Compile();

        // Query has {e1}, collection has {e1, e2}
        Assert.False(query.SetEquals([e1, e2]));
    }

    [Fact]
    public void SetEquals_Empty_Query_Equals_Empty_Collection()
    {
        using var world = new World();
        world.Spawn().Add("string");  // Won't match int query
        var query = world.Query<int>().Compile();

        Assert.True(query.IsEmpty);
        Assert.True(query.SetEquals([]));
    }

    [Fact]
    public void SetEquals_Returns_False_When_Different_Elements()
    {
        using var world = new World();
        var e1 = world.Spawn().Add(1);
        var e2 = world.Spawn().Add(2);
        var e3 = world.Spawn().Add("string");  // Won't match int query
        var query = world.Query<int>().Compile();

        // Query has {e1, e2}, collection has {e1, e3} - different
        Assert.False(query.SetEquals([e1, e3]));
    }

    [Fact]
    public void SetEquals_Throws_On_Null()
    {
        using var world = new World();
        var query = world.Query<int>().Compile();

        Assert.Throws<ArgumentNullException>(() => query.SetEquals(null!));
    }

    #endregion

    #region Cross-Query Tests

    [Fact]
    public void Can_Compare_Two_Queries_With_Overlaps()
    {
        using var world = new World();
        world.Spawn().Add(1).Add("also string");
        world.Spawn().Add(2);
        world.Spawn().Add("only string");

        var intQuery = world.Query<int>().Compile();
        var stringQuery = world.Query<string>().Compile();

        // Both queries contain the entity with int+string
        Assert.True(intQuery.Overlaps(stringQuery));
        Assert.True(stringQuery.Overlaps(intQuery));
    }

    [Fact]
    public void Can_Compare_Disjoint_Queries()
    {
        using var world = new World();
        world.Spawn().Add(1);
        world.Spawn().Add(2);
        world.Spawn().Add("only string");
        world.Spawn().Add(3.14f);

        var intQuery = world.Query<int>().Compile();
        var stringQuery = world.Query<string>().Compile();

        // No overlap - no entity has both int and string
        Assert.False(intQuery.Overlaps(stringQuery));
        Assert.False(stringQuery.Overlaps(intQuery));
    }

    [Fact]
    public void Query_Is_Subset_Of_Superset_Query()
    {
        using var world = new World();
        world.Spawn().Add(1).Add("tagged");  // Matches both queries
        world.Spawn().Add(2);                 // Only matches int query
        world.Spawn().Add(3);                 // Only matches int query

        var intQuery = world.Query<int>().Compile();
        var taggedQuery = world.Query<int>().Has<string>().Compile();

        // taggedQuery is a subset of intQuery
        Assert.True(taggedQuery.IsSubsetOf(intQuery));
        Assert.True(taggedQuery.IsProperSubsetOf(intQuery));
        Assert.True(intQuery.IsSupersetOf(taggedQuery));
        Assert.True(intQuery.IsProperSupersetOf(taggedQuery));
    }

    [Fact]
    public void SetEquals_With_Other_Query()
    {
        using var world = new World();
        world.Spawn().Add(1).Add("tagged");
        world.Spawn().Add(2).Add("also tagged");

        var query1 = world.Query<int>().Has<string>().Compile();
        var query2 = world.Query<string>().Has<int>().Compile();

        // Both queries match exactly the same entities
        Assert.True(query1.SetEquals(query2));
        Assert.True(query2.SetEquals(query1));
    }

    #endregion

    #region Edge Cases

    [Fact]
    public void All_Operations_Work_With_Single_Entity()
    {
        using var world = new World();
        var e1 = world.Spawn().Add(1);
        var query = world.Query<int>().Compile();

        Assert.True(query.Contains(e1));
        Assert.True(query.IsSubsetOf([e1]));
        Assert.True(query.IsSupersetOf([e1]));
        Assert.False(query.IsProperSubsetOf([e1]));
        Assert.False(query.IsProperSupersetOf([e1]));
        Assert.True(query.Overlaps([e1]));
        Assert.True(query.SetEquals([e1]));
    }

    [Fact]
    public void All_Operations_Work_With_Large_Entity_Count()
    {
        using var world = new World();
        var entities = new List<Entity>();
        for (var i = 0; i < 1000; i++)
        {
            entities.Add(world.Spawn().Add(i));
        }

        var query = world.Query<int>().Compile();

        Assert.True(query.SetEquals(entities));
        Assert.True(query.IsSupersetOf(entities.Take(500)));
        Assert.True(query.IsProperSupersetOf(entities.Take(500)));
        Assert.True(query.IsSubsetOf(entities));
        Assert.False(query.IsProperSubsetOf(entities));  // Same size
        Assert.True(query.Overlaps(entities.Take(1)));
    }

    [Fact]
    public void Operations_Work_Across_Multiple_Archetypes()
    {
        using var world = new World();
        var e1 = world.Spawn().Add(1);                    // Archetype: int
        var e2 = world.Spawn().Add(2).Add("string");      // Archetype: int+string
        var e3 = world.Spawn().Add(3).Add(3.14f);         // Archetype: int+float

        var query = world.Query<int>().Compile();

        // Query should match all 3 entities across 3 different archetypes
        Assert.Equal(3, query.Count);
        Assert.Contains(e1, query);
        Assert.Contains(e2, query);
        Assert.Contains(e3, query);
        Assert.True(query.SetEquals([e1, e2, e3]));
    }

    #endregion
}
