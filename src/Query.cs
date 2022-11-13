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

    protected readonly Dictionary<int, Array[]> Storages = new();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Query(Archetypes archetypes, Mask mask, List<Table> tables)
    {
        Tables = tables;
        Archetypes = archetypes;
        Mask = mask;

        UpdateStorages();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Has(Entity entity)
    {
        var meta = Archetypes.GetEntityMeta(entity.Identity);
        return Storages.ContainsKey(meta.TableId);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void AddTable(Table table)
    {
        Tables.Add(table);
        UpdateStorages();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected virtual Array[] GetStorages(Table table)
    {
        throw new Exception("Invalid Enumerator");
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    void UpdateStorages()
    {
        Storages.Clear();

        foreach (var table in Tables)
        {
            var storages = GetStorages(table);
            Storages.Add(table.Id, storages);
        }
    }
}

public class Query<C> : Query
    where C : struct
{
    public Query(Archetypes archetypes, Mask mask, List<Table> tables) : base(archetypes, mask, tables)
    {
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected override Array[] GetStorages(Table table)
    {
        return new Array[] { table.GetStorage<C>(Identity.None) };
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ref C Get(Entity entity)
    {
        var meta = Archetypes.GetEntityMeta(entity.Identity);
        var storage = (C[])Storages[meta.TableId][0];
        return ref storage[meta.Row];
    }

    public void Run(Action<int, C[]> action)
    {
        Archetypes.Lock();
        
        for (var t = 0; t < Tables.Count; t++)
        {
            var table = Tables[t];

            if (table.IsEmpty) continue;

            var storages = Storages[table.Id];

            var s1 = (C[])storages[0];

            action(table.Count, s1);
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

            var storages = Storages[table.Id];
            
            var s = (C[])storages[0];
            
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
    protected override Array[] GetStorages(Table table)
    {
        return new Array[]
        {
            table.GetStorage<C1>(Identity.None),
            table.GetStorage<C2>(Identity.None),
        };
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public RefValueTuple<C1, C2> Get(Entity entity)
    {
        var meta = Archetypes.GetEntityMeta(entity.Identity);
        var storages = Storages[meta.TableId];
        var storage1 = (C1[])storages[0];
        var storage2 = (C2[])storages[1];
        return new RefValueTuple<C1, C2>(ref storage1[meta.Row], ref storage2[meta.Row]);
    }

    public void Run(Action<int, C1[], C2[]> action)
    {
        Archetypes.Lock();
        
        for (var t = 0; t < Tables.Count; t++)
        {
            var table = Tables[t];

            if (table.IsEmpty) continue;

            var storages = Storages[table.Id];

            var s1 = (C1[])storages[0];
            var s2 = (C2[])storages[1];

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

            var storages = Storages[table.Id];

            var s1 = (C1[])storages[0];
            var s2 = (C2[])storages[1];

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
    protected override Array[] GetStorages(Table table)
    {
        return new Array[]
        {
            table.GetStorage<C1>(Identity.None),
            table.GetStorage<C2>(Identity.None),
            table.GetStorage<C3>(Identity.None),
        };
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public RefValueTuple<C1, C2, C3> Get(Entity entity)
    {
        var meta = Archetypes.GetEntityMeta(entity.Identity);
        var storages = Storages[meta.TableId];
        var storage1 = (C1[])storages[0];
        var storage2 = (C2[])storages[1];
        var storage3 = (C3[])storages[2];
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

            var storages = Storages[table.Id];

            var s1 = (C1[])storages[0];
            var s2 = (C2[])storages[1];
            var s3 = (C3[])storages[2];

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

            var storages = Storages[table.Id];
            
            var s1 = (C1[])storages[0];
            var s2 = (C2[])storages[1];
            var s3 = (C3[])storages[2];
            
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
    protected override Array[] GetStorages(Table table)
    {
        return new Array[]
        {
            table.GetStorage<C1>(Identity.None),
            table.GetStorage<C2>(Identity.None),
            table.GetStorage<C3>(Identity.None),
            table.GetStorage<C4>(Identity.None),
        };
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public RefValueTuple<C1, C2, C3, C4> Get(Entity entity)
    {
        var meta = Archetypes.GetEntityMeta(entity.Identity);
        var storages = Storages[meta.TableId];
        var storage1 = (C1[])storages[0];
        var storage2 = (C2[])storages[1];
        var storage3 = (C3[])storages[2];
        var storage4 = (C4[])storages[3];
        return new RefValueTuple<C1, C2, C3, C4>(ref storage1[meta.Row], ref storage2[meta.Row],
            ref storage3[meta.Row], ref storage4[meta.Row]);
    }

    public void Run(Action<int, C1[], C2[], C3[], C4[]> action)
    {
        Archetypes.Lock();
        
        for (var t = 0; t < Tables.Count; t++)
        {
            var table = Tables[t];

            if (table.IsEmpty) continue;

            var storages = Storages[table.Id];

            var s1 = (C1[])storages[0];
            var s2 = (C2[])storages[1];
            var s3 = (C3[])storages[2];
            var s4 = (C4[])storages[3];

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

            var storages = Storages[table.Id];
            
            var s1 = (C1[])storages[0];
            var s2 = (C2[])storages[1];
            var s3 = (C3[])storages[2];
            var s4 = (C4[])storages[3];
            
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
    protected override Array[] GetStorages(Table table)
    {
        return new Array[]
        {
            table.GetStorage<C1>(Identity.None),
            table.GetStorage<C2>(Identity.None),
            table.GetStorage<C3>(Identity.None),
            table.GetStorage<C4>(Identity.None),
            table.GetStorage<C5>(Identity.None),
        };
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public RefValueTuple<C1, C2, C3, C4, C5> Get(Entity entity)
    {
        var meta = Archetypes.GetEntityMeta(entity.Identity);
        var storages = Storages[meta.TableId];
        var storage1 = (C1[])storages[0];
        var storage2 = (C2[])storages[1];
        var storage3 = (C3[])storages[2];
        var storage4 = (C4[])storages[3];
        var storage5 = (C5[])storages[4];
        return new RefValueTuple<C1, C2, C3, C4, C5>(ref storage1[meta.Row], ref storage2[meta.Row],
            ref storage3[meta.Row], ref storage4[meta.Row], ref storage5[meta.Row]);
    }

    public void Run(Action<int, C1[], C2[], C3[], C4[], C5[]> action)
    {
        Archetypes.Lock();
        
        for (var t = 0; t < Tables.Count; t++)
        {
            var table = Tables[t];

            if (table.IsEmpty) continue;

            var storages = Storages[table.Id];

            var s1 = (C1[])storages[0];
            var s2 = (C2[])storages[1];
            var s3 = (C3[])storages[2];
            var s4 = (C4[])storages[3];
            var s5 = (C5[])storages[4];

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

            var storages = Storages[table.Id];
            
            var s1 = (C1[])storages[0];
            var s2 = (C2[])storages[1];
            var s3 = (C3[])storages[2];
            var s4 = (C4[])storages[3];
            var s5 = (C5[])storages[4];
            
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
    protected override Array[] GetStorages(Table table)
    {
        return new Array[]
        {
            table.GetStorage<C1>(Identity.None),
            table.GetStorage<C2>(Identity.None),
            table.GetStorage<C3>(Identity.None),
            table.GetStorage<C4>(Identity.None),
            table.GetStorage<C5>(Identity.None),
            table.GetStorage<C6>(Identity.None),
        };
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public RefValueTuple<C1, C2, C3, C4, C5, C6> Get(Entity entity)
    {
        var meta = Archetypes.GetEntityMeta(entity.Identity);
        var storages = Storages[meta.TableId];
        var storage1 = (C1[])storages[0];
        var storage2 = (C2[])storages[1];
        var storage3 = (C3[])storages[2];
        var storage4 = (C4[])storages[3];
        var storage5 = (C5[])storages[4];
        var storage6 = (C6[])storages[5];
        return new RefValueTuple<C1, C2, C3, C4, C5, C6>(ref storage1[meta.Row], ref storage2[meta.Row],
            ref storage3[meta.Row], ref storage4[meta.Row], ref storage5[meta.Row],
            ref storage6[meta.Row]);
    }

    public void Run(Action<int, C1[], C2[], C3[], C4[], C5[], C6[]> action)
    {
        Archetypes.Lock();
        
        for (var t = 0; t < Tables.Count; t++)
        {
            var table = Tables[t];

            if (table.IsEmpty) continue;

            var storages = Storages[table.Id];

            var s1 = (C1[])storages[0];
            var s2 = (C2[])storages[1];
            var s3 = (C3[])storages[2];
            var s4 = (C4[])storages[3];
            var s5 = (C5[])storages[4];
            var s6 = (C6[])storages[5];

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

            var storages = Storages[table.Id];
            
            var s1 = (C1[])storages[0];
            var s2 = (C2[])storages[1];
            var s3 = (C3[])storages[2];
            var s4 = (C4[])storages[3];
            var s5 = (C5[])storages[4];
            var s6 = (C6[])storages[5];
            
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
    protected override Array[] GetStorages(Table table)
    {
        return new Array[]
        {
            table.GetStorage<C1>(Identity.None),
            table.GetStorage<C2>(Identity.None),
            table.GetStorage<C3>(Identity.None),
            table.GetStorage<C4>(Identity.None),
            table.GetStorage<C5>(Identity.None),
            table.GetStorage<C6>(Identity.None),
            table.GetStorage<C7>(Identity.None),
        };
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public RefValueTuple<C1, C2, C3, C4, C5, C6, C7> Get(Entity entity)
    {
        var meta = Archetypes.GetEntityMeta(entity.Identity);
        var storages = Storages[meta.TableId];
        var storage1 = (C1[])storages[0];
        var storage2 = (C2[])storages[1];
        var storage3 = (C3[])storages[2];
        var storage4 = (C4[])storages[3];
        var storage5 = (C5[])storages[4];
        var storage6 = (C6[])storages[5];
        var storage7 = (C7[])storages[6];
        return new RefValueTuple<C1, C2, C3, C4, C5, C6, C7>(ref storage1[meta.Row], ref storage2[meta.Row],
            ref storage3[meta.Row], ref storage4[meta.Row], ref storage5[meta.Row],
            ref storage6[meta.Row], ref storage7[meta.Row]);
    }

    public void Run(Action<int, C1[], C2[], C3[], C4[], C5[], C6[], C7[]> action)
    {
        Archetypes.Lock();
        
        for (var t = 0; t < Tables.Count; t++)
        {
            var table = Tables[t];

            if (table.IsEmpty) continue;

            var storages = Storages[table.Id];

            var s1 = (C1[])storages[0];
            var s2 = (C2[])storages[1];
            var s3 = (C3[])storages[2];
            var s4 = (C4[])storages[3];
            var s5 = (C5[])storages[4];
            var s6 = (C6[])storages[5];
            var s7 = (C7[])storages[6];

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

            var storages = Storages[table.Id];
            
            var s1 = (C1[])storages[0];
            var s2 = (C2[])storages[1];
            var s3 = (C3[])storages[2];
            var s4 = (C4[])storages[3];
            var s5 = (C5[])storages[4];
            var s6 = (C6[])storages[5];
            var s7 = (C7[])storages[6];
            
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
    protected override Array[] GetStorages(Table table)
    {
        return new Array[]
        {
            table.GetStorage<C1>(Identity.None),
            table.GetStorage<C2>(Identity.None),
            table.GetStorage<C3>(Identity.None),
            table.GetStorage<C4>(Identity.None),
            table.GetStorage<C5>(Identity.None),
            table.GetStorage<C6>(Identity.None),
            table.GetStorage<C7>(Identity.None),
            table.GetStorage<C8>(Identity.None),
        };
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public RefValueTuple<C1, C2, C3, C4, C5, C6, C7, C8> Get(Entity entity)
    {
        var meta = Archetypes.GetEntityMeta(entity.Identity);
        var storages = Storages[meta.TableId];
        var storage1 = (C1[])storages[0];
        var storage2 = (C2[])storages[1];
        var storage3 = (C3[])storages[2];
        var storage4 = (C4[])storages[3];
        var storage5 = (C5[])storages[4];
        var storage6 = (C6[])storages[5];
        var storage7 = (C7[])storages[6];
        var storage8 = (C8[])storages[7];
        return new RefValueTuple<C1, C2, C3, C4, C5, C6, C7, C8>(ref storage1[meta.Row], ref storage2[meta.Row],
            ref storage3[meta.Row], ref storage4[meta.Row], ref storage5[meta.Row],
            ref storage6[meta.Row], ref storage7[meta.Row], ref storage8[meta.Row]);
    }

    public void Run(Action<int, C1[], C2[], C3[], C4[], C5[], C6[], C7[], C8[]> action)
    {
        Archetypes.Lock();
        
        for (var t = 0; t < Tables.Count; t++)
        {
            var table = Tables[t];

            if (table.IsEmpty) continue;

            var storages = Storages[table.Id];

            var s1 = (C1[])storages[0];
            var s2 = (C2[])storages[1];
            var s3 = (C3[])storages[2];
            var s4 = (C4[])storages[3];
            var s5 = (C5[])storages[4];
            var s6 = (C6[])storages[5];
            var s7 = (C7[])storages[6];
            var s8 = (C8[])storages[7];

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

            var storages = Storages[table.Id];
            
            var s1 = (C1[])storages[0];
            var s2 = (C2[])storages[1];
            var s3 = (C3[])storages[2];
            var s4 = (C4[])storages[3];
            var s5 = (C5[])storages[4];
            var s6 = (C6[])storages[5];
            var s7 = (C7[])storages[6];
            var s8 = (C8[])storages[7];
            
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
    protected override Array[] GetStorages(Table table)
    {
        return new Array[]
        {
            table.GetStorage<C1>(Identity.None),
            table.GetStorage<C2>(Identity.None),
            table.GetStorage<C3>(Identity.None),
            table.GetStorage<C4>(Identity.None),
            table.GetStorage<C5>(Identity.None),
            table.GetStorage<C6>(Identity.None),
            table.GetStorage<C7>(Identity.None),
            table.GetStorage<C8>(Identity.None),
            table.GetStorage<C9>(Identity.None),
        };
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public RefValueTuple<C1, C2, C3, C4, C5, C6, C7, C8, C9> Get(Entity entity)
    {
        var meta = Archetypes.GetEntityMeta(entity.Identity);
        var storages = Storages[meta.TableId];
        var storage1 = (C1[])storages[0];
        var storage2 = (C2[])storages[1];
        var storage3 = (C3[])storages[2];
        var storage4 = (C4[])storages[3];
        var storage5 = (C5[])storages[4];
        var storage6 = (C6[])storages[5];
        var storage7 = (C7[])storages[6];
        var storage8 = (C8[])storages[7];
        var storage9 = (C9[])storages[8];
        return new RefValueTuple<C1, C2, C3, C4, C5, C6, C7, C8, C9>(ref storage1[meta.Row], ref storage2[meta.Row],
            ref storage3[meta.Row], ref storage4[meta.Row], ref storage5[meta.Row],
            ref storage6[meta.Row], ref storage7[meta.Row], ref storage8[meta.Row], ref storage9[meta.Row]);
    }

    public void Run(Action<int, C1[], C2[], C3[], C4[], C5[], C6[], C7[], C8[], C9[]> action)
    {
        Archetypes.Lock();
        
        for (var t = 0; t < Tables.Count; t++)
        {
            var table = Tables[t];
            
            if (table.IsEmpty) continue;

            var storages = Storages[table.Id];
            
            var s1 = (C1[])storages[0];
            var s2 = (C2[])storages[1];
            var s3 = (C3[])storages[2];
            var s4 = (C4[])storages[3];
            var s5 = (C5[])storages[4];
            var s6 = (C6[])storages[5];
            var s7 = (C7[])storages[6];
            var s8 = (C8[])storages[7];
            var s9 = (C9[])storages[8];
            
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

            var storages = Storages[table.Id];
            
            var s1 = (C1[])storages[0];
            var s2 = (C2[])storages[1];
            var s3 = (C3[])storages[2];
            var s4 = (C4[])storages[3];
            var s5 = (C5[])storages[4];
            var s6 = (C6[])storages[5];
            var s7 = (C7[])storages[6];
            var s8 = (C8[])storages[7];
            var s9 = (C9[])storages[8];
            
            action(table.Count, s1, s2, s3, s4, s5, s6, s7, s8, s9);
        });
        
        Archetypes.Unlock();
    }
}