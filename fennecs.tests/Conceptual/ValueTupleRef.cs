using fennecs.storage;

namespace fennecs.tests.Conceptual;

public class ValueTupleRef
{
    /* Maybe for Language version 17+ ?
    public void Test<T, U>((T rwint, U rstring) tuple) 
        where T : allows ref struct 
        where U : allows ref struct 
    {
        var x = tuple.Item1;
        var y = tuple.Item2;
        
        var tuple2 = (ref x, ref y);
    }
    */
}