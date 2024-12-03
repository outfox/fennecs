using fennecs.storage;

namespace fennecs.tests.Storage;

public class RWImmediateTests
{
    [Fact]
    public void Can_Read()
    {
        using var world = new World();
        var entity = world.Spawn();
        
        var x = 1;
        var rw = new RWImmediate<int>(ref x, entity, default);
        
        Assert.Equal(1, rw.Read);
    }

    [Fact]
    public void Can_Write()
    {
        using var world = new World();
        var entity = world.Spawn();
        
        var x = 1;
        
        // ReSharper disable once UseObjectOrCollectionInitializer
        var rw = new RWImmediate<int>(ref x, entity, default);
        rw.Write = 2; // user usually does not use initializer code
        
        Assert.Equal(2, rw.Read);
    }
    
    [Fact]
    public void Can_Consume()
    {
        using var world = new World();
        var entity = world.Spawn();
        
        var x = 77; // TODO: Implement this on the actual component
        entity.Add(x);

        var rw = new RWImmediate<int>(ref x, entity, default);
        
        Assert.Equal(77, rw.Consume);
        Assert.False(entity.Has<int>());
    }
}
