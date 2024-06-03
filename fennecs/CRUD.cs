namespace fennecs;

public interface IAddRemoveComponent<out SELF>
{
    public SELF Add<C>() where C : notnull, new() => Add(new C());
    public SELF Add<C>(C value) where C : notnull;
    public SELF Add<R>(R value, Entity relation) where R : notnull;
    public SELF Add<L>(Link<L> link) where L : class;
    
    /*
    public B Set<T>(T value) where T : notnull;
    public B Set<T>(T value, Entity target) where T : notnull;
    public B Set<T>(Link<T> target) where T : class;
    */
    
    public SELF Remove<C>() where C : notnull;
    public SELF Remove<R>(Entity relation) where R : notnull;
    public SELF Remove<L>(L linkedObject) where L : class => Remove(Link<L>.With(linkedObject));
    public SELF Remove<L>(Link<L> link) where L : class;
    public SELF RemoveAny(Match match);
}


public interface IHasComponent<out SELF>
{
    public bool Has<C>() where C : notnull;
    public bool Has<R>(Entity relation) where R : notnull;
    public bool Has<L>(L linkedObject) where L : class => Has(Link<L>.With(linkedObject));
    public bool Has<L>(Link<L> link) where L : class;
    public bool HasAny(Match match);
}