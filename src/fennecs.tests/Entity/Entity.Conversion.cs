// SPDX-License-Identifier: MIT

namespace fennecs.tests;

public class EntityConversionTests(ITestOutputHelper output)
{
    [Fact]
    public void Entity_Decays_to_Identity()
    {
        using var world = new World();
        var entity = world.Spawn();
        Identity identity = entity;
        Assert.Equal(entity.Id, identity);
    }

    [Fact]
    public void Entity_has_ToString()
    {
        using var world = new World();
        var entity = world.Spawn();
        var builder = new Entity(world, entity.Id);
        Assert.Equal(entity.ToString(), builder.ToString());

        entity.Add(123);
        entity.Add(7.0f, world.Spawn());
        entity.Add(Link.With("hello"));
        output.WriteLine(entity.ToString());
        
        world.Despawn(entity);
        output.WriteLine(entity.ToString());
    }
}
