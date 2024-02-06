// SPDX-License-Identifier: MIT

namespace fennecs.tests;

public class MaskTests
{
    [Fact]
    public void Masks_are_distinct_from_default()
    {
        var mask1 = new Mask();
        Assert.NotEqual(default, mask1);
        Assert.False(mask1.Equals(null));
    }

    [Fact]
    public void Empty_Masks_are_Indistinct()
    {
        var mask1 = new Mask();
        var mask2 = new Mask();
        Assert.Equal(mask1, mask2);
    }

    [Fact]
    public void Masks_can_become_Distinct()
    {
        var mask1 = new Mask();
        var mask2 = new Mask();

        mask2.Has(TypeExpression.Create<int>());
        Assert.NotEqual(mask1, mask2);

        mask1.Has(TypeExpression.Create<int>());
        Assert.Equal(mask1, mask2);

        mask1.Any(TypeExpression.Create<float>());
        Assert.NotEqual(mask1, mask2);

        mask2.Any(TypeExpression.Create<float>());
        Assert.Equal(mask1, mask2);

        mask2.Not(TypeExpression.Create<string>());
        Assert.NotEqual(mask1, mask2);

        mask1.Not(TypeExpression.Create<string>());
        Assert.Equal(mask1, mask2);
    }

    [Fact]
    public void Clear_makes_masks_Equal_to_empty()
    {
        var mask1 = new Mask();
        var mask2 = new Mask();

        mask2.Has(TypeExpression.Create<int>());
        mask2.Any(TypeExpression.Create<float>());
        mask2.Not(TypeExpression.Create<string>());
        
        mask2.Clear();
        Assert.Equal(mask1, mask2);
    }
}