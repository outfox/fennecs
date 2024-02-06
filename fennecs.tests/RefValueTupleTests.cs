// SPDX-License-Identifier: MIT

namespace fennecs.tests;

public class RefTupleTests
{
    [Fact]
    private void RefValueTuple_Maintains_Refs()
    {
        var a = 1;
        var b = 2;
        
        var t = new RefValueTuple<int, int>(ref a, ref b);
        Assert.Equal(1, t.Item1);
        Assert.Equal(2, t.Item2);
        
        t.Item1.Value = 3;
        t.Item2.Value = 4;
        Assert.Equal(3, a);
        Assert.Equal(4, b);
        
        var (x, y) = t;
        Assert.Equal(3, x);
        Assert.Equal(4, y);
        
        b = 5;
        Assert.Equal(5, t.Item2);

        x.Value = 9;
        Assert.Equal(9, a);
    }
}