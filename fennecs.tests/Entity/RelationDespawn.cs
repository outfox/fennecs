using System.Security.Cryptography.X509Certificates;

namespace fennecs.tests;

public class RelationDespawn
{
    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(10)]
    [InlineData(69)]
    [InlineData(234)]
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
            subject.Add(rnd.Next(), Relate.To(target));
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

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(10)]
    [InlineData(69)]
    [InlineData(200)]
    public void DespawningInSelfReferencedArchetypeIsPossible(int relations)
    {
        using var world = new World();
        
        var subjects = new List<Entity>();
        var rnd = new Random(1234 + relations);
        
        for (var i = 0; i < relations; i++)
        {
            subjects.Add(world.Spawn());
        }

        // Create a bunch of self-referential relations
        foreach (var subject in subjects)
        {
            subject.Add(rnd.Next(), Relate.To(subject));
        }

        var query = world.Query<int>(Match.Entity).Compile();
        Assert.Equal(relations, query.Count);
        
        query.Truncate(relations/2);

        Assert.Equal(relations/2, query.Count);
    }
    
}
