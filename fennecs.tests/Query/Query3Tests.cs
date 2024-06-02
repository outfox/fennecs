using System.Numerics;

namespace fennecs.tests.Query;

// ReSharper disable ParameterOnlyUsedForPreconditionCheck.Local
// ReSharper disable once ClassNeverInstantiated.Global
public class Query3Tests
{
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

        query.For(12f, (Entity x, ref int _, ref string str, ref char _, float uniform) =>
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

        query.Job(6, (ref int index, ref string str, ref char _, int uniform) =>
        {
            Assert.Equal(index, index);
            Assert.Equal("five", str);
            str = uniform.ToString();
        });


        query.For(7, (ref int _, ref string str, ref char _, int uniform) =>
        {
            Assert.Equal(6.ToString(), str);
            str = uniform.ToString();
        });
        
        query.Raw(8, (_, strings, _, uniform) =>
        {
            for (var i = 0; i < count; i++)
            {
                Assert.Equal(7.ToString(), strings.Span[i]);
                strings.Span[i] = uniform.ToString();
            }
        });

        query.Raw(9, (_, strings, _, uniform) =>
        {
            for (var i = 0; i < count; i++)
            {
                Assert.Equal(8.ToString(), strings.Span[i]);
                strings.Span[i] = uniform.ToString();
            }
        });

        query.For((Entity e, ref int _, ref string str, ref char _) =>
        {
            Assert.Equal(9.ToString(), str);
            str = "10";
        });


        query.For(11, (Entity e, ref int _, ref string str, ref char _, int uniform) =>
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
}
