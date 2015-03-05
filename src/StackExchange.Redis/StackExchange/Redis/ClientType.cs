namespace StackExchange.Redis
{
    /// <summary>
    /// The class of the connection
    /// </summary>
    public enum ClientType
    {
        /// <summary>
        /// Regular connections, including MONITOR connections
        /// </summary>
        Normal,
        /// <summary>
        /// Replication connections
        /// </summary>
        Slave,
        /// <summary>
        /// Subscription connections
        /// </summary>
        PubSub
    }
}
