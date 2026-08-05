namespace fennecs.tests;

public class FamilyMatchTests
{
    private class Animal;

    private class Fox : Animal;

    private class Fennec : Fox;

    private class Rock;

    private struct Pebble;


    [Fact]
    public void Family_Matches_Self_And_Derived_Plain_Components()
    {
        using var world = new World();
        var animal = world.Spawn().Add(new Animal());
        var fox = world.Spawn().Add(new Fox());
        var fennec = world.Spawn().Add(new Fennec());
        var rock = world.Spawn().Add(new Rock());

        var query = world.Query().Has<Animal>(Match.Family).Compile();

        Assert.True(query.Contains(animal));
        Assert.True(query.Contains(fox));
        Assert.True(query.Contains(fennec));
        Assert.False(query.Contains(rock));
        Assert.Equal(3, query.Count);
    }


    [Fact]
    public void Family_Matches_Grandchild_Through_Intermediate_Base()
    {
        using var world = new World();
        var fennec = world.Spawn().Add(new Fennec());

        var foxes = world.Query().Has<Fox>(Match.Family).Compile();
        var animals = world.Query().Has<Animal>(Match.Family).Compile();

        Assert.True(foxes.Contains(fennec));
        Assert.True(animals.Contains(fennec));
    }


    [Fact]
    public void Family_Not_Clause_Excludes_Derived()
    {
        using var world = new World();
        world.Spawn().Add(new Fennec()).Add(42);
        var rocky = world.Spawn().Add(new Rock()).Add(43);

        var query = world.Query().Has<int>().Not<Animal>(Match.Family).Compile();

        Assert.Single(query);
        Assert.True(query.Contains(rocky));
    }


    [Fact]
    public void Family_Any_Clause_Participates()
    {
        using var world = new World();
        var fox = world.Spawn().Add(new Fox()).Add(1);
        var rock = world.Spawn().Add(new Rock()).Add(2);
        world.Spawn().Add(3);

        var query = world.Query().Has<int>()
            .Any<Animal>(Match.Family)
            .Any<Rock>()
            .Compile();

        Assert.Equal(2, query.Count);
        Assert.True(query.Contains(fox));
        Assert.True(query.Contains(rock));
    }


    [Fact]
    public void Family_On_Struct_Matches_Only_Itself()
    {
        using var world = new World();
        var pebble = world.Spawn().Add(new Pebble());
        world.Spawn().Add(new Rock());

        var query = world.Query().Has<Pebble>(Match.Family).Compile();

        Assert.Single(query);
        Assert.True(query.Contains(pebble));
    }


    [Fact]
    public void Family_Does_Not_Match_Relations_Or_Links()
    {
        using var world = new World();
        var target = world.Spawn();
        world.Spawn().Add(new Fox(), target);       // Entity relation backed by Fox
        world.Spawn().Add(Link.With("linked"));

        var query = world.Query().Has<Animal>(Match.Family).Compile();

        Assert.Empty(query);
    }


    [Fact]
    public void Family_Works_In_FilteredStream()
    {
        using var world = new World();
        world.Spawn().Add(new Fennec()).Add(1);
        world.Spawn().Add(new Rock()).Add(2);
        world.Spawn().Add(3);

        var stream = world.Query<int>().Stream();

        Assert.Equal(1, stream.Has(Comp<Animal>.Matching(Match.Family)).Count);
        Assert.Equal(2, stream.Not(Comp<Animal>.Matching(Match.Family)).Count);
    }


    [Fact]
    public void Family_As_Stream_Type_Throws()
    {
        using var world = new World();
        world.Spawn().Add(new Animal());

        Assert.Throws<InvalidOperationException>(() => world.Query<Animal>(Match.Family).Stream());
    }


    [Fact]
    public void Family_Unrelated_Base_Stays_Unmatched()
    {
        // Exercises both the bloom fast-reject and (on any collision) the precise ancestor confirm.
        var signature = new Signature(
            TypeExpression.Of<Fennec>(Match.Plain),
            TypeExpression.Of<int>(Match.Plain));
        var bits = new ArchetypeBits(signature);

        Assert.True(bits.MatchesElement(TypeExpression.Of<Animal>(Match.Family)));
        Assert.True(bits.MatchesElement(TypeExpression.Of<Fox>(Match.Family)));
        Assert.False(bits.MatchesElement(TypeExpression.Of<Rock>(Match.Family)));
        Assert.False(bits.MatchesElement(TypeExpression.Of<string>(Match.Family)));
    }


    [Fact]
    public void Family_In_Batch_Conflict_Masks()
    {
        // SafeForAddition: an addition is safe when the Not clause filters it out — including via Family.
        var notFamily = new Mask().Not(TypeExpression.Of<Animal>(Match.Family));
        Assert.True(notFamily.SafeForAddition(TypeExpression.Of<Fennec>(Match.Plain)));
        Assert.True(notFamily.SafeForAddition(TypeExpression.Of<Animal>(Match.Plain)));
        Assert.False(notFamily.SafeForAddition(TypeExpression.Of<Rock>(Match.Plain)));

        // Bidirectional: a Family expression is covered by plain clause entries of derived types.
        var plainClause = new Mask().Has(TypeExpression.Of<Fennec>(Match.Plain));
        Assert.True(plainClause.Bits.Has.MatchesBidirectional(TypeExpression.Of<Animal>(Match.Family)));
        Assert.False(plainClause.Bits.Has.MatchesBidirectional(TypeExpression.Of<Rock>(Match.Family)));
    }


    [Fact]
    public void Wildcard_Semantics_Unchanged_By_Family_Support()
    {
        using var world = new World();
        var plain = world.Spawn().Add(new Fox());

        // Family never leaks into Any/Target semantics for stored plain components.
        Assert.True(world.Query().Has<Fox>(Match.Any).Compile().Contains(plain));
        Assert.False(world.Query().Has<Fox>(Match.Target).Compile().Contains(plain));
    }
}
