// SPDX-License-Identifier: MIT

namespace fennecs.tests;

/// <summary>
/// Tests for the <see cref="EntityRef"/> ref struct handed out by Stream runners.
/// </summary>
public class LiveEntityRefTests
{
    [Fact]
    public void Exposes_World_Entity_and_Alive()
    {
        using var world = new World();
        var entity = world.Spawn().Add(123);

        var stream = world.Query<int>().Stream();
        var visited = 0;
        stream.For((in EntityRef e, ref int _) =>
        {
            Assert.Equal(world, e.World);
            Assert.Equal(entity, e.Entity);
            Assert.True(e.Alive);

            Entity converted = e;
            Assert.Equal(entity, converted);

            Assert.Equal(entity.ToString(), e.ToString());
            visited++;
        });
        Assert.Equal(1, visited);
    }


    [Fact]
    public void Ref_Reads_Archetype_Storage_Directly()
    {
        using var world = new World();
        var entity = world.Spawn().Add(123);

        var stream = world.Query<int>().Stream();
        stream.For((in EntityRef e, ref int c) =>
        {
            Assert.Equal(c, e.Ref<int>());
            e.Ref<int>() = 456;
            Assert.Equal(456, c);
        });
        Assert.Equal(456, entity.Ref<int>());
    }


    [Fact]
    public void Ref_Falls_Back_to_World_for_Components_in_Other_Aspects()
    {
        using var world = new World();
        world.AddAspect("strings").Owns<string>();

        var entity = world.Spawn().Add(123).Add("hello");

        var stream = world.Query<int>().Stream();
        stream.For((in EntityRef e, ref int _) =>
        {
            Assert.Equal("hello", e.Ref<string>());
            e.Ref<string>() = "goodbye";
        });
        Assert.Equal("goodbye", entity.Ref<string>());
    }


    [Fact]
    public void Ref_Throws_for_Missing_Component()
    {
        using var world = new World();
        world.Spawn().Add(123);

        var stream = world.Query<int>().Stream();
        Assert.Throws<InvalidOperationException>(() =>
            stream.For((in EntityRef e, ref int _) => e.Ref<float>()));
    }


    [Fact]
    public void Has_Checks_Archetype_and_Other_Aspects()
    {
        using var world = new World();
        world.AddAspect("strings").Owns<string>();

        world.Spawn().Add(123).Add("hello");

        var stream = world.Query<int>().Stream();
        var visited = 0;
        stream.For((in EntityRef e, ref int _) =>
        {
            Assert.True(e.Has<int>());
            Assert.True(e.Has<string>()); // lives in the "strings" Aspect
            Assert.False(e.Has<float>());
            visited++;
        });
        Assert.Equal(1, visited);
    }


    [Fact]
    public void Has_Checks_Relations_and_Links()
    {
        using var world = new World();
        var target = world.Spawn();
        var other = world.Spawn();
        world.Spawn().Add(123).Add(7.5f, target).Add(Link.With("linked"));

        var stream = world.Query<int>().Stream();
        var visited = 0;
        stream.For((in EntityRef e, ref int _) =>
        {
            Assert.True(e.Has<float>(target));
            Assert.False(e.Has<float>(other));

            Assert.True(e.Has(Link.With("linked")));
            Assert.False(e.Has(Link.With("unrelated")));
            visited++;
        });
        Assert.Equal(1, visited);
    }


    [Fact]
    public void Add_is_Deferred_and_Chainable()
    {
        using var world = new World();
        var target = world.Spawn();
        var entity = world.Spawn().Add(123);

        var stream = world.Query<int>().Stream();
        stream.For((in EntityRef e, ref int _) =>
        {
            e.Add(2.5f)             // plain with data
                .Add<Tag>()         // newable plain
                .Add(9.0, target)   // relation
                .Add(Link.With("linked")); // object link

            // Structural changes are deferred inside the runner.
            Assert.False(e.Has<Tag>());
        });

        Assert.Equal(2.5f, entity.Ref<float>());
        Assert.True(entity.Has<Tag>());
        Assert.True(entity.Has<double>(target));
        Assert.True(entity.Has(Link.With("linked")));
    }


    [Fact]
    public void Remove_is_Deferred_and_Chainable()
    {
        using var world = new World();
        var target = world.Spawn();
        var entity = world.Spawn().Add(123)
            .Add(2.5f)
            .Add(9.0, target)
            .Add(Link.With("linked"));

        var stream = world.Query<int>().Stream();
        stream.For((in EntityRef e, ref int _) =>
        {
            e.Remove<float>()
                .Remove<double>(target)
                .Remove(Link.With("linked"));

            // Structural changes are deferred inside the runner.
            Assert.True(e.Has<float>());
        });

        Assert.False(entity.Has<float>());
        Assert.False(entity.Has<double>(target));
        Assert.False(entity.Has(Link.With("linked")));
        Assert.True(entity.Alive);
    }


    [Fact]
    public void Despawn_is_Deferred()
    {
        using var world = new World();
        var entity = world.Spawn().Add(123);

        var stream = world.Query<int>().Stream();
        stream.For((in EntityRef e, ref int _) =>
        {
            e.Despawn();
            Assert.True(e.Alive); // deferred: still alive inside the runner
        });

        Assert.False(entity.Alive);
    }


    private struct Tag;
}
