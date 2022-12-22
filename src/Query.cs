using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace HypEcs;

public class Query
{
    public readonly List<Table> Tables;

    internal readonly Archetypes Archetypes;
    internal readonly Mask Mask;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Query(Archetypes archetypes, Mask mask, List<Table> tables)
    {
        Tables = tables;
        Archetypes = archetypes;
        Mask = mask;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Has(Entity entity)
    {
        var meta = Archetypes.GetEntityMeta(entity.Identity);
        var table = Archetypes.GetTable(meta.TableId);
        return Tables.Contains(table);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void AddTable(Table table)
    {
        Tables.Add(table);
    }
}

public class Query<C> : Query
    where C : struct
{
    public Query(Archetypes archetypes, Mask mask, List<Table> tables) : base(archetypes, mask, tables)
    {
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ref C Get(Entity entity)
    {
        var meta = Archetypes.GetEntityMeta(entity.Identity);
        var table = Archetypes.GetTable(meta.TableId);
        var storage = table.GetStorage<C>(Identity.None);
        return ref storage[meta.Row];
    }

    public void Run(Action<int, C[]> action)
    {
        Archetypes.Lock();
        
        for (var t = 0; t < Tables.Count; t++)
        {
            var table = Tables[t];

            if (table.IsEmpty) continue;

            var s = table.GetStorage<C>(Identity.None);

            action(table.Count, s);
        }
        
        Archetypes.Unlock();
    }
    
    public void RunParallel(Action<int, C[]> action)
    {
        Archetypes.Lock();

        Parallel.For(0, Tables.Count, t =>
        {
            var table = Tables[t];
            
            if (table.IsEmpty) return;

            var s = table.GetStorage<C>(Identity.None);
            
            action(table.Count, s);
        });
        
        Archetypes.Unlock();
    }
}

public class Query<C1, C2> : Query
    where C1 : struct
    where C2 : struct
{
    public Query(Archetypes archetypes, Mask mask, List<Table> tables) : base(archetypes, mask, tables)
    {
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public RefValueTuple<C1, C2> Get(Entity entity)
    {
        var meta = Archetypes.GetEntityMeta(entity.Identity);
        var table = Archetypes.GetTable(meta.TableId);
        var storage1 = table.GetStorage<C1>(Identity.None);
        var storage2 = table.GetStorage<C2>(Identity.None);
        return new RefValueTuple<C1, C2>(ref storage1[meta.Row], ref storage2[meta.Row]);
    }

    public void Run(Action<int, C1[], C2[]> action)
    {
        Archetypes.Lock();
        
        for (var t = 0; t < Tables.Count; t++)
        {
            var table = Tables[t];

            if (table.IsEmpty) continue;

            var s1 = table.GetStorage<C1>(Identity.None);
            var s2 = table.GetStorage<C2>(Identity.None);

            action(table.Count, s1, s2);
        }
        
        Archetypes.Unlock();
    }
    
    public void RunParallel(Action<int, C1[], C2[]> action)
    {
        Archetypes.Lock();

        Parallel.For(0, Tables.Count, t =>
        {
            var table = Tables[t];

            if (table.IsEmpty) return;

            var s1 = table.GetStorage<C1>(Identity.None);
            var s2 = table.GetStorage<C2>(Identity.None);

            action(table.Count, s1, s2);
        });
        
        Archetypes.Unlock();
    }
}

public class Query<C1, C2, C3> : Query
    where C1 : struct
    where C2 : struct
    where C3 : struct
{
    public Query(Archetypes archetypes, Mask mask, List<Table> tables) : base(archetypes, mask, tables)
    {
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public RefValueTuple<C1, C2, C3> Get(Entity entity)
    {
        var meta = Archetypes.GetEntityMeta(entity.Identity);
        var table = Archetypes.GetTable(meta.TableId);
        var storage1 = table.GetStorage<C1>(Identity.None);
        var storage2 = table.GetStorage<C2>(Identity.None);
        var storage3 = table.GetStorage<C3>(Identity.None);
        return new RefValueTuple<C1, C2, C3>(ref storage1[meta.Row], ref storage2[meta.Row],
            ref storage3[meta.Row]);
    }

    public void Run(Action<int, C1[], C2[], C3[]> action)
    {
        Archetypes.Lock();
        
        for (var t = 0; t < Tables.Count; t++)
        {
            var table = Tables[t];

            if (table.IsEmpty) continue;

            var s1 = table.GetStorage<C1>(Identity.None);
            var s2 = table.GetStorage<C2>(Identity.None);
            var s3 = table.GetStorage<C3>(Identity.None);

            action(table.Count, s1, s2, s3);
        }
        
        Archetypes.Unlock();
    }
    
    public void RunParallel(Action<int, C1[], C2[], C3[]> action)
    {
        Archetypes.Lock();

        Parallel.For(0, Tables.Count, t =>
        {
            var table = Tables[t];
            
            if (table.IsEmpty) return;

            var s1 = table.GetStorage<C1>(Identity.None);
            var s2 = table.GetStorage<C2>(Identity.None);
            var s3 = table.GetStorage<C3>(Identity.None);
            
            action(table.Count, s1, s2, s3);
        });
        
        Archetypes.Unlock();
    }
}

public class Query<C1, C2, C3, C4> : Query
    where C1 : struct
    where C2 : struct
    where C3 : struct
    where C4 : struct
{
    public Query(Archetypes archetypes, Mask mask, List<Table> tables) : base(archetypes, mask, tables)
    {
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public RefValueTuple<C1, C2, C3, C4> Get(Entity entity)
    {
        var meta = Archetypes.GetEntityMeta(entity.Identity);
        var table = Archetypes.GetTable(meta.TableId);
        var s1 = table.GetStorage<C1>(Identity.None);
        var s2 = table.GetStorage<C2>(Identity.None);
        var s3 = table.GetStorage<C3>(Identity.None);
        var s4 = table.GetStorage<C4>(Identity.None);
        return new RefValueTuple<C1, C2, C3, C4>(ref s1[meta.Row], ref s2[meta.Row],
            ref s3[meta.Row], ref s4[meta.Row]);
    }

    public void Run(Action<int, C1[], C2[], C3[], C4[]> action)
    {
        Archetypes.Lock();
        
        for (var t = 0; t < Tables.Count; t++)
        {
            var table = Tables[t];

            if (table.IsEmpty) continue;
            
            var s1 = table.GetStorage<C1>(Identity.None);
            var s2 = table.GetStorage<C2>(Identity.None);
            var s3 = table.GetStorage<C3>(Identity.None);
            var s4 = table.GetStorage<C4>(Identity.None);

            action(table.Count, s1, s2, s3, s4);
        }
        
        Archetypes.Unlock();
    }
    
    public void RunParallel(Action<int, C1[], C2[], C3[], C4[]> action)
    {
        Archetypes.Lock();

        Parallel.For(0, Tables.Count, t =>
        {
            var table = Tables[t];
            
            if (table.IsEmpty) return;

            var s1 = table.GetStorage<C1>(Identity.None);
            var s2 = table.GetStorage<C2>(Identity.None);
            var s3 = table.GetStorage<C3>(Identity.None);
            var s4 = table.GetStorage<C4>(Identity.None);
            
            action(table.Count, s1, s2, s3, s4);
        });
        
        Archetypes.Unlock();
    }
}

public class Query<C1, C2, C3, C4, C5> : Query
    where C1 : struct
    where C2 : struct
    where C3 : struct
    where C4 : struct
    where C5 : struct
{
    public Query(Archetypes archetypes, Mask mask, List<Table> tables) : base(archetypes, mask, tables)
    {
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public RefValueTuple<C1, C2, C3, C4, C5> Get(Entity entity)
    {
        var meta = Archetypes.GetEntityMeta(entity.Identity);
        var table = Archetypes.GetTable(meta.TableId);
        var s1 = table.GetStorage<C1>(Identity.None);
        var s2 = table.GetStorage<C2>(Identity.None);
        var s3 = table.GetStorage<C3>(Identity.None);
        var s4 = table.GetStorage<C4>(Identity.None);
        var s5 = table.GetStorage<C5>(Identity.None);
        return new RefValueTuple<C1, C2, C3, C4, C5>(ref s1[meta.Row], ref s2[meta.Row],
            ref s3[meta.Row], ref s4[meta.Row], ref s5[meta.Row]);
    }

    public void Run(Action<int, C1[], C2[], C3[], C4[], C5[]> action)
    {
        Archetypes.Lock();
        
        for (var t = 0; t < Tables.Count; t++)
        {
            var table = Tables[t];

            if (table.IsEmpty) continue;

            var s1 = table.GetStorage<C1>(Identity.None);
            var s2 = table.GetStorage<C2>(Identity.None);
            var s3 = table.GetStorage<C3>(Identity.None);
            var s4 = table.GetStorage<C4>(Identity.None);
            var s5 = table.GetStorage<C5>(Identity.None);

            action(table.Count, s1, s2, s3, s4, s5);
        }
        
        Archetypes.Unlock();
    }
    
    public void RunParallel(Action<int, C1[], C2[], C3[], C4[], C5[]> action)
    {
        Archetypes.Lock();

        Parallel.For(0, Tables.Count, t =>
        {
            var table = Tables[t];
            
            if (table.IsEmpty) return;

            var s1 = table.GetStorage<C1>(Identity.None);
            var s2 = table.GetStorage<C2>(Identity.None);
            var s3 = table.GetStorage<C3>(Identity.None);
            var s4 = table.GetStorage<C4>(Identity.None);
            var s5 = table.GetStorage<C5>(Identity.None);
            
            action(table.Count, s1, s2, s3, s4, s5);
        });
        
        Archetypes.Unlock();
    }
}

public class Query<C1, C2, C3, C4, C5, C6> : Query
    where C1 : struct
    where C2 : struct
    where C3 : struct
    where C4 : struct
    where C5 : struct
    where C6 : struct
{
    public Query(Archetypes archetypes, Mask mask, List<Table> tables) : base(archetypes, mask, tables)
    {
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public RefValueTuple<C1, C2, C3, C4, C5, C6> Get(Entity entity)
    {
        var meta = Archetypes.GetEntityMeta(entity.Identity);
        var table = Archetypes.GetTable(meta.TableId);
        var s1 = table.GetStorage<C1>(Identity.None);
        var s2 = table.GetStorage<C2>(Identity.None);
        var s3 = table.GetStorage<C3>(Identity.None);
        var s4 = table.GetStorage<C4>(Identity.None);
        var s5 = table.GetStorage<C5>(Identity.None);
        var s6 = table.GetStorage<C6>(Identity.None);
        return new RefValueTuple<C1, C2, C3, C4, C5, C6>(ref s1[meta.Row], ref s2[meta.Row],
            ref s3[meta.Row], ref s4[meta.Row], ref s5[meta.Row],
            ref s6[meta.Row]);
    }

    public void Run(Action<int, C1[], C2[], C3[], C4[], C5[], C6[]> action)
    {
        Archetypes.Lock();
        
        for (var t = 0; t < Tables.Count; t++)
        {
            var table = Tables[t];

            if (table.IsEmpty) continue;

            var s1 = table.GetStorage<C1>(Identity.None);
            var s2 = table.GetStorage<C2>(Identity.None);
            var s3 = table.GetStorage<C3>(Identity.None);
            var s4 = table.GetStorage<C4>(Identity.None);
            var s5 = table.GetStorage<C5>(Identity.None);
            var s6 = table.GetStorage<C6>(Identity.None);

            action(table.Count, s1, s2, s3, s4, s5, s6);
        }
        
        Archetypes.Unlock();
    }
    
    public void RunParallel(Action<int, C1[], C2[], C3[], C4[], C5[], C6[]> action)
    {
        Archetypes.Lock();

        Parallel.For(0, Tables.Count, t =>
        {
            var table = Tables[t];
            
            if (table.IsEmpty) return;

            var s1 = table.GetStorage<C1>(Identity.None);
            var s2 = table.GetStorage<C2>(Identity.None);
            var s3 = table.GetStorage<C3>(Identity.None);
            var s4 = table.GetStorage<C4>(Identity.None);
            var s5 = table.GetStorage<C5>(Identity.None);
            var s6 = table.GetStorage<C6>(Identity.None);
            
            action(table.Count, s1, s2, s3, s4, s5, s6);
        });
        
        Archetypes.Unlock();
    }
}

public class Query<C1, C2, C3, C4, C5, C6, C7> : Query
    where C1 : struct
    where C2 : struct
    where C3 : struct
    where C4 : struct
    where C5 : struct
    where C6 : struct
    where C7 : struct
{
    public Query(Archetypes archetypes, Mask mask, List<Table> tables) : base(archetypes, mask, tables)
    {
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public RefValueTuple<C1, C2, C3, C4, C5, C6, C7> Get(Entity entity)
    {
        var meta = Archetypes.GetEntityMeta(entity.Identity);
        var table = Archetypes.GetTable(meta.TableId);
        var s1 = table.GetStorage<C1>(Identity.None);
        var s2 = table.GetStorage<C2>(Identity.None);
        var s3 = table.GetStorage<C3>(Identity.None);
        var s4 = table.GetStorage<C4>(Identity.None);
        var s5 = table.GetStorage<C5>(Identity.None);
        var s6 = table.GetStorage<C6>(Identity.None);
        var s7 = table.GetStorage<C7>(Identity.None);
        return new RefValueTuple<C1, C2, C3, C4, C5, C6, C7>(ref s1[meta.Row], ref s2[meta.Row],
            ref s3[meta.Row], ref s4[meta.Row], ref s5[meta.Row],
            ref s6[meta.Row], ref s7[meta.Row]);
    }

    public void Run(Action<int, C1[], C2[], C3[], C4[], C5[], C6[], C7[]> action)
    {
        Archetypes.Lock();
        
        for (var t = 0; t < Tables.Count; t++)
        {
            var table = Tables[t];

            if (table.IsEmpty) continue;

            var s1 = table.GetStorage<C1>(Identity.None);
            var s2 = table.GetStorage<C2>(Identity.None);
            var s3 = table.GetStorage<C3>(Identity.None);
            var s4 = table.GetStorage<C4>(Identity.None);
            var s5 = table.GetStorage<C5>(Identity.None);
            var s6 = table.GetStorage<C6>(Identity.None);
            var s7 = table.GetStorage<C7>(Identity.None);

            action(table.Count, s1, s2, s3, s4, s5, s6, s7);
        }
        
        Archetypes.Unlock();
    }
    
    public void RunParallel(Action<int, C1[], C2[], C3[], C4[], C5[], C6[], C7[]> action)
    {
        Archetypes.Lock();

        Parallel.For(0, Tables.Count, t =>
        {
            var table = Tables[t];
            
            if (table.IsEmpty) return;

            var s1 = table.GetStorage<C1>(Identity.None);
            var s2 = table.GetStorage<C2>(Identity.None);
            var s3 = table.GetStorage<C3>(Identity.None);
            var s4 = table.GetStorage<C4>(Identity.None);
            var s5 = table.GetStorage<C5>(Identity.None);
            var s6 = table.GetStorage<C6>(Identity.None);
            var s7 = table.GetStorage<C7>(Identity.None);
            
            action(table.Count, s1, s2, s3, s4, s5, s6, s7);
        });
        
        Archetypes.Unlock();
    }
}

public class Query<C1, C2, C3, C4, C5, C6, C7, C8> : Query
    where C1 : struct
    where C2 : struct
    where C3 : struct
    where C4 : struct
    where C5 : struct
    where C6 : struct
    where C7 : struct
    where C8 : struct
{
    public Query(Archetypes archetypes, Mask mask, List<Table> tables) : base(archetypes, mask, tables)
    {
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public RefValueTuple<C1, C2, C3, C4, C5, C6, C7, C8> Get(Entity entity)
    {
        var meta = Archetypes.GetEntityMeta(entity.Identity);
        var table = Archetypes.GetTable(meta.TableId);
        var s1 = table.GetStorage<C1>(Identity.None);
        var s2 = table.GetStorage<C2>(Identity.None);
        var s3 = table.GetStorage<C3>(Identity.None);
        var s4 = table.GetStorage<C4>(Identity.None);
        var s5 = table.GetStorage<C5>(Identity.None);
        var s6 = table.GetStorage<C6>(Identity.None);
        var s7 = table.GetStorage<C7>(Identity.None);
        var s8 = table.GetStorage<C8>(Identity.None);
        return new RefValueTuple<C1, C2, C3, C4, C5, C6, C7, C8>(ref s1[meta.Row], ref s2[meta.Row],
            ref s3[meta.Row], ref s4[meta.Row], ref s5[meta.Row],
            ref s6[meta.Row], ref s7[meta.Row], ref s8[meta.Row]);
    }

    public void Run(Action<int, C1[], C2[], C3[], C4[], C5[], C6[], C7[], C8[]> action)
    {
        Archetypes.Lock();
        
        for (var t = 0; t < Tables.Count; t++)
        {
            var table = Tables[t];

            if (table.IsEmpty) continue;

            var s1 = table.GetStorage<C1>(Identity.None);
            var s2 = table.GetStorage<C2>(Identity.None);
            var s3 = table.GetStorage<C3>(Identity.None);
            var s4 = table.GetStorage<C4>(Identity.None);
            var s5 = table.GetStorage<C5>(Identity.None);
            var s6 = table.GetStorage<C6>(Identity.None);
            var s7 = table.GetStorage<C7>(Identity.None);
            var s8 = table.GetStorage<C8>(Identity.None);

            action(table.Count, s1, s2, s3, s4, s5, s6, s7, s8);
        }
        
        Archetypes.Unlock();
    }
    
    public void RunParallel(Action<int, C1[], C2[], C3[], C4[], C5[], C6[], C7[], C8[]> action)
    {
        Archetypes.Lock();

        Parallel.For(0, Tables.Count, t =>
        {
            var table = Tables[t];
            
            if (table.IsEmpty) return;

            var s1 = table.GetStorage<C1>(Identity.None);
            var s2 = table.GetStorage<C2>(Identity.None);
            var s3 = table.GetStorage<C3>(Identity.None);
            var s4 = table.GetStorage<C4>(Identity.None);
            var s5 = table.GetStorage<C5>(Identity.None);
            var s6 = table.GetStorage<C6>(Identity.None);
            var s7 = table.GetStorage<C7>(Identity.None);
            var s8 = table.GetStorage<C8>(Identity.None);
            
            action(table.Count, s1, s2, s3, s4, s5, s6, s7, s8);
        });
        
        Archetypes.Unlock();
    }
}

public class Query<C1, C2, C3, C4, C5, C6, C7, C8, C9> : Query
    where C1 : struct
    where C2 : struct
    where C3 : struct
    where C4 : struct
    where C5 : struct
    where C6 : struct
    where C7 : struct
    where C8 : struct
    where C9 : struct
{
    public Query(Archetypes archetypes, Mask mask, List<Table> tables) : base(archetypes, mask, tables)
    {
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public RefValueTuple<C1, C2, C3, C4, C5, C6, C7, C8, C9> Get(Entity entity)
    {
        var meta = Archetypes.GetEntityMeta(entity.Identity);
        var table = Archetypes.GetTable(meta.TableId);
        var s1 = table.GetStorage<C1>(Identity.None);
        var s2 = table.GetStorage<C2>(Identity.None);
        var s3 = table.GetStorage<C3>(Identity.None);
        var s4 = table.GetStorage<C4>(Identity.None);
        var s5 = table.GetStorage<C5>(Identity.None);
        var s6 = table.GetStorage<C6>(Identity.None);
        var s7 = table.GetStorage<C7>(Identity.None);
        var s8 = table.GetStorage<C8>(Identity.None);
        var s9 = table.GetStorage<C9>(Identity.None);
        return new RefValueTuple<C1, C2, C3, C4, C5, C6, C7, C8, C9>(ref s1[meta.Row], ref s2[meta.Row],
            ref s3[meta.Row], ref s4[meta.Row], ref s5[meta.Row],
            ref s6[meta.Row], ref s7[meta.Row], ref s8[meta.Row], ref s9[meta.Row]);
    }

    public void Run(Action<int, C1[], C2[], C3[], C4[], C5[], C6[], C7[], C8[], C9[]> action)
    {
        Archetypes.Lock();
        
        for (var t = 0; t < Tables.Count; t++)
        {
            var table = Tables[t];
            
            if (table.IsEmpty) continue;

            var s1 = table.GetStorage<C1>(Identity.None);
            var s2 = table.GetStorage<C2>(Identity.None);
            var s3 = table.GetStorage<C3>(Identity.None);
            var s4 = table.GetStorage<C4>(Identity.None);
            var s5 = table.GetStorage<C5>(Identity.None);
            var s6 = table.GetStorage<C6>(Identity.None);
            var s7 = table.GetStorage<C7>(Identity.None);
            var s8 = table.GetStorage<C8>(Identity.None);
            var s9 = table.GetStorage<C9>(Identity.None);
            
            action(table.Count, s1, s2, s3, s4, s5, s6, s7, s8, s9);
        }
        
        Archetypes.Unlock();
    }
    
    public void RunParallel(Action<int, C1[], C2[], C3[], C4[], C5[], C6[], C7[], C8[], C9[]> action)
    {
        Archetypes.Lock();

        Parallel.For(0, Tables.Count, t =>
        {
            var table = Tables[t];
            
            if (table.IsEmpty) return;

            var s1 = table.GetStorage<C1>(Identity.None);
            var s2 = table.GetStorage<C2>(Identity.None);
            var s3 = table.GetStorage<C3>(Identity.None);
            var s4 = table.GetStorage<C4>(Identity.None);
            var s5 = table.GetStorage<C5>(Identity.None);
            var s6 = table.GetStorage<C6>(Identity.None);
            var s7 = table.GetStorage<C7>(Identity.None);
            var s8 = table.GetStorage<C8>(Identity.None);
            var s9 = table.GetStorage<C9>(Identity.None);
            
            action(table.Count, s1, s2, s3, s4, s5, s6, s7, s8, s9);
        });
        
        Archetypes.Unlock();
    }
}