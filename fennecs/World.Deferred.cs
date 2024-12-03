using System.Collections.Concurrent;


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
            lock (world._modeChangeLock)
            {
                _world = world;
                _world.Mode = WorldMode.Deferred;
                _world._locks++;
            }
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
            switch (op.Opcode)
            {
                case Opcode.Add:
                    AddComponent(op.Entity, op.TypeExpression, op.Data);
                    break;

                case Opcode.Remove:
                    RemoveComponent(op.Entity, op.TypeExpression);
                    break;

                case Opcode.Despawn:
                    DespawnImpl(op.Entity);
                    break;

                case Opcode.Batch:
                    var batch = (Batch) op.Data;
                    Commit(batch);
                    batch.Dispose();
                    break;
            }
    }


    internal struct DeferredOperation(Batch operation)
    {
        internal required Opcode Opcode = Opcode.Batch;
        internal TypeExpression TypeExpression = default;
        internal Entity Entity = default;
        internal object Data = operation;


        //[SetsRequiredMembers]
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