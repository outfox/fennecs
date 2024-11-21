namespace fennecs;

public partial record Stream<C0, C1, C2>
{
    public void RawFuture(Action<Span<C0>, ReadOnlySpan<C1>, ReadOnlySpan<C2>> action)
    {
        using var worldLock = World.Lock();

        foreach (var table in Filtered)
        {
            using var join = table.CrossJoin<C0, C1, C2>(StreamTypes.AsSpan());
            if (join.Empty) continue;

            do
            {
                var (s0, s1, s2) = join.Select;
                var span0 = s0.Span; var type0 = s0.Expression; var span1 = s1.Span; var span2 = s2.Span;
                action(span0, span1, span2);
            } while (join.Iterate());
        }
    }

}