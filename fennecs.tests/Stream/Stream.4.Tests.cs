using System.Collections;
using System.Numerics;
using fennecs.storage;

namespace fennecs.tests.Stream;

// ReSharper disable ParameterOnlyUsedForPreconditionCheck.Local
// ReSharper disable once ClassNeverInstantiated.Global
public class Stream4Tests(ITestOutputHelper output)
{
    [Fact]
    public void Can_Enumerate_Stream_NewJobRWWW()
    {
        using var world = new World();
        var arnold = world.Spawn().Add("Arnold").Add(1).Add(7.0f).Add('x').Add(5d);
        var dolph = world.Spawn().Add("Dolph").Add(2).Add(8.0f).Add('y').Add(6d);
        
        var stream = world.Stream<string, int, float, char>();
        stream.Job((e, s, i, f, c) =>
        {
            s.write = "haha!";
            output.WriteLine("" + s.read);
            output.WriteLine(i.ToString());
        });
    }

    [Fact]
    public void Can_Enumerate_Stream_NewForRWWW()
    {
        using var world = new World();
        var arnold = world.Spawn().Add("Arnold").Add(1).Add(7.0f).Add('x').Add(5d);
        var dolph = world.Spawn().Add("Dolph").Add(2).Add(8.0f).Add('y').Add(6d);
        
        var stream = world.Stream<string, int, float, char>();
        stream.For((e, s, i, f, c) =>
        {
            s.write = "haha!";
            output.WriteLine(s.read);
            var j = i.read;
            output.WriteLine(""+i.read);
        });
    }


    [Fact]
    public void Can_Enumerate_Stream()
    {
        using var world = new World();
        var arnold = world.Spawn().Add("Arnold").Add(1).Add(7.0f).Add('x').Add(5d);
        var dolph = world.Spawn().Add("Dolph").Add(2).Add(8.0f).Add('y').Add(6d);
        
        List<(Entity, string, int, float, char)> list = [(arnold, "Arnold", 1, 7.0f, 'x'), (dolph, "Dolph", 2, 8.0f, 'y')];
        
        var stream = world.Stream<string, int, float, char>();
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
        
        List<object> list = [(arnold, "Arnold", 1, 7.0f, 'x'), (dolph, "Dolph", 2, 8.0f, 'y')];
        
        IEnumerable stream = world.Stream<string, int, float, char>();
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

        var stream = world.Stream<string, int, float, char>();
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

        var query = world.Query<double, int, string, char>().Stream();

        //Create an empty table by spawning and despawning a single entity
        //that matches our test Query (but is a larger Archetype)
        if (createEmptyTable)
        {
            var dead = world.Spawn().Add<int>().Add<char>().Add<double>().Add(0.25f).Add("will be removed");
            world.Despawn(dead);
        }

        for (var index = 0; index < count; index++)
        {
            Assert.Equal(index, query.Count);

            world.Spawn()
                .Add(99.999)
                .Add(index)
                .Add("one")
                .Add('Q');
        }

        query.For(( _,  _, str,  _) =>
        {
            Assert.Equal("one", str);
            str.write = "two";
        });

        query.Raw((_, integers, strings, _) =>
        {
            for (var i = 0; i < count; i++)
            {
                Assert.Equal(i, integers.Span[i]);
                Assert.Equal("two", strings.Span[i]);
                strings.write[i] = "three";
            }
        });

        query.Raw((_, integers, strings, _) =>
        {
            for (var i = 0; i < count; i++)
            {
                Assert.Equal(i, integers.Span[i]);
                Assert.Equal("three", strings.Span[i]);
                strings.write[i] = "four";
            }
        });

        query.Job((_, index, str, _) =>
        {
            Assert.Equal(index.read, index);
            Assert.Equal("four", str);
            str.write = "five";
        });

        query.Job(6, (uniform, _, index, str, _) =>
        {
            Assert.Equal(index, index.read);
            Assert.Equal("five", str);
            str.write = uniform.ToString();
        });


        query.For(7, (uniform, _, _, str, _) =>
        {
            Assert.Equal(6.ToString(), str);
            str.write = uniform.ToString();
        });


        query.Raw(8, (uniform, _, _, strings, _) =>
        {
            for (var i = 0; i < count; i++)
            {
                Assert.Equal(7.ToString(), strings.Span[i]);
                strings.Span[i] = uniform.ToString();
            }
        });

        query.Raw(9, (uniform, _, _, strings, _) =>
        {
            for (var i = 0; i < count; i++)
            {
                Assert.Equal(8.ToString(), strings.Span[i]);
                strings.Span[i] = uniform.ToString();
            }
        });


        query.For((e, _, _, str, _) =>
        {
            Assert.True(e.Alive);
            Assert.Equal(9.ToString(), str);
            str.write = "10";
        });

        
        query.For(11, (_, uniform, _, _, str, _) =>
        {
            Assert.Equal(10.ToString(), str);
            str.write = uniform.ToString();
        });


        query.For((ref double _, ref int _, ref string str, ref char _) => { Assert.Equal(11.ToString(), str); });
    }
    
        
    [Fact]
    private void Can_Warmup()
    {
        using var world = new World();
        var stream = world.Query<string, Vector3, int, Matrix4x4>().Stream();
        stream.Query.Warmup();
    }
    
    [Fact]
    public void Cannot_Run_Job_on_Wildcard_Query()
    {
        using var world = new World();
        world.Spawn().Add("jason").Add(123).Add(1.5f).Add('3');

        var stream = world.Query<string, int, float, char>(Match.Any).Stream();
        Assert.Throws<InvalidOperationException>(() => stream.Job((ref string str) => { output.WriteLine(str);}));

        stream = world.Query<string, int, float, char>(Match.Entity).Stream();
        Assert.Throws<InvalidOperationException>(() => stream.Job((ref string str) => { output.WriteLine(str);}));

        stream = world.Query<string, int, float, char>(Match.Target).Stream();
        Assert.Throws<InvalidOperationException>(() => stream.Job((ref string str) => { output.WriteLine(str);}));

        stream = world.Query<string, int, float, char>(Match.Object).Stream();
        Assert.Throws<InvalidOperationException>(() => stream.Job((ref string str) => { output.WriteLine(str);}));

        stream = world.Query<string, int, float, char>(Match.Plain).Stream();
        var ran = false;
        stream.Job((ref string str, ref int _, ref float _, ref char _) =>
        { 
            output.WriteLine(str); 
            ran = true;
        });
        Assert.True(ran);
    }
}