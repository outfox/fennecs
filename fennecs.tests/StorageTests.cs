namespace fennecs.tests;

public class StorageTests
{
    private struct ValueType;

    private class ReferenceType;

    [Fact]
    public void Storage_Can_Be_Created()
    {
        Assert.NotNull(new Storage<ValueType>());
        Assert.NotNull(new Storage<ReferenceType>());
    }

    [Fact]
    public void Storage_Stores_Values()
    {
#pragma warning disable CA1859
        IStorage storage = new Storage<int>();
#pragma warning restore CA1859

        storage.Append(1);
        Assert.Equal(1, storage.Count);
        storage.Append(337, 2);
        Assert.Equal(3, storage.Count);

        var refStorage = new Storage<ReferenceType>();
        var rt = new ReferenceType();
        refStorage.Append(rt);
        Assert.Equal(1, refStorage.Count);
        refStorage.Append(rt, 2);
        Assert.Equal(3, refStorage.Count);
        Assert.Equal(rt, refStorage[0]);
        Assert.Equal(rt, refStorage[1]);
        Assert.Equal(rt, refStorage[2]);
    }

    [Fact]
    public void Storage_Interface_Denies_Wrong_Types()
    {
#pragma warning disable CA1859
        IStorage storage = new Storage<int>();
#pragma warning restore CA1859

        Assert.Throws<InvalidCastException>(() => storage.Append(8.5f));
        Assert.Throws<InvalidCastException>(() => storage.Append("Dieter", 69));
        Assert.Throws<InvalidCastException>(() => storage.Append(new object()));
        storage.Append(420);
    }

    [Fact]
    public void Storage_Can_Blit()
    {
        var storage = new Storage<int>();

#pragma warning disable CA1859
        IStorage generic = storage;
#pragma warning restore CA1859

        generic.Append(7, 3);
        Assert.Equal(7, storage[0]);
        Assert.Equal(7, storage[1]);
        Assert.Equal(7, storage[2]);

        generic.Blit(42);
        Assert.Equal(42, storage[0]);
        Assert.Equal(42, storage[1]);
        Assert.Equal(42, storage[2]);
    }

    [Fact]
    public void Storage_Can_Clear()
    {
        var storage = new Storage<int>();
        storage.Append(7, 3);
        Assert.Equal(3, storage.Count);

        storage.Clear();
        Assert.Equal(0, storage.Count);
        Assert.Equal(default, storage[0]);

        storage.Clear(); // clear empty storage
        Assert.Equal(0, storage.Count);
        Assert.Equal(default, storage[0]);
    }

    [Fact]
    public void Storage_Contiguous_After_Delete()
    {
        var storage = new Storage<int>();
        storage.Append(420, 3);
        storage.Append(69, 3);
        Assert.Equal(6, storage.Count);

        storage.Delete(1);
        Assert.Equal(5, storage.Count);
        
        // Check if element was moved into gap from the back!
        Assert.Equal(420, storage[0]);
        Assert.Equal(69, storage[1]);
        Assert.Equal(420, storage[2]);
        Assert.Equal(69, storage[3]);
        Assert.Equal(69, storage[4]);
        Assert.Equal(default, storage[5]);
    }

    [Fact]
    public void Storage_Can_Compact()
    {
        var storage = new Storage<float>();
        for (var i = 0; i < 10; i++)
        {
            storage.Append(i * 1.337f);
        }
        Assert.Equal(10, storage.Count);
        //Assert.Equal(16, storage.Capacity); //This is not guaranteed based on how Arraypools work
        
        storage.Delete(3, 5);
        storage.Compact();
        Assert.Equal(5, storage.Count);
        //Assert.Equal(8, storage.Capacity); //This is not guaranteed based on how Arraypools work
    }
    
    
    [Fact]
    public void Storage_Identical_After_Compact()
    {
        var storage = new Storage<int>();
        storage.Append(420, 3);
        storage.Append(69, 2);
        Assert.Equal(5, storage.Count);

        storage.Delete(1);
        Assert.Equal(4, storage.Count);
        //Assert.Equal(8, storage.Capacity); //This is not guaranteed based on how Arraypools work

        storage.Compact(); // should internally resize down to 4
        //Assert.Equal(4, storage.Capacity); //This is not guaranteed based on how Arraypools work
        Assert.Equal(4, storage.Count);

        Assert.Equal(420, storage[0]);
        Assert.Equal(69, storage[1]);
        Assert.Equal(420, storage[2]);
        Assert.Equal(69, storage[3]);
    }

    [Fact]
    public void Can_Append_or_Delete_Zero()
    {
        var storage = new Storage<int>();
        storage.Append(420, 0);
        Assert.Equal(0, storage.Count);

        storage.Append(420, 3);
        storage.Delete(1, 0);
        Assert.Equal(3, storage.Count);
        Assert.Equal(420, storage[0]);
        Assert.Equal(420, storage[1]);
        Assert.Equal(420, storage[2]);
    }

    [Fact]
    public void Can_Migrate_Generic()
    {
        var source = new Storage<string>();
        var destination = new Storage<string>();
        
        destination.Append("world", 3);
        
        source.Append("hello", 3);

#pragma warning disable CA1859
        var genericSource = (IStorage)source;
#pragma warning restore CA1859
        genericSource.Migrate(destination);
        
        Assert.Equal(0, source.Count);
        Assert.Equal(6, destination.Count);
        
        Assert.Equal("world", destination[0]);
        Assert.Equal("world", destination[1]);
        Assert.Equal("world", destination[2]);
        Assert.Equal("hello", destination[3]);
        Assert.Equal("hello", destination[4]);
        Assert.Equal("hello", destination[5]);
    }


    [Fact]
    public void Can_Move()
    {
        var source = new Storage<string>();
        source.Append("hello", 3);

        var destination = new Storage<string>();
        destination.Append("world", 3);
        
        
        source.Move(1, destination);
        
        Assert.Equal(2, source.Count);
        Assert.Equal(4, destination.Count); 
        
        Assert.Equal("hello", source[0]);
        Assert.Equal("hello", source[1]);
        
        Assert.Equal("world", destination[0]);
        Assert.Equal("world", destination[1]);
        Assert.Equal("world", destination[2]);
        Assert.Equal("hello", destination[3]);

    }

    [Fact]
    public void All_Elements_Moved_After_Migrate()
    {
        var source = new Storage<string>();
        source.Append("hello", 3);

        var destination = new Storage<string>();
        destination.Append("world", 3);
        
        source.Migrate(destination);
        
        Assert.Equal(6, destination.Count);
        
        Assert.Equal("world", destination[0]);
        Assert.Equal("world", destination[1]);
        Assert.Equal("world", destination[2]);
        Assert.Equal("hello", destination[3]);
        Assert.Equal("hello", destination[4]);
        Assert.Equal("hello", destination[5]);
    }

    [Fact]
    public void Empty_After_Migrate()
    {
        var source = new Storage<string>();
        source.Append("hello", 3);

        var destination = new Storage<string>();
        destination.Append("world", 3);
        
        source.Migrate(destination);
        
        Assert.Equal(0, source.Count);
    }

    [Fact]
    public void Can_Store_Object()
    {
        var storage = new Storage<string>();
        storage.Append("world");
        Assert.Equal("world", storage.Span[0]);
        
        object obj = "hello";
        storage.Store(0, obj);
        Assert.Equal(1, storage.Count);
        Assert.Equal("hello", storage.Span[0]);
    }
    
    [Fact]
    public void Can_Get_Type()
    {
        var storage1 = new Storage<string>();
        Assert.Equal(typeof(string), storage1.Type);
        
        var storage2 = new Storage<int>();
        Assert.Equal(typeof(int), storage2.Type);
        
        var storage3 = new Storage<object>();
        Assert.Equal(typeof(object), storage3.Type);
    }

    [Fact]
    public void AsMemory_Default_Is_Entire_Size()
    {
        var storage = new Storage<int>();
        var memory1 = storage.AsMemory();
        Assert.Equal(0, memory1.Length);

        storage.Append(1, 1);
        var memory2 = storage.AsMemory();
        Assert.Equal(1, memory2.Length);

        storage.Append(2, 3);
        var memory3 = storage.AsMemory();
        Assert.Equal(4, memory3.Length);
    }
    
}