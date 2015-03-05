using System;

namespace StackExchange.Redis
{
    /// <summary>
    /// Additional options for the MIGRATE command
    /// </summary>
    [Flags]
    public enum MigrateOptions
    {
        /// <summary>
        /// No options specified
        /// </summary>
        None = 0,
        /// <summary>
        /// Do not remove the key from the local instance.
        /// </summary>
        Copy = 1,
        /// <summary>
        /// Replace existing key on the remote instance.
        /// </summary>
        Replace = 2
    }
}
