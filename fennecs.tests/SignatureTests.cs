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
            new[] {TypeExpression.Of<int>()}.ToImmutableSortedSet(),
            new[] {TypeExpression.Of<int>()}.ToImmutableSortedSet(),
        ];
        yield return
        [
            new[] {TypeExpression.Of<int>(), TypeExpression.Of<string>()}.ToImmutableSortedSet(),
            new[] {TypeExpression.Of<int>(), TypeExpression.Of<string>()}.ToImmutableSortedSet(),
        ];
        yield return
        [
            new[] {TypeExpression.Of<int>(), TypeExpression.Of<string>(Identity.Of("Hello World"))}.ToImmutableSortedSet(),
            new[] {TypeExpression.Of<string>(Identity.Of("Hello World")), TypeExpression.Of<int>()}.ToImmutableSortedSet(),
        ];
        yield return
        [
            new[] {TypeExpression.Of<int>(), TypeExpression.Of<string>()}.ToImmutableSortedSet(),
            new[] {TypeExpression.Of<string>(), TypeExpression.Of<int>()}.ToImmutableSortedSet(),
        ];
    }


    public static IEnumerable<object[]> NotEqualCases()
    {
        yield return
        [
            new[] {TypeExpression.Of<int>()}.ToImmutableSortedSet(),
            new[] {TypeExpression.Of<string>()}.ToImmutableSortedSet(),
        ];
        yield return
        [
            new[] {TypeExpression.Of<int>(), TypeExpression.Of<string>()}.ToImmutableSortedSet(),
            new[] {TypeExpression.Of<int>(), TypeExpression.Of<string>(Identity.Of("Hello World"))}.ToImmutableSortedSet(),
        ];
    }


    public static IEnumerable<object[]> AddCases()
    {
        yield return [TypeExpression.Of<int>()];
        yield return [TypeExpression.Of<float>(new Identity(id: 123))];
        yield return [TypeExpression.Of<Thread>()];
    }


    [Theory]
    [MemberData(nameof(EqualCases))]
    public void Signature_Hash_Identical(ImmutableSortedSet<TypeExpression> a, ImmutableSortedSet<TypeExpression> b)
    {
        var signatureA = new Signature<TypeExpression>(a);
        var signatureB = new Signature<TypeExpression>(b);

        Assert.Equal(signatureA, signatureB);
        Assert.Equal(signatureA.GetHashCode(), signatureB.GetHashCode());
    }


    [Theory]
    [MemberData(nameof(NotEqualCases))]
    public void Signature_Hash_Different(ImmutableSortedSet<TypeExpression> a, ImmutableSortedSet<TypeExpression> b)
    {
        var signatureA = new Signature<TypeExpression>(a);
        var signatureB = new Signature<TypeExpression>(b);

        Assert.NotEqual(signatureA, signatureB);
        Assert.NotEqual(signatureA.GetHashCode(), signatureB.GetHashCode());
    }


    [Theory]
    [MemberData(nameof(AddCases))]
    public void Signature_Add_Remove_Changes_and_Restores_Equality(TypeExpression type)
    {
        var signature = new Signature<TypeExpression>(Array.Empty<TypeExpression>());
        var changedSignature = signature.Add(type);

        Assert.NotEqual(signature, changedSignature);
        Assert.True(changedSignature.Contains(type));

        var restoredSignature = changedSignature.Remove(type);
        Assert.Equal(signature, restoredSignature);
    }


    [Fact]
    public void Signature_Determines_Set_Comparisons()
    {
        var signatureA = new Signature<TypeExpression>(TypeExpression.Of<int>(), TypeExpression.Of<string>());
        var signatureEqual = new Signature<TypeExpression>(TypeExpression.Of<int>(), TypeExpression.Of<string>());
        var signatureSubset = new Signature<TypeExpression>(TypeExpression.Of<int>());
        var signatureSuperset = new Signature<TypeExpression>(TypeExpression.Of<int>(), TypeExpression.Of<string>(), TypeExpression.Of<float>());


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
        var signatureA = new Signature<TypeExpression>(TypeExpression.Of<int>(), TypeExpression.Of<string>());
        var signatureB = new Signature<TypeExpression>(TypeExpression.Of<int>(), TypeExpression.Of<float>());
        var signatureC = new Signature<TypeExpression>(TypeExpression.Of<string>(), TypeExpression.Of<float>());
        var signatureD = new Signature<TypeExpression>(TypeExpression.Of<int>(), TypeExpression.Of<string>(), TypeExpression.Of<float>());

        var union = signatureA.Union(signatureB);
        var intersect = signatureA.Intersect(signatureC);
        var symmetricExcept = signatureA.SymmetricExcept(signatureD);
        var except = signatureA.Except(signatureB);

        Assert.Equal(signatureD, union);
        Assert.Equal(new Signature<TypeExpression>(TypeExpression.Of<string>()), intersect);
        Assert.Equal(new Signature<TypeExpression>(TypeExpression.Of<float>()), symmetricExcept);
        Assert.Equal(new Signature<TypeExpression>(TypeExpression.Of<string>()), except);
    }


    [Fact]
    public void Signature_Has_Equals()
    {
        var signatureA = new Signature<TypeExpression>(TypeExpression.Of<int>(), TypeExpression.Of<string>());
        var signatureEqual = new Signature<TypeExpression>(TypeExpression.Of<int>(), TypeExpression.Of<string>());
        var signatureSubset = new Signature<TypeExpression>(TypeExpression.Of<int>());
        var signatureSuperset = new Signature<TypeExpression>(TypeExpression.Of<int>(), TypeExpression.Of<string>(), TypeExpression.Of<float>());

        Assert.True(signatureA.Equals(signatureEqual));
        Assert.False(signatureA.Equals(signatureSubset));
        Assert.False(signatureA.Equals(signatureSuperset));
        
        Assert.False(signatureA.Equals(null));
        Assert.False(signatureA.Equals(new object()));
    }
    
    [Fact]
    public void Signature_Has_Enumerator()
    {
        var signature = new Signature<TypeExpression>(TypeExpression.Of<int>(), TypeExpression.Of<string>());
        using var enumerator = signature.GetEnumerator();
        
        Assert.True(enumerator.MoveNext());
        Assert.Equal(TypeExpression.Of<string>(), enumerator.Current);
        Assert.True(enumerator.MoveNext());
        Assert.Equal(TypeExpression.Of<int>(), enumerator.Current);
        Assert.False(enumerator.MoveNext());
    }


    [Fact]
    public void Signature_Has_Clear()
    {
        var signature = new Signature<TypeExpression>(TypeExpression.Of<int>(), TypeExpression.Of<string>());
        var cleared = signature.Clear();
        Assert.Empty(cleared);
    }
    
    
    [Fact]
    public void Signature_Has_TryGetValue()
    {
        var signature = new Signature<TypeExpression>(TypeExpression.Of<int>(), TypeExpression.Of<string>());
        Assert.True(signature.TryGetValue(TypeExpression.Of<int>(), out var value));
        Assert.Equal(TypeExpression.Of<int>(), value);
        Assert.False(signature.TryGetValue(TypeExpression.Of<float>(), out _));
    }


    [Fact]
    public void Signature_Has_Equality_Operator()
    {
        var signatureA = new Signature<TypeExpression>(TypeExpression.Of<int>(), TypeExpression.Of<string>());
        var signatureEqual = new Signature<TypeExpression>(TypeExpression.Of<int>(), TypeExpression.Of<string>());
        var signatureSubset = new Signature<TypeExpression>(TypeExpression.Of<int>());
        var signatureSuperset = new Signature<TypeExpression>(TypeExpression.Of<int>(), TypeExpression.Of<string>(), TypeExpression.Of<float>());

        Assert.True(signatureA == signatureEqual);
        Assert.False(signatureA == signatureSubset);
        Assert.True(signatureA != signatureSubset);
        Assert.False(signatureA == signatureSuperset);
        Assert.True(signatureA != signatureSuperset);
    }


    [Fact]
    public void Signature_Has_Indexer()
    {
        var signature = new Signature<TypeExpression>(TypeExpression.Of<int>(), TypeExpression.Of<string>());
        Assert.Equal(TypeExpression.Of<string>(), signature[0]);
        Assert.Equal(TypeExpression.Of<int>(), signature[1]);
    }

    [Fact]
    public void Signature_Has_ToString()
    {
        var tInt = TypeExpression.Of<int>();
        var tString = TypeExpression.Of<string>();
        var signature = new Signature<TypeExpression>(tInt, tString);
        Assert.Contains(tString.ToString(), signature.ToString());
        Assert.Contains(tInt.ToString(), signature.ToString());
    }
    
    [Fact]
    public void Signature_Has_Blank_Enumerator()
    {
        var signature = new Signature<TypeExpression>(TypeExpression.Of<int>(), TypeExpression.Of<string>());

        IEnumerable enumerable = signature;
        
        foreach (var expr in enumerable)
        {
            if (expr is TypeExpression expression)
            {
                Assert.True(expression.Equals(TypeExpression.Of<int>()) || expression.Equals(TypeExpression.Of<string>()));
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
        var signature = new Signature<TypeExpression>(TypeExpression.Of<int>(), TypeExpression.Of<string>());
        Assert.True(signature.CompareTo(default) > 0);
    }

    [Fact]
    public void Differing_Signature_Of_Same_Length_Comparable_Complementary()
    {
        var signature1 = new Signature<TypeExpression>(TypeExpression.Of<int>(), TypeExpression.Of<string>());
        var signature2 = new Signature<TypeExpression>(TypeExpression.Of<int>(), TypeExpression.Of<float>());
        
        Assert.Equal(-1 * signature1.CompareTo(signature2), signature2.CompareTo(signature1));
    }
}