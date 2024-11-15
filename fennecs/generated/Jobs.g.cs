    using fennecs;

    internal record JobR<C0> : IThreadPoolWorkItem
        where C0 : notnull
    {
        public ReadOnlyMemory<Identity> MemoryE = null!;
        public World World = null!;

        // Memories
        ...

        // Types
        public TypeExpression Type0 = default;

        public Action<...> Action = null!;
        public CountdownEvent CountDown = null!;
        public void Execute()
        {
            var identities = MemoryE.Span;
            var span0 = Memory0.Span

            var count = identities.Length;
            for (var i = 0; i < count; i++)
            {
                var entity = new Entity(World, identities[i]);
                Action(...);
            }
            CountDown.Signal();
        }
    }
