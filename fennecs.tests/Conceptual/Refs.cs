namespace fennecs.tests.Conceptual;

public class Refs
{
    record struct Owes(decimal Amount);

    [Fact]
    public void CanOweAmount()
    {
        using var world = new World();

        var alice = world.Spawn();
        var eve = world.Spawn();
        var bob = world.Spawn();
        
        bob.Add<Owes>(new(10M), alice);  // bob owes alice $10 (Relation Owes->alice)
        bob.Add<Owes>(new(23M), eve);    // and he owes eve $23 (Relation Owes->eve)

        Assert.True(bob.Has<Owes>(Match.Entity));
        Assert.True(bob.Has<Owes>(alice));
        Assert.True(bob.Has<Owes>(eve));

        Assert.Equal(23M, bob.Get<Owes>(eve).Amount);

        if (!bob.Has<Owes>(eve))
        {
            bob.Add<Owes>(new(7M), eve);
        }
        else
        {
            bob.Write<Owes>(eve).Amount += 7M;
        }
        
        Assert.Equal(30M, bob.Read<Owes>(eve).Amount);
    }
}
