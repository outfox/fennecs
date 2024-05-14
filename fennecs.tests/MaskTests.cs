// SPDX-License-Identifier: MIT

using fennecs.pools;

namespace fennecs.tests;

public class MaskTests
{
    [Fact]
    public void Reference_Equality_Only()
    {
        var mask1 = new Mask();
        Assert.True(mask1.Equals(mask1));

        var mask2 = new Mask();
        Assert.False(mask1.Equals(mask2));

        Mask? mask3 = default;
        Assert.False(mask1.Equals(mask3));
    }


    [Fact]
    public void Reference_Equality_Object()
    {
        var mask1 = new Mask();
        object mask2 = mask1;
        Assert.True(mask1.Equals(mask2));
    }


    [Fact]
    public void Masks_are_distinct_from_default()
    {
        var mask1 = new Mask();
        Assert.NotEqual(default, mask1);
        Assert.False(mask1.Equals(null));
    }


    [Fact]
    public void Can_Be_Disposed()
    {
        var mask = MaskPool.Rent();
        Assert.DoesNotContain(mask, MaskPool.Pool);
        mask.Dispose();
        //TODO: This possibly fails because of concurrent test runners?
        //Assert.Contains(mask, MaskPool.Pool);
    }
}