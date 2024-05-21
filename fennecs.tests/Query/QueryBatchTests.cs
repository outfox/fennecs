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
        intQuery.Batch(Batch.AddConflict.Preserve).Add("batched").Submit();
    }


    [Fact]
    public void Can_Batch_Primitive_Types()
    {
        using var world = new World();
        world.Spawn().Add(123);

        var intQuery = world.Query<int>().Build();
        intQuery.Batch(Batch.AddConflict.Preserve).Add(123456.0f).Submit();
        intQuery.Batch(Batch.AddConflict.Preserve).Add<float>(default).Submit();
    }


    [Fact]
    public void Can_Use_One_Shot_Batch_Ops()
    {
        using var world = new World();
        world.Spawn().Add(123);

        var intQuery = world.Query<int>().Not<float>().Build();
        intQuery.Add(123456.0f);
        intQuery.Add<float>();
        intQuery.Remove<int>();
    }


    [Fact]
    public void Can_Batch_Struct_Types()
    {
        using var world = new World();
        world.Spawn().Add(123);

        var intQuery = world.Query<int>().Build();
        intQuery.Batch(Batch.AddConflict.Preserve).Add(new TypeA(55)).Submit();
        intQuery.Batch(Batch.AddConflict.Preserve).Add<TypeA>(default).Submit();
    }


    [Fact]
    public void Can_Batch_Add_Immediate()
    {
        using var world = new World();
        var e1 = world.Spawn().Add(123);
        var e2 = world.Spawn().Add(234);
        var e3 = world.Spawn().Add("lala!");
        var e4 = world.Spawn().Add(69).Add("sixty-nine!");

        var stringQuery = world.Query<string>().Build();
        Assert.Equal(2, stringQuery.Count);
        Assert.DoesNotContain(e1, stringQuery);
        Assert.DoesNotContain(e2, stringQuery);
        Assert.Contains(e3, stringQuery);
        Assert.Contains(e4, stringQuery);

        var intQuery = world.Query<int>().Not<string>().Build();
        intQuery.Batch(Batch.AddConflict.Replace).Add("batched").Submit();

        Assert.Equal(4, stringQuery.Count);

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

        var worldLock = world.Lock();
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

        var worldLock = world.Lock();
        intQuery.Batch(Batch.AddConflict.Preserve).Add("batched").Submit();

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
        stringQuery.Batch(Batch.AddConflict.Preserve).Add(123f).Submit();
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
        intQuery.Batch(Batch.AddConflict.Preserve).AddRelation<float>(e3).Submit();

        Assert.Equal(2, relationQuery.Count);
        Assert.Contains(e1, relationQuery);
        Assert.Contains(e2, relationQuery);
        Assert.DoesNotContain(e3, relationQuery);
    }


    [Fact]
    public void Can_Create_Batched_ObjectBacked_Relation()
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

        var relationQuery = world.Query<string>(Match.Entity).Build();
        Assert.Empty(relationQuery);

        var intQuery = world.Query<int>().Build();
        intQuery.Batch(Batch.AddConflict.Preserve).AddRelation<string>("object backed, buddy!", e3).Submit();

        Assert.Equal(2, relationQuery.Count);
        Assert.Contains(e1, relationQuery);
        Assert.Contains(e2, relationQuery);
        Assert.DoesNotContain(e3, relationQuery);
    }


    [Fact]
    public void Can_RemoveLink_Batched()
    {
        using var world = new World();
        const string link = "doom";
        
        var e1 = world.Spawn().AddLink<string>(link);

        var linkQuery = world.Query<string>(Identity.Of(link)).Build();
        Assert.Single(linkQuery);
        Assert.Contains(e1, linkQuery);
        
        linkQuery.Batch().RemoveLink<string>(link).Submit();
        
        Assert.Empty(linkQuery);
    }

    [Fact]
    public void Can_Remove_Batched_Relation()
    {
        using var world = new World();
        var target = world.Spawn();
        var e1 = world.Spawn().AddRelation(target, 123);

        Assert.True(e1.HasRelation<int>(target));
        var intQuery = world.Query<int>(target).Build();
        intQuery.Batch().RemoveRelation<int>(target).Submit();
        Assert.False(e1.HasRelation<int>(target));
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
        intQuery.Batch(Batch.AddConflict.Preserve).AddLink<string>("doom").Submit();

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

        using var _ = world.Lock();

        world.Spawn().Add(123);
        world.Spawn().Add(234);
        world.Spawn().Add(123).Add("Archetype 2");
        world.Spawn().Add(234).Add("Archetype 2");

        var intQuery = world.Query<int>().Build();

        Assert.Throws<InvalidOperationException>(() => intQuery.Truncate(1, fennecs.Query.TruncateMode.PerArchetype));
    }


    [Fact]
    public void Can_Batch_Add_Replace()
    {
        using var world = new World();

        var e1 = world.Spawn().Add(123);
        var e2 = world.Spawn().Add(234);
        var e3 = world.Spawn().Add(567).Add("lala!");

        var intQuery = world.Query<int>().Build();
        Assert.Equal(3, intQuery.Count);

        // ! no lock !
        intQuery.Batch(Batch.AddConflict.Replace).Add("batched").Submit();
        // ! no lock !

        Assert.Equal(3, intQuery.Count);
        Assert.True(e1.Has<string>());
        Assert.Equal("batched", e1.Ref<string>());
        Assert.True(e2.Has<string>());
        Assert.Equal("batched", e2.Ref<string>());
        Assert.True(e3.Has<string>());
        Assert.Equal("batched", e3.Ref<string>());
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

        var worldLock = world.Lock();
        intQuery.Batch(Batch.AddConflict.Replace).Add("batched").Submit();
        worldLock.Dispose();

        Assert.Equal(3, intQuery.Count);
        Assert.True(e1.Has<string>());
        Assert.Equal("batched", e1.Ref<string>());
        Assert.True(e2.Has<string>());
        Assert.Equal("batched", e2.Ref<string>());
        Assert.True(e3.Has<string>());
        Assert.Equal("batched", e3.Ref<string>());
    }


    [Fact]
    public void Can_Batch_Remove_Immediate_Alternate()
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

        var notStringQuery = world.Query().Not<string>().Build();
        Assert.DoesNotContain(e1, notStringQuery);
        Assert.DoesNotContain(e2, notStringQuery);
        Assert.DoesNotContain(e3, notStringQuery);

        stringQuery.Remove<string>();
        Assert.Equal(0, stringQuery.Count);

        Assert.DoesNotContain(e1, stringQuery);
        Assert.DoesNotContain(e2, stringQuery);
        Assert.DoesNotContain(e3, stringQuery);

        Assert.Contains(e1, notStringQuery);
        Assert.Contains(e2, notStringQuery);
        Assert.Contains(e3, notStringQuery);
    }


    [Fact]
    public void Cannot_Remove_Add_Conflict_with_Disallow()
    {
        using var world = new World();
        var stringQuery = world.Query<string>().Build();
        Assert.Throws<InvalidOperationException>(() =>
        {
            stringQuery.Batch(Batch.AddConflict.Strict)
                .Add<float>()
                .Submit();
        });
    }


    [Fact]
    public void Cannot_Duplicate_Remove()
    {
        using var world = new World();
        var stringQuery = world.Query<string>().Has<float>().Build();
        Assert.Throws<InvalidOperationException>(() =>
        {
            stringQuery.Batch(Batch.RemoveConflict.Strict)
                .Remove<float>()
                .Remove<float>()
                .Submit();
        });
        Assert.Throws<InvalidOperationException>(() =>
        {
            stringQuery.Batch(Batch.RemoveConflict.Allow)
                .Remove<float>()
                .Remove<float>()
                .Submit();
        });
    }


    [Fact]
    public void Cannot_Duplicate_Add()
    {
        using var world = new World();
        var stringQuery = world.Query<string>().Not<float>().Build();
        Assert.Throws<InvalidOperationException>(() =>
        {
            stringQuery.Batch(Batch.AddConflict.Strict)
                .Add<float>()
                .Add<float>()
                .Submit();
        });
        Assert.Throws<InvalidOperationException>(() =>
        {
            stringQuery.Batch(Batch.AddConflict.Preserve)
                .Add<float>()
                .Add<float>()
                .Submit();
        });
        Assert.Throws<InvalidOperationException>(() =>
        {
            stringQuery.Batch(Batch.AddConflict.Replace)
                .Add<float>()
                .Add<float>()
                .Submit();
        });
    }


    [Fact]
    public void Cannot_Remove_and_Add()
    {
        using var world = new World();
        var stringQuery = world.Query<string>().Build();
        Assert.Throws<InvalidOperationException>(() =>
        {
            stringQuery.Batch(Batch.AddConflict.Strict)
                .Remove<string>()
                .Add<string>("lala!")
                .Submit();
        });
        Assert.Throws<InvalidOperationException>(() =>
        {
            stringQuery.Batch(Batch.AddConflict.Preserve)
                .Remove<string>()
                .Add<string>("lala!")
                .Submit();
        });
        Assert.Throws<InvalidOperationException>(() =>
        {
            stringQuery.Batch(Batch.AddConflict.Replace)
                .Remove<string>()
                .Add<string>("lala!")
                .Submit();
        });
    }


    [Fact]
    public void Cannot_Add_and_Remove()
    {
        using var world = new World();
        var stringQuery = world.Query<string>().Not<float>().Build();
        Assert.Throws<InvalidOperationException>(() =>
        {
            stringQuery.Batch(Batch.AddConflict.Strict, Batch.RemoveConflict.Allow)
                .Add(55.5f)
                .Remove<float>() //this fails because of the wrong reason, but is ok.
                .Submit();
        });
        Assert.Throws<InvalidOperationException>(() =>
        {
            stringQuery.Batch(Batch.AddConflict.Preserve, Batch.RemoveConflict.Allow)
                .Add(55.5f)
                .Remove<float>()
                .Submit();
        });
        Assert.Throws<InvalidOperationException>(() =>
        {
            stringQuery.Batch(Batch.AddConflict.Replace, Batch.RemoveConflict.Allow)
                .Add(55.5f)
                .Remove<float>()
                .Submit();
        });
    }


    [Fact]
    public void Cannot_Remove_Conflict_with_Disallow()
    {
        using var world = new World();
        var stringQuery = world.Query<string>().Build();
        Assert.Throws<InvalidOperationException>(() =>
        {
            stringQuery.Batch(Batch.RemoveConflict.Strict)
                .Remove<float>()
                .Submit();
        });
    }


    [Fact]
    public void Can_Remove_Conflict_with_Skip()
    {
        using var world = new World();
        world.Spawn().Add(123).Add("I must go, my people need me");
        var stringQuery = world.Query<string>().Build();

        stringQuery
            .Batch(Batch.RemoveConflict.Allow)
            .Remove<string>()
            .Remove<float>()
            .Submit();
    }


    [Fact]
    public void Can_Add_Remove_Conflict_with_Skip_Allow()
    {
        using var world = new World();
        world.Spawn().Add(123).Add("I must go, my people need me");
        var stringQuery = world.Query<string>().Build();

        stringQuery
            .Batch(Batch.AddConflict.Preserve, Batch.RemoveConflict.Allow)
            .Remove<string>()
            .Remove<float>()
            .Submit();
    }


    [Fact]
    public void Can_Add_With_Newable()
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

        stringQuery.Batch(Batch.AddConflict.Preserve).Add<int>().Submit();

        Assert.Equal(1, stringQuery.Count);
        Assert.Contains(e3, stringQuery);

        var intQuery = world.Query<int>().Build();
        Assert.Equal(3, intQuery.Count);
        Assert.Contains(e1, intQuery);
        Assert.Contains(e2, intQuery);
        Assert.Contains(e3, intQuery);
    }
}