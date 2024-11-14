using fennecs.storage;
// ReSharper disable InconsistentNaming
#pragma warning disable CS0414 // Field is assigned but its value is never used

namespace fennecs.jobs;


internal record JobR<C0> : IThreadPoolWorkItem where C0 : notnull
{
    public required ReadOnlyMemory<Identity> MemoryE = null!;
    public required World World = null!;
    
    public required ReadOnlyMemory<C0> Memory0 = null!;
    public required TypeExpression Type0 = default;
    
    public required Action<R<C0>> Action = null!;
    public required CountdownEvent CountDown = null!;
    public void Execute() 
    {
        var identities = MemoryE.Span;
        var span0 = Memory0.Span;

        var count = identities.Length;
        for (var i = 0; i < count; i++)
        {
            var entity = new Entity(World, identities[i]);
            Action(new R<C0>(in span0[i]));
        }
        CountDown.Signal();
    }
}

internal record JobW<C0> : IThreadPoolWorkItem where C0 : notnull
{
    public required ReadOnlyMemory<Identity> MemoryE = null!;
    public required World World = null!;
    
    public required Memory<C0> Memory0 = null!;
    public required TypeExpression Type0 = default;
    
    public required Action<RW<C0>> Action = null!;
    public required CountdownEvent CountDown = null!;
    public void Execute() 
    {
        var identities = MemoryE.Span;
        var span0 = Memory0.Span;
        
        var count = identities.Length;
        for (var i = 0; i < count; i++)
        {
            var entity = new Entity(World, identities[i]);
            Action(new RW<C0>(ref span0[i], in entity, in Type0));
        }

        CountDown.Signal();
    }
}

internal record JobUW<U, C0> : IThreadPoolWorkItem where C0 : notnull
{
    public required ReadOnlyMemory<Identity> MemoryE = null!;
    public required World World = null!;
    
    public required Memory<C0> Memory0 = null!;
    public required TypeExpression Type0 = default;
    
    public required U Uniform = default!;
    public required Action<U, RW<C0>> Action = null!;
    public required CountdownEvent CountDown = null!;
    public void Execute() 
    {
        var identities = MemoryE.Span;
        var span0 = Memory0.Span;
        
        var count = identities.Length;
        for (var i = 0; i < count; i++)
        {
            var entity = new Entity(World, identities[i]);
            Action(Uniform, new RW<C0>(ref span0[i], in entity, in Type0));
        }

        CountDown.Signal();
    }
}

internal record JobEW<C0> : IThreadPoolWorkItem where C0 : notnull
{
    public required ReadOnlyMemory<Identity> MemoryE = null!;
    public required World World = null!;
    
    public required Memory<C0> Memory0 = null!;
    public required TypeExpression Type0 = default;
    
    public required Action<EntityRef, RW<C0>> Action = null!;
    public required CountdownEvent CountDown = null!;
    public void Execute() 
    {
        var identities = MemoryE.Span;
        var span0 = Memory0.Span;

        var count = identities.Length;
        for (var i = 0; i < count; i++)
        {
            var entity = new Entity(World, identities[i]);
            Action(new EntityRef(in entity), new RW<C0>(ref span0[i], in entity, in Type0));
        }

        CountDown.Signal();
    }
}

internal record JobEUW<U, C0> : IThreadPoolWorkItem where C0 : notnull
{
    public required ReadOnlyMemory<Identity> MemoryE = null!;
    public required World World = null!;
    
    public required Memory<C0> Memory0 = null!;
    public required TypeExpression Type0 = default;

    public required U Uniform = default!;
    public required Action<EntityRef, U, RW<C0>> Action = null!;
    public required CountdownEvent CountDown = null!;
    public void Execute() 
    {
        var identities = MemoryE.Span;
        var span0 = Memory0.Span;

        var count = identities.Length;
        for (var i = 0; i < count; i++)
        {
            var entity = new Entity(World, identities[i]);
            Action(new EntityRef(in entity), Uniform, new RW<C0>(ref span0[i], in entity, in Type0));
        }

        CountDown.Signal();
    }
}

#pragma warning restore CS0414 // Field is assigned but its value is never used
