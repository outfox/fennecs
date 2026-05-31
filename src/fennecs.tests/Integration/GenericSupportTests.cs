namespace fennecs.tests.Integration;

public class GenericSupportTests
{
    private record struct GenericStruct<T>(T value);

    [Fact]
    public void SupportsGenericStructs()
    {
        using var world = new World();

        var entity = world.Spawn().Add(new GenericStruct<int>(42));

        Assert.Equal(42, entity.Ref<GenericStruct<int>>(Match.Plain).value);

        var components = entity.Get<GenericStruct<int>>(Match.Any);
        Assert.Single(components);
        Assert.Equal(42, components.Single().value);
    }
}
