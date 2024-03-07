// SPDX-License-Identifier: MIT

using fennecs.pools;

namespace fennecs;

public partial class World
{
    internal bool Submit(BatchOperation operation)
    {
        if (Mode != WorldMode.Immediate)
        {
            _deferredOperations.Enqueue(new DeferredOperation(operation));
            return false;
        }
        
        Commit(operation);
        return true;
    }


    private void Commit(BatchOperation operation)
    {
        foreach (var archetype in operation.Archetypes)
        {
            var newSignature = archetype.Signature
                .Except(operation.Removals)
                .Union(operation.Additions);

            var newArchetype = GetArchetype(newSignature);
            archetype.Migrate(newArchetype, operation.Additions, operation.BackFill);
        }
    }


    public readonly struct BatchOperation : IDisposable
    {
        private readonly World _world;
        private readonly Mask _mask;
        
        internal readonly PooledList<Archetype> Archetypes = PooledList<Archetype>.Rent();
        internal readonly PooledList<TypeExpression> Additions = PooledList<TypeExpression>.Rent();
        internal readonly PooledList<TypeExpression> Removals = PooledList<TypeExpression>.Rent();
        internal readonly PooledList<object> BackFill = PooledList<object>.Rent();

        internal readonly AddConflictMode AddMode;
        internal readonly RemoveConflictMode RemoveMode;
        
        public void Submit()
        {
            if (_world.Submit(this)) Dispose();
        }
        
        internal BatchOperation(IReadOnlyList<Archetype> archetypes, World world, Mask mask, AddConflictMode addMode, RemoveConflictMode removeMode)
        {
            Archetypes.AddRange(archetypes);
            _world = world;
            _mask = mask;
            AddMode = addMode;
            RemoveMode = removeMode;
        }



        public BatchOperation Add<T>(T data) => AddComponent(data, default);
        public BatchOperation AddLink<T>(T target) where T : class => AddComponent(target, Identity.Of(target));
        public BatchOperation AddRelation<T>(Entity target) where T : new() => AddComponent<T>(new T(), target.Id);
        public BatchOperation AddRelation<T>(T data, Entity target) where T : notnull => AddComponent(data, target.Id);

        public BatchOperation Remove<T>() => RemoveComponent<T>();
        public BatchOperation Remove<T>(Entity target) => RemoveComponent<T>(target.Id);
        public BatchOperation RemoveLink<T>(T target) where T : class => RemoveComponent<T>(Identity.Of(target));
        public BatchOperation RemoveRelation<T>(Entity target) where T : new() => RemoveComponent<T>(target.Id);


        private BatchOperation AddComponent<T>(T data, Identity target)
        {
            var typeExpression = TypeExpression.Of<T>(target);

            if (!_mask.SafeForAddition(typeExpression))
            {
                throw new InvalidOperationException($"TypeExpression {typeExpression} is not filtered out via Not<T> by this Query/Mask, additions would cause invalid runtime state.");
            }

            if (Additions.Contains(typeExpression))
                throw new InvalidOperationException($"Duplicate addition {typeExpression} : {data}");
            
            if (Removals.Contains(typeExpression))
                throw new InvalidOperationException($"Addition {typeExpression} conflicts with removal");
            
            Additions.Add(typeExpression);
            BackFill.Add(data!);
            return this;
        }


        private BatchOperation RemoveComponent<T>(Identity target = default)
        {
            var typeExpression = TypeExpression.Of<T>(target);

            /*
            if (!_mask.SafeForRemoval(typeExpression))
            {
              throw new InvalidOperationException($"TypeExpression {typeExpression} is not included via Has<T> or Any<T> by this Query/Mask, removals would cause invalid runtime state.");
            }
            */

            if (Additions.Contains(typeExpression))
                throw new InvalidOperationException($"Removal {typeExpression} conflicts with addition");

            if (Removals.Contains(typeExpression))
                throw new InvalidOperationException($"Duplicate removal {typeExpression}");

            Removals.Add(typeExpression);
            return this;
        }


        public void Dispose()
        {
            Archetypes.Dispose();
            Additions.Dispose();
            Removals.Dispose();
            BackFill.Dispose();
            _mask.Dispose();
        }


        /// <summary>
        /// Batch Addition conflict resolution mode.
        /// </summary>
        public enum AddConflictMode
        {
            /// <summary>
            /// Disallow add operation if the Component to be added is not guaranteed to be absent
            /// on ALL matched Archetypes see <see cref="QueryBuilder.Not{T}(fennecs.Identity)"/>.
            /// </summary>
            Disallow = default,

            /// <summary>
            /// Skip Archetypes where the Component to be added is already present.
            /// </summary>
            Skip,
            
            /// <summary>
            /// Replace (overwrite) the Component to be added if it is already present.
            /// </summary>
            Replace,
        }


        /// <summary>
        /// Batch Removal conflict resolution mode.
        /// </summary>
        public enum RemoveConflictMode
        {
            /// <summary>
            /// Skip Archetypes where the Component to be removed is not present.
            /// </summary>
            Skip = default,

            /// <summary>
            /// Disallow remove operation if the Component to be removed is not guaranteed to be present
            /// on ALL matched Archetypes, see <see cref="QueryBuilder.Has{T}(fennecs.Identity)"/>.
            /// </summary>
            Disallow,
        }
    }
}