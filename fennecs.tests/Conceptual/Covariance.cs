// ReSharper disable MemberCanBePrivate.Global
namespace fennecs.tests.Conceptual;

public class MyClass<T> where T : MyClass<T>
{
    public T Fluent()
    {
        return (T)this;
    }
}

public class MyClass<C1, C2> : MyClass<MyClass<C1, C2>>
{ }

public class BoxingAndCovariance()
{
    public void Test2()
    {
        var test = new MyClass<int, bool>();
        var fluent = test.Fluent();

    }


    
    public record struct CoolPerson(string Value) : ICool;


    [Fact]
    public void CoolPerson_is_Cool()
    {
        var coolPerson = new CoolPerson("cool");
        coolPerson.DoCoolStuff(); // boxing!
        
        var coolOne = coolPerson as ICool; // boxing!
        coolOne.DoCoolStuff();             // no boxing
    }

    
}

public interface ICool;

public static class CoolExtension
{
    public static void DoCoolStuff(this ICool cool)
    {
        Console.WriteLine("Whoa, so cool.");
    }
}




