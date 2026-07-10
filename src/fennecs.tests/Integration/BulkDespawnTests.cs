namespace fennecs.tests.Integration;

public class BulkDespawnTests
{
    [Fact]
    public void CanDespawnBulk()
    {
        using var world = new World();

        world.Spawn().Add('a');
        world.Spawn().Add('a').Add("fennecs");
        world.Spawn().Add(1.0f).Add("fennecs");
        world.Spawn().Add('b').Add("fennecs");
        world.Spawn().Add(1).Add('c').Add("fennecs");

        world.Query<string>(Match.Plain).Compile().Batch(Batch.RemoveConflict.Allow).Remove<char>().Submit();
        world.Query<int>(Match.Plain).Stream().Despawn();
    }
}
