using fennecs.storage;

// ReSharper disable InconsistentNaming
#pragma warning disable CS0414 // Field is assigned but its value is never used

namespace fennecs;


internal record JobR<C0> : IThreadPoolWorkItem where C0 : notnull
{
    public ReadOnlyMemory<Identity> MemoryE = null!;
    public World World = null!;
    
    public ReadOnlyMemory<C0> Memory0 = null!;
    public TypeExpression Type0 = default;
    
    public Action<R<C0>> Action = null!;
    public CountdownEvent CountDown = null!;
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
    public ReadOnlyMemory<Identity> MemoryE = null!;
    public World World = null!;
    
    public Memory<C0> Memory0 = null!;
    public TypeExpression Type0 = default;
    
    public Action<RW<C0>> Action = null!;
    public CountdownEvent CountDown = null!;
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
    public ReadOnlyMemory<Identity> MemoryE = null!;
    public World World = null!;
    
    public Memory<C0> Memory0 = null!;
    public TypeExpression Type0 = default;
    
    public U Uniform = default!;
    public Action<U, RW<C0>> Action = null!;
    public CountdownEvent CountDown = null!;
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


internal record JobUR<U, C0> : IThreadPoolWorkItem where C0 : notnull
{
    public ReadOnlyMemory<Identity> MemoryE = null!;
    public World World = null!;
    
    public ReadOnlyMemory<C0> Memory0 = null!;
    public TypeExpression Type0 = default;
    
    public U Uniform = default!;
    public Action<U, R<C0>> Action = null!;
    public CountdownEvent CountDown = null!;
    public void Execute() 
    {
        var identities = MemoryE.Span;
        var span0 = Memory0.Span;
        
        var count = identities.Length;
        for (var i = 0; i < count; i++)
        {
            var entity = new Entity(World, identities[i]);
            Action(Uniform, new R<C0>(in span0[i]));
        }

        CountDown.Signal();
    }
}

internal record JobEW<C0> : IThreadPoolWorkItem where C0 : notnull
{
    public ReadOnlyMemory<Identity> MemoryE = null!;
    public World World = null!;
    
    public Memory<C0> Memory0 = null!;
    public TypeExpression Type0 = default;
    
    public Action<EntityRef, RW<C0>> Action = null!;
    public CountdownEvent CountDown = null!;
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

internal record JobER<C0> : IThreadPoolWorkItem where C0 : notnull
{
    public ReadOnlyMemory<Identity> MemoryE = null!;
    public World World = null!;
    
    public ReadOnlyMemory<C0> Memory0 = null!;
    public TypeExpression Type0 = default;
    
    public Action<EntityRef, R<C0>> Action = null!;
    public CountdownEvent CountDown = null!;
    public void Execute() 
    {
        var identities = MemoryE.Span;
        var span0 = Memory0.Span;

        var count = identities.Length;
        for (var i = 0; i < count; i++)
        {
            var entity = new Entity(World, identities[i]);
            Action(new EntityRef(in entity), new R<C0>(in span0[i]));
        }

        CountDown.Signal();
    }
}

internal record JobEUW<U, C0> : IThreadPoolWorkItem where C0 : notnull
{
    public ReadOnlyMemory<Identity> MemoryE = null!;
    public World World = null!;
    
    public Memory<C0> Memory0 = null!;
    public TypeExpression Type0 = default;

    public U Uniform = default!;
    public Action<EntityRef, U, RW<C0>> Action = null!;
    public CountdownEvent CountDown = null!;
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

internal record JobEUR<U, C0> : IThreadPoolWorkItem where C0 : notnull
{
    public ReadOnlyMemory<Identity> MemoryE = null!;
    public World World = null!;
    
    public ReadOnlyMemory<C0> Memory0 = null!;
    public TypeExpression Type0 = default;

    public U Uniform = default!;
    public Action<EntityRef, U, R<C0>> Action = null!;
    public CountdownEvent CountDown = null!;
    public void Execute() 
    {
        var identities = MemoryE.Span;
        var span0 = Memory0.Span;

        var count = identities.Length;
        for (var i = 0; i < count; i++)
        {
            var entity = new Entity(World, identities[i]);
            Action(new EntityRef(in entity), Uniform, new R<C0>(in span0[i]));
        }

        CountDown.Signal();
    }
}

#pragma warning restore CS0414 // Field is assigned but its value is never used
