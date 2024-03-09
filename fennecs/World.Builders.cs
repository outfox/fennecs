namespace fennecs;

public partial class World
{
    public QueryBuilder<Identity> Query()
    {
        return new QueryBuilder<Identity>(this);
    }


    public QueryBuilder<C> Query<C>()
    {
        return new QueryBuilder<C>(this, Match.Any);
    }

    public QueryBuilder<C> Query<C>(Identity match)
    {
        return new QueryBuilder<C>(this, match);
    }


    public QueryBuilder<C1, C2> Query<C1, C2>()
    {
        return new QueryBuilder<C1, C2>(this, Match.Any, Match.Any);
    }


    public QueryBuilder<C1, C2> Query<C1, C2>(Identity match1, Identity match2)
    {
        return new QueryBuilder<C1, C2>(this, match1, match2);
    }


    public QueryBuilder<C1, C2, C3> Query<C1, C2, C3>()
    {
        return new QueryBuilder<C1, C2, C3>(this, Match.Any, Match.Any, Match.Any);
    }


    public QueryBuilder<C1, C2, C3> Query<C1, C2, C3>(Identity match1, Identity match2, Identity match3)
    {
        return new QueryBuilder<C1, C2, C3>(this, match1, match2, match3);
    }


    public QueryBuilder<C1, C2, C3, C4> Query<C1, C2, C3, C4>()
    {
        return new QueryBuilder<C1, C2, C3, C4>(this, Match.Any, Match.Any, Match.Any, Match.Any);
    }


    public QueryBuilder<C1, C2, C3, C4> Query<C1, C2, C3, C4>(Identity match1, Identity match2, Identity match3, Identity match4)
    {
        return new QueryBuilder<C1, C2, C3, C4>(this, match1, match2, match3, match4);
    }


    public QueryBuilder<C1, C2, C3, C4, C5> Query<C1, C2, C3, C4, C5>()
    {
        return new QueryBuilder<C1, C2, C3, C4, C5>(this, Match.Any, Match.Any, Match.Any, Match.Any, Match.Any);
    }


    public QueryBuilder<C1, C2, C3, C4, C5> Query<C1, C2, C3, C4, C5>(Identity match1, Identity match2, Identity match3, Identity match4, Identity match5)
    {
        return new QueryBuilder<C1, C2, C3, C4, C5>(this, match1, match2, match3, match4, match5);
    }
}