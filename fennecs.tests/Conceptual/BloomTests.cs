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

    [Fact]
    public void GetInterfaces()
    {

        Vector256<uint> bloom = default;

        var hash = Vector256<long>.Zero;

        output.WriteLine($"Interfaces: {typeof(TestType).GetInterfaces().Length}");
        
        foreach (var item in typeof(TestType).GetInterfaces())
        {
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
}
