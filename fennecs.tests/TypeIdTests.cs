// SPDX-License-Identifier: MIT

using System.Runtime.InteropServices;

namespace fennecs.tests;

public class TypeIdTests
{
    [Fact]
    public void Entity_is_64_bits()
    {
        Assert.Equal(64 / 8, Marshal.SizeOf<Entity>());
    }


    [Fact]
    public void TypeAssigner_Id_Unique()
    {
        Assert.NotEqual(
            LanguageType<int>.Id,
            LanguageType<string>.Id);

        Assert.NotEqual(
            LanguageType<ushort>.Id,
            LanguageType<short>.Id);

        Assert.NotEqual(
            LanguageType<Type1>.Id,
            LanguageType<Type2>.Id);
    }


    [Fact]
    public void TypeAssigner_Id_Same_For_Same_Type()
    {
        Assert.Equal(
            LanguageType<int>.Id,
            LanguageType<int>.Id);

        Assert.Equal(
            LanguageType<Type1>.Id,
            LanguageType<Type1>.Id);

        Assert.Equal(
            LanguageType<Type2>.Id,
            LanguageType<Type2>.Id);

        Assert.Equal(
            LanguageType<Dictionary<string, string>>.Id,
            LanguageType<Dictionary<string, string>>.Id);
    }


    [Fact]
    public void TypeAssigner_None_Matches_Identical()
    {
        var id1 = TypeExpression.Of<int>(default);
        var id2 = TypeExpression.Of<int>(default);

        Assert.True(id1.Matches(id2));
    }


    [Fact]
    public void TypeAssigner_Any_Matches_Default()
    {
        // Keeping the default case to ensure it remains at default
        // ReSharper disable once RedundantArgumentDefaultValue
        var id1 = TypeExpression.Of<int>(default);
        var id2 = TypeExpression.Of<int>(Match.Any);

        Assert.True(id2.Matches(id1));
        Assert.False(id1.Matches(id2)); //Non-commutative, TODO: Review whether we actually want this.
    }


    [Fact]
    public void TypeAssigner_does_not_Match_Identical()
    {
        var id1 = TypeExpression.Of<int>(default);
        var id2 = TypeExpression.Of<float>(default);

        Assert.False(id1.Matches(id2));
    }


    [Fact]
    public void TypeAssigner_None_does_not_match_Any()
    {
        var id1 = TypeExpression.Of<int>(default);
        var id2 = TypeExpression.Of<int>(new(new Entity(123)));
        var id3 = TypeExpression.Of<int>(Match.Any);

        Assert.False(id1.Matches(id2));
        Assert.False(id1.Matches(id3));
    }


    [Fact]
    public void TypeId_from_Generic_is_same_as_Identify()
    {
        var id1 = TypeExpression.Of<int>(default).TypeId;
        var id2 = LanguageType.Identify(typeof(int));
        Assert.Equal(id1, id2);

        _ = TypeExpression.Of<string>((Key) default);

        var id3 = LanguageType.Identify(typeof(bool));
        var id4 = TypeExpression.Of<bool>(default).TypeId;
        Assert.Equal(id3, id4);

        Assert.NotEqual(id1, id3);
        Assert.NotEqual(id2, id4);
    }


    [Fact]
    public void Match_from_Anonymous_Object_Entity_Is_Differentiable()
    {
        var target1 = new { doot = "foo" };
        var typeExpression1 = TypeExpression.Of<object>(Link.With(target1));
        var typeExpression2 = TypeExpression.Of<object>(Link.With(target1));
        
        Assert.Equal(typeExpression1, typeExpression2);
        
        var target2 = new { doot = "bar"};
        var typeExpression3 = TypeExpression.Of<object>(Link.With(target2));
        var typeExpression4 = TypeExpression.Of<object>(Link.With(target2));
        
        Assert.False(ReferenceEquals(target1, target2));
        Assert.NotEqual(target1, target2);
        
        Assert.Equal(typeExpression1, typeExpression2);
        Assert.Equal(typeExpression3, typeExpression4);
        Assert.NotEqual(typeExpression1, typeExpression3);
        Assert.NotEqual(typeExpression2, typeExpression4);

        var obj1 = new object();
        var obj2 = new object();
        
        Assert.NotEqual(obj1, obj2);
        Assert.NotEqual(obj1.GetHashCode(), obj2.GetHashCode());
        Assert.False(ReferenceEquals(obj1, obj2));
    }

    private struct Type1337;
    private struct Type1338;
    
    [Fact]
    public void Can_Identify_Exotic_Type()
    {
        var id1 = LanguageType.Identify(typeof(Dictionary<string, Type1337>));
        var id2 = TypeExpression.Of<Dictionary<string, Type1337>>((Key) default).TypeId;
        Assert.Equal(id1, id2);

        id1 = TypeExpression.Of<Dictionary<string, Type1338>>((Key) default).TypeId;
        id2 = LanguageType.Identify(typeof(Dictionary<string, Type1338>));
        Assert.Equal(id1, id2);
    }
    
    private struct Type1;

    private struct Type2;
}