namespace StackExchange.Redis
{
    /// <summary>
    /// Additional operations to perform when making a server a master
    /// </summary>
    public enum ReplicationChangeOptions
    {
        /// <summary>
        /// No additional operations
        /// </summary>
        None = 0,
        /// <summary>
        /// Set the tie-breaker key on all available masters, to specify this server
        /// </summary>
        SetTiebreaker = 1,
        /// <summary>
        /// Broadcast to the pub-sub channel to listening clients to reconfigure themselves
        /// </summary>
        Broadcast = 2,
        /// <summary>
        /// Issue a SLAVEOF to all other known nodes, making this this master of all
        /// </summary>
        EnslaveSubordinates = 4,
        /// <summary>
        /// All additional operations
        /// </summary>
        All = SetTiebreaker | Broadcast | EnslaveSubordinates
    }
}
