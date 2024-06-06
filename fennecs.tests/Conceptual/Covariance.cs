namespace fennecs.tests.Conceptual;

public class MyClass<T> where T : MyClass<T>
{
    public T Fluent()
    {
        return (T) this;
    }
}


public class MyClass<C1, C2> : MyClass<MyClass<C1, C2>>
{
}


public class Test()
{
    public void Test2()
    {
        var test = new MyClass<int, bool>();
        var fluent = test.Fluent();
        
    }
}