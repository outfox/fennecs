using System.Runtime.Intrinsics;

namespace fennecs.tests.Conceptual;

public class BloomTests(ITestOutputHelper output)
{
    private interface ITest : IEnumerable<KeyValuePair<string,string>>;

    private class TestType : Dictionary<string, string>, ITest;

    private static readonly Vector256<long>[] One =
    [
        Vector256.Create(1, 0, 0, 0),
        Vector256.Create(0, 1, 0, 0),
        Vector256.Create(0, 0, 1, 0),
        Vector256.Create(0, 0, 0, 1)
    ];

    private Vector256<long> OneFunc(int slot)
    {
        return slot switch
        {
            0 => Vector256.Create(1, 0, 0, 0),
            1 => Vector256.Create(0, 1, 0, 0),
            2 => Vector256.Create(0, 0, 1, 0),
            3 => Vector256.Create(0, 0, 0, 1),
            _ => Vector256<long>.One // if invalid, force bloom match all
        };
    }

    Type[] ParentTypes(Type type)
    {
        var types = new List<Type>();
        var currentType = type.BaseType;
    
        while (currentType != null)
        {
            types.Add(currentType);
            currentType = currentType.BaseType;
        }
    
        return types.ToArray();
    }

    [Fact]
    public void GetInterfaces()
    {

        output.WriteLine($"Hardware: {Vector256.IsHardwareAccelerated}");

        var hash = Vector256<long>.Zero;

        output.WriteLine($"Interfaces: {typeof(TestType).GetInterfaces().Length}");
        
        foreach (var item in typeof(TestType).GetInterfaces())
        {
            output.WriteLine(item.FullName);
            var slot = (int)((uint)item.GetHashCode() % 256 / 64);
            var bit = (int)((uint)item.GetHashCode() % 64);
            hash = Vector256.BitwiseOr(hash, Vector256.ShiftLeft(One[slot], bit));
        }

        foreach (var item in typeof(TestType).GetInterfaces())
        {
            var slot = (int)((uint)item.FullName!.GetHashCode() % 256 / 64);
            var bit = (int)((uint)item.FullName!.GetHashCode() % 64);
            hash = Vector256.BitwiseOr(hash, Vector256.ShiftLeft(One[slot], bit));
        }

        for (var slot = 0; slot < Vector256<long>.Count; slot++)
        {
            output.WriteLine($"{slot}: {hash[slot]:b64}");
        }
    }
    
    [Fact]
    public void GetParentTypes()
    {

        output.WriteLine($"Hardware: {Vector256.IsHardwareAccelerated}");
        
        Vector256<uint> bloom = default;

        var hash = Vector256<long>.Zero;

        output.WriteLine($"Parents: {ParentTypes(typeof(TestType)).Length}");

        foreach (var item in ParentTypes(typeof(TestType)))
        {   
            output.WriteLine(item.FullName);
            var slot = (int)((uint)item.GetHashCode() % 256 / 64);
            var bit = (int)((uint)item.GetHashCode() % 64);
            hash = Vector256.BitwiseOr(hash, Vector256.ShiftLeft(One[slot], bit));
        }

        foreach (var item in ParentTypes(typeof(TestType)))
        {
            var slot = (int)((uint)item.FullName!.GetHashCode() % 256 / 64);
            var bit = (int)((uint)item.FullName!.GetHashCode() % 64);
            hash = Vector256.BitwiseOr(hash, Vector256.ShiftLeft(One[slot], bit));
        }

        for (var slot = 0; slot < Vector256<long>.Count; slot++)
        {
            output.WriteLine($"{slot}: {hash[slot]:b64}");
        }
        
        output.WriteLine($"Bloom: {bloom}");
    }

}
