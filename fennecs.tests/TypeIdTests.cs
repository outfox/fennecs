// SPDX-License-Identifier: MIT

using System.Runtime.InteropServices;

namespace fennecs.tests;

public class TypeIdTests
{
    private struct Type1;

    private struct Type2;

    private struct Type3Backlink : IRelationBacklink;

    [Fact]
    public void BackLink_Correctly_Declarable()
    {
        var id1 = TypeExpression.Create<Type3Backlink>(new Identity(1234));
        Assert.True(id1.isBacklink);
        
        var id2 = TypeExpression.Create<Type2>(new Identity(1234));
        Assert.False(id2.isBacklink);
    }

    [Fact]
    public void Non_BackLink_is_Default()
    {
        var id = TypeExpression.Create<Type2>(new Identity(1234));
        Assert.False(id.isBacklink);
        Assert.True(id.TypeId > 0);
    }

    [Fact]
    public void TypeId_is_64_bits()
    {
        Assert.Equal(64 / 8, Marshal.SizeOf<TypeExpression>());
    }

    [Fact]
    public void Identity_is_64_bits()
    {
        Assert.Equal(64 / 8, Marshal.SizeOf<Identity>());
    }

    

    /* TODO: Remove obsolete tests that no longer apply to type structure.
    [Theory]
    [InlineData(0)]
    [InlineData(1848)]
    [InlineData(0x7FFF)]
    [InlineData(0xFFFF)]
    [InlineData(0x1111)]
    [InlineData(0xABCD)]
    public void TypeId_Generation_is_Congruent(ushort input)
    {
        var id = new TypeId {Generation = input};
        Assert.Equal(input, id.Generation);
        Assert.Equal(input, id.Target.Generation);
        Assert.Equal(0, id.TypeNumber);
        Assert.Equal(0, id.Target.Id);
        Assert.Equal(0, id.Id);
    }

    [Fact]
    public void TypeId_Identity_is_Congruent()
    {
        var id = new TypeId {Id = 900069420};
        Assert.Equal(900069420, id.Id);
        Assert.Equal(900069420, id.Target.Id);
        Assert.Equal(0, id.TypeNumber);
        Assert.Equal(0, id.Generation);
        Assert.Equal(0, id.Target.Generation);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(42069)]
    [InlineData(0x7FFF)]
    [InlineData(0xFFFF)]
    [InlineData(0x1111)]
    [InlineData(0xABCD)]
    public void TypeId_TypeNumber_is_Congruent(ushort input)
    {
        var id = new TypeId {TypeNumber = input};
        Assert.Equal(input, id.TypeNumber);
        Assert.Equal(0, id.Generation);
        Assert.Equal(0, id.Target.Generation);
        Assert.Equal(0, id.Target.Id);
        Assert.Equal(0, id.Id);
    }
    */

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
        var id1 = TypeExpression.Create<int>();
        var id2 = TypeExpression.Create<int>();

        Assert.True(id1.Matches(id2));
    }

    [Fact]
    public void TypeAssigner_None_Matches_Default()
    {
        var id1 = TypeExpression.Create<int>();
        // Keeping the default case to ensure it remains at default
        // ReSharper disable once RedundantArgumentDefaultValue
        var id2 = TypeExpression.Create<int>(default);
        var id3 = TypeExpression.Create<int>(Identity.None);

        Assert.True(id1.Matches(id2));
        Assert.True(id1.Matches(id3));
        Assert.True(id2.Matches(id3));
        Assert.True(id3.Matches(id2));
    }

    [Fact]
    public void TypeAssigner_does_not_Match_Identical()
    {
        var id1 = TypeExpression.Create<int>();
        var id2 = TypeExpression.Create<float>();

        Assert.False(id1.Matches(id2));
    }

    [Fact]
    public void TypeAssigner_None_does_not_match_Any()
    {
        var id1 = TypeExpression.Create<int>();
        var id2 = TypeExpression.Create<int>(new Identity(123));
        var id3 = TypeExpression.Create<int>(Identity.Any);

        Assert.False(id1.Matches(id2));
        Assert.False(id1.Matches(id3));
    }

    [Fact]
    public void TypeId_from_Generic_is_same_as_Identify()
    {
        var id1 = TypeExpression.Create<int>().TypeId;
        var id2 = LanguageType.Identify(typeof(int));
        Assert.Equal(id1, id2);

        _ = TypeExpression.Create<string>();
        
        var id3 = LanguageType.Identify(typeof(bool));
        var id4 = TypeExpression.Create<bool>().TypeId;
        Assert.Equal(id3, id4);
        
        Assert.NotEqual( id1, id3);
        Assert.NotEqual( id2, id4);
    }
}