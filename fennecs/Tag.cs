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
}
