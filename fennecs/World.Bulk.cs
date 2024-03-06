// SPDX-License-Identifier: MIT

using System.Diagnostics.CodeAnalysis;
using fennecs.pools;

namespace fennecs;

public partial class World
{
    public void BulkAdd<T>(Archetype archetype, T data)
    {
        var typeExpression = TypeExpression.Of<T>();
        var edge = archetype.GetTableEdge(typeExpression);
        
        if (edge.Add == null)
        {
            var types = archetype.Types.Add(typeExpression);
            edge.Add = AddTable(types);
        }

        using var list = PooledList<(TypeExpression type, object data)>.Rent();
        list.Add((typeExpression, data!));
        archetype.Migrate(edge.Add, list);
    }


    internal void Apply(BulkOperation operation)
    {
        foreach (var archetype in operation.Archetypes)
        {
            var types = archetype.Types
                .Except(operation.Removals)
                .Union(operation.Additions);

            
            var cursor = archetype;
            foreach (var type in operation.Removals)
            {
                var edge = cursor!.GetTableEdge(type);
                cursor = cursor.GetTableEdge(type).Remove;
                
                if (edge.Add == null)
                {
                    var newTypes = archetype.Types.Add(type);
                    edge.Add = AddTable(newTypes);
                }
            }
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
