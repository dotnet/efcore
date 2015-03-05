namespace StackExchange.Redis
{
    /// <summary>
    /// Indicates when this operation should be performed (only some variations are legal in a given context)
    /// </summary>
    public enum When
    {
        /// <summary>
        /// The operation should occur whether or not there is an existing value 
        /// </summary>
        Always,
        /// <summary>
        /// The operation should only occur when there is an existing value 
        /// </summary>
        Exists,
        /// <summary>
        /// The operation should only occur when there is not an existing value 
        /// </summary>
        NotExists
    }
}
