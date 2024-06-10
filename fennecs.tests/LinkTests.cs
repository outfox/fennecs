namespace fennecs.tests;

public class LinkTests(ITestOutputHelper output)
{
    [Fact]
    public void Can_Create_Link()
    {
        var link = Link.With("123");
        Assert.Equal("123", link.Object);
    }


    [Fact]
    public void Stable_To_String()
    {
        var link0 = Link.With("123");
        output.WriteLine(link0.ToString());        

        var link1 = Link.With<string>(null!);
        output.WriteLine(link1.ToString());        
    }
}
