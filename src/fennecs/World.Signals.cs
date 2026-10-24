// SPDX-License-Identifier: MIT

using System.Collections;

namespace fennecs;

public partial class World
{
    #region State & Storage

    // TypeID -> Signals watching that Component type. Usually zero or one entry per type;
    // several only when both plain and Wildcard expressions of the same type are watched.
    private readonly Dictionary<TypeID, List<Signal>> _signals = new();

    // Signals the GC dropped from the registry for lack of subscribers. Weak, so they cost
    // nothing to keep and the runtime collects them once nobody holds them any more.
    private readonly Dictionary<TypeExpression, WeakReference<Signal>> _idle = new();

    /// <summary>
    /// tuple of bit with for index the type id. First bit is true if watching added, second for remove.
    /// </summary>
    private readonly BitArray _watch = new (LanguageType.MaxTypeId * 2);

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

    internal void SignalSubscribed(Signal signal)
    {
        _subscribers++;
        SignalVersion++;

        // A Signal kept past a GC is no longer in the registry: subscribing is what brings it back.
        if (!signal.Registered) Register(signal);
        Refresh(signal.Expression.TypeId);
    }

    internal void SignalUnsubscribed(Signal signal)
    {
        _subscribers--;
        SignalVersion++;
        Refresh(signal.Expression.TypeId);
    }


    // Subscriptions not yet disposed, so disposing the World can dispose them too.
    private readonly HashSet<Subscription> _subscriptions = [];

    internal void SubscriptionOpened(Subscription subscription) => _subscriptions.Add(subscription);

    internal void SubscriptionClosed(Subscription subscription) => _subscriptions.Remove(subscription);


    // Recomputes the watch bits of one Component type from the Signals that cover it.
    private void Refresh(TypeID typeId)
    {
        // From scratch: the last unsubscribe must clear the bits, or the CRUD paths keep taking the Lock.
        _watch[typeId * 2] = false;
        _watch[typeId * 2 + 1] = false;

        if (_signals.TryGetValue(typeId, out var signals))
        {
            foreach (var signal in signals)
            {
                if (signal.WantsAdded)
                    _watch[typeId * 2] = true;
                if (signal.WantsRemoved)
                    _watch[typeId * 2 + 1] = true;
            }
        }
    }

    private void Register(Signal signal)
    {
        if (!_signals.TryGetValue(signal.Expression.TypeId, out var signals))
        {
            signals = [];
            _signals[signal.Expression.TypeId] = signals;
        }

        signals.Add(signal);
        signal.Registered = true;
        _idle.Remove(signal.Expression);
    }


    /// <summary>
    /// Drops every Signal nobody subscribes to from the registry. They are not forgotten outright:
    /// the World keeps a weak reference, so <see cref="On{T}(Match)"/> hands the very same instance
    /// to whoever still holds it, and subscribing to it re-registers it. Whatever nobody holds
    /// is collected by the runtime.
    /// </summary>
    internal void CollectIdleSignals()
    {
        foreach (var (expression, reference) in _idle)
        {
            if (!reference.TryGetTarget(out _)) _idle.Remove(expression);
        }

        if (_signals.Count == 0) return;

        var toClear = new List<TypeID>();

        foreach (var (type, signals) in _signals)
        {
            foreach (var signal in signals)
            {
                if (signal.WantsAdded || signal.WantsRemoved) continue;

                signal.Registered = false;
                _idle[signal.Expression] = new(signal);
            }

            signals.RemoveAll(static signal => !signal.Registered);
            if (signals.Count == 0) toClear.Add(type);
        }

        foreach (var key in toClear) _signals.Remove(key);
        _signals.TrimExcess();
    }


    internal void ResetSubscribers()
    {
        // The World disposes what it handed out: those Subscriptions report inactive from now on.
        foreach (var subscription in _subscriptions) subscription.Release();
        _subscriptions.Clear();

        // Subscriptions outliving the World must not count down a World that is gone.
        foreach (var signals in _signals.Values)
        {
            foreach (var signal in signals) signal.Clear();
        }
        foreach (var reference in _idle.Values)
        {
            if (reference.TryGetTarget(out var signal)) signal.Clear();
        }

        _subscribers = 0;
        SignalVersion++;
        _watch.SetAll(false);
    }


    /// <summary>
    /// Fast lookup (using an array) to check if there is any event watching the <paramref name="typeId"/>.
    /// </summary>
    internal bool Watching(TypeID typeId, bool added) => added ? _watch[typeId * 2] : _watch[typeId * 2 + 1];

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
    /// world.On&lt;Position&gt;().BeforeRemoved += (entities, positions, cause) => index.Remove(entities);
    /// </code>
    /// </example>
    /// </summary>
    /// <remarks>
    /// <para>
    /// Signals are memoized per expression: calling this repeatedly returns the same instance.
    /// To subscribe a lambda, prefer <see cref="SubscribeAdded{T}(ComponentAdded{T})"/> /
    /// <see cref="SubscribeRemoved{T}(ComponentRemoved{T})"/>: disposing the returned
    /// <see cref="Subscription"/> unsubscribes it, where <c>-=</c> would need the very same delegate.
    /// </para>
    /// <para>
    /// <see cref="GC"/> drops every Signal left without subscribers from the World's registry, but
    /// only weakly: a <see cref="Signal{T}"/> you kept is handed out again by this method, and
    /// subscribing to it re-registers it. Storing one is therefore safe; it is merely unnecessary.
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

        // Still held by someone since the last GC: the same instance goes back into service.
        if (_idle.TryGetValue(expression, out var weak) && weak.TryGetTarget(out var revived))
        {
            Register(revived);
            return (Signal<T>)revived;
        }

        var signal = new Signal<T>(this, expression);
        signal.Registered = true;
        signals.Add(signal);
        return signal;
    }


    /// <summary>
    /// Subscribes <paramref name="handler"/> to the <see cref="Signal{T}.AfterAdded"/> of plain Components
    /// of type <typeparamref name="T"/>, until the returned <see cref="Subscription"/> is disposed.
    /// </summary>
    /// <example>
    /// <code>
    /// using var subscription = world.SubscribeAdded&lt;Position&gt;((entities, positions) => index.Insert(entities, positions));
    /// </code>
    /// </example>
    public Subscription SubscribeAdded<T>(ComponentAdded<T> handler) where T : notnull => On<T>().SubscribeAdded(handler);

    /// <inheritdoc cref="SubscribeAdded{T}(ComponentAdded{T})"/>
    /// <param name="match"><inheritdoc cref="On{T}(Match)" path="/param[@name='match']"/></param>
    /// <param name="handler">the handler to subscribe</param>
    public Subscription SubscribeAdded<T>(Match match, ComponentAdded<T> handler) where T : notnull => On<T>(match).SubscribeAdded(handler);

    /// <summary>
    /// Subscribes <paramref name="handler"/> to the <see cref="Signal{T}.BeforeRemoved"/> of plain Components
    /// of type <typeparamref name="T"/>, until the returned <see cref="Subscription"/> is disposed.
    /// </summary>
    /// <example>
    /// <code>
    /// using var subscription = world.SubscribeRemoved&lt;Position&gt;((entities, positions, cause) => index.Remove(entities));
    /// </code>
    /// </example>
    public Subscription SubscribeRemoved<T>(ComponentRemoved<T> handler) where T : notnull => On<T>().SubscribeRemoved(handler);

    /// <inheritdoc cref="SubscribeRemoved{T}(ComponentRemoved{T})"/>
    /// <param name="match"><inheritdoc cref="On{T}(Match)" path="/param[@name='match']"/></param>
    /// <param name="handler">the handler to subscribe</param>
    public Subscription SubscribeRemoved<T>(Match match, ComponentRemoved<T> handler) where T : notnull => On<T>(match).SubscribeRemoved(handler);

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
