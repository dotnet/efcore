using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace StackExchange.Redis
{
    internal class RedisTransaction : RedisDatabase, ITransaction
    {
        private List<ConditionResult> conditions;

        private List<QueuedMessage> pending;

        public RedisTransaction(RedisDatabase wrapped, object asyncState) : base(wrapped.multiplexer, wrapped.Database, asyncState ?? wrapped.AsyncState)
        {
            // need to check we can reliably do this...
            var commandMap = multiplexer.CommandMap;
            commandMap.AssertAvailable(RedisCommand.MULTI);
            commandMap.AssertAvailable(RedisCommand.EXEC);
            commandMap.AssertAvailable(RedisCommand.DISCARD);
        }

        public ConditionResult AddCondition(Condition condition)
        {
            if (condition == null) throw new ArgumentNullException("condition");

            var commandMap = multiplexer.CommandMap;
            if (conditions == null)
            {
                // we don't demand these unless the user is requesting conditions, but we need both...
                commandMap.AssertAvailable(RedisCommand.WATCH);
                commandMap.AssertAvailable(RedisCommand.UNWATCH);
                conditions = new List<ConditionResult>();
            }
            condition.CheckCommands(commandMap);
            var result = new ConditionResult(condition);
            conditions.Add(result);
            return result;
        }

        public void Execute()
        {
            Execute(CommandFlags.FireAndForget);
        }

        public bool Execute(CommandFlags flags)
        {
            ResultProcessor<bool> proc;
            var msg = CreateMessage(flags, out proc);
            return base.ExecuteSync(msg, proc); // need base to avoid our local "not supported" override
        }

        public Task<bool> ExecuteAsync(CommandFlags flags)
        {
            ResultProcessor<bool> proc;
            var msg = CreateMessage(flags, out proc);
            return base.ExecuteAsync(msg, proc); // need base to avoid our local wrapping override
        }

        internal override Task<T> ExecuteAsync<T>(Message message, ResultProcessor<T> processor, ServerEndPoint server = null)
        {
            if (message == null) return CompletedTask<T>.Default(asyncState);
            multiplexer.CheckMessage(message);

            multiplexer.Trace("Wrapping " + message.Command, "Transaction");
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

            // prepare an outer-command that decorates that, but expects QUEUED
            var queued = new QueuedMessage(message);
            var wasQueued = ResultBox<bool>.Get(null);
            queued.SetSource(wasQueued, QueuedProcessor.Default);

            // store it, and return the task of the *outer* command
            // (there is no task for the inner command)
            (pending ?? (pending = new List<QueuedMessage>())).Add(queued);


            switch(message.Command)
            {
                case RedisCommand.EVAL:
                case RedisCommand.EVALSHA:
                    // people can do very naughty things in an EVAL
                    // including change the DB; change it back to what we
                    // think it should be!
                    var sel = PhysicalConnection.GetSelectDatabaseCommand(message.Db);
                    queued = new QueuedMessage(sel);
                    wasQueued = ResultBox<bool>.Get(null);
                    queued.SetSource(wasQueued, QueuedProcessor.Default);
                    pending.Add(queued);
                    break;
            }
            return task;
        }

        internal override T ExecuteSync<T>(Message message, ResultProcessor<T> processor, ServerEndPoint server = null)
        {
            throw new NotSupportedException("ExecuteSync cannot be used inside a transaction");
        }
        private Message CreateMessage(CommandFlags flags, out ResultProcessor<bool> processor)
        {
            var work = pending;
            pending = null; // any new operations go into a different queue
            var cond = conditions;
            conditions = null; // any new conditions go into a different queue

            if ((work == null || work.Count == 0) && (cond == null || cond.Count == 0))
            {
                if ((flags & CommandFlags.FireAndForget) != 0)
                {
                    processor = null;
                    return null; // they won't notice if we don't do anything...
                }                
                processor = ResultProcessor.DemandPONG;
                return Message.Create(-1, flags, RedisCommand.PING);
            }
            processor = TransactionProcessor.Default;
            return new TransactionMessage(Database, flags, cond, work);
        }
        class QueuedMessage : Message
        {
            private readonly Message wrapped;
            private volatile bool wasQueued;

            public QueuedMessage(Message message) : base(message.Db, message.Flags | CommandFlags.NoRedirect, message.Command)
            {
                message.SetNoRedirect();
                this.wrapped = message;
            }

            public bool WasQueued
            {
                get { return wasQueued; }
                set { wasQueued = value; }
            }

            public Message Wrapped { get { return wrapped; } }
            internal override void WriteImpl(PhysicalConnection physical)
            {
                wrapped.WriteImpl(physical);
            }
        }

        class QueuedProcessor : ResultProcessor<bool>
        {
            public static readonly ResultProcessor<bool> Default = new QueuedProcessor();
            static readonly byte[] QUEUED = Encoding.UTF8.GetBytes("QUEUED");
            protected override bool SetResultCore(PhysicalConnection connection, Message message, RawResult result)
            {
                if(result.Type == ResultType.SimpleString && result.IsEqual(QUEUED))
                {
                    var q = message as QueuedMessage;
                    if (q != null) q.WasQueued = true;
                    return true;
                }
                return false;
            }
        }
        class TransactionMessage : Message, IMultiMessage
        {

            static readonly ConditionResult[] NixConditions = new ConditionResult[0];

            static readonly QueuedMessage[] NixMessages = new QueuedMessage[0];

            static readonly Message SharedMulti = Message.Create(-1, CommandFlags.None, RedisCommand.MULTI);

            private ConditionResult[] conditions;

            private QueuedMessage[] operations;

            public TransactionMessage(int db, CommandFlags flags, List<ConditionResult> conditions, List<QueuedMessage> operations)
                : base(db, flags, RedisCommand.EXEC)
            {
                this.operations = (operations == null || operations.Count == 0) ? NixMessages : operations.ToArray();
                this.conditions = (conditions == null || conditions.Count == 0) ? NixConditions : conditions.ToArray();
            }

            public QueuedMessage[] InnerOperations { get { return operations; } }

            public bool IsAborted
            {
                get { return command != RedisCommand.EXEC; }
            }

            public override void AppendStormLog(StringBuilder sb)
            {
                base.AppendStormLog(sb);
                if (conditions.Length != 0) sb.Append(", ").Append(conditions.Length).Append(" conditions");
                sb.Append(", ").Append(operations.Length).Append(" operations");
            }
            public override int GetHashSlot(ServerSelectionStrategy serverSelectionStrategy)
            {
                int slot = ServerSelectionStrategy.NoSlot;
                for(int i = 0; i < conditions.Length;i++)
                {
                    int newSlot = conditions[i].Condition.GetHashSlot(serverSelectionStrategy);
                    slot = serverSelectionStrategy.CombineSlot(slot, newSlot);
                    if (slot == ServerSelectionStrategy.MultipleSlots) return slot;
                }
                for(int i = 0; i < operations.Length;i++)
                {
                    int newSlot = operations[i].Wrapped.GetHashSlot(serverSelectionStrategy);
                    slot = serverSelectionStrategy.CombineSlot(slot, newSlot);
                    if (slot == ServerSelectionStrategy.MultipleSlots) return slot;
                }
                return slot;
            }

            public IEnumerable<Message> GetMessages(PhysicalConnection connection)
            {
                ResultBox lastBox = null;
                try
                {
                    // Important: if the server supports EXECABORT, then we can check the pre-conditions (pause there),
                    // which will usually be pretty small and cheap to do - if that passes, we can just isue all the commands
                    // and rely on EXECABORT to kick us if we are being idiotic inside the MULTI. However, if the server does
                    // *not* support EXECABORT, then we need to explicitly check for QUEUED anyway; we might as well defer
                    // checking the preconditions to the same time to avoid having to pause twice. This will mean that on
                    // up-version servers, pre-condition failures exit with UNWATCH; and on down-version servers pre-condition
                    // failures exit with DISCARD - but that's ok : both work fine

                    bool explicitCheckForQueued = !connection.Bridge.ServerEndPoint.GetFeatures().ExecAbort;
                    var multiplexer = connection.Multiplexer;

                    // PART 1: issue the pre-conditions
                    if (!IsAborted && conditions.Length != 0)
                    {
                        for (int i = 0; i < conditions.Length; i++)
                        {
                            // need to have locked them before sending them
                            // to guarantee that we see the pulse
                            ResultBox latestBox = conditions[i].GetBox();
                            Monitor.Enter(latestBox);
                            if (lastBox != null) Monitor.Exit(lastBox);
                            lastBox = latestBox;
                            foreach (var msg in conditions[i].CreateMessages(Db))
                            {
                                msg.SetNoRedirect(); // need to keep them in the current context only
                                yield return msg;
                            }
                        }

                        if (!explicitCheckForQueued && lastBox != null)
                        {
                            // need to get those sent ASAP; if they are stuck in the buffers, we die
                            multiplexer.Trace("Flushing and waiting for precondition responses");
                            connection.Flush();
                            if (Monitor.Wait(lastBox, multiplexer.TimeoutMilliseconds))
                            {
                                if (!AreAllConditionsSatisfied(multiplexer))
                                    command = RedisCommand.UNWATCH; // somebody isn't happy
                            }
                            else
                            { // timeout running pre-conditions
                                multiplexer.Trace("Timeout checking preconditions");
                                command = RedisCommand.UNWATCH;
                            }
                            Monitor.Exit(lastBox);
                            lastBox = null;
                        }
                    }

                    // PART 2: begin the transaction
                    if (!IsAborted)
                    {
                        multiplexer.Trace("Begining transaction");
                        yield return SharedMulti;
                    }

                    // PART 3: issue the commands
                    if (!IsAborted && operations.Length != 0)
                    {
                        multiplexer.Trace("Issuing transaction operations");

                        foreach (var op in operations)
                        {
                            if (explicitCheckForQueued)
                            {   // need to have locked them before sending them
                                // to guarantee that we see the pulse
                                ResultBox thisBox = op.ResultBox;
                                if (thisBox != null)
                                {
                                    Monitor.Enter(thisBox);
                                    if (lastBox != null) Monitor.Exit(lastBox);
                                    lastBox = thisBox;
                                }
                            }
                            yield return op;
                        }

                        if (explicitCheckForQueued && lastBox != null)
                        {
                            multiplexer.Trace("Flushing and waiting for precondition+queued responses");
                            connection.Flush(); // make sure they get sent, so we can check for QUEUED (and the pre-conditions if necessary)
                            if (Monitor.Wait(lastBox, multiplexer.TimeoutMilliseconds))
                            {
                                if (!AreAllConditionsSatisfied(multiplexer))
                                {
                                    command = RedisCommand.DISCARD;
                                }
                                else
                                {
                                    foreach (var op in operations)
                                    {
                                        if (!op.WasQueued)
                                        {
                                            multiplexer.Trace("Aborting: operation was not queued: " + op.Command);
                                            command = RedisCommand.DISCARD;
                                            break;
                                        }
                                    }
                                }
                                multiplexer.Trace("Confirmed: QUEUED x " + operations.Length);
                            }
                            else
                            {
                                multiplexer.Trace("Aborting: timeout checking queued messages");
                                command = RedisCommand.DISCARD;
                            }
                            Monitor.Exit(lastBox);
                            lastBox = null;
                        }
                    }
                }
                finally
                {
                    if (lastBox != null) Monitor.Exit(lastBox);
                }
                if (IsAborted)
                {
                    connection.Multiplexer.Trace("Aborting: canceling wrapped messages");
                    var bridge = connection.Bridge;
                    foreach (var op in operations)
                    {
                        op.Wrapped.Cancel();
                        bridge.CompleteSyncOrAsync(op.Wrapped);
                    }
                }
                connection.Multiplexer.Trace("End ot transaction: " + Command);
                yield return this; // acts as either an EXEC or an UNWATCH, depending on "aborted"
            }

            internal override void WriteImpl(PhysicalConnection physical)
            {
                physical.WriteHeader(Command, 0);
            }

            private bool AreAllConditionsSatisfied(ConnectionMultiplexer multiplexer)
            {
                bool result = true;
                for (int i = 0; i < conditions.Length; i++)
                {
                    var condition = conditions[i];
                    if (condition.UnwrapBox())
                    {
                        multiplexer.Trace("Precondition passed: " + condition.Condition);
                    }
                    else
                    {
                        multiplexer.Trace("Precondition failed: " + condition.Condition);
                        result = false;
                    }
                }
                return result;
            }
        }

        class TransactionProcessor : ResultProcessor<bool>
        {
            public static readonly TransactionProcessor Default = new TransactionProcessor();
            
            public override bool SetResult(PhysicalConnection connection, Message message, RawResult result)
            {
                if (result.IsError)
                {
                    var tran = message as TransactionMessage;
                    if (tran != null)
                    {
                        string error = result.GetString();
                        var bridge = connection.Bridge;
                        foreach(var op in tran.InnerOperations)
                        {
                            ServerFail(op.Wrapped, error);
                            bridge.CompleteSyncOrAsync(op.Wrapped);
                        }
                    }
                }
                return base.SetResult(connection, message, result);
            }
            protected override bool SetResultCore(PhysicalConnection connection, Message message, RawResult result)
            {
                var tran = message as TransactionMessage;
                if (tran != null)
                {
                    var bridge = connection.Bridge;
                    var wrapped = tran.InnerOperations;
                    switch (result.Type)
                    {
                        case ResultType.SimpleString:
                            if (tran.IsAborted && result.IsEqual(RedisLiterals.BytesOK))
                            {
                                connection.Multiplexer.Trace("Acknowledging UNWATCH (aborted electively)");
                                SetResult(message, false);
                                return true;
                            }
                            break;
                        case ResultType.MultiBulk:
                            if (!tran.IsAborted)
                            {
                                var arr = result.GetItems();
                                if (arr == null)
                                {
                                    connection.Multiplexer.Trace("Server aborted due to failed WATCH");
                                    foreach (var op in wrapped)
                                    {
                                        op.Wrapped.Cancel();
                                        bridge.CompleteSyncOrAsync(op.Wrapped);
                                    }
                                    SetResult(message, false);
                                    return true;
                                }
                                else if (wrapped.Length == arr.Length)
                                {
                                    connection.Multiplexer.Trace("Server committed; processing nested replies");
                                    for (int i = 0; i < arr.Length; i++)
                                    {
                                        if (wrapped[i].Wrapped.ComputeResult(connection, arr[i]))
                                        {
                                            bridge.CompleteSyncOrAsync(wrapped[i].Wrapped);
                                        }
                                    }
                                    SetResult(message, true);
                                    return true;
                                }
                            }
                            break;
                    }
                    // even if we didn't fully understand the result, we still need to do something with
                    // the pending tasks
                    foreach (var op in wrapped)
                    {
                        op.Wrapped.Fail(ConnectionFailureType.ProtocolFailure, null);
                        bridge.CompleteSyncOrAsync(op.Wrapped);
                    }
                }
                return false;
            }
        }
    }
    //internal class RedisDatabaseTransaction : RedisCoreTransaction, ITransaction<IRedisDatabaseAsync>
    //{
    //    public IRedisDatabaseAsync Pending { get { return this; } }

    //    bool ITransaction<IRedisDatabaseAsync>.Execute(CommandFlags flags)
    //    {
    //        return ExecuteTransaction(flags);
    //    }
    //    Task<bool> ITransaction<IRedisDatabaseAsync>.ExecuteAsync(CommandFlags flags)
    //    {
    //        return ExecuteTransactionAsync(flags);
    //    }
    //}
}