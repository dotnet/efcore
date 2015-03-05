using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace StackExchange.Redis
{
    [Flags]
    internal enum UnselectableFlags
    {
        None = 0,
        RedundantMaster = 1,
        DidNotRespond = 2,
        ServerType = 4

    }

    internal sealed partial class ServerEndPoint : IDisposable
    {
        internal volatile ServerEndPoint Master;
        internal volatile ServerEndPoint[] Slaves = NoSlaves;
        private static readonly Regex nameSanitizer = new Regex("[^!-~]", RegexOptions.Compiled);
        private static readonly ServerEndPoint[] NoSlaves = new ServerEndPoint[0];
        private readonly EndPoint endpoint;


        private readonly Hashtable knownScripts = new Hashtable(StringComparer.Ordinal);
        private readonly ConnectionMultiplexer multiplexer;

        private int databases, writeEverySeconds;

        private PhysicalBridge interactive, subscription;

        bool isDisposed;

        ServerType serverType;

        private bool slaveReadOnly, isSlave;

        private volatile UnselectableFlags unselectableReasons;

        private Version version;


        internal void ResetNonConnected()
        {
            var tmp = interactive;
            if (tmp != null) tmp.ResetNonConnected();
            tmp = subscription;
            if (tmp != null) tmp.ResetNonConnected();
        }
        public ServerEndPoint(ConnectionMultiplexer multiplexer, EndPoint endpoint)
        {
            this.multiplexer = multiplexer;
            this.endpoint = endpoint;
            var config = multiplexer.RawConfig;
            version = config.DefaultVersion;
            slaveReadOnly = true;
            isSlave = false;
            databases = 0;
            writeEverySeconds = config.KeepAlive > 0 ? config.KeepAlive : 60;
            interactive = CreateBridge(ConnectionType.Interactive);
            serverType = ServerType.Standalone;

            // overrides for twemproxy
            if (multiplexer.RawConfig.Proxy == Proxy.Twemproxy)
            {
                databases = 1;
                serverType = ServerType.Twemproxy;
            }
        }

        public ClusterConfiguration ClusterConfiguration { get; private set; }

        public int Databases { get { return databases; } set { SetConfig(ref databases, value); } }

        public EndPoint EndPoint { get { return endpoint; } }

        public bool HasDatabases { get { return serverType == ServerType.Standalone; } }

        public bool IsConnected
        {
            get
            {
                var tmp = interactive;
                return tmp != null && tmp.IsConnected;
            }
        }

        public bool IsSlave { get { return isSlave; } set { SetConfig(ref isSlave, value); } }

        public long OperationCount
        {
            get
            {
                long total = 0;
                var tmp = interactive;
                if (tmp != null) total += tmp.OperationCount;
                tmp = subscription;
                if (tmp != null) total += tmp.OperationCount;
                return total;
            }
        }

        public bool RequiresReadMode { get { return serverType == ServerType.Cluster && IsSlave; } }

        public ServerType ServerType { get { return serverType; } set { SetConfig(ref serverType, value); } }

        public bool SlaveReadOnly { get { return slaveReadOnly; } set { SetConfig(ref slaveReadOnly, value); } }

        public Version Version { get { return version; } set { SetConfig(ref version, value); } }

        public int WriteEverySeconds { get { return writeEverySeconds; } set { SetConfig(ref writeEverySeconds, value); } }
        internal ConnectionMultiplexer Multiplexer { get { return multiplexer; } }

        public void ClearUnselectable(UnselectableFlags flags)
        {
            var oldFlags = unselectableReasons;
            if (oldFlags != 0)
            {
                unselectableReasons &= ~flags;
                if (unselectableReasons != oldFlags)
                {
                    multiplexer.Trace(unselectableReasons == 0 ? "Now usable" : ("Now unusable: " + flags), ToString());
                }
            }
        }

        public void Dispose()
        {
            isDisposed = true;
            var tmp = interactive;
            interactive = null;
            if (tmp != null) tmp.Dispose();

            tmp = subscription;
            subscription = null;
            if (tmp != null) tmp.Dispose();
        }

        public PhysicalBridge GetBridge(ConnectionType type, bool create = true)
        {
            if (isDisposed) return null;
            switch (type)
            {
                case ConnectionType.Interactive:
                    return interactive ?? (create ? interactive = CreateBridge(ConnectionType.Interactive) : null);
                case ConnectionType.Subscription:
                    return subscription ?? (create ? subscription = CreateBridge(ConnectionType.Subscription) : null);
            }
            return null;
        }
        public PhysicalBridge GetBridge(RedisCommand command, bool create = true)
        {
            if (isDisposed) return null;
            switch (command)
            {
                case RedisCommand.SUBSCRIBE:
                case RedisCommand.UNSUBSCRIBE:
                case RedisCommand.PSUBSCRIBE:
                case RedisCommand.PUNSUBSCRIBE:
                    return subscription ?? (create ? subscription = CreateBridge(ConnectionType.Subscription) : null);
                default:
                    return interactive;
            }
        }

        public RedisFeatures GetFeatures()
        {
            return new RedisFeatures(version);
        }

        public void SetClusterConfiguration(ClusterConfiguration configuration)
        {

            ClusterConfiguration = configuration;

            if (configuration != null)
            {
                multiplexer.Trace("Updating cluster ranges...");
                multiplexer.UpdateClusterRange(configuration);
                multiplexer.Trace("Resolving genealogy...");
                var thisNode = configuration.Nodes.FirstOrDefault(x => x.EndPoint == this.EndPoint);
                if (thisNode != null)
                {
                    List<ServerEndPoint> slaves = null;
                    ServerEndPoint master = null;
                    foreach (var node in configuration.Nodes)
                    {
                        if (node.NodeId == thisNode.ParentNodeId)
                        {
                            master = multiplexer.GetServerEndPoint(node.EndPoint);
                        }
                        else if (node.ParentNodeId == thisNode.NodeId)
                        {
                            if (slaves == null) slaves = new List<ServerEndPoint>();
                            slaves.Add(multiplexer.GetServerEndPoint(node.EndPoint));
                        }
                    }
                    Master = master;
                    Slaves = slaves == null ? NoSlaves : slaves.ToArray();
                }
                multiplexer.Trace("Cluster configured");
            }
        }

        public void SetUnselectable(UnselectableFlags flags)
        {
            if (flags != 0)
            {
                var oldFlags = unselectableReasons;
                unselectableReasons |= flags;
                if (unselectableReasons != oldFlags)
                {
                    multiplexer.Trace(unselectableReasons == 0 ? "Now usable" : ("Now unusable: " + flags), ToString());
                }
            }
        }
        public override string ToString()
        {
            return Format.ToString(EndPoint);
        }

        public bool TryEnqueue(Message message)
        {
            var bridge = GetBridge(message.Command);
            return bridge != null && bridge.TryEnqueue(message, isSlave);
        }

        internal void Activate(ConnectionType type)
        {
            GetBridge(type, true);
        }

        internal void AddScript(string script, byte[] hash)
        {
            lock (knownScripts)
            {
                knownScripts[script] = hash;
            }
        }

        internal void AutoConfigure(PhysicalConnection connection)
        {
            if (serverType == ServerType.Twemproxy)
            {
                // don't try to detect configuration; all the config commands are disabled, and
                // the fallback master/slave detection won't help
                return;
            }

            var commandMap = multiplexer.CommandMap;
            const CommandFlags flags = CommandFlags.FireAndForget | CommandFlags.HighPriority | CommandFlags.NoRedirect;

            var features = GetFeatures();
            Message msg;

            if (commandMap.IsAvailable(RedisCommand.CONFIG))
            {
                if (multiplexer.RawConfig.KeepAlive <= 0)
                {
                    msg = Message.Create(-1, flags, RedisCommand.CONFIG, RedisLiterals.GET, RedisLiterals.timeout);
                    msg.SetInternalCall();
                    WriteDirectOrQueueFireAndForget(connection, msg, ResultProcessor.AutoConfigure);
                }
                msg = Message.Create(-1, flags, RedisCommand.CONFIG, RedisLiterals.GET, RedisLiterals.slave_read_only);
                msg.SetInternalCall();
                WriteDirectOrQueueFireAndForget(connection, msg, ResultProcessor.AutoConfigure);
                msg = Message.Create(-1, flags, RedisCommand.CONFIG, RedisLiterals.GET, RedisLiterals.databases);
                msg.SetInternalCall();
                WriteDirectOrQueueFireAndForget(connection, msg, ResultProcessor.AutoConfigure);
            }
            if (commandMap.IsAvailable(RedisCommand.INFO))
            {
                lastInfoReplicationCheckTicks = Environment.TickCount;
                if (features.InfoSections)
                {
                    msg = Message.Create(-1, flags, RedisCommand.INFO, RedisLiterals.replication);
                    msg.SetInternalCall();
                    WriteDirectOrQueueFireAndForget(connection, msg, ResultProcessor.AutoConfigure);

                    msg = Message.Create(-1, flags, RedisCommand.INFO, RedisLiterals.server);
                    msg.SetInternalCall();
                    WriteDirectOrQueueFireAndForget(connection, msg, ResultProcessor.AutoConfigure);
                }
                else
                {
                    msg = Message.Create(-1, flags, RedisCommand.INFO);
                    msg.SetInternalCall();
                    WriteDirectOrQueueFireAndForget(connection, msg, ResultProcessor.AutoConfigure);
                }
            }
            else if (commandMap.IsAvailable(RedisCommand.SET))
            {
                // this is a nasty way to find if we are a slave, and it will only work on up-level servers, but...
                RedisKey key = Guid.NewGuid().ToByteArray();
                msg = Message.Create(0, flags, RedisCommand.SET, key, RedisLiterals.slave_read_only, RedisLiterals.PX, 1, RedisLiterals.NX);
                msg.SetInternalCall();
                WriteDirectOrQueueFireAndForget(connection, msg, ResultProcessor.AutoConfigure);
            }
            if (commandMap.IsAvailable(RedisCommand.CLUSTER))
            {
                msg = Message.Create(-1, flags, RedisCommand.CLUSTER, RedisLiterals.NODES);
                msg.SetInternalCall();
                WriteDirectOrQueueFireAndForget(connection, msg, ResultProcessor.ClusterNodes);
            }
        }

        internal Task Close()
        {
            var tmp = interactive;
            Task result;
            if (tmp == null || !tmp.IsConnected || !multiplexer.CommandMap.IsAvailable(RedisCommand.QUIT))
            {
                result = CompletedTask<bool>.Default(null);
            }
            else
            {
                result = QueueDirectAsync(Message.Create(-1, CommandFlags.None, RedisCommand.QUIT), ResultProcessor.DemandOK, bridge: interactive);
            }
            return result;
        }

        internal void FlushScriptCache()
        {
            lock (knownScripts)
            {
                knownScripts.Clear();
            }
        }

        private string runId;
        internal string RunId
        {
            get { return runId; }
            set
            {
                if (value != runId) // we only care about changes
                {
                    // if we had an old run-id, and it has changed, then the
                    // server has been restarted; which means the script cache
                    // is toast
                    if (runId != null) FlushScriptCache();
                    runId = value;
                }
            }
        }

        internal ServerCounters GetCounters()
        {
            var counters = new ServerCounters(endpoint);
            var tmp = interactive;
            if (tmp != null) tmp.GetCounters(counters.Interactive);
            tmp = subscription;
            if (tmp != null) tmp.GetCounters(counters.Subscription);
            return counters;
        }

        internal int GetOutstandingCount(RedisCommand command, out int inst, out int qu, out int qs, out int qc, out int wr, out int wq, out int @in, out int ar)
        {
            var bridge = GetBridge(command, false);
            if (bridge == null)
            {
                return inst = qu = qs = qc = wr = wq = @in = ar = 0;
            }

            return bridge.GetOutstandingCount(out inst, out qu, out qs, out qc, out wr, out wq, out @in, out ar);
        }

        internal string GetProfile()
        {
            var sb = new StringBuilder();
            sb.Append("Circular op-count snapshot; int:");
            var tmp = interactive;
            if (tmp != null) tmp.AppendProfile(sb);
            sb.Append("; sub:");
            tmp = subscription;
            if (tmp != null) tmp.AppendProfile(sb);
            return sb.ToString();
        }

        internal byte[] GetScriptHash(string script, RedisCommand command)
        {
            var found = (byte[])knownScripts[script];
            if(found == null && command == RedisCommand.EVALSHA)
            {
                // the script provided is a hex sha; store and re-use the ascii for that
                found = Encoding.ASCII.GetBytes(script);
                lock(knownScripts)
                {
                    knownScripts[script] = found;
                }
            }
            return found;
        }

        internal string GetStormLog(RedisCommand command)
        {
            var bridge = GetBridge(command);
            return bridge == null ? null : bridge.GetStormLog();
        }

        internal Message GetTracerMessage(bool assertIdentity)
        {
            // different configurations block certain commands, as can ad-hoc local configurations, so
            // we'll do the best with what we have available.
            // note that the muxer-ctor asserts that one of ECHO, PING, TIME of GET is available
            // see also: TracerProcessor
            var map = multiplexer.CommandMap;
            Message msg;
            const CommandFlags flags = CommandFlags.NoRedirect | CommandFlags.FireAndForget;
            if (assertIdentity && map.IsAvailable(RedisCommand.ECHO))
            {
                msg = Message.Create(-1, flags, RedisCommand.ECHO, (RedisValue)multiplexer.UniqueId);
            }
            else if (map.IsAvailable(RedisCommand.PING))
            {
                msg = Message.Create(-1, flags, RedisCommand.PING);
            }
            else if (map.IsAvailable(RedisCommand.TIME))
            {
                msg = Message.Create(-1, flags, RedisCommand.TIME);
            }
            else if (!assertIdentity && map.IsAvailable(RedisCommand.ECHO))
            {
                // we'll use echo as a PING substitute if it is all we have (in preference to EXISTS)
                msg = Message.Create(-1, flags, RedisCommand.ECHO, (RedisValue)multiplexer.UniqueId);
            }
            else
            {
                map.AssertAvailable(RedisCommand.EXISTS);
                msg = Message.Create(0, flags, RedisCommand.EXISTS, (RedisValue)multiplexer.UniqueId);
            }
            msg.SetInternalCall();
            return msg;
        }

        internal bool IsSelectable(RedisCommand command)
        {
            var bridge = unselectableReasons == 0 ? GetBridge(command, false) : null;
            return bridge != null && bridge.IsConnected;
        }

        internal void OnEstablishing(PhysicalConnection connection)
        {
            try
            {
                if (connection == null) return;
                Handshake(connection);
            }
            catch (Exception ex)
            {
                connection.RecordConnectionFailed(ConnectionFailureType.InternalFailure, ex);
            }
        }

        internal void OnFullyEstablished(PhysicalConnection connection)
        {
            try
            {
                if (connection == null) return;
                var bridge = connection.Bridge;
                if (bridge == subscription)
                {
                    multiplexer.ResendSubscriptions(this);
                }
                multiplexer.OnConnectionRestored(endpoint, bridge.ConnectionType);
            }
            catch (Exception ex)
            {
                connection.RecordConnectionFailed(ConnectionFailureType.InternalFailure, ex);
            }
        }

        
        internal int LastInfoReplicationCheckSecondsAgo
        {
            get { return unchecked(Environment.TickCount - Thread.VolatileRead(ref lastInfoReplicationCheckTicks)) / 1000; }
        }

        private EndPoint masterEndPoint;
        public EndPoint MasterEndPoint
        {
            get { return masterEndPoint; }
            set { SetConfig(ref masterEndPoint, value); }
        }


        internal bool CheckInfoReplication()
        {
            lastInfoReplicationCheckTicks = Environment.TickCount;
            PhysicalBridge bridge;
            if (version >= RedisFeatures.v2_8_0 && multiplexer.CommandMap.IsAvailable(RedisCommand.INFO)
                && (bridge = GetBridge(ConnectionType.Interactive, false)) != null)
            {
                var msg = Message.Create(-1, CommandFlags.FireAndForget | CommandFlags.HighPriority | CommandFlags.NoRedirect, RedisCommand.INFO, RedisLiterals.replication);
                msg.SetInternalCall();
                QueueDirectFireAndForget(msg, ResultProcessor.AutoConfigure, bridge);
                return true;
            }
            return false;
        }
        private int lastInfoReplicationCheckTicks;

        internal void OnHeartbeat()
        {
            try
            {
                var tmp = interactive;
                if (tmp != null) tmp.OnHeartbeat(false);
                tmp = subscription;
                if (tmp != null) tmp.OnHeartbeat(false);
            } catch(Exception ex)
            {
                multiplexer.OnInternalError(ex, EndPoint);
            }

        }

        internal Task<T> QueueDirectAsync<T>(Message message, ResultProcessor<T> processor, object asyncState = null, PhysicalBridge bridge = null)
        {
            var tcs = TaskSource.CreateDenyExecSync<T>(asyncState);
            var source = ResultBox<T>.Get(tcs);
            message.SetSource(processor, source);
            if(!(bridge ?? GetBridge(message.Command)).TryEnqueue(message, isSlave))
            {
                ConnectionMultiplexer.ThrowFailed(tcs, ExceptionFactory.NoConnectionAvailable(multiplexer.IncludeDetailInExceptions, message.Command, message, this));
            }
            return tcs.Task;
        }

        internal void QueueDirectFireAndForget<T>(Message message, ResultProcessor<T> processor, PhysicalBridge bridge = null)
        {
            if (message != null)
            {
                message.SetSource(processor, null);
                multiplexer.Trace("Enqueue: " + message);
                (bridge ?? GetBridge(message.Command)).TryEnqueue(message, isSlave);
            }
        }

        internal void ReportNextFailure()
        {
            var tmp = interactive;
            if (tmp != null) tmp.ReportNextFailure();
            tmp = subscription;
            if (tmp != null) tmp.ReportNextFailure();
        }

        internal Task<bool> SendTracer()
        {
            return QueueDirectAsync(GetTracerMessage(false), ResultProcessor.Tracer);
        }

        internal string Summary()
        {
            var sb = new StringBuilder(Format.ToString(endpoint))
                .Append(": ").Append(serverType).Append(" v").Append(version).Append(", ").Append(isSlave ? "slave" : "master");
            

            if (databases > 0) sb.Append("; ").Append(databases).Append(" databases");
            if (writeEverySeconds > 0)
                sb.Append("; keep-alive: ").Append(TimeSpan.FromSeconds(writeEverySeconds));
            var tmp = interactive;
            sb.Append("; int: ").Append(tmp == null ? "n/a" : tmp.ConnectionState.ToString());
            tmp = subscription;
            if(tmp == null)
            {
                sb.Append("; sub: n/a");
            } else
            {
                var state = tmp.ConnectionState;
                sb.Append("; sub: ").Append(state);
                if(state == PhysicalBridge.State.ConnectedEstablished)
                {
                    sb.Append(", ").Append(tmp.SubscriptionCount).Append(" active");
                }
            }

            var flags = unselectableReasons;
            if (flags != 0)
            {
                sb.Append("; not in use: ").Append(flags);
            }
            return sb.ToString();
        }
        internal void WriteDirectOrQueueFireAndForget<T>(PhysicalConnection connection, Message message, ResultProcessor<T> processor)
        {
            if (message != null)
            {
                message.SetSource(processor, null);
                if (connection == null)
                {
                    multiplexer.Trace("Enqueue: " + message);
                    GetBridge(message.Command).TryEnqueue(message, isSlave);
                }
                else
                {
                    multiplexer.Trace("Writing direct: " + message);
                    connection.Bridge.WriteMessageDirect(connection, message);
                }
            }
        }

        private PhysicalBridge CreateBridge(ConnectionType type)
        {
            multiplexer.Trace(type.ToString());
            var bridge = new PhysicalBridge(this, type);
            bridge.TryConnect();
            return bridge;
        }
        void Handshake(PhysicalConnection connection)
        {
            multiplexer.Trace("Server handshake");
            if (connection == null)
            {
                multiplexer.Trace("No connection!?");
                return;
            }
            Message msg;
            string password = multiplexer.RawConfig.Password;
            if (!string.IsNullOrWhiteSpace(password))
            {
                multiplexer.Trace("Sending password");
                msg = Message.Create(-1, CommandFlags.FireAndForget, RedisCommand.AUTH, (RedisValue)password);
                msg.SetInternalCall();
                WriteDirectOrQueueFireAndForget(connection, msg, ResultProcessor.DemandOK);
            }
            if (multiplexer.CommandMap.IsAvailable(RedisCommand.CLIENT))
            {
                string name = multiplexer.ClientName;
                if (!string.IsNullOrWhiteSpace(name))
                {
                    name = nameSanitizer.Replace(name, "");
                    if (!string.IsNullOrWhiteSpace(name))
                    {
                        multiplexer.Trace("Setting client name: " + name);
                        msg = Message.Create(-1, CommandFlags.FireAndForget, RedisCommand.CLIENT, RedisLiterals.SETNAME, (RedisValue)name);
                        msg.SetInternalCall();
                        WriteDirectOrQueueFireAndForget(connection, msg, ResultProcessor.DemandOK);
                    }
                }
            }

            var connType = connection.Bridge.ConnectionType;

            if (connType == ConnectionType.Interactive)
            {
                multiplexer.Trace("Auto-configure...");
                AutoConfigure(connection);
            }
            multiplexer.Trace("Sending critical tracer");
            WriteDirectOrQueueFireAndForget(connection, GetTracerMessage(true), ResultProcessor.EstablishConnection);


            // note: this **must** be the last thing on the subscription handshake, because after this
            // we will be in subscriber mode: regular commands cannot be sent
            if (connType == ConnectionType.Subscription)
            {
                var configChannel = multiplexer.ConfigurationChangedChannel;
                if(configChannel != null)
                {
                    msg = Message.Create(-1, CommandFlags.FireAndForget, RedisCommand.SUBSCRIBE, (RedisChannel)configChannel);
                    WriteDirectOrQueueFireAndForget(connection, msg, ResultProcessor.TrackSubscriptions);
                }
            }

            connection.Flush();
        }

        private void SetConfig<T>(ref T field, T value, [CallerMemberName] string caller = null)
        {
            if(!EqualityComparer<T>.Default.Equals(field, value))
            {
                multiplexer.Trace(caller + " changed from " + field + " to " + value, "Configuration");
                field = value;
                multiplexer.ReconfigureIfNeeded(endpoint, false, caller);
            }
        }
    }
}
