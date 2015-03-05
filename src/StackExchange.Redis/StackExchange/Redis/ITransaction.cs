using System.Threading.Tasks;

namespace StackExchange.Redis
{
    /// <summary>
    /// Represents a group of operations that will be sent to the server as a single unit,
    /// and processed on the server as a single unit. Transactions can also include constraints
    /// (implemented via WATCH), but note that constraint checking involves will (very briefly)
    /// block the connection, since the transaction cannot be correctly committed (EXEC),
    /// aborted (DISCARD) or not applied in the first place (UNWATCH) until the responses from
    /// the constraint checks have arrived.
    /// </summary>
    /// <remarks>http://redis.io/topics/transactions</remarks>
    /// <remarks>Note that on a cluster, it may be required that all keys involved in the transaction
    /// (including constraints) are in the same hash-slot</remarks>
    public interface ITransaction : IBatch
    {
        /// <summary>
        /// Adds a precondition for this transaction
        /// </summary>
        ConditionResult AddCondition(Condition condition);

        /// <summary>
        /// Execute the batch operation, sending all queued commands to the server.
        /// </summary>
        bool Execute(CommandFlags flags = CommandFlags.None);

        /// <summary>
        /// Execute the batch operation, sending all queued commands to the server.
        /// </summary>
        Task<bool> ExecuteAsync(CommandFlags flags = CommandFlags.None);
    }
}
