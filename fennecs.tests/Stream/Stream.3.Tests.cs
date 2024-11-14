﻿using System.Collections;
using System.Numerics;

namespace fennecs.tests.Stream;

// ReSharper disable ParameterOnlyUsedForPreconditionCheck.Local
// ReSharper disable once ClassNeverInstantiated.Global
public class Stream3Tests(ITestOutputHelper output)
{
    
    [Fact] public void Can_Use_RW_Inferred()
    {
        using var world = new World();
        var entity = world.Spawn();
        entity.Add(123).Add(890f).Add("ramen");

        var stream = world.Stream<int, float, string>();

        
        stream.For(static (a, b, s) =>
        {
            Assert.Equal(123, a.read);
            Assert.Equal(890f, b.read);
            b.write = 456f;
        });
        
    }

    
    [Fact]
    public void Can_Enumerate_Stream()
    {
        using var world = new World();
        var arnold = world.Spawn().Add("Arnold").Add(1).Add(7.0f);
        var dolph = world.Spawn().Add("Dolph").Add(2).Add(8.0f);
        
        List<(Entity, string, int, float)> list = [(arnold, "Arnold", 1, 7.0f), (dolph, "Dolph", 2, 8.0f)];
        
        var stream = world.Stream<string, int, float>();
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
        var arnold = world.Spawn().Add("Arnold").Add(1).Add(7.0f);
        var dolph = world.Spawn().Add("Dolph").Add(2).Add(8.0f);
        
        List<object> list = [(arnold, "Arnold", 1, 7.0f), (dolph, "Dolph", 2, 8.0f)];
        
        IEnumerable stream = world.Stream<string, int, float>();
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
        world.Spawn().Add("Arnold").Add(1).Add(7.0f);
        world.Spawn().Add("Dolph").Add(2).Add(8.0f);

        var stream = world.Stream<string, int, float>();
        
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

        var query = world.Query<int, string, char>().Stream();

        //Create an empty table by spawning and despawning a single entity
        //that matches our test Query (but is a larger Archetype)
        if (createEmptyTable)
        {
            var dead = world.Spawn().Add<int>().Add<char>().Add(0.25f).Add("will be removed");
            world.Despawn(dead);
        }

        for (var index = 0; index < count; index++)
        {
            Assert.Equal(index, query.Count);

            world.Spawn()
                .Add(index)
                .Add("one")
                .Add('Q');
        }

        query.For(12f, (float _, in Entity _, ref int _, ref string str, ref char _) =>
        {
            Assert.Equal("one", str);
            str = "two";
        });

        query.Raw((integers, strings, _) =>
        {
            for (var i = 0; i < count; i++)
            {
                Assert.Equal(i, integers.Span[i]);
                Assert.Equal("two", strings.Span[i]);
                strings.Span[i] = "three";
            }
        });

        query.Raw((integers, strings, _) =>
        {
            for (var i = 0; i < count; i++)
            {
                Assert.Equal(i, integers.Span[i]);
                Assert.Equal("three", strings.Span[i]);
                strings.Span[i] = "four";
            }
        });

        query.Job((ref int index, ref string str, ref char _) =>
        {
            Assert.Equal(index, index);
            Assert.Equal("four", str);
            str = "five";
        });

        query.Job(6, (int uniform, ref int index, ref string str, ref char _) =>
        {
            Assert.Equal(index, index);
            Assert.Equal("five", str);
            str = uniform.ToString();
        });


        query.For(7, (int uniform, ref int _, ref string str, ref char _) =>
        {
            Assert.Equal(6.ToString(), str);
            str = uniform.ToString();
        });
        
        query.Raw(8, (uniform, _, strings, _) =>
        {
            for (var i = 0; i < count; i++)
            {
                Assert.Equal(7.ToString(), strings.Span[i]);
                strings.Span[i] = uniform.ToString();
            }
        });

        query.Raw(9, (uniform, _, strings, _) =>
        {
            for (var i = 0; i < count; i++)
            {
                Assert.Equal(8.ToString(), strings.Span[i]);
                strings.Span[i] = uniform.ToString();
            }
        });

        query.For((in Entity _, ref int _, ref string str, ref char _) =>
        {
            Assert.Equal(9.ToString(), str);
            str = "10";
        });


        query.For(11, (int uniform, in Entity _, ref int _, ref string str, ref char _) =>
        {
            Assert.Equal(10.ToString(), str);
            str = uniform.ToString();
        });

        query.For((ref int _, ref string str, ref char _) => { Assert.Equal(11.ToString(), str); });
    }

    [Fact]
    private void Can_Warmup()
    {
        using var world = new World();
        var stream = world.Query<string, Vector3, int>().Stream();
        stream.Query.Warmup();
    }
    
    [Fact]
    public void Cannot_Run_Job_on_Wildcard_Query()
    {
        using var world = new World();
        world.Spawn().Add("jason").Add(123).Add(1.5f);

        var stream = world.Query<string, int, float>(Match.Any).Stream();
        Assert.Throws<InvalidOperationException>(() => stream.Job((ref string str) => { output.WriteLine(str);}));

        stream = world.Query<string, int, float>(Match.Entity).Stream();
        Assert.Throws<InvalidOperationException>(() => stream.Job((ref string str) => { output.WriteLine(str);}));

        stream = world.Query<string, int, float>(Match.Target).Stream();
        Assert.Throws<InvalidOperationException>(() => stream.Job((ref string str) => { output.WriteLine(str);}));

        stream = world.Query<string, int, float>(Match.Object).Stream();
        Assert.Throws<InvalidOperationException>(() => stream.Job((ref string str) => { output.WriteLine(str);}));

        stream = world.Query<string, int, float>(Match.Plain).Stream();
        var ran = false;
        stream.Job((ref string str, ref int _, ref float _) =>
        { 
            output.WriteLine(str); 
            ran = true;
        });
        Assert.True(ran);
    }
}
