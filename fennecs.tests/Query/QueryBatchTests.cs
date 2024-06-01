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

        var intQuery = world.Query<int>().Compile();
        intQuery.Batch(Batch.AddConflict.Preserve).Add("batched").Submit();
    }


    [Fact]
    public void Can_Batch_Primitive_Types()
    {
        using var world = new World();
        world.Spawn().Add(123);

        var intQuery = world.Query<int>().Compile();
        intQuery.Batch(Batch.AddConflict.Preserve).Add(123456.0f).Submit();
        intQuery.Batch(Batch.AddConflict.Preserve).Add<float>(default).Submit();
    }


    [Fact]
    public void Can_Use_One_Shot_Batch_Ops()
    {
        using var world = new World();
        world.Spawn().Add(123);

        var intQuery = world.Query<int>().Not<float>().Compile();
        intQuery.Add(123456.0f);
        intQuery.Add<float>();
        intQuery.Remove<int>();
    }


    [Fact]
    public void Can_Batch_Struct_Types()
    {
        using var world = new World();
        world.Spawn().Add(123);

        var intQuery = world.Query<int>().Compile();
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

        var stringQuery = world.Query<string>().Compile();
        Assert.Equal(2, stringQuery.Count);
        Assert.DoesNotContain(e1, stringQuery);
        Assert.DoesNotContain(e2, stringQuery);
        Assert.Contains(e3, stringQuery);
        Assert.Contains(e4, stringQuery);

        var intQuery = world.Query<int>().Not<string>().Compile();
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

        var stringQuery = world.Query<string>().Compile();
        Assert.Contains(e1, stringQuery);
        Assert.Contains(e2, stringQuery);
        Assert.Contains(e3, stringQuery);
        Assert.Equal(3, stringQuery.Count);


        var intQuery = world.Query<int>().Not<string>().Compile();
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

        var floatQuery = world.Query<float>().Compile();
        Assert.Contains(e3, floatQuery);
    }


    [Fact]
    public void Can_Batch_Remove_Deferred()
    {
        using var world = new World();
        var e1 = world.Spawn().Add(123).Add("I must go, my people need me");
        var e2 = world.Spawn().Add(234).Add("I must go, my people need me");
        var e3 = world.Spawn().Add("lala!").Add<float>();

        var stringQuery = world.Query<string>().Compile();
        Assert.Contains(e1, stringQuery);
        Assert.Contains(e2, stringQuery);
        Assert.Contains(e3, stringQuery);
        Assert.Equal(3, stringQuery.Count);


        var intQuery = world.Query<int>().Not<string>().Compile();
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

        var floatQuery = world.Query<float>().Compile();
        Assert.Contains(e3, floatQuery);
    }


    [Fact]
    public void Can_Batch_Add_Deferred()
    {
        using var world = new World();

        var e1 = world.Spawn().Add(123);
        var e2 = world.Spawn().Add(234);
        var e3 = world.Spawn().Add("lala!");

        var stringQuery = world.Query<string>().Compile();
        Assert.Equal(1, stringQuery.Count);

        var intQuery = world.Query<int>().Compile();

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

        var floatQuery = world.Query<float>().Not<string>().Compile();

        Assert.Empty(floatQuery);

        var stringQuery = world.Query<string>().Compile();
        stringQuery.Batch(Batch.AddConflict.Preserve).Add(123f).Submit();
    }


    [Fact]
    public void Can_Create_Batched_Relation()
    {
        using var world = new World();
        var e1 = world.Spawn().Add(123);
        var e2 = world.Spawn().Add(234);
        var e3 = world.Spawn().Add("lala!");

        var stringQuery = world.Query<string>().Compile();
        Assert.Equal(1, stringQuery.Count);
        Assert.DoesNotContain(e1, stringQuery);
        Assert.DoesNotContain(e2, stringQuery);
        Assert.Contains(e3, stringQuery);

        var relationQuery = world.Query<float>(Match.Entity).Compile();
        Assert.Empty(relationQuery);

        var intQuery = world.Query<int>().Compile();
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

        var stringQuery = world.Query<string>().Compile();
        Assert.Equal(1, stringQuery.Count);
        Assert.DoesNotContain(e1, stringQuery);
        Assert.DoesNotContain(e2, stringQuery);
        Assert.Contains(e3, stringQuery);

        var relationQuery = world.Query<string>(Match.Entity).Compile();
        Assert.Empty(relationQuery);

        var intQuery = world.Query<int>().Compile();
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
        const string doom = "doom";
        
        var e1 = world.Spawn().Add(Link.With(doom));

        var linkQuery = world.Query<string>(Link.With(doom)).Compile();
        Assert.Single(linkQuery);
        Assert.Contains(e1, linkQuery);
        
        linkQuery.Batch().RemoveLink(Link.With(doom)).Submit();
        
        Assert.Empty(linkQuery);
    }

    [Fact]
    public void Can_Remove_Batched_Relation()
    {
        using var world = new World();
        var target = world.Spawn();
        var e1 = world.Spawn().AddRelation(target, 123);

        Assert.True(e1.HasRelation<int>(target));
        var intQuery = world.Query<int>(Match.Relation(target)).Compile();
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

        var stringQuery = world.Query<string>().Compile();
        Assert.Equal(1, stringQuery.Count);
        Assert.DoesNotContain(e1, stringQuery);
        Assert.DoesNotContain(e2, stringQuery);
        Assert.Contains(e3, stringQuery);

        var linkQuery = world.Query<string>(Match.Object).Compile();
        Assert.Empty(linkQuery);

        var intQuery = world.Query<int>().Compile();
        intQuery.Batch(Batch.AddConflict.Preserve).Add(Link.With("doom")).Submit();

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

        var intQuery = world.Query<int>().Compile();
        var stringQuery = world.Query<string>().Compile();

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

        var intQuery = world.Query<int>().Compile();

        Assert.Throws<InvalidOperationException>(() => intQuery.Truncate(1, fennecs.Query.TruncateMode.PerArchetype));
    }


    [Fact]
    public void Blit_1()
    {
        using var world = new World();

        var e1 = world.Spawn().Add(123);
        var e2 = world.Spawn().Add(234);
        var e3 = world.Spawn().Add(567).Add("pre-existing");

        var intQuery = world.Query<int>().Compile();
        
        intQuery.Blit(314);
        
        Assert.Equal(3, intQuery.Count);
        Assert.True(e1.Has<int>());
        Assert.Equal(314, e1.Ref<int>());
        Assert.True(e2.Has<int>());
        Assert.Equal(314, e2.Ref<int>());
        Assert.True(e3.Has<int>());
        Assert.Equal(314, e3.Ref<int>());
    }


    [Fact]
    public void Blit_2()
    {
        using var world = new World();

        var e1 = world.Spawn().Add(123).Add("jalthers");
        var e2 = world.Spawn().Add(234).Add("goofy");
        var e3 = world.Spawn().Add(567).Add("pre-existing");

        var query = world.Query<int, string>().Compile();
        
        query.Blit(314);
        query.Blit("works");
        
        Assert.Equal(3, query.Count);
        
        Assert.True(e1.Has<int>());
        Assert.Equal(314, e1.Ref<int>());
        Assert.Equal("works", e1.Ref<string>());
        
        Assert.True(e2.Has<int>());
        Assert.Equal(314, e2.Ref<int>());
        Assert.Equal("works", e2.Ref<string>());
        
        Assert.True(e3.Has<int>());
        Assert.Equal(314, e3.Ref<int>());
        Assert.Equal("works", e3.Ref<string>());
    }


    [Fact]
    public void Blit_Empty_Query()
    {
        using var world = new World();

        world.Spawn().Add(123.5f);

        var query = world.Query<int, string>().Compile();
        
        query.Blit(314);
        query.Blit("works");
        
        Assert.Equal(0, query.Count);
    }


    [Fact]
    public void Can_Batch_Add_Preserve()
    {
        using var world = new World();

        var e1 = world.Spawn().Add(123);
        var e2 = world.Spawn().Add(234);
        var e3 = world.Spawn().Add(567).Add("pre-existing");

        // Added here because there was an issue below that currupted e3's meta,
        // and needed to ruled that out as a precondition.
        Assert.Equal("pre-existing", e3.Ref<string>());

        var intQuery = world.Query<int>().Compile();
        Assert.Equal(3, intQuery.Count);

        // ! no lock !
        intQuery.Batch(Batch.AddConflict.Preserve).Add("batched").Submit();
        // ! no lock !

        Assert.Equal(3, intQuery.Count);
        Assert.True(e1.Has<string>());
        Assert.Equal("batched", e1.Ref<string>());
        Assert.True(e2.Has<string>());
        Assert.Equal("batched", e2.Ref<string>());
        Assert.True(e3.Has<string>());
        Assert.Equal("pre-existing", e3.Ref<string>());
    }


    [Fact]
    public void Can_Batch_Add_Replace()
    {
        using var world = new World();

        var e1 = world.Spawn().Add(123);
        var e2 = world.Spawn().Add(234);
        var e3 = world.Spawn().Add(567).Add("pre-existing");

        // Added here because ther was an issue below that currupted e3's meta,
        // and needed to ruled that out as a precondition.
        Assert.Equal("pre-existing", e3.Ref<string>());

        var intQuery = world.Query<int>().Compile();
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

        var intQuery = world.Query<int>().Compile();
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

        var stringQuery = world.Query<string>().Compile();
        Assert.Contains(e1, stringQuery);
        Assert.Contains(e2, stringQuery);
        Assert.Contains(e3, stringQuery);
        Assert.Equal(3, stringQuery.Count);

        var notStringQuery = world.Query().Not<string>().Compile();
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
        var stringQuery = world.Query<string>().Compile();
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
        var stringQuery = world.Query<string>().Has<float>().Compile();
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
        var stringQuery = world.Query<string>().Not<float>().Compile();
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
        var stringQuery = world.Query<string>().Compile();
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
        var stringQuery = world.Query<string>().Not<float>().Compile();
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
        var stringQuery = world.Query<string>().Compile();
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
        var stringQuery = world.Query<string>().Compile();

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
        var stringQuery = world.Query<string>().Compile();

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

        var stringQuery = world.Query<string>().Compile();
        Assert.Equal(1, stringQuery.Count);
        Assert.DoesNotContain(e1, stringQuery);
        Assert.DoesNotContain(e2, stringQuery);
        Assert.Contains(e3, stringQuery);

        stringQuery.Batch(Batch.AddConflict.Preserve).Add<int>().Submit();

        Assert.Equal(1, stringQuery.Count);
        Assert.Contains(e3, stringQuery);

        var intQuery = world.Query<int>().Compile();
        Assert.Equal(3, intQuery.Count);
        Assert.Contains(e1, intQuery);
        Assert.Contains(e2, intQuery);
        Assert.Contains(e3, intQuery);
    }
}