// SPDX-License-Identifier: MIT

namespace fennecs.tests;

public class EntityTemplateNeedsTests
{
    private record struct Health(int Value);
    private record struct Position(float X, float Y);
    private record struct Loyalty(float Value);
    private record struct Werewolf;

    private record struct Name(string Value)
    {
        public static implicit operator Name(string value) => new(value);
    }


    [Fact]
    public void Needs_Returns_Wider_Template()
    {
        using var world = new World();
        using var template = world.Template().Needs<Health>();
        Assert.IsType<EntityTemplate<Health>>(template);

        using var wider = world.Template().Needs<Health>().Needs<Position>();
        Assert.IsType<EntityTemplate<Health, Position>>(wider);
    }


    [Fact]
    public void Typed_Spawn_Provides_Required_Values()
    {
        using var world = new World();
        using var template = world.Template()
            .Add<Werewolf>()
            .Needs<Health>()
            .Needs<Position>();

        var entity = template.Spawn(new Health(250), new Position(1, 2));

        Assert.True(entity.Alive);
        Assert.True(entity.Has<Werewolf>());
        Assert.Equal(new Health(250), entity.Ref<Health>());
        Assert.Equal(new Position(1, 2), entity.Ref<Position>());
    }


    [Fact]
    public void Typed_Spawn_Uses_Implicit_Conversions()
    {
        using var world = new World();
        using var template = world.Template().Needs<Name>();

        var entity = template.Spawn("Chonker");
        Assert.Equal(new Name("Chonker"), entity.Ref<Name>());
    }


    [Fact]
    public void Uniform_Bulk_Spawn_Shares_Values()
    {
        using var world = new World();
        using var template = world.Template()
            .Add<Werewolf>()
            .Needs<Health>();

        template.Spawn(10, new Health(9000));

        Assert.Equal(10, world.Count);
        var stream = world.Query<Health>().Stream();
        stream.For((ref Health health) => Assert.Equal(9000, health.Value));
    }


    [Fact]
    public void Uniform_Bulk_Spawn_Delivers_Handles()
    {
        using var world = new World();
        using var template = world.Template().Needs<Health>();

        var pack = new Entity[13];
        template.Spawn(pack, new Health(100));

        Assert.All(pack, entity => Assert.True(entity.Alive));
        Assert.All(pack, entity => Assert.Equal(100, entity.Ref<Health>().Value));
    }


    [Fact]
    public void Factory_Bulk_Spawn_Provides_Per_Entity_Values()
    {
        using var world = new World();
        using var template = world.Template()
            .Needs<Health>()
            .Needs<Position>();

        template.Spawn(100, i => (new Health(i), new Position(i, -i)));

        Assert.Equal(100, world.Count);
        var seen = new HashSet<int>();
        foreach (var entity in world)
        {
            var health = entity.Ref<Health>();
            Assert.Equal(new Position(health.Value, -health.Value), entity.Ref<Position>());
            Assert.True(seen.Add(health.Value));
        }
        Assert.Equal(100, seen.Count);
    }


    [Fact]
    public void Factory_Bulk_Spawn_Delivers_Handles()
    {
        using var world = new World();
        using var template = world.Template().Needs<Health>();

        var pack = new Entity[7];
        template.Spawn(pack, i => new Health(i));

        for (var i = 0; i < pack.Length; i++) Assert.Equal(i, pack[i].Ref<Health>().Value);
    }


    [Fact]
    public void Span_Parallel_Spawn_Provides_Per_Entity_Values()
    {
        using var world = new World();
        using var template = world.Template()
            .Add<Werewolf>()
            .Needs<Health>()
            .Needs<Position>();

        var healths = new Health[5];
        var positions = new Position[5];
        for (var i = 0; i < 5; i++)
        {
            healths[i] = new(i * 10);
            positions[i] = new(i, i);
        }

        var pack = new Entity[5];
        template.Spawn(pack, healths, positions);

        for (var i = 0; i < 5; i++)
        {
            Assert.Equal(healths[i], pack[i].Ref<Health>());
            Assert.Equal(positions[i], pack[i].Ref<Position>());
        }
    }


    [Fact]
    public void Span_Parallel_Spawn_Rejects_Mismatched_Lengths()
    {
        using var world = new World();
        using var template = world.Template().Needs<Health>();

        Assert.Throws<ArgumentException>(() =>
        {
            var pack = new Entity[5];
            template.Spawn(pack, new Health[4]);
        });
    }


    [Fact]
    public void Needs_Relation_Bakes_Target_Value_Per_Spawn()
    {
        using var world = new World();
        var leader = world.Spawn();

        using var template = world.Template()
            .Add<Werewolf>()
            .Needs<Loyalty>(leader);

        var wolf = template.Spawn(new Loyalty(0.8f));

        Assert.True(wolf.Has<Loyalty>(leader));
        Assert.Equal(0.8f, wolf.Ref<Loyalty>(Match.Relation(leader)).Value, 3);
    }


    [Fact]
    public void Needs_Relation_Per_Entity_Values()
    {
        using var world = new World();
        var leader = world.Spawn();

        using var template = world.Template().Needs<Loyalty>(leader);

        template.Spawn(10, i => new Loyalty(i * 0.1f));

        var count = 0;
        foreach (var entity in world)
        {
            if (entity == leader) continue;
            Assert.True(entity.Has<Loyalty>(leader));
            count++;
        }
        Assert.Equal(10, count);
    }


    [Fact]
    public void Add_Then_Needs_Same_Component_Throws()
    {
        using var world = new World();
        using var template = world.Template().Add(new Health(100));

        Assert.Throws<InvalidOperationException>(() => template.Needs<Health>());
    }


    [Fact]
    public void Needs_Then_Add_Same_Component_Throws()
    {
        using var world = new World();
        using var template = world.Template().Needs<Health>();

        Assert.Throws<InvalidOperationException>(() => template.Add(new Health(100)));
    }


    [Fact]
    public void Duplicate_Needs_Throws()
    {
        using var world = new World();
        using var template = world.Template().Needs<Health>();

        Assert.Throws<InvalidOperationException>(() => template.Needs<Health>());
    }


    [Fact]
    public void Needs_Same_Type_Different_Relation_Targets_Is_Allowed()
    {
        using var world = new World();
        var alpha = world.Spawn();
        var beta = world.Spawn();

        // Distinct Type Expressions: same backing type, different secondary keys.
        using var template = world.Template()
            .Needs<Loyalty>(alpha)
            .Needs<Loyalty>(beta);

        var wolf = template.Spawn(new Loyalty(1.0f), new Loyalty(0.5f));

        Assert.True(wolf.Has<Loyalty>(alpha));
        Assert.True(wolf.Has<Loyalty>(beta));
    }


    [Fact]
    public void Remove_Of_Required_Component_Throws()
    {
        using var world = new World();
        using var template = world.Template().Needs<Health>();

        Assert.Throws<InvalidOperationException>(() => template.Remove<Health>());
    }


    [Fact]
    public void Consumed_Template_Cannot_Be_Used()
    {
        using var world = new World();
        var narrow = world.Template().Add<Werewolf>();
        using var wide = narrow.Needs<Health>();

        Assert.Throws<ObjectDisposedException>(() => narrow.Add(new Health(1)));
        Assert.Throws<ObjectDisposedException>(() => narrow.Spawn());
    }


    [Fact]
    public void Consumed_Template_Dispose_Is_NoOp()
    {
        using var world = new World();
        var narrow = world.Template();
        using var wide = narrow.Needs<Health>();

        narrow.Dispose(); // must not throw, and must not return pooled state twice
        narrow.Dispose();

        // the wide template still works
        var entity = wide.Spawn(new Health(42));
        Assert.Equal(42, entity.Ref<Health>().Value);
    }


    [Fact]
    public void Baked_Components_Survive_Widening()
    {
        using var world = new World();
        using var template = world.Template()
            .Add<Werewolf>()
            .Add(new Name("Pack Member"))
            .Needs<Health>()
            .Needs<Position>();

        var entity = template.Spawn(new Health(1), new Position(0, 0));

        Assert.True(entity.Has<Werewolf>());
        Assert.Equal("Pack Member", entity.Ref<Name>().Value);
    }


    [Fact]
    public void Max_Arity_Template_Spawns()
    {
        using var world = new World();
        using var template = world.Template()
            .Needs<int>()
            .Needs<long>()
            .Needs<float>()
            .Needs<double>()
            .Needs<short>()
            .Needs<byte>();

        var entity = template.Spawn(1, 2L, 3f, 4d, (short)5, (byte)6);

        Assert.Equal(1, entity.Ref<int>());
        Assert.Equal(2L, entity.Ref<long>());
        Assert.Equal(3f, entity.Ref<float>());
        Assert.Equal(4d, entity.Ref<double>());
        Assert.Equal(5, entity.Ref<short>());
        Assert.Equal(6, entity.Ref<byte>());
    }


    [Fact]
    public void Per_Entity_Values_Blit_Across_Aspects()
    {
        using var world = new World();
        world.AddAspect("visuals").Owns<Position>();

        using var template = world.Template()
            .Add<Werewolf>()
            .Needs<Health>()      // owned by Main
            .Needs<Position>();   // owned by "visuals" Aspect

        var pack = new Entity[50];
        template.Spawn(pack, i => (new Health(i), new Position(i, i * 2)));

        for (var i = 0; i < pack.Length; i++)
        {
            Assert.Equal(new Health(i), pack[i].Ref<Health>());
            Assert.Equal(new Position(i, i * 2), pack[i].Ref<Position>());
        }
    }


    [Fact]
    public void Zero_And_Negative_Counts_Spawn_Nothing()
    {
        using var world = new World();
        using var template = world.Template().Needs<Health>();

        template.Spawn(0, new Health(1));
        template.Spawn(-5, new Health(1));
        template.Spawn(0, i => new Health(i));
        template.Spawn([], new Health(1));

        Assert.Equal(0, world.Count);
    }


    [Fact]
    public void Typed_Template_Remains_Reusable_And_Mutable()
    {
        using var world = new World();
        using var template = world.Template().Needs<Health>();

        template.Spawn(new Health(1));
        template.Add<Werewolf>(); // baked additions still work after spawning
        var elite = template.Spawn(new Health(2));

        Assert.False(world.Query<Health>().Has<Werewolf>().Stream().Count == 0);
        Assert.True(elite.Has<Werewolf>());
        Assert.Equal(2, world.Count);
    }
}
