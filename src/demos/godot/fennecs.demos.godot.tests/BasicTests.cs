using Godot;
using twodog.fixture;
using twodog.xunit;

namespace fennecs.demos.godot.Tests;

[Collection<GodotHeadlessCollection>]
public class BasicTests(GodotHeadlessFixture godot)
{
    [Fact]
    public void LoadMainScene_Succeeds()
    {
        // Arrange - load whatever main scene project.godot configures
        var mainScene = (string)ProjectSettings.GetSetting("application/run/main_scene", "");
        Assert.SkipWhen(mainScene == "", "No run/main_scene configured in project.godot");
        var scene = GD.Load<PackedScene>(mainScene);

        // Act
        var instance = scene.Instantiate();
        godot.Tree.Root.AddChild(instance);

        // Assert
        Assert.NotNull(instance);
        Assert.NotNull(instance.GetParent());
    }
    
    [Fact]
    public void PhysicsIteration_Succeeds()
    {
        // Arrange & Act
        godot.Tree.Root.PhysicsInterpolationMode = Node.PhysicsInterpolationModeEnum.Off;
        godot.GodotInstance.Iteration();

        // Assert - if we get here without crashing, test passes
        Assert.True(true);
    }

    [Fact]
    public void CreateNode_AddsToTree()
    {
        // Arrange
        var node = new Node();
        node.Name = "TestNode";

        // Act
        godot.Tree.Root.AddChild(node);

        // Assert
        Assert.True(godot.Tree.Root.HasNode("TestNode"));
        Assert.Equal("TestNode", (string)godot.Tree.Root.GetNode("TestNode").Name);

        // Cleanup
        node.QueueFree();
    }
}
