namespace fennecs.tests.Conceptual;

public class CovariantRunners(ITestOutputHelper output)
{
    private class BaseClass;
    private class DerivedClass : BaseClass;

    private delegate void RefAction<C0>(ref C0 preciseOnly);
    private delegate void CovariantAction<in C0>(C0 assignableTo);


    [Fact]
    public void CanRunCovariantAction()
    {
        var runner = new Runner<DerivedClass>(new DerivedClass());
        runner.For(WriteRef);
        runner.For(WriteCovariant);
        runner.For((ref DerivedClass t) => WriteRef(ref t));
        runner.For(delegate(ref DerivedClass t) { WriteRef(ref t);});
        //runner.For((BaseClass t) => WriteCovariant(t)); // ambiguous, can't compile
        //runner.For(delegate(BaseClass t) { WriteCovariant(t);}); // ambiguous, can't compile
        runner.For((t) => WriteRef(ref t));
    }

    private void WriteCovariant(BaseClass t)
    {
        output.WriteLine($"Covariant {t}");
    }

    private void WriteRef(ref DerivedClass t)
    {
        output.WriteLine($"Ref {t}");
    }

    private struct Runner<T>(T value)
    {
        private T _value = value;

        public void For(CovariantAction<T> action)
        {
            action(_value);
        }
        
        public void For(RefAction<T> action)
        {
            action(ref _value);
        }
    }
}