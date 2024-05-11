namespace fennecs;

internal class Work<C1> : IThreadPoolWorkItem
{
    public Memory<C1> Memory1 = null!;
    public RefAction<C1> Action = null!;
    public CountdownEvent CountDown = null!;


    public void Execute()
    {
        foreach (ref var c in Memory1.Span) Action(ref c);
        CountDown.Signal();
    }
}


internal class UniformWork<C1, U> : IThreadPoolWorkItem
{
    public Memory<C1> Memory1 = null!;
    public RefActionU<C1, U> Action = null!;
    public CountdownEvent CountDown = null!;
    public U Uniform = default!;


    public void Execute()
    {
        foreach (ref var c in Memory1.Span) Action(ref c, Uniform);
        CountDown.Signal();
    }
}


internal class Work<C1, C2> : IThreadPoolWorkItem
{
    public Memory<C1> Memory1 = null!;
    public Memory<C2> Memory2 = null!;
    public RefAction<C1, C2> Action = null!;
    public CountdownEvent CountDown = null!;


    public void Execute()
    {
        for (var i = 0; i < Memory1.Length; i++) Action(ref Memory1.Span[i], ref Memory2.Span[i]);
        CountDown.Signal();
    }
}


internal class UniformWork<C1, C2, U> : IThreadPoolWorkItem
{
    public Memory<C1> Memory1 = null!;
    public Memory<C2> Memory2 = null!;
    public RefActionU<C1, C2, U> Action = null!;
    public CountdownEvent CountDown = null!;
    public U Uniform = default!;


    public void Execute()
    {
        for (var i = 0; i < Memory1.Length; i++) 
            Action(ref Memory1.Span[i], ref Memory2.Span[i], Uniform);
        CountDown.Signal();
    }
}


internal class Work<C1, C2, C3> : IThreadPoolWorkItem
{
    public Memory<C1> Memory1 = null!;
    public Memory<C2> Memory2 = null!;
    public Memory<C3> Memory3 = null!;
    public RefAction<C1, C2, C3> Action = null!;
    public CountdownEvent CountDown = null!;


    public void Execute()
    {
        for (var i = 0; i < Memory1.Length; i++) Action(ref Memory1.Span[i], ref Memory2.Span[i], ref Memory3.Span[i]);
        CountDown.Signal();
    }
}


internal class UniformWork<C1, C2, C3, U> : IThreadPoolWorkItem
{
    public Memory<C1> Memory1 = null!;
    public Memory<C2> Memory2 = null!;
    public Memory<C3> Memory3 = null!;
    public RefActionU<C1, C2, C3, U> Action = null!;
    public CountdownEvent CountDown = null!;
    public U Uniform = default!;


    public void Execute()
    {
        for (var i = 0; i < Memory1.Length; i++) Action(ref Memory1.Span[i], ref Memory2.Span[i], ref Memory3.Span[i], Uniform);
        CountDown.Signal();
    }
}


internal class Work<C1, C2, C3, C4> : IThreadPoolWorkItem
{
    public Memory<C1> Memory1 = null!;
    public Memory<C2> Memory2 = null!;
    public Memory<C3> Memory3 = null!;
    public Memory<C4> Memory4 = null!;
    public RefAction<C1, C2, C3, C4> Action = null!;
    public CountdownEvent CountDown = null!;


    public void Execute()
    {
        for (var i = 0; i < Memory1.Length; i++) Action(ref Memory1.Span[i], ref Memory2.Span[i], ref Memory3.Span[i], ref Memory4.Span[i]);
        CountDown.Signal();
    }
}


internal class UniformWork<C1, C2, C3, C4, U> : IThreadPoolWorkItem
{
    public Memory<C1> Memory1 = null!;
    public Memory<C2> Memory2 = null!;
    public Memory<C3> Memory3 = null!;
    public Memory<C4> Memory4 = null!;
    public RefActionU<C1, C2, C3, C4, U> Action = null!;
    public CountdownEvent CountDown = null!;
    public U Uniform = default!;


    public void Execute()
    {
        for (var i = 0; i < Memory1.Length; i++) Action(ref Memory1.Span[i], ref Memory2.Span[i], ref Memory3.Span[i], ref Memory4.Span[i], Uniform);
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
    public RefAction<C1, C2, C3, C4, C5> Action = null!;
    public CountdownEvent CountDown = null!;


    public void Execute()
    {
        for (var i = 0; i < Memory1.Length; i++) Action(ref Memory1.Span[i], ref Memory2.Span[i], ref Memory3.Span[i], ref Memory4.Span[i], ref Memory5.Span[i]);
        CountDown.Signal();
    }
}


internal class UniformWork<C1, C2, C3, C4, C5, U> : IThreadPoolWorkItem
{
    public Memory<C1> Memory1 = null!;
    public Memory<C2> Memory2 = null!;
    public Memory<C3> Memory3 = null!;
    public Memory<C4> Memory4 = null!;
    public Memory<C5> Memory5 = null!;
    public RefActionU<C1, C2, C3, C4, C5, U> Action = null!;
    public CountdownEvent CountDown = null!;
    public U Uniform = default!;


    public void Execute()
    {
        for (var i = 0; i < Memory1.Length; i++) Action(ref Memory1.Span[i], ref Memory2.Span[i], ref Memory3.Span[i], ref Memory4.Span[i], ref Memory5.Span[i], Uniform);
        CountDown.Signal();
    }
}