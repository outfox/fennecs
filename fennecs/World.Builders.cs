namespace fennecs;

public partial class World
{
    public QueryBuilder<Entity> Query()
    {
        return new QueryBuilder<Entity>(this);
    }

    public QueryBuilder<C> Query<C>(Entity match = default)
    {
        return new QueryBuilder<C>(this, match);
    }
    
    public QueryBuilder<C1, C2> Query<C1, C2>()
    {
        return new QueryBuilder<C1, C2>(this, default, default);
    }

    public QueryBuilder<C1, C2> Query<C1, C2>(Entity match1, Entity match2)
    {
        return new QueryBuilder<C1, C2>(this, match1, match2);
    }

    public QueryBuilder<C1, C2, C3> Query<C1, C2, C3>()
    {
        return new QueryBuilder<C1, C2, C3>(this, default, default, default);
    }

    public QueryBuilder<C1, C2, C3> Query<C1, C2, C3>(Entity match1, Entity match2, Entity match3)
    {
        return new QueryBuilder<C1, C2, C3>(this, match1, match2, match3);
    }

    public QueryBuilder<C1, C2, C3, C4> Query<C1, C2, C3, C4>()
    {
        return new QueryBuilder<C1, C2, C3, C4>(this, default, default, default, default);
    }
    
    public QueryBuilder<C1, C2, C3, C4> Query<C1, C2, C3, C4>(Entity match1, Entity match2, Entity match3, Entity match4)
    {
        return new QueryBuilder<C1, C2, C3, C4>(this, match1, match2, match3, match4);
    }
    
    public QueryBuilder<C1, C2, C3, C4, C5> Query<C1, C2, C3, C4, C5>()
    {
        return new QueryBuilder<C1, C2, C3, C4, C5>(this, default, default, default, default, default);
    }
    
    public QueryBuilder<C1, C2, C3, C4, C5> Query<C1, C2, C3, C4, C5>(Entity match1, Entity match2, Entity match3, Entity match4, Entity match5)
    {
        return new QueryBuilder<C1, C2, C3, C4, C5>(this, match1, match2, match3, match4, match5);
    }
}