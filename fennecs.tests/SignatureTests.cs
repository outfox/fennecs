using System.Collections;
using System.Collections.Immutable;

namespace fennecs.tests;

public class SignatureTests
{
    public static IEnumerable<object[]> EqualCases()
    {
        yield return
        [
            ImmutableSortedSet<TypeExpression>.Empty,
            ImmutableSortedSet<TypeExpression>.Empty,
        ];
        yield return
        [
            new[] { TypeExpression.Of<int>() }.ToImmutableSortedSet(),
            new[] { TypeExpression.Of<int>() }.ToImmutableSortedSet(),
        ];
        yield return
        [
            new[] { TypeExpression.Of<int>(), TypeExpression.Of<string>((Key) default) }.ToImmutableSortedSet(),
            new[] { TypeExpression.Of<int>(), TypeExpression.Of<string>((Key) default) }.ToImmutableSortedSet(),
        ];
        yield return
        [
            new[] { TypeExpression.Of<int>(), TypeExpression.Of("Hello World") }.ToImmutableSortedSet(),
            new[] { TypeExpression.Of<string>("Hello World"), TypeExpression.Of<int>() }.ToImmutableSortedSet(),
        ];
        yield return
        [
            new[] { TypeExpression.Of<int>(), TypeExpression.Of<string>((Key) default) }.ToImmutableSortedSet(),
            new[] { TypeExpression.Of<string>((Key) default), TypeExpression.Of<int>() }.ToImmutableSortedSet(),
        ];
    }


    public static IEnumerable<object[]> NotEqualCases()
    {
        yield return
        [
            new[] { TypeExpression.Of<int>() }.ToImmutableSortedSet(),
            new[] { TypeExpression.Of<string>((Key) default) }.ToImmutableSortedSet(),
        ];
        yield return
        [
            new[] { TypeExpression.Of<int>(), TypeExpression.Of<string>((Key) default) }.ToImmutableSortedSet(),
            new[] { TypeExpression.Of<int>(), TypeExpression.Of<string>("Hello World") }.ToImmutableSortedSet(),
        ];
    }


    public static IEnumerable<object[]> AddCases()
    {
        yield return [TypeExpression.Of<int>()];
        yield return [TypeExpression.Of<float>(new Entity(1, 123))];
        yield return [TypeExpression.Of<Thread>((Key) default)];
    }


    [Theory]
    [MemberData(nameof(EqualCases))]
    internal void Signature_Hash_Identical(ImmutableSortedSet<TypeExpression> a, ImmutableSortedSet<TypeExpression> b)
    {
        var signatureA = new Signature(a);
        var signatureB = new Signature(b);

        Assert.Equal(signatureA, signatureB);
        Assert.Equal(signatureA.GetHashCode(), signatureB.GetHashCode());
    }


    [Theory]
    [MemberData(nameof(NotEqualCases))]
    internal void Signature_Hash_Different(ImmutableSortedSet<TypeExpression> a, ImmutableSortedSet<TypeExpression> b)
    {
        var signatureA = new Signature(a);
        var signatureB = new Signature(b);

        Assert.NotEqual(signatureA, signatureB);
        Assert.NotEqual(signatureA.GetHashCode(), signatureB.GetHashCode());
    }


    [Theory]
    [MemberData(nameof(AddCases))]
    internal void Signature_Add_Remove_Changes_and_Restores_Equality(TypeExpression type)
    {
        var signature = new Signature(Array.Empty<TypeExpression>());
        var changedSignature = signature.Add(type);

        Assert.NotEqual(signature, changedSignature);
        Assert.True(changedSignature.Matches(type));

        var restoredSignature = changedSignature.Remove(type);
        Assert.Equal(signature, restoredSignature);
    }


    [Fact]
    public void Signature_Determines_Set_Comparisons()
    {
        var signatureA = new Signature(TypeExpression.Of<int>(), TypeExpression.Of<string>((Key) default));
        var signatureEqual = new Signature(TypeExpression.Of<int>(), TypeExpression.Of<string>((Key) default));
        var signatureSubset = new Signature(TypeExpression.Of<int>());
        var signatureSuperset = new Signature(TypeExpression.Of<int>(), TypeExpression.Of<string>((Key) default), TypeExpression.Of<float>());


        Assert.True(signatureA.SetEquals(signatureEqual));
        Assert.True(signatureA.IsSubsetOf(signatureSuperset));
        Assert.True(signatureA.IsSupersetOf(signatureSubset));
        Assert.True(signatureA.IsProperSubsetOf(signatureSuperset));
        Assert.True(signatureA.IsProperSupersetOf(signatureSubset));
        Assert.True(signatureA.Overlaps(signatureSuperset));
        Assert.True(signatureA.Overlaps(signatureSubset));
        Assert.True(signatureA.Overlaps(signatureEqual));
    }

    [Fact]
    public void Signature_Union_Intersect_SymmetricExcept()
    {
        var signatureA = new Signature(TypeExpression.Of<int>(), TypeExpression.Of<string>((Key) default));
        var signatureB = new Signature(TypeExpression.Of<int>(), TypeExpression.Of<float>());
        var signatureC = new Signature(TypeExpression.Of<string>((Key) default), TypeExpression.Of<float>());
        var signatureD = new Signature(TypeExpression.Of<int>(), TypeExpression.Of<string>((Key) default), TypeExpression.Of<float>());

        var union = signatureA.Union(signatureB);
        var intersect = signatureA.Intersect(signatureC);
        var symmetricExcept = signatureA.SymmetricExcept(signatureD);
        var except = signatureA.Except(signatureB);

        Assert.Equal(signatureD, union);
        Assert.Equal(new Signature(TypeExpression.Of<string>((Key) default)), intersect);
        Assert.Equal(new Signature(TypeExpression.Of<float>()), symmetricExcept);
        Assert.Equal(new Signature(TypeExpression.Of<string>((Key) default)), except);
    }


    [Fact]
    public void Signature_Has_Equals()
    {
        var signatureA = new Signature(TypeExpression.Of<int>(), TypeExpression.Of<string>((Key) default));
        var signatureEqual = new Signature(TypeExpression.Of<int>(), TypeExpression.Of<string>((Key) default));
        var signatureSubset = new Signature(TypeExpression.Of<int>());
        var signatureSuperset = new Signature(TypeExpression.Of<int>(), TypeExpression.Of<string>((Key) default), TypeExpression.Of<float>());

        Assert.True(signatureA.Equals(signatureEqual));
        Assert.False(signatureA.Equals(signatureSubset));
        Assert.False(signatureA.Equals(signatureSuperset));

        Assert.False(signatureA.Equals(null));
        Assert.False(signatureA.Equals(new object()));
    }

    [Fact]
    public void Signature_Has_Enumerator()
    {
        var signature = new Signature(TypeExpression.Of<int>(), TypeExpression.Of<string>((Key) default));
        using var enumerator = signature.GetEnumerator();

        Assert.True(enumerator.MoveNext());
        Assert.Equal(TypeExpression.Of<int>(), enumerator.Current);
        Assert.True(enumerator.MoveNext());
        Assert.Equal(TypeExpression.Of<string>((Key) default), enumerator.Current);
        Assert.False(enumerator.MoveNext());
    }


    [Fact]
    public void Signature_Has_Clear()
    {
        var signature = new Signature(TypeExpression.Of<int>(), TypeExpression.Of<string>((Key) default));
        var cleared = signature.Clear();
        Assert.Empty(cleared);
    }


    [Fact]
    public void Signature_Has_TryGetValue()
    {
        var signature = new Signature(TypeExpression.Of<int>(), TypeExpression.Of<string>((Key) default));
        Assert.True(signature.TryGetValue(TypeExpression.Of<int>(), out var value));
        Assert.Equal(TypeExpression.Of<int>(), value);
        Assert.False(signature.TryGetValue(TypeExpression.Of<float>(), out _));
    }


    [Fact]
    public void Signature_Has_Equality_Operator()
    {
        var signatureA = new Signature(TypeExpression.Of<int>(), TypeExpression.Of<string>((Key) default));
        var signatureEqual = new Signature(TypeExpression.Of<int>(), TypeExpression.Of<string>((Key) default));
        var signatureSubset = new Signature(TypeExpression.Of<int>());
        var signatureSuperset = new Signature(TypeExpression.Of<int>(), TypeExpression.Of<string>((Key) default), TypeExpression.Of<float>());

        Assert.True(signatureA == signatureEqual);
        Assert.False(signatureA == signatureSubset);
        Assert.True(signatureA != signatureSubset);
        Assert.False(signatureA == signatureSuperset);
        Assert.True(signatureA != signatureSuperset);
    }


    [Fact]
    public void Signature_Has_Indexer()
    {
        var signature = new Signature(TypeExpression.Of<int>(), TypeExpression.Of<string>((Key) default));
        Assert.Equal(TypeExpression.Of<int>(), signature[0]);
        Assert.Equal(TypeExpression.Of<string>((Key) default), signature[1]);
    }

    [Fact]
    public void Signature_Has_ToString()
    {
        var tInt = TypeExpression.Of<int>();
        var tString = TypeExpression.Of<string>((Key) default);
        var signature = new Signature(tInt, tString);
        Assert.Contains(tString.ToString(), signature.ToString());
        Assert.Contains(tInt.ToString(), signature.ToString());
    }

    [Fact]
    public void Signature_Has_Blank_Enumerator()
    {
        var signature = new Signature(TypeExpression.Of<int>(), TypeExpression.Of<string>((Key) default));

        IEnumerable enumerable = signature;

        foreach (var expr in enumerable)
        {
            if (expr is TypeExpression expression)
            {
                Assert.True(expression.Equals(TypeExpression.Of<int>()) || expression.Equals(TypeExpression.Of<string>((Key) default)));
            }
            else
            {
                Assert.Fail();
            }
        }
    }

    [Fact]
    public void Signature_Always_Greater_Than_Default()
    {
        var signature = new Signature(TypeExpression.Of<int>(), TypeExpression.Of<string>((Key) default));
        Assert.True(signature.CompareTo(default) > 0);
    }
    
    [Fact]
    public void Differing_Signature_Of_Same_Length_Comparable_Complementary()
    {
        var signature1 = new Signature(TypeExpression.Of<int>(), TypeExpression.Of<string>((Key) default));
        var signature2 = new Signature(TypeExpression.Of<int>(), TypeExpression.Of<float>());

        Assert.Equal(-1 * signature1.CompareTo(signature2), signature2.CompareTo(signature1));
    }

    [Fact]
    public void Signature_Can_Match_Component_Sets()
    {
        var signature1 = new Signature(TypeExpression.Of<int>(), TypeExpression.Of<string>((Key) default));
        
        ImmutableSortedSet<Comp> componentSet1 = [Comp<int>.Plain];
        ImmutableSortedSet<Comp> componentSet2 = [Comp<string>.Plain];
        ImmutableSortedSet<Comp> componentSet3 = [Comp<float>.Plain];
        
        Assert.True(signature1.Matches(componentSet1));
        Assert.True(signature1.Matches(componentSet2));
        Assert.False(signature1.Matches(componentSet3));
    }
}
