namespace fennecs.tests;

public class EntityTypelessTests
{
    [Fact]
    public void Has_Component()
    {
        using var world = new World();
        var entity = world.Spawn();

        object boxed = 123;
        entity.Add(boxed);
        
        Assert.True(entity.Has(typeof(int), Match.Any));
        Assert.True(entity.Has(typeof(int), default));
        Assert.False(entity.Has(typeof(int), Match.Entity));
        Assert.False(entity.Has(typeof(int), Match.Target));
        Assert.False(entity.Has(typeof(int), Match.Link));
        Assert.True(entity.Has<int>());
    }

    [Fact]
    public void Set_Component()
    {
        using var world = new World();
        var entity = world.Spawn();

        object boxed = 123;
        entity.Add(boxed);
        
        Assert.Equal(123, entity.Read<int>());
        Assert.Equal(123, entity.Get(typeof(int)));
        Assert.Equal(boxed, entity.GetAll<int>(Match.Any)[0].value);
        Assert.Equal(boxed, entity.Get<int>(default));
    }

    [Fact]
    public void Set_Component_Literal()
    {
        using var world = new World();
        var entity = world.Spawn();

        object boxed = 123;
        entity.Add(123);
        
        Assert.Equal(123, entity.Ref<int>().Read);
        Assert.Equal(123, entity.Get(typeof(int)));
        Assert.Equal(boxed, entity.GetAll<int>(Match.Any)[0].value);
        Assert.Equal(boxed, entity.Get<int>(default));
    }

    [Fact]
    public void Clear_Component()
    {
        using var world = new World();
        var entity = world.Spawn();
        entity.Add(123);

        entity.Remove(TypeExpression.Of<int>());
        
        Assert.False(entity.TryGet(typeof(int), out _));
        Assert.Empty(entity.GetAll<int>(Match.Any));
    }

    [Fact]
    public void Clear_Component_Typeless()
    {
        using var world = new World();
        var entity = world.Spawn();
        entity.Add(123);

#pragma warning disable CA2263
        entity.Remove(MatchExpression.Of(typeof(int), default));
#pragma warning restore CA2263
        
        Assert.False(entity.TryGet(typeof(int), out _));
        Assert.Empty(entity.GetAll<int>(Match.Any));
    }

    [Fact]
    public void Can_Boxed_Set()
    {
        using var world = new World();
        var entity = world.Spawn().Add(42);
        
        object boxed = 123;
        Assert.True(boxed is int);
        
        entity.Set(boxed);
        
        Assert.Equal(123, entity.Get<int>());
    }
    
    [Fact]
    public void Can_object_Set()
    {
        using var world = new World();
        var original = new object();
        
        var entity = world.Spawn().Add(original);
        Assert.Equal(original, entity.Get<object>());
        
        var other = new object();
        entity.Set(other);
        
        Assert.Equal(other, entity.Get<object>());
        Assert.NotEqual(original, entity.Get<object>());
    }
    
    [Fact]
    public void Can_Get_Set_Wildcard()
    {
        using var world = new World();
        var other = world.Spawn();
        var entity = world.Spawn()
            .Add(420)
            .Add(69, other);

        Assert.Equal(420, entity.GetAll<int>(Match.Plain)[0].value);
        
        object boxed = 123;
        entity.Set(boxed, Match.Any);
        
        Assert.Equal(boxed, entity.Get<int>(other));
        Assert.Equal(boxed, entity.Get<int>());

        var all = entity.GetAll<int>(Match.Any);
        Assert.Equal(boxed, all[0].value);
        Assert.Equal(boxed, all[1].value);
        Assert.Equal(2, all.Count);
        
    }
    
    [Fact]
    public void Cannot_Clear_Nonexistent()
    {
        using var world = new World();
        var entity = world.Spawn();
        
        Assert.Throws<InvalidOperationException>(() => entity.Remove<int>());
    }
    
    [Fact]
    public void Cannot_Get_Nonexistent()
    {
        using var world = new World();

        var entity = world.Spawn();
        Assert.Throws<InvalidOperationException>(() => entity.Get(typeof(int)));
    }

    
    [Fact]
    public void Can_Clear_Wildcard()
    {
        using var world = new World();
        var entity = world.Spawn();
        entity.Add(123);
        entity.Add(456, entity);

#pragma warning disable CA2263
        entity.Remove(MatchExpression.Of(typeof(int), Match.Entity));
        entity.Remove(MatchExpression.Of(typeof(int), default));
#pragma warning restore CA2263

        Assert.False(entity.TryGet(typeof(int), out var none));
        
        Assert.Empty(entity.GetAll<int>(Match.Any));
    }

    [Fact]
    public void Can_Clear_Any()
    {
        using var world = new World();
        var entity = world.Spawn();
        entity.Add(123);
        entity.Add(456, entity);
        
        entity.Remove(MatchExpression.Of<int>(Match.Any));
        
        Assert.False(entity.TryGet(typeof(int), out _));
        
        Assert.Empty(entity.GetAll<int>(Match.Any));
    }
    
    [Fact]
    public void Get_Outputs_Null_When_No_Component()
    {
        using var world = new World();
        var entity = world.Spawn();
        
        Assert.False(entity.TryGet(typeof(int), out var none));
        Assert.Null(none);
    }
}
