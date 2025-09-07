namespace fennecs.tests.Integration;

public record struct Position(float X, float Y);         // structs, records, and classes
public record struct Velocity(float Dx, float Dy); //equally work here

public sealed class ArchExample 
{
    [Fact]
    public void CanIterate() 
    {     
        // Create a world and entities with position and velocity.
        var world = new World();
        world.Entity()
            .Add(default(Position))
            .Add(new Velocity(1,1))
            .Spawn(1000); //can also world.Spawn().Add<...> 1000 times

        
        // Query and modify entities 
        var stream = world.Stream<Position, Velocity>(); // shorthand for world.Query<Pos, Vel>().Stream();
        stream.For((ref pos, ref vel) => {
            pos.X += vel.Dx;
            pos.Y += vel.Dy;
        });
    }
}
