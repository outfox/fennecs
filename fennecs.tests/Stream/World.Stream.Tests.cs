namespace fennecs.tests.Stream;

public class WorldStreamTests
{
    [Fact]
    public void Stream_1()
    {
        using var world = new World();
        var stream = world.Stream<int>();

        Assert.Empty(world);
        Assert.Empty(stream);
        
        Assert.Equal(0, stream.Count);
        
        world.Spawn().Add(1);
        Assert.Equal(1, stream.Count);
        
        Assert.NotEmpty(world);
        Assert.NotEmpty(stream);
    }
    
    [Fact]
    public void Stream_2()
    {
        using var world = new World();
        var stream = world.Stream<int, float>();

        Assert.Empty(world);
        Assert.Empty(stream);
        
        Assert.Equal(0, stream.Count);
        
        world.Spawn().Add(1).Add(1.0f);
        Assert.Equal(1, stream.Count);
        
        Assert.NotEmpty(world);
        Assert.NotEmpty(stream);
    }
    
    [Fact]
    public void Stream_3()
    {
        using var world = new World();
        var stream = world.Stream<int, float, string>();

        Assert.Empty(world);
        Assert.Empty(stream);
        
        Assert.Equal(0, stream.Count);
        
        world.Spawn().Add(1).Add(1.0f).Add("a");
        Assert.Equal(1, stream.Count);
        
        Assert.NotEmpty(world);
        Assert.NotEmpty(stream);
    }
    
    [Fact]
    public void Stream_4()
    {
        using var world = new World();
        var stream = world.Stream<int, float, string, bool>();

        Assert.Empty(world);
        Assert.Empty(stream);
        
        Assert.Equal(0, stream.Count);
        
        world.Spawn().Add(1).Add(1.0f).Add("a").Add(true);
        Assert.Equal(1, stream.Count);
        
        Assert.NotEmpty(world);
        Assert.NotEmpty(stream);
    }
    
    [Fact]
    public void Stream_5()
    {
        using var world = new World();
        var stream = world.Stream<int, float, string, bool, char>();

        Assert.Empty(world);
        Assert.Empty(stream);
        
        Assert.Equal(0, stream.Count);
        
        world.Spawn().Add(1).Add(1.0f).Add("a").Add(true).Add('a');
        Assert.Equal(1, stream.Count);
        
        Assert.NotEmpty(world);
        Assert.NotEmpty(stream);
    }
}
