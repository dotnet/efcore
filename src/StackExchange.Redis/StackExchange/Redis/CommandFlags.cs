using System;
namespace StackExchange.Redis
{
    /// <summary>
    /// Behaviour markers associated with a given command
    /// </summary>
    [Flags]
    public enum CommandFlags
    {
        /// <summary>
        /// Default behaviour.
        /// </summary>
        None = 0,
        /// <summary>
        /// This command may jump regular-priority commands that have not yet been written to the redis stream.
        /// </summary>
        HighPriority = 1,
        /// <summary>
        /// The caller is not interested in the result; the caller will immediately receive a default-value
        /// of the expected return type (this value is not indicative of anything at the server).
        /// </summary>
        FireAndForget = 2,


        /// <summary>
        /// This operation should be performed on the master if it is available, but read operations may
        /// be performed on a slave if no master is available. This is the default option.
        /// </summary>
        PreferMaster = 0,

        /// <summary>
        /// This operation should only be performed on the master.
        /// </summary>
        DemandMaster = 4,

        /// <summary>
        /// This operation should be performed on the slave if it is available, but will be performed on
        /// a master if no slaves are available. Suitable for read operations only.
        /// </summary>
        PreferSlave = 8,

        /// <summary>
        /// This operation should only be performed on a slave. Suitable for read operations only.
        /// </summary>
        DemandSlave = 12,

        // 16: reserved for additional "demand/prefer" options

        // 32: used for "asking" flag; never user-specified, so not visible on the public API

        /// <summary>
        /// Indicates that this operation should not be forwarded to other servers as a result of an ASK or MOVED response
        /// </summary>
        NoRedirect = 64,

        // 128: used for "internal call"; never user-specified, so not visible on the public API

        // 256: used for "retry"; never user-specified, so not visible on the public API
    }
}
