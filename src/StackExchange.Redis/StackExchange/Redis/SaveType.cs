using System;
namespace StackExchange.Redis
{
    /// <summary>
    /// The type of save operation to perform
    /// </summary>
    public enum SaveType
    {
        /// <summary>
        /// Instruct Redis to start an Append Only File rewrite process. The rewrite will create a small optimized version of the current Append Only File.
        /// </summary>
        /// <remarks>http://redis.io/commands/bgrewriteaof</remarks>
        BackgroundRewriteAppendOnlyFile,
        /// <summary>
        /// Save the DB in background. The OK code is immediately returned. Redis forks, the parent continues to serve the clients, the child saves the DB on disk then exits. A client my be able to check if the operation succeeded using the LASTSAVE command.
        /// </summary>
        /// <remarks>http://redis.io/commands/bgsave</remarks>
        BackgroundSave,
        /// <summary>
        /// Save the DB in foreground. This is almost never a good thing to do, and could cause significant blocking. Only do this if you know you need to save
        /// </summary>
        /// <remarks>http://redis.io/commands/save</remarks>
        [Obsolete("Saving on the foreground can cause significant blocking; use with extreme caution")]
        ForegroundSave
    }
}
