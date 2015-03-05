using System;
using System.Net;
using System.Text;

namespace StackExchange.Redis
{
    /// <summary>
    /// Describes internal errors (mainly intended for debugging)
    /// </summary>
    public class InternalErrorEventArgs : EventArgs, ICompletable
    {
        private readonly ConnectionType connectionType;
        private readonly EndPoint endpoint;
        private readonly Exception exception;
        private readonly EventHandler<InternalErrorEventArgs> handler;
        private readonly string origin;
        private readonly object sender;
        internal InternalErrorEventArgs(EventHandler<InternalErrorEventArgs> handler, object sender, EndPoint endpoint, ConnectionType connectionType, Exception exception, string origin)
        {
            this.handler = handler;
            this.sender = sender;
            this.endpoint = endpoint;
            this.connectionType = connectionType;
            this.exception = exception;
            this.origin = origin;
        }
        /// <summary>
        /// Gets the connection-type of the failing connection
        /// </summary>
        public ConnectionType ConnectionType
        {
            get { return connectionType; }
        }

        /// <summary>
        /// Gets the failing server-endpoint (this can be null)
        /// </summary>
        public EndPoint EndPoint
        {
            get { return endpoint; }
        }
        /// <summary>
        /// Gets the exception if available (this can be null)
        /// </summary>
        public Exception Exception
        {
            get { return exception; }
        }

        /// <summary>
        /// The underlying origin of the error
        /// </summary>
        public string Origin
        {
            get { return origin; }
        }
        void ICompletable.AppendStormLog(StringBuilder sb)
        {
            sb.Append("event, internal-error: ").Append(origin);
            if (endpoint != null) sb.Append(", ").Append(Format.ToString(endpoint));
        }

        bool ICompletable.TryComplete(bool isAsync)
        {
            return ConnectionMultiplexer.TryCompleteHandler(handler, sender, this, isAsync);
        }
    }
}