// SPDX-License-Identifier: MIT

namespace fennecs.tests;

public class EntityConversionTests(ITestOutputHelper output)
{
    [Fact]
    public void Entity_Decays_to_Key()
    {
        using var world = new World();
        var entity = world.Spawn();
        Key key = entity;
        Assert.Equal(entity.Key, key);
        Assert.True(key.IsEntity);
    }

    [Fact]
    public void Entity_has_ToString()
    {
        using var world = new World();
        var entity = world.Spawn();
        var builder = new Entity(entity.ToRaw());
        Assert.Equal(entity.ToString(), builder.ToString());

        entity.Add(123);
        entity.Add(7.0f, world.Spawn());
        entity.Add(Link.With("hello"));
        output.WriteLine(entity.ToString());

        world.Despawn(entity);
        output.WriteLine(entity.ToString());
    }

    [Fact]
    public void Entity_has_ToRaw()
    {
        using var world = new World();
        var entity1 = world.Spawn();
        var entity2 = world.Spawn();

        Assert.NotEqual(0UL, entity1.ToRaw());
        Assert.NotEqual(entity1.ToRaw(), entity2.ToRaw());

        // same entity always yields the same raw value
        Assert.Equal(entity1.ToRaw(), new Entity(entity1.ToRaw()).ToRaw());
    }

    [Fact]
    public void Entity_has_Dump()
    {
        using var world = new World();
        var entity = world.Spawn().Add(123).Add("attached");

        var dump = entity.Dump();
        output.WriteLine(dump);

        Assert.StartsWith(entity.ToString(), dump);
        Assert.Contains(TypeExpression.Of<int>(Match.Plain).ToString(), dump);
        Assert.Contains(TypeExpression.Of<string>(Match.Plain).ToString(), dump);

        world.Despawn(entity);
        var deadDump = entity.Dump();
        output.WriteLine(deadDump);

        Assert.Contains("-DEAD-", deadDump);
    }
}
