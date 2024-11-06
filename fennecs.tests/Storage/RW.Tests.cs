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
        var match = default(Match);
        var rw = new RW<int>(ref x, ref entity, ref match);
        
        Assert.Equal(1, rw.read);
    }

    [Fact]
    public void Can_Write()
    {
        using var world = new World();
        var entity = world.Spawn();
        
        var x = 1;
        
        var match = default(Match);
        // ReSharper disable once UseObjectOrCollectionInitializer
        var rw = new RW<int>(ref x, ref entity, ref match);
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

        var match = default(Match);
        var rw = new RW<int>(ref x, ref entity, ref match);
        
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

        Match match = target;
        Assert.True(entity.Has<int>(match));

        var rw = new RW<int>(ref x, ref entity, ref match);
        
        Assert.Equal(77, rw.consume);
        Assert.False(entity.Has<int>(match));
    }

    [Fact]
    public void Can_Remove()
    {
        using var world = new World();
        var entity = world.Spawn();

        int x = 77;
        entity.Add(x);

        Assert.True(entity.Has<int>());

        var plain = Match.Plain;
        var rw = new RW<int>(ref x, ref entity, ref plain);
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
}
