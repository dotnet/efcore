namespace StackExchange.Redis
{
    /// <summary>
    /// The direction in which to sequence elements
    /// </summary>
    public enum Order
    {
        /// <summary>
        /// Ordered from low values to high values
        /// </summary>
        Ascending,
        /// <summary>
        /// Ordered from high values to low values
        /// </summary>
        Descending
    }
}
