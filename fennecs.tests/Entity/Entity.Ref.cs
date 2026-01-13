// SPDX-License-Identifier: MIT

namespace fennecs.tests;

public class EntityRefTests
{
    [Fact]
    public void Can_Get_Component_as_Ref()
    {
        using var world = new World();
        var entity = world.Spawn();
        entity.Add(123);
        ref var component = ref entity.Ref<int>();
        Assert.Equal(123, component);
        component = 456;
        Assert.Equal(456, entity.Ref<int>());
    }

    [Fact]
    public void Cannot_Get_Missing_Component_as_Ref()
    {
        using var world = new World();
        var entity = world.Spawn();
        Assert.Throws<InvalidOperationException>(() => entity.Ref<int>());
    }

    [Fact]
    public void Can_Get_Link_Object_as_Ref()
    {
        using var world = new World();
        var entity = world.Spawn();
        Name helloWorld = new("hello world");
        entity.Add(Link.With(helloWorld));
        ref var component = ref entity.Ref(Link.With(helloWorld));
        Assert.Equal(helloWorld, component);
    }

    [Fact]
    public void Can_Get_Relation_Backing_as_Ref()
    {
        using var world = new World();
        var entity = world.Spawn();
        var target = world.Spawn();
        entity.Add<int>(target);
        ref var component = ref entity.Ref<int>(target);
        Assert.Equal(0, component);
        component = 123;
        Assert.Equal(123, entity.Ref<int>(target));
    }

    // ReSharper disable once NotAccessedPositionalProperty.Local
    private record Name(string _);
}
