namespace fennecs.tests.Conceptual;

public class ParamsTests
{
    private void Test(params ReadOnlySpan<int> numbers)
    {
        Assert.Equal(0, numbers.Length);
    }

    private void MethodTester(Action<ReadOnlySpan<int>> test)
    {
        test([]);
    }
    
    [Fact]
    public void Params_Are_Correct()
    {
        Test();
    }

    [Fact]
    public void Can_Use_Params_in_Method_Group()
    {
        MethodTester(Test);
    }

}
