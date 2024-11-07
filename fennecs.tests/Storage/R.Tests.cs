using fennecs.storage;

namespace fennecs.tests.Storage;

public class RTests
{
    [Fact]
    public void Can_Read()
    {
        // TODO: Implement this on the actual component on an actual entity.
        var x = 1;
        var rw = new R<int>(ref x);
        
        Assert.Equal(1, rw.read);
    }
    
    [Fact]
    public void Implicitly_Casts_to_Value()
    {
        var x = 1;
        var r = new R<int>(ref x);
        
        Assert.Equal(1, r);
    }
}
