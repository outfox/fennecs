// SPDX-License-Identifier: MIT

namespace fennecs;

/// <summary>
/// A handler subscribed to a <see cref="Signal{T}"/>, as returned by
/// <see cref="World.SubscribeAdded{T}(ComponentAdded{T})"/> and <see cref="World.SubscribeRemoved{T}(ComponentRemoved{T})"/>.
/// Disposing it unsubscribes the handler.
/// </summary>
/// <remarks>
/// <para>
/// Owning the subscription instead of the handler is what makes lambdas safe to subscribe: there is
/// nothing to keep around to <c>-=</c> later, and no Signal is left behind with a handler nobody can
/// remove. Once its last subscription is gone, a Signal is idle, and <see cref="World.GC"/> lets go of it.
/// </para>
/// <para>
/// A Subscription lives as long as you keep it, not as long as the Component it watches: a Signal with
/// a live Subscription is never dropped by <see cref="World.GC"/>, even when no Entity stores the Component.
/// The <see cref="World"/> disposes every Subscription it still holds when it is disposed itself.
/// Disposing twice, or after the World, does nothing.
/// </para>
/// <example>
/// <code>
/// using var subscription = world.SubscribeAdded&lt;Position&gt;((entities, positions) => index.Insert(entities, positions));
/// </code>
/// </example>
/// </remarks>

public sealed class Subscription : IDisposable
{
    // Null once disposed — by its owner, or by the World being disposed.
    private World? _world;

    // Exactly what was subscribed, so Dispose can take that very handler back off.
    // (a ComponentAdded<T> when _added, else a ComponentRemoved<T>, for the T of Signal)
    private readonly Delegate _handler;
    private readonly bool _added;

    internal Subscription(World world, Signal signal, Delegate handler, bool added)
    {
        _world = world;
        Signal = signal;
        _handler = handler;
        _added = added;

        world.SubscriptionOpened(this);
    }

    /// <summary>The Signal the handler is subscribed to.</summary>
    public Signal Signal { get; }

    /// <summary>Is the handler still subscribed? (<see langword="false"/> once disposed, or once its World was)</summary>
    public bool Active => _world is not null;

    /// <summary>Unsubscribes the handler.</summary>
    public void Dispose()
    {
        var world = _world;
        if (world is null) return;
        _world = null;

        world.SubscriptionClosed(this);

        if (_added) Signal.RemoveAdded(_handler);
        else Signal.RemoveRemoved(_handler);
    }

    /// <summary>
    /// The World is being disposed and drops every handler itself: only mark this Subscription as done,
    /// without unsubscribing from a World that is going away.
    /// </summary>
    internal void Release() => _world = null;
}
