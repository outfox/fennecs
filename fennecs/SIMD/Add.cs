using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.Arm;
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
    /// <summary>
    /// 
    /// </summary>
    /// <param name="destination"></param>
    /// <param name="uniform"></param>
    /// <typeparam name="T"></typeparam>
    /// <exception cref="DataMisalignedException"></exception>
    public void AddI32<T>(IBlittable destination, T uniform) where T : unmanaged
    {
        if (Unsafe.SizeOf<T>() % Unsafe.SizeOf<Int32>() != 0 || destination.TypeExpression.SIMDsize % Unsafe.SizeOf<Int32>() != 0)
        {
            throw new DataMisalignedException($"sizeof {nameof(T)} must be an integer multiple of sizeof System.Int32 and also of the destination's size.");
        }
        
        //if (Avx2.IsSupported) 
        //AddI32_impl_AVX2((Blittable<T>) destination, uniform);
        //else if (AdvSimd.IsSupported) AddI32_impl_AdvSimd(destination, uniform);
        //else if (Sse2.IsSupported) AddI32_impl_Sse2(destination, uniform);
        //else AddI32_impl_Scalar(destination, uniform);
    }
    
    internal void AddI32_impl_AVX2<T>(Blittable<T> destination, T uniform) where T : unmanaged
    {
        var uniformVector = Vector256.Create(uniform).As<T, int>();
        var uniformI32 = Unsafe.As<T, Int32>(ref uniform);
        
        foreach (var table in Query.Archetypes)
        {
            using var dest = table.GetStorage<T>(destination.match).AsMemory().Pin();

            var count = table.Count;
            var vectorSize = Vector256<Int32>.Count;

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
                for (; i < count; i++) pd[i] = pd[i] + uniformI32;
            }
        }
    }

    public void AddI32(Int32 uniform, Match match0 = default)
    {
        Entity entity = default;
        
        /*
        var plainComponent = Comp<Vector2>.Plain;
        var anyRelation = Comp<Vector2>.Matching(Entity.Any);
        var specificRelation = Comp<Vector2>.Matching(entity);
        var anyLink = Comp<string>.Matching(Link.Any);
        var specificLink = Comp<string>.Matching(Link.With("This hurts."));
        var illegalLink = Comp<float>.Matching(Link.With("Straight to jail."));
        var veryIllegalLink = Comp<float>.Matching(Link.Any);
        */
        
        //AddImplAvx2(uniform, match0);
        /*
        if (Avx2.IsSupported) AddImplAvx2Unroll4(uniform, match0);
        else if (AdvSimd.IsSupported) AddImplAdvSimd(uniform, match0);
        else if (Sse2.IsSupported) AddImplSse2(uniform, match0);
        else AddImplScalar(uniform, match0);
        */
    }


    public void AddI32Unroll(Int32 uniform, Match match0 = default)
    {
        //AddImplAvx2Unroll4(uniform, match0);
        //else if (AdvSimd.IsSupported) AddImplAdvSimd(uniform, match0);
        //else if (Sse2.IsSupported) AddImplSse2(uniform, match0);
        //else AddImplScalar(uniform, match0);
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
    public void AddI32<OUT, O1, O2>(Blittable<OUT> destination, Blittable<O1> operand1, Blittable<O2> operand2)
        where OUT : unmanaged
        where O1 : unmanaged
        where O2 : unmanaged
    {
        foreach (var table in Query.Archetypes)
        {

            using var dest = table.GetStorage<OUT>(destination.match).AsMemory().Pin();
            using var op1 = table.GetStorage<O1>(operand1.match).AsMemory().Pin();
            using var op2 = table.GetStorage<O2>(operand2.match).AsMemory().Pin();

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
        }
    }
    
    public void AddI32<OUT, O1, O2, O3>(Blittable<OUT> destination, Blittable<O1> operand1, Blittable<O2> operand2, Blittable<O3> operand3)
        where OUT : unmanaged
        where O1 : unmanaged
        where O2 : unmanaged
        where O3 : unmanaged
    {
        foreach (var table in Query.Archetypes)
        {
            if (table.IsEmpty) continue;
            
            using var dest = table.GetStorage<OUT>(destination.match).AsMemory().Pin();
            var op1 = table.GetStorage<O1>(operand1.match).AsMemory().Pin();
            var op2 = table.GetStorage<O2>(operand2.match).AsMemory().Pin();
            var op3 = table.GetStorage<O3>(operand3.match).AsMemory().Pin();

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
                    var v1 = Avx.LoadVector256(p1 + i);
                    var v2 = Avx.LoadVector256(p2 + i);
                    var v3 = Avx.LoadVector256(p3 + i);

                    var sum = Avx2.Add(v2, v3);
                    sum = Avx2.Add(sum, v1);
                    Avx.Store(pd + i, sum);
                }

                for (; i < count; i++) // remaining elements
                {
                    p1[i] = p1[i] + p2[i] + p3[i];
                }
            }
        }
    }

    public void SumI32Burst<OUT, O1, O2, O3>(Blittable<OUT> destination, Blittable<O1> operand1, Blittable<O2> operand2, Blittable<O3> operand3)
        where OUT : unmanaged
        where O1 : unmanaged
        where O2 : unmanaged
        where O3 : unmanaged
    {
        foreach (var table in Query.Archetypes)
        {
            if (table.IsEmpty) continue;

            using var dest = table.GetStorage<OUT>(destination.match).AsMemory().Pin();
            var op1 = table.GetStorage<O1>(operand1.match).AsMemory().Pin();
            var op2 = table.GetStorage<O2>(operand2.match).AsMemory().Pin();
            var op3 = table.GetStorage<O3>(operand3.match).AsMemory().Pin();

            var count = table.Count;
            var vectorSize = Vector256<int>.Count;
            var burstSize = vectorSize * 2;

            unsafe
            {
                var pd = (int*)dest.Pointer;
                var p1 = (int*)op1.Pointer;
                var p2 = (int*)op2.Pointer;
                var p3 = (int*)op3.Pointer;

                var i = 0;
                var burstEnd = count - count % burstSize;

                for (; i < burstEnd; i += burstSize)
                {
                    var v1a = Avx.LoadVector256(p1 + i);
                    var v1b = Avx.LoadVector256(p1 + i + vectorSize);
                    var v2a = Avx.LoadVector256(p2 + i);
                    var v2b = Avx.LoadVector256(p2 + i + vectorSize);
                    var v3a = Avx.LoadVector256(p3 + i);
                    var v3b = Avx.LoadVector256(p3 + i + vectorSize);


                    var suma = Avx2.Add(v1a, v2a);
                    suma = Avx2.Add(suma, v3a);
                    var sumb = Avx2.Add(v1b, v2b);
                    sumb = Avx2.Add(sumb, v3b);

                    Avx.Store(pd + i, suma);
                    Avx.Store(pd + i + vectorSize, sumb);
                }

                for (; i < count; i++) // remaining elements
                {
                    p1[i] = p1[i] + p2[i] + p3[i];
                }
            }
        }
    }

    public void AddI32<OUT, O1, O2, O3, O4>(Blittable<OUT> destination, Blittable<O1> operand1, Blittable<O2> operand2, Blittable<O3> operand3, Blittable<O4> operand4)
        where OUT : unmanaged
        where O1 : unmanaged
        where O2 : unmanaged
        where O3 : unmanaged
        where O4 : unmanaged
    {
        foreach (var table in Query.Archetypes)
        {
            if (table.IsEmpty) continue;

            using var dest = table.GetStorage<OUT>(destination.match).AsMemory().Pin();
            var op1 = table.GetStorage<O1>(operand1.match).AsMemory().Pin();
            var op2 = table.GetStorage<O2>(operand2.match).AsMemory().Pin();
            var op3 = table.GetStorage<O3>(operand3.match).AsMemory().Pin();
            var op4 = table.GetStorage<O4>(operand4.match).AsMemory().Pin();

            var count = table.Count;
            var vectorSize = Vector256<Int32>.Count;

            unsafe
            {
                var pd = (Int32*)dest.Pointer;
                var p1 = (Int32*)op1.Pointer;
                var p2 = (Int32*)op2.Pointer;
                var p3 = (Int32*)op3.Pointer;
                var p4 = (Int32*)op4.Pointer;

                var i = 0;
                var vectorEnd = count - count % vectorSize;

                for (; i < vectorEnd; i += vectorSize)
                {
                    var v1 = Avx.LoadVector256(p1 + i);
                    var v2 = Avx.LoadVector256(p2 + i);
                    var v3 = Avx.LoadVector256(p3 + i);
                    var v4 = Avx.LoadVector256(p4 + i);

                    var sum1 = Avx2.Add(v3, v4);
                    var sum2 = Avx2.Add(v1, v2);
                    Avx.Store(pd + i, Avx2.Add(sum1, sum2));
                }

                for (; i < count; i++) // remaining elements
                {
                    p1[i] = p1[i] + p2[i] + p3[i] + p4[i];
                }
            }
        }
    }

    public unsafe void AddF32(Single uniform, Match match0 = default)
    {
        /*
        if (Avx2.IsSupported) AddImplAvx2(uniform, match0);
        else if (AdvSimd.IsSupported) AddImplAdvSimd(uniform, match0);
        else if (Sse2.IsSupported) AddImplSse2(uniform, match0);
        else AddImplScalar(uniform, match0);
        */
    }
    /*
    internal void AddImplAvx2<C0, T2>(T2 summand, Match match0) where C0: unmanaged where T2: unmanaged, INumber<T2>
    {
        var summandVector = Vector256.Create(summand);
        var vectorSize = Vector256<T2>.Count;

        foreach (var table in Query.Archetypes)
        {
            if (table.IsEmpty) continue;

            var count = table.Count;
            (int from, int to) range = (0, count);

            using var mem1 = table.GetStorage<C0>(match0).AsMemory(range.from, range.to).Pin();
            unsafe
            {
                var p1 = (T2*)mem1.Pointer;
                var i = range.from;
                var vectorEnd = count - count % vectorSize;

                for (; i < vectorEnd; i += vectorSize)
                {
                    var v1 = Avx.LoadVector256(p1 + i);

                    var sum = Avx2.Add(v1, summandVector);
                    Avx.Store(p1 + i, sum);
                }

                for (; i < count; i++) // remaining elements
                {
                    p1[i] = p1[i] + summand;
                }
            }
        }
    }


    internal void AddImplAvx2Unroll4(Int32 summand, Match match0)
    {
        var summandVector = Vector256.Create(summand);
        var vectorSize = Vector256<Int32>.Count;
        const int burstSize = 256 / 8 / sizeof(Int32) * 4;

        foreach (var table in Query.Archetypes)
        {
            if (table.IsEmpty) continue;

            var count = table.Count;
            (int from, int to) range = (0, count);

            using var mem1 = table.GetStorage<C0>(match0).AsMemory(range.from, range.to).Pin();
            unsafe
            {
                var p1 = (Int32*)mem1.Pointer;
                var i = range.from;
                var vectorEnd = count - count % (burstSize);

                for (; i < vectorEnd; i += burstSize)
                {
                    var v1 = Avx.LoadVector256(p1 + i + 0 * vectorSize);
                    var v2 = Avx.LoadVector256(p1 + i + 1 * vectorSize);
                    var v3 = Avx.LoadVector256(p1 + i + 2 * vectorSize);
                    var v4 = Avx.LoadVector256(p1 + i + 3 * vectorSize);

                    var sum1 = Avx2.Add(v1, summandVector);
                    var sum2 = Avx2.Add(v2, summandVector);
                    var sum3 = Avx2.Add(v3, summandVector);
                    var sum4 = Avx2.Add(v4, summandVector);

                    Avx.Store(p1 + i + 0 * burstSize, sum1);
                    Avx.Store(p1 + i + 1 * burstSize, sum2);
                    Avx.Store(p1 + i + 2 * burstSize, sum3);
                    Avx.Store(p1 + i + 3 * burstSize, sum4);
                }

                for (; i < count; i++) // remaining elements
                {
                    p1[i] = p1[i] + summand;
                }
            }
        }
    }


    internal void AddImplAdvSimd(int summand, Match match0)
    {
        var summandVector = Vector128.Create(summand);
        var vectorSize = Vector128<int>.Count;

        foreach (var table in Query.Archetypes)
        {
            if (table.IsEmpty) continue;

            var count = table.Count;
            (int from, int to) range = (0, count);

            using var mem1 = table.GetStorage<C0>(match0).AsMemory(range.from, range.to).Pin();

            unsafe
            {
                var p1 = (int*)mem1.Pointer;
                var i = range.from;
                var vectorEnd = count - count % vectorSize;

                for (; i < vectorEnd; i += vectorSize)
                {
                    var v1 = AdvSimd.LoadVector128(p1 + i);

                    var sum = AdvSimd.Add(v1, summandVector);
                    AdvSimd.Store(p1 + i, sum);
                }

                for (; i < range.to; i++) // remaining elements
                {
                    p1[i] = p1[i] + summand;
                }
            }
        }
    }

    internal void AddImplSse2(int summand, Match match0)
    {
        var summandVector = Vector128.Create(summand);
        var vectorSize = Vector128<int>.Count;

        foreach (var table in Query.Archetypes)
        {
            if (table.IsEmpty) continue;

            var count = table.Count;
            (int from, int to) range = (0, count);

            using var mem1 = table.GetStorage<C0>(match0).AsMemory().Pin();

            unsafe
            {
                var p1 = (int*)mem1.Pointer;
                var i = range.from;
                var vectorEnd = count - count % vectorSize;

                for (; i < vectorEnd; i += vectorSize)
                {
                    var v1 = Sse2.LoadVector128(p1 + i);

                    var sum = Sse2.Add(v1, summandVector);
                    Sse2.Store(p1 + i, sum);
                }

                for (; i < range.to; i++) // remaining elements
                {
                    p1[i] = p1[i] + summand;
                }
            }
        }
    }


    internal void AddImplScalar<C0>(int summand, Match match0) where C0 : struct
    {
        foreach (var table in Query.Archetypes)
        {
            if (table.IsEmpty) continue;
            
            var span = MemoryMarshal.Cast<C0, int>(table.GetStorage<C0>(match0).Span);
            foreach (ref var value in span) value += summand;
        }
    }
    */
}