namespace fennecs.tests.Integration;

public class ObjectTrackingTests
{
    
    [Fact(Skip = "cleanup")]
    private void Query_serves_Tracked_Objects()
    {
        var world = new World();
        world.Spawn().AddLink("foo").Id();

        var query = world.Query<string>().Build();
        
        var didRun = false;
        query.ForEach((ref string s) =>
        {
            didRun = true;
            Assert.Equal("foo", s);
        });
        
        Assert.True(didRun);
    }
}