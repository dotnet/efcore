using System;
using System.Text;

namespace StackExchange.Redis
{
    sealed class MessageCompletable : ICompletable
    {
        private readonly RedisChannel channel;

        private readonly Action<RedisChannel, RedisValue> handler;

        private readonly RedisValue message;

        public MessageCompletable(RedisChannel channel, RedisValue message, Action<RedisChannel, RedisValue> handler)
        {
            this.channel = channel;
            this.message = message;
            this.handler = handler;
        }

        public override string ToString()
        {
            return (string)channel;
        }
        public bool TryComplete(bool isAsync)
        {
            if (handler == null) return true;
            if (isAsync)
            {
                ConnectionMultiplexer.TraceWithoutContext("Invoking...: " + (string)channel, "Subscription");
                foreach(Action<RedisChannel, RedisValue> sub in handler.GetInvocationList())
                {
                    try { sub.Invoke(channel, message); }
                    catch { }
                }
                ConnectionMultiplexer.TraceWithoutContext("Invoke complete", "Subscription");
                return true;
            }
            // needs to be called async (unless there is nothing to do!)
            return false;
        }

        void ICompletable.AppendStormLog(StringBuilder sb)
        {
            sb.Append("event, pub/sub: ").Append((string)channel);
        }
    }
}
