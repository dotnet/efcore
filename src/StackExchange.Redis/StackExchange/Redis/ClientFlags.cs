using System;

namespace StackExchange.Redis
{
    /// <summary>
    /// The client flags can be a combination of:
    /// O: the client is a slave in MONITOR mode
    /// S: the client is a normal slave server
    /// M: the client is a master
    /// x: the client is in a MULTI/EXEC context
    /// b: the client is waiting in a blocking operation
    /// i: the client is waiting for a VM I/O (deprecated)
    /// d: a watched keys has been modified - EXEC will fail
    /// c: connection to be closed after writing entire reply
    /// u: the client is unblocked
    /// A: connection to be closed ASAP
    /// N: no specific flag set
    /// </summary>
    [Flags]
    public enum ClientFlags : long
    {
        /// <summary>
        /// no specific flag set
        /// </summary>
        None = 0,
        /// <summary>
        /// the client is a slave in MONITOR mode
        /// </summary>
        SlaveMonitor = 1,
        /// <summary>
        /// the client is a normal slave server
        /// </summary>
        Slave = 2,
        /// <summary>
        /// the client is a master
        /// </summary>
        Master = 4,
        /// <summary>
        /// the client is in a MULTI/EXEC context
        /// </summary>
        Transaction = 8,
        /// <summary>
        /// the client is waiting in a blocking operation
        /// </summary>
        Blocked = 16,
        /// <summary>
        /// a watched keys has been modified - EXEC will fail
        /// </summary>
        TransactionDoomed = 32,
        /// <summary>
        /// connection to be closed after writing entire reply
        /// </summary>
        Closing = 64,
        /// <summary>
        /// the client is unblocked
        /// </summary>
        Unblocked = 128,
        /// <summary>
        /// connection to be closed ASAP
        /// </summary>
        CloseASAP = 256,

    }


}
