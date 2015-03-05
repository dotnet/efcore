namespace StackExchange.Redis
{
    /// <summary>
    /// Defines the persistence behaviour of the server during shutdown
    /// </summary>
    public enum ShutdownMode
    {
        /// <summary>
        /// The data is persisted if save points are configured
        /// </summary>
        Default,
        /// <summary>
        /// The data is NOT persisted even if save points are configured
        /// </summary>
        Never,
        /// <summary>
        /// The data is persisted even if save points are NOT configured
        /// </summary>
        Always
    }

}
