using System.Collections;
using System.Numerics;

namespace fennecs.tests.Stream;

// ReSharper disable ParameterOnlyUsedForPreconditionCheck.Local
// ReSharper disable once ClassNeverInstantiated.Global
public class Stream5Tests(ITestOutputHelper output)
{
    [Fact]
    public void Can_Enumerate_Stream()
    {
        using var world = new World();
        var arnold = world.Spawn().Add("Arnold").Add(1).Add(7.0f).Add('x').Add(5d);
        var dolph = world.Spawn().Add("Dolph").Add(2).Add(8.0f).Add('y').Add(6d);
        
        List<(Entity, string, int, float, char, double)> list = [(arnold, "Arnold", 1, 7.0f, 'x', 5d), (dolph, "Dolph", 2, 8.0f, 'y', 6d)];
        
        var stream = world.Stream<string, int, float, char, double>();
        foreach (var row in stream)
        {
            Assert.True(list.Remove(row));
        }
        
        Assert.Empty(list);
    }


    [Fact]
    public void Can_Enumerate_Stream_Boxed()
    {
        using var world = new World();
        var arnold = world.Spawn().Add("Arnold").Add(1).Add(7.0f).Add('x').Add(5d);
        var dolph = world.Spawn().Add("Dolph").Add(2).Add(8.0f).Add('y').Add(6d);
        
        List<object> list = [(arnold, "Arnold", 1, 7.0f, 'x', 5d), (dolph, "Dolph", 2, 8.0f, 'y', 6d)];
        
        IEnumerable stream = world.Stream<string, int, float, char, double>();
        foreach (var row in stream)
        {
            Assert.True(list.Remove(row));
        }
        
        Assert.Empty(list);
    }
    
    [Fact]
    public void Cannot_Structural_Change_While_Enumerating()
    {
        using var world = new World();
        world.Spawn().Add("Arnold").Add(1).Add(7.0f).Add('x').Add(5d);
        world.Spawn().Add("Dolph").Add(2).Add(8.0f).Add('y').Add(6d);

        var stream = world.Stream<string, int, float, char, double>();
        Assert.Throws<InvalidOperationException>(() =>
        {
            foreach (var row in stream)
            {
                row.Item1.Remove<int>();
            }
        });
    }
    [Theory]
    [ClassData(typeof(QueryCountGenerator))]
    private void All_Runners_Applicable(int count, bool createEmptyTable)
    {
        using var world = new World();

        var query = world.Query<TypeA, double, int, string, char>().Stream();

        //Create an empty table by spawning and despawning a single entity
        //that matches our test Query (but is a larger Archetype)
        if (createEmptyTable)
        {
            var dead = world.Spawn().Add<int>().Add<TypeA>().Add<char>().Add<double>().Add(0.25f).Add("will be removed");
            world.Despawn(dead);
        }

        for (var index = 0; index < count; index++)
        {
            Assert.Equal(index, query.Count);

            var e = world.Spawn();
                e.Add(new TypeA(){entity = e})
                .Add(99.999)
                .Add(index)
                .Add("one")
                .Add('Q');
        }

        query.For(static (ref TypeA _, ref double _, ref int _, ref string str, ref char _) =>
        {
            Assert.Equal("one", str);
            str = "two";
        });

        query.Raw((_, _, integers, strings, _) =>
        {
            for (var i = 0; i < count; i++)
            {
                Assert.Equal(i, integers.Span[i]);
                Assert.Equal("two", strings.Span[i]);
                strings.Span[i] = "three";
            }
        });

        query.Raw((_, _, integers, strings, _) =>
        {
            for (var i = 0; i < count; i++)
            {
                Assert.Equal(i, integers.Span[i]);
                Assert.Equal("three", strings.Span[i]);
                strings.Span[i] = "four";
            }
        });

        query.Job((ref TypeA _, ref double _, ref int index, ref string str, ref char _) =>
        {
            Assert.Equal(index, index);
            Assert.Equal("four", str);
            str = "five";
        });

        query.Job(6, delegate(int uniform, ref TypeA _, ref double _, ref int index, ref string str, ref char _)
        {
            Assert.Equal(index, index);
            Assert.Equal("five", str);
            str = uniform.ToString();
        });


        query.For(7, static (int uniform, ref TypeA _, ref double _, ref int _, ref string str, ref char _) =>
        {
            Assert.Equal(6.ToString(), str);
            str = uniform.ToString();
        });


        query.Raw(8, (uniform, _, _, _, strings, _) =>
        {
            for (var i = 0; i < count; i++)
            {
                Assert.Equal(7.ToString(), strings.Span[i]);
                strings.Span[i] = uniform.ToString();
            }
        });

        query.Raw(9, (uniform, _, _, _, strings, _) =>
        {
            for (var i = 0; i < count; i++)
            {
                Assert.Equal(8.ToString(), strings.Span[i]);
                strings.Span[i] = uniform.ToString();
            }
        });


        query.For((in Entity e, ref TypeA a, ref double _, ref int _, ref string str, ref char _) =>
        {
            Assert.True(e.Alive);
            Assert.Equal(e, a.entity);
            
            Assert.Equal(9.ToString(), str);
            str = "10";
        });

        
        query.For(11, (int uniform, in Entity _, ref TypeA _, ref double _, ref int _, ref string str, ref char _) =>
        {
            Assert.Equal(10.ToString(), str);
            str = uniform.ToString();
        });


        query.For((ref TypeA _, ref double _, ref int _, ref string str, ref char _) => { Assert.Equal(11.ToString(), str); });
    }

    
    [Fact]
    public void Cannot_Run_Job_on_Wildcard_Query()
    {
        using var world = new World();
        world.Spawn().Add("jason").Add(123).Add(1.5f).Add('3').Add(3.0d);

        var stream = world.Query<string, int, float, char, double>(Match.Any).Stream();
        Assert.Throws<InvalidOperationException>(() => stream.Job((ref str, ref _, ref _, ref _, ref _) => { output.WriteLine(str);}));

        stream = world.Query<string, int, float, char, double>(Match.Entity).Stream();
        Assert.Throws<InvalidOperationException>(() => stream.Job((ref str, ref _, ref _, ref _, ref _) => { output.WriteLine(str);}));

        stream = world.Query<string, int, float, char, double>(Match.Target).Stream();
        Assert.Throws<InvalidOperationException>(() => stream.Job((ref str, ref _, ref _, ref _, ref _) => { output.WriteLine(str);}));

        stream = world.Query<string, int, float, char, double>(Match.Object).Stream();
        Assert.Throws<InvalidOperationException>(() => stream.Job((ref str, ref _, ref _, ref _, ref _) => { output.WriteLine(str);}));

        stream = world.Query<string, int, float, char, double>(Match.Plain).Stream();
        var ran = false;
        stream.Job((ref string str, ref int _, ref float _, ref char _, ref double _) =>
        { 
            output.WriteLine(str); 
            ran = true;
        });
        Assert.True(ran);
    }

    
    [Fact]
    private void Can_Warmup()
    {
        using var world = new World();
        var stream = world.Query<string, Vector3, int, Matrix4x4, object>().Stream();
        stream.Query.Warmup();
    }

    private struct TypeA
    {
        public Entity entity;
    };
    
    
    [Fact]
    public void Has_Batch_Interface()
    {
        using var world = new World();
        
        world.Spawn().Add(1).Add(2.0f).Add(123.0).Add('c').Add(true);
        
        var stream = world.Query<int, float, double, char, bool>().Not<string>().Stream();
        var batch = stream.Batch();
        batch.Add<string>("visited");
        batch.Submit();

        var check = world.Query<int, float, double, char, bool>().Compile();
        var i = 0;
        foreach (var entity in check)
        {
            i++;
            Assert.True(entity.Has<string>());
        }
        Assert.Equal(1, i);
    }
}