// SPDX-License-Identifier: MIT

namespace fennecs;

/// <summary>
/// Handler invoked right <b>after</b> a Component of type <typeparamref name="T"/> was added to one
/// or more Entities — once per structural operation, however many Entities it touched
/// (a single Add is simply a span of length one).
/// </summary>
/// <remarks>
/// The Components are already stored: <paramref name="values"/> is a slice of their Storage, and
/// writes through it are visible to everyone. Both spans are only valid for the duration of the call,
/// and are index-aligned: <c>values[i]</c> belongs to <c>entities[i]</c>.
/// </remarks>
/// <param name="entities">the Entities that received the Component</param>
/// <param name="values">the newly stored Component values, one per Entity</param>
public delegate void ComponentAdded<T>(ReadOnlySpan<Entity> entities, Span<T> values);


/// <summary>
/// Handler invoked right <b>before</b> a Component of type <typeparamref name="T"/> is removed from
/// one or more Entities — once per structural operation, however many Entities it touched
/// (a single Remove is simply a span of length one).
/// </summary>
/// <remarks>
/// The Components are still stored when this runs (so their values can be read, and the Entities still
/// report <c>Has&lt;T&gt;()</c>). This is what makes the outgoing values observable at all — after the
/// structural change they are gone. Both spans are only valid for the duration of the call, and are
/// index-aligned: <c>values[i]</c> belongs to <c>entities[i]</c>.
/// </remarks>
/// <param name="entities">the Entities that are losing the Component</param>
/// <param name="values">the Component values about to be discarded, one per Entity</param>
/// <param name="cause">why the Components are going away — in particular, whether the Entities survive it</param>
public delegate void ComponentRemoved<T>(ReadOnlySpan<Entity> entities, ReadOnlySpan<T> values, RemoveCause cause);


/// <summary>
/// Why a Component is being removed from an Entity, as reported to <see cref="ComponentRemoved{T}"/>.
/// </summary>
public enum RemoveCause
{
    /// <summary>
    /// The Component was removed on its own — by <c>Remove</c>, a Wildcard removal, or a Batch.
    /// <b>The Entity lives on</b> without it.
    /// </summary>
    Removed = 0,

    /// <summary>
    /// The Entity itself is going away — <c>Despawn</c>, or a bulk eviction such as
    /// <see cref="Query.Truncate"/> / <c>Clear</c> — and takes all of its Components with it.
    /// This is the last Signal you will ever see for that Entity.
    /// </summary>
    Despawned,

    /// <summary>
    /// A Relation or Link lost its <i>target</i>: the targeted Entity was despawned, so the
    /// relation Component is cleaned up. <b>The Entity holding it lives on</b> — only the
    /// relation is gone. (never reported for plain Components)
    /// </summary>
    TargetDespawned,
}


/// <summary>
/// Non-generic base of <see cref="Signal{T}"/>. Cannot be derived from outside this assembly.
/// </summary>
public abstract class Signal
{
    internal readonly TypeExpression Expression;

    /// <summary>The World that hands out this Signal, and counts its live subscriptions.</summary>
    private protected readonly World World;

    /// <summary>Is this Signal in its World's registry? (the GC takes idle ones out, subscribing puts them back)</summary>
    internal bool Registered;

    internal Signal(World world, TypeExpression expression)
    {
        World = world;
        Expression = expression;
    }

    /// <summary>
    /// The Component type this Signal watches.
    /// </summary>
    public Type Type => Expression.Type;

    /// <summary>
    /// The Match term this Signal watches: <see cref="fennecs.Match.Plain"/> for plain Components,
    /// a specific relation target, or a Wildcard covering several.
    /// </summary>
    public Match Match => Expression.Match;

    internal abstract bool WantsAdded { get; }
    internal abstract bool WantsRemoved { get; }

    /// <summary>Drops every handler, without telling the World. (only for <see cref="fennecs.World.Dispose"/>)</summary>
    internal abstract void Clear();

    /// <summary>Unsubscribes a handler a <see cref="Subscription"/> added to <see cref="Signal{T}.AfterAdded"/>.</summary>
    internal abstract void RemoveAdded(Delegate handler);

    /// <summary>Unsubscribes a handler a <see cref="Subscription"/> added to <see cref="Signal{T}.BeforeRemoved"/>.</summary>
    internal abstract void RemoveRemoved(Delegate handler);

    // All Entities live in the same Archetype of `aspect`, in consecutive rows, in this order.
    internal abstract void InvokeAdded(Aspect aspect, ReadOnlySpan<Entity> entities, TypeExpression expression);
    internal abstract void InvokeRemoved(Aspect aspect, ReadOnlySpan<Entity> entities, TypeExpression expression, RemoveCause cause);

    /// <inheritdoc />
    public override string ToString() => $"Signal<{Type.Name}>({Match})";
}


/// <summary>
/// Subscription point for the lifecycle of a Component type (optionally narrowed to a
/// relation target or Wildcard). Obtain one via <see cref="fennecs.World.On{T}()"/>.
/// </summary>
/// <remarks>
/// <para>
/// Obtain one through <see cref="fennecs.World.On{T}()"/>. Keeping it in a field is safe:
/// <see cref="fennecs.World.GC"/> only drops Signals without subscribers from the registry weakly, and
/// subscribing to a kept one registers it again.
/// </para>
/// <para>
/// Signals are dispatched while the World is <b>locked</b>: structural changes made by handlers are
/// deferred and applied once dispatch completes, so handlers may freely add, remove, spawn and despawn.
/// </para>
/// <para>
/// A Signal watching a Wildcard (e.g. <c>world.On&lt;Damage&gt;(Match.Any)</c>) fires for every
/// concrete expression it covers, and receives that concrete expression's Entity and value.
/// </para>
/// </remarks>
/// <typeparam name="T">the watched Component type</typeparam>
public sealed class Signal<T> : Signal where T : notnull
{
    internal Signal(World world, TypeExpression expression) : base(world, expression) { }

    private ComponentAdded<T>? _added;
    private ComponentRemoved<T>? _removed;

    // Explicit accessors so the World can keep an exact count of the subscribed directions:
    // that count is what lets every structural change bail out on a single field compare.

    /// <summary>
    /// Raised just after a matching Component was added to an Entity.
    /// </summary>
    public event ComponentAdded<T> AfterAdded
    {
        add
        {
            var before = _added;
            _added += value;
            if (before is null && _added is not null) World.SignalSubscribed(this);
        }
        remove
        {
            var before = _added;
            _added -= value;
            if (before is not null && _added is null) World.SignalUnsubscribed(this);
        }
    }

    /// <summary>
    /// Raised just before a matching Component is removed from an Entity — including when the
    /// Entity is despawned, truncated away, or loses the Component to relation cleanup.
    /// The <see cref="RemoveCause"/> tells the three apart; in particular, whether the Entity
    /// survives the removal.
    /// </summary>
    public event ComponentRemoved<T> BeforeRemoved
    {
        add
        {
            var before = _removed;
            _removed += value;
            if (before is null && _removed is not null) World.SignalSubscribed(this);
        }
        remove
        {
            var before = _removed;
            _removed -= value;
            if (before is not null && _removed is null) World.SignalUnsubscribed(this);
        }
    }

    internal override bool WantsAdded => _added is not null;
    internal override bool WantsRemoved => _removed is not null;

    // The World is gone: drop the handlers without telling it, so a later -= (or a Subscription
    // disposed after the World) finds nothing to unsubscribe and leaves the count alone.
    internal override void Clear()
    {
        _added = null;
        _removed = null;
    }

    /// <summary>
    /// Subscribes <paramref name="handler"/> to <see cref="AfterAdded"/>, for as long as the returned
    /// <see cref="fennecs.Subscription"/> is not disposed.
    /// </summary>
    public Subscription SubscribeAdded(ComponentAdded<T> handler)
    {
        AfterAdded += handler;
        return new(World, this, handler, added: true);
    }

    /// <summary>
    /// Subscribes <paramref name="handler"/> to <see cref="BeforeRemoved"/>, for as long as the returned
    /// <see cref="fennecs.Subscription"/> is not disposed.
    /// </summary>
    public Subscription SubscribeRemoved(ComponentRemoved<T> handler)
    {
        BeforeRemoved += handler;
        return new(World, this, handler, added: false);
    }

    // What a Subscription hands back on Dispose is the very delegate it got from the methods above.
    internal override void RemoveAdded(Delegate handler) => AfterAdded -= (ComponentAdded<T>)handler;
    internal override void RemoveRemoved(Delegate handler) => BeforeRemoved -= (ComponentRemoved<T>)handler;

    internal override void InvokeAdded(Aspect aspect, ReadOnlySpan<Entity> entities, TypeExpression expression)
    {
        var handler = _added;
        if (handler is null) return;

        handler(entities, Values(aspect, entities, expression));
    }

    internal override void InvokeRemoved(Aspect aspect, ReadOnlySpan<Entity> entities, TypeExpression expression, RemoveCause cause)
    {
        var handler = _removed;
        if (handler is null) return;

        handler(entities, Values(aspect, entities, expression), cause);
    }


    // The stored values of a run of Entities: one slice of one Storage, found with a single lookup.
    private static Span<T> Values(Aspect aspect, ReadOnlySpan<Entity> entities, TypeExpression expression)
    {
        var (archetype, row) = aspect.GetEntityMeta(entities[0]);

        System.Diagnostics.Debug.Assert(AreConsecutive(aspect, entities, archetype, row), "Signalled Entities must occupy consecutive rows of one Archetype.");

        return ((Storage<T>)archetype.GetStorage(expression)).Span.Slice(row, entities.Length);
    }

    private static bool AreConsecutive(Aspect aspect, ReadOnlySpan<Entity> entities, Archetype archetype, int row)
    {
        for (var i = 0; i < entities.Length; i++)
        {
            var meta = aspect.GetEntityMeta(entities[i]);
            if (meta.Archetype != archetype || meta.Row != row + i) return false;
        }
        return true;
    }
}
