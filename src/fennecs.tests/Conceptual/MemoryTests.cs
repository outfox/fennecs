namespace fennecs.tests.Conceptual;

public class MemoryTests
{

    [Fact]
    private void Memory_Write_via_Span()
    {
        var array = new int[10];
        var memory = new Memory<int>(array);
        var span = memory.Span;
        span[0] = 123;
        Assert.Equal(123, array[0]);
    }

    [Fact]
    private void Memory_Write_via_Array()
    {
        var array = new int[10];
        var memory = new Memory<int>(array);
        var span = memory.Span;
        array[0] = 123;
        Assert.Equal(123, span[0]);
    }


    [Fact]
    private void Memory_ephemerally_intact_after_Array_Reseat()
    {
        var array = new int[10];
        var memory = new Memory<int>(array);
        array[0] = 123;

        Array.Resize(ref array, 20);

        Assert.Equal(123, memory.Span[0]);
        Assert.Equal(123, array[0]);

        // The span now still points to the original array!
        memory.Span[0] = 345;
        Assert.NotEqual(345, array[0]);
        Assert.Equal(123, array[0]);
    }


    [Fact]
    private unsafe void Memory_handle_differs_after_Array_Reseat()
    {
        var array = new int[10];
        array[0] = 123;

        var memory1 = new Memory<int>(array);
        var handle = memory1.Pin();
        var previousPointer = handle.Pointer;
        handle.Dispose();

        Array.Resize(ref array, 20);

        var memory2 = new Memory<int>(array);
        var afterHandle = memory2.Pin();
        var afterPointer = afterHandle.Pointer;
        afterHandle.Dispose();

        array[11] = 345;

        Assert.True(previousPointer != afterPointer);

        Assert.Equal(123, memory2.Span[0]);
        Assert.Equal(345, memory2.Span[11]);
        Assert.Equal(123, array[0]);
    }

    [Fact]
    private unsafe void Memory_Pin_allows_Array_Reseat()
    {
        var array = new int[10];
        array[0] = 123;

        var memory1 = new Memory<int>(array);
        var beforeHandle = memory1.Pin();
        var beforePointer = beforeHandle.Pointer;

        Array.Resize(ref array, 20);

        var memory2 = new Memory<int>(array);
        var afterHandle = memory2.Pin();
        var afterPointer = afterHandle.Pointer;
        array[11] = 345;

        Assert.True(beforePointer != afterPointer);

        Assert.Equal(123, memory2.Span[0]);
        Assert.Equal(345, memory2.Span[11]);
        Assert.Equal(123, array[0]);

        afterHandle.Dispose();
        beforeHandle.Dispose();
    }
}