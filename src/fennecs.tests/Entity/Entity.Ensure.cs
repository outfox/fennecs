// SPDX-License-Identifier: MIT

namespace fennecs.tests;

public class EntityEnsureTests
{
    [Fact]
    public void Ensure_Adds_Component_When_Missing()
    {
        using var world = new World();
        var entity = world.Spawn();

        Assert.False(entity.Has<int>());
        
        ref var component = ref entity.Ensure<int>();
        
        Assert.True(entity.Has<int>());
        Assert.Equal(0, component);
    }

    [Fact]
    public void Ensure_Returns_Existing_Component()
    {
        using var world = new World();
        var entity = world.Spawn();
        entity.Add(42);

        ref var component = ref entity.Ensure<int>();

        Assert.Equal(42, component);
    }

    [Fact]
    public void Ensure_Does_Not_Overwrite_Existing_Component()
    {
        using var world = new World();
        var entity = world.Spawn();
        entity.Add(42);

        _ = entity.Ensure(999);

        Assert.Equal(42, entity.Ref<int>());
    }

    [Fact]
    public void Ensure_Ref_Can_Modify_Component()
    {
        using var world = new World();
        var entity = world.Spawn();

        ref var component = ref entity.Ensure<int>();
        component = 123;

        Assert.Equal(123, entity.Ref<int>());
    }

    [Fact]
    public void Ensure_With_Custom_Default_Value()
    {
        using var world = new World();
        var entity = world.Spawn();

        ref var component = ref entity.Ensure(999);

        Assert.Equal(999, component);
        Assert.Equal(999, entity.Ref<int>());
    }

    [Fact]
    public void Ensure_With_Default_Struct_Value()
    {
        using var world = new World();
        var entity = world.Spawn();

        ref var component = ref entity.Ensure<TestStruct>();

        Assert.Equal(0, component.Value);
        Assert.Null(component.Name);
    }

    [Fact]
    public void Ensure_With_Custom_Struct_Value()
    {
        using var world = new World();
        var entity = world.Spawn();

        var customValue = new TestStruct { Value = 42, Name = "test" };
        ref var component = ref entity.Ensure(customValue);

        Assert.Equal(42, component.Value);
        Assert.Equal("test", component.Name);
    }

    [Fact]
    public void Ensure_Multiple_Calls_Are_Idempotent()
    {
        using var world = new World();
        var entity = world.Spawn();

        ref var first = ref entity.Ensure(100);
        first = 200;
        
        ref var second = ref entity.Ensure(999);
        
        Assert.Equal(200, second);
    }

    [Fact]
    public void Ensure_With_Relation_Match_Adds_When_Missing()
    {
        using var world = new World();
        var entity = world.Spawn();
        var target = world.Spawn();

        Assert.False(entity.Has<int>(target));

        ref var component = ref entity.Ensure(42, target);

        Assert.True(entity.Has<int>(target));
        Assert.Equal(42, component);
    }

    [Fact]
    public void Ensure_With_Relation_Match_Returns_Existing()
    {
        using var world = new World();
        var entity = world.Spawn();
        var target = world.Spawn();
        entity.Add(123, target);

        ref var component = ref entity.Ensure(999, target);

        Assert.Equal(123, component);
    }

    [Fact]
    public void Ensure_Plain_And_Relation_Are_Independent()
    {
        using var world = new World();
        var entity = world.Spawn();
        var target = world.Spawn();

        // ReSharper disable once UnusedVariable
        ref var plain = ref entity.Ensure(100);
        // ReSharper disable once UnusedVariable
        ref var relation = ref entity.Ensure(200, target);

        Assert.Equal(100, entity.Ref<int>());
        Assert.Equal(200, entity.Ref<int>(target));
        Assert.NotEqual(entity.Ref<int>(), entity.Ref<int>(target));
    }

    [Fact]
    public void Ensure_Multiple_Relations_Are_Independent()
    {
        using var world = new World();
        var entity = world.Spawn();
        var target1 = world.Spawn();
        var target2 = world.Spawn();

        // ReSharper disable once UnusedVariable
        ref var rel1 = ref entity.Ensure(111, target1);
        // ReSharper disable once UnusedVariable
        ref var rel2 = ref entity.Ensure(222, target2);

        Assert.Equal(111, entity.Ref<int>(target1));
        Assert.Equal(222, entity.Ref<int>(target2));
    }

    [Fact]
    public void Ensure_Different_Types_Are_Independent()
    {
        using var world = new World();
        var entity = world.Spawn();

        // Note: Don't hold refs across structural changes (Ensure calls that add components)
        // Each Ensure may move the entity to a new archetype, invalidating previous refs
        entity.Ensure(42);
        entity.Ensure(3.14f);
        entity.Ensure(2.718);

        Assert.Equal(42, entity.Ref<int>());
        Assert.Equal(3.14f, entity.Ref<float>());
        Assert.Equal(2.718, entity.Ref<double>());
    }

    [Fact]
    public void Ensure_Returns_Modifiable_Ref_For_Existing()
    {
        using var world = new World();
        var entity = world.Spawn();
        entity.Add(10);

        ref var component = ref entity.Ensure<int>();
        component *= 5;

        Assert.Equal(50, entity.Ref<int>());
    }

    [Fact]
    public void Ensure_With_Match_Plain()
    {
        using var world = new World();
        var entity = world.Spawn();

        ref var component = ref entity.Ensure(42, Match.Plain);

        Assert.True(entity.Has<int>(Match.Plain));
        Assert.Equal(42, component);
    }

    [Fact]
    public void Ensure_Increment_Pattern()
    {
        using var world = new World();
        var entity = world.Spawn();

        // Common pattern: ensure and increment
        entity.Ensure<int>()++;
        entity.Ensure<int>()++;
        entity.Ensure<int>()++;

        Assert.Equal(3, entity.Ref<int>());
    }

    [Fact]
    public void Ensure_Default_Match_Is_Plain()
    {
        using var world = new World();
        var entity = world.Spawn();
        var target = world.Spawn();

        // Add a relation component first
        entity.Add(999, target);
        
        // Ensure with default match should create a plain component
        ref var plain = ref entity.Ensure(42);

        // Both should exist independently
        Assert.True(entity.Has<int>(Match.Plain));
        Assert.True(entity.Has<int>(target));
        Assert.Equal(42, plain);
        Assert.Equal(999, entity.Ref<int>(target));
    }

    private struct TestStruct
    {
        public int Value;
        public string Name;
    }
}
