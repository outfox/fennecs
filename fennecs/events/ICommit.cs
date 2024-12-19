namespace fennecs.events;

/// <summary>
/// Interface marking Components that must be committed (backed by a split storage with a read and a write part).
/// Used implicitly by <see cref="IModified{C}"/> but can be used expressly by user code if desired.
/// </summary>
public interface ICommit
{
    
}