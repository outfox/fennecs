namespace fennecs;

public interface CRUD<B>
{
    public B Add<T>(T value, Relate target = default) where T : notnull;
    public B Add<T>(T value, Link<T> target) where T : class;
}
