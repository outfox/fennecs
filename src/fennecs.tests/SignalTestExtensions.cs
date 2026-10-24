namespace fennecs.tests;

internal static class SignalTestExtensions
{
    internal static void AddedEach<T>(this Signal<T> signal, Action<Entity, T> each) where T : notnull
    {
        signal.AfterAdded += (entities, values) =>
        {
            for (var i = 0; i < entities.Length; i++) each(entities[i], values[i]);
        };
    }

    internal static void RemovedEach<T>(this Signal<T> signal, Action<Entity, T, RemoveCause> each) where T : notnull
    {
        signal.BeforeRemoved += (entities, values, cause) =>
        {
            for (var i = 0; i < entities.Length; i++) each(entities[i], values[i], cause);
        };
    }
}
