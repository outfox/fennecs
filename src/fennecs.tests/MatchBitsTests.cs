namespace fennecs.tests;

public class MatchBitsTests
{
    private class Animal;

    private class Fox : Animal;

    private class Fennec : Fox;

    private class Rock;

    private static readonly Entity E1 = new(1, 123, 1);
    private static readonly Entity E2 = new(1, 456, 1);

    // Archetype content: one plain, one Entity relation, one Object Link.
    private static Signature ArchetypeSignature => new(
        TypeExpression.Of<int>(Match.Plain),
        TypeExpression.Of<float>(Match.Relation(E1)),
        TypeExpression.Of<string>(Match.Link("hello")));


    private static IEnumerable<TypeExpression> AllQueryForms()
    {
        Match[] matches =
        [
            Match.Plain, Match.Any, Match.Target, Match.Entity, Match.Object,
            Match.Relation(E1), Match.Relation(E2),
        ];

        foreach (var match in matches)
        {
            yield return TypeExpression.Of<int>(match);
            yield return TypeExpression.Of<float>(match);
            yield return TypeExpression.Of<string>(match);
            yield return TypeExpression.Of<double>(match);
        }

        yield return TypeExpression.Of<string>(Match.Link("hello"));
        yield return TypeExpression.Of<string>(Match.Link("world"));
    }


    [Fact]
    public void MatchesElement_Truth_Table()
    {
        var bits = new ArchetypeBits(ArchetypeSignature);

        // Plain: only the plain int matches.
        Assert.True(bits.MatchesElement(TypeExpression.Of<int>(Match.Plain)));
        Assert.False(bits.MatchesElement(TypeExpression.Of<float>(Match.Plain)));
        Assert.False(bits.MatchesElement(TypeExpression.Of<string>(Match.Plain)));

        // Any: every present type in any key form.
        Assert.True(bits.MatchesElement(TypeExpression.Of<int>(Match.Any)));
        Assert.True(bits.MatchesElement(TypeExpression.Of<float>(Match.Any)));
        Assert.True(bits.MatchesElement(TypeExpression.Of<string>(Match.Any)));
        Assert.False(bits.MatchesElement(TypeExpression.Of<double>(Match.Any)));

        // Target: non-plain forms only.
        Assert.False(bits.MatchesElement(TypeExpression.Of<int>(Match.Target)));
        Assert.True(bits.MatchesElement(TypeExpression.Of<float>(Match.Target)));
        Assert.True(bits.MatchesElement(TypeExpression.Of<string>(Match.Target)));

        // Entity / Object Wildcards: their category only.
        Assert.True(bits.MatchesElement(TypeExpression.Of<float>(Match.Entity)));
        Assert.False(bits.MatchesElement(TypeExpression.Of<string>(Match.Entity)));
        Assert.True(bits.MatchesElement(TypeExpression.Of<string>(Match.Object)));
        Assert.False(bits.MatchesElement(TypeExpression.Of<float>(Match.Object)));

        // Specific keys: exact target only, and only on the right type.
        Assert.True(bits.MatchesElement(TypeExpression.Of<float>(Match.Relation(E1))));
        Assert.False(bits.MatchesElement(TypeExpression.Of<float>(Match.Relation(E2))));
        Assert.False(bits.MatchesElement(TypeExpression.Of<int>(Match.Relation(E1))));
        Assert.True(bits.MatchesElement(TypeExpression.Of<string>(Match.Link("hello"))));
        Assert.False(bits.MatchesElement(TypeExpression.Of<string>(Match.Link("world"))));
    }


    // Independent oracle: a query expression matches a signature iff it pairwise-matches any element.
    private static bool ReferenceContains(Signature signature, TypeExpression expression) =>
        signature.Any(expression.Matches);


    private static bool ReferenceFamilyMatches(TypeExpression query, TypeExpression candidate) =>
        query.Key == Key.Family && candidate.Key == default
        && LanguageType.IsInFamily(query.TypeId, candidate.TypeId);


    [Fact]
    public void MatchesElement_Agrees_With_Pairwise_Reference()
    {
        var signature = ArchetypeSignature;
        var bits = new ArchetypeBits(signature);

        foreach (var expression in AllQueryForms())
            Assert.Equal(ReferenceContains(signature, expression), bits.MatchesElement(expression));
    }


    [Fact]
    public void Family_Matching_Agrees_With_Inheritance_Oracle()
    {
        TypeExpression[] candidates =
        [
            TypeExpression.Of<Animal>(Match.Plain), TypeExpression.Of<Fox>(Match.Plain),
            TypeExpression.Of<Fennec>(Match.Plain), TypeExpression.Of<Rock>(Match.Plain),
            TypeExpression.Of<Fennec>(Match.Relation(E1)),
        ];
        TypeExpression[] queries =
        [
            TypeExpression.Of<Animal>(Match.Family), TypeExpression.Of<Fox>(Match.Family),
            TypeExpression.Of<Fennec>(Match.Family), TypeExpression.Of<Rock>(Match.Family),
        ];

        foreach (var candidate in candidates)
        {
            var bits = new ArchetypeBits(new Signature(candidate));
            foreach (var query in queries)
                Assert.Equal(ReferenceFamilyMatches(query, candidate), bits.MatchesElement(query));
        }

        foreach (var candidate in candidates)
        {
            var clause = ClauseBits.Of([candidate]);
            foreach (var query in queries)
                Assert.Equal(ReferenceFamilyMatches(query, candidate), clause.MatchesBidirectional(query));
        }

        foreach (var query in queries)
        {
            var clause = ClauseBits.Of([query]);
            foreach (var candidate in candidates)
                Assert.Equal(ReferenceFamilyMatches(query, candidate), clause.MatchesBidirectional(candidate));
        }
    }


    // The historical formula of Archetype.Matches(Mask), rebuilt on the pairwise oracle.
    private static bool ReferenceMatches(Signature signature, Mask mask)
    {
        if (mask.NotTypes.Any(e => ReferenceContains(signature, e))) return false;
        if (!mask.HasTypes.All(e => ReferenceContains(signature, e))) return false;
        return mask.AnyTypes.Count == 0 || mask.AnyTypes.Any(e => ReferenceContains(signature, e));
    }


    private static void AssertMaskAgreement(Signature signature, Mask mask, bool? expected = null)
    {
        var actual = new ArchetypeBits(signature).Matches(MaskBits.Of(mask));
        Assert.Equal(ReferenceMatches(signature, mask), actual);
        if (expected is not null) Assert.Equal(expected, actual);
    }


    [Fact]
    public void Mask_Scenarios()
    {
        var signature = ArchetypeSignature;

        AssertMaskAgreement(signature, new Mask(), expected: true);
        AssertMaskAgreement(signature, new Mask().Has(TypeExpression.Of<int>(Match.Plain)), expected: true);
        AssertMaskAgreement(signature, new Mask().Has(TypeExpression.Of<double>(Match.Plain)), expected: false);
        AssertMaskAgreement(signature, new Mask()
            .Has(TypeExpression.Of<int>(Match.Plain))
            .Has(TypeExpression.Of<float>(Match.Relation(E1))), expected: true);
        AssertMaskAgreement(signature, new Mask()
            .Has(TypeExpression.Of<float>(Match.Relation(E2))), expected: false);

        // Not overrides Has.
        AssertMaskAgreement(signature, new Mask()
            .Has(TypeExpression.Of<int>(Match.Plain))
            .Not(TypeExpression.Of<string>(Match.Object)), expected: false);
        AssertMaskAgreement(signature, new Mask()
            .Not(TypeExpression.Of<double>(Match.Any)), expected: true);
        AssertMaskAgreement(signature, new Mask()
            .Not(TypeExpression.Of<float>(Match.Relation(E2))), expected: true);
        AssertMaskAgreement(signature, new Mask()
            .Not(TypeExpression.Of<float>(Match.Relation(E1))), expected: false);

        // Any: at least one entry.
        AssertMaskAgreement(signature, new Mask()
            .Any(TypeExpression.Of<double>(Match.Plain))
            .Any(TypeExpression.Of<string>(Match.Target)), expected: true);
        AssertMaskAgreement(signature, new Mask()
            .Any(TypeExpression.Of<double>(Match.Plain))
            .Any(TypeExpression.Of<long>(Match.Any)), expected: false);
    }


    [Fact]
    public void Randomized_Mask_Agreement()
    {
        TypeExpression[] signaturePool =
        [
            TypeExpression.Of<int>(Match.Plain),
            TypeExpression.Of<float>(Match.Plain),
            TypeExpression.Of<double>(Match.Plain),
            TypeExpression.Of<long>(Match.Plain),
            TypeExpression.Of<float>(Match.Relation(E1)),
            TypeExpression.Of<float>(Match.Relation(E2)),
            TypeExpression.Of<long>(Match.Relation(E1)),
            TypeExpression.Of<string>(Match.Link("hello")),
            TypeExpression.Of<string>(Match.Link("world")),
        ];

        var queryPool = AllQueryForms().ToArray();
        var random = new Random(20260805);

        for (var round = 0; round < 10_000; round++)
        {
            var content = signaturePool.Where(_ => random.Next(2) == 0).ToArray();
            var signature = new Signature(content);

            var mask = new Mask();
            for (var i = random.Next(0, 4); i > 0; i--) mask.Has(queryPool[random.Next(queryPool.Length)]);
            for (var i = random.Next(0, 3); i > 0; i--) mask.Not(queryPool[random.Next(queryPool.Length)]);
            for (var i = random.Next(0, 3); i > 0; i--) mask.Any(queryPool[random.Next(queryPool.Length)]);

            AssertMaskAgreement(signature, mask);
        }
    }


    [Fact]
    public void MatchesBidirectional_Agrees_With_Pairwise_Scan()
    {
        var pool = AllQueryForms().ToArray();
        var random = new Random(99);

        for (var round = 0; round < 2000; round++)
        {
            var clause = pool.Where(_ => random.Next(3) == 0).ToArray();
            var bits = ClauseBits.Of(clause);

            foreach (var expression in pool)
            {
                var reference = clause.Any(entry =>
                    expression.Equals(entry) || expression.Matches(entry) || entry.Matches(expression));
                Assert.Equal(reference, bits.MatchesBidirectional(expression));
            }
        }
    }


    [Fact]
    public void Bloom_False_Positive_Falls_Through_To_Precise_No()
    {
        var target = TypeExpression.Of<float>(Match.Relation(E1));
        var signature = new Signature(TypeExpression.Of<int>(Match.Plain), target);
        var bits = new ArchetypeBits(signature);
        var targetPattern = KeyBloom.Of(target.Raw);

        // Brute-force a different relation whose bloom pattern is covered by the target's.
        var found = false;
        for (uint index = 1; index < 500_000 && !found; index++)
        {
            if (index == 123) continue;

            var candidate = TypeExpression.Of<float>(Match.Relation(new Entity(1, index, 1)));
            if (!targetPattern.MayContain(KeyBloom.Of(candidate.Raw))) continue;

            found = true;
            Assert.False(bits.MatchesElement(candidate));
            Assert.False(bits.Overlaps(ClauseBits.Of([candidate])));
            Assert.False(bits.IsSupersetOf(ClauseBits.Of([candidate])));
        }

        Assert.True(found, "No bloom collision found in 500k candidates — hash may be degenerate.");
    }


    [Fact]
    public void Empty_Signature_Behaves()
    {
        var signature = new Signature(Array.Empty<TypeExpression>());
        var bits = new ArchetypeBits(signature);

        Assert.False(bits.MatchesElement(TypeExpression.Of<int>(Match.Any)));
        AssertMaskAgreement(signature, new Mask(), expected: true);
        AssertMaskAgreement(signature, new Mask().Has(TypeExpression.Of<int>(Match.Plain)), expected: false);
    }
}
