// SPDX-License-Identifier: MIT

using System.Numerics;
using System.Runtime.CompilerServices;

namespace fennecs;

public partial class World
{
    #region Aspects

    private readonly List<Aspect> _aspects = [];

    // Flat routing table: TypeID -> owning Aspect (null = unregistered, routes to Main).
    // Starts small and grows to fit the highest registered TypeID.
    private Aspect?[] _aspectByTypeId = new Aspect?[16];

    // Types that have been materialized in an Archetype anywhere in this World; their ownership is frozen.
    private readonly HashSet<TypeID> _materializedTypes = [];

    private readonly int _initialCapacity;


    /// <summary>
    /// All Aspects of this World. The first entry is always <see cref="Main"/>.
    /// </summary>
    public IReadOnlyList<Aspect> Aspects => _aspects;


    /// <summary>
    /// When true, every Component type must be registered to an Aspect via <see cref="Aspect.Owns{T}()"/>
    /// before use; using an unregistered type throws instead of silently routing to <see cref="Main"/>.
    /// </summary>
    public bool StrictAspects { get; init; }


    /// <summary>
    /// Adds a new Aspect to this World: a separate collection of Archetypes with its own memory layout,
    /// sharing this World's Entities. Declare the Component types it stores via <see cref="Aspect.Owns{T}()"/>.
    /// </summary>
    /// <param name="name">unique name for the Aspect within this World</param>
    /// <exception cref="ArgumentException">if an Aspect of that name already exists</exception>
    public Aspect AddAspect(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        if (_aspects.Any(aspect => aspect.Name == name)) throw new ArgumentException($"An Aspect named \"{name}\" already exists in this World.", nameof(name));

        var aspect = new Aspect(this, name, Math.Max(_initialCapacity, _identityPool.Created + 1));
        _aspects.Add(aspect);
        return aspect;
    }


    /// <inheritdoc cref="AddAspect(string)"/>
    /// <param name="name">unique name for the Aspect within this World</param>
    /// <param name="owns">Component types owned by (stored in) this Aspect</param>
    public Aspect AddAspect(string name, params Type[] owns) => AddAspect(name).Owns(owns);


    /// <summary>
    /// Returns the Aspect that stores the given Component type: its registered owner,
    /// or <see cref="Main"/> for unregistered types (unless <see cref="StrictAspects"/>).
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal Aspect AspectOf(TypeExpression type)
    {
        var typeId = type.TypeId;
        var byId = _aspectByTypeId;
        var aspect = (uint)typeId < (uint)byId.Length ? byId[typeId] : null;
        if (aspect is not null) return aspect;

        return StrictAspects ? AssertRegistered(type) : Main;
    }


    // Out-of-line so AspectOf stays inlineable on the hot path.
    [MethodImpl(MethodImplOptions.NoInlining)]
    private Aspect AssertRegistered(TypeExpression type)
    {
        if (type.TypeId == LanguageType<Identity>.Id) return Main;

        throw new InvalidOperationException(
            $"World \"{Name}\" requires Aspect ownership for all Component types (StrictAspects = true), " +
            $"but {type} is not owned by any Aspect. Register it via aspect.Owns<T>() before use.");
    }


    internal void RegisterOwnership(Aspect aspect, TypeID typeId, Type type)
    {
        // (checked by Type, not TypeID — Identity has both a reserved and a registry-assigned TypeID)
        if (type == typeof(Identity))
        {
            throw new InvalidOperationException("The Identity component is present in every Aspect and cannot be owned by one.");
        }

        var existing = (uint)typeId < (uint)_aspectByTypeId.Length ? _aspectByTypeId[typeId] : null;
        if (existing == aspect) return; // idempotent re-registration

        if (existing is not null)
        {
            throw new InvalidOperationException(
                $"Component type {type} is already owned by Aspect \"{existing.Name}\" and cannot be re-registered to \"{aspect.Name}\".");
        }

        if (_materializedTypes.Contains(typeId))
        {
            throw new InvalidOperationException(
                $"Component type {type} is already in use by Aspect \"{Main.Name}\" — ownership freezes at first use. " +
                $"Register it via Owns<T>() before the first Entity or Archetype uses the type.");
        }

        if (typeId >= _aspectByTypeId.Length) Array.Resize(ref _aspectByTypeId, (int)BitOperations.RoundUpToPowerOf2((uint)typeId + 1));
        _aspectByTypeId[typeId] = aspect;
    }


    internal void MarkMaterialized(TypeExpression type) => _materializedTypes.Add(type.TypeId);


    /// <summary>
    /// Resolves the single Aspect a Query Mask refers to, from all types in the Mask
    /// (stream types and Has/Not/Any filters). A Query matches Archetypes of exactly one Aspect.
    /// </summary>
    /// <exception cref="InvalidOperationException">if the Mask mixes types stored in different Aspects</exception>
    internal Aspect ResolveAspect(Mask mask)
    {
        if (_aspects.Count == 1) return Main;

        Aspect? resolved = null;
        var mixed = false;

        foreach (var type in mask.HasTypes.Concat(mask.NotTypes).Concat(mask.AnyTypes))
        {
            if (type.TypeId == LanguageType<Identity>.Id) continue;

            var aspect = AspectOf(type);
            resolved ??= aspect;
            if (resolved != aspect) mixed = true;
        }

        if (!mixed) return resolved ?? Main;

        var listing = string.Join("\n", mask.HasTypes.Concat(mask.NotTypes).Concat(mask.AnyTypes)
            .Where(type => type.TypeId != LanguageType<Identity>.Id)
            .Select(type => $"  {type} -> Aspect \"{AspectOf(type).Name}\""));

        throw new InvalidOperationException(
            "A Query can only match Component types stored in a single Aspect, but this Query's types span several:\n" +
            $"{listing}\n" +
            "Group hot data into the same Aspect, or access other Aspects' components via entity.Ref<T>() inside the loop.");
    }

    #endregion
}
