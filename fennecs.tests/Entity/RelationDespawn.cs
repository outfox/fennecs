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
            subject.Add(rnd.Next(), target);
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
    public void DespawningBulkInSelfReferencedArchetypeIsPossible(int relations)
    {
        using var world = new World();
        
        var subjects = new List<Entity>();
        var rnd = new Random(1234 + relations);
        
        // Spawn the other Entities
        for (var i = 0; i < relations; i++)
        {
            subjects.Add(world.Spawn());
        }

        // Create a bunch of self-referential relations
        foreach (var self in subjects)
        {
            self.Add(rnd.Next(), self);
        }

        var query = world.Query<int>(Match.Entity).Compile();
        Assert.Equal(relations, query.Count);
        
        query.Truncate(relations/2);

        Assert.Empty(query);
    }
   
    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(10)]
    [InlineData(69)]
    [InlineData(200)]
    public void DespawningSingleInSelfReferencedArchetypeIsPossible(int relations)
    {
        using var world = new World();
        
        var subjects = new List<Entity>();
        var rnd = new Random(1234 + relations);
        
        for (var i = 0; i < relations; i++)
        {
            subjects.Add(world.Spawn());
        }

        // Add single "survivor" relation
        var survivor = world.Spawn();
        survivor.Add(rnd.Next(), world.Spawn());
        
        // Create a bunch of self-referential relations
        foreach (var self in subjects)
        {
            self.Add(rnd.Next(), self);
        }

        var query = world.Query<int>(Match.Entity).Compile();
        Assert.Equal(relations+1, query.Count);
        
        // Create a bunch of self-referential relations
        foreach (var subject in subjects)
        {
            subject.Despawn();
        }
        
        Assert.Single(query);
    }
   
    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(10)]
    [InlineData(69)]
    [InlineData(200)]
    public void DespawningSingleInSelfReferencedArchetypeIsPossibleWithOtherRelations(int relations)
    {
        using var world = new World();
        
        var subjects = new List<Entity>();
        var rnd = new Random(1234 + relations);
        
        for (var i = 0; i < relations * 2; i++)
        {
            subjects.Add(world.Spawn());
        }

        // Create a bunch of self-referential relations
        for (var i = 0; i < relations; i++)
        {
            subjects[i].Add(rnd.Next(), subjects[i]);
        }

        // Create a bunch of normal relations
        foreach (var self in subjects)
        {
            self.Add(rnd.Next(), subjects[relations + rnd.Next(relations/2)]);
        }

        var query = world.Query<int>(Match.Entity).Compile();
        Assert.Equal(relations*2, query.Count);
        
        // Create a bunch of self-referential relations
        foreach (var subject in subjects)
        {
            subject.Despawn();
        }
        
        Assert.Empty(query);
    }
}
