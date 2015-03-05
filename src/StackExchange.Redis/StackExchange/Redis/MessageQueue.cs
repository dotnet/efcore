using System.Collections.Generic;
using System.Text;

namespace StackExchange.Redis
{
    sealed partial class MessageQueue
    {
        private readonly Queue<Message>
            regular = new Queue<Message>(),
            high = new Queue<Message>();

        public object SyncLock { get { return regular; } }

        public Message Dequeue()
        {
            lock (regular)
            {
                if (high.Count != 0)
                {
                    return high.Dequeue();
                }
                if (regular.Count != 0)
                {
                    return regular.Dequeue();
                }
            }
            return null;
        }

        public Message PeekPing(out int queueLength)
        {
            lock (regular)
            {
                Message peeked;
                queueLength = high.Count + regular.Count;
                if (high.Count != 0 && (peeked = high.Peek()).Command == RedisCommand.PING)
                {
                    return peeked;
                }
                if (regular.Count != 0 && (peeked = regular.Peek()).Command == RedisCommand.PING)
                {
                    return peeked;
                }
            }
            return null;
        }

        public bool Push(Message message)
        {
            lock (regular)
            {
                (message.IsHighPriority ? high : regular).Enqueue(message);
                return high.Count + regular.Count == 1;
            }
        }

        internal bool Any()
        {
            lock (regular)
            {
                return high.Count != 0 || regular.Count != 0;
            }
        }

        internal int Count()
        {
            lock (regular)
            {
                return high.Count + regular.Count;
            }
        }

        internal Message[] DequeueAll()
        {
            lock (regular)
            {
                int count = high.Count + regular.Count;
                if (count == 0) return Message.EmptyArray;

                var arr = new Message[count];
                high.CopyTo(arr, 0);
                regular.CopyTo(arr, high.Count);
                high.Clear();
                regular.Clear();
                return arr;
            }
        }
        internal void GetStormLog(StringBuilder sb)
        {
            lock(regular)
            {
                int total = 0;
                if (high.Count == 0 && regular.Count == 0) return;
                sb.Append("Unsent: ").Append(high.Count + regular.Count).AppendLine();
                foreach (var item in high)
                {
                    if (++total >= 500) break;
                    item.AppendStormLog(sb);
                    sb.AppendLine();
                }
                foreach (var item in regular)
                {
                    if (++total >= 500) break;

                    item.AppendStormLog(sb);
                    sb.AppendLine();
                }
            }
        }
    }
}
