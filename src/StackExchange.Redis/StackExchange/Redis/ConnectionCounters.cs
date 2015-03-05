using System.Text;

namespace StackExchange.Redis
{
    /// <summary>
    /// Illustrates the counters associated with an individual connection
    /// </summary>
    public class ConnectionCounters
    {
        private readonly ConnectionType connectionType;

        internal ConnectionCounters(ConnectionType connectionType)
        {
            this.connectionType = connectionType;
        }

        /// <summary>
        /// The number of operations that have been completed asynchronously
        /// </summary>
        public long CompletedAsynchronously { get; internal set; }

        /// <summary>
        /// The number of operations that have been completed synchronously
        /// </summary>
        public long CompletedSynchronously { get; internal set; }

        /// <summary>
        /// The type of this connection
        /// </summary>
        public ConnectionType ConnectionType {  get {  return connectionType; } }
        /// <summary>
        /// The number of operations that failed to complete asynchronously
        /// </summary>
        public long FailedAsynchronously { get; internal set; }

        /// <summary>
        /// Indicates if there are any pending items or failures on this connection
        /// </summary>
        public bool IsEmpty
        {
            get { return PendingUnsentItems == 0 && SentItemsAwaitingResponse == 0 && ResponsesAwaitingAsyncCompletion == 0 && FailedAsynchronously == 0; }
        }

        /// <summary>
        /// Indicates the total number of messages despatched to a non-preferred endpoint, for example sent to a master
        /// when the caller stated a preference of slave
        /// </summary>
        public long NonPreferredEndpointCount { get; internal set; }

        /// <summary>
        /// The number of operations performed on this connection
        /// </summary>
        public long OperationCount { get; internal set; }

        /// <summary>
        /// Operations that have been requested, but which have not yet been sent to the server
        /// </summary>
        public int PendingUnsentItems { get; internal set; }

        /// <summary>
        /// Operations for which the response has been processed, but which are awaiting asynchronous completion
        /// </summary>
        public int ResponsesAwaitingAsyncCompletion { get; internal set; }

        /// <summary>
        /// Operations that have been sent to the server, but which are awaiting a response
        /// </summary>
        public int SentItemsAwaitingResponse { get; internal set; }

        /// <summary>
        /// The number of sockets used by this logical connection (total, including reconnects)
        /// </summary>
        public long SocketCount { get; internal set; }

        /// <summary>
        /// The number of subscriptions (with and without patterns) currently held against this connection
        /// </summary>
        public long Subscriptions { get;internal set; }

        /// <summary>
        /// Indicates the total number of outstanding items against this connection
        /// </summary>
        public int TotalOutstanding { get { return PendingUnsentItems + SentItemsAwaitingResponse + ResponsesAwaitingAsyncCompletion; } }

        /// <summary>
        /// Indicates the total number of writers items against this connection
        /// </summary>
        public int WriterCount { get; internal set; }

        /// <summary>
        /// See Object.ToString()
        /// </summary>
        public override string ToString()
        {
            var sb = new StringBuilder();
            Append(sb);
            return sb.ToString();
        }

        internal void Add(ConnectionCounters other)
        {
            if (other == null) return;
            this.CompletedAsynchronously += other.CompletedAsynchronously;
            this.CompletedSynchronously += other.CompletedSynchronously;
            this.FailedAsynchronously += other.FailedAsynchronously;
            this.OperationCount += other.OperationCount;
            this.PendingUnsentItems += other.PendingUnsentItems;
            this.ResponsesAwaitingAsyncCompletion += other.ResponsesAwaitingAsyncCompletion;
            this.SentItemsAwaitingResponse += other.SentItemsAwaitingResponse;
            this.SocketCount += other.SocketCount;
            this.Subscriptions += other.Subscriptions;
            this.WriterCount += other.WriterCount;
            this.NonPreferredEndpointCount += other.NonPreferredEndpointCount;
        }

        internal bool Any()
        {
            return CompletedAsynchronously != 0 || CompletedSynchronously != 0
                || FailedAsynchronously != 0 || OperationCount != 0
                || PendingUnsentItems != 0 || ResponsesAwaitingAsyncCompletion != 0
                || SentItemsAwaitingResponse != 0 || SocketCount != 0
                || Subscriptions != 0 || WriterCount != 0
                || NonPreferredEndpointCount != 0;
        }
        internal void Append(StringBuilder sb)
        {
            sb.Append("ops=").Append(OperationCount).Append(", qu=").Append(PendingUnsentItems)
                .Append(", qs=").Append(SentItemsAwaitingResponse).Append(", qc=").Append(ResponsesAwaitingAsyncCompletion)
                .Append(", wr=").Append(WriterCount);
            if (Subscriptions != 0) sb.Append(", subs=").Append(Subscriptions);
            if (FailedAsynchronously != 0) sb.Append(", async-fail=").Append(FailedAsynchronously);
            if (CompletedSynchronously != 0) sb.Append(", sync=").Append(CompletedSynchronously);
            if (CompletedAsynchronously != 0) sb.Append(", async=").Append(CompletedAsynchronously);
            if (SocketCount != 0) sb.Append(", socks=").Append(SocketCount);
            if (NonPreferredEndpointCount != 0) sb.Append(", non-pref=").Append(NonPreferredEndpointCount);
        }
    }

}
