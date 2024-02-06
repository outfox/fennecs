// SPDX-License-Identifier: MIT

namespace fennecs.tests;

public class TypeExpressionTests
{
    private struct Type1;

    [Fact]
    public void Id_is_Comparable()
    {
        var t1 = TypeExpression.Create<Type1>();
        var t2 = TypeExpression.Create<Type1>();
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
    public void Id_is_Distinct()
    {
        var t1 = TypeExpression.Create<int>();
        var t2 = TypeExpression.Create<ushort>();
        Assert.NotEqual(t1 , t2);
    }
    
    [Fact]
    public void Id_implicitly_decays_to_Type()
    {
        var t1 = TypeExpression.Create<Type1>().Type;
        var t2 = typeof(Type1);
        Assert.Equal(t2, t1);
        Assert.Equal(t1, t2);
    }
}