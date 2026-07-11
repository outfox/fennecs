// SPDX-License-Identifier: MIT

namespace fennecs.tests;

public class KeyTests
{
    [Fact]
    public void Is_Ordered_by_Raw_Value()
    {
        Assert.True(Key.AnyEntity.CompareTo(Key.AnyObject) < 0);
        Assert.True(Key.AnyObject.CompareTo(Key.AnyEntity) > 0);
        Assert.Equal(0, Key.Target.CompareTo(Key.Target));

        var e1 = new Entity(1, 1, 1).Key;
        var e2 = new Entity(1, 2, 1).Key;
        Assert.True(e1.CompareTo(e2) < 0);
    }


    [Fact]
    public void ToString_Describes_All_Categories()
    {
        Assert.Equal("[None]", Key.Plain.ToString());

        Assert.Equal("wildcard[Any]", Key.Any.ToString());
        Assert.Equal("wildcard[Target]", Key.Target.ToString());
        Assert.Equal("wildcard[Entity]", Key.AnyEntity.ToString());
        Assert.Equal("wildcard[Object]", Key.AnyObject.ToString());

        var entity = new Entity(1, 123, 1);
        Assert.StartsWith("E-", entity.Key.ToString());

        var link = Key.Of("hello");
        Assert.StartsWith("O-<System.String>", link.ToString());
    }


    [Fact]
    public void ToString_Describes_Forged_Keys()
    {
        // A Wildcard nibble outside the recognized categories (reserved: Family).
        var forgedWildcard = new Key((ulong) SecondaryKind.Family << Key.KindShift);
        Assert.True(forgedWildcard.IsWildcard);
        Assert.StartsWith("wildcard[?-", forgedWildcard.ToString());

        // A specific (non-wildcard) Key that is neither an Entity nor an Object.
        var forgedKey = new Key(((ulong) SecondaryKind.Family << Key.KindShift) | 42);
        Assert.False(forgedKey.IsWildcard);
        Assert.False(forgedKey.IsEntity);
        Assert.False(forgedKey.IsObject);
        Assert.StartsWith("?-", forgedKey.ToString());
    }


    [Fact]
    public void EntityIndex_Prints_as_Hex()
    {
        Assert.Equal("#00000abc", new EntityIndex(0xABC).ToString());
    }
}
