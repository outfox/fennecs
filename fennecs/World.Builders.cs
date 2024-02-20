namespace fennecs;

public partial class World
{
    public QueryBuilder<Entity> Query()
    {
        return new QueryBuilder<Entity>(this);
    }

    public QueryBuilder<C> Query<C>()
    {
        return new QueryBuilder<C>(this);
    }

    public QueryBuilder<C1, C2> Query<C1, C2>()
    {
        return new QueryBuilder<C1, C2>(this);
    }

    public QueryBuilder<C1, C2, C3> Query<C1, C2, C3>()
    {
        return new QueryBuilder<C1, C2, C3>(this);
    }

    public QueryBuilder<C1, C2, C3, C4> Query<C1, C2, C3, C4>()
    {
        return new QueryBuilder<C1, C2, C3, C4>(this);
    }

    public QueryBuilder<C1, C2, C3, C4, C5> Query<C1, C2, C3, C4, C5>()
    {
        return new QueryBuilder<C1, C2, C3, C4, C5>(this);
    }
}