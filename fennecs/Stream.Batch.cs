namespace fennecs;

/// <summary>
/// Extension methods for <see cref="Stream"/>.
/// </summary>
public static class StreamExtensions
{
    /// <inheritdoc cref="fennecs.Query.Batch()"/>
    public static Batch Batch<C0>(this Stream<C0> stream) where C0 : notnull => stream.Query.Batch();
    
    /// <inheritdoc cref="fennecs.Query.Batch()"/>
    public static Batch Batch<C0>(this Stream<C0> stream, Batch.AddConflict add) where C0 : notnull => stream.Query.Batch(add);
    
    /// <inheritdoc cref="fennecs.Query.Batch()"/>
    public static Batch Batch<C0>(this Stream<C0> stream, Batch.RemoveConflict remove) where C0 : notnull => stream.Query.Batch(remove);

    /// <inheritdoc cref="fennecs.Query.Batch()"/>
    public static Batch Batch<C0>(this Stream<C0> stream, Batch.AddConflict add, Batch.RemoveConflict remove) where C0 : notnull => stream.Query.Batch(add, remove);
    
    
    /// <inheritdoc cref="fennecs.Query.Batch()"/>
    public static Batch Batch<C0, C1>(this Stream<C0, C1> stream) where C1 : notnull where C0 : notnull => stream.Query.Batch();
    
    /// <inheritdoc cref="fennecs.Query.Batch()"/>
    public static Batch Batch<C0, C1>(this Stream<C0, C1> stream, Batch.AddConflict add) where C1 : notnull where C0 : notnull => stream.Query.Batch(add);
    
    /// <inheritdoc cref="fennecs.Query.Batch()"/>
    public static Batch Batch<C0, C1>(this Stream<C0, C1> stream, Batch.RemoveConflict remove) where C0 : notnull where C1 : notnull => stream.Query.Batch(remove);

    /// <inheritdoc cref="fennecs.Query.Batch()"/>
    public static Batch Batch<C0, C1>(this Stream<C0, C1> stream, Batch.AddConflict add, Batch.RemoveConflict remove) where C1 : notnull where C0 : notnull => stream.Query.Batch(add, remove);
    
    
    /// <inheritdoc cref="fennecs.Query.Batch()"/>
    public static Batch Batch<C0, C1, C2>(this Stream<C0, C1, C2> stream) where C1 : notnull where C0 : notnull where C2 : notnull => stream.Query.Batch();
    
    /// <inheritdoc cref="fennecs.Query.Batch()"/>
    public static Batch Batch<C0, C1, C2>(this Stream<C0, C1, C2> stream, Batch.AddConflict add) where C1 : notnull where C0 : notnull where C2 : notnull => stream.Query.Batch(add);
    
    /// <inheritdoc cref="fennecs.Query.Batch()"/>
    public static Batch Batch<C0, C1, C2>(this Stream<C0, C1, C2> stream, Batch.RemoveConflict remove) where C0 : notnull where C1 : notnull where C2 : notnull => stream.Query.Batch(remove);

    /// <inheritdoc cref="fennecs.Query.Batch()"/>
    public static Batch Batch<C0, C1, C2>(this Stream<C0, C1, C2> stream, Batch.AddConflict add, Batch.RemoveConflict remove) where C1 : notnull where C0 : notnull where C2 : notnull => stream.Query.Batch(add, remove);
    
    
    /// <inheritdoc cref="fennecs.Query.Batch()"/>
    public static Batch Batch<C0, C1, C2, C3>(this Stream<C0, C1, C2, C3> stream) where C1 : notnull where C0 : notnull where C2 : notnull where C3 : notnull => stream.Query.Batch();
    
    /// <inheritdoc cref="fennecs.Query.Batch()"/>
    public static Batch Batch<C0, C1, C2, C3>(this Stream<C0, C1, C2, C3> stream, Batch.AddConflict add) where C1 : notnull where C0 : notnull where C2 : notnull where C3 : notnull => stream.Query.Batch(add);
    
    /// <inheritdoc cref="fennecs.Query.Batch()"/>
    public static Batch Batch<C0, C1, C2, C3>(this Stream<C0, C1, C2, C3> stream, Batch.RemoveConflict remove) where C0 : notnull where C1 : notnull where C2 : notnull where C3 : notnull => stream.Query.Batch(remove);

    /// <inheritdoc cref="fennecs.Query.Batch()"/>
    public static Batch Batch<C0, C1, C2, C3>(this Stream<C0, C1, C2, C3> stream, Batch.AddConflict add, Batch.RemoveConflict remove) where C1 : notnull where C0 : notnull where C2 : notnull where C3 : notnull => stream.Query.Batch(add, remove);
    
    
    /// <inheritdoc cref="fennecs.Query.Batch()"/>
    public static Batch Batch<C0, C1, C2, C3, C4>(this Stream<C0, C1, C2, C3, C4> stream) where C1 : notnull where C0 : notnull where C2 : notnull where C3 : notnull where C4 : notnull => stream.Query.Batch();
    
    /// <inheritdoc cref="fennecs.Query.Batch()"/>
    public static Batch Batch<C0, C1, C2, C3, C4>(this Stream<C0, C1, C2, C3, C4> stream, Batch.AddConflict add) where C1 : notnull where C0 : notnull where C2 : notnull where C3 : notnull where C4 : notnull => stream.Query.Batch(add);
    
    /// <inheritdoc cref="fennecs.Query.Batch()"/>
    public static Batch Batch<C0, C1, C2, C3, C4>(this Stream<C0, C1, C2, C3, C4> stream, Batch.RemoveConflict remove) where C0 : notnull where C1 : notnull where C2 : notnull where C3 : notnull where C4 : notnull => stream.Query.Batch(remove);

    /// <inheritdoc cref="fennecs.Query.Batch()"/>
    public static Batch Batch<C0, C1, C2, C3, C4>(this Stream<C0, C1, C2, C3, C4> stream, Batch.AddConflict add, Batch.RemoveConflict remove) where C1 : notnull where C0 : notnull where C2 : notnull where C3 : notnull where C4 : notnull => stream.Query.Batch(add, remove);
}