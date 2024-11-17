using fennecs.events;
using fennecs.storage;

namespace fennecs.tests.Storage;

public class RWTests
{
    [Fact]
    public void Can_Read()
    {
        using var world = new World();
        var entity = world.Spawn();
        
        var x = 1;
        var type = TypeExpression.Of<int>(default);
        var b = false;
        var rw = new RW<int>(ref x, ref b, in entity, in type);
        
        Assert.Equal(1, rw.read);
    }

    [Fact]
    public void Implicitly_Casts_to_Value()
    {
        using var world = new World();
        var entity = world.Spawn();
        
        var x = 1;
        var type = TypeExpression.Of<int>(default);
        var b = false;
        var rw = new RW<int>(ref x, ref b, in entity, in type);
        
        Assert.Equal(1, rw);
    }

    [Fact]
    public void Can_Write()
    {
        using var world = new World();
        var entity = world.Spawn();
        
        var x = 1;
        
        var type = TypeExpression.Of<int>(default);
        var b = false;
        // ReSharper disable once UseObjectOrCollectionInitializer
        var rw = new RW<int>(ref x, ref b, in entity, in type);
        rw.write = 2; // user usually does not use initializer code
        
        Assert.Equal(2, rw.read);
    }
    
    [Fact]
    public void Can_Consume()
    {
        using var world = new World();
        var entity = world.Spawn();
        
        var x = 77; // TODO: Implement this on the actual component
        entity.Add(x);

        var type = TypeExpression.Of<int>(default);
        var b = false;
        var rw = new RW<int>(ref x, ref b, in entity, in type);
        
        Assert.Equal(77, rw.consume);
        Assert.False(entity.Has<int>());
    }

    [Fact]
    public void Can_Consume_Relation()
    {
        using var world = new World();
        var entity = world.Spawn();
        var target = world.Spawn();
        
        var x = 77; // TODO: Implement this on the actual component
        entity.Add(x, target);

        var type = TypeExpression.Of<int>(target);
        Assert.True(entity.Has<int>(target));

        var b = false;
        var rw = new RW<int>(ref x, ref b, in entity, in type);
        
        Assert.Equal(77, rw.consume);
        Assert.False(entity.Has<int>(target));
    }

    [Fact]
    public void Can_Remove()
    {
        using var world = new World();
        var entity = world.Spawn();

        var x = 77;
        entity.Add(x);

        Assert.True(entity.Has<int>());

        var type = TypeExpression.Of<int>(default);
        
        var b = false;
        var rw = new RW<int>(ref x, ref b, in entity, in type);
        rw.Remove();
        Assert.False(entity.Has<int>());
    }
    
    [Fact]
    public void Can_Remove_Relation()
    {
        using var world = new World();
        var entity = world.Spawn();
        var target = world.Spawn();
        
        const int x = 77;
        entity.Add(x, target);

        Match match = target;
        Assert.True(entity.Has<int>(match));
        
        entity.RW<int>(match).Remove();
        Assert.False(entity.Has<int>(match));
    }
    
    
    private struct Type69 : Modified<Type69>;

    [Fact]
    public void Triggers_Entities_on_Modified_Value()
    {
        using var world = new World();
        var entity = world.Spawn();
        entity.Add(new Type69());

        var match = default(Match);
        var rw = entity.RW<Type69>(match);

        var modified = false;
        
        Modified<Type69>.Entities += entities =>
        {
            modified = true;
            Assert.Equal(1, entities.Length);
            Assert.Equal(entity, entities[0]);
        };
        
        rw.write = new();
        Assert.True(modified);
        
        Modified<Type69>.Clear();
    }

    private class Type42 : Modified<Type42>;

    [Fact]
    public void Triggers_Entities_on_Modified_Reference()
    {
        using var world = new World();
        var entity = world.Spawn();
        entity.Add(new Type42());

        var match = default(Match);
        var rw = entity.RW<Type42>(match);

        var modified = false;
        
        Modified<Type42>.Entities += entities =>
        {
            modified = true;
            Assert.Equal(1, entities.Length);
            Assert.Equal(entity, entities[0]);
        };
        
        rw.write = new();
        Assert.True(modified);

        Modified<Type42>.Clear();
    }

    [Fact]
    public void Triggers_Values_on_Modified_Reference()
    {
        using var world = new World();
        var entity = world.Spawn();
        var original = new Type42();
        entity.Add(original);

        var match = default(Match);
        var rw = entity.RW<Type42>(match);

        var modified = false;
        var updated = new Type42();
        
        Modified<Type42>.Values += (entities, originals, updateds) =>
        {
            modified = true;
            Assert.Equal(1, entities.Length);
            Assert.Equal(entity, entities[0]);
            Assert.Equal(original, originals[0]);
            Assert.Equal(updated, updateds[0]);
        };
        
        rw.write = updated;
        Assert.True(modified);
        
        Modified<Type42>.Clear();
    }
}
