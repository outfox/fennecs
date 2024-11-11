namespace fennecs.tests.Conceptual;

using fennecs.storage;

public class ReflectionTests
{
    [Fact]
    public void MakeGenericType_is_Not_Unique()
    {
        var t1 = typeof(Storage<>).MakeGenericType(typeof(int));
        var t2 = typeof(Storage<>).MakeGenericType(typeof(int));
        Assert.Equal(t1, t2);
    }

    [Fact]
    public void MakeGenericType_is_Same_As_Static()
    {
        var t1 = typeof(Storage<>).MakeGenericType(typeof(int));
        var t2 = typeof(Storage<int>);
        Assert.Equal(t1, t2);
    }
}
