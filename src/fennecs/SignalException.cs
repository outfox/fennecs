// SPDX-License-Identifier: MIT

namespace fennecs;

/// <summary>
/// Wraps any exception that escaped a <see cref="Signal{T}"/> handler, so a failure caused by an
/// observer can never be mistaken for a misuse of the <c>Add</c>, <c>Remove</c>, <c>Despawn</c>,
/// or <c>Batch</c> call that happened to trigger it.
/// </summary>
/// <remarks>
/// <para>
/// It covers both moments a handler can fail:
/// </para>
/// <list type="bullet">
/// <item>
/// <b>during dispatch</b>, the handler itself threw. <see cref="Signal"/> names the offending
/// subscription, and <see cref="Deferred"/> is <see langword="false"/>.
/// </item>
/// <item>
/// <b>at catch-up</b>, the handler returned cleanly, but a structural change it queued failed when
/// the World applied it (like removing a Component that is already on its way out).
/// <see cref="Deferred"/> is <see langword="true"/> and <see cref="Signal"/> is <see langword="null"/>: by then, the
/// handler is long gone from the stack.
/// </item>
/// </list>
/// <para>
/// The original exception is always the <see cref="Exception.InnerException"/>. Dispatch is
/// abandoned at the first failure; remaining handlers and Entities of that operation do not run.
/// </para>
/// </remarks>
public sealed class SignalException : Exception
{
    /// <summary>
    /// The Signal whose handler threw, or <see langword="null"/> when the failure surfaced later,
    /// applying work the handler had queued (see <see cref="Deferred"/>).
    /// </summary>
    public Signal? Signal { get; }

    /// <summary>
    /// The Entity being signalled, or the Entity the failed deferred operation targeted.
    /// (<see langword="default"/> for a deferred Batch, which has no single Entity)
    /// </summary>
    public Entity Entity { get; }

    /// <summary>
    /// <see langword="false"/> if the handler threw while it was running; else <see langword="true"/> if it completed, and
    /// the structural change it requested failed afterwards, when the World caught up.
    /// </summary>
    /// <remarks>
    /// A deferred operation can raise Signals of its own. If one of <i>those</i> handlers throws, what
    /// you catch is a dispatch-phase <see cref="SignalException"/> (<see cref="Deferred"/> is
    /// <see langword="false"/>) that surfaced from the catch-up, never one wrapped inside another.
    /// </remarks>
    public bool Deferred { get; }


    internal SignalException(Signal signal, Entity entity, string stage, Exception inner)
        : base($"A {stage} handler of {signal} threw for Entity {entity}. " +
               "The originating call is not at fault. This is a fault in the Signal handler. See InnerException.", inner)
    {
        Signal = signal;
        Entity = entity;
        Deferred = false;
    }


    internal SignalException(string operation, Entity entity, Exception inner)
        : base($"A structural change queued by a Signal handler failed when the World caught up: {operation}. " +
               "The originating call is not at fault. The Signal handler asked for something that no longer applies. See InnerException.", inner)
    {
        Signal = null;
        Entity = entity;
        Deferred = true;
    }
}
