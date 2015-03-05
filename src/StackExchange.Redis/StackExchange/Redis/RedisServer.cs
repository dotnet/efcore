using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace StackExchange.Redis
{

    internal sealed partial class RedisServer : RedisBase, IServer
    {
        private readonly ServerEndPoint server;

        internal RedisServer(ConnectionMultiplexer multiplexer, ServerEndPoint server, object asyncState) : base(multiplexer, asyncState)
        {
            if (server == null) throw new ArgumentNullException("server");
            this.server = server;
        }

        public ClusterConfiguration ClusterConfiguration
        {
            get { return server.ClusterConfiguration; }
        }

        public EndPoint EndPoint { get { return server.EndPoint; } }

        public RedisFeatures Features
        {
            get { return server.GetFeatures(); }
        }

        public bool IsConnected { get { return server.IsConnected; } }

        public bool IsSlave { get { return server.IsSlave; } }

        public ServerType ServerType { get { return server.ServerType; } }

        public Version Version { get { return server.Version; } }

        public void ClientKill(EndPoint endpoint, CommandFlags flags = CommandFlags.None)
        {
            var msg = Message.Create(-1, flags, RedisCommand.CLIENT, RedisLiterals.KILL, (RedisValue)Format.ToString(endpoint));
            ExecuteSync(msg, ResultProcessor.DemandOK);
        }

        public Task ClientKillAsync(EndPoint endpoint, CommandFlags flags = CommandFlags.None)
        {
            var msg = Message.Create(-1, flags, RedisCommand.CLIENT, RedisLiterals.KILL, (RedisValue)Format.ToString(endpoint));
            return ExecuteAsync(msg, ResultProcessor.DemandOK);
        }

        public long ClientKill(long? id = null, ClientType? clientType = null, EndPoint endpoint = null, bool skipMe = true, CommandFlags flags = CommandFlags.None)
        {
            var msg = GetClientKillMessage(endpoint, id, clientType, skipMe, flags);
            return ExecuteSync(msg, ResultProcessor.Int64);
        }

        public Task<long> ClientKillAsync(long? id = null, ClientType? clientType = null, EndPoint endpoint = null, bool skipMe = true, CommandFlags flags = CommandFlags.None)
        {
            var msg = GetClientKillMessage(endpoint, id, clientType, skipMe, flags);
            return ExecuteAsync(msg, ResultProcessor.Int64);
        }
        Message GetClientKillMessage(EndPoint endpoint, long? id, ClientType? clientType, bool skipMe, CommandFlags flags)
        {
            List<RedisValue> parts = new List<RedisValue>(9);
            parts.Add(RedisLiterals.KILL);
            if(id != null)
            {
                parts.Add(RedisLiterals.ID);
                parts.Add(id.Value);
            }
            if (clientType != null)
            {
                parts.Add(RedisLiterals.TYPE);
                switch(clientType.Value)
                {
                    case ClientType.Normal:
                        parts.Add(RedisLiterals.normal);
                        break;
                    case ClientType.Slave:
                        parts.Add(RedisLiterals.slave);
                        break;
                    case ClientType.PubSub:
                        parts.Add(RedisLiterals.pubsub);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException("clientType");
                }
                parts.Add(id.Value);
            }
            if (endpoint != null)
            {
                parts.Add(RedisLiterals.ADDR);
                parts.Add((RedisValue)Format.ToString(endpoint));
            }
            if(!skipMe)
            {
                parts.Add(RedisLiterals.SKIPME);
                parts.Add(RedisLiterals.no);
            }
            return Message.Create(-1, flags, RedisCommand.CLIENT, parts);
        }

        public ClientInfo[] ClientList(CommandFlags flags = CommandFlags.None)
        {
            var msg = Message.Create(-1, flags, RedisCommand.CLIENT, RedisLiterals.LIST);
            return ExecuteSync(msg, ClientInfo.Processor);
        }

        public Task<ClientInfo[]> ClientListAsync(CommandFlags flags = CommandFlags.None)
        {
            var msg = Message.Create(-1, flags, RedisCommand.CLIENT, RedisLiterals.LIST);
            return ExecuteAsync(msg, ClientInfo.Processor);
        }

        public ClusterConfiguration ClusterNodes(CommandFlags flags = CommandFlags.None)
        {
            var msg = Message.Create(-1, flags, RedisCommand.CLUSTER, RedisLiterals.NODES);
            return ExecuteSync(msg, ResultProcessor.ClusterNodes);
        }

        public Task<ClusterConfiguration> ClusterNodesAsync(CommandFlags flags = CommandFlags.None)
        {
            var msg = Message.Create(-1, flags, RedisCommand.CLUSTER, RedisLiterals.NODES);
            return ExecuteAsync(msg, ResultProcessor.ClusterNodes);
        }

        public string ClusterNodesRaw(CommandFlags flags = CommandFlags.None)
        {
            var msg = Message.Create(-1, flags, RedisCommand.CLUSTER, RedisLiterals.NODES);
            return ExecuteSync(msg, ResultProcessor.ClusterNodesRaw);
        }

        public Task<string> ClusterNodesRawAsync(CommandFlags flags = CommandFlags.None)
        {
            var msg = Message.Create(-1, flags, RedisCommand.CLUSTER, RedisLiterals.NODES);
            return ExecuteAsync(msg, ResultProcessor.ClusterNodesRaw);
        }

        public KeyValuePair<string, string>[] ConfigGet(RedisValue pattern = default(RedisValue), CommandFlags flags = CommandFlags.None)
        {
            if (pattern.IsNullOrEmpty) pattern = RedisLiterals.Wildcard;
            var msg = Message.Create(-1, flags, RedisCommand.CONFIG, RedisLiterals.GET, pattern);
            return ExecuteSync(msg, ResultProcessor.StringPairInterleaved);
        }

        public Task<KeyValuePair<string, string>[]> ConfigGetAsync(RedisValue pattern = default(RedisValue), CommandFlags flags = CommandFlags.None)
        {
            if (pattern.IsNullOrEmpty) pattern = RedisLiterals.Wildcard;
            var msg = Message.Create(-1, flags, RedisCommand.CONFIG, RedisLiterals.GET, pattern);
            return ExecuteAsync(msg, ResultProcessor.StringPairInterleaved);
        }

        public void ConfigResetStatistics(CommandFlags flags = CommandFlags.None)
        {
            var msg = Message.Create(-1, flags, RedisCommand.CONFIG, RedisLiterals.RESETSTAT);
            ExecuteSync(msg, ResultProcessor.DemandOK);
        }

        public Task ConfigResetStatisticsAsync(CommandFlags flags = CommandFlags.None)
        {
            var msg = Message.Create(-1, flags, RedisCommand.CONFIG, RedisLiterals.RESETSTAT);
            return ExecuteAsync(msg, ResultProcessor.DemandOK);
        }

        public void ConfigRewrite(CommandFlags flags = CommandFlags.None)
        {
            var msg = Message.Create(-1, flags, RedisCommand.CONFIG, RedisLiterals.REWRITE);
            ExecuteSync(msg, ResultProcessor.DemandOK);
        }

        public Task ConfigRewriteAsync(CommandFlags flags = CommandFlags.None)
        {
            var msg = Message.Create(-1, flags, RedisCommand.CONFIG, RedisLiterals.REWRITE);
            return ExecuteAsync(msg, ResultProcessor.DemandOK);
        }

        public void ConfigSet(RedisValue setting, RedisValue value, CommandFlags flags = CommandFlags.None)
        {
            var msg = Message.Create(-1, flags, RedisCommand.CONFIG, RedisLiterals.SET, setting, value);
            ExecuteSync(msg, ResultProcessor.DemandOK);
            ExecuteSync(Message.Create(-1, flags | CommandFlags.FireAndForget, RedisCommand.CONFIG, RedisLiterals.GET, setting), ResultProcessor.AutoConfigure);
        }

        public Task ConfigSetAsync(RedisValue setting, RedisValue value, CommandFlags flags = CommandFlags.None)
        {
            var msg = Message.Create(-1, flags, RedisCommand.CONFIG, RedisLiterals.SET, setting, value);
            var task = ExecuteAsync(msg, ResultProcessor.DemandOK);
            ExecuteSync(Message.Create(-1, flags | CommandFlags.FireAndForget, RedisCommand.CONFIG, RedisLiterals.GET, setting), ResultProcessor.AutoConfigure);
            return task;
        }
        public long DatabaseSize(int database = 0, CommandFlags flags = CommandFlags.None)
        {
            var msg = Message.Create(database, flags, RedisCommand.DBSIZE);
            return ExecuteSync(msg, ResultProcessor.Int64);
        }

        public Task<long> DatabaseSizeAsync(int database = 0, CommandFlags flags = CommandFlags.None)
        {
            var msg = Message.Create(database, flags, RedisCommand.DBSIZE);
            return ExecuteAsync(msg, ResultProcessor.Int64);
        }

        public void FlushAllDatabases(CommandFlags flags = CommandFlags.None)
        {
            var msg = Message.Create(-1, flags, RedisCommand.FLUSHALL);
            ExecuteSync(msg, ResultProcessor.DemandOK);
        }
        public Task FlushAllDatabasesAsync(CommandFlags flags = CommandFlags.None)
        {
            var msg = Message.Create(-1, flags, RedisCommand.FLUSHALL);
            return ExecuteAsync(msg, ResultProcessor.DemandOK);
        }

        public void FlushDatabase(int database = 0, CommandFlags flags = CommandFlags.None)
        {
            var msg = Message.Create(database, flags, RedisCommand.FLUSHDB);
            ExecuteSync(msg, ResultProcessor.DemandOK);
        }

        public Task FlushDatabaseAsync(int database = 0, CommandFlags flags = CommandFlags.None)
        {
            var msg = Message.Create(database, flags, RedisCommand.FLUSHDB);
            return ExecuteAsync(msg, ResultProcessor.DemandOK);
        }

        public ServerCounters GetCounters()
        {
            return server.GetCounters();
        }

        public IGrouping<string, KeyValuePair<string, string>>[] Info(RedisValue section = default(RedisValue), CommandFlags flags = CommandFlags.None)
        {
            var msg = section.IsNullOrEmpty
                ? Message.Create(-1, flags, RedisCommand.INFO)
                : Message.Create(-1, flags, RedisCommand.INFO, section);

            return ExecuteSync(msg, ResultProcessor.Info);
        }

        public Task<IGrouping<string, KeyValuePair<string, string>>[]> InfoAsync(RedisValue section = default(RedisValue), CommandFlags flags = CommandFlags.None)
        {
            var msg = section.IsNullOrEmpty
                ? Message.Create(-1, flags, RedisCommand.INFO)
                : Message.Create(-1, flags, RedisCommand.INFO, section);

            return ExecuteAsync(msg, ResultProcessor.Info);
        }

        public string InfoRaw(RedisValue section = default(RedisValue), CommandFlags flags = CommandFlags.None)
        {
            var msg = section.IsNullOrEmpty
                ? Message.Create(-1, flags, RedisCommand.INFO)
                : Message.Create(-1, flags, RedisCommand.INFO, section);

            return ExecuteSync(msg, ResultProcessor.String);
        }

        public Task<string> InfoRawAsync(RedisValue section = default(RedisValue), CommandFlags flags = CommandFlags.None)
        {
            var msg = section.IsNullOrEmpty
                ? Message.Create(-1, flags, RedisCommand.INFO)
                : Message.Create(-1, flags, RedisCommand.INFO, section);

            return ExecuteAsync(msg, ResultProcessor.String);
        }

        IEnumerable<RedisKey> IServer.Keys(int database, RedisValue pattern, int pageSize, CommandFlags flags)
        {
            return Keys(database, pattern, pageSize, CursorUtils.Origin, 0, flags);
        }

        public IEnumerable<RedisKey> Keys(int database = 0, RedisValue pattern = default(RedisValue), int pageSize = CursorUtils.DefaultPageSize, long cursor = CursorUtils.Origin, int pageOffset = 0, CommandFlags flags = CommandFlags.None)
        {
            if (pageSize <= 0) throw new ArgumentOutOfRangeException("pageSize");
            if (CursorUtils.IsNil(pattern)) pattern = RedisLiterals.Wildcard;

            if (multiplexer.CommandMap.IsAvailable(RedisCommand.SCAN))
            {
                var features = server.GetFeatures();

                if (features.Scan) return new KeysScanEnumerable(this, database, pattern, pageSize, cursor, pageOffset, flags);
            }

            if (cursor != 0 || pageOffset != 0) throw ExceptionFactory.NoCursor(RedisCommand.KEYS);
            Message msg = Message.Create(database, flags, RedisCommand.KEYS, pattern);
            return ExecuteSync(msg, ResultProcessor.RedisKeyArray);
        }

        public DateTime LastSave(CommandFlags flags = CommandFlags.None)
        {
            var msg = Message.Create(-1, flags, RedisCommand.LASTSAVE);
            return ExecuteSync(msg, ResultProcessor.DateTime);
        }

        public Task<DateTime> LastSaveAsync(CommandFlags flags = CommandFlags.None)
        {
            var msg = Message.Create(-1, flags, RedisCommand.LASTSAVE);
            return ExecuteAsync(msg, ResultProcessor.DateTime);
        }

        public void MakeMaster(ReplicationChangeOptions options, TextWriter log = null)
        {
            multiplexer.MakeMaster(server, options, log);
        }
        public void Save(SaveType type, CommandFlags flags = CommandFlags.None)
        {
            var msg = GetSaveMessage(type, flags);
            ExecuteSync(msg, GetSaveResultProcessor(type));
        }

        public Task SaveAsync(SaveType type, CommandFlags flags = CommandFlags.None)
        {
            var msg = GetSaveMessage(type, flags);
            return ExecuteAsync(msg, GetSaveResultProcessor(type));
        }

        public bool ScriptExists(string script, CommandFlags flags = CommandFlags.None)
        {
            var msg = Message.Create(-1, flags, RedisCommand.SCRIPT, RedisLiterals.EXISTS, ScriptHash.Hash(script));
            return ExecuteSync(msg, ResultProcessor.Boolean);
        }

        public bool ScriptExists(byte[] sha1, CommandFlags flags = CommandFlags.None)
        {
            var msg = Message.Create(-1, flags, RedisCommand.SCRIPT, RedisLiterals.EXISTS, ScriptHash.Encode(sha1));
            return ExecuteSync(msg, ResultProcessor.Boolean);
        }

        public Task<bool> ScriptExistsAsync(string script, CommandFlags flags = CommandFlags.None)
        {
            var msg = Message.Create(-1, flags, RedisCommand.SCRIPT, RedisLiterals.EXISTS, ScriptHash.Hash(script));
            return ExecuteAsync(msg, ResultProcessor.Boolean);
        }

        public Task<bool> ScriptExistsAsync(byte[] sha1, CommandFlags flags = CommandFlags.None)
        {
            var msg = Message.Create(-1, flags, RedisCommand.SCRIPT, RedisLiterals.EXISTS, ScriptHash.Encode(sha1));
            return ExecuteAsync(msg, ResultProcessor.Boolean);
        }

        public void ScriptFlush(CommandFlags flags = CommandFlags.None)
        {
            if (!multiplexer.RawConfig.AllowAdmin) throw ExceptionFactory.AdminModeNotEnabled(multiplexer.IncludeDetailInExceptions, RedisCommand.SCRIPT, null, server);
            var msg = Message.Create(-1, flags, RedisCommand.SCRIPT, RedisLiterals.FLUSH);
            ExecuteSync(msg, ResultProcessor.DemandOK);
        }

        public Task ScriptFlushAsync(CommandFlags flags = CommandFlags.None)
        {
            if (!multiplexer.RawConfig.AllowAdmin) throw ExceptionFactory.AdminModeNotEnabled(multiplexer.IncludeDetailInExceptions, RedisCommand.SCRIPT, null, server);
            var msg = Message.Create(-1, flags, RedisCommand.SCRIPT, RedisLiterals.FLUSH);
            return ExecuteAsync(msg, ResultProcessor.DemandOK);
        }

        public byte[] ScriptLoad(string script, CommandFlags flags = CommandFlags.None)
        {
            var msg = new RedisDatabase.ScriptLoadMessage(flags, script);
            return ExecuteSync(msg, ResultProcessor.ScriptLoad);
        }

        public Task<byte[]> ScriptLoadAsync(string script, CommandFlags flags = CommandFlags.None)
        {
            var msg = new RedisDatabase.ScriptLoadMessage(flags, script);
            return ExecuteAsync(msg, ResultProcessor.ScriptLoad);
        }

        public void Shutdown(ShutdownMode shutdownMode = ShutdownMode.Default, CommandFlags flags = CommandFlags.None)
        {
            Message msg;
            switch (shutdownMode)
            {
                case ShutdownMode.Default:
                    msg = Message.Create(-1, flags, RedisCommand.SHUTDOWN);
                    break;
                case ShutdownMode.Always:
                    msg = Message.Create(-1, flags, RedisCommand.SHUTDOWN, RedisLiterals.SAVE);
                    break;
                case ShutdownMode.Never:
                    msg = Message.Create(-1, flags, RedisCommand.SHUTDOWN, RedisLiterals.NOSAVE);
                    break;
                default:
                    throw new ArgumentOutOfRangeException("shutdownMode");
            }
            try
            {
                ExecuteSync(msg, ResultProcessor.DemandOK);
            }
            catch (RedisConnectionException ex)
            {
                switch (ex.FailureType)
                {
                    case ConnectionFailureType.SocketClosed:
                    case ConnectionFailureType.SocketFailure:
                        // that's fine
                        return;
                }
                throw; // otherwise, not something we were expecting
            }
        }

        public CommandTrace[] SlowlogGet(int count = 0, CommandFlags flags = CommandFlags.None)
        {
            var msg = count > 0
                ? Message.Create(-1, flags, RedisCommand.SLOWLOG, RedisLiterals.GET, count)
                : Message.Create(-1, flags, RedisCommand.SLOWLOG, RedisLiterals.GET);

            return ExecuteSync(msg, CommandTrace.Processor);
        }

        public Task<CommandTrace[]> SlowlogGetAsync(int count = 0, CommandFlags flags = CommandFlags.None)
        {
            var msg = count > 0
                ? Message.Create(-1, flags, RedisCommand.SLOWLOG, RedisLiterals.GET, count)
                : Message.Create(-1, flags, RedisCommand.SLOWLOG, RedisLiterals.GET);

            return ExecuteAsync(msg, CommandTrace.Processor);
        }

        public void SlowlogReset(CommandFlags flags = CommandFlags.None)
        {
            var msg = Message.Create(-1, flags, RedisCommand.SLOWLOG, RedisLiterals.RESET);
            ExecuteSync(msg, ResultProcessor.DemandOK);
        }

        public Task SlowlogResetAsync(CommandFlags flags = CommandFlags.None)
        {
            var msg = Message.Create(-1, flags, RedisCommand.SLOWLOG, RedisLiterals.RESET);
            return ExecuteAsync(msg, ResultProcessor.DemandOK);
        }

        public RedisValue StringGet(int db, RedisKey key, CommandFlags flags = CommandFlags.None)
        {
            var msg = Message.Create(db, flags, RedisCommand.GET, key);
            return ExecuteSync(msg, ResultProcessor.RedisValue);
        }

        public Task<RedisValue> StringGetAsync(int db, RedisKey key, CommandFlags flags = CommandFlags.None)
        {
            var msg = Message.Create(db, flags, RedisCommand.GET, key);
            return ExecuteAsync(msg, ResultProcessor.RedisValue);
        }

        public RedisChannel[] SubscriptionChannels(RedisChannel pattern = default(RedisChannel), CommandFlags flags = CommandFlags.None)
        {
            var msg = pattern.IsNullOrEmpty ? Message.Create(-1, flags, RedisCommand.PUBSUB, RedisLiterals.CHANNELS)
                : Message.Create(-1, flags, RedisCommand.PUBSUB, RedisLiterals.CHANNELS, pattern);
            return ExecuteSync(msg, ResultProcessor.RedisChannelArrayLiteral);
        }

        public Task<RedisChannel[]> SubscriptionChannelsAsync(RedisChannel pattern = default(RedisChannel), CommandFlags flags = CommandFlags.None)
        {
            var msg = pattern.IsNullOrEmpty ? Message.Create(-1, flags, RedisCommand.PUBSUB, RedisLiterals.CHANNELS)
                : Message.Create(-1, flags, RedisCommand.PUBSUB, RedisLiterals.CHANNELS, pattern);
            return ExecuteAsync(msg, ResultProcessor.RedisChannelArrayLiteral);
        }

        public long SubscriptionPatternCount(CommandFlags flags = CommandFlags.None)
        {
            var msg = Message.Create(-1, flags, RedisCommand.PUBSUB, RedisLiterals.NUMPAT);
            return ExecuteSync(msg, ResultProcessor.Int64);
        }

        public Task<long> SubscriptionPatternCountAsync(CommandFlags flags = CommandFlags.None)
        {
            var msg = Message.Create(-1, flags, RedisCommand.PUBSUB, RedisLiterals.NUMPAT);
            return ExecuteAsync(msg, ResultProcessor.Int64);
        }

        public long SubscriptionSubscriberCount(RedisChannel channel, CommandFlags flags = CommandFlags.None)
        {
            var msg = Message.Create(-1, flags, RedisCommand.PUBSUB, RedisLiterals.NUMSUB, channel);
            return ExecuteSync(msg, ResultProcessor.PubSubNumSub);
        }

        public Task<long> SubscriptionSubscriberCountAsync(RedisChannel channel, CommandFlags flags = CommandFlags.None)
        {
            var msg = Message.Create(-1, flags, RedisCommand.PUBSUB, RedisLiterals.NUMSUB, channel);
            return ExecuteAsync(msg, ResultProcessor.PubSubNumSub);
        }

        public DateTime Time(CommandFlags flags = CommandFlags.None)
        {
            var msg = Message.Create(-1, flags, RedisCommand.TIME);
            return ExecuteSync(msg, ResultProcessor.DateTime);
        }

        public Task<DateTime> TimeAsync(CommandFlags flags = CommandFlags.None)
        {
            var msg = Message.Create(-1, flags, RedisCommand.TIME);
            return ExecuteAsync(msg, ResultProcessor.DateTime);
        }

        internal static Message CreateSlaveOfMessage(EndPoint endpoint, CommandFlags flags = CommandFlags.None)
        {
            RedisValue host, port;
            if (endpoint == null)
            {
                host = "NO";
                port = "ONE";
            }
            else
            {
                string hostRaw;
                int portRaw;
                if (Format.TryGetHostPort(endpoint, out hostRaw, out portRaw))
                {
                    host = hostRaw;
                    port = portRaw;
                }
                else
                {
                    throw new NotSupportedException("Unknown endpoint type: " + endpoint.GetType().Name);
                }
            }
            return Message.Create(-1, flags, RedisCommand.SLAVEOF, host, port);
        }

        internal override Task<T> ExecuteAsync<T>(Message message, ResultProcessor<T> processor, ServerEndPoint server = null)
        {   // inject our expected server automatically
            if (server == null) server = this.server;
            FixFlags(message, server);
            if (!server.IsConnected)
            {
                if (message == null) return CompletedTask<T>.Default(asyncState);
                if (message.IsFireAndForget) return CompletedTask<T>.Default(null); // F+F explicitly does not get async-state

                // no need to deny exec-sync here; will be complete before they see if
                var tcs = TaskSource.Create<T>(asyncState);
                ConnectionMultiplexer.ThrowFailed(tcs, ExceptionFactory.NoConnectionAvailable(multiplexer.IncludeDetailInExceptions, message.Command, message, server));
                return tcs.Task;
            }
            return base.ExecuteAsync<T>(message, processor, server);
        }

        internal override T ExecuteSync<T>(Message message, ResultProcessor<T> processor, ServerEndPoint server = null)
        {   // inject our expected server automatically
            if (server == null) server = this.server;
            FixFlags(message, server);
            if (!server.IsConnected)
            {
                if (message == null || message.IsFireAndForget) return default(T);
                throw ExceptionFactory.NoConnectionAvailable(multiplexer.IncludeDetailInExceptions, message.Command, message, server);
            }
            return base.ExecuteSync<T>(message, processor, server);
        }

        internal override RedisFeatures GetFeatures(int db, RedisKey key, CommandFlags flags, out ServerEndPoint server)
        {
            server = this.server;
            return new RedisFeatures(server.Version);
        }

        public void SlaveOf(EndPoint endpoint, CommandFlags flags = CommandFlags.None)
        {
            var msg = CreateSlaveOfMessage(endpoint, flags);
            if (endpoint == server.EndPoint)
            {
                throw new ArgumentException("Cannot slave to self");
            }
            ExecuteSync(msg, ResultProcessor.DemandOK);
        }

        public Task SlaveOfAsync(EndPoint endpoint, CommandFlags flags = CommandFlags.None)
        {
            var msg = CreateSlaveOfMessage(endpoint, flags);
            if (endpoint == server.EndPoint)
            {
                throw new ArgumentException("Cannot slave to self");
            }
            return ExecuteAsync(msg, ResultProcessor.DemandOK);
        }

        private void FixFlags(Message message, ServerEndPoint server)
        {
            // since the server is specified explicitly, we don't want defaults
            // to make the "non-preferred-endpoint" counters look artificially
            // inflated; note we only change *prefer* options
            switch (Message.GetMasterSlaveFlags(message.Flags))
            {
                case CommandFlags.PreferMaster:
                    if (server.IsSlave) message.SetPreferSlave();
                    break;
                case CommandFlags.PreferSlave:
                    if (!server.IsSlave) message.SetPreferMaster();
                    break;
            }
        }

        Message GetSaveMessage(SaveType type, CommandFlags flags = CommandFlags.None)
        {
            switch(type)
            {
                case SaveType.BackgroundRewriteAppendOnlyFile: return Message.Create(-1, flags, RedisCommand.BGREWRITEAOF);
                case SaveType.BackgroundSave: return Message.Create(-1, flags, RedisCommand.BGSAVE);
#pragma warning disable 0618
                case SaveType.ForegroundSave: return Message.Create(-1, flags, RedisCommand.SAVE);
#pragma warning restore 0618
                default:  throw new ArgumentOutOfRangeException("type");
            }
        }

        ResultProcessor<bool> GetSaveResultProcessor(SaveType type)
        {
            switch (type)
            {
                case SaveType.BackgroundRewriteAppendOnlyFile: return ResultProcessor.DemandOK;
                case SaveType.BackgroundSave: return ResultProcessor.BackgroundSaveStarted;
#pragma warning disable 0618
                case SaveType.ForegroundSave: return ResultProcessor.DemandOK;
#pragma warning restore 0618
                default: throw new ArgumentOutOfRangeException("type");
            }
        }

        static class ScriptHash
        {
            static readonly byte[] hex = {
                (byte)'0', (byte)'1', (byte)'2', (byte)'3', (byte)'4', (byte)'5', (byte)'6', (byte)'7',
                (byte)'8', (byte)'9', (byte)'a', (byte)'b', (byte)'c', (byte)'d', (byte)'e', (byte)'f' };
            public static RedisValue Encode(byte[] value)
            {
                if (value == null) return default(RedisValue);
                byte[] result = new byte[value.Length * 2];
                int offset = 0;
                for (int i = 0; i < value.Length; i++)
                {
                    int val = value[i];
                    result[offset++] = hex[val / 16];
                    result[offset++] = hex[val % 16];
                }
                return result;
            }
            public static RedisValue Hash(string value)
            {
                if (value == null) return default(RedisValue);
                using (var sha1 = SHA1.Create())
                {
                    var bytes = sha1.ComputeHash(Encoding.UTF8.GetBytes(value));
                    return Encode(bytes);
                }
            }
        }

        sealed class KeysScanEnumerable : CursorEnumerable<RedisKey>
        {
            private readonly RedisValue pattern;

            public KeysScanEnumerable(RedisServer server, int db, RedisValue pattern, int pageSize, long cursor, int pageOffset, CommandFlags flags)
                : base(server, server.server, db, pageSize, cursor, pageOffset, flags)
            {
                this.pattern = pattern;
            }

            protected override Message CreateMessage(long cursor)
            {
                if (CursorUtils.IsNil(pattern))
                {
                    if (pageSize == CursorUtils.DefaultPageSize)
                    {
                        return Message.Create(db, flags, RedisCommand.SCAN, cursor);
                    }
                    else
                    {
                        return Message.Create(db, flags, RedisCommand.SCAN, cursor, RedisLiterals.COUNT, pageSize);
                    }
                }
                else
                {
                    if (pageSize == CursorUtils.DefaultPageSize)
                    {
                        return Message.Create(db, flags, RedisCommand.SCAN, cursor, RedisLiterals.MATCH, pattern);
                    }
                    else
                    {
                        return Message.Create(db, flags, RedisCommand.SCAN, cursor, RedisLiterals.MATCH, pattern, RedisLiterals.COUNT, pageSize);
                    }
                }
            }
            protected override ResultProcessor<ScanResult> Processor
            {
                get { return processor; }
            }

            public static readonly ResultProcessor<ScanResult> processor = new KeysResultProcessor();
            private class KeysResultProcessor : ResultProcessor<ScanResult>
            {
                protected override bool SetResultCore(PhysicalConnection connection, Message message, RawResult result)
                {
                    switch (result.Type)
                    {
                        case ResultType.MultiBulk:
                            var arr = result.GetItems();
                            long i64;
                            if (arr.Length == 2 && arr[1].Type == ResultType.MultiBulk && arr[0].TryGetInt64(out i64))
                            {
                                var keysResult = new ScanResult(i64, arr[1].GetItemsAsKeys());
                                SetResult(message, keysResult);
                                return true;
                            }
                            break;
                    }
                    return false;
                }
            }
        }

        #region Sentinel

        public EndPoint SentinelGetMasterAddressByName(string serviceName, CommandFlags flags = CommandFlags.None)
        {
            var msg = Message.Create(-1, flags, RedisCommand.SENTINEL, RedisLiterals.GETMASTERADDRBYNAME, (RedisValue)serviceName);
            return ExecuteSync(msg, ResultProcessor.SentinelMasterEndpoint);
        }

        public Task<EndPoint> SentinelGetMasterAddressByNameAsync(string serviceName, CommandFlags flags = CommandFlags.None)
        {
            var msg = Message.Create(-1, flags, RedisCommand.SENTINEL, RedisLiterals.GETMASTERADDRBYNAME, (RedisValue)serviceName);
            return ExecuteAsync(msg, ResultProcessor.SentinelMasterEndpoint);
        }

        public KeyValuePair<string, string>[] SentinelMaster(string serviceName, CommandFlags flags = CommandFlags.None)
        {
            var msg = Message.Create(-1, flags, RedisCommand.SENTINEL, RedisLiterals.MASTER, (RedisValue)serviceName);
            return ExecuteSync(msg, ResultProcessor.StringPairInterleaved);
        }

        public Task<KeyValuePair<string, string>[]> SentinelMasterAsync(string serviceName, CommandFlags flags = CommandFlags.None)
        {
            var msg = Message.Create(-1, flags, RedisCommand.SENTINEL, RedisLiterals.MASTER, (RedisValue)serviceName);
            return ExecuteAsync(msg, ResultProcessor.StringPairInterleaved);
        }

        public void SentinelFailover(string serviceName, CommandFlags flags = CommandFlags.None)
        {
            var msg = Message.Create(-1, flags, RedisCommand.SENTINEL, RedisLiterals.FAILOVER, (RedisValue)serviceName);
            ExecuteSync(msg, ResultProcessor.DemandOK);
        }

        public Task SentinelFailoverAsync(string serviceName, CommandFlags flags = CommandFlags.None)
        {
            var msg = Message.Create(-1, flags, RedisCommand.SENTINEL, RedisLiterals.FAILOVER, (RedisValue)serviceName);
            return ExecuteAsync(msg, ResultProcessor.DemandOK);
        }

        public KeyValuePair<string, string>[][] SentinelMasters(CommandFlags flags = CommandFlags.None)
        {
            var msg = Message.Create(-1, flags, RedisCommand.SENTINEL, RedisLiterals.MASTERS);
            return ExecuteSync(msg, ResultProcessor.SentinelArrayOfArrays);
        }

        public Task<KeyValuePair<string, string>[][]> SentinelMastersAsync(CommandFlags flags = CommandFlags.None)
        {
            var msg = Message.Create(-1, flags, RedisCommand.SENTINEL, RedisLiterals.MASTERS);
            return ExecuteAsync(msg, ResultProcessor.SentinelArrayOfArrays);
        }

        public KeyValuePair<string, string>[][] SentinelSlaves(string serviceName, CommandFlags flags = CommandFlags.None)
        {
            var msg = Message.Create(-1, flags, RedisCommand.SENTINEL, RedisLiterals.SLAVES, (RedisValue)serviceName);
            return ExecuteSync(msg, ResultProcessor.SentinelArrayOfArrays);
        }

        public Task<KeyValuePair<string, string>[][]> SentinelSlavesAsync(string serviceName, CommandFlags flags = CommandFlags.None)
        {
            var msg = Message.Create(-1, flags, RedisCommand.SENTINEL, RedisLiterals.SLAVES, (RedisValue)serviceName);
            return ExecuteAsync(msg, ResultProcessor.SentinelArrayOfArrays);
        }

        #endregion
    }
}
