namespace fennecs.tests.Conceptual;

public class RefStructUniforTests
{
    [Fact]
    public void Can_Use_RefStruct_Uniform()
    {
        using var world = new World();
        var entity = world.Spawn();
        entity.Add(123);

        var stream = world.Query<int>().Stream();

        Span<short> uniform = [1, 2, 3];

        stream.For(uniform, static (u, i) =>
        {
            i.write = u[0]; 
            Assert.Equal(1, i);
            i.write = u[1]; 
            Assert.Equal(2, i);
            i.write = u[2]; 
            Assert.Equal(3, i);
        });
    }
}