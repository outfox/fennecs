// SPDX-License-Identifier: MIT

namespace fennecs.tests;

public class EntityGetTests
{
    [Fact]
    public void Can_Get_Link_Object_via_Get()
    {
        using var world = new World();
        var entity = world.Spawn();
        var literal = "hello world";
        Name helloWorld = new(literal);
        entity.Add(Link.With(helloWorld));
        entity.Add(Link.With(literal));
        var strings = entity.Get<Name>(Match.Any);
        Assert.Equal(helloWorld, strings[0]);
        Assert.Single(strings);
    }

    [Fact]
    public void Can_Get_Link_Objects_via_Get()
    {
        using var world = new World();
        var entity = world.Spawn();
        const string literal1 = "hello world1";
        var literal2 = "hello world2";
        Name helloWorld1 = new(literal1);
        Name helloWorld2 = new(literal2);
        entity.Add(Link.With(helloWorld1));
        entity.Add(Link.With(helloWorld2));
        var strings = entity.Get<Name>(Match.Any);
        Assert.Contains(helloWorld1, strings);
        Assert.Contains(helloWorld2, strings);
        Assert.Equal(2, strings.Length);
    }

    // ReSharper disable once NotAccessedPositionalProperty.Local
    private record Name(string _);
}
