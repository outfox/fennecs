using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

// ReSharper disable BuiltInTypeReferenceStyle
// ReSharper disable ConvertToCompoundAssignment - We do not do this here because RyuJIT prefers the non-compound version.

namespace fennecs;

/// <summary>
/// Accessor to a Stream that exposes high-speed SIMD operations.
/// </summary>
/// <typeparam name="C0">component type to stream. if this type is not in the query, the stream will always be length zero.</typeparam>
// ReSharper disable once NotAccessedPositionalProperty.Global
public record SIMD(Query Query)
{
    private static void AssertCongruence<T, U>(int size) where T : unmanaged where U : unmanaged
    {
        if (size > 0 && Unsafe.SizeOf<T>() % Unsafe.SizeOf<U>() == 0 || size % Unsafe.SizeOf<U>() == 0) return;
        throw new DataMisalignedException($"sizeof {nameof(T)} must be an integer multiple of sizeof {typeof(Int32)} and also of the destination component's size.");
    }

    private static void AssertEqualSize(int size, int otherSize)
    {
        if (size == otherSize) return;
        throw new DataMisalignedException($"Component sizes must be equal for SIMD operations.");
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="destination"></param>
    /// <param name="uniform"></param>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="U"></typeparam>
    /// <exception cref="DataMisalignedException"></exception>
    public void Add<T>(Comp<T> destination, Int32 uniform) where T : unmanaged
    {
        AssertCongruence<T, int>(destination.SIMDsize);

        if (Avx2.IsSupported) Add_UI32_impl_AVX2(destination, uniform);
    }

    /// <summary>
    /// Adds a uniform value to a component on all Entities in a Query.
    /// </summary>
    /// <remarks>
    /// The destination component must be backed by an unmanaged type, and be be (a multiple of) the same size as the uniform value.
    /// </remarks>
    public void Add<T>(Comp<T> destination, Single uniform) where T : unmanaged
    {
        AssertCongruence<T, float>(destination.SIMDsize);

        if (Avx2.IsSupported) Add_UF32_impl_AVX(destination, uniform);
    }


    /// <summary>
    /// 
    /// </summary>
    /// <remarks>destination can also be operand 1 or operand 2</remarks>
    /// <param name="destination"></param>
    /// <exception cref="DataMisalignedException">all operand components must be congruent with Int32, and the same size.</exception>
    public void AddAsI32<O1, O2, DEST>(Comp<O1> operand1, Comp<O2> operand2, Comp<DEST> destination) 
        where O1 : unmanaged
        where O2 : unmanaged
        where DEST : unmanaged
    {
        AssertCongruence<DEST, int>(destination.SIMDsize);
        AssertEqualSize(destination.SIMDsize, operand1.SIMDsize);
        AssertEqualSize(destination.SIMDsize, operand2.SIMDsize);

        if (Avx2.IsSupported) Add_2I32_impl_AVX2(destination, operand1, operand2);
    }
    
    public void AddAsI32<O1, O2>(Comp<O1> operand1, Comp<O2> operand2) 
        where O1 : unmanaged 
        where O2 : unmanaged 
        => AddAsI32(operand1, operand2, operand1); 


    
    internal void Add_UI32_impl_AVX2<T>(Comp<T> destination, Int32 uniform) where T : unmanaged
    {
        var uniformVector = Vector256.Create(uniform);

        foreach (var table in Query.Archetypes)
        {
            using var join = table.CrossJoin<T>([destination.Expression]);

            if (join.Empty) continue;
            do
            {
                using var dest = join.Select.AsMemory().Pin();
                
                var count = table.Count;
                var vectorSize = Vector256<Single>.Count;

                unsafe
                {
                    var pd = (Int32*)dest.Pointer;
                    var i = 0;
                    var vectorEnd = count - count % vectorSize;

                    for (; i < vectorEnd; i += vectorSize)
                    {
                        var sum = Avx2.Add(uniformVector, Avx.LoadVector256(pd + i));
                        Avx.Store(pd + i, sum);
                    }

                    // remaining elements
                    for (; i < count; i++) pd[i] = pd[i] + uniform;
                }
            } while (join.Iterate());
        }
    }


    private void Add_UF32_impl_AVX<T>(Comp<T> destination, float uniform) where T : unmanaged
    {
        var uniformVector = Vector256.Create(uniform);

        foreach (var table in Query.Archetypes)
        {
            using var join = table.CrossJoin<T>([destination.Expression]);
            
            if (join.Empty) continue;
            do
            {
                using var dest = join.Select.AsMemory().Pin();
                
                var count = table.Count;
                var vectorSize = Vector256<Single>.Count;

                unsafe
                {
                    var pd = (Single*)dest.Pointer;
                    var i = 0;
                    var vectorEnd = count - count % vectorSize;

                    for (; i < vectorEnd; i += vectorSize)
                    {
                        var sum = Avx.Add(uniformVector, Avx.LoadVector256(pd + i));
                        Avx.Store(pd + i, sum);
                    }

                    // remaining elements
                    for (; i < count; i++) pd[i] = pd[i] + uniform;
                }
            } while (join.Iterate());
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="destination"></param>
    /// <param name="operand1"></param>
    /// <param name="operand2"></param>
    /// <typeparam name="OUT"></typeparam>
    /// <typeparam name="O1"></typeparam>
    /// <typeparam name="O2"></typeparam>
    public void Add_2I32_impl_AVX2<OUT, O1, O2>(Comp<OUT> destination, Comp<O1> operand1, Comp<O2> operand2)
        where OUT : unmanaged
        where O1 : unmanaged
        where O2 : unmanaged
    {
        foreach (var table in Query.Archetypes)
        {
            using var join = table.CrossJoin<OUT, O1, O2>([destination.Expression, operand1.Expression, operand2.Expression]);

            if (join.Empty) continue;
            do
            {
                var (destinationStorage, op1Storage, op2Storage) = join.Select;

                using var dest = destinationStorage.AsMemory().Pin();
                using var op1 = op1Storage.AsMemory().Pin();
                using var op2 = op2Storage.AsMemory().Pin();

                var count = table.Count;
                var vectorSize = Vector256<Int32>.Count;

                unsafe
                {
                    var pd = (Int32*)dest.Pointer;
                    var p1 = (Int32*)op1.Pointer;
                    var p2 = (Int32*)op2.Pointer;

                    var i = 0;
                    var vectorEnd = count - count % vectorSize;

                    for (; i < vectorEnd; i += vectorSize)
                    {
                        var o1 = Avx.LoadVector256(p1 + i);
                        var o2 = Avx.LoadVector256(p2 + i);

                        var sum = Avx2.Add(o1, o2);
                        Avx.Store(pd + i, sum);
                    }

                    for (; i < count; i++) // remaining elements
                    {
                        pd[i] = p1[i] + p2[i];
                    }
                }
            } while (join.Iterate());
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="destination"></param>
    /// <param name="operand1"></param>
    /// <param name="operand2"></param>
    /// <typeparam name="OUT"></typeparam>
    /// <typeparam name="O1"></typeparam>
    /// <typeparam name="O2"></typeparam>
    public void Add_2F32_impl_AVX<OUT, O1, O2>(Comp<OUT> destination, Comp<O1> operand1, Comp<O2> operand2)
        where OUT : unmanaged
        where O1 : unmanaged
        where O2 : unmanaged
    {
        foreach (var table in Query.Archetypes)
        {
            using var join = table.CrossJoin<OUT, O1, O2>([destination.Expression, operand1.Expression, operand2.Expression]);

            if (join.Empty) continue;
            do
            {
                var (destinationStorage, op1Storage, op2Storage) = join.Select;

                using var dest = destinationStorage.AsMemory().Pin();
                using var op1 = op1Storage.AsMemory().Pin();
                using var op2 = op2Storage.AsMemory().Pin();

                var count = table.Count;
                var vectorSize = Vector256<Int32>.Count;

                unsafe
                {
                    var pd = (Single*)dest.Pointer;
                    var p1 = (Single*)op1.Pointer;
                    var p2 = (Single*)op2.Pointer;

                    var i = 0;
                    var vectorEnd = count - count % vectorSize;

                    for (; i < vectorEnd; i += vectorSize)
                    {
                        var o1 = Avx.LoadVector256(p1 + i);
                        var o2 = Avx.LoadVector256(p2 + i);

                        var sum = Avx.Add(o1, o2);
                        Avx.Store(pd + i, sum);
                    }

                    for (; i < count; i++) // remaining elements
                    {
                        pd[i] = p1[i] + p2[i];
                    }
                }
            } while (join.Iterate());
        }
    }

    public void Add_3I32_impl_AVX2<OUT, O1, O2, O3>(Comp<OUT> destination, Comp<O1> operand1, Comp<O2> operand2, Comp<O3> operand3)
        where OUT : unmanaged
        where O1 : unmanaged
        where O2 : unmanaged
        where O3 : unmanaged
    {
        foreach (var table in Query.Archetypes)
        {
            using var join = table.CrossJoin<OUT, O1, O2, O3>([destination.Expression, operand1.Expression, operand2.Expression, operand3.Expression]);

            if (join.Empty) continue;
            do
            {
                var (destinationStorage, op1Storage, op2Storage, op3Storage) = join.Select;

                using var dest = destinationStorage.AsMemory().Pin();
                using var op1 = op1Storage.AsMemory().Pin();
                using var op2 = op2Storage.AsMemory().Pin();
                using var op3 = op3Storage.AsMemory().Pin();

                var count = table.Count;
                var vectorSize = Vector256<Int32>.Count;

                unsafe
                {
                    var pd = (Int32*)dest.Pointer;
                    var p1 = (Int32*)op1.Pointer;
                    var p2 = (Int32*)op2.Pointer;
                    var p3 = (Int32*)op3.Pointer;
                     

                    var i = 0;
                    var vectorEnd = count - count % vectorSize;

                    for (; i < vectorEnd; i += vectorSize)
                    {
                        var o1 = Avx.LoadVector256(p1 + i);
                        var o2 = Avx.LoadVector256(p2 + i);
                        var o3 = Avx.LoadVector256(p3 + i);

                        var sum1 = Avx2.Add(o1, o2);
                        var sum2 = Avx2.Add(sum1, o3);
                        Avx.Store(pd + i, sum2);
                    }

                    for (; i < count; i++) // remaining elements
                    {
                        pd[i] = p1[i] + p2[i] + p3[i];
                    }
                }
            } while (join.Iterate());
        }
    }
}
