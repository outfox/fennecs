// SPDX-License-Identifier: MIT

using System.Diagnostics.CodeAnalysis;
using fennecs.pools;

namespace fennecs;

public partial class World
{
    public void BulkAdd<T>(Archetype archetype, T data)
    {
        var typeExpression = TypeExpression.Of<T>();
        var newSignature = archetype.Signature.Add(typeExpression);
        var newArchetype = GetArchetype(newSignature);

        using var list = PooledList<(TypeExpression type, object data)>.Rent();
        list.Add((typeExpression, data!));
        archetype.Migrate(newArchetype, list);
    }


    internal void Apply(BulkOperation operation)
    {
        foreach (var archetype in operation.Archetypes)
        {
            var newSignature = archetype.Signature
                .Except(operation.Removals)
                .Union(operation.Additions);
            
            var newArchetype = GetArchetype(newSignature);
            
            //TODO: Implement back-fill / change types
            //archetype.Migrate(newArchetype, operation.BackFill);
        }
    }
    
    internal readonly struct BulkOperation : IDisposable
    {
        internal readonly PooledList<Archetype> Archetypes = PooledList<Archetype>.Rent();
        internal readonly PooledList<TypeExpression> Removals = PooledList<TypeExpression>.Rent();
        internal readonly PooledList<TypeExpression> Additions = PooledList<TypeExpression>.Rent();
        internal readonly PooledList<object> BackFill = PooledList<object>.Rent();
        
        public BulkOperation(IList<Archetype> archetypes)
        {
            Archetypes.AddRange(archetypes);
        }

        public BulkOperation Add<T>(T data, Identity target = default)
        {
            var typeExpression = TypeExpression.Of<T>(target);
            Additions.Add(typeExpression);
            BackFill.Add(data!);
            return this;
        }
        
        public BulkOperation Remove<T>(Identity target = default)
        {
            var typeExpression = TypeExpression.Of<T>(target);
            Removals.Add(typeExpression);
            return this;
        }
        
        public void Dispose()
        {
            Additions.Dispose();
            Removals.Dispose();
        }
    }
}
