namespace fennecs.tests.Conceptual;

public class RefStructUniformTests
{
    [Fact]
    public void For_allows_RefStruct_Uniform()
    {
        using var world = new World();
        var entity = world.Spawn();
        entity.Add(123);

        var stream = world.Query<int>().Stream();

        Span<short> uniform = [1, 2, 3];

        stream.For(uniform, static ([RW] u, i) =>
        {
            i.write = u[0];
            Assert.Equal(1, i);
            i.write = u[1];
            Assert.Equal(2, i);
            i.write = u[2];
            Assert.Equal(3, i);
        });
    }

    [Fact]
    public void Raw_allows_RefStruct_Uniform()
    {
        using var world = new World();
        var entity = world.Spawn();
        entity.Add(123);

        var stream = world.Query<int>().Stream();

        Span<short> uniform = [1, 2, 3];

        stream.Raw(uniform, static (Span<short> u, Span<int> i) =>
        {
            i[0] = u[0];
            Assert.Equal(1, i[0]);
            i[0] = u[1];
            Assert.Equal(2, i[0]);
            i[0] = u[2];
            Assert.Equal(3, i[0]);
        });
    }
    [Fact]
    public void Mem_allows_RefStruct_Uniform()
    {
        using var world = new World();
        var entity = world.Spawn();
        entity.Add(123);

        var stream = world.Query<int>().Stream();

        Span<short> uniform = [1, 2, 3];

        stream.Mem(uniform, static (u, i) =>
        {
            i.write[0] = u[0];
            Assert.Equal(1, i.read[0]);
            i.write[0] = u[1];
            Assert.Equal(2, i.read[0]);
            i.write[0] = u[2];
            Assert.Equal(3, i.read[0]);
        });
    }
}