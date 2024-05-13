using System.Numerics;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;

namespace fennecs
{
    /// <summary>
    /// Fox Typing is a way to wrap existing types in a new type, so they can have individual
    /// storages in Archetypes, be used as individual Stream Types in Queries, and so forth.
    /// </summary>
    /// <remarks>
    /// The opposite of Duck Typing; if it contains a Fox but doesn't talk or walk like a Fox, is it still a Fox?
    /// </remarks>
    /// <typeparam name="T">any type, let the compiler sort'em out</typeparam>
    public interface Fox<T>
    {
        /// <summary>
        /// The semantically wrapped value of the <see cref="Fox{T}"/>.
        /// </summary>
        /// <returns></returns>
        public T Value { get; set; }
    }
    
    /// <summary>
    /// A 128-bit Fox, for SIMD operations.
    /// Promises that the struct will be 128 bits.
    /// (4 floats/ints)
    /// </summary>
    public interface Fox128<T> : Fox<Vector128<T>>;
    
    
    /// <summary>
    /// A 256-bit Fox, for SIMD operations.
    /// Promises that the struct will be 256 bits.
    /// (8 floats/ints)
    /// </summary>
    public interface Fox256<T> : Fox<Vector256<T>>;
}
