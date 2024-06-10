namespace fennecs.tests.Stream;

public class Stream1Tests(ITestOutputHelper output)
{
    [Fact]
    public void Can_Enumerate_Stream()
    {
        using var world = new World();
        var arnold = world.Spawn().Add("Arnold");
        var dolph = world.Spawn().Add("Dolph");
        
        List<(Entity, string)> list = [(arnold, "Arnold"), (dolph, "Dolph")];
        
        var stream = world.Stream<string>();
        foreach (var row in stream)
        {
            Assert.True(list.Remove(row));
        }
        
        Assert.Empty(list);
    }
    
    
    [Fact]
    public void Can_Create_Stream_From_Query()
    {
        using var world = new World();
        var query = world.Query<string>().Compile();
        
        var stream = query.Stream<string>();

        world.Spawn().Add("Arnold");
        world.Spawn().Add("Dolph");
        
        List<string> list = ["Arnold", "Dolph"];
        
        stream.For((ref string c0) =>
        {
            list.Remove(c0);
        });
        
        Assert.Empty(list);
    }
    
    [Fact]
    public void Can_Create_Reference_Stream_From_World()
    {
        using var world = new World();
        
        var stream = world.Stream<string>();

        world.Spawn().Add("Arnold");
        world.Spawn().Add("Dolph");
        
        List<string> list = ["Arnold", "Dolph"];
        
        stream.For((ref string c0) =>
        {
            Assert.True(list.Remove(c0));
        });
        
        Assert.Empty(list);
    }

    [Fact]
    public void Can_Create_Value_Stream_From_World()
    {
        using var world = new World();

        var stream1 = world.Stream<string>();
        var stream2 = world.Stream<int>();

        world.Spawn().Add("Arnold").Add(123);
        world.Spawn().Add("Dolph").Add(678);
        
        List<string> list = ["Arnold", "Dolph"];
        stream1.For((ref string c0) =>
        {
            Assert.True(list.Remove(c0));
        });
        Assert.Empty(list);
        
        List<int> list2 = [123, 678];
        stream2.For((ref int c0) =>
        {
            Assert.True(list2.Remove(c0));
        });
        Assert.Empty(list2);
    }

    [Fact]
    public void World_Stream_Backed_by_Cached_Query()
    {
        using var world = new World();
        var query = world.Query<string>().Compile();
        
        var stream1 = world.Stream<string>();
        var stream2 = world.Stream<string>();
        var stream3 = query.Stream<string>();
        
        Assert.Equal(stream1.Query, stream2.Query);
        Assert.Equal(stream1.Query, stream3.Query);
    }


    [Fact]
    public void Cannot_Run_Job_on_Wildcard_Query()
    {
        using var world = new World();
        world.Spawn().Add("jason");

        var stream = world.Query<string>(Match.Any).Stream();
        Assert.Throws<InvalidOperationException>(() => stream.Job((ref string str) => { output.WriteLine(str);}));

        stream = world.Query<string>(Match.Entity).Stream();
        Assert.Throws<InvalidOperationException>(() => stream.Job((ref string str) => { output.WriteLine(str);}));

        stream = world.Query<string>(Match.Target).Stream();
        Assert.Throws<InvalidOperationException>(() => stream.Job((ref string str) => { output.WriteLine(str);}));

        stream = world.Query<string>(Match.Object).Stream();
        Assert.Throws<InvalidOperationException>(() => stream.Job((ref string str) => { output.WriteLine(str);}));

        stream = world.Query<string>(Match.Plain).Stream();
        var ran = false;
        stream.Job((ref string str) =>
        { 
            output.WriteLine(str); 
            ran = true;
        });
        Assert.True(ran);
        
    }
}
