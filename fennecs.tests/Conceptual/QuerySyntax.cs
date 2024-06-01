#if EXPERIMENTAL
namespace fennecs.tests.Conceptual;
public class QuerySyntax
{
    private void Syntax()
    {
        /*
         Cute idea, Copilot. :P
        // Query syntax
        var querySyntax = from e in entities
                          where e.Has<Position>() && e.Has<Velocity>()
                          select e;

        // Method syntax
        var methodSyntax = entities.Where(e => e.Has<Position>() && e.Has<Velocity>());
        */
        
        using var world = new World();
        using var spawner = world.Spawn();

        var stream = world.Stream<string, Position>();
        
        var query = world.Query()
            .Has<string, Position, Vector3>(Match.Plain)
            .Has<int>()
            .Compile(); // yields a query

        var stream = world.Query<int, float, int>()
            .Has<string, Position, Vector3>(Match.Plain)
            .Has<int>()
            .Stream(); // compiles Query and yields a Stream
    }
}
#endif