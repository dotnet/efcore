using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;

namespace StackExchange.Redis
{
    enum WriteResult
    {
        QueueEmptyAfterWrite,
        NothingToDo,
        MoreWork,
        CompetingWriter,
        NoConnection,
    }

    sealed partial class PhysicalBridge : IDisposable
    {
        internal readonly string Name;

        internal int inWriteQueue = 0;

        const int ProfileLogSamples = 10;

        const double ProfileLogSeconds = (ConnectionMultiplexer.MillisecondsPerHeartbeat * ProfileLogSamples) / 1000.0;

        private static readonly Message
                                           ReusableAskingCommand = Message.Create(-1, CommandFlags.FireAndForget, RedisCommand.ASKING);

        private readonly CompletionManager completionManager;
        private readonly ConnectionType connectionType;
        private readonly ConnectionMultiplexer multiplexer;
        readonly long[] profileLog = new long[ProfileLogSamples];
        private readonly MessageQueue queue = new MessageQueue();
        private readonly ServerEndPoint serverEndPoint;
        int activeWriters = 0;
        private int beating;
        int failConnectCount = 0;
        volatile bool isDisposed;
        long nonPreferredEndpointCount;

        //private volatile int missedHeartbeats;
        private long operationCount, socketCount;
        private volatile PhysicalConnection physical;


        long profileLastLog;
        int profileLogIndex;
        volatile bool reportNextFailure = true, reconfigureNextFailure = false;
        private volatile int state = (int)State.Disconnected;
        public PhysicalBridge(ServerEndPoint serverEndPoint, ConnectionType type)
        {
            this.serverEndPoint = serverEndPoint;
            this.connectionType = type;
            this.multiplexer = serverEndPoint.Multiplexer;
            this.Name = Format.ToString(serverEndPoint.EndPoint) + "/" + connectionType.ToString();
            this.completionManager = new CompletionManager(multiplexer, Name);
        }

        public enum State : byte
        {
            Connecting,
            ConnectedEstablishing,
            ConnectedEstablished,
            Disconnected
        }

        public ConnectionType ConnectionType { get { return connectionType; } }

        public bool IsConnected
        {
            get
            {
                return state == (int)State.ConnectedEstablished;
            }
        }

        public ConnectionMultiplexer Multiplexer { get { return multiplexer; } }

        public ServerEndPoint ServerEndPoint { get { return serverEndPoint; } }

        public long SubscriptionCount
        {
            get
            {
                var tmp = physical;
                return tmp == null ? 0 : physical.SubscriptionCount;
            }
        }

        internal State ConnectionState { get { return (State)state; } }
        internal bool IsBeating { get { return Interlocked.CompareExchange(ref beating, 0, 0) == 1; } }

        internal long OperationCount
        {
            get { return Interlocked.Read(ref operationCount); }
        }
        public void CompleteSyncOrAsync(ICompletable operation)
        {
            completionManager.CompleteSyncOrAsync(operation);
        }

        public void Dispose()
        {
            isDisposed = true;
            using (var tmp = physical)
            {
                physical = null;
            }
        }

        public void ReportNextFailure()
        {
            reportNextFailure = true;
        }

        public override string ToString()
        {
            return connectionType + "/" + Format.ToString(serverEndPoint.EndPoint);
        }

        public void TryConnect()
        {
            GetConnection();
        }

        public bool TryEnqueue(Message message, bool isSlave)
        {
            if (isDisposed) throw new ObjectDisposedException(Name);
            if (!IsConnected)
            {
                if (message.IsInternalCall)
                {
                    // you can go in the queue, but we won't be starting
                    // a worker, because the handshake has not completed
                    queue.Push(message);
                    return true;
                }
                else
                {
                    // sorry, we're just not ready for you yet;
                    return false;
                }
            }

            bool reqWrite = queue.Push(message);
            LogNonPreferred(message.Flags, isSlave);
            Trace("Now pending: " + GetPendingCount());

            if (reqWrite)
            {
                multiplexer.RequestWrite(this, false);
            }
            return true;
        }
        internal void AppendProfile(StringBuilder sb)
        {
            long[] clone = new long[ProfileLogSamples + 1];
            for (int i = 0; i < ProfileLogSamples; i++)
            {
                clone[i] = Interlocked.Read(ref profileLog[i]);
            }
            clone[ProfileLogSamples] = Interlocked.Read(ref operationCount);
            Array.Sort(clone);
            sb.Append(" ").Append(clone[0]);
            for (int i = 1; i < clone.Length; i++)
            {
                if (clone[i] != clone[i - 1])
                {
                    sb.Append("+").Append(clone[i] - clone[i - 1]);
                }
            }
            if (clone[0] != clone[ProfileLogSamples])
            {
                sb.Append("=").Append(clone[ProfileLogSamples]);
            }
            double rate = (clone[ProfileLogSamples] - clone[0]) / ProfileLogSeconds;
            sb.Append(" (").Append(rate.ToString("N2")).Append(" ops/s; spans ").Append(ProfileLogSeconds).Append("s)");
        }

        internal bool ConfirmRemoveFromWriteQueue()
        {
            lock (queue.SyncLock)
            {
                if (queue.Count() == 0)
                {
                    Interlocked.Exchange(ref inWriteQueue, 0);
                    return true;
                }
            }
            return false;
        }

        internal void GetCounters(ConnectionCounters counters)
        {
            counters.PendingUnsentItems = queue.Count();
            counters.OperationCount = OperationCount;
            counters.SocketCount = Interlocked.Read(ref socketCount);
            counters.WriterCount = Interlocked.CompareExchange(ref activeWriters, 0, 0);
            counters.NonPreferredEndpointCount = Interlocked.Read(ref nonPreferredEndpointCount);
            completionManager.GetCounters(counters);
            var tmp = physical;
            if (tmp != null)
            {
                tmp.GetCounters(counters);
            }
        }

        internal int GetOutstandingCount(out int inst, out int qu, out int qs, out int qc, out int wr, out int wq, out int @in, out int ar)
        {// defined as: PendingUnsentItems + SentItemsAwaitingResponse + ResponsesAwaitingAsyncCompletion
            inst = (int)(Interlocked.Read(ref operationCount) - Interlocked.Read(ref profileLastLog));
            qu = queue.Count();
            var tmp = physical;
            if(tmp == null)
            {
                qs = @in = ar = 0;
            } else
            {
                qs = tmp.GetSentAwaitingResponseCount();
                @in = tmp.GetAvailableInboundBytes(out ar);
            }
            qc = completionManager.GetOutstandingCount();
            wr = Interlocked.CompareExchange(ref activeWriters, 0, 0);
            wq = Interlocked.CompareExchange(ref inWriteQueue, 0, 0);
            return qu + qs + qc;
        }

        internal int GetPendingCount()
        {
            return queue.Count();
        }

        internal string GetStormLog()
        {
            var sb = new StringBuilder("Storm log for ").Append(Format.ToString(serverEndPoint.EndPoint)).Append(" / ").Append(connectionType)
                .Append(" at ").Append(DateTime.UtcNow)
                .AppendLine().AppendLine();
            queue.GetStormLog(sb);
            var tmp = physical;
            if (tmp != null) tmp.GetStormLog(sb);
            completionManager.GetStormLog(sb);
            sb.Append("Circular op-count snapshot:");
            AppendProfile(sb);
            sb.AppendLine();
            return sb.ToString();
        }

        internal void IncrementOpCount()
        {
            Interlocked.Increment(ref operationCount);
        }

        internal void KeepAlive()
        {
            var commandMap = multiplexer.CommandMap;
            Message msg = null;
            switch (connectionType)
            {
                case ConnectionType.Interactive:
                    msg = serverEndPoint.GetTracerMessage(false);
                    msg.SetSource(ResultProcessor.Tracer, null);
                    break;
                case ConnectionType.Subscription:
                    if (commandMap.IsAvailable(RedisCommand.UNSUBSCRIBE))
                    {
                        msg = Message.Create(-1, CommandFlags.FireAndForget, RedisCommand.UNSUBSCRIBE,
                            (RedisChannel)Guid.NewGuid().ToByteArray());
                        msg.SetSource(ResultProcessor.TrackSubscriptions, null);
                    }
                    break;
            }
            if (msg != null)
            {
                msg.SetInternalCall();
                multiplexer.Trace("Enqueue: " + msg);
                if (!TryEnqueue(msg, serverEndPoint.IsSlave))
                {
                    OnInternalError(ExceptionFactory.NoConnectionAvailable(multiplexer.IncludeDetailInExceptions, msg.Command, msg, serverEndPoint));
                }
            }
        }

        internal void OnConnected(PhysicalConnection connection)
        {
            Trace("OnConnected");
            if (physical == connection && !isDisposed && ChangeState(State.Connecting, State.ConnectedEstablishing))
            {
                serverEndPoint.OnEstablishing(connection);
            }
            else
            {
                try
                {
                    connection.Dispose();
                }
                catch
                { }
            }
        }


        internal void ResetNonConnected()
        {
            var tmp = physical;
            if (tmp != null && state != (int)State.ConnectedEstablished)
            {
                tmp.RecordConnectionFailed(ConnectionFailureType.UnableToConnect);
            }
            GetConnection();
        }

        internal void OnConnectionFailed(PhysicalConnection connection, ConnectionFailureType failureType, Exception innerException)
        {
            if (reportNextFailure)
            {
                reportNextFailure = false; // until it is restored
                var endpoint = serverEndPoint.EndPoint;
                multiplexer.OnConnectionFailed(endpoint, connectionType, failureType, innerException, reconfigureNextFailure);
            }
        }

        internal void OnDisconnected(ConnectionFailureType failureType, PhysicalConnection connection, out bool isCurrent, out State oldState)
        {
            Trace("OnDisconnected");

            // if the next thing in the pipe is a PING, we can tell it that we failed (this really helps spot doomed connects)
            // note that for simplicity we haven't removed it from the queue; that's OK
            int count;
            var ping = queue.PeekPing(out count);
            if (ping != null)
            {
                Trace("Marking PING as failed (queue length: " + count + ")");
                ping.Fail(failureType, null);
                CompleteSyncOrAsync(ping);
            }
            oldState = default(State); // only defined when isCurrent = true
            if (isCurrent = (physical == connection))
            {
                Trace("Bridge noting disconnect from active connection" + (isDisposed ? " (disposed)" : ""));
                oldState = ChangeState(State.Disconnected);
                physical = null;

                if (!isDisposed && Interlocked.Increment(ref failConnectCount) == 1)
                {
                    GetConnection(); // try to connect immediately
                }
            }
            else if (physical == null)
            {
                Trace("Bridge noting disconnect (already terminated)");
            }
            else
            {
                Trace("Bridge noting disconnect, but from different connection");
            }
        }

        internal void OnFullyEstablished(PhysicalConnection connection)
        {
            Trace("OnFullyEstablished");
            if (physical == connection && !isDisposed && ChangeState(State.ConnectedEstablishing, State.ConnectedEstablished))
            {
                reportNextFailure = reconfigureNextFailure = true;
                Interlocked.Exchange(ref failConnectCount, 0);
                serverEndPoint.OnFullyEstablished(connection);
                multiplexer.RequestWrite(this, true);
                if(connectionType == ConnectionType.Interactive) serverEndPoint.CheckInfoReplication();
            }
            else
            {
                try { connection.Dispose(); } catch { }
            }
        }

        private int connectStartTicks;
        internal void OnHeartbeat(bool ifConnectedOnly)
        {
            bool runThisTime = false;
            try
            {
                runThisTime = !isDisposed && Interlocked.CompareExchange(ref beating, 1, 0) == 0;
                if (!runThisTime) return;

                uint index = (uint)Interlocked.Increment(ref profileLogIndex);
                long newSampleCount = Interlocked.Read(ref operationCount);
                Interlocked.Exchange(ref profileLog[index % ProfileLogSamples], newSampleCount);
                Interlocked.Exchange(ref profileLastLog, newSampleCount);
                Trace("OnHeartbeat: " + (State)state);
                switch (state)
                {
                    case (int)State.Connecting:
                        int connectTimeMilliseconds = unchecked(Environment.TickCount - Thread.VolatileRead(ref connectStartTicks));
                        if (connectTimeMilliseconds >= multiplexer.RawConfig.ConnectTimeout)
                        {
                            Trace("Aborting connect");
                            // abort and reconnect
                            var snapshot = physical;
                            bool isCurrent;
                            State oldState;
                            OnDisconnected(ConnectionFailureType.UnableToConnect, snapshot, out isCurrent, out oldState);
                            using (snapshot) { } // dispose etc
                            TryConnect();
                        }
                        if (!ifConnectedOnly)
                        {
                            AbortUnsent();
                        }
                        break;
                    case (int)State.ConnectedEstablishing:
                    case (int)State.ConnectedEstablished:
                        var tmp = physical;
                        if (tmp != null)
                        {
                            if(state == (int)State.ConnectedEstablished)
                            {
                                tmp.Bridge.ServerEndPoint.ClearUnselectable(UnselectableFlags.DidNotRespond);
                            }
                            tmp.OnHeartbeat();
                            int writeEverySeconds = serverEndPoint.WriteEverySeconds,
                                checkConfigSeconds = multiplexer.RawConfig.ConfigCheckSeconds;

                            if(state == (int)State.ConnectedEstablished && connectionType == ConnectionType.Interactive
                                && checkConfigSeconds > 0 && serverEndPoint.LastInfoReplicationCheckSecondsAgo >= checkConfigSeconds
                                && serverEndPoint.CheckInfoReplication())
                            {
                                // that serves as a keep-alive, if it is accepted
                            }
                            else if (writeEverySeconds > 0 && tmp.LastWriteSecondsAgo >= writeEverySeconds)
                            {
                                Trace("OnHeartbeat - overdue");
                                if (state == (int)State.ConnectedEstablished)
                                {
                                    KeepAlive();
                                }
                                else
                                {
                                    bool ignore;
                                    State oldState;
                                    OnDisconnected(ConnectionFailureType.SocketFailure, tmp, out ignore, out oldState);
                                }
                            }
                            else if (!queue.Any() && tmp.GetSentAwaitingResponseCount() != 0)
                            {
                                // there's a chance this is a dead socket; sending data will shake that
                                // up a bit, so if we have an empty unsent queue and a non-empty sent
                                // queue, test the socket
                                KeepAlive();
                            }
                        }
                        break;
                    case (int)State.Disconnected:
                        if (!ifConnectedOnly)
                        {
                            AbortUnsent();
                            multiplexer.Trace("Resurrecting " + this.ToString());
                            GetConnection();
                        }
                        break;
                    default:
                        if (!ifConnectedOnly)
                        {
                            AbortUnsent();
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                OnInternalError(ex);
                Trace("OnHeartbeat error: " + ex.Message);
            }
            finally
            {
                if (runThisTime) Interlocked.Exchange(ref beating, 0);
            }
        }

        internal void RemovePhysical(PhysicalConnection connection)
        {
#pragma warning disable 0420
            Interlocked.CompareExchange(ref physical, null, connection);
#pragma warning restore 0420
        }

        [Conditional("VERBOSE")]
        internal void Trace(string message)
        {
            multiplexer.Trace(message, ToString());
        }

        [Conditional("VERBOSE")]
        internal void Trace(bool condition, string message)
        {
            if (condition) multiplexer.Trace(message, ToString());
        }

        internal bool TryEnqueue(List<Message> messages, bool isSlave)
        {
            if (messages == null || messages.Count == 0) return true;

            if (isDisposed) throw new ObjectDisposedException(Name);

            if (!IsConnected)
            {
                return false;
            }
            bool reqWrite = false;
            foreach (var message in messages)
            {   // deliberately not taking a single lock here; we don't care if
                // other threads manage to interleave - in fact, it would be desirable
                // (to avoid a batch monopolising the connection)
                if (queue.Push(message)) reqWrite = true;
                LogNonPreferred(message.Flags, isSlave);
            }
            Trace("Now pending: " + GetPendingCount());
            if (reqWrite) // was empty before
            {
                multiplexer.RequestWrite(this, false);
            }
            return true;
        }

        /// <summary>
        /// This writes a message **directly** to the output stream; note
        /// that this ignores the queue, so should only be used *either*
        /// from the regular dequeue loop, *or* from the "I've just
        /// connected" handshake (when there is no dequeue loop) - otherwise,
        /// you can pretty much assume you're going to destroy the stream
        /// </summary>
        internal bool WriteMessageDirect(PhysicalConnection tmp, Message next)
        {
            Trace("Writing: " + next);
            if (next is IMultiMessage)
            {
                SelectDatabase(tmp, next); // need to switch database *before* the transaction
                foreach (var subCommand in ((IMultiMessage)next).GetMessages(tmp))
                {
                    if (!WriteMessageToServer(tmp, subCommand))
                    {
                        // we screwed up; abort; note that WriteMessageToServer already
                        // killed the underlying connection
                        Trace("Unable to write to server");
                        next.Fail(ConnectionFailureType.ProtocolFailure, null);
                        CompleteSyncOrAsync(next);
                        return false;
                    }
                }
                return true;
            }
            else
            {
                return WriteMessageToServer(tmp, next);
            }
        }

        internal WriteResult WriteQueue(int maxWork)
        {
            bool weAreWriter = false;
            PhysicalConnection conn = null;
            try
            {
                Trace("Writing queue from bridge");

                weAreWriter = Interlocked.CompareExchange(ref activeWriters, 1, 0) == 0;
                if (!weAreWriter)
                {
                    Trace("(aborting: existing writer)");
                    return WriteResult.CompetingWriter;
                }

                conn = GetConnection();
                if (conn == null)
                {
                    AbortUnsent();
                    Trace("Connection not available; exiting");
                    return WriteResult.NoConnection;
                }

                Message last = null;
                int count = 0;
                while (true)
                {
                    var next = queue.Dequeue();
                    if (next == null)
                    {
                        Trace("Nothing to write; exiting");
                        if(count == 0)
                        {
                            conn.Flush(); // only flush on an empty run
                            return WriteResult.NothingToDo;
                        }
                        return WriteResult.QueueEmptyAfterWrite;
                    }
                    last = next;

                    Trace("Now pending: " + GetPendingCount());
                    if (!WriteMessageDirect(conn, next))
                    {
                        AbortUnsent();
                        Trace("write failed; connection is toast; exiting");
                        return WriteResult.NoConnection;
                    }
                    count++;
                    if (maxWork > 0 && count >= maxWork)
                    {
                        Trace("Work limit; exiting");
                        Trace(last != null, "Flushed up to: " + last);
                        conn.Flush();
                        break;
                    }
                }
            }
            catch (IOException ex)
            {
                if (conn != null)
                {
                    conn.RecordConnectionFailed(ConnectionFailureType.SocketFailure, ex);
                    conn = null;
                }
                AbortUnsent();
            }
            catch (Exception ex)
            {
                AbortUnsent();
                OnInternalError(ex);
            }
            finally
            {
                if (weAreWriter)
                {
                    Interlocked.Exchange(ref activeWriters, 0);
                    Trace("Exiting writer");
                }
            }
            return queue.Any() ? WriteResult.MoreWork : WriteResult.QueueEmptyAfterWrite;
        }

        private void AbortUnsent()
        {
            var dead = queue.DequeueAll();
            Trace(dead.Length != 0, "Aborting " + dead.Length + " messages");
            for (int i = 0; i < dead.Length; i++)
            {
                var msg = dead[i];
                msg.Fail(ConnectionFailureType.UnableToResolvePhysicalConnection, null);
                CompleteSyncOrAsync(msg);
            }
        }

        private State ChangeState(State newState)
        {
#pragma warning disable 0420
            var oldState = (State)Interlocked.Exchange(ref state, (int)newState);
#pragma warning restore 0420
            if (oldState != newState)
            {
                multiplexer.Trace(connectionType + " state changed from " + oldState + " to " + newState);

                if (newState == State.Disconnected)
                {
                    AbortUnsent();
                }
            }
            return oldState;
        }

        private bool ChangeState(State oldState, State newState)
        {
#pragma warning disable 0420
            bool result = Interlocked.CompareExchange(ref state, (int)newState, (int)oldState) == (int)oldState;
#pragma warning restore 0420
            if (result)
            {
                multiplexer.Trace(connectionType + " state changed from " + oldState + " to " + newState);
            }
            return result;
        }

        private void Flush()
        {
            var tmp = physical;
            if (tmp != null)
            {
                try
                {
                    Trace(connectionType + " flushed");
                    tmp.Flush();
                }
                catch (Exception ex)
                {
                    OnInternalError(ex);
                }
            }
        }

        private PhysicalConnection GetConnection()
        {
            if (state == (int)State.Disconnected)
            {
                try
                {
                    if (!multiplexer.IsDisposed)
                    {
                        Multiplexer.Trace("Connecting...", Name);
                        if (ChangeState(State.Disconnected, State.Connecting))
                        {
                            Interlocked.Increment(ref socketCount);
                            Interlocked.Exchange(ref connectStartTicks, Environment.TickCount);
                            // separate creation and connection for case when connection completes synchronously
                            // in that case PhysicalConnection will call back to PhysicalBridge, and most of  PhysicalBridge methods assumes that physical is not null;
                            physical = new PhysicalConnection(this);
                            physical.BeginConnect();
                        }
                    }
                    return null;
                }
                catch (Exception ex)
                {
                    Multiplexer.Trace("Connect failed: " + ex.Message, Name);
                    ChangeState(State.Disconnected);
                    OnInternalError(ex);
                    throw;
                }
            }
            return physical;
        }

        private void LogNonPreferred(CommandFlags flags, bool isSlave)
        {
            if ((flags & Message.InternalCallFlag) == 0) // don't log internal-call
            {
                if (isSlave)
                {
                    if (Message.GetMasterSlaveFlags(flags) == CommandFlags.PreferMaster)
                        Interlocked.Increment(ref nonPreferredEndpointCount);
                }
                else
                {
                    if (Message.GetMasterSlaveFlags(flags) == CommandFlags.PreferSlave)
                        Interlocked.Increment(ref nonPreferredEndpointCount);
                }
            }
        }
        private void OnInternalError(Exception exception, [CallerMemberName] string origin = null)
        {
            multiplexer.OnInternalError(exception, serverEndPoint.EndPoint, connectionType, origin);
        }
        private void SelectDatabase(PhysicalConnection connection, Message message)
        {
            int db = message.Db;
            if (db >= 0)
            {
                var sel = connection.GetSelectDatabaseCommand(db, message);
                if (sel != null)
                {
                    connection.Enqueue(sel);
                    sel.WriteImpl(connection);
                    IncrementOpCount();
                }
            }
        }
        private bool WriteMessageToServer(PhysicalConnection connection, Message message)
        {
            if (message == null) return true;

            try
            {
                var cmd = message.Command;
                bool isMasterOnly = message.IsMasterOnly();
                if (isMasterOnly && serverEndPoint.IsSlave)
                {
                    throw ExceptionFactory.MasterOnly(multiplexer.IncludeDetailInExceptions, message.Command, message, ServerEndPoint);
                }

                SelectDatabase(connection, message);

                if (!connection.TransactionActive)
                {
                    var readmode = connection.GetReadModeCommand(isMasterOnly);
                    if (readmode != null)
                    {
                        connection.Enqueue(readmode);
                        readmode.WriteTo(connection);
                        IncrementOpCount();
                    }

                    if (message.IsAsking)
                    {
                        var asking = ReusableAskingCommand;
                        connection.Enqueue(asking);
                        asking.WriteImpl(connection);
                        IncrementOpCount();
                    }
                }
                switch (cmd)
                {
                    case RedisCommand.WATCH:
                    case RedisCommand.MULTI:
                        connection.TransactionActive = true;
                        break;
                    case RedisCommand.UNWATCH:
                    case RedisCommand.EXEC:
                    case RedisCommand.DISCARD:
                        connection.TransactionActive = false;
                        break;
                }

                connection.Enqueue(message);
                message.WriteImpl(connection);
                IncrementOpCount();

                // some commands smash our ability to trust the database; some commands
                // demand an immediate flush
                switch (cmd)
                {
                    case RedisCommand.EVAL:
                    case RedisCommand.EVALSHA:
                        if(!serverEndPoint.GetFeatures().ScriptingDatabaseSafe)
                        {
                            connection.SetUnknownDatabase();
                        }
                        break;
                    case RedisCommand.DISCARD:
                    case RedisCommand.EXEC:
                        connection.SetUnknownDatabase();
                        break;
                }
                return true;
            }
            catch (RedisCommandException ex)
            {
                Trace("Write failed: " + ex.Message);
                message.Fail(ConnectionFailureType.ProtocolFailure, ex);
                CompleteSyncOrAsync(message);
                // this failed without actually writing; we're OK with that... unless there's a transaction

                if (connection != null && connection.TransactionActive)
                {
                    // we left it in a broken state; need to kill the connection
                    connection.RecordConnectionFailed(ConnectionFailureType.ProtocolFailure, ex);
                    return false;
                }
                return true;
            }
            catch (Exception ex)
            {
                Trace("Write failed: " + ex.Message);
                message.Fail(ConnectionFailureType.InternalFailure, ex);
                CompleteSyncOrAsync(message);

                // we're not sure *what* happened here; probably an IOException; kill the connection
                if(connection != null) connection.RecordConnectionFailed(ConnectionFailureType.InternalFailure, ex);
                return false;
            }
        }
    }
}
