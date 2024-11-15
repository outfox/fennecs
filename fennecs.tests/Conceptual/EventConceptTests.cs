namespace fennecs.tests.Conceptual;

public class EventConceptTests
{
    public EventConceptTests(ITestOutputHelper output)
    {
        _output = output;
    }

    private static ITestOutputHelper _output = null!;

    internal interface IAdded<in T>
    {
        static void Added(Entity entity, T value)
        {
            _output.WriteLine($"Added {value}");
        }
    }

    private interface IValue<T>
    {
        T Value { get; set; }
    }

    private interface IModified<in C>
    {
        static void Modified(C value)
        {
            _output.WriteLine($"Modified {value}");
        }
    }

    public interface IRemoved<in T>
    {
        static void Removed(Entity entity, T value)
        {
            _output.WriteLine($"Removed {value}");
        }
    }

    private record struct TestComponent(int Value) :
        IValue<int>,
        IAdded<TestComponent>,
        IModified<TestComponent>,
        IRemoved<TestComponent>
    {
        private int _value = Value;

        public int Value
        {
            readonly get => _value;
            set
            {
                _value = value;
                IModified<TestComponent>.Modified(this);
            }
        }
    }

    [Fact]
    public void Test()
    {
        using var world = new World();
        var entity = world.Spawn();

        var component = new TestComponent(1);
        entity.TestAdd(component);

        component.Value = 2;
        component.Value = 3;
        component.Value = 4;

        entity.TestRemove<TestComponent>();
    }
}

public static class EntityExtensions
{
    public static void TestAdd<T>(this Entity entity, T component) where T : notnull
    {
        entity.Add(component);
        if (component is EventConceptTests.IAdded<T>)
        {
            EventConceptTests.IAdded<T>.Added(entity, component);
        }
    }

    public static void TestRemove<T>(this Entity entity) where T : EventConceptTests.IRemoved<T>
    {
        entity.Remove<T>();
        if (typeof(T).IsAssignableTo(typeof(EventConceptTests.IRemoved<T>)))
        {
            EventConceptTests.IRemoved<T>.Removed(entity, default!);
        }
    }
}