// SPDX-License-Identifier: MIT

namespace fennecs;

public partial class World
{
    #region State & Storage

    // TypeID -> Signals watching that Component type. Usually zero or one entry per type;
    // several only when both plain and Wildcard expressions of the same type are watched.
    private readonly Dictionary<TypeID, List<Signal>> _signals = new();

    /// <summary>
    /// Fast bail-out for the CRUD paths: is any Signal registered in this World at all?
    /// (a field read and a compare — the structural change itself dwarfs it)
    /// </summary>
    internal bool Signalling => _signals.Count > 0;

    // Nesting depth of Signal dispatch. Structural changes enqueued while this is non-zero were
    // requested by a handler, and are marked as such so a later failure can be attributed to it.
    private int _dispatching;

    /// <summary>Is a Signal handler on the stack right now?</summary>
    internal bool InSignalDispatch => _dispatching > 0;

    #endregion

    #region Subscription

    /// <summary>
    /// The <see cref="Signal{T}"/> for plain Components of type <typeparamref name="T"/>, to
    /// subscribe to their addition and removal.
    /// <example>
    /// <code>
    /// world.On&lt;Position&gt;().Added += (EntityRef e, ref Position p) => index.Insert(e, p);
    /// world.On&lt;Position&gt;().Removed += (EntityRef e, in Position p, RemoveCause cause) => index.Remove(e);
    /// </code>
    /// </example>
    /// </summary>
    /// <remarks>
    /// Signals are memoized per expression: calling this repeatedly returns the same instance.
    /// </remarks>
    public Signal<T> On<T>() where T : notnull => On<T>(default);


    /// <inheritdoc cref="On{T}()"/>
    /// <param name="match">
    /// narrows the Signal to a specific relation target or link, or widens it to a Wildcard
    /// (e.g. <see cref="fennecs.Match.Any"/>) covering every target of the type
    /// </param>
    public Signal<T> On<T>(Match match) where T : notnull
    {
        var expression = TypeExpression.Of<T>(match);

        if (!_signals.TryGetValue(expression.TypeId, out var signals))
        {
            signals = [];
            _signals[expression.TypeId] = signals;
        }

        foreach (var existing in signals)
        {
            if (existing.Expression == expression) return (Signal<T>)existing;
        }

        var signal = new Signal<T>(expression);
        signals.Add(signal);
        return signal;
    }

    #endregion

    #region Dispatch

    // Emission never locks the World itself — the call sites do, wrapping [emit + structural change]
    // in a single WorldLock and performing their own mutation through the Aspect (which is immediate,
    // and thus unaffected by the lock). Handlers therefore see a consistent World, and any structural
    // change they make is deferred until the whole operation has completed.

    /// <summary>Emits Added for one Component that was just stored.</summary>
    internal void SignalAdded(Aspect aspect, Entity entity, TypeExpression expression)
    {
        if (!_signals.TryGetValue(expression.TypeId, out var signals)) return;

        EmitAdded(signals, aspect, entity, expression);
    }


    /// <summary>
    /// Emits Removed for one Component that is about to be discarded, or — for a Wildcard
    /// <paramref name="pattern"/> — for every stored expression it covers.
    /// Must be called <i>before</i> the structural change, while the values are still readable.
    /// </summary>
    internal void SignalRemoving(Aspect aspect, Entity entity, TypeExpression pattern, RemoveCause cause = RemoveCause.Removed)
    {
        if (!_signals.TryGetValue(pattern.TypeId, out var signals)) return;
        if (!aspect.Contains(entity)) return;

        if (!pattern.isWildcard)
        {
            if (aspect.HasComponent(entity, pattern))
                EmitRemoved(signals, aspect, entity, pattern, cause);
            return;
        }

        foreach (var stored in aspect.GetSignature(entity))
        {
            if (pattern.Matches(stored))
                EmitRemoved(signals, aspect, entity, stored, cause);
        }
    }


    /// <summary>
    /// Emits Removed for every Component this Aspect stores for the Entity — the Despawn case.
    /// Must be called <i>before</i> the Entity's rows are deleted.
    /// </summary>
    internal void SignalRemovingAll(Aspect aspect, Entity entity, RemoveCause cause = RemoveCause.Despawned)
    {
        if (!aspect.Contains(entity)) return;


        foreach (var stored in aspect.GetSignature(entity))
        {
            if (!_signals.TryGetValue(stored.TypeId, out var signals)) continue;
            EmitRemoved(signals, aspect, entity, stored, cause);
        }
    }


    /// <summary>
    /// Emits Removed for a set of Components across a contiguous run of Archetype rows.
    /// Must be called <i>before</i> the structural change.
    /// </summary>
    internal void SignalRemovingRows(Aspect aspect, ReadOnlySpan<EntityIndex> indices, IEnumerable<TypeExpression> types, RemoveCause cause)
    {
        var watched = Watched(types);
        if (watched.Count == 0) return;


        foreach (var index in indices)
        {
            var entity = EntityFor(index);
            foreach (var (expression, signals) in watched)
                EmitRemoved(signals, aspect, entity, expression, cause);
        }
    }


    /// <summary>
    /// Emits Added for a set of Components across a batch of Entities.
    /// Must be called <i>after</i> the structural change, with the Entities in their new home.
    /// </summary>
    internal void SignalAddedRows(Aspect aspect, ReadOnlySpan<Entity> entities, IEnumerable<TypeExpression> types)
    {
        var watched = Watched(types);
        if (watched.Count == 0) return;


        foreach (var entity in entities)
        {
            foreach (var (expression, signals) in watched)
                EmitAdded(signals, aspect, entity, expression);
        }
    }


    /// <summary>
    /// Emits Added for freshly spawned Entities, routing each Component to its owning Aspect.
    /// (the <see cref="EntityTemplate"/> path writes straight into the final Archetypes)
    /// </summary>
    internal void SignalSpawned(ReadOnlySpan<Entity> entities, IReadOnlyList<TypeExpression> types)
    {
        var watched = Watched(types);
        if (watched.Count == 0) return;

        foreach (var entity in entities)
        {
            foreach (var (expression, signals) in watched)
                EmitAdded(signals, AspectOf(expression), entity, expression);
        }
    }


    /// <summary>
    /// Pre-filters a set of expressions down to those actually watched, so the per-Entity
    /// loops of the bulk paths do not re-probe the registry.
    /// </summary>
    private List<(TypeExpression expression, List<Signal> signals)> Watched(IEnumerable<TypeExpression> types)
    {
        List<(TypeExpression, List<Signal>)> watched = [];
        foreach (var type in types)
        {
            if (_signals.TryGetValue(type.TypeId, out var signals))
                watched.Add((type, signals));
        }
        return watched;
    }


    private void EmitAdded(List<Signal> signals, Aspect aspect, Entity entity, TypeExpression expression)
    {
        var entityRef = Live(aspect, entity);

        _dispatching++;
        try
        {
            foreach (var signal in signals)
            {
                // Non-commutative on purpose: a Wildcard Signal covers concrete expressions, see summary
                if (!signal.Expression.Matches(expression)) continue;

                if (!signal.WantsAdded) continue;

                // A handler's fault is never the caller's fault: wrap it so the two can be told apart.
                try
                {
                    signal.InvokeAdded(entityRef, expression);
                }
                catch (Exception exception) when (exception is not SignalException)
                {
                    throw new SignalException(signal, entity, "Added", exception);
                }
            }
        }
        finally
        {
            _dispatching--;
        }
    }


    private void EmitRemoved(List<Signal> signals, Aspect aspect, Entity entity, TypeExpression expression, RemoveCause cause)
    {
        var entityRef = Live(aspect, entity);

        _dispatching++;
        try
        {
            foreach (var signal in signals)
            {
                if (!signal.Expression.Matches(expression)) continue;

                if (!signal.WantsRemoved) continue;

                try
                {
                    signal.InvokeRemoved(entityRef, expression, cause);
                }
                catch (Exception exception) when (exception is not SignalException)
                {
                    throw new SignalException(signal, entity, "Removed", exception);
                }
            }
        }
        finally
        {
            _dispatching--;
        }
    }


    /// <summary>
    /// The Entity as handed to the handlers: a live EntityRef. The World is locked for the duration
    /// of the dispatch, so the Entity cannot move rows while they hold it.
    /// </summary>
    private static EntityRef Live(Aspect aspect, Entity entity)
    {
        ref var meta = ref aspect.GetEntityMeta(entity);
        return new(meta.Archetype, meta.Row);
    }


    #endregion
}
