// SPDX-License-Identifier: MIT

namespace fennecs.tests;

public class CompTests(ITestOutputHelper output)
{
    [Fact]
    public void Comp_has_ToString()
    {
        var plain = Comp<int>.Plain;
        output.WriteLine(plain.ToString());

        // renders as $"Comp<{typeof(T).Name}>({Expression.Match})"
        Assert.StartsWith($"Comp<{typeof(int).Name}>(", plain.ToString());
        Assert.EndsWith(")", plain.ToString());

        var other = Comp<string>.Plain;
        Assert.NotEqual(plain.ToString(), other.ToString());
    }
}
