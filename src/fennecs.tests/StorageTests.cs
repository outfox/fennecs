namespace fennecs.tests;

public class StorageTests
{
    private struct ValueType;

    private class ReferenceType;

    private class DerivedReferenceType : ReferenceType;

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
    public void GetAs_Validates_Type_And_Logical_Bounds()
    {
        var value = new DerivedReferenceType();
        var storage = new Storage<DerivedReferenceType>();
        storage.Append(value);

        Assert.Same(value, storage.GetAs<ReferenceType>(0));
        Assert.Throws<InvalidCastException>(() => storage.GetAs<string>(0));
        Assert.Throws<IndexOutOfRangeException>(() => storage.GetAs<ReferenceType>(storage.Count));
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

        storage.Clear(); // clear empty storage
        Assert.Equal(0, storage.Count);
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
        Assert.True(storage.Capacity >= 16);

        storage.Delete(3, 5);
        storage.Compact();
        Assert.Equal(5, storage.Count);
        Assert.True(storage.Capacity >= 8);
    }


    [Fact]
    public void Storage_Identical_After_Compact()
    {
        var storage = new Storage<int>();
        storage.Append(420, 32);
        storage.Append(69, 32);
        Assert.Equal(64, storage.Count);

        storage.Delete(1);
        Assert.Equal(63, storage.Count);
        Assert.True(storage.Capacity >= 64);

        storage.Compact(); // should internally resize down to 4, but the array pool might just return the same array.
        Assert.True(storage.Capacity >= 32);
        Assert.Equal(63, storage.Count);
    }

    [Fact]
    public void Storage_Compact_Shrinks_After_Bulk_Delete()
    {
        var storage = new Storage<int>();
        storage.Append(7, 100);
        Assert.Equal(100, storage.Count);
        Assert.True(storage.Capacity >= 128); // grown to the next power of 2

        storage.Delete(10, 90);
        Assert.Equal(10, storage.Count);

        // newSize = max(InitialCapacity 32, Count 10) = 32 != 128 -> the shrink path runs
        // (the earlier tests never grew past the point where Compact is a no-op)
        storage.Compact();

        Assert.True(storage.Capacity < 128);
        Assert.True(storage.Capacity >= storage.Count);
        Assert.Equal(10, storage.Count);

        // contents survive the move into the smaller array
        for (var i = 0; i < storage.Count; i++) Assert.Equal(7, storage[i]);
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

        storage.Append(1);
        var memory2 = storage.AsMemory();
        Assert.Equal(1, memory2.Length);

        storage.Append(2, 3);
        var memory3 = storage.AsMemory();
        Assert.Equal(4, memory3.Length);
    }


    [Fact]
    public void Append_Ignores_Zero_And_Negative_Additions()
    {
        var storage = new Storage<int>();
        storage.Append(7);

        storage.Append(8, 0);
        storage.Append(9, -1);

        Assert.Equal(1, storage.Count);
        Assert.Equal(7, storage[0]);
    }


    [Fact]
    public void Delete_Middle_Range_Preserves_Remaining_Values()
    {
        // Shift branch: too few trailing elements to fill the gap.
        var small = new Storage<int>();
        for (var i = 1; i <= 3; i++) small.Append(i);
        small.Delete(0, 2);
        Assert.Equal(1, small.Count);
        Assert.Equal(3, small[0]);

        // Gap-fill branch: the trailing elements are copied into the removal site.
        var big = new Storage<int>();
        for (var i = 0; i < 10; i++) big.Append(i);
        big.Delete(1, 2);
        Assert.Equal(8, big.Count);
        Assert.Equal(0, big[0]);
        Assert.Equal(8, big[1]);
        Assert.Equal(9, big[2]);
        Assert.Equal(3, big[3]);
        Assert.Equal(7, big[7]);
    }


    [Fact]
    public void Delete_Ignores_Zero_And_Negative_Removals()
    {
        var storage = new Storage<int>();
        storage.Append(7);
        storage.Append(8);

        storage.Delete(0, 0);
        storage.Delete(0, -1);

        Assert.Equal(2, storage.Count);
        Assert.Equal(7, storage[0]);
        Assert.Equal(8, storage[1]);
    }


    // Each probe type gets its own static ArrayPool<T>, so these tests observe pool
    // round-trips deterministically without interference from other tests.
    private struct GrowProbe;
    private struct CompactProbe;
    private struct NoOpProbe;
    private sealed class ReferenceProbe;
    private sealed class CompactReferenceProbe;

    private struct MixedReferenceProbe
    {
        public object? Reference;
        public int Value;
    }

    private static T[] BackingArray<T>(Storage<T> storage) =>
        (T[])typeof(Storage<T>).GetField("_data", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!.GetValue(storage)!;

    private static System.Buffers.ArrayPool<T> PoolOf<T>() =>
        (System.Buffers.ArrayPool<T>)typeof(Storage<T>).GetField("Pool", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic)!.GetValue(null)!;


    [Fact]
    public void EnsureCapacity_Returns_Previous_Array_To_Pool()
    {
        var storage = new Storage<GrowProbe>();
        var previous = BackingArray(storage);

        storage.EnsureCapacity(previous.Length * 2);

        Assert.NotSame(previous, BackingArray(storage));
        Assert.Same(previous, PoolOf<GrowProbe>().Rent(previous.Length));
    }


    [Fact]
    public void Compact_Returns_Previous_Array_To_Pool()
    {
        var storage = new Storage<CompactProbe>();
        storage.Append(default, 100);
        storage.Delete(0, 100);
        var previous = BackingArray(storage);

        storage.Compact();

        Assert.NotSame(previous, BackingArray(storage));
        Assert.Same(previous, PoolOf<CompactProbe>().Rent(previous.Length));
    }


    [Fact]
    public void Compact_At_Target_Size_Keeps_Backing_Array()
    {
        var storage = new Storage<NoOpProbe>();
        var backing = BackingArray(storage);

        storage.Compact();

        Assert.Same(backing, BackingArray(storage));
    }


    [Fact]
    public void Clear_Releases_Contained_References()
    {
        var storage = new Storage<MixedReferenceProbe>();
        storage.Append(new() { Reference = new(), Value = 42 });

        storage.Clear();

        Assert.Null(storage[0].Reference);
        Assert.Equal(0, storage[0].Value);
    }


    [Fact]
    public void Delete_Releases_Removed_References()
    {
        var storage = new Storage<ReferenceProbe>();
        storage.Append(new(), 3);

        storage.Delete(1);

        Assert.Equal(2, storage.Count);
        Assert.Null(storage[2]);
    }


    [Fact]
    public void Migrate_Releases_Source_References()
    {
        var source = new Storage<ReferenceProbe>();
        var destination = new Storage<ReferenceProbe>();
        source.Append(new());

        source.Migrate(destination);

        Assert.Equal(0, source.Count);
        Assert.Null(source[0]);
        Assert.Equal(1, destination.Count);
        Assert.NotNull(destination[0]);
    }


    [Fact]
    public void EnsureCapacity_Clears_Returned_Reference_Array()
    {
        var storage = new Storage<ReferenceProbe>();
        storage.Append(new());
        var previous = BackingArray(storage);

        storage.EnsureCapacity(previous.Length * 2);

        Assert.Null(previous[0]);
        Assert.NotNull(storage[0]);
        Assert.Same(previous, PoolOf<ReferenceProbe>().Rent(previous.Length));
    }


    [Fact]
    public void Compact_Clears_Returned_Reference_Array()
    {
        var storage = new Storage<CompactReferenceProbe>();
        storage.Append(new(), 100);
        storage.Delete(10, 90);
        var previous = BackingArray(storage);

        storage.Compact();

        Assert.All(previous, Assert.Null);
        Assert.NotNull(storage[0]);
        Assert.Same(previous, PoolOf<CompactReferenceProbe>().Rent(previous.Length));
    }
}
