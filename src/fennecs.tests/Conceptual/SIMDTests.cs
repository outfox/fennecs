using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace fennecs.tests.Conceptual;

public class SIMDTests
{
    private static readonly Vector4 UniformConstantVector = new(1, 2, 3, 4);

    [Fact]
    public void CanWriteVectorToArray()
    {
        var array = new Vector4[100000];
        for (var i = 0; i < array.Length; i++)
        {
            Assert.Equal(Vector4.Zero, array[i]);
        }
        
        array.AsSpan().Fill(UniformConstantVector);
        
        for (var i = 0; i < array.Length; i++)
        {
            Assert.Equal(UniformConstantVector, array[i]);
        }
    }

    [Fact]
    public void CanSimdWriteVectorToArray()
    {
        var array = new Vector4[100000];

        var mem = array.AsMemory();
        
        using var handle = mem.Pin();
        unsafe
        {
            var length = mem.Length * sizeof(Vector4) / sizeof(float);
            
            var p1 = (float*)handle.Pointer;

            var uHalf = UniformConstantVector.AsVector128();
            var u256 = Vector256.Create(uHalf, uHalf);

            var vectorSize = Vector256<float>.Count;
            var vectorEnd = length / vectorSize * vectorSize;

            for (var i = 0; i < length; i += vectorSize)
            {
                var addr = p1 + i;
                Avx.Store(addr, u256);
            }

            for (var i = vectorEnd; i < length; i++) // remaining elements
            {
                p1[i] = UniformConstantVector[i % 4];
            }
        }

        
        foreach (var t in array)
        {
            Assert.Equal(UniformConstantVector, t);
        }
    }


    [Fact]
    public void CanSimdWriteIntArray()
    {
        var array = new int[100000];
        
        var mem = array.AsMemory();
        
        using var handle = mem.Pin();
        var count = mem.Length;

        unsafe
        {
            var p1 = (int*)handle.Pointer;

            var vectorSize = Vector256<int>.Count;
            var vectorEnd = count - count % vectorSize;
            for (var i = 0; i < vectorEnd; i += vectorSize)
            {
                var v1 = Avx.LoadVector256(p1 + i);
                Avx.Store(p1 + i, Avx2.Add(v1, Vector256<int>.One));
            }

            for (var i = vectorEnd; i < count; i++) // remaining elements
            {
                p1[i]++;
            }
        }

        foreach (var t in array)
        {
            Assert.Equal(1, t);
        }
    }
}
