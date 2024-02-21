using fennecs.pools;

namespace fennecs.tests;

public class ReferenceStoreTests(ITestOutputHelper output)
{
    [Fact]
    public void Request_StoresItem()
    {
        var store = new ReferenceStore();
        var item = new object();
        var identity = store.Request(item);
        var stored = store.Get<object>(identity);
        Assert.Equal(item, stored);
    }
    
    [Fact]
    public void Request_Returns_Same_Identity()
    {
        var store = new ReferenceStore();
        var item = new object();
        var identity1 = store.Request(item);
        var identity2 = store.Request(item);
        Assert.Equal(identity1, identity2);
    }

    
    private class BadlyHashingClass
    {
        public override int GetHashCode() => 69;
    }
    
    [Fact]
    public void Request_Throws_On_Hash_Collision()
    {
        var store = new ReferenceStore();
        var item1 = new BadlyHashingClass();
        var item2 = new BadlyHashingClass();
        store.Request(item1);
        Assert.Throws<InvalidOperationException>(() =>store.Request(item2));
    }
    
    [Fact]
    public void Release_RemovesItem()
    {
        var store = new ReferenceStore();
        var item = new object();
        var identity = store.Request(item);
        store.Release(identity);
        Assert.Throws<KeyNotFoundException>(() => store.Get<object>(identity));
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(50)]
    public void Request_RefCount_Matches_Release_RefCount(int count)
    {
        var store = new ReferenceStore();
        var item = new object();
        Entity entity = default;
        
        for (var i = 0; i < count; i++)
        {
            entity = store.Request(item);
        }
        
        Assert.Equal(item, store.Get<object>(entity));
        
        for (var i = 0; i < count; i++)
        {
            store.Release(entity);
        }

        Assert.Throws<KeyNotFoundException>(() => store.Get<object>(entity));
    }

    [Fact]
    public void Release_Fails_If_RefCount_Zero()
    {
        var store = new ReferenceStore();
        var item = new object();
        var identity = store.Request(item);
        store.Release(identity);
        Assert.Throws<KeyNotFoundException>(() => store.Release(identity));
    }

    [Fact]
    public void Get_Fails_If_Released()
    {
        var store = new ReferenceStore();
        var item = new object();
        var identity = store.Request(item);
        store.Release(identity);
        Assert.Throws<KeyNotFoundException>(() => store.Get<object>(identity));
    }

    [Fact]
    public void StoredReference_Different_For_Different_Instances()
    {
        var store = new ReferenceStore();
        var item1 = new object();
        var item2 = new object();
        var identity1 = store.Request(item1);
        var identity2 = store.Request(item2);
        Assert.NotEqual(identity1, identity2);
    }

    [Fact]
    public void StoredReference_ToString()
    {
        var reference = new ReferenceStore.StoredReference<List<int>>
        {
            Item = new List<int>(3),
            Count = 7,
        };
        output.WriteLine(reference.ToString());
        Assert.Equal($"{typeof(List<int>)} x7", reference.ToString());
    }
    
}