using System.Net;
using System.Text;

namespace StackExchange.Redis
{
    /// <summary>
    /// Illustrates the queues associates with this server
    /// </summary>
    public class ServerCounters
    {
        internal ServerCounters(EndPoint endpoint)
        {
            this.EndPoint = endpoint;
            this.Interactive = new ConnectionCounters(ConnectionType.Interactive);
            this.Subscription = new ConnectionCounters(ConnectionType.Subscription);
            this.Other = new ConnectionCounters(ConnectionType.None);
        }

        /// <summary>
        /// The endpoint to which this data relates (this can be null if the data represents all servers)
        /// </summary>
        public EndPoint EndPoint { get; private set; }

        /// <summary>
        /// Counters associated with the interactive (non pub-sub) connection
        /// </summary>
        public ConnectionCounters Interactive { get; private set; }

        /// <summary>
        /// Counters associated with other ambient activity
        /// </summary>
        public ConnectionCounters Other { get; private set; }

        /// <summary>
        /// Counters associated with the subscription (pub-sub) connection
        /// </summary>
        public ConnectionCounters Subscription { get; private set; }
        /// <summary>
        /// Indicates the total number of outstanding items against this server
        /// </summary>
        public long TotalOutstanding { get { return Interactive.TotalOutstanding + Subscription.TotalOutstanding + Other.TotalOutstanding; } }

        /// <summary>
        /// See Object.ToString();
        /// </summary>
        public override string ToString()
        {
            string prettyName = EndPoint == null ? "Total" : Format.ToString(EndPoint);
            var sb = new StringBuilder(prettyName).Append(": int ");
            Interactive.Append(sb);
            sb.Append("; sub ");
            Subscription.Append(sb);
            if (Other.Any())
            {
                sb.Append("; other ");
                Other.Append(sb);
            }
            return sb.ToString();
        }

        internal void Add(ServerCounters other)
        {
            if (other == null) return;
            this.Interactive.Add(other.Interactive);
            this.Subscription.Add(other.Subscription);
        }
    }

}
