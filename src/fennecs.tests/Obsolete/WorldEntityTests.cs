// SPDX-License-Identifier: MIT

namespace fennecs.tests.Obsolete;

// Tests for deprecated APIs live in this category until the APIs are retired.
// Each test suppresses the obsolescence warning locally, on purpose.
public class WorldEntityTests
{
    [Fact]
    public void World_Entity_Still_Returns_Working_Template()
    {
#pragma warning disable CS0618 // World.Entity() is obsolete, renamed to World.Template()
        using var world = new World();
        using var template = world.Entity().Add(123);
#pragma warning restore CS0618

        var entity = template.Spawn();
        Assert.True(entity.Alive);
        Assert.Equal(123, entity.Ref<int>());

        template.Spawn(9);
        Assert.Equal(10, world.Count);
    }
}
