// SPDX-License-Identifier: MIT

using fennecs.CRUD;

namespace fennecs.tests;

public class EntityHasTests
{
    [Fact]
    public void Entity_provides_Has()
    {
        using var world = new World();
        var entity = world.Spawn();
        entity.Add(123);

        Assert.True(entity.Has<int>());
        Assert.True(entity.Has<int>(Match.Plain));
        Assert.True(entity.Has<int>(Match.Any));

        Assert.False(entity.Has<int>(Entity.Any));
        Assert.False(entity.Has<int>(Link.Any));
        Assert.False(entity.Has<int>(Match.Target));

        Assert.False(entity.Has<float>(Match.Any));
    }

    [Fact]
    public void Entity_provides_HasLink()
    {
        using var world = new World();
        var entity = world.Spawn();
        world.Spawn();
        entity.Add(Link.With("hello world"));

        Assert.True(entity.Has<string>("hello world"));
        Assert.True(entity.Has<string>(Match.Any));
        Assert.True(entity.Has<string>(Match.Object));
        Assert.True(entity.Has<string>(Link.Any));
        Assert.True(entity.Has<string>(Match.Target));

        Assert.False(entity.Has<string>("goodbye world"));
        Assert.False(entity.Has<int>(Match.Entity));
    }

    [Fact]
    public void Entity_provides_Has_overload_With_Plain_MatchExpression()
    {
        using var world = new World();
        var entity = world.Spawn();
        world.Spawn();
        entity.Add(Link.With("hello world"));
        entity.Add("bellum gallicum");

        Assert.True(entity.Has<string>("hello world"));
        Assert.True(entity.Has<string>());
        Assert.False(entity.Has<EntityHasTests>());
    }

    [Fact]
    public void Entity_provides_HasRelation()
    {
        using var world = new World();
        var entity = world.Spawn();
        var target = world.Spawn();
        entity.Add<int>(target);

        Assert.True(entity.Has<int>(target));
        Assert.True(entity.Has<int>(Match.Target));
        Assert.True(entity.Has<int>(Match.Any));

        Assert.False(entity.Has<int>(new Entity(world, new(9001))));
        Assert.False(entity.Has<int>(Match.Object));
    }

    [Fact]
    public void Entity_provides_HasRelation_overload_With_Plain_MatchExpression()
    {
        using var world = new World();
        var entity = world.Spawn();
        var target = world.Spawn();
        entity.Add<int>(target);

        Assert.False(entity.Has<int>());
        Assert.True(entity.Has<int>(Match.Entity));
        Assert.False(entity.Has<float>(Match.Entity));
    }

    [Fact]
    public void Implements_IHasComponent()
    {
        using var world = new World();
        var other = world.Spawn();
        
        var entity = world.Spawn().Add(123);
        var interfaceEntity = (IHasTyped) entity;
        Assert.True(entity.Has<int>());
        
        entity.Add("123");
        Assert.True(interfaceEntity.Has<string>());
        
        entity.Add(Link.With("666"));
        Assert.True(interfaceEntity.Has(Link.With("666")));
        
        Assert.False(interfaceEntity.Has<int>(other));
        Assert.False(interfaceEntity.Has<string>(other));

        entity.Add(123, other);
        Assert.True(interfaceEntity.Has<int>(other));
        Assert.False(interfaceEntity.Has<string>(other));
        
        entity.Add("123", other);
        Assert.True(interfaceEntity.Has<int>(other));
        Assert.True(interfaceEntity.Has<string>(other));
    }
}
