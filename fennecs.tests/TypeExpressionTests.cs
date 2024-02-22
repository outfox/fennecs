// SPDX-License-Identifier: MIT

namespace fennecs.tests;

public class TypeExpressionTests
{
    private struct TypeA;

    [Fact]
    public void Id_is_Comparable()
    {
        var t1 = TypeExpression.Create<TypeA>();
        var t2 = TypeExpression.Create<TypeA>();
        Assert.Equal(t1, t2);
    }

    [Fact]
    public void Id_is_Comparable_for_BaseTypes()
    {
        var t1 = TypeExpression.Create<double>();
        var t2 = TypeExpression.Create<double>();
        Assert.Equal(t1, t2);
    }

    [Fact]
    public void Is_Distinct()
    {
        var t1 = TypeExpression.Create<int>();
        var t2 = TypeExpression.Create<ushort>();
        Assert.NotEqual(t1, t2);
    }

    [Fact]
    public void Is_Sorted_By_TypeId_First()
    {
        var random = new Random(4711);
        for (var i = 0; i < 10_000; i++)
        {
            var id = random.Next();
            var deco = (TypeID) (random.Next() % TypeID.MaxValue);
            var t1 = new TypeExpression(new Entity(id, deco), (TypeID) i);
            var t2 = new TypeExpression(new Entity(id, deco), (TypeID) (i + 1));

            //  If this test fails, Archetypes will not be able to build immutable buckets for wildcards.
            Assert.True(t1.CompareTo(t2) < 0);
            Assert.True(t2.CompareTo(t1) > 0);
        }
    }

    [Fact]
    public void Implicitly_decays_to_Type()
    {
        var t1 = TypeExpression.Create<TypeA>().Type;
        var t2 = typeof(TypeA);
        Assert.Equal(t2, t1);
        Assert.Equal(t1, t2);
    }

    [Fact]
    public void Implicitly_decays_to_ulong()
    {
        ulong t1 = TypeExpression.Create<TypeA>();
        ulong t2 = TypeExpression.Create<TypeA>();
        ulong t3 = TypeExpression.Create<string>();
        Assert.Equal(t2, t1);
        Assert.NotEqual(t3, t1);
    }

    [Fact]
    public void Has_Equality_Operator()
    {
        var t1 = TypeExpression.Create<TypeA>();
        var t2 = TypeExpression.Create<TypeA>();
        var t3 = TypeExpression.Create<string>();
        Assert.True(t1 == t2);
        Assert.False(t1 == t3);
    }

    [Fact]
    public void Has_Inequality_Operator()
    {
        var t1 = TypeExpression.Create<TypeA>();
        var t2 = TypeExpression.Create<int>();
        var t3 = TypeExpression.Create<int>();
        Assert.True(t1 != t2);
        Assert.False(t3 != t2);
    }

    [Fact]
    public void Prevents_Boxing_Equality()
    {
        object o = "don't @ me";
        var id = TypeExpression.Create<TypeA>();
        Assert.Throws<InvalidCastException>(() => id.Equals(o));
    }

    [Fact]
    public void Can_Create_For_Type()
    {
        var tx1 = TypeExpression.Create(typeof(TypeA));
        var tx2 = TypeExpression.Create(typeof(TypeA), Entity.Any);
        var tx3 = TypeExpression.Create(typeof(TypeA), new Entity(123));

        Assert.False(tx1.isRelation);
        Assert.True(tx2.isRelation);
        Assert.True(tx3.isRelation);
    }
}