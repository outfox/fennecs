using fennecs.tests.Util.Components;

namespace fennecs.tests.Stream;

public class StreamProceduralTests
{
    
    [Fact]
    public void Can_Read_Value_Components()
    {
        using var world = new World();

        // Creates an entity...
        var entity = world.Spawn()
            .Add(new Val0(0))
            .Add(new Val1(1))
            .Add(new Val2(2))
            .Add(new Val3(3))
            .Add(new Val4(4))
            .Add(new Val5(5));

        // ... and an empty group of Archetypes (Val0 is not queried)
        entity.Remove<Val0>();

        ReadEqualsWrite(world, entity);
            
        StreamRead1(world, entity);
        StreamRead2(world, entity);
        StreamRead3(world, entity);
        StreamRead4(world, entity);
        StreamRead5(world, entity);
    }

    [Fact]
    public void Can_Read_Reference_Components()
    {
        using var world = new World();
        var ref1 = new Ref1();
        var ref2 = new Ref2();
        var ref3 = new Ref3();
        var ref4 = new Ref4();
        var ref5 = new Ref5();

        var entity = world.Spawn()
            .Add(ref1)
            .Add(ref2)
            .Add(ref3)
            .Add(ref4)
            .Add(ref5);

        // ReSharper disable method ParameterOnlyUsedForPreconditionCheck.Local
        world.Stream<Ref1, Ref2, Ref3, Ref4, Ref5>().For((
            e,
            v1,
            v2,
            v3,
            v4,
            v5
        ) =>
        {
            Assert.Equal(entity, e);
            Assert.Equal(ref1, v1);
            Assert.Equal(ref2, v2);
            Assert.Equal(ref3, v3);
            Assert.Equal(ref4, v4);
            Assert.Equal(ref5, v5);
        });

        world.Stream<Ref1, Ref2, Ref3, Ref4, Ref5>().For((
            v1,
            v2,
            v3,
            v4,
            v5
        ) =>
        {
            Assert.Equal(ref1, v1);
            Assert.Equal(ref2, v2);
            Assert.Equal(ref3, v3);
            Assert.Equal(ref4, v4);
            Assert.Equal(ref5, v5);
        });
    }

    [Fact]
    public void Can_Write_Reference_Components()
    {
        using var world = new World();
        var ref1 = new Ref1();
        var ref2 = new Ref2();
        var ref3 = new Ref3();
        var ref4 = new Ref4();
        var ref5 = new Ref5();

        var _ = world.Spawn()
            .Add(ref1)
            .Add(ref2)
            .Add(ref3)
            .Add(ref4)
            .Add(ref5);

        // ReSharper disable method ParameterOnlyUsedForPreconditionCheck.Local
        world.Stream<Ref1, Ref2, Ref3, Ref4, Ref5>().For((
            v1,
            v2,
            v3,
            v4,
            v5
        ) =>
        {
            v1.write = new Ref1(6);
            v2.write = new Ref2(7);
            v3.write = new Ref3(8);
            v4.write = new Ref4(9);
            v5.write = new Ref5(10);

            ref1 = v1.read;
            ref2 = v2.read;
            ref3 = v3.read;
            ref4 = v4.read;
            ref5 = v5.read;
        });

        Assert.Equal(6, ref1.Value);
        Assert.Equal(7, ref2.Value);
        Assert.Equal(8, ref3.Value);
        Assert.Equal(9, ref4.Value);
        Assert.Equal(10, ref5.Value);

        world.Stream<Ref1, Ref2, Ref3, Ref4, Ref5>().For((
            v1,
            v2,
            v3,
            v4,
            v5
        ) =>
        {
            v1.write.Value = 11;
            v2.write.Value = 12;
            v3.write.Value = 13;
            v4.write.Value = 14;
            v5.write.Value = 15;
        });

        Assert.Equal(11, ref1.Value);
        Assert.Equal(12, ref2.Value);
        Assert.Equal(13, ref3.Value);
        Assert.Equal(14, ref4.Value);
        Assert.Equal(15, ref5.Value);
    }


    #region Stream Read

    // ReSharper disable method ParameterOnlyUsedForPreconditionCheck.Local

    private static void StreamRead1(World world, Entity entity)
    {
        world.Stream<Val1>().For((e, v1) =>
        {
            Assert.Equal(entity, e);
            Assert.Equal(1, v1.read.Value);
        });

        world.Stream<Val1>().For((v1) => { Assert.Equal(1, v1.read.Value); });
    }

    private static void StreamRead2(World world, Entity entity)
    {
        world.Stream<Val1, Val2>().For((e, v1, v2) =>
        {
            Assert.Equal(entity, e);
            Assert.Equal(1, v1.read.Value);
            Assert.Equal(2, v2.read.Value);
        });

        world.Stream<Val1, Val2>().For((v1, v2) =>
        {
            Assert.Equal(1, v1.read.Value);
            Assert.Equal(2, v2.read.Value);
        });
    }

    private static void StreamRead3(World world, Entity entity)
    {
        world.Stream<Val1, Val2, Val3>().For((e, v1, v2, v3) =>
        {
            Assert.Equal(entity, e);
            Assert.Equal(1, v1.read.Value);
            Assert.Equal(2, v2.read.Value);
            Assert.Equal(3, v3.read.Value);
        });

        world.Stream<Val1, Val2, Val3>().For((v1, v2, v3) =>
        {
            Assert.Equal(1, v1.read.Value);
            Assert.Equal(2, v2.read.Value);
            Assert.Equal(3, v3.read.Value);
        });
    }

    private static void StreamRead4(World world, Entity entity)
    {
        world.Stream<Val1, Val2, Val3, Val4>().For((e, v1, v2, v3, v4) =>
        {
            Assert.Equal(entity, e);
            Assert.Equal(1, v1.read.Value);
            Assert.Equal(2, v2.read.Value);
            Assert.Equal(3, v3.read.Value);
            Assert.Equal(4, v4.read.Value);
        });

        world.Stream<Val1, Val2, Val3, Val4>().For((v1, v2, v3, v4) =>
        {
            Assert.Equal(1, v1.read.Value);
            Assert.Equal(2, v2.read.Value);
            Assert.Equal(3, v3.read.Value);
            Assert.Equal(4, v4.read.Value);
        });

        //Permutation
        world.Stream<Val5, Val2, Val3, Val4>().For((e, v5, v2, v3, v4) =>
        {
            Assert.Equal(entity, e);
            Assert.Equal(5, v5.read.Value);
            Assert.Equal(2, v2.read.Value);
            Assert.Equal(3, v3.read.Value);
            Assert.Equal(4, v4.read.Value);
        });

        world.Stream<Val3, Val4, Val5, Val2>().For((v3, v4, v5, v2) =>
        {
            Assert.Equal(5, v5.read.Value);
            Assert.Equal(2, v2.read.Value);
            Assert.Equal(3, v3.read.Value);
            Assert.Equal(4, v4.read.Value);
        });
    }

    private static void ReadEqualsWrite(World world, Entity entity)
    {
        world.Stream<Ref1, Ref2, Ref3, Ref4, Ref5>().For((
            v1,
            v2,
            v3,
            v4,
            v5
        ) =>
        {
            Assert.Equal(v1.write.Value, v1.read.Value);
            Assert.Equal(v2.write.Value, v2.read.Value);
            Assert.Equal(v3.write.Value, v3.read.Value);
            Assert.Equal(v4.write.Value, v4.read.Value);
            Assert.Equal(v5.write.Value, v5.read.Value);

            Assert.Equal(v1.read.Value, v1.write.Value);
            Assert.Equal(v2.read.Value, v2.write.Value);
            Assert.Equal(v3.read.Value, v3.write.Value);
            Assert.Equal(v4.read.Value, v4.write.Value);
            Assert.Equal(v5.read.Value, v5.write.Value);

            Assert.Equal(v1.write, v1.read);
            Assert.Equal(v2.write, v2.read);
            Assert.Equal(v3.write, v3.read);
            Assert.Equal(v4.write, v4.read);
            Assert.Equal(v5.write, v5.read);

            Assert.Equal(v1.read, v1.write);
            Assert.Equal(v2.read, v2.write);
            Assert.Equal(v3.read, v3.write);
            Assert.Equal(v4.read, v4.write);
            Assert.Equal(v5.read, v5.write);
        });
    }


    private static void StreamRead5(World world, Entity entity)
    {
        world.Stream<Val1, Val2, Val3, Val4, Val5>().For((e, v1, v2, v3, v4, v5) =>
        {
            Assert.Equal(entity, e);
            Assert.Equal(1, v1.read.Value);
            Assert.Equal(2, v2.read.Value);
            Assert.Equal(3, v3.read.Value);
            Assert.Equal(4, v4.read.Value);
            Assert.Equal(5, v5.read.Value);
        });

        world.Stream<Val1, Val2, Val3, Val4, Val5>().For((v1, v2, v3, v4, v5) =>
        {
            Assert.Equal(1, v1.read.Value);
            Assert.Equal(2, v2.read.Value);
            Assert.Equal(3, v3.read.Value);
            Assert.Equal(4, v4.read.Value);
            Assert.Equal(5, v5.read.Value);
        });
    }

    #endregion
}