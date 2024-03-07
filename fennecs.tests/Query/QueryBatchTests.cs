namespace fennecs.tests.Query;

public class QueryBatchTests
{
    private struct TypeA(int i)
    {
        // ReSharper disable once UnusedMember.Local
        public long _1 = i;
    }
    
    [Fact]
    public void Can_Batch_Reference_Types()
    {
        using var world = new World();
        world.Spawn().Add(123);

        var intQuery = world.Query<int>().Build();
        intQuery.Batch(World.BatchOperation.AddConflictMode.Skip).Add("batched").Submit();
    }


    [Fact]
    public void Can_Batch_Primitive_Types()
    {
        using var world = new World();
        world.Spawn().Add(123);

        var intQuery = world.Query<int>().Build();
        intQuery.Batch(World.BatchOperation.AddConflictMode.Skip).Add(123456.0f).Submit();
        intQuery.Batch(World.BatchOperation.AddConflictMode.Skip).Add<float>(default).Submit();
    }


    [Fact]
    public void Can_Batch_Struct_Types()
    {
        using var world = new World();
        world.Spawn().Add(123);

        var intQuery = world.Query<int>().Build();
        intQuery.Batch(World.BatchOperation.AddConflictMode.Skip).Add(new TypeA(55)).Submit();
        intQuery.Batch(World.BatchOperation.AddConflictMode.Skip).Add<TypeA>(default).Submit();
    }

    
    [Fact]
    public void Can_Batch_Add_Immediate()
    {
        using var world = new World();
        var e1 = world.Spawn().Add(123);
        var e2 = world.Spawn().Add(234);
        var e3 = world.Spawn().Add("lala!");

        var stringQuery = world.Query<string>().Build();
        Assert.Equal(1, stringQuery.Count);
        Assert.DoesNotContain(e1, stringQuery);
        Assert.DoesNotContain(e2, stringQuery);
        Assert.Contains(e3, stringQuery);

        var intQuery = world.Query<int>().Build();
        intQuery.Batch(World.BatchOperation.AddConflictMode.Skip).Add("batched").Submit();

        Assert.Equal(3, stringQuery.Count);

        Assert.Contains(e1, stringQuery);
        Assert.Contains(e2, stringQuery);
        Assert.Contains(e3, stringQuery);
    }


    [Fact]
    public void Can_Batch_Remove_Immediate()
    {
        using var world = new World();
        var e1 = world.Spawn().Add(123).Add("I must go, my people need me");
        var e2 = world.Spawn().Add(234).Add("I must go, my people need me");
        var e3 = world.Spawn().Add("lala!").Add<float>();

        var stringQuery = world.Query<string>().Build();
        Assert.Contains(e1, stringQuery);
        Assert.Contains(e2, stringQuery);
        Assert.Contains(e3, stringQuery);
        Assert.Equal(3, stringQuery.Count);


        var intQuery = world.Query<int>().Not<string>().Build();
        Assert.DoesNotContain(e1, intQuery);
        Assert.DoesNotContain(e2, intQuery);
        Assert.DoesNotContain(e3, intQuery);

        stringQuery.Batch().Remove<string>().Submit();

        Assert.Equal(0, stringQuery.Count);
        Assert.Equal(2, intQuery.Count);

        Assert.DoesNotContain(e1, stringQuery);
        Assert.DoesNotContain(e2, stringQuery);
        Assert.DoesNotContain(e3, stringQuery);

        Assert.Contains(e1, intQuery);
        Assert.Contains(e2, intQuery);
        Assert.DoesNotContain(e3, intQuery);

        var floatQuery = world.Query<float>().Build();
        Assert.Contains(e3, floatQuery);
    }


    [Fact]
    public void Can_Batch_Remove_Deferred()
    {
        using var world = new World();
        var e1 = world.Spawn().Add(123).Add("I must go, my people need me");
        var e2 = world.Spawn().Add(234).Add("I must go, my people need me");
        var e3 = world.Spawn().Add("lala!").Add<float>();

        var stringQuery = world.Query<string>().Build();
        Assert.Contains(e1, stringQuery);
        Assert.Contains(e2, stringQuery);
        Assert.Contains(e3, stringQuery);
        Assert.Equal(3, stringQuery.Count);


        var intQuery = world.Query<int>().Not<string>().Build();
        Assert.DoesNotContain(e1, intQuery);
        Assert.DoesNotContain(e2, intQuery);
        Assert.DoesNotContain(e3, intQuery);

        var worldLock = world.Lock;
        stringQuery.Batch().Remove<string>().Submit();

        // Deferred operations are not immediately visible
        Assert.DoesNotContain(e1, intQuery);
        Assert.DoesNotContain(e2, intQuery);
        Assert.DoesNotContain(e3, intQuery);

        worldLock.Dispose();
        Assert.Equal(0, stringQuery.Count);
        Assert.Equal(2, intQuery.Count);

        Assert.DoesNotContain(e1, stringQuery);
        Assert.DoesNotContain(e2, stringQuery);
        Assert.DoesNotContain(e3, stringQuery);

        Assert.Contains(e1, intQuery);
        Assert.Contains(e2, intQuery);
        Assert.DoesNotContain(e3, intQuery);

        var floatQuery = world.Query<float>().Build();
        Assert.Contains(e3, floatQuery);
    }


    [Fact]
    public void Can_Batch_Add_Deferred()
    {
        using var world = new World();

        var e1 = world.Spawn().Add(123);
        var e2 = world.Spawn().Add(234);
        var e3 = world.Spawn().Add("lala!");

        var stringQuery = world.Query<string>().Build();
        Assert.Equal(1, stringQuery.Count);

        var intQuery = world.Query<int>().Build();

        var worldLock = world.Lock;
        intQuery.Batch(World.BatchOperation.AddConflictMode.Skip).Add("batched").Submit();

        // Deferred operations are not immediately visible
        Assert.DoesNotContain(e1, stringQuery);
        Assert.DoesNotContain(e2, stringQuery);
        Assert.Contains(e3, stringQuery);

        worldLock.Dispose();

        Assert.Equal(3, stringQuery.Count);

        Assert.Contains(e1, stringQuery);
        Assert.Contains(e2, stringQuery);
        Assert.Contains(e3, stringQuery);
    }


    [Fact]
    public void Can_Batch_Mixed_Deferred()
    {
        using var world = new World();
        world.Spawn().Add(123).Add("I must go, my people need me");
        world.Spawn().Add(234).Add("I must go, my people need me");
        world.Spawn().Add("lala!").Add<float>();

        var floatQuery = world.Query<float>().Not<string>().Build();

        Assert.Empty(floatQuery);

        var stringQuery = world.Query<string>().Build();
        stringQuery.Batch(World.BatchOperation.AddConflictMode.Skip).Add(123f).Submit();
    }


    [Fact]
    public void Can_Create_Batched_Relation()
    {
        using var world = new World();
        var e1 = world.Spawn().Add(123);
        var e2 = world.Spawn().Add(234);
        var e3 = world.Spawn().Add("lala!");

        var stringQuery = world.Query<string>().Build();
        Assert.Equal(1, stringQuery.Count);
        Assert.DoesNotContain(e1, stringQuery);
        Assert.DoesNotContain(e2, stringQuery);
        Assert.Contains(e3, stringQuery);

        var relationQuery = world.Query<float>(Match.Entity).Build();
        Assert.Empty(relationQuery);

        var intQuery = world.Query<int>().Build();
        intQuery.Batch(World.BatchOperation.AddConflictMode.Skip).AddRelation<float>(e3).Submit();

        Assert.Equal(2, relationQuery.Count);
        Assert.Contains(e1, relationQuery);
        Assert.Contains(e2, relationQuery);
        Assert.DoesNotContain(e3, relationQuery);
    }


    [Fact]
    public void Can_Create_Batched_Link()
    {
        using var world = new World();
        var e1 = world.Spawn().Add(123);
        var e2 = world.Spawn().Add(234);
        var e3 = world.Spawn().Add("lala!");

        var stringQuery = world.Query<string>().Build();
        Assert.Equal(1, stringQuery.Count);
        Assert.DoesNotContain(e1, stringQuery);
        Assert.DoesNotContain(e2, stringQuery);
        Assert.Contains(e3, stringQuery);

        var linkQuery = world.Query<string>(Match.Object).Build();
        Assert.Empty(linkQuery);

        var intQuery = world.Query<int>().Build();
        intQuery.Batch(World.BatchOperation.AddConflictMode.Skip).AddLink<string>("doom").Submit();

        Assert.Equal(2, linkQuery.Count);
        Assert.Contains(e1, linkQuery);
        Assert.Contains(e2, linkQuery);
        Assert.DoesNotContain(e3, linkQuery);
    }


    [Fact]
    public void Can_Truncate_PerArchetype()
    {
        using var world = new World();
        world.Spawn().Add(123);
        world.Spawn().Add(234);
        world.Spawn().Add(123).Add("Archetype 2");
        world.Spawn().Add(234).Add("Archetype 2");

        var intQuery = world.Query<int>().Build();
        var stringQuery = world.Query<string>().Build();

        Assert.Equal(4, intQuery.Count);
        Assert.Equal(2, stringQuery.Count);

        intQuery.Truncate(1, fennecs.Query.TruncateMode.PerArchetype);

        Assert.Equal(2, intQuery.Count);
        Assert.Equal(1, stringQuery.Count);
    }


    [Fact]
    public void Cannot_Truncate_When_Locked()
    {
        using var world = new World();
        
        using var _ = world.Lock;
        
        world.Spawn().Add(123);
        world.Spawn().Add(234);
        world.Spawn().Add(123).Add("Archetype 2");
        world.Spawn().Add(234).Add("Archetype 2");

        var intQuery = world.Query<int>().Build();

        Assert.Throws<InvalidOperationException>(() => intQuery.Truncate(1, fennecs.Query.TruncateMode.PerArchetype));
    }



    [Fact]
    public void Can_Batch_Add_Replace_Deferred()
    {
        using var world = new World();

        var e1 = world.Spawn().Add(123);
        var e2 = world.Spawn().Add(234);
        var e3 = world.Spawn().Add(567).Add("lala!");

        var intQuery = world.Query<int>().Build();
        Assert.Equal(3, intQuery.Count);

        //var worldLock = world.Lock;
        intQuery.Batch(World.BatchOperation.AddConflictMode.Replace).Add("batched").Submit();
        //worldLock.Dispose();

        Assert.Equal(3, intQuery.Count);
        Assert.True(e1.Has<string>());
        Assert.Equal("batched", e1.Ref<string>());
        Assert.True(e2.Has<string>());
        Assert.Equal("batched", e2.Ref<string>());
        Assert.True(e3.Has<string>());
        Assert.Equal("batched", e3.Ref<string>());
        
    }


    
}