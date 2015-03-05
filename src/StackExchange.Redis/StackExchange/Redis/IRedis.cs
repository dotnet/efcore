using System;
using System.Diagnostics;

namespace StackExchange.Redis
{
    /// <summary>
    /// Common operations available to all redis connections
    /// </summary>
    public partial interface IRedis : IRedisAsync
    {
        /// <summary>
        /// This command is often used to test if a connection is still alive, or to measure latency.
        /// </summary>
        /// <returns>The observed latency.</returns>
        /// <remarks>http://redis.io/commands/ping</remarks>
        TimeSpan Ping(CommandFlags flags = CommandFlags.None);
    }

    /// <summary>
    /// Represents a resumable, cursor-based scanning operation
    /// </summary>
    public interface IScanningCursor
    {
        /// <summary>
        /// Returns the cursor that represents the *active* page of results (not the pending/next page of results as returned by SCAN/HSCAN/ZSCAN/SSCAN)
        /// </summary>
        long Cursor { get; }

        /// <summary>
        /// The page size of the current operation
        /// </summary>
        int PageSize { get; }

        /// <summary>
        /// The offset into the current page
        /// </summary>
        int PageOffset { get; }
    }

    [Conditional("DEBUG")]
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    internal class IgnoreNamePrefixAttribute : Attribute
    {
        public IgnoreNamePrefixAttribute(bool ignoreEntireMethod = false)
        {
            this.IgnoreEntireMethod = ignoreEntireMethod;
        }

        public bool IgnoreEntireMethod { get; private set; }
    }
}