using fennecs.storage;

namespace fennecs.tests.Storage;

public class RTests
{
    [Fact]
    public void Can_Read()
    {
        // TODO: Implement this on the actual component on an actual entity.
        var x = 1;
        var r = new R<int>(in x);
        
        Assert.Equal(1, r.read);
    }
    
    [Fact]
    public void Implicitly_Casts_to_Value()
    {
        // TODO: Implement this on the actual component on an actual entity.
        var x = 1;
        var r = new R<int>(in x);
        
        Assert.Equal(1, r);
    }
}
