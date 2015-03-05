namespace StackExchange.Redis
{
    /// <summary>
    /// Describes an algebraic set operation that can be performed to combine multiple sets
    /// </summary>
    public enum SetOperation
    {
        /// <summary>
        /// Returns the members of the set resulting from the union of all the given sets.
        /// </summary>
        Union,
        /// <summary>
        /// Returns the members of the set resulting from the intersection of all the given sets.
        /// </summary>
        Intersect,
        /// <summary>
        /// Returns the members of the set resulting from the difference between the first set and all the successive sets.
        /// </summary>
        Difference
    }
}
