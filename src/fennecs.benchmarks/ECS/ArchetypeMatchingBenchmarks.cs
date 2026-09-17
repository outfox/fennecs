using System.Collections.Immutable;

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;

using fennecs;

namespace Benchmark.ECS;

/// <summary>
/// Archetype/query matching cost: the bit-plane + bloom path (ArchetypeBits/MaskBits) against a
/// reference reproducing the retired approach (wildcard-expanded ImmutableSortedSet set operations).
///
/// The match loop is the per-call cost of FilteredStream.Includes and the per-creation cost of
/// archetype/query matching, scaled by <see cref="Fragments"/> distinct Archetypes.
/// </summary>
[ShortRunJob]
[MemoryDiagnoser]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
[HideColumns("Job", "Error", "RatioSD")]
[MedianColumn]
public class ArchetypeMatchingBenchmarks
{
    private record struct Position(int Value);

    // Cold relation, splinters one Archetype per distinct target.
    private record struct Grouping;

    // Present on every 2nd Archetype: selectivity for the Has/Not filters.
    private record struct Tagged;

    private class BaseThing;

    private class DerivedThing : BaseThing;


    // ReSharper disable once UnusedAutoPropertyAccessor.Global
    [Params(16, 256, 4096)]
    public int Fragments { get; set; }

    private World _world = null!;
    private Archetype[] _archetypes = null!;
    private ImmutableSortedSet<TypeExpression>[] _expanded = null!;

    private FilteredStream<Position> _filtered;

    private MaskBits _compositeBits;
    private MaskBits _specificBits;
    private MaskBits _familyBits;

    private SortedSet<TypeExpression> _referenceHas = null!;
    private SortedSet<TypeExpression> _referenceNot = null!;
    private SortedSet<TypeExpression> _referenceSpecificHas = null!;


    [GlobalSetup]
    public void Setup()
    {
        _world = new World(Fragments * 2);

        var targets = new Entity[Fragments];
        for (var i = 0; i < Fragments; i++) targets[i] = _world.Spawn();

        for (var i = 0; i < Fragments; i++)
        {
            var entity = _world.Spawn()
                .Add(new Position(i))
                .Add(new Grouping(), targets[i]);

            if (i % 2 == 0) entity.Add(new Tagged());
            if (i % 3 == 0) entity.Add(new DerivedThing());
        }

        _filtered = _world.Query<Position>().Stream()
            .Has(Comp<Tagged>.Plain)
            .Not(Comp<DerivedThing>.Matching(Match.Any));

        _archetypes = _filtered.Stream.Query.Archetypes.ToArray();
        _expanded = _archetypes.Select(archetype => Expand(archetype.Signature)).ToArray();

        // Composite: plain Has + plain Not + Entity-wildcard Has.
        var composite = new Mask()
            .Has(TypeExpression.Of<Position>(Match.Plain))
            .Has(TypeExpression.Of<Grouping>(Match.Entity))
            .Not(TypeExpression.Of<Tagged>(Match.Plain));
        _compositeBits = MaskBits.Of(composite);
        _referenceHas = [TypeExpression.Of<Position>(Match.Plain), TypeExpression.Of<Grouping>(Match.Entity)];
        _referenceNot = [TypeExpression.Of<Tagged>(Match.Plain)];

        // Specific relation: exercises the keyed bloom (mass rejection) + precise confirm (one hit).
        var specific = new Mask()
            .Has(TypeExpression.Of<Position>(Match.Plain))
            .Has(TypeExpression.Of<Grouping>(Match.Relation(targets[0])));
        _specificBits = MaskBits.Of(specific);
        _referenceSpecificHas =
            [TypeExpression.Of<Position>(Match.Plain), TypeExpression.Of<Grouping>(Match.Relation(targets[0]))];

        // Inheritance-aware: no reference exists, the retired machinery could not express this.
        var family = new Mask().Has(TypeExpression.Of<BaseThing>(Match.Family));
        _familyBits = MaskBits.Of(family);
    }


    [GlobalCleanup]
    public void Cleanup() => _world.Dispose();


    // The retired matching structure: each concrete expression plus its wildcard-equivalent forms.
    private static ImmutableSortedSet<TypeExpression> Expand(Signature signature)
    {
        var builder = ImmutableSortedSet.CreateBuilder<TypeExpression>();

        foreach (var expression in signature)
        {
            builder.Add(expression);
            var key = expression.Key;

            if (key == default)
            {
                builder.Add(expression.WithKey(Key.Any));
            }
            else if (key.IsObject)
            {
                builder.Add(expression.WithKey(Key.Any));
                builder.Add(expression.WithKey(Key.Target));
                builder.Add(expression.WithKey(Key.AnyObject));
            }
            else if (key.IsEntity)
            {
                builder.Add(expression.WithKey(Key.Any));
                builder.Add(expression.WithKey(Key.Target));
                builder.Add(expression.WithKey(Key.AnyEntity));
            }
        }

        return builder.ToImmutable();
    }


    private static bool ReferenceMatches(ImmutableSortedSet<TypeExpression> expanded,
        SortedSet<TypeExpression> has, SortedSet<TypeExpression> not) =>
        !expanded.Overlaps(not) && expanded.IsSupersetOf(has);


    [Benchmark(Baseline = true)]
    public int Composite_Reference()
    {
        var count = 0;
        foreach (var expanded in _expanded)
            if (ReferenceMatches(expanded, _referenceHas, _referenceNot)) count++;
        return count;
    }


    [Benchmark]
    public int Composite_Bits()
    {
        var count = 0;
        foreach (var archetype in _archetypes)
            if (archetype.Matches(_compositeBits)) count++;
        return count;
    }


    [Benchmark]
    public int SpecificRelation_Reference()
    {
        var count = 0;
        foreach (var expanded in _expanded)
            if (expanded.IsSupersetOf(_referenceSpecificHas)) count++;
        return count;
    }


    [Benchmark]
    public int SpecificRelation_Bits()
    {
        var count = 0;
        foreach (var archetype in _archetypes)
            if (archetype.Matches(_specificBits)) count++;
        return count;
    }


    [Benchmark]
    public int Family_Bits()
    {
        var count = 0;
        foreach (var archetype in _archetypes)
            if (archetype.Matches(_familyBits)) count++;
        return count;
    }


    [Benchmark]
    public int FilteredStream_Count() => _filtered.Count;
}
