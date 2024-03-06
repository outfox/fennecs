using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;

namespace fennecs;

public partial class World
{
    #region State & Storage
    private readonly ConcurrentQueue<DeferredOperation> _deferredOperations = new();
    #endregion


    #region Locking & Deferred Operations
    public struct WorldLock : IDisposable
    {
        private World _world;


        public WorldLock(World world)
        {
            lock (world._modeChangeLock)
            {
                _world = world;
                _world.Mode = WorldMode.Deferred;
                _world._locks++;
            }
        }


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
                    AddComponent(op.Identity, op.TypeExpression, op.Data);
                    break;

                case Opcode.Remove:
                    RemoveComponent(op.Identity, op.TypeExpression);
                    break;

                case Opcode.Despawn:
                    DespawnImpl(op.Identity);
                    break;

                case Opcode.Batch:
                    var batch = (BatchOperation) op.Data;
                    Commit(batch);
                    batch.Dispose();
                    break;
                
                default:
                    throw new NotImplementedException($"OpCode {op.Opcode} not implemented");
            }
    }


    internal struct DeferredOperation
    {
        internal required Opcode Opcode;
        internal TypeExpression TypeExpression;
        internal Identity Identity;
        internal object Data;
        internal Archetype Archetype;


        [SetsRequiredMembers]
        public DeferredOperation(BatchOperation operation)
        {
            Opcode = Opcode.Batch;
            Data = operation;

            Archetype = default!;
            TypeExpression = default;
            Identity = default;
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