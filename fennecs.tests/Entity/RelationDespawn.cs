namespace fennecs.tests;

public class RelationDespawn
{
    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(10)]
    [InlineData(100)]
    [InlineData(345)]
    public void DespawnRelationTargetRemovesComponent(int relations)
    {
        using var world = new World();
        
        var subject = world.Spawn();
        
        world.Entity()
            .Add<int>(default, subject)
            .Add(Link.With("relation target"))
            .Spawn(relations)
            .Dispose();
        
        var targets = new List<Entity>(world.Query<int>(subject).Compile());

        var rnd = new Random(1234 + relations);
        foreach (var target in targets)
        {
            subject.Add<int>(Relate.To(target));
        }

        while (targets.Count > 0)
        {
            var target = targets[rnd.Next(targets.Count)];

            Assert.True(subject.Has<int>(target));
            
            target.Despawn();
            targets.Remove(target);

            Assert.False(subject.Has<int>(target));
        }
    }
}
