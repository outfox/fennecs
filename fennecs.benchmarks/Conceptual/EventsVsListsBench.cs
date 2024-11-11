using BenchmarkDotNet.Attributes;

namespace Benchmark.Conceptual;

public class EventsVsListsBench
{
    [Params(100)]
    public int Count { get; set; }
    
    private List<Action<int>> _listeners = null!;
    private event Action<int>? Event = null!;

    [GlobalSetup]
    public void Setup()
    {
        _listeners = new(1_000_000)
        {
            OnEvent,
        };
        Event += OnEvent;
    }

    private int _sum;
    private Random _rnd = null!;
    private void OnEvent(int number)
    {
        _sum += number;
    }

    [IterationSetup]
    public void SetupEvents()
    {
        _sum = 0;
        _rnd = new();
        
        _listeners.Clear();
        Event = null!;

        for (var i = 0; i < Count; i++)
        {
            _listeners.Add(OnEvent);
            Event += OnEvent;
        }
    }


    [Benchmark]
    public void AddListeners()
    {
        for (var i = 0; i < 10_000; i++)
        {
            _listeners.Add(OnEvent);
        }

        for (var i = 0; i < 10_000; i++)
        {
            _listeners.Remove(OnEvent);
        }
    }

    [Benchmark]
    public void AddEvents()
    {
        for (var i = 0; i < 10_000; i++)
        {
            Event += OnEvent;
        }

        for (var i = 0; i < 10_000; i++)
        {
            Event -= OnEvent;
        }
    }

    [Benchmark(Baseline = true)]
    public int InvokeEvents()
    {
        for (var i = 0; i < 1_000_000; i++)
        {
            var number = _rnd.Next();
            Event?.Invoke(number);
        }
        return _sum;
    }

    [Benchmark]
    public int InvokeListeners()
    {
        for (var i = 0; i < 1_000_000; i++)
        {
            var number = _rnd.Next();
            foreach (var action in _listeners) action(number);
        }
        return _sum;
    }
}
