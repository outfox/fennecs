// SPDX-License-Identifier: MIT

namespace fennecs.tests;

public class TypeExpressionTests(ITestOutputHelper output)
{
    [Fact]
    public void To_String()
    {
        output.WriteLine(TypeExpression.Of<TypeA>(Identity.Plain).ToString());
        output.WriteLine(TypeExpression.Of<TypeA>(Identity.Any).ToString());
        output.WriteLine(TypeExpression.Of<TypeA>(Identity.Object).ToString());
        output.WriteLine(TypeExpression.Of<TypeA>(Identity.Entity).ToString());
        output.WriteLine(TypeExpression.Of<TypeA>(new(new(123))).ToString());
    }


    [Fact]
    public void Id_is_Comparable()
    {
        var t1 = TypeExpression.Of<TypeA>(Identity.Plain);
        var t2 = TypeExpression.Of<TypeA>(Identity.Plain);
        Assert.Equal(t1, t2);
    }


    [Fact]
    public void Id_is_Comparable_for_BaseTypes()
    {
        var t1 = TypeExpression.Of<double>(Identity.Plain);
        var t2 = TypeExpression.Of<double>(Identity.Plain);
        Assert.Equal(t1, t2);
    }


    [Fact]
    public void Is_Distinct()
    {
        var t1 = TypeExpression.Of<int>(Identity.Plain);
        var t2 = TypeExpression.Of<ushort>(Identity.Plain);
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
            var t1 = new TypeExpression(new Target(new Identity(id, deco)), (TypeID) i);
            var t2 = new TypeExpression(new Target(new Identity(id, deco)), (TypeID) (i + 1));

            //  If this test fails, Archetypes will not be able to build immutable buckets for Wildcards.
            Assert.True(t1.CompareTo(t2) > 0);
            Assert.True(t2.CompareTo(t1) < 0);
        }
    }


    [Fact]
    public void Implicitly_decays_to_Type()
    {
        var t1 = TypeExpression.Of<TypeA>(Identity.Plain).Type;
        var t2 = typeof(TypeA);
        Assert.Equal(t2, t1);
        Assert.Equal(t1, t2);
    }


    [Fact]
    public void Has_Equality_Operator()
    {
        var t1 = TypeExpression.Of<TypeA>(Identity.Plain);
        var t2 = TypeExpression.Of<TypeA>(Identity.Plain);
        var t3 = TypeExpression.Of<string>(Identity.Plain);
        Assert.True(t1 == t2);
        Assert.False(t1 == t3);
    }


    [Fact]
    public void Has_Inequality_Operator()
    {
        var t1 = TypeExpression.Of<TypeA>(Identity.Plain);
        var t2 = TypeExpression.Of<int>(Identity.Plain);
        var t3 = TypeExpression.Of<int>(Identity.Plain);
        Assert.True(t1 != t2);
        Assert.False(t3 != t2);
    }


    [Fact]
    public void Prevents_Boxing_Equality()
    {
        object o = "don't @ me";
        var id = TypeExpression.Of<TypeA>(Identity.Plain);
        Assert.Throws<InvalidCastException>(() => id.Equals(o));
    }


    [Fact]
    public void Can_Create_For_Type()
    {
        var tx1 = TypeExpression.Of(typeof(TypeA), Identity.Plain);
        var tx2 = TypeExpression.Of(typeof(TypeA), Identity.Any);
        var tx3 = TypeExpression.Of(typeof(TypeA), new Entity(null!, new(123)));

        Assert.False(tx1.isRelation);
        Assert.True(tx2.isWildcard);
        Assert.True(tx3.isRelation);
    }


    [Fact]
    public void None_Matches_only_None()
    {
        var none = TypeExpression.Of<TypeA>(Identity.Plain);
        var any = TypeExpression.Of<TypeA>(Identity.Any);
        var obj = TypeExpression.Of<TypeA>(Identity.Object);
        var rel = TypeExpression.Of<TypeA>(Identity.Entity);

        var ent = TypeExpression.Of<TypeA>(new Entity(null!, new(123)));
        var lnk = TypeExpression.Of<TypeA>(Link.With("hello world"));

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
        var any = TypeExpression.Of<TypeA>(Identity.Any);

        var typ = TypeExpression.Of<TypeA>(Identity.Plain);
        var ent = TypeExpression.Of<TypeA>(new Entity(null!, new(123)));
        var lnk = TypeExpression.Of<TypeA>(Link.With("hello world"));

        Assert.True(any.Matches(typ));
        Assert.True(any.Matches(ent));
        Assert.True(any.Matches(lnk));
    }


    [Fact]
    public void Object_Matches_only_Objects()
    {
        var obj = TypeExpression.Of<TypeA>(Identity.Object);

        var typ = TypeExpression.Of<TypeA>(Identity.Plain);
        var ent = TypeExpression.Of<TypeA>(new Entity(null!, new(123)));
        var lnk = TypeExpression.Of<TypeA>(Link.With("hello world"));

        Assert.False(obj.Matches(typ));
        Assert.False(obj.Matches(ent));
        Assert.True(obj.Matches(lnk));
    }


    [Fact]
    public void Relation_Matches_only_Relations()
    {
        var rel = TypeExpression.Of<TypeA>(Identity.Entity);

        var typ = TypeExpression.Of<TypeA>(Identity.Plain);
        var ent = TypeExpression.Of<TypeA>(new Entity(null!, new(123)));
        var lnk = TypeExpression.Of<TypeA>(Link.With("hello world"));

        Assert.False(rel.Matches(typ));
        Assert.True(rel.Matches(ent));
        Assert.False(rel.Matches(lnk));
    }


    [Fact]
    public void Target_Matches_all_Entity_Target_Relations()
    {
        var rel = TypeExpression.Of<TypeA>(Identity.Target);

        var typ = TypeExpression.Of<TypeA>(Identity.Plain);
        var ent = TypeExpression.Of<TypeA>(new Entity(null!, new(123)));
        var lnk = TypeExpression.Of<TypeA>(Link.With("hello world"));

        Assert.False(rel.Matches(typ));
        Assert.True(rel.Matches(ent));
        Assert.True(rel.Matches(lnk));
    }


    [Fact]
    public void Entity_only_matches_Entity()
    {
        var typ = TypeExpression.Of<TypeA>(Identity.Plain);
        var ent = TypeExpression.Of<TypeA>(new Entity(null!, new(123)));
        var lnk = TypeExpression.Of<TypeA>(Link.With("hello world"));

        Assert.False(ent.Matches(typ));
        Assert.True(ent.Matches(ent));
        Assert.False(ent.Matches(lnk));
    }


    private struct TypeA;
}