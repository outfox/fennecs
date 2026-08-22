using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;

namespace fennecs;

public partial class World
{
    #region State & Storage
    private readonly ConcurrentQueue<DeferredOperation> _deferredOperations = new();
    #endregion


    #region Locking & Deferred Operations
    /// <summary>
    /// Locks the World (setting into a Deferred mode) for the scope of the returned WorldLock.
    /// Used internally during the execution of Query runners.
    /// Can be used to "freeze" a World, deferring structural changes to it until the lock is released.
    /// </summary>
    public struct WorldLock : IDisposable
    {
        private World _world;

        internal WorldLock(World world)
        {
            _world = world;
            _world.Mode = WorldMode.Deferred;
            Interlocked.Increment(ref _world._locks);
        }


        /// <summary>
        /// Releases the lock, allowing deferred operations to be executed.
        /// </summary>
        /// <remarks>
        /// The execution is immediate if this is the last lock to be released.
        /// The world first moves from <see cref="WorldMode.Deferred"/> to <see cref="WorldMode.CatchUp"/> 
        /// and finally returns to <see cref="WorldMode.Immediate"/> after all <see cref="DeferredOperation"/> have been executed.
        /// </remarks>
        /// <inheritdoc />
        public void Dispose()
        {
            _world.Unlock();
            _world = null!;
        }
    }


    private void CatchUp(ConcurrentQueue<DeferredOperation> operations)
    {
        while (operations.TryDequeue(out var op))
        {
            if (!op.FromSignal)
            {
                Apply(op);
                continue;
            }

            // Queued by a Signal handler: a failure here belongs to that handler, not to whoever
            // took out the Lock (they are usually several stack frames apart by now).
            try
            {
                Apply(op);
            }
            catch (Exception exception) when (exception is not SignalException)
            {
                throw new SignalException(op.Describe(), op.Entity, exception);
            }
        }
    }


    private void Apply(DeferredOperation op)
    {
        switch (op.Opcode)
        {
            case Opcode.Add:
                AddComponent(op.Entity, op.TypeExpression, op.Data);
                break;

            case Opcode.Remove:
                RemoveComponent(op.Entity, op.TypeExpression, op.RemoveMode);
                break;

            case Opcode.Despawn:
                DespawnImpl(op.Entity);
                break;

            case Opcode.Batch:
                var batch = (Batch)op.Data;
                Commit(batch);
                batch.Dispose();
                break;
        }
    }


    internal struct DeferredOperation
    {
        internal required Opcode Opcode;
        internal TypeExpression TypeExpression;
        internal Entity Entity;
        internal object Data;
        internal Archetype Archetype;
        internal RemoveConflict RemoveMode;

        internal bool FromSignal;

        // Human-readable form of this operation, for diagnostics.
        internal readonly string Describe() => Opcode switch
        {
            Opcode.Add => $"Add {TypeExpression} to {Entity}",
            Opcode.Remove => $"Remove {TypeExpression} from {Entity}",
            Opcode.Despawn => $"Despawn {Entity}",
            Opcode.Batch => "Batch",
            _ => Opcode.ToString(),
        };


        [SetsRequiredMembers]
        // ReSharper disable once ConvertToPrimaryConstructor
        public DeferredOperation(Batch operation)
        {
            Opcode = Opcode.Batch;
            Data = operation;

            Archetype = null!;
            TypeExpression = default;
            Entity = default;
        }
    }


    internal enum Opcode
    {
        // Entity operations
        Add,
        Remove,
        Despawn,

        // Archetype operations
        Batch,
    }
    #endregion
}
