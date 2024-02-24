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
            var t1 = new TypeExpression(new Identity(id, deco), (TypeID) i);
            var t2 = new TypeExpression(new Identity(id, deco), (TypeID) (i + 1));

            //  If this test fails, Archetypes will not be able to build immutable buckets for Wildcards.
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
        var tx2 = TypeExpression.Create(typeof(TypeA), Match.Any);
        var tx3 = TypeExpression.Create(typeof(TypeA), new Identity(123));

        Assert.False(tx1.isRelation);
        Assert.True(tx2.isRelation);
        Assert.True(tx3.isRelation);
    }

    [Fact]
    public void None_Matches_only_None()
    {
        var none = TypeExpression.Create<TypeA>(Match.Plain);
        var any = TypeExpression.Create<TypeA>(Match.Any);
        var obj = TypeExpression.Create<TypeA>(Match.Object);
        var rel = TypeExpression.Create<TypeA>(Match.Identity);

        var ent = TypeExpression.Create<TypeA>(new Identity(123));
        var lnk = TypeExpression.Create<TypeA>(Identity.Of("hello world"));

        Assert.True(none.Matches(none));
        Assert.False(none.Matches(any));
        Assert.False(none.Matches(obj));
        Assert.False(none.Matches(rel));
        Assert.False(none.Matches(ent));
        Assert.False(none.Matches(lnk));
    }

    [Fact]
    public void Any_Matches_only_All()
    {
        var any = TypeExpression.Create<TypeA>(Match.Any);
        
        var typ = TypeExpression.Create<TypeA>();
        var ent = TypeExpression.Create<TypeA>(new Identity(123));
        var lnk = TypeExpression.Create<TypeA>(Identity.Of("hello world"));

        Assert.True(any.Matches(typ));
        Assert.True(any.Matches(ent));
        Assert.True(any.Matches(lnk));
    }

    [Fact]
    public void Object_Matches_only_Objects()
    {
        var obj = TypeExpression.Create<TypeA>(Match.Object);
        
        var typ = TypeExpression.Create<TypeA>();
        var ent = TypeExpression.Create<TypeA>(new Identity(123));
        var lnk = TypeExpression.Create<TypeA>(Identity.Of("hello world"));

        Assert.False(obj.Matches(typ));
        Assert.False(obj.Matches(ent));
        Assert.True(obj.Matches(lnk));
    }

    [Fact]
    public void Relation_Matches_only_Relations()
    {
        var rel = TypeExpression.Create<TypeA>(Match.Identity);
        
        var typ = TypeExpression.Create<TypeA>();
        var ent = TypeExpression.Create<TypeA>(new Identity(123));
        var lnk = TypeExpression.Create<TypeA>(Identity.Of("hello world"));

        Assert.False(rel.Matches(typ));
        Assert.True(rel.Matches(ent));
        Assert.False(rel.Matches(lnk));
    }
    
    [Fact]
    public void Target_Matches_all_Entity_Target_Relations()
    {
        var rel = TypeExpression.Create<TypeA>(Match.Relation);
        
        var typ = TypeExpression.Create<TypeA>();
        var ent = TypeExpression.Create<TypeA>(new Identity(123));
        var lnk = TypeExpression.Create<TypeA>(Identity.Of("hello world"));

        Assert.False(rel.Matches(typ));
        Assert.True(rel.Matches(ent));
        Assert.True(rel.Matches(lnk));
    }

    [Fact]
    public void Entity_only_matches_Entity()
    {
        var ent = TypeExpression.Create<TypeA>(new Identity(123));
        
        var typ = TypeExpression.Create<TypeA>();
        var lnk = TypeExpression.Create<TypeA>(Identity.Of("hello world"));

        Assert.False(ent.Matches(typ));
        Assert.True(ent.Matches(ent));
        Assert.False(ent.Matches(lnk));
    }
}