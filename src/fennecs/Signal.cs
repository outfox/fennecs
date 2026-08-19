// SPDX-License-Identifier: MIT

namespace fennecs;

/// <summary>
/// Handler invoked right <b>after</b> a Component of type <typeparamref name="T"/> was added to an Entity.
/// </summary>
/// <remarks>
/// The Component is already stored: <paramref name="value"/> is a reference into its Storage, and
/// writes through it are visible to everyone. The reference is only valid for the duration of the call.
/// </remarks>
/// <param name="entity">
/// the Entity that received the Component, as a live <see cref="EntityRef"/> — its accessors read the
/// Archetype storage directly. Convert it to a storable <see cref="fennecs.Entity"/> to keep it around.
/// </param>
/// <param name="value">reference to the newly stored Component value</param>
public delegate void ComponentAdded<T>(EntityRef entity, ref T value);


/// <summary>
/// Handler invoked right <b>before</b> a Component of type <typeparamref name="T"/> is removed from an Entity.
/// </summary>
/// <remarks>
/// The Component is still stored when this runs (so its value can be read, and the Entity still
/// reports <c>Has&lt;T&gt;()</c>). This is what makes the outgoing value observable at all — after the
/// structural change it is gone. The reference is only valid for the duration of the call.
/// </remarks>
/// <param name="entity">
/// the Entity that is losing the Component, as a live <see cref="EntityRef"/> — its accessors read the
/// Archetype storage directly. Convert it to a storable <see cref="fennecs.Entity"/> to keep it around.
/// </param>
/// <param name="value">reference to the Component value about to be discarded</param>
/// <param name="cause">why the Component is going away — in particular, whether the Entity survives it</param>
public delegate void ComponentRemoved<T>(EntityRef entity, in T value, RemoveCause cause);


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

    internal Signal(TypeExpression expression) => Expression = expression;

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

    internal abstract void InvokeAdded(EntityRef entity, TypeExpression expression);
    internal abstract void InvokeRemoved(EntityRef entity, TypeExpression expression, RemoveCause cause);

    /// <inheritdoc />
    public override string ToString() => $"Signal<{Type.Name}>({Match})";
}


/// <summary>
/// Subscription point for the lifecycle of a Component type (optionally narrowed to a
/// relation target or Wildcard). Obtain one via <see cref="fennecs.World.On{T}()"/>.
/// </summary>
/// <remarks>
/// <para>
/// ⚠️ Obtain one through <see cref="fennecs.World.On{T}()"/> at every subscription, and do not keep it
/// in a field: <see cref="fennecs.World.GC"/> drops Signals that have no subscribers left, and a stored
/// one outlives that drop as an orphan — still accepting handlers, never firing again.
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
    internal Signal(TypeExpression expression) : base(expression) { }

    /// <summary>
    /// Raised just after a matching Component was added to an Entity.
    /// </summary>
    public event ComponentAdded<T>? Added;

    /// <summary>
    /// Raised just before a matching Component is removed from an Entity — including when the
    /// Entity is despawned, truncated away, or loses the Component to relation cleanup.
    /// The <see cref="RemoveCause"/> tells the three apart; in particular, whether the Entity
    /// survives the removal.
    /// </summary>
    public event ComponentRemoved<T>? Removed;

    internal override bool WantsAdded => Added is not null;
    internal override bool WantsRemoved => Removed is not null;

    internal override void InvokeAdded(EntityRef entity, TypeExpression expression)
    {
        var handler = Added;
        if (handler is null) return;

        ref var value = ref entity.Ref<T>(expression.Match);
        handler(entity, ref value);
    }

    internal override void InvokeRemoved(EntityRef entity, TypeExpression expression, RemoveCause cause)
    {
        var handler = Removed;
        if (handler is null) return;

        ref var value = ref entity.Ref<T>(expression.Match);
        handler(entity, in value, cause);
    }
}
