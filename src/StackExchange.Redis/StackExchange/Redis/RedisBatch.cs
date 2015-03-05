using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StackExchange.Redis
{
    class RedisBatch : RedisDatabase, IBatch
    {
        List<Message> pending;

        public RedisBatch(RedisDatabase wrapped, object asyncState) : base(wrapped.multiplexer, wrapped.Database, asyncState ?? wrapped.AsyncState)
        {
        }

        public void Execute()
        {
            var snapshot = pending;
            pending = null;
            if (snapshot == null || snapshot.Count == 0) return;

            // group into per-bridge chunks
            var byBridge = new Dictionary<PhysicalBridge, List<Message>>();

            // optimisation: assume most things are in a single bridge
            PhysicalBridge lastBridge = null;
            List<Message> lastList = null;
            foreach (var message in snapshot)
            {
                var server = multiplexer.SelectServer(message);
                if (server == null)
                {
                    FailNoServer(snapshot);
                    throw ExceptionFactory.NoConnectionAvailable(multiplexer.IncludeDetailInExceptions, message.Command, message, server);
                }
                var bridge = server.GetBridge(message.Command);
                if (bridge == null)
                {
                    FailNoServer(snapshot);
                    throw ExceptionFactory.NoConnectionAvailable(multiplexer.IncludeDetailInExceptions, message.Command, message, server);
                }

                // identity a list
                List<Message> list;
                if (bridge == lastBridge)
                {
                    list = lastList;
                }
                else if (!byBridge.TryGetValue(bridge, out list))
                {
                    list = new List<Message>();
                    byBridge.Add(bridge, list);
                }
                lastBridge = bridge;
                lastList = list;

                list.Add(message);
            }

            foreach (var pair in byBridge)
            {
                if (!pair.Key.TryEnqueue(pair.Value, pair.Key.ServerEndPoint.IsSlave))
                {
                    FailNoServer(pair.Value);
                }
            }
        }

        internal override Task<T> ExecuteAsync<T>(Message message, ResultProcessor<T> processor, ServerEndPoint server = null)
        {
            if (message == null) return CompletedTask<T>.Default(asyncState);
            multiplexer.CheckMessage(message);

            // prepare the inner command as a task
            Task<T> task;
            if (message.IsFireAndForget)
            {
                task = CompletedTask<T>.Default(null); // F+F explicitly does not get async-state
            }
            else
            {
                var tcs = TaskSource.CreateDenyExecSync<T>(asyncState);
                var source = ResultBox<T>.Get(tcs);
                message.SetSource(source, processor);
                task = tcs.Task;
            }

            // store it
            (pending ?? (pending = new List<Message>())).Add(message);
            return task;
        }

        internal override T ExecuteSync<T>(Message message, ResultProcessor<T> processor, ServerEndPoint server = null)
        {
            throw new NotSupportedException("ExecuteSync cannot be used inside a transaction");
        }
        private void FailNoServer(List<Message> messages)
        {
            if (messages == null) return;
            var completion = multiplexer.UnprocessableCompletionManager;
            foreach(var msg in messages)
            {
                msg.Fail(ConnectionFailureType.UnableToResolvePhysicalConnection, null);
                completion.CompleteSyncOrAsync(msg);

            }
        }
    }
}
