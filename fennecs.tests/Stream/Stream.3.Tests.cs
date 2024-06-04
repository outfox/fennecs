using System.Numerics;

namespace fennecs.tests.Stream;

// ReSharper disable ParameterOnlyUsedForPreconditionCheck.Local
// ReSharper disable once ClassNeverInstantiated.Global
public class Stream3Tests
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

        query.For(12f, (float _, Entity _, ref int _, ref string str, ref char _) =>
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

        query.For((Entity _, ref int _, ref string str, ref char _) =>
        {
            Assert.Equal(9.ToString(), str);
            str = "10";
        });


        query.For(11, (int uniform, Entity _, ref int _, ref string str, ref char _) =>
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
