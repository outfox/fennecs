namespace fennecs.tests;

public class EntityBuilderTests
{
    [Fact(Skip = "Need to clarify if needed, desired, or invalid.")]
    public void Cannot_Relate_To_Any()
    {
        using var world = new World();
        var entity = world.Spawn().Id();
        var builder = new EntityBuilder(world, entity);
        Assert.Throws<InvalidOperationException>(() => { builder.Link<int>(Entity.Any); });
    }

    [Fact(Skip = "Need to clarify if needed, desired, or invalid.")]
    public void Cannot_Relate_To_Any_with_Data()
    {
        using var world = new World();
        var entity = world.Spawn().Id();
        var builder = new EntityBuilder(world, entity);
        Assert.Throws<InvalidOperationException>(() => { builder.Link(Entity.Any, 123); });
    }
}