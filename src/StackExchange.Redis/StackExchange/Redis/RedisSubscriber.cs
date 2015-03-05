using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace StackExchange.Redis
{

    partial class ConnectionMultiplexer
    {

        private readonly Dictionary<RedisChannel, Subscription> subscriptions = new Dictionary<RedisChannel, Subscription>();

        internal static bool TryCompleteHandler<T>(EventHandler<T> handler, object sender, T args, bool isAsync) where T : EventArgs
        {
            if (handler == null) return true;
            if (isAsync)
            {
                foreach (EventHandler<T> sub in handler.GetInvocationList())
                {
                    try
                    { sub.Invoke(sender, args); }
                    catch
                    { }
                }
                return true;
            }
            return false;
        }

        internal Task AddSubscription(RedisChannel channel, Action<RedisChannel, RedisValue> handler, CommandFlags flags, object asyncState)
        {
            if (handler != null)
            {
                lock (subscriptions)
                {
                    Subscription sub;
                    if (subscriptions.TryGetValue(channel, out sub))
                    {
                        sub.Add(handler);
                    }
                    else
                    {
                        sub = new Subscription(handler);
                        subscriptions.Add(channel, sub);
                        var task = sub.SubscribeToServer(this, channel, flags, asyncState, false);
                        if (task != null) return task;
                    }

                }
            }
            return CompletedTask<bool>.Default(asyncState);
        }

        internal ServerEndPoint GetSubscribedServer(RedisChannel channel)
        {
            if (!channel.IsNullOrEmpty)
            {
                lock (subscriptions)
                {
                    Subscription sub;
                    if (subscriptions.TryGetValue(channel, out sub))
                    {
                        return sub.GetOwner();
                    }
                }
            }
            return null;
        }

        internal void OnMessage(RedisChannel subscription, RedisChannel channel, RedisValue payload)
        {
            ICompletable completable = null;
            lock (subscriptions)
            {
                Subscription sub;
                if (subscriptions.TryGetValue(subscription, out sub))
                {
                    completable = sub.ForInvoke(channel, payload);
                }
            }
            if (completable != null) unprocessableCompletionManager.CompleteSyncOrAsync(completable);
        }

        internal Task RemoveAllSubscriptions(CommandFlags flags, object asyncState)
        {
            Task last = CompletedTask<bool>.Default(asyncState);
            lock (subscriptions)
            {
                foreach (var pair in subscriptions)
                {
                    pair.Value.Remove(null); // always wipes
                    var task = pair.Value.UnsubscribeFromServer(pair.Key, flags, asyncState, false);
                    if (task != null) last = task;
                }
                subscriptions.Clear();
            }
            return last;
        }

        internal Task RemoveSubscription(RedisChannel channel, Action<RedisChannel, RedisValue> handler, CommandFlags flags, object asyncState)
        {
            lock (subscriptions)
            {
                Subscription sub;
                if (subscriptions.TryGetValue(channel, out sub))
                {
                    if (sub.Remove(handler))
                    {
                        subscriptions.Remove(channel);
                        var task = sub.UnsubscribeFromServer(channel, flags, asyncState, false);
                        if (task != null) return task;
                    }
                }
            }
            return CompletedTask<bool>.Default(asyncState);
        }

        internal void ResendSubscriptions(ServerEndPoint server)
        {
            if (server == null) return;
            lock (subscriptions)
            {
                foreach (var pair in subscriptions)
                {
                    pair.Value.Resubscribe(pair.Key, server);
                }
            }
        }

        internal bool SubscriberConnected(RedisChannel channel = default(RedisChannel))
        {
            var server = GetSubscribedServer(channel);
            if (server != null) return server.IsConnected;

            server = SelectServer(-1, RedisCommand.SUBSCRIBE, CommandFlags.DemandMaster, default(RedisKey));
            return server != null && server.IsConnected;
        }



        internal long ValidateSubscriptions()
        {
            lock (subscriptions)
            {
                long count = 0;
                foreach (var pair in subscriptions)
                {
                    if (pair.Value.Validate(this, pair.Key)) count++;
                }
                return count;
            }
        }

        private sealed class Subscription
        {
            private Action<RedisChannel, RedisValue> handler;
            private ServerEndPoint owner;

            public Subscription(Action<RedisChannel, RedisValue> value)
            {
                handler = value;
            }
            public void Add(Action<RedisChannel, RedisValue> value)
            {
                handler += value;
            }
            public ICompletable ForInvoke(RedisChannel channel, RedisValue message)
            {
                var tmp = handler;
                return tmp == null ? null : new MessageCompletable(channel, message, tmp);
            }

            public bool Remove(Action<RedisChannel, RedisValue> value)
            {
                if (value == null)
                { // treat as blanket wipe
                    handler = null;
                    return true;
                }
                else
                {
                    return (handler -= value) == null;
                }
            }
            public Task SubscribeToServer(ConnectionMultiplexer multiplexer, RedisChannel channel, CommandFlags flags, object asyncState, bool internalCall)
            {
                var cmd = channel.IsPatternBased ? RedisCommand.PSUBSCRIBE : RedisCommand.SUBSCRIBE;
                var selected = multiplexer.SelectServer(-1, cmd, CommandFlags.DemandMaster, default(RedisKey));

                if (selected == null || Interlocked.CompareExchange(ref owner, selected, null) != null) return null;

                var msg = Message.Create(-1, flags, cmd, channel);

                return selected.QueueDirectAsync(msg, ResultProcessor.TrackSubscriptions, asyncState);
            }

            public Task UnsubscribeFromServer(RedisChannel channel, CommandFlags flags, object asyncState, bool internalCall)
            {
                var oldOwner = Interlocked.Exchange(ref owner, null);
                if (oldOwner == null) return null;

                var cmd = channel.IsPatternBased ? RedisCommand.PUNSUBSCRIBE : RedisCommand.UNSUBSCRIBE;
                var msg = Message.Create(-1, flags, cmd, channel);
                if (internalCall) msg.SetInternalCall();
                return oldOwner.QueueDirectAsync(msg, ResultProcessor.TrackSubscriptions, asyncState);
            }

            internal ServerEndPoint GetOwner()
            {
                return Interlocked.CompareExchange(ref owner, null, null);
            }
            internal void Resubscribe(RedisChannel channel, ServerEndPoint server)
            {
                if (server != null && Interlocked.CompareExchange(ref owner, server, server) == server)
                {
                    var cmd = channel.IsPatternBased ? RedisCommand.PSUBSCRIBE : RedisCommand.SUBSCRIBE;
                    var msg = Message.Create(-1, CommandFlags.FireAndForget, cmd, channel);
                    msg.SetInternalCall();
                    server.QueueDirectFireAndForget(msg, ResultProcessor.TrackSubscriptions);
                }
            }

            internal bool Validate(ConnectionMultiplexer multiplexer, RedisChannel channel)
            {
                bool changed = false;
                var oldOwner = Interlocked.CompareExchange(ref owner, null, null);
                if (oldOwner != null && !oldOwner.IsSelectable(RedisCommand.PSUBSCRIBE))
                {
                    if (UnsubscribeFromServer(channel, CommandFlags.FireAndForget, null, true) != null)
                    {
                        changed = true;
                    }
                    oldOwner = null;
                }
                if (oldOwner == null)
                {
                    if (SubscribeToServer(multiplexer, channel, CommandFlags.FireAndForget, null, true) != null)
                    {
                        changed = true;
                    }
                }
                return changed;
            }


        }
    }

    internal sealed class RedisSubscriber : RedisBase, ISubscriber
    {
        internal RedisSubscriber(ConnectionMultiplexer multiplexer, object asyncState) : base(multiplexer, asyncState)
        {
        }

        public EndPoint IdentifyEndpoint(RedisChannel channel, CommandFlags flags = CommandFlags.None)
        {
            var msg = Message.Create(-1, flags, RedisCommand.PUBSUB, RedisLiterals.NUMSUB, channel);
            msg.SetInternalCall();
            return ExecuteSync(msg, ResultProcessor.ConnectionIdentity);
        }

        public Task<EndPoint> IdentifyEndpointAsync(RedisChannel channel, CommandFlags flags = CommandFlags.None)
        {
            var msg = Message.Create(-1, flags, RedisCommand.PUBSUB, RedisLiterals.NUMSUB, channel);
            msg.SetInternalCall();
            return ExecuteAsync(msg, ResultProcessor.ConnectionIdentity);
        }

        public bool IsConnected(RedisChannel channel = default(RedisChannel))
        {
            return multiplexer.SubscriberConnected(channel);
        }

        public override TimeSpan Ping(CommandFlags flags = CommandFlags.None)
        {
            // can't use regular PING, but we can unsubscribe from something random that we weren't even subscribed to...
            RedisValue channel = Guid.NewGuid().ToByteArray();
            var msg = ResultProcessor.TimingProcessor.CreateMessage(-1, flags, RedisCommand.UNSUBSCRIBE, channel);
            return ExecuteSync(msg, ResultProcessor.ResponseTimer);
        }

        public override Task<TimeSpan> PingAsync(CommandFlags flags = CommandFlags.None)
        {
            // can't use regular PING, but we can unsubscribe from something random that we weren't even subscribed to...
            RedisValue channel = Guid.NewGuid().ToByteArray();
            var msg = ResultProcessor.TimingProcessor.CreateMessage(-1, flags, RedisCommand.UNSUBSCRIBE, channel);
            return ExecuteAsync(msg, ResultProcessor.ResponseTimer);
        }

        public long Publish(RedisChannel channel, RedisValue message, CommandFlags flags = CommandFlags.None)
        {
            if (channel.IsNullOrEmpty) throw new ArgumentNullException("channel");
            var msg = Message.Create(-1, flags, RedisCommand.PUBLISH, channel, message);
            return ExecuteSync(msg, ResultProcessor.Int64);
        }

        public Task<long> PublishAsync(RedisChannel channel, RedisValue message, CommandFlags flags = CommandFlags.None)
        {
            if (channel.IsNullOrEmpty) throw new ArgumentNullException("channel");
            var msg = Message.Create(-1, flags, RedisCommand.PUBLISH, channel, message);
            return ExecuteAsync(msg, ResultProcessor.Int64);
        }

        public void Subscribe(RedisChannel channel, Action<RedisChannel, RedisValue> handler, CommandFlags flags = CommandFlags.None)
        {
            var task = SubscribeAsync(channel, handler, flags);
            if ((flags & CommandFlags.FireAndForget) == 0) Wait(task);
        }

        public Task SubscribeAsync(RedisChannel channel, Action<RedisChannel, RedisValue> handler, CommandFlags flags = CommandFlags.None)
        {
            
            if (channel.IsNullOrEmpty) throw new ArgumentNullException("channel");
            return multiplexer.AddSubscription(channel, handler, flags, asyncState);
        }


        public EndPoint SubscribedEndpoint(RedisChannel channel)
        {
            var server = multiplexer.GetSubscribedServer(channel);
            return server == null ? null : server.EndPoint;
        }

        public void Unsubscribe(RedisChannel channel, Action<RedisChannel, RedisValue> handler = null, CommandFlags flags = CommandFlags.None)
        {
            var task = UnsubscribeAsync(channel, handler, flags);
            if ((flags & CommandFlags.FireAndForget) == 0) Wait(task);
        }

        public void UnsubscribeAll(CommandFlags flags = CommandFlags.None)
        {
            var task = UnsubscribeAllAsync(flags);
            if ((flags & CommandFlags.FireAndForget) == 0) Wait(task);
        }

        public Task UnsubscribeAllAsync(CommandFlags flags = CommandFlags.None)
        {
            return multiplexer.RemoveAllSubscriptions(flags, asyncState);
        }

        public Task UnsubscribeAsync(RedisChannel channel, Action<RedisChannel, RedisValue> handler = null, CommandFlags flags = CommandFlags.None)
        {
            if (channel.IsNullOrEmpty) throw new ArgumentNullException("channel");
            return multiplexer.RemoveSubscription(channel, handler, flags, asyncState);
        }
    }
}
