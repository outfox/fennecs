namespace fennecs.tests;

public class EntityTypelessTests
{
    [Fact]
    public void Has_Component()
    {
        using var world = new World();
        var entity = world.Spawn();

        object boxed = 123;
        entity.Set(boxed);
        
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
        entity.Set(boxed);
        
        Assert.Equal(123, entity.Read<int>());
        Assert.Equal(123, entity.Get(typeof(int)));
        Assert.Equal(boxed, entity.Get<int>(Match.Any)[0]);
        Assert.Equal(boxed, entity.Get<int>(default));
    }

    [Fact]
    public void Set_Component_Literal()
    {
        using var world = new World();
        var entity = world.Spawn();

        object boxed = 123;
        entity.Set(123);
        
        Assert.Equal(123, entity.Ref<int>().Read);
        Assert.Equal(123, entity.Get(typeof(int)));
        Assert.Equal(boxed, entity.Get<int>(Match.Any)[0]);
        Assert.Equal(boxed, entity.Get<int>(default));
    }

    [Fact]
    public void Clear_Component()
    {
        using var world = new World();
        var entity = world.Spawn();
        entity.Add(123);

        entity.Remove(TypeExpression.Of<int>());
        
        Assert.Null(entity.Get(typeof(int)));
        Assert.False(entity.Get(typeof(int), out _));
        Assert.Empty(entity.Get<int>(Match.Any));
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
        
        Assert.Null(entity.Get(typeof(int)));
        Assert.False(entity.Get(typeof(int), out _));
        Assert.Empty(entity.Get<int>(Match.Any));
    }

    [Fact]
    public void Cannot_Duplicate_Set()
    {
        using var world = new World();
        var entity = world.Spawn();
        
        object boxed = 123;
        entity.Set(boxed);
        
        Assert.Throws<InvalidOperationException>(() => entity.Set(boxed));
    }
    
    [Fact]
    public void Cannot_Set_Wildcard()
    {
        using var world = new World();
        var entity = world.Spawn();
        
        object boxed = 123;
        
        Assert.Throws<ArgumentException>(() => entity.Set(boxed, Match.Link));
    }
    
    [Fact]
    public void Cannot_Clear_Nonexistent()
    {
        using var world = new World();
        var entity = world.Spawn();
        
        Assert.Throws<InvalidOperationException>(() => entity.Remove<int>());
    }

    [Fact]
    public void Can_Clear_Wildcard()
    {
        using var world = new World();
        var entity = world.Spawn();
        entity.Add(123);
        entity.Add(456, entity);

        entity.Remove(MatchExpression.Of(typeof(int), Match.Entity));
        entity.Remove(MatchExpression.Of(typeof(int), default));
        
        Assert.Null(entity.Get(typeof(int)));
        Assert.False(entity.Get(typeof(int), out _));
        Assert.Empty(entity.Get<int>(Match.Any));
    }

    [Fact]
    public void Can_Clear_Any()
    {
        using var world = new World();
        var entity = world.Spawn();
        entity.Add(123);
        entity.Add(456, entity);

        entity.Remove(typeof(int), Match.Any);
        
        Assert.Null(entity.Get(typeof(int)));
        Assert.False(entity.Get(typeof(int), out _));
        Assert.Empty(entity.Get<int>(Match.Any));
    }
}
