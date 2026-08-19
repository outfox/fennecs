// SPDX-License-Identifier: MIT

namespace fennecs.tests;

public class SignalTests
{
    private record struct Position(float X, float Y);
    private record struct Health(int Value);

    private sealed class Sprite;


    [Fact]
    public void Signals_Are_Memoized_Per_Expression()
    {
        using var world = new World();

        Assert.Same(world.On<Position>(), world.On<Position>());
        Assert.Same(world.On<Position>(Match.Any), world.On<Position>(Match.Any));
        Assert.NotSame(world.On<Position>(), world.On<Position>(Match.Any));
    }


    [Fact]
    public void Added_Fires_With_Stored_Value()
    {
        using var world = new World();

        var seen = new List<(Entity entity, Position position)>();
        world.On<Position>().Added += (e, ref p) => seen.Add((e.Entity, p));

        var entity = world.Spawn().Add(new Position(1, 2));

        Assert.Single(seen);
        Assert.Equal(entity, seen[0].entity);
        Assert.Equal(new(1, 2), seen[0].position);
    }


    [Fact]
    public void Added_Handler_Can_Write_Through_The_Reference()
    {
        using var world = new World();

        world.On<Position>().Added += (_, ref p) => p = new(p.X * 10, p.Y * 10);

        var entity = world.Spawn().Add(new Position(1, 2));

        Assert.Equal(new(10, 20), entity.Ref<Position>());
    }


    [Fact]
    public void Removed_Fires_Before_The_Component_Is_Gone()
    {
        using var world = new World();

        var entity = world.Spawn().Add(new Position(3, 4));

        var seen = new List<Position>();
        world.On<Position>().Removed += (e, in p) =>
        {
            seen.Add(p);
            Assert.True(e.Has<Position>());
        };

        entity.Remove<Position>();

        Assert.Single(seen);
        Assert.Equal(new(3, 4), seen[0]);
        Assert.False(entity.Has<Position>());
    }


    [Fact]
    public void Signals_Are_Not_Raised_For_Other_Types()
    {
        using var world = new World();

        var added = 0;
        world.On<Position>().Added += (_, ref _) => added++;

        world.Spawn().Add(new Health(100));

        Assert.Equal(0, added);
    }


    [Fact]
    public void Removed_Is_Not_Raised_For_A_Tolerated_No_Op()
    {
        using var world = new World();

        var removed = 0;
        world.On<Position>().Removed += (_, in _) => removed++;

        world.Spawn().Remove<Position>(default, RemoveConflict.Allow);

        Assert.Equal(0, removed);
    }


    [Fact]
    public void Despawn_Raises_Removed_For_Every_Component()
    {
        using var world = new World();

        var entity = world.Spawn().Add(new Position(1, 2)).Add(new Health(50));

        var positions = 0;
        var healths = 0;
        world.On<Position>().Removed += (_, in _) => positions++;
        world.On<Health>().Removed += (_, in h) =>
        {
            Assert.Equal(50, h.Value);
            healths++;
        };

        entity.Despawn();

        Assert.Equal(1, positions);
        Assert.Equal(1, healths);
    }


    [Fact]
    public void Wildcard_Signal_Covers_Relations_And_Plain()
    {
        using var world = new World();

        var target = world.Spawn();

        var seen = 0;
        world.On<Health>(Match.Any).Added += (EntityRef _, ref Health _) => seen++;

        world.Spawn().Add(new Health(1)).Add(new Health(2), target);

        Assert.Equal(2, seen);
    }


    [Fact]
    public void Plain_Signal_Does_Not_Cover_Relations()
    {
        using var world = new World();

        var target = world.Spawn();

        var added = 0;
        world.On<Health>().Added += (_, ref _) => added++;

        world.Spawn().Add(new Health(2), target);

        Assert.Equal(0, added);
    }


    [Fact]
    public void Wildcard_Removal_Raises_One_Signal_Per_Stored_Expression()
    {
        using var world = new World();

        var alpha = world.Spawn();
        var beta = world.Spawn();
        var entity = world.Spawn().Add(new Health(1), alpha).Add(new Health(2), beta);

        var seen = new List<int>();
        world.On<Health>(Match.Any).Removed += (_, in h) => seen.Add(h.Value);

        entity.Remove<Health>(Match.Any);

        Assert.Equal(2, seen.Count);
        Assert.Contains(1, seen);
        Assert.Contains(2, seen);
    }


    [Fact]
    public void Despawned_Relation_Target_Raises_Removed_On_The_Relating_Entity()
    {
        using var world = new World();

        var target = world.Spawn();
        var entity = world.Spawn().Add(new Health(7), target);

        var seen = new List<(Entity entity, int value)>();
        world.On<Health>(Match.Any).Removed += (e, in h) => seen.Add((e.Entity, h.Value));

        target.Despawn();

        Assert.Single(seen);
        Assert.Equal(entity, seen[0].entity);
        Assert.Equal(7, seen[0].value);
    }


    [Fact]
    public void Object_Links_Raise_Signals()
    {
        using var world = new World();

        var sprite = new Sprite();

        var added = 0;
        var removed = 0;
        var signal = world.On<Sprite>(Match.Any);
        signal.Added += (_, ref _) => added++;
        signal.Removed += (_, in _) => removed++;

        var entity = world.Spawn().Add(Link.With(sprite));
        Assert.Equal(1, added);

        entity.Remove(sprite);
        Assert.Equal(1, removed);
    }


    [Fact]
    public void Deferred_Operations_Signal_Exactly_Once_On_Catch_Up()
    {
        using var world = new World();

        var entity = world.Spawn().Add(new Health(1));

        var added = 0;
        var removed = 0;
        world.On<Position>().Added += (_, ref _) => added++;
        world.On<Health>().Removed += (_, in _) => removed++;

        using (var _ = world.Lock())
        {
            entity.Add(new Position(1, 2));
            entity.Remove<Health>();

            Assert.Equal(0, added);
            Assert.Equal(0, removed);
        }

        Assert.Equal(1, added);
        Assert.Equal(1, removed);
    }


    [Fact]
    public void Handlers_May_Make_Structural_Changes()
    {
        using var world = new World();

        // Every Entity that gains a Position also gains a Health, and loses it with the Position.
        world.On<Position>().Added += (e, ref _) => e.Add(new Health(100));
        world.On<Position>().Removed += (e, in _) => e.Remove<Health>();

        var entity = world.Spawn().Add(new Position(1, 2));
        Assert.True(entity.Has<Health>());

        entity.Remove<Position>();
        Assert.False(entity.Has<Health>());
        Assert.False(entity.Has<Position>());

        world.On<Health>().Added += (EntityRef e, ref Health _) => e.Remove<Health>();
        entity.Add<Health>();

        Assert.False(entity.Has<Health>());
    }


    [Fact]
    public void Template_Spawn_Raises_Added()
    {
        using var world = new World();

        var seen = new List<Position>();
        world.On<Position>().Added += (_, ref p) => seen.Add(p);

        world.Template()
            .Add(new Position(1, 2))
            .Add(new Health(3))
            .Spawn(10)
            .Dispose();

        Assert.Equal(10, seen.Count);
        Assert.All(seen, position => Assert.Equal(new(1, 2), position));
    }


    [Fact]
    public void Template_Spawn_Raises_Added_Across_Aspects()
    {
        using var world = new World();
        world.AddAspect("visuals").Owns<Position>();

        var positions = 0;
        var healths = 0;
        world.On<Position>().Added += (_, ref _) => positions++;
        world.On<Health>().Added += (_, ref _) => healths++;

        world.Template()
            .Add(new Position(1, 2))
            .Add(new Health(3))
            .Spawn(5)
            .Dispose();

        Assert.Equal(5, positions);
        Assert.Equal(5, healths);
    }


    [Fact]
    public void Batch_Raises_Added_And_Removed()
    {
        using var world = new World();

        for (var i = 0; i < 10; i++) world.Spawn().Add(new Health(i));

        var added = new List<Position>();
        var removed = new List<int>();
        world.On<Position>().Added += (_, ref p) => added.Add(p);
        world.On<Health>().Removed += (_, in h) => removed.Add(h.Value);

        world.Query<Health>().Not<Position>().Compile()
            .Batch()
            .Add(new Position(9, 9))
            .Remove<Health>()
            .Submit();

        Assert.Equal(10, added.Count);
        Assert.All(added, position => Assert.Equal(new(9, 9), position));
        Assert.Equal(10, removed.Count);
        Assert.Equal(Enumerable.Range(0, 10), removed.Order());
    }


    [Fact]
    public void Batch_Does_Not_Signal_Components_That_Do_Not_Change()
    {
        using var world = new World();

        for (var i = 0; i < 4; i++) world.Spawn().Add(new Health(i)).Add(new Position(0, 0));

        var added = 0;
        world.On<Position>().Added += (_, ref _) => added++;

        // Replacing a value on Entities that already have the Component is not an addition.
        world.Query<Health>().Compile()
            .Batch(AddConflict.Replace)
            .Add(new Position(1, 1))
            .Submit();

        Assert.Equal(0, added);
    }


    [Fact]
    public void Truncate_Raises_Removed()
    {
        using var world = new World();

        for (var i = 0; i < 10; i++) world.Spawn().Add(new Health(i));

        var removed = 0;
        world.On<Health>().Removed += (_, in _) => removed++;

        world.Query<Health>().Compile().Truncate(4);

        Assert.Equal(6, removed);
        Assert.Equal(4, world.Count);
    }


    [Fact]
    public void Unsubscribing_Stops_The_Signals()
    {
        using var world = new World();

        var added = 0;
        void Handler(EntityRef entity, ref Position position) => added++;

        world.On<Position>().Added += Handler;
        world.Spawn().Add(new Position(1, 2));
        Assert.Equal(1, added);

        world.On<Position>().Added -= Handler;
        world.Spawn().Add(new Position(3, 4));
        Assert.Equal(1, added);
    }

    [Fact]
    public void Unsubscribe_Durring_Handling()
    {
        using var world = new World();

        var added = 0;
        void Handler(EntityRef entity, ref Position position)
        {
            added++;
            world.On<Position>().Added -= Handler;
        }

        world.On<Position>().Added += Handler;
        world.Spawn().Add(new Position(1, 2));
        Assert.Equal(1, added);

        world.Spawn().Add(new Position(1, 2));
        Assert.Equal(1, added);
    }

    [Fact]
    public void Catchup_Fully_Follow_History()
    {
        using var world = new World();

        var added = 0;
        world.On<Position>().Added += (_, ref _) => added++;

        using (world.Lock())
        {
            var entity = world.Spawn().Add(new Position(1, 2));
            entity.Remove<Position>();
        }

        Assert.Equal(1, added);
        added = 0;

        using (world.Lock())
        {
            var entity = world.Spawn().Add(new Position(1, 2));
            entity.Remove<Position>();
            entity.Add(new Position(1, 2));
        }

        Assert.Equal(2, added);
    }

    [Fact]
    public void A_Bulk_Deferred_Despawn_Does_Not_Recurse_Per_Entity()
    {
        using var world = new World();

        // Signal dispatch takes a World Lock of its own; releasing it must not start a nested
        // drain of the deferred queue, or a bulk despawn recurses one stack frame per Entity.
        var removed = 0;
        world.On<Health>().Removed += (_, in _) => removed++;

        for (var i = 0; i < 10_000; i++) world.Spawn().Add(new Health(i));

        world.DespawnAllWith<Health>();

        Assert.Equal(10_000, removed);
        Assert.Equal(0, world.Count);
    }
}
