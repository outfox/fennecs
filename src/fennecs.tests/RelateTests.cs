namespace fennecs.tests;

public class RelateTests(ITestOutputHelper output)
{
    [Fact]
    public void Relate_has_ToString()
    {
        using var world = new World();
        var entity = world.Spawn();
        
        var target = Relate.To(entity);
        output.WriteLine(target.ToString());
        Assert.Equal(entity.Key.ToString(), target.ToString());
    }
}
