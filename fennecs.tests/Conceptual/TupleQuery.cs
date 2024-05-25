namespace fennecs.tests.Conceptual;

internal class TupleQuery<TComponents> where TComponents : struct
{
    private const int Count = 100;
    public void For(Action<TComponents> action)
    {
        // Dummy Implementation, pretend we're getting the actual
        // components from the Archetypes here.
        TComponents components = default;
        for (var i = 0; i < Count; i++) action(components);
    }
}

internal class TupleTest : TupleQuery<(int, float)>;