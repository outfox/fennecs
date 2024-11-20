using System.Runtime.InteropServices;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
namespace fennecs;

public partial record Stream<C0, C1, C2>
{
    public ref struct EntityFuture
    {
        public ref readonly Entity Entity;
        public ref C0 Component0;
        public ref readonly C1 Component1;
        public ref readonly C2 Component2;
        
        internal readonly TypeExpression Type0;
        internal readonly TypeExpression Type1;
        internal readonly TypeExpression Type2;

        internal EntityFuture(TypeExpression type0, TypeExpression type1, TypeExpression type2, ref readonly Entity entity)
        {
            Entity = ref entity;
            Type0 = type0;
            Type1 = type1;
            Type2 = type2;
        }
        
    }
    
    public delegate void EntityFutureAction([In] ref readonly EntityFuture entityFuture);
    public delegate void EntityFutureAction2(EntityFuture entityFuture);
    public void For(EntityFutureAction action)
    {
        using var worldLock = World.Lock();

        foreach (var table in Filtered)
        {
            using var join = table.CrossJoin<C0, C1, C2>(StreamTypes.AsSpan());
            if (join.Empty) continue;


            var entities = table.Entities;
            var count = table.Entities.Length;
            do
            {
                
                var (s0, s1, s2) = join.Select;
                var span0 = s0.Span; var type0 = s0.Expression; var span1 = s1.Span; var type1 = s1.Expression; var span2 = s2.Span; var type2 = s2.Expression;
                var entity = default(Entity);
                var future = new EntityFuture(type0, type1, type2, in entity);
                
                for (var i = 0; i < count; i++)
                {
                    entity = new Entity(World, entities[i]);
                    future.Component0 = ref span0[i];
                    future.Component1 = ref span1[i];
                    future.Component2 = ref span2[i];
                    action(in future);
                }
            } while (join.Iterate());
        }
    }
    public void For(EntityFutureAction2 action)
    {
        using var worldLock = World.Lock();

        foreach (var table in Filtered)
        {
            using var join = table.CrossJoin<C0, C1, C2>(StreamTypes.AsSpan());
            if (join.Empty) continue;


            var entities = table.Entities;
            var count = table.Entities.Length;
            do
            {
                var (s0, s1, s2) = join.Select;
                var span0 = s0.Span; var type0 = s0.Expression; var span1 = s1.Span; var type1 = s1.Expression; var span2 = s2.Span; var type2 = s2.Expression;
                
                var entity = default(Entity);
                var future = new EntityFuture(type0, type1, type2, in entity);

                for (var i = 0; i < count; i++)
                {
                    entity = new Entity(World, entities[i]);
                    future.Component0 = ref span0[i];
                    future.Component1 = ref span1[i];
                    future.Component2 = ref span2[i];
                    action(future);
                }
            } while (join.Iterate());
        }
    }

}