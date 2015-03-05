namespace StackExchange.Redis
{
    /// <summary>
    /// Specifies how to compare elements for sorting
    /// </summary>
    public enum SortType
    {
        /// <summary>
        /// Elements are interpreted as a double-precision floating point number and sorted numerically
        /// </summary>
        Numeric,
        /// <summary>
        /// Elements are sorted using their alphabetic form (Redis is UTF-8 aware as long as the !LC_COLLATE environment variable is set at the server)
        /// </summary>
        Alphabetic
    }
}
