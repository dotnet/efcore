using System;
using System.Threading.Tasks;

namespace StackExchange.Redis
{
    /// <summary>
    /// Common operations available to all redis connections
    /// </summary>
    public partial interface IRedisAsync
    {
        /// <summary>
        /// Gets the multiplexer that created this instance
        /// </summary>
        ConnectionMultiplexer Multiplexer { get; }

        /// <summary>
        /// This command is often used to test if a connection is still alive, or to measure latency.
        /// </summary>
        /// <returns>The observed latency.</returns>
        /// <remarks>http://redis.io/commands/ping</remarks>
        Task<TimeSpan> PingAsync(CommandFlags flags = CommandFlags.None);
        /// <summary>
        /// Wait for a given asynchronous operation to complete (or timeout), reporting which
        /// </summary>
        bool TryWait(Task task);

        /// <summary>
        /// Wait for a given asynchronous operation to complete (or timeout)
        /// </summary>
        void Wait(Task task);
        /// <summary>
        /// Wait for a given asynchronous operation to complete (or timeout)
        /// </summary>
        T Wait<T>(Task<T> task);
        /// <summary>
        /// Wait for the given asynchronous operations to complete (or timeout)
        /// </summary>

        void WaitAll(params Task[] tasks);
    }
}
