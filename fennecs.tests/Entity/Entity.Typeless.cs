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
        Assert.False(entity.Has(typeof(int), Match.Object));
        Assert.True(entity.Has<int>());
    }

    [Fact]
    public void Set_Component()
    {
        using var world = new World();
        var entity = world.Spawn();

        object boxed = 123;
        entity.Set(boxed);
        
        Assert.Equal(123, entity.Ref<int>());
        Assert.Equal(123, entity.Get(typeof(int)));
        Assert.Equal(boxed, entity.Get<int>(default)[0]);
    }

    [Fact]
    public void Set_Component_Literal()
    {
        using var world = new World();
        var entity = world.Spawn();

        object boxed = 123;
        entity.Set(123);
        
        Assert.Equal(123, entity.Ref<int>());
        Assert.Equal(123, entity.Get(typeof(int)));
        Assert.Equal(boxed, entity.Get<int>(default)[0]);
    }

    [Fact]
    public void Clear_Component()
    {
        using var world = new World();
        var entity = world.Spawn();
        entity.Add(123);

        entity.Clear(typeof(int), default);
        
        Assert.Null(entity.Get(typeof(int)));
        Assert.False(entity.Get(out _, typeof(int)));
        Assert.Empty(entity.Get<int>(default));
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
        
        Assert.Throws<ArgumentException>(() => entity.Set(boxed, Match.Object));
    }
    
    [Fact]
    public void Cannot_Clear_Nonexistent()
    {
        using var world = new World();
        var entity = world.Spawn();
        
        Assert.Throws<InvalidOperationException>(() => entity.Clear(typeof(int)));
    }

    [Fact]
    public void Can_Clear_Wildcard()
    {
        using var world = new World();
        var entity = world.Spawn();
        entity.Add(123);
        entity.Add(456, entity);

        entity.Clear(typeof(int), Match.Entity);
        entity.Clear(typeof(int), Match.Plain);
        
        Assert.Null(entity.Get(typeof(int)));
        Assert.False(entity.Get(out _, typeof(int)));
        Assert.Empty(entity.Get<int>(Match.Any));
    }

    [Fact]
    public void Can_Clear_Any()
    {
        using var world = new World();
        var entity = world.Spawn();
        entity.Add(123);
        entity.Add(456, entity);

        entity.Clear(typeof(int), Match.Any);
        
        Assert.Null(entity.Get(typeof(int)));
        Assert.False(entity.Get(out _, typeof(int)));
        Assert.Empty(entity.Get<int>(Match.Any));
    }
}
