using System;
using System.Net;
using System.Text;

namespace StackExchange.Redis
{
    /// <summary>
    /// Notification of errors from the redis server
    /// </summary>
    public sealed class RedisErrorEventArgs : EventArgs, ICompletable
    {
        private readonly EndPoint endpoint;
        private readonly EventHandler<RedisErrorEventArgs> handler;
        private readonly string message;
        private readonly object sender;
        internal RedisErrorEventArgs(
            EventHandler<RedisErrorEventArgs> handler, object sender,
            EndPoint endpoint, string message)
        {
            this.handler = handler;
            this.sender = sender;
            this.message = message;
            this.endpoint = endpoint;
        }

        /// <summary>
        /// The origin of the message
        /// </summary>
        public EndPoint EndPoint { get { return endpoint; } }

        /// <summary>
        /// The message from the server
        /// </summary>
        public string Message { get { return message; } }

        void ICompletable.AppendStormLog(StringBuilder sb)
        {
            sb.Append("event, error: ").Append(message);
        }

        bool ICompletable.TryComplete(bool isAsync)
        {
            return ConnectionMultiplexer.TryCompleteHandler(handler, sender, this, isAsync);
        }

    }
}
