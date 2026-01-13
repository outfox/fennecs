// SPDX-License-Identifier: MIT

using System.Runtime.CompilerServices;

namespace fennecs.tests;

public class EntityComponentsTests(ITestOutputHelper output)
{
    [Fact]
    public void CanGetComponents()
    {
        using var world = new World();
        var entity = world.Spawn();
        entity.Add(123);
        entity.Add(69.420);
        entity.Add(new TypeA());
        entity.Add(Link.With("hello"));
        
        var components = entity.Components;
        Assert.Equal(4, components.Count);

        List<IStrongBox> expected  = [new StrongBox<int>(123), new StrongBox<double>(69.420), new StrongBox<TypeA>(new()), new StrongBox<string>("hello")];
        foreach (var component in components)
        {
            var found = expected.Aggregate(false, (current, box) => current | box.Value!.Equals(component.Box.Value));
            Assert.True(found, $"Component {component.Type} = {component.Box.Value} not found in expected list.");
        }
    }

    [Fact]
    public void ComponentToString()
    {
        using var world = new World();
        var entity = world.Spawn();
        entity.Add(123);
        entity.Add('c', entity);
        entity.Add(69.420);
        entity.Add(new TypeA());
        entity.Add(Link.With("hello"));

        foreach (var component in entity.Components)
        {
            output.WriteLine(component.ToString());
        }
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(1337)]
    [InlineData(41697)]
    public void GottenComponentsCanBeRelations(int seed)
    {
        var random = new Random(seed);
        
        using var world = new World();
        var entity = world.Spawn();
        var other = world.Spawn();
        
        entity.Add(123, other);
        var literal = "hello" + random.Next();
        entity.Add(Link.With(literal));
        
        var components = entity.Components;
        Assert.Equal(2, components.Count);
        
        Assert.True(components[0].isRelation);
        Assert.False(components[1].isRelation);
        Assert.True(components[1].Box.Value is string);
        Assert.Equal(literal, components[1].Box.Value);
        Assert.True(components[1].isLink);
    }

    [Fact]
    public void GottenComponentRelationsHaveCorrectEntity()
    {
        using var world = new World();
        var entity = world.Spawn();
        var other = world.Spawn();
        
        entity.Add(123, other);
        
        var components = entity.Components;
        Assert.Single(components);
        Assert.True(components[0].isRelation);
        Assert.Equal(other, components[0].targetEntity);
    }

    [Fact]
    public void CannotGetEntityFromNonRelation()
    {
        using var world = new World();
        var entity = world.Spawn();
        entity.Add(123);
        
        var components = entity.Components;
        Assert.Single(components);
        Assert.False(components[0].isRelation);
        Assert.Throws<InvalidOperationException>(() => components[0].targetEntity);
    }

    private struct TypeA;
}
