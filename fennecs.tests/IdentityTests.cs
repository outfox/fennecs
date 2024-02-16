// SPDX-License-Identifier: MIT

namespace fennecs.tests;

public class IdentityTests(ITestOutputHelper output)
{
    [Fact]
    public void Virtual_Entities_have_no_Successors()
    {
        Assert.Throws<InvalidCastException>(() => Identity.Any.Successor);
        Assert.Throws<InvalidCastException>(() => Identity.None.Successor);
        Assert.Throws<InvalidCastException>(() => new Identity(typeof(bool)).Successor);
    }

    [Fact]
    public void Identity_Resolves_Type()
    {
        var boolType = new Identity(typeof(bool));
        Assert.Equal(typeof(bool), boolType.Type);

        Assert.Equal(typeof(LanguageType.Any), Identity.Any.Type);
        Assert.Equal(typeof(LanguageType.None), Identity.None.Type);

        using var world = new World();
        var entity = world.Spawn().Id();
        Assert.Equal(typeof(Entity), entity.Identity.Type);
    }
    
    [Fact]
    public void Identity_None_is_Zeros()
    {
        var none = Identity.None;
        Assert.Equal(default, none.Generation);
        output.WriteLine(none.Generation.ToString());
        output.WriteLine(none.ToString());
        Assert.Equal(default, none.Id);
    }

    [Fact]
    public void Identity_ToString()
    {
        output.WriteLine(Identity.None.ToString());
        output.WriteLine(Identity.Any.ToString());
        output.WriteLine(new Identity(123).ToString());
    }

    [Fact]
    public void Identity_None_cannot_Match_One()
    {
        var zero = new Identity(0);
        Assert.NotEqual(Identity.None, zero);

        var one = new Identity(1);
        Assert.NotEqual(Identity.None, one);
    }

    [Fact]
    public void Identity_Matches_Only_Self()
    {
        var self = new Identity(12345);
        Assert.Equal(self, self);

        var successor = new Identity(12345, 3);
        Assert.NotEqual(self, successor);

        var other = new Identity(9000, 3);
        Assert.NotEqual(self, other);

    }

    [Theory]
    [InlineData(1500, 1500)]
    internal void Identity_HashCodes_are_Unique(ushort idCount, ushort genCount)
    {
        var ids = new Dictionary<int, Identity>((int) (idCount * genCount * 4f));

        //Identities
        for (var i = 0; i < idCount ; i++)
        {
            //Generations
            for (ushort g = 1; g < genCount; g++)
            {
                var identity = new Identity(i, g);

                Assert.NotEqual(identity, Identity.Any);
                Assert.NotEqual(identity, Identity.None);

                if (ids.ContainsKey(identity.GetHashCode()))
                {
                    Assert.Fail($"Collision of {identity} with {ids[identity.GetHashCode()]}, #{identity.GetHashCode()}");
                }
                else
                {
                    ids.Add(identity.GetHashCode(), identity);
                }
            }
        }
    }

    [Fact]
    public void Equals_Prevents_Boxing_as_InvalidCastException()
    {
        object o = "don't @ me";
        var id = new Identity(69, 420);
        Assert.Throws<InvalidCastException>(() => id.Equals(o));
    }

    [Fact]
    public void Any_and_None_are_Distinct()
    {
        Assert.NotEqual(Identity.Any, Identity.None);
        Assert.NotEqual(Identity.Any.GetHashCode(), Identity.None.GetHashCode());
    }

    [Fact]
    public void Identity_Matches_Self_if_Same()
    {
        var random = new Random(420960);
        for (var i = 0; i < 1_000; i++)
        {
            var id = random.Next();
            var gen = (ushort) random.Next();
            
            var self = new Identity(id, gen);
            var other = new Identity(id, gen);

            Assert.Equal(self, other);
        }
    }
}