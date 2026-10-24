// SPDX-License-Identifier: MIT

using System.Collections.Concurrent;

namespace fennecs.tests;

public class SignalTests
{
    private record struct Position(float X, float Y);
    private record struct Health(int Value);

    private record struct Index(int Value);

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
        world.On<Position>().AddedEach((e, p) => seen.Add((e, p)));

        var entity = world.Spawn().Add(new Position(1, 2));

        Assert.Single(seen);
        Assert.Equal(entity, seen[0].entity);
        Assert.Equal(new(1, 2), seen[0].position);
    }


    [Fact]
    public void Added_Handler_Can_Write_Through_The_Reference()
    {
        using var world = new World();

        world.On<Position>().AfterAdded += (_, positions) =>
        {
            foreach (ref var p in positions) p = new(p.X * 10, p.Y * 10);
        };

        var entity = world.Spawn().Add(new Position(1, 2));

        Assert.Equal(new(10, 20), entity.Ref<Position>());
    }


    [Fact]
    public void Removed_Fires_Before_The_Component_Is_Gone()
    {
        using var world = new World();

        var entity = world.Spawn().Add(new Position(3, 4));

        var seen = new List<Position>();
        world.On<Position>().RemovedEach((e, p, _) => {
            seen.Add(p);
            Assert.True(e.Has<Position>());
        });

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
        world.On<Position>().AddedEach((_, _) => added++);

        world.Spawn().Add(new Health(100));

        Assert.Equal(0, added);
    }


    [Fact]
    public void Removed_Is_Not_Raised_For_A_Tolerated_No_Op()
    {
        using var world = new World();

        var removed = 0;
        world.On<Position>().RemovedEach((_, _, _) => removed++);

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
        world.On<Position>().RemovedEach((_, _, _) => positions++);
        world.On<Health>().RemovedEach((_, h, _) => {
            Assert.Equal(50, h.Value);
            healths++;
        });

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
        world.On<Health>(Match.Any).AddedEach((_, _) => seen++);

        world.Spawn().Add(new Health(1)).Add(new Health(2), target);

        Assert.Equal(2, seen);
    }


    [Fact]
    public void Plain_Signal_Does_Not_Cover_Relations()
    {
        using var world = new World();

        var target = world.Spawn();

        var added = 0;
        world.On<Health>().AddedEach((_, _) => added++);

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
        world.On<Health>(Match.Any).RemovedEach((_, h, _) => seen.Add(h.Value));

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
        world.On<Health>(Match.Any).RemovedEach((e, h, _) => seen.Add((e, h.Value)));

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
        signal.AddedEach((_, _) => added++);
        signal.RemovedEach((_, _, _) => removed++);

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
        world.On<Position>().AddedEach((_, _) => added++);
        world.On<Health>().RemovedEach((_, _, _) => removed++);

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
        world.On<Position>().AddedEach((e, _) => e.Add(new Health(100)));
        world.On<Position>().RemovedEach((e, _, _) => e.Remove<Health>());

        var entity = world.Spawn().Add(new Position(1, 2));
        Assert.True(entity.Has<Health>());

        entity.Remove<Position>();
        Assert.False(entity.Has<Health>());
        Assert.False(entity.Has<Position>());

        world.On<Health>().AddedEach((e, _) => e.Remove<Health>());
        entity.Add<Health>();

        Assert.False(entity.Has<Health>());
    }


    [Fact]
    public void A_Template_Spawn_In_A_Handler_Does_Not_Move_The_Span_Under_It()
    {
        using var world = new World();

        var target = world.Spawn();
        var spawned = new List<Entity>();

        world.On<Position>().AfterAdded += (entities, positions) =>
        {
            if (entities[0] != target) return;

            // Enough to grow the Archetype's Storages many times over, were it immediate.
            using var template = world.Template().Add(new Position(7, 7));
            for (var i = 0; i < 1000; i++) spawned.Add(template.Spawn());

            // Written after the spawn: must land in the World, not in a discarded array.
            positions[0] = new(42, 42);
        };

        target.Add(new Position(1, 1));

        Assert.Equal(new(42, 42), target.Ref<Position>());
        Assert.Equal(1000, spawned.Count);
        Assert.All(spawned, e => Assert.Equal(new(7, 7), e.Ref<Position>()));
    }


    [Fact]
    public void A_Template_Spawn_In_A_Runner_Is_Deferred()
    {
        using var world = new World();
        world.Spawn().Add(new Health(1));

        using var template = world.Template().Add(new Health(2));
        var spawned = default(Entity);

        world.Query<Health>().Stream().For((ref Health health) =>
        {
            spawned = template.Spawn();

            // The handle is usable straight away; its Components arrive when the runner is done.
            Assert.True(spawned.Alive);
            Assert.False(spawned.Has<Health>());
            health = new(10);
        });

        Assert.Equal(new(2), spawned.Ref<Health>());
        Assert.Equal(2, world.Query<Health>().Compile().Count);
    }


    [Fact]
    public void Template_Spawn_Raises_Added()
    {
        using var world = new World();

        var seen = new List<Position>();
        world.On<Position>().AddedEach((_, p) => seen.Add(p));

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
        world.On<Position>().AddedEach((_, _) => positions++);
        world.On<Health>().AddedEach((_, _) => healths++);

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
        world.On<Position>().AddedEach((_, p) => added.Add(p));
        world.On<Health>().RemovedEach((_, h, _) => removed.Add(h.Value));

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
        world.On<Position>().AddedEach((_, _) => added++);

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
        world.On<Health>().RemovedEach((_, _, _) => removed++);

        world.Query<Health>().Compile().Truncate(4);

        Assert.Equal(6, removed);
        Assert.Equal(4, world.Count);
    }


    [Fact]
    public void Unsubscribing_Stops_The_Signals()
    {
        using var world = new World();

        var added = 0;
        void Handler(ReadOnlySpan<Entity> entities, Span<Position> positions) => added += entities.Length;

        world.On<Position>().AfterAdded += Handler;
        world.Spawn().Add(new Position(1, 2));
        Assert.Equal(1, added);

        world.On<Position>().AfterAdded -= Handler;
        world.Spawn().Add(new Position(3, 4));
        Assert.Equal(1, added);
    }

    [Fact]
    public void Unsubscribe_Durring_Handling()
    {
        using var world = new World();

        var added = 0;
        void Handler(ReadOnlySpan<Entity> entities, Span<Position> positions)
        {
            added++;
            world.On<Position>().AfterAdded -= Handler;
        }

        world.On<Position>().AfterAdded += Handler;
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
        world.On<Position>().AddedEach((_, _) => added++);

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
    public void Removed_Reports_A_Plain_Removal_As_Removed()
    {
        using var world = new World();

        var causes = new List<RemoveCause>();
        world.On<Health>().RemovedEach((_, _, cause) => causes.Add(cause));

        // per-Entity CRUD
        world.Spawn().Add(new Health(1)).Remove<Health>();

        // ...and the Batch path
        world.Spawn().Add(new Health(2));
        world.Query<Health>().Compile().Batch().Remove<Health>().Submit();

        Assert.Equal([RemoveCause.Removed, RemoveCause.Removed], causes);
    }


    [Fact]
    public void Removed_Reports_A_Despawn_As_Despawned()
    {
        using var world = new World();

        var causes = new List<RemoveCause>();
        world.On<Health>().RemovedEach((_, _, cause) => causes.Add(cause));

        world.Spawn().Add(new Health(1)).Despawn();

        // Truncate evicts Entities wholesale - also a despawn
        world.Spawn().Add(new Health(2));
        world.Query<Health>().Compile().Truncate(0);

        Assert.Equal([RemoveCause.Despawned, RemoveCause.Despawned], causes);
    }


    [Fact]
    public void Removed_Reports_A_Dead_Relation_Target_As_TargetDespawned()
    {
        using var world = new World();

        var target = world.Spawn();
        var entity = world.Spawn().Add(new Health(7), target);

        var causes = new List<RemoveCause>();
        world.On<Health>(Match.Any).RemovedEach((_, _, cause) => causes.Add(cause));

        target.Despawn();

        // the relation is gone, but the Entity holding it lives on
        Assert.Equal([RemoveCause.TargetDespawned], causes);
        Assert.True(entity.Alive);
        Assert.False(entity.Has<Health>(Match.Any));
    }


    [Fact]
    public void Despawn_Reports_Despawned_For_Both_Components_And_Relations()
    {
        using var world = new World();

        var target = world.Spawn();
        var entity = world.Spawn().Add(new Health(1)).Add(new Health(2), target);

        var causes = new List<RemoveCause>();
        world.On<Health>(Match.Any).RemovedEach((_, _, cause) => causes.Add(cause));

        entity.Despawn();

        // the Entity took both with it - neither is a TargetDespawned
        Assert.Equal([RemoveCause.Despawned, RemoveCause.Despawned], causes);
    }

    [Fact]
    public void A_Bulk_Deferred_Despawn_Does_Not_Recurse_Per_Entity()
    {
        using var world = new World();

        // Signal dispatch takes a World Lock of its own; releasing it must not start a nested
        // drain of the deferred queue, or a bulk despawn recurses one stack frame per Entity.
        var removed = 0;
        world.On<Health>().RemovedEach((_, _, _) => removed++);

        for (var i = 0; i < 10_000; i++) world.Spawn().Add(new Health(i));

        world.DespawnAllWith<Health>();

        Assert.Equal(10_000, removed);
        Assert.Equal(0, world.Count);
    }

    [Fact]
    public void A_Throwing_Handler_Is_Wrapped_In_A_SignalException()
    {
        using var world = new World();

        var boom = new InvalidOperationException("boom");
        var signal = world.On<Health>();
        signal.AddedEach((_, _) => throw boom);

        var entity = world.Spawn();
        var thrown = Assert.Throws<SignalException>(() => entity.Add(new Health(1)));

        Assert.Same(boom, thrown.InnerException);
        Assert.Same(signal, thrown.Signal);
        Assert.Single(thrown.Entities);
        Assert.Equal(entity, thrown.Entities[0]);
        Assert.False(thrown.Deferred);
    }


    [Fact]
    public void A_Throwing_Removed_Handler_Is_Wrapped_Too()
    {
        using var world = new World();

        var signal = world.On<Health>();
        signal.RemovedEach((_, _, _) => throw new InvalidOperationException("boom"));

        var entity = world.Spawn().Add(new Health(1));
        var thrown = Assert.Throws<SignalException>(() => entity.Despawn());

        Assert.Same(signal, thrown.Signal);
        Assert.False(thrown.Deferred);
    }


    [Fact]
    public void A_Throwing_Removed_Handler_Leaves_The_World_Consistent()
    {
        using var world = new World();

        var entity = world.Spawn().Add(new Health(1));
        world.On<Health>().RemovedEach((_, _, _) => throw new InvalidOperationException("boom"));

        Assert.Throws<SignalException>(() => entity.Remove<Health>());

        // The removal never happened: the handler runs before the structural change.
        Assert.True(entity.Has<Health>());
        Assert.Equal(1, world.Count);
        Assert.Single(world.All);
    }


    [Fact]
    public void A_Throwing_Despawn_Handler_In_A_Secondary_Aspect_Leaves_The_World_Consistent()
    {
        using var world = new World();
        world.AddAspect("visuals").Owns<Position>();

        var entity = world.Spawn().Add(new Health(1)).Add(new Position(1, 2));
        world.On<Position>().RemovedEach((_, _, _) => throw new InvalidOperationException("boom"));

        Assert.Throws<SignalException>(() => entity.Despawn());

        // The Despawn fails as a whole: the Entity is still alive, in every Aspect, with everything it had.
        Assert.True(entity.Alive);
        Assert.Equal(1, world.Count);
        Assert.Single(world.All);
        Assert.True(entity.Has<Health>());
        Assert.True(entity.Has<Position>());

        // ... and the World is still usable afterwards.
        world.Spawn().Add(new Health(2));
        Assert.Equal(2, world.Count);
        Assert.Equal(2, world.All.Count);
    }


    [Fact]
    public void A_Throwing_Despawn_Handler_Of_The_Main_Aspect_Leaves_The_Secondary_Aspect_Intact()
    {
        using var world = new World();
        world.AddAspect("visuals").Owns<Position>();

        var entity = world.Spawn().Add(new Health(1)).Add(new Position(1, 2));
        world.On<Health>().RemovedEach((_, _, _) => throw new InvalidOperationException("boom"));

        Assert.Throws<SignalException>(() => entity.Despawn());

        Assert.True(entity.Alive);
        Assert.Equal(1, world.Count);
        Assert.Single(world.All);
        Assert.True(entity.Has<Health>());
        Assert.True(entity.Has<Position>());
    }


    [Fact]
    public void A_Throwing_Truncate_Handler_In_A_Secondary_Aspect_Leaves_The_World_Consistent()
    {
        using var world = new World();
        world.AddAspect("visuals").Owns<Position>();

        for (var i = 0; i < 10; i++) world.Spawn().Add(new Health(i)).Add(new Position(i, i));
        world.On<Position>().RemovedEach((_, _, _) => throw new InvalidOperationException("boom"));

        var query = world.Query<Health>().Compile();
        Assert.Throws<SignalException>(() => query.Truncate(4));

        Assert.Equal(10, world.Count);
        Assert.Equal(10, world.All.Count);
        Assert.Equal(10, query.Count);
        Assert.Equal(10, world.Query<Position>().Compile().Count);
    }


    [Fact]
    public void A_Throwing_Batch_Removed_Handler_Leaves_The_World_Consistent()
    {
        using var world = new World();

        for (var i = 0; i < 10; i++) world.Spawn().Add(new Health(i));
        world.On<Health>().RemovedEach((_, _, _) => throw new InvalidOperationException("boom"));

        var query = world.Query<Health>().Compile();
        Assert.Throws<SignalException>(() => query.Batch().Remove<Health>().Submit());

        Assert.Equal(10, world.Count);
        Assert.Equal(10, world.All.Count);
        Assert.Equal(10, query.Count);
    }


    [Fact]
    public void A_Handlers_Failed_Deferred_Change_Is_Wrapped_As_Deferred()
    {
        using var world = new World();

        // Removing the Component that is already on its way out: legal to ask for, impossible to apply.
        world.On<Health>().RemovedEach((e, _, _) => e.Remove<Health>());

        var entity = world.Spawn().Add(new Health(1));
        var thrown = Assert.Throws<SignalException>(() => entity.Remove<Health>());

        Assert.True(thrown.Deferred);
        Assert.Null(thrown.Signal);
        Assert.Single(thrown.Entities);
        Assert.Equal(entity, thrown.Entities[0]);
        Assert.IsType<InvalidOperationException>(thrown.InnerException);
    }


    [Fact]
    public void The_Callers_Own_Mistakes_Are_Not_Wrapped()
    {
        using var world = new World();

        world.On<Health>().AddedEach((_, _) => { });
        world.On<Health>().RemovedEach((_, _, _) => { });

        var entity = world.Spawn().Add(new Health(1));

        // Signals are live, but these are the caller's errors - they must surface unwrapped.
        Assert.Throws<InvalidOperationException>(() => entity.Add(new Health(2)));
        Assert.Throws<InvalidOperationException>(() => entity.Remove<Position>());
    }


    [Fact]
    public void A_Nested_SignalException_Is_Not_Wrapped_Twice()
    {
        using var world = new World();

        world.On<Position>().AddedEach((e, _) => e.Add(new Health(1)));
        world.On<Health>().AddedEach((_, _) => throw new InvalidOperationException("boom"));

        var entity = world.Spawn();
        var thrown = Assert.Throws<SignalException>(() => entity.Add(new Position(1, 2)));

        Assert.IsType<InvalidOperationException>(thrown.InnerException);
    }

    [Fact]
    public void Signals_Of_A_Parallel_Job_Dispatch_Once_On_A_Single_Thread()
    {
        using var world = new World();

        const int count = 10_000;

        var entities = new Entity[count];
        for (var i = 0; i < count; i++) entities[i] = world.Spawn().Add(new Index(i));

        // Materialize Position before the Job: its first use registers the type with an Aspect,
        // and that is not what this test is about.
        world.Spawn().Add(new Position(0, 0)).Despawn();

        var dispatchThreads = new ConcurrentDictionary<int, byte>();

        // Deliberately a plain increment, not Interlocked: if dispatch were concurrent, this
        // would lose counts and the test would fail - which is exactly the guarantee under test.
        var added = 0;
        world.On<Position>().AddedEach((_, _) => {
            dispatchThreads.TryAdd(Environment.CurrentManagedThreadId, 0);
            added++;
        });

        // Worker threads request structural changes concurrently; the World is locked for the
        // duration of the runner, so they queue up and are applied at catch-up.
        world.Query<Index>().Stream()
            .Job(entities, static (Entity[] all, ref Index index) => all[index.Value].Add(new Position(index.Value, 0)));

        Assert.Equal(count, added);
        Assert.Single(dispatchThreads);
        Assert.All(entities, entity => Assert.True(entity.Has<Position>()));
    }


    [Fact]
    public void A_Parallel_Job_Does_Not_Signal_While_Its_Workers_Run()
    {
        using var world = new World();

        var work = new Workers { All = new Entity[1_000] };
        for (var i = 0; i < work.All.Length; i++) work.All[i] = world.Spawn().Add(new Index(i));

        world.Spawn().Add(new Position(0, 0)).Despawn();

        var whileWorking = 0;
        world.On<Position>().AddedEach((_, _) => {
            if (Volatile.Read(ref work.Active) > 0) whileWorking++;
        });

        world.Query<Index>().Stream()
            .Job(work, static (Workers workers, ref Index index) =>
            {
                Interlocked.Increment(ref workers.Active);
                workers.All[index.Value].Add(new Position(index.Value, 0));
                Interlocked.Decrement(ref workers.Active);
            });

        // The runner releases its Lock only once every worker is done, so dispatch can never
        // interleave with the iteration - which is what makes the EntityRef handlers get safe.
        Assert.Equal(0, whileWorking);
    }


    private sealed class Workers
    {
        public Entity[] All = [];
        public int Active;
    }

    [Fact]
    public void A_Dead_Relation_Target_Defers_What_Its_Handlers_Do()
    {
        using var world = new World();

        var target = world.Spawn();
        var relating = world.Spawn().Add(new Health(7), target);

        // Nothing watches what the *target* holds — only the relation on the other Entity. The
        // Despawn must still take the Lock, or this handler would mutate the relating Entity in
        // the middle of the dependency migration.
        world.On<Health>(Match.Any).RemovedEach((e, _, cause) => {
            Assert.Equal(RemoveCause.TargetDespawned, cause);
            e.Add(new Position(1, 2));
        });

        target.Despawn();

        Assert.True(relating.Alive);
        Assert.True(relating.Has<Position>());
        Assert.False(relating.Has<Health>(Match.Any));
    }


    [Fact]
    public void A_Handler_May_Register_Another_Signal_Of_The_Same_Type()
    {
        using var world = new World();

        // Subscribing from inside a handler appends to the very list the dispatch is walking.
        world.On<Health>().AddedEach((_, _) => world.On<Health>(Match.Any));

        var entity = world.Spawn();
        entity.Add(new Health(1));   // must not throw a bare InvalidOperationException

        Assert.True(entity.Has<Health>());
    }


    [Fact]
    public void A_Wildcard_Batch_Removal_Reaches_A_Plain_Signal()
    {
        using var world = new World();

        for (var i = 0; i < 4; i++) world.Spawn().Add(new Index(i)).Add(new Health(i));

        var removed = 0;
        world.On<Health>().RemovedEach((_, _, _) => removed++);

        // The Batch pattern is a Wildcard; the plain Signal still covers what it actually strips.
        world.Query<Index>().Has<Health>(Match.Any).Compile()
            .Batch()
            .Remove<Health>(Match.Any)
            .Submit();

        Assert.Equal(4, removed);
    }


    [Fact]
    public void GC_Drops_Idle_Signals_From_The_Registry_But_A_Kept_One_Is_Revived()
    {
        using var world = new World();

        void Handler(ReadOnlySpan<Entity> entities, Span<Health> healths) { }

        var signal = world.On<Health>();
        signal.AfterAdded += Handler;
        signal.AfterAdded -= Handler;

        world.GC();

        // Still held by us: the World hands the very same instance back.
        Assert.Same(signal, world.On<Health>());
    }


    [Fact]
    public void A_Stored_Signal_Is_Not_An_Orphan_After_GC()
    {
        using var world = new World();
        var seen = 0;

        var stored = world.On<Health>();
        world.GC();   // never subscribed to: dropped from the registry

        stored.AddedEach((_, _) => seen++);
        world.Spawn().Add(new Health(1));

        Assert.Equal(1, seen);
        Assert.Same(stored, world.On<Health>());
    }


    [Fact]
    public void A_Signal_Nobody_Holds_Is_Collected()
    {
        using var world = new World();

        static WeakReference Touch(World world)
        {
            var signal = world.On<Health>();
            return new WeakReference(signal);
        }

        var weak = Touch(world);
        world.GC();
        System.GC.Collect();
        System.GC.WaitForPendingFinalizers();

        Assert.False(weak.IsAlive);

        // and a fresh one is minted on demand, wired up normally
        var seen = 0;
        world.On<Health>().AddedEach((_, _) => seen++);
        world.Spawn().Add(new Health(1));
        Assert.Equal(1, seen);
    }

    [Fact]
    public void Disposing_A_Subscription_Unsubscribes_Its_Lambda()
    {
        using var world = new World();

        var added = 0;
        var removed = 0;
        var onAdded = world.SubscribeAdded<Health>((entities, _) => added += entities.Length);
        var onRemoved = world.SubscribeRemoved<Health>((entities, _, _) => removed += entities.Length);

        Assert.True(onAdded.Active);
        Assert.Same(world.On<Health>(), onAdded.Signal);

        var entity = world.Spawn().Add(new Health(1));
        entity.Remove<Health>();
        Assert.Equal(1, added);
        Assert.Equal(1, removed);

        onAdded.Dispose();
        onRemoved.Dispose();
        Assert.False(onAdded.Active);

        entity.Add(new Health(2));
        entity.Remove<Health>();
        Assert.Equal(1, added);
        Assert.Equal(1, removed);
    }


    [Fact]
    public void The_Last_Unsubscribe_Stops_Watching_The_Type()
    {
        using var world = new World();
        var typeId = TypeExpression.Of<Health>(default).TypeId;

        var added = world.SubscribeAdded<Health>((_, _) => { });
        var removed = world.SubscribeRemoved<Health>((_, _, _) => { });
        Assert.True(world.Watching(typeId, added: true));
        Assert.True(world.Watching(typeId, added: false));

        added.Dispose();
        Assert.False(world.Watching(typeId, added: true));
        Assert.True(world.Watching(typeId, added: false));

        removed.Dispose();
        Assert.False(world.Watching(typeId, added: false));
    }


    [Fact]
    public void A_Subscription_Only_Removes_Its_Own_Handler()
    {
        using var world = new World();

        var first = 0;
        var second = 0;
        using (world.SubscribeAdded<Health>((_, _) => first++))
        {
            using var _ = world.SubscribeAdded<Health>((_, _) => second++);
            world.Spawn().Add(new Health(1));
        }
        world.SubscribeAdded<Health>((_, _) => second++);

        world.Spawn().Add(new Health(2));

        Assert.Equal(1, first);
        Assert.Equal(2, second);
    }


    [Fact]
    public void A_Disposed_Subscription_Leaves_An_Idle_Signal_For_The_GC()
    {
        using var world = new World();

        static WeakReference SubscribeAndDispose(World world)
        {
            using var subscription = world.SubscribeAdded<Health>((_, _) => { });
            return new WeakReference(subscription.Signal);
        }

        var weak = SubscribeAndDispose(world);

        world.GC();
        System.GC.Collect();
        System.GC.WaitForPendingFinalizers();

        Assert.False(weak.IsAlive);
    }


    [Fact]
    public void A_Subscription_May_Outlive_Its_World()
    {
        var world = new World();
        var subscription = world.SubscribeAdded<Health>(Match.Any, (_, _) => { });
        var removal = world.SubscribeRemoved<Health>((_, _, _) => { });

        // The World disposes what it handed out.
        world.Dispose();
        Assert.False(subscription.Active);
        Assert.False(removal.Active);

        subscription.Dispose();
        subscription.Dispose();
        removal.Dispose();

        Assert.False(subscription.Active);
    }


    [Fact]
    public void GC_Keeps_A_Subscribed_Signal_Even_Without_The_Component()
    {
        using var world = new World();

        var seen = 0;
        using var subscription = world.SubscribeAdded<Health>((entities, _) => seen += entities.Length);

        // No Entity stores Health, nor ever did: the Subscription alone keeps the Signal in service.
        world.GC();
        System.GC.Collect();
        System.GC.WaitForPendingFinalizers();

        Assert.True(subscription.Active);
        Assert.Same(subscription.Signal, world.On<Health>());

        world.Spawn().Add(new Health(1));
        Assert.Equal(1, seen);
    }


    [Fact]
    public void A_Single_Add_Is_A_Span_Of_One()
    {
        using var world = new World();
        var lengths = new List<int>();
        world.On<Position>().AfterAdded += (entities, values) => lengths.Add(entities.Length * 10 + values.Length);

        world.Spawn().Add(new Position(1, 2));

        Assert.Equal([11], lengths);
    }


    [Fact]
    public void A_Spawn_Wave_Is_Raised_Once_With_Aligned_Values()
    {
        using var world = new World();
        var calls = 0;
        var pairs = new List<(Entity, Position)>();
        world.On<Position>().AfterAdded += (entities, values) =>
        {
            calls++;
            for (var i = 0; i < entities.Length; i++) pairs.Add((entities[i], values[i]));
        };

        var spawned = new Entity[5];
        world.Template().Add(new Position(7, 7)).Spawn(spawned).Dispose();

        Assert.Equal(1, calls);
        Assert.Equal(spawned, pairs.Select(p => p.Item1));
        Assert.All(pairs, p => Assert.Equal(new Position(7, 7), p.Item2));
    }


    [Fact]
    public void A_Batch_Is_Raised_Once_Per_Archetype_And_Values_Follow_Their_Entities()
    {
        using var world = new World();
        var entities = new List<Entity>();
        for (var i = 0; i < 6; i++) entities.Add(world.Spawn().Add(new Index(i)));

        var calls = 0;
        var seen = new List<(Entity, int)>();
        world.On<Health>().AfterAdded += (es, hs) =>
        {
            calls++;
            for (var i = 0; i < es.Length; i++) seen.Add((es[i], hs[i].Value));
        };

        world.Query<Index>().Not<Health>().Compile().Batch().Add(new Health(9)).Submit();

        Assert.Equal(1, calls);
        Assert.Equal(6, seen.Count);
        Assert.All(seen, pair => Assert.Equal(9, pair.Item2));
        Assert.Equal(entities.OrderBy(e => e.Value), seen.Select(p => p.Item1).OrderBy(e => e.Value));
    }


    [Fact]
    public void A_Batch_Removal_Reads_Every_Outgoing_Value_In_One_Call()
    {
        using var world = new World();
        for (var i = 0; i < 4; i++) world.Spawn().Add(new Health(i + 1)).Add(new Index(i));

        var calls = 0;
        var total = 0;
        world.On<Health>().BeforeRemoved += (es, hs, _) =>
        {
            calls++;
            Assert.Equal(es.Length, hs.Length);
            foreach (var h in hs) total += h.Value;
        };

        world.Query<Health>().Compile().Batch().Remove<Health>().Submit();

        Assert.Equal(1, calls);
        Assert.Equal(1 + 2 + 3 + 4, total);
    }


    [Fact]
    public void Batch_Handlers_May_Write_Through_The_Span()
    {
        using var world = new World();
        for (var i = 0; i < 4; i++) world.Spawn().Add(new Index(i));

        world.On<Health>().AfterAdded += (_, values) =>
        {
            for (var i = 0; i < values.Length; i++) values[i] = new Health(100 + i);
        };

        world.Query<Index>().Not<Health>().Compile().Batch().Add(new Health(0)).Submit();

        var stored = new List<int>();
        world.Query<Health>().Stream().For((ref Health h) => stored.Add(h.Value));
        Assert.Equal([100, 101, 102, 103], stored.OrderBy(v => v));
    }
}
