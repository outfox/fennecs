// SPDX-License-Identifier: MIT

namespace fennecs;

public partial class World
{
    #region State & Storage

    // TypeID -> Signals watching that Component type. Usually zero or one entry per type;
    // several only when both plain and Wildcard expressions of the same type are watched.
    private readonly Dictionary<TypeID, List<Signal>> _signals = new();

    // Number of subscribed directions (a Signal's Added and Removed count separately).
    // Maintained by Signal<T>'s event accessors; a registered Signal nobody subscribed to is worth nothing.
    private int _subscribers;

    /// <summary>
    /// Fast bail-out for the CRUD paths: can any Signal in this World fire at all?
    /// (a field read and a compare — the structural change itself dwarfs it)
    /// </summary>
    internal bool Signalling => _subscribers > 0;

    /// <summary>
    /// Bumped whenever the set of subscribed Signals changes, so caches keyed on it (see
    /// <see cref="Archetype.WatchedForRemoval"/>) know when to recompute.
    /// </summary>
    internal int SignalVersion { get; private set; }

    internal void SignalSubscribed()
    {
        _subscribers++;
        SignalVersion++;
    }

    internal void SignalUnsubscribed()
    {
        _subscribers--;
        SignalVersion++;
    }

    internal void ResetSubscribers()
    {
        _subscribers = 0;
        SignalVersion++;
    }


    /// <summary>
    /// Could a Component of this type fire anything in this direction? An over-approximation by
    /// design: it ignores relation targets, so it stays a probe and a short scan — cheaper than
    /// the World Lock it saves the caller from taking.
    /// </summary>
    internal bool Watching(TypeID typeId, bool added)
    {
        if (_subscribers == 0) return false;
        if (!_signals.TryGetValue(typeId, out var signals)) return false;

        foreach (var signal in signals)
        {
            if (added ? signal.WantsAdded : signal.WantsRemoved) return true;
        }
        return false;
    }


    /// <summary>
    /// Would despawning this Entity fire anything? Scans what each Aspect stores for it,
    /// so the Despawn path can skip the Lock (and the dispatch) entirely.
    /// </summary>
    internal bool WatchingAnyOf(Entity entity)
    {
        if (_subscribers == 0) return false;

        // Per-Archetype, memoized: every Entity of an Archetype has the same Components, so this
        // question is answered once per Archetype per subscription change, not once per Despawn.
        foreach (var aspect in _aspects)
        {
            if (aspect.Contains(entity) && aspect.GetEntityMeta(entity).Archetype.WatchedForRemoval) return true;

            // Despawning a relation *target* strips Relations from other Entities, and those
            // TargetDespawned Signals need the Lock just as much: a handler must not run while
            // the dependency migration is half done.
            if (aspect.WatchedDependencies(entity)) return true;
        }
        return false;
    }

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
    /// world.On&lt;Position&gt;().AfterAdded += (entities, positions) => index.Insert(entities, positions);
    /// world.On&lt;Position&gt;().BeforRemoved += (entities, positions, cause) => index.Remove(entities);
    /// </code>
    /// </example>
    /// </summary>
    /// <remarks>
    /// <para>
    /// Signals are memoized per expression: calling this repeatedly returns the same instance.
    /// Subscribe through this method every time — <b>do not store the returned
    /// <see cref="Signal{T}"/></b>.
    /// </para>
    /// <para>
    /// The memoization holds only between garbage collections: <see cref="GC"/> drops every Signal
    /// left without subscribers, and a stored one survives that drop as an orphan the World no
    /// longer knows about. Subscribing to it still succeeds — and never fires again.
    /// </para>
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

        var signal = new Signal<T>(this, expression);
        signals.Add(signal);
        return signal;
    }

    #endregion

    #region Dispatch

    // Emission never locks the World itself — the call sites do, wrapping [emit + structural change]
    // in a single WorldLock and performing their own mutation through the Aspect (which is immediate,
    // and thus unaffected by the lock). Handlers therefore see a consistent World, and any structural
    // change they make is deferred until the whole operation has completed.

    /// <summary>
    /// Emits Added for a specific expression.
    /// Must be called <i>after</i> the structural change, with the Entities in their new home.
    /// </summary>
    /// <param name="aspect">the Aspect storing the Components; <see langword="null"/> routes each Component to its owning Aspect (freshly spawned Entities)</param>
    /// <param name="entities">the Entities that received the Components</param>
    /// <param name="expression"></param>
    internal void SignalAdded(Aspect? aspect, ReadOnlySpan<Entity> entities, TypeExpression expression)
    {
        if (!Watching(expression.TypeId, added: true)) return;

        EmitAdded(_signals[expression.TypeId], aspect ?? AspectOf(expression), entities, expression);
    }

    /// <summary>
    /// Emits Added for a set of Components across a run of Entities that were just given them.
    /// Must be called <i>after</i> the structural change, with the Entities in their new home.
    /// </summary>
    /// <param name="aspect"><inheritdoc cref="SignalAdded(Aspect?, ReadOnlySpan{Entity}, TypeExpression)"/></param>
    /// <param name="entities"><inheritdoc cref="SignalAdded(Aspect?, ReadOnlySpan{Entity}, TypeExpression)"/></param>
    /// <param name="types">the Components that were added to every one of them</param>
    internal void SignalAdded(Aspect? aspect, ReadOnlySpan<Entity> entities, IEnumerable<TypeExpression> types)
    {
        foreach (var expression in types)
        {
            SignalAdded(aspect, entities, expression);
        }
    }


    /// <summary>
    /// Emits Removed for a specific expression across a run of Entities that are about to lose it,
    /// or — for a Wildcard <paramref name="expression"/> — once for every stored expression it covers.
    /// Must be called <i>before</i> the structural change, while the values are still readable.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Nothing is emitted if no Signal watches the type of <paramref name="expression"/>, or if the
    /// Entities do not store it. (a plain expression is checked against the first Entity only)
    /// </para>
    /// <para>
    /// Every Entity must live in the same Archetype of <paramref name="aspect"/>, so that what one of
    /// them stores is what all of them store. <paramref name="entities"/> must not be empty and must be
    /// members of <paramref name="aspect"/>: this overload trusts its caller, the one for a collection
    /// of expressions checks it for all of them at once.
    /// </para>
    /// </remarks>
    /// <param name="aspect">the Aspect storing the Components</param>
    /// <param name="entities">the Entities that are about to lose the Component</param>
    /// <param name="expression">the Component to signal; a Wildcard expands to the stored expressions it matches</param>
    /// <param name="cause">why the Component is going away</param>
    internal void SignalRemoving(Aspect aspect, ReadOnlySpan<Entity> entities, TypeExpression expression, RemoveCause cause)
    {
        if (!Watching(expression.TypeId, added: false)) return;

        var signals = _signals[expression.TypeId];

        if (!expression.isWildcard)
        {
            if (aspect.HasComponent(entities[0], expression))
                EmitRemoved(signals, aspect, entities, expression, cause);
            return;
        }

        foreach (var stored in aspect.GetSignature(entities[0]))
        {
            if (expression.Matches(stored))
                EmitRemoved(signals, aspect, entities, stored, cause);
        }
    }

    /// <summary>
    /// Emits Removed for a set of Components across a run of Entities that are about to lose them,
    /// or — for a Wildcard pattern — for every stored expression it covers.
    /// Must be called <i>before</i> the structural change, while the values are still readable.
    /// </summary>
    /// <remarks>
    /// Every Entity must live in the same Archetype of <paramref name="aspect"/>, so that what one of
    /// them stores is what all of them store.
    /// </remarks>
    internal void SignalRemoving(Aspect aspect, ReadOnlySpan<Entity> entities, IEnumerable<TypeExpression> types, RemoveCause cause)
    {
        if (entities.IsEmpty || !aspect.Contains(entities[0])) return;

        foreach (var expression in types)
        {
            SignalRemoving(aspect, entities, expression, cause);
        }
    }

    /// <summary>
    /// Emits Removed for every Component this Aspect stores for the Entity — the Despawn case.
    /// Must be called <i>before</i> the Entity's rows are deleted.
    /// </summary>
    /// <remarks>The Entity must be a member of <paramref name="aspect"/>.</remarks>
    internal void SignalRemovingAll(Aspect aspect, Entity entity, RemoveCause cause = RemoveCause.Despawned)
    {
        foreach (var stored in aspect.GetSignature(entity))
        {
            if (!Watching(stored.TypeId, added: false)) continue;

            EmitRemoved(_signals[stored.TypeId], aspect, new ReadOnlySpan<Entity>(in entity), stored, cause);
        }
    }


    /// <summary>
    /// Does any Signal covering this expression actually have a handler for this direction?
    /// </summary>
    internal bool WatchesAny(IEnumerable<TypeExpression> types, bool added)
    {
        if (_subscribers == 0) return false;

        foreach (var type in types)
        {
            if (Watching(type.TypeId, added)) return true;
        }
        return false;
    }


    private void EmitAdded(List<Signal> signals, Aspect aspect, ReadOnlySpan<Entity> entities, TypeExpression expression)
    {
        _dispatching++;
        try
        {
            // By index, re-reading Count: a handler is allowed to call On<T>() for this same
            // Component type, which appends to the very list being walked. foreach would throw an
            // unwrapped InvalidOperationException; this simply picks the newcomer up (or not).
            for (var i = 0; i < signals.Count; i++)
            {
                var signal = signals[i];

                // Non-commutative on purpose: a Wildcard Signal covers concrete expressions, see summary
                if (!signal.Expression.Matches(expression)) continue;

                if (!signal.WantsAdded) continue;

                // A handler's fault is never the caller's fault: wrap it so the two can be told apart.
                try
                {
                    signal.InvokeAdded(aspect, entities, expression);
                }
                catch (Exception exception) when (exception is not SignalException)
                {
                    throw new SignalException(signal, entities.ToArray(), "Added", exception);
                }
            }
        }
        finally
        {
            _dispatching--;
        }
    }


    private void EmitRemoved(List<Signal> signals, Aspect aspect, ReadOnlySpan<Entity> entities, TypeExpression expression, RemoveCause cause)
    {
        _dispatching++;
        try
        {
            for (var i = 0; i < signals.Count; i++)
            {
                var signal = signals[i];

                if (!signal.Expression.Matches(expression)) continue;

                if (!signal.WantsRemoved) continue;

                try
                {
                    signal.InvokeRemoved(aspect, entities, expression, cause);
                }
                catch (Exception exception) when (exception is not SignalException)
                {
                    throw new SignalException(signal, entities.ToArray(), "Removed", exception);
                }
            }
        }
        finally
        {
            _dispatching--;
        }
    }


    #endregion
}
