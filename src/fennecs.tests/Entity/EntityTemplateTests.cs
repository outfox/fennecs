// SPDX-License-Identifier: MIT

using System.Collections.Immutable;

namespace fennecs.tests;

public class EntityTemplateTests
{
    [Fact]
    public void Can_Obtain_Template()
    {
        using var world = new World();
        using var template = world.Template();
        Assert.NotNull(template);
    }

    
    [Fact]
    public void Can_Dispose_Template_Once()
    {
        using var world = new World();
        var template = world.Template();
        template.Dispose();
        Assert.Throws<ObjectDisposedException>(template.Dispose);
    }


    [Fact]
    public void Disposed_Template_Cannot_Be_Used()
    {
        using var world = new World();
        var template = world.Template();
        template.Dispose();

        Assert.Throws<ObjectDisposedException>(() => template.Add(123));
        Assert.Throws<ObjectDisposedException>(() => template.Remove<int>());
        Assert.Throws<ObjectDisposedException>(() => template.Spawn());
        Assert.Throws<ObjectDisposedException>(() => template.Spawn(5));
        Assert.Throws<ObjectDisposedException>(() =>
        {
            var wave = new Entity[2];
            template.Spawn(wave);
        });
        Assert.Throws<ObjectDisposedException>(() => template.Needs<int>());
    }

    
    [Fact]
    public void Can_Spawn_One_Entity()
    {
        using var world = new World();
        using var template = world.Template().Add(123);
        var entity = template.Spawn();
        Assert.Equal(1, world.Count);
        Assert.True(entity.Alive);
        Assert.Equal(123, entity.Ref<int>());
        template.Spawn(1);
        Assert.Equal(2, world.Count);
    }


    [Fact]
    public void Spawn_Returns_Distinct_Entities()
    {
        using var world = new World();
        using var template = world.Template();
        var first = template.Spawn();
        var second = template.Spawn();
        Assert.NotEqual(first, second);
        Assert.True(first.Alive);
        Assert.True(second.Alive);
    }


    [Fact]
    public void Spawn_Recycles_With_New_Generation()
    {
        using var world = new World();
        using var template = world.Template();
        var first = template.Spawn();
        world.Despawn(first);
        var second = template.Spawn();
        Assert.False(first.Alive);
        Assert.True(second.Alive);
        Assert.NotEqual(first, second);
    }

    
    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(10)]
    [InlineData(69)]
    [InlineData(420)]
    [InlineData(1_000_000)]
    public void Can_Spawn_Specific_Numbers_of_Entities(int amount)
    {
        using var world = new World();
        using var template = world.Template();
        template.Spawn(amount);
        Assert.Equal(amount, world.Count);
    }


    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(10)]
    [InlineData(69)]
    public void Can_Spawn_With_Component(int amount)
    {
        using var world = new World();
        using var template = world.Template();
        template.Add(Random.Shared.Next());
        template.Spawn(amount);

        using var query = world.Query<int>().Compile();
        Assert.Equal(amount, query.Count);
    }
    

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(13)]
    [InlineData(42)]
    [InlineData(69)]
    [InlineData(444)]
    [InlineData(4096)]
    public void Can_Add_Component_and_Spawn_Again(int amount)
    {
        using var world = new World();
        using var template = world.Template();
        template.Add(Random.Shared.Next());
        template.Spawn(amount);

        template.Add(Random.Shared.NextSingle());
        template.Spawn(amount * 2);

        using var query0 = world.Query<int>().Compile();
        using var query1 = world.Query<float>().Compile();

        Assert.Equal(amount * 3, query0.Count);
        Assert.Equal(amount * 2, query1.Count);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(31)]
    [InlineData(41)]
    [InlineData(69)]
    [InlineData(512)]
    [InlineData(4096)]
    public void Can_Remove_Component_and_Spawn_Again(int amount)
    {
        using var world = new World();
        using var template = world.Template();
        
        template
            .Add(Random.Shared.Next())
            .Add(Random.Shared.NextSingle())
            .Spawn(amount);

        template.Remove<int>()
            .Spawn(amount);

        using var query0 = world.Query<int>().Compile();
        using var query1 = world.Query<float>().Compile();

        Assert.Equal(amount * 1, query0.Count);
        Assert.Equal(amount * 2, query1.Count);
    }

    [Fact]
    public void Safe_to_Spawn_Negative_Amounts()
    {
        using var world = new World();
        using var template = world.Template().Add(123); //Must add Component to cause inner loop to run
        template.Spawn(-1);
        template.Spawn(-2);
        template.Spawn(-69);
        
        Assert.Equal(0, world.Count);
    }


    private record Type69;
    private struct Type42;
    
    [Fact]
    public void Can_Add_Plain_Newable()
    {
        using var world = new World();
        using var template1 = world.Template().Add<Type69>(); 
        template1.Spawn();
        
        using var query1 = world.Query<Type69>().Compile();
        Assert.Single(query1);
        Assert.Equal(1, world.Count);

        using var template2 = world.Template().Add<Type42>(); 
        template2.Spawn();
        
        using var query2 = world.Query<Type42>().Compile();
        Assert.Single(query2);
        Assert.Equal(2, world.Count);

        var entity = world.Spawn();
        entity.Add<Type69>();
        entity.Add<Type42>();
        
        Assert.Equal(2, query1.Count);
        Assert.Equal(2, query2.Count);
        Assert.Equal(3, world.Count);
    }

    [Fact]
    public void Can_Add_Relation()
    {
        using var world = new World();
        var other = world.Spawn();
        using var template1 = world.Template().Add<Type69>(new(), other); 
        template1.Spawn();
        
        using var query1 = world.Query<Type69>(other).Compile();
        Assert.Single(query1);
        Assert.Equal(2, world.Count);

        using var template2 = world.Template().Add<Type42>(other);  // Newable implcit new()
        template2.Spawn();
        
        using var query2 = world.Query<Type42>(other).Compile();
        Assert.Single(query2);
        Assert.Equal(3, world.Count);

        var entity = world.Spawn();
        entity.Add<Type69>(other);
        entity.Add<Type42>(other);
        
        Assert.Equal(2, query1.Count);
        Assert.Equal(2, query2.Count);
        Assert.Equal(4, world.Count);
    }

    [Fact]
    public void Can_Remove_Relation()
    {
        using var world = new World();
        var other = world.Spawn();
        using var template1 = world.Template().Add<Type69>(new(), other); 
        template1.Spawn();
        
        using var query1 = world.Query<Type69>(other).Compile();
        Assert.Single(query1);
        Assert.Equal(2, world.Count);
        
        template1.Remove<Type69>(other).Spawn();
        Assert.Single(query1);
        Assert.Equal(3, world.Count);
    }

    [Fact]
    public void Can_Remove_Link()
    {
        using var world = new World();
        var other = world.Spawn();
        using var template1 = world.Template().Add(Link.With("hello")); 
        template1.Spawn();
        
        using var query1 = world.Query<string>(Link.With("hello")).Compile();
        Assert.Single(query1);
        Assert.Equal(2, world.Count);
        
        template1.Remove("hello").Spawn();
        Assert.Single(query1);
        Assert.Equal(3, world.Count);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(69)]
    [InlineData(4096)]
    public void Can_Spawn_Into_Span(int amount)
    {
        using var world = new World();
        using var template = world.Template().Add(123);

        var entities = new Entity[amount];
        template.Spawn(entities.AsSpan());

        Assert.Equal(amount, world.Count);
        Assert.Equal(amount, entities.ToImmutableSortedSet().Count);
        Assert.All(entities, entity =>
        {
            Assert.True(entity.Alive);
            Assert.Equal(123, entity.Ref<int>());
        });
    }

    [Fact]
    public void Can_Spawn_Into_Stackalloc_Span()
    {
        using var world = new World();
        using var template = world.Template().Add(123);

        Span<Entity> entities = stackalloc Entity[16];
        template.Spawn(entities);

        Assert.Equal(16, world.Count);
        foreach (var entity in entities) Assert.True(entity.Alive);
    }

    [Fact]
    public void Empty_Span_Spawns_Nothing()
    {
        using var world = new World();
        using var template = world.Template().Add(123);
        template.Spawn(Span<Entity>.Empty);
        Assert.Equal(0, world.Count);
    }

    [Fact]
    public void Span_Spawn_Is_Fluent()
    {
        using var world = new World();
        using var template = world.Template().Add(123);

        var first = new Entity[3];
        var second = new Entity[5];
        template.Spawn(first).Add(2.0f).Spawn(second);

        using var ints = world.Query<int>().Compile();
        using var floats = world.Query<float>().Compile();
        Assert.Equal(8, ints.Count);
        Assert.Equal(5, floats.Count);
        Assert.All(second, entity => Assert.True(entity.Has<float>()));
    }

    [Fact]
    public void Template_Reusable_After_Span_Spawn()
    {
        using var world = new World();
        using var template = world.Template().Add(123);

        var first = new Entity[4];
        template.Spawn(first);
        template.Spawn(2);
        var second = new Entity[4];
        template.Spawn(second);

        Assert.Equal(10, world.Count);
        Assert.Equal(8, first.Concat(second).ToImmutableSortedSet().Count);
    }

    [Fact]
    public void Span_Spawn_Recycles_With_New_Generations()
    {
        using var world = new World();
        using var template = world.Template();

        var first = new Entity[10];
        template.Spawn(first.AsSpan());
        world.Despawn(first);

        var second = new Entity[10];
        template.Spawn(second.AsSpan());

        Assert.All(first, entity => Assert.False(entity.Alive));
        Assert.All(second, entity => Assert.True(entity.Alive));
        Assert.Empty(first.Intersect(second));
    }

    private record struct Position(float X, float Y);
    private record struct CrewData(int Count);

    [Fact]
    public void Can_Spawn_Into_Span_Across_Aspects()
    {
        using var world = new World();
        var visuals = world.AddAspect("visuals").Owns<Position>();
        var game = world.AddAspect("game").Owns<CrewData>();

        using var template = world.Template()
            .Add(new Position(1, 2))
            .Add(new CrewData(5));

        var entities = new Entity[100];
        template.Spawn(entities.AsSpan());

        Assert.Equal(100, world.Count);
        Assert.Equal(100, visuals.Count);
        Assert.Equal(100, game.Count);
        Assert.All(entities, entity =>
        {
            Assert.True(entity.Alive);
            Assert.Equal(new(1, 2), entity.Ref<Position>());
            Assert.Equal(new(5), entity.Ref<CrewData>());
        });
    }
}
