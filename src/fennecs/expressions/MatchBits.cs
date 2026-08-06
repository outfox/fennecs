// SPDX-License-Identifier: MIT

using System.Diagnostics;

namespace fennecs;

/// <summary>A specific keyed (relation/link) clause entry with its precomputed bloom pattern.</summary>
internal readonly struct KeyedEntry(TypeExpression expression)
{
    internal readonly TypeExpression Expression = expression;
    internal readonly KeyBloom Pattern = KeyBloom.Of(expression.Raw);
}


/// <summary>
/// One compiled query clause (Has, Not, or Any): its entries bucketed by key form into TypeId
/// requirement planes, plus specific keyed entries with bloom patterns.
/// </summary>
internal readonly struct ClauseBits
{
    /// <summary>TypeIds required as plain Components.</summary>
    internal readonly TypeBits Plain;

    /// <summary>TypeIds required with the Any Wildcard (any key form, including Plain).</summary>
    internal readonly TypeBits WildAny;

    /// <summary>TypeIds required with the Target Wildcard (any non-plain key).</summary>
    internal readonly TypeBits WildTarget;

    /// <summary>TypeIds required with the Entity Wildcard (any specific Entity relation).</summary>
    internal readonly TypeBits WildEntity;

    /// <summary>TypeIds required with the Object Wildcard (any specific Object Link).</summary>
    internal readonly TypeBits WildObject;

    /// <summary>Entries with one specific relation/link target.</summary>
    internal readonly KeyedEntry[] Keyed;

    /// <summary>Union of all <see cref="Keyed"/> patterns: group-level bloom reject in one op.</summary>
    internal readonly KeyBloom KeyedBloom;

    /// <summary>TypeIds having ≥1 specific Entity-keyed entry. (bidirectional SafeFor* tests only)</summary>
    internal readonly TypeBits SpecEntity;

    /// <summary>TypeIds having ≥1 specific Object-keyed entry. (bidirectional SafeFor* tests only)</summary>
    internal readonly TypeBits SpecObject;

    /// <summary>Family (inheritance-aware) entries: match plain Components of their type or any derived type.</summary>
    internal readonly KeyedEntry[] Family;

    /// <summary>TypeIds of the plain entries, for the bidirectional Family arm. (SafeFor* tests only)</summary>
    internal readonly TypeID[] PlainIds;

    internal readonly bool IsEmpty;

    internal static readonly ClauseBits Empty = Of([]);


    private ClauseBits(TypeBits plain, TypeBits wildAny, TypeBits wildTarget, TypeBits wildEntity,
        TypeBits wildObject, KeyedEntry[] keyed, KeyBloom keyedBloom, TypeBits specEntity, TypeBits specObject,
        KeyedEntry[] family, TypeID[] plainIds)
    {
        Plain = plain;
        WildAny = wildAny;
        WildTarget = wildTarget;
        WildEntity = wildEntity;
        WildObject = wildObject;
        Keyed = keyed;
        KeyedBloom = keyedBloom;
        SpecEntity = specEntity;
        SpecObject = specObject;
        Family = family;
        PlainIds = plainIds;
        IsEmpty = plain.IsEmpty && wildAny.IsEmpty && wildTarget.IsEmpty && wildEntity.IsEmpty
                  && wildObject.IsEmpty && keyed.Length == 0 && family.Length == 0;
    }


    internal static ClauseBits Of(IEnumerable<TypeExpression> clause)
    {
        List<TypeID> plain = [], wildAny = [], wildTarget = [], wildEntity = [], wildObject = [];
        List<TypeID> specEntity = [], specObject = [];
        List<KeyedEntry> keyed = [], family = [];
        var keyedBloom = KeyBloom.Empty;

        foreach (var expression in clause)
        {
            var key = expression.Key;
            if (key == default)
            {
                plain.Add(expression.TypeId);
            }
            else if (key.IsWildcard)
            {
                switch (key.Kind)
                {
                    case SecondaryKind.Any: wildAny.Add(expression.TypeId); break;
                    case SecondaryKind.Target: wildTarget.Add(expression.TypeId); break;
                    case SecondaryKind.Entity: wildEntity.Add(expression.TypeId); break;
                    case SecondaryKind.Object: wildObject.Add(expression.TypeId); break;
                    case SecondaryKind.Family: family.Add(new(expression)); break;
                    default:
                        throw new NotSupportedException($"Unsupported Wildcard in clause: {expression}");
                }
            }
            else
            {
                Debug.Assert(key.IsRelation, $"Clause entry is neither Plain, Wildcard, nor specific: {expression}");
                var entry = new KeyedEntry(expression);
                keyed.Add(entry);
                keyedBloom = keyedBloom.Union(entry.Pattern);
                (key.IsObject ? specObject : specEntity).Add(expression.TypeId);
            }
        }

        return new(
            Plane(plain), Plane(wildAny), Plane(wildTarget), Plane(wildEntity), Plane(wildObject),
            keyed.ToArray(), keyedBloom, Plane(specEntity), Plane(specObject),
            family.ToArray(), plain.ToArray());
    }


    private static TypeBits Plane(List<TypeID> typeIds)
    {
        if (typeIds.Count == 0) return TypeBits.Empty;

        var max = 0;
        foreach (var id in typeIds) max = Math.Max(max, id);

        var words = TypeBits.AllocateFor(max);
        foreach (var id in typeIds) TypeBits.Set(words, id);
        return new(words);
    }


    /// <summary>
    /// Bidirectional single-expression test: does <paramref name="expression"/> match, or get matched by,
    /// any clause entry? (the symmetrized pairwise semantics of Batch conflict checks)
    /// </summary>
    internal bool MatchesBidirectional(TypeExpression expression)
    {
        var typeId = expression.TypeId;
        var key = expression.Key;

        if (key == default) return Plain.Get(typeId) || WildAny.Get(typeId) || FamilyCovers(typeId);

        if (!key.IsWildcard)
        {
            if (WildAny.Get(typeId) || WildTarget.Get(typeId)) return true;
            if ((key.IsObject ? WildObject : WildEntity).Get(typeId)) return true;

            if (Keyed.Length == 0 || !KeyedBloom.MayContain(KeyBloom.Of(expression.Raw))) return false;
            foreach (var entry in Keyed)
                if (entry.Expression.Equals(expression)) return true;
            return false;
        }

        var anyWildPlane = WildAny.Get(typeId) || WildTarget.Get(typeId)
                           || WildEntity.Get(typeId) || WildObject.Get(typeId);

        return key.Kind switch
        {
            SecondaryKind.Any => Plain.Get(typeId) || SpecEntity.Get(typeId) || SpecObject.Get(typeId)
                                 || anyWildPlane,
            SecondaryKind.Target => SpecEntity.Get(typeId) || SpecObject.Get(typeId) || anyWildPlane,
            SecondaryKind.Entity => SpecEntity.Get(typeId) || WildAny.Get(typeId) || WildTarget.Get(typeId)
                                    || WildEntity.Get(typeId),
            SecondaryKind.Object => SpecObject.Get(typeId) || WildAny.Get(typeId) || WildTarget.Get(typeId)
                                    || WildObject.Get(typeId),
            SecondaryKind.Family => WildAny.Get(typeId) || WildTarget.Get(typeId) || FamilyIdPresent(typeId)
                                    || PlainDescendsFrom(typeId),
            _ => throw new NotSupportedException($"Unsupported Wildcard expression: {expression}"),
        };
    }


    // Does any Family entry cover this plain TypeId? (same type, or one of its base classes)
    private bool FamilyCovers(TypeID typeId)
    {
        if (Family.Length == 0) return false;

        foreach (var entry in Family)
        {
            var baseId = entry.Expression.TypeId;
            if (baseId == typeId) return true;

            if (LanguageType.IsInFamily(baseId, typeId)) return true;
        }

        return false;
    }


    private bool FamilyIdPresent(TypeID typeId)
    {
        foreach (var entry in Family)
            if (entry.Expression.TypeId == typeId) return true;
        return false;
    }


    // Does any plain entry name this type, or a type derived from it?
    private bool PlainDescendsFrom(TypeID baseId)
    {
        foreach (var plainId in PlainIds)
        {
            if (plainId == baseId) return true;

            if (LanguageType.IsInFamily(baseId, plainId)) return true;
        }

        return false;
    }
}


/// <summary>A compiled <see cref="Mask"/>: its three clauses in bit-testable form.</summary>
internal readonly struct MaskBits
{
    internal readonly ClauseBits Has;
    internal readonly ClauseBits Not;
    internal readonly ClauseBits Any;

    /// <summary>Hoisted: an empty Any clause always passes.</summary>
    internal readonly bool AnyEmpty;


    private MaskBits(ClauseBits has, ClauseBits not, ClauseBits any)
    {
        Has = has;
        Not = not;
        Any = any;
        AnyEmpty = any.IsEmpty;
    }


    internal static MaskBits Of(Mask mask) =>
        new(ClauseBits.Of(mask.HasTypes), ClauseBits.Of(mask.NotTypes), ClauseBits.Of(mask.AnyTypes));
}


/// <summary>
/// An Archetype's Signature in bit-testable form: per-key-kind TypeId presence planes, plus a bloom
/// filter over its specific keyed (relation/link) expressions.
/// </summary>
/// <remarks>
/// Archetype signatures contain no Wildcards, so the three planes answer every type-level Wildcard
/// question exactly; only specific-key membership needs the bloom (certain NO) and the exact
/// Signature (certain verdict).
/// </remarks>
internal readonly struct ArchetypeBits
{
    /// <summary>TypeIds present as plain Components.</summary>
    internal readonly TypeBits Plain;

    /// <summary>TypeIds present with ≥1 specific Entity-relation key.</summary>
    internal readonly TypeBits Entity;

    /// <summary>TypeIds present with ≥1 specific Object-Link key.</summary>
    internal readonly TypeBits Object;

    /// <summary>Bloom over all specific keyed expressions.</summary>
    internal readonly KeyBloom Keyed;

    /// <summary>
    /// Bloom over the Family expressions of every plain Component's base classes:
    /// a base-type Family query tests one pattern; a miss is certain absence of any derived Component.
    /// </summary>
    internal readonly KeyBloom Family;

    // The exact signature is the precise oracle for specific-key membership after a bloom maybe.
    private readonly Signature _signature;


    internal ArchetypeBits(Signature signature)
    {
        _signature = signature;

        List<TypeID> plain = [], entity = [], obj = [];
        var keyed = KeyBloom.Empty;
        var family = KeyBloom.Empty;

        foreach (var expression in signature)
        {
            Debug.Assert(!expression.isWildcard, $"Archetype Signatures must not contain Wildcards: {expression}");

            var key = expression.Key;
            if (key == default)
            {
                plain.Add(expression.TypeId);

                foreach (var ancestor in LanguageType.AncestorsById(expression.TypeId))
                    family = family.Union(KeyBloom.Of(FamilyRaw(ancestor)));
            }
            else
            {
                keyed = keyed.Union(KeyBloom.Of(expression.Raw));
                (key.IsObject ? obj : entity).Add(expression.TypeId);
            }
        }

        // All three planes share one allocation width so typical tests stay within one block.
        var max = 0;
        foreach (var expression in signature) max = Math.Max(max, expression.TypeId);

        Plain = Plane(plain, max);
        Entity = Plane(entity, max);
        Object = Plane(obj, max);
        Keyed = keyed;
        Family = family;
    }


    private static ulong FamilyRaw(Type type) => unchecked((uint)type.GetHashCode());


    // Precise confirm after a Family bloom maybe: is any plain Component derived from the base type?
    private bool ConfirmFamily(TypeID baseId)
    {
        foreach (var expression in _signature)
        {
            if (expression.Key != default) continue;

            if (LanguageType.IsInFamily(baseId, expression.TypeId)) return true;
        }

        return false;
    }


    // Family test: the base type itself as a plain Component, or bloom + precise derived-type confirm.
    private bool MatchesFamily(TypeID baseId) =>
        Plain.Get(baseId)
        || (Family.MayContain(KeyBloom.Of(FamilyRaw(LanguageType.Resolve(baseId)))) && ConfirmFamily(baseId));


    private static TypeBits Plane(List<TypeID> typeIds, int maxTypeId)
    {
        if (typeIds.Count == 0) return TypeBits.Empty;

        var words = TypeBits.AllocateFor(maxTypeId);
        foreach (var id in typeIds) TypeBits.Set(words, id);
        return new(words);
    }


    /// <summary>Does this Archetype contain a match for the given (possibly Wildcard) expression?</summary>
    internal bool MatchesElement(TypeExpression expression)
    {
        var key = expression.Key;
        var typeId = expression.TypeId;

        if (key == default) return Plain.Get(typeId);

        if (!key.IsWildcard)
            return (key.IsObject ? Object : Entity).Get(typeId)
                   && Keyed.MayContain(KeyBloom.Of(expression.Raw))
                   && _signature.Contains(expression);

        return key.Kind switch
        {
            SecondaryKind.Any => Plain.Get(typeId) || Entity.Get(typeId) || Object.Get(typeId),
            SecondaryKind.Target => Entity.Get(typeId) || Object.Get(typeId),
            SecondaryKind.Entity => Entity.Get(typeId),
            SecondaryKind.Object => Object.Get(typeId),
            SecondaryKind.Family => MatchesFamily(typeId),
            _ => throw new NotSupportedException($"Unsupported Wildcard expression: {expression}"),
        };
    }


    /// <summary>Does this Archetype contain a match for every entry of the clause? (Has semantics)</summary>
    internal bool IsSupersetOf(in ClauseBits clause)
    {
        if (!Plain.ContainsAll(clause.Plain)) return false;
        if (!Entity.ContainsAll(clause.WildEntity)) return false;
        if (!Object.ContainsAll(clause.WildObject)) return false;
        if (!TypeBits.ContainsAll2(clause.WildTarget, Entity, Object)) return false;
        if (!TypeBits.ContainsAll3(clause.WildAny, Plain, Entity, Object)) return false;

        foreach (var entry in clause.Family)
        {
            if (!MatchesFamily(entry.Expression.TypeId)) return false;
        }

        if (clause.Keyed.Length == 0) return true;
        if (!Keyed.MayContain(clause.KeyedBloom)) return false;

        foreach (var entry in clause.Keyed)
        {
            var gate = entry.Expression.Key.IsObject ? Object : Entity;
            if (!gate.Get(entry.Expression.TypeId)) return false;
            if (!_signature.Contains(entry.Expression)) return false;
        }

        return true;
    }


    /// <summary>Does this Archetype contain a match for at least one entry of the clause? (Not/Any semantics)</summary>
    internal bool Overlaps(in ClauseBits clause)
    {
        if (clause.Plain.Intersects(Plain)) return true;
        if (clause.WildEntity.Intersects(Entity)) return true;
        if (clause.WildObject.Intersects(Object)) return true;
        if (TypeBits.Intersects2(clause.WildTarget, Entity, Object)) return true;
        if (TypeBits.Intersects3(clause.WildAny, Plain, Entity, Object)) return true;

        foreach (var entry in clause.Family)
        {
            if (MatchesFamily(entry.Expression.TypeId)) return true;
        }

        // Group-level certain NO in one op; a bloom maybe alone must never count as a hit.
        if (clause.Keyed.Length == 0 || !Keyed.Intersects(clause.KeyedBloom)) return false;

        foreach (var entry in clause.Keyed)
        {
            var gate = entry.Expression.Key.IsObject ? Object : Entity;
            if (gate.Get(entry.Expression.TypeId)
                && Keyed.MayContain(entry.Pattern)
                && _signature.Contains(entry.Expression)) return true;
        }

        return false;
    }


    /// <summary>Does this Archetype match the compiled Mask? (Not overrides Any and Has)</summary>
    internal bool Matches(in MaskBits mask)
    {
        if (Overlaps(mask.Not)) return false;
        if (!IsSupersetOf(mask.Has)) return false;
        return mask.AnyEmpty || Overlaps(mask.Any);
    }
}
