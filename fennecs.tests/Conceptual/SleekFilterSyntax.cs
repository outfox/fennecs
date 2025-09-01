using System.Numerics;
using System.Linq;

namespace fennecs.tests.Conceptual;

public class SleekFilterSyntax
{
    private static Stream<Index> Setup(int count1, int count2 = 0)
    {
        using var world = new World();

        using var _ = world.Entity()
            .Add<Index>()
            .Spawn(count1)
            .Add(true)
            .Spawn(count2);

        // This is shorthand for a stream query.
        return world.Stream<Index>();
    }
    
    record struct Test1(int Value);
    record struct Test2(float Value) : IComparable<float>
    {
        public int CompareTo(Test2 other)
        {
            return Value.CompareTo(other.Value);
        }

        public int CompareTo(float other)
        {
            throw new NotImplementedException();
        }
    }

    [Fact]
    public void FilterWithLambda()
    {
        var world = new World();

        var allPositions = world.Stream<Vector3, int>();
        var allPositions2 = world.Stream<Vector3, Vector3>();
        var belowTheGround = allPositions with
        {
            F0 = (in Vector3 v) => v.Y < 0,
        };
        
        belowTheGround.For((ref Vector3 index) =>
        {
            Assert.True(index.Y < 0);   
        });

        var belowTheGround1 = allPositions.Where((in Vector3 v) => v.Y < 0); //C#13
        var belowTheGround2 = allPositions.Where((in int i) => i == 12345);
        
        var belowTheGround3 = allPositions.Where((in v) => v.Y < 0); //C#14
        var belowTheGround4 = allPositions.Where((in i) => i == 12345);
        
        
        var testStream = world.Stream<int, float>();
        var testFiltered1 = testStream.Where((in int v) => v < 0); //C#14
        var testFiltered2 = testStream.Where((in float i) => i >= 0.5f);
    }
}
