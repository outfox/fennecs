namespace fennecs;

internal class Work<C1> : IThreadPoolWorkItem
{
    public Memory<C1> Memory1 = null!;
    public ComponentAction<C1> Action = null!;
    public CountdownEvent CountDown = null!;


    public void Execute()
    {
        foreach (ref var c in Memory1.Span) Action(ref c);
        CountDown.Signal();
    }
}

internal class UniformWork<U, C1> : IThreadPoolWorkItem
{
    public Memory<C1> Memory1 = null!;
    public UniformComponentAction<U, C1> Action = null!;
    public CountdownEvent CountDown = null!;
    public U Uniform = default!;


    public void Execute()
    {
        foreach (ref var c in Memory1.Span) Action(Uniform, ref c);
        CountDown.Signal();
    }
}

internal class Work<C1, C2> : IThreadPoolWorkItem
{
    public Memory<C1> Memory1 = null!;
    public Memory<C2> Memory2 = null!;
    public FilterDelegate<C1,C2> Pass = null!;
    public ComponentAction<C1, C2> Action = null!;
    public CountdownEvent CountDown = null!;


    public void Execute()
    {
        var s1 = Memory1.Span;
        var s2 = Memory2.Span;
        
        for (var i = 0; i < Memory1.Length; i++)
        {
            if (!Pass(in s1[i], in s2[i])) continue;
            Action(ref s1[i], ref s2[i]);
        }
        CountDown.Signal();
    }
}

internal class UniformWork<U, C1, C2> : IThreadPoolWorkItem
{
    public Memory<C1> Memory1 = null!;
    public Memory<C2> Memory2 = null!;
    public UniformComponentAction<U, C1, C2> Action = null!;
    public CountdownEvent CountDown = null!;
    public U Uniform = default!;
    public FilterDelegate<C1, C2> Pass = null!;


    public void Execute()
    {
        for (var i = 0; i < Memory1.Length; i++)
        {
            var s1 = Memory1.Span;
            var s2 = Memory2.Span;
            if (!Pass(in s1[i], in s2[i])) continue;
            Action(Uniform, ref s1[i], ref s2[i]);
        }
        CountDown.Signal();
    }
}

internal class Work<C1, C2, C3> : IThreadPoolWorkItem
{
    public Memory<C1> Memory1 = null!;
    public Memory<C2> Memory2 = null!;
    public Memory<C3> Memory3 = null!;
    public ComponentAction<C1, C2, C3> Action = null!;
    public CountdownEvent CountDown = null!;


    public void Execute()
    {
        for (var i = 0; i < Memory1.Length; i++) Action(ref Memory1.Span[i], ref Memory2.Span[i], ref Memory3.Span[i]);
        CountDown.Signal();
    }
}

internal class UniformWork<U, C1, C2, C3> : IThreadPoolWorkItem
{
    public Memory<C1> Memory1 = null!;
    public Memory<C2> Memory2 = null!;
    public Memory<C3> Memory3 = null!;
    public UniformComponentAction<U, C1, C2, C3> Action = null!;
    public CountdownEvent CountDown = null!;
    public U Uniform = default!;


    public void Execute()
    {
        for (var i = 0; i < Memory1.Length; i++) Action(Uniform, ref Memory1.Span[i], ref Memory2.Span[i], ref Memory3.Span[i]);
        CountDown.Signal();
    }
}

internal class Work<C1, C2, C3, C4> : IThreadPoolWorkItem
{
    public Memory<C1> Memory1 = null!;
    public Memory<C2> Memory2 = null!;
    public Memory<C3> Memory3 = null!;
    public Memory<C4> Memory4 = null!;
    public ComponentAction<C1, C2, C3, C4> Action = null!;
    public CountdownEvent CountDown = null!;


    public void Execute()
    {
        for (var i = 0; i < Memory1.Length; i++) Action(ref Memory1.Span[i], ref Memory2.Span[i], ref Memory3.Span[i], ref Memory4.Span[i]);
        CountDown.Signal();
    }
}

internal class UniformWork<U, C1, C2, C3, C4> : IThreadPoolWorkItem
{
    public Memory<C1> Memory1 = null!;
    public Memory<C2> Memory2 = null!;
    public Memory<C3> Memory3 = null!;
    public Memory<C4> Memory4 = null!;
    public UniformComponentAction<U, C1, C2, C3, C4> Action = null!;
    public CountdownEvent CountDown = null!;
    public U Uniform = default!;


    public void Execute()
    {
        for (var i = 0; i < Memory1.Length; i++) Action(Uniform, ref Memory1.Span[i], ref Memory2.Span[i], ref Memory3.Span[i], ref Memory4.Span[i]);
        CountDown.Signal();
    }
}

internal class Work<C1, C2, C3, C4, C5> : IThreadPoolWorkItem
{
    public Memory<C1> Memory1 = null!;
    public Memory<C2> Memory2 = null!;
    public Memory<C3> Memory3 = null!;
    public Memory<C4> Memory4 = null!;
    public Memory<C5> Memory5 = null!;
    public ComponentAction<C1, C2, C3, C4, C5> Action = null!;
    public CountdownEvent CountDown = null!;


    public void Execute()
    {
        for (var i = 0; i < Memory1.Length; i++) Action(ref Memory1.Span[i], ref Memory2.Span[i], ref Memory3.Span[i], ref Memory4.Span[i], ref Memory5.Span[i]);
        CountDown.Signal();
    }
}

internal class UniformWork<U, C1, C2, C3, C4, C5> : IThreadPoolWorkItem
{
    public Memory<C1> Memory1 = null!;
    public Memory<C2> Memory2 = null!;
    public Memory<C3> Memory3 = null!;
    public Memory<C4> Memory4 = null!;
    public Memory<C5> Memory5 = null!;
    public UniformComponentAction<U, C1, C2, C3, C4, C5> Action = null!;
    public CountdownEvent CountDown = null!;
    public U Uniform = default!;


    public void Execute()
    {
        for (var i = 0; i < Memory1.Length; i++) Action(Uniform, ref Memory1.Span[i], ref Memory2.Span[i], ref Memory3.Span[i], ref Memory4.Span[i], ref Memory5.Span[i]);
        CountDown.Signal();
    }
}