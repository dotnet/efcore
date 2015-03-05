using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace StackExchange.Redis
{
    internal class RedisDatabase : RedisBase, IDatabase
    {
        private readonly int Db;

        internal RedisDatabase(ConnectionMultiplexer multiplexer, int db, object asyncState) : base(multiplexer, asyncState)
        {
            this.Db = db;
        }

        public object AsyncState { get { return asyncState; } }

        public int Database { get { return Db; } }

        public IBatch CreateBatch(object asyncState)
        {
            if (this is IBatch) throw new NotSupportedException("Nested batches are not supported");
            return new RedisBatch(this, asyncState);
        }

        public ITransaction CreateTransaction(object asyncState)
        {
            if (this is IBatch) throw new NotSupportedException("Nested transactions are not supported");
            return new RedisTransaction(this, asyncState);
        }

        private ITransaction CreateTransactionIfAvailable(object asyncState)
        {
            var map = multiplexer.CommandMap;
            if(!map.IsAvailable(RedisCommand.MULTI) || !map.IsAvailable(RedisCommand.EXEC))
            {
                return null;
            }
            return CreateTransaction(asyncState);
        }

        public RedisValue DebugObject(RedisKey key, CommandFlags flags = CommandFlags.None)
        {
            var msg = Message.Create(Db, flags, RedisCommand.DEBUG, RedisLiterals.OBJECT, key);
            return ExecuteSync(msg, ResultProcessor.RedisValue);
        }

        public Task<RedisValue> DebugObjectAsync(RedisKey key, CommandFlags flags = CommandFlags.None)
        {
            var msg = Message.Create(Db, flags, RedisCommand.DEBUG, RedisLiterals.OBJECT, key);
            return ExecuteAsync(msg, ResultProcessor.RedisValue);
        }

        public long HashDecrement(RedisKey key, RedisValue hashField, long value = 1, CommandFlags flags = CommandFlags.None)
        {
            return HashIncrement(key, hashField, -value, flags);
        }

        public double HashDecrement(RedisKey key, RedisValue hashField, double value, CommandFlags flags = CommandFlags.None)
        {
            return HashIncrement(key, hashField, -value, flags);
        }

        public Task<long> HashDecrementAsync(RedisKey key, RedisValue hashField, long value = 1, CommandFlags flags = CommandFlags.None)
        {
            return HashIncrementAsync(key, hashField, -value, flags);
        }

        public Task<double> HashDecrementAsync(RedisKey key, RedisValue hashField, double value, CommandFlags flags = CommandFlags.None)
        {
            return HashIncrementAsync(key, hashField, -value, flags);
        }

        public bool HashDelete(RedisKey key, RedisValue hashField, CommandFlags flags = CommandFlags.None)
        {
            var msg = Message.Create(Db, flags, RedisCommand.HDEL, key, hashField);
            return ExecuteSync(msg, ResultProcessor.Boolean);
        }

        public long HashDelete(RedisKey key, RedisValue[] hashFields, CommandFlags flags = CommandFlags.None)
        {
            if (hashFields == null) throw new ArgumentNullException("hashFields");
            var msg = hashFields.Length == 0 ? null : Message.Create(Db, flags, RedisCommand.HDEL, key, hashFields);
            return ExecuteSync(msg, ResultProcessor.Int64);
        }

        public Task<bool> HashDeleteAsync(RedisKey key, RedisValue hashField, CommandFlags flags = CommandFlags.None)
        {
            var msg = Message.Create(Db, flags, RedisCommand.HDEL, key, hashField);
            return ExecuteAsync(msg, ResultProcessor.Boolean);
        }

        public Task<long> HashDeleteAsync(RedisKey key, RedisValue[] hashFields, CommandFlags flags = CommandFlags.None)
        {
            if (hashFields == null) throw new ArgumentNullException("hashFields");

            var msg = hashFields.Length == 0 ? null : Message.Create(Db, flags, RedisCommand.HDEL, key, hashFields);
            return ExecuteAsync(msg, ResultProcessor.Int64);
        }

        public bool HashExists(RedisKey key, RedisValue hashField, CommandFlags flags = CommandFlags.None)
        {
            var msg = Message.Create(Db, flags, RedisCommand.HEXISTS, key, hashField);
            return ExecuteSync(msg, ResultProcessor.Boolean);
        }

        public Task<bool> HashExistsAsync(RedisKey key, RedisValue hashField, CommandFlags flags = CommandFlags.None)
        {
            var msg = Message.Create(Db, flags, RedisCommand.HEXISTS, key, hashField);
            return ExecuteAsync(msg, ResultProcessor.Boolean);
        }

        public RedisValue HashGet(RedisKey key, RedisValue hashField, CommandFlags flags = CommandFlags.None)
        {
            var msg = Message.Create(Db, flags, RedisCommand.HGET, key, hashField);
            return ExecuteSync(msg, ResultProcessor.RedisValue);
        }

        public RedisValue[] HashGet(RedisKey key, RedisValue[] hashFields, CommandFlags flags = CommandFlags.None)
        {
            if (hashFields == null) throw new ArgumentNullException("hashFields");
            if (hashFields.Length == 0) return new RedisValue[0];
            var msg = Message.Create(Db, flags, RedisCommand.HMGET, key, hashFields);
            return ExecuteSync(msg, ResultProcessor.RedisValueArray);
        }

        public HashEntry[] HashGetAll(RedisKey key, CommandFlags flags = CommandFlags.None)
        {
            var msg = Message.Create(Db, flags, RedisCommand.HGETALL, key);
            return ExecuteSync(msg, ResultProcessor.HashEntryArray);
        }

        public Task<HashEntry[]> HashGetAllAsync(RedisKey key, CommandFlags flags = CommandFlags.None)
        {
            var msg = Message.Create(Db, flags, RedisCommand.HGETALL, key);
            return ExecuteAsync(msg, ResultProcessor.HashEntryArray);
        }

        public Task<RedisValue> HashGetAsync(RedisKey key, RedisValue hashField, CommandFlags flags = CommandFlags.None)
        {
            var msg = Message.Create(Db, flags, RedisCommand.HGET, key, hashField);
            return ExecuteAsync(msg, ResultProcessor.RedisValue);
        }

        public Task<RedisValue[]> HashGetAsync(RedisKey key, RedisValue[] hashFields, CommandFlags flags = CommandFlags.None)
        {
            if (hashFields == null) throw new ArgumentNullException("hashFields");
            if (hashFields.Length == 0) return CompletedTask<RedisValue[]>.FromResult(new RedisValue[0], asyncState);
            var msg = Message.Create(Db, flags, RedisCommand.HMGET, key, hashFields);
            return ExecuteAsync(msg, ResultProcessor.RedisValueArray);
        }

        public long HashIncrement(RedisKey key, RedisValue hashField, long value = 1, CommandFlags flags = CommandFlags.None)
        {
            var msg = value == 0 && (flags & CommandFlags.FireAndForget) != 0
                ? null : Message.Create(Db, flags, RedisCommand.HINCRBY, key, hashField, value);
            return ExecuteSync(msg, ResultProcessor.Int64);
        }

        public double HashIncrement(RedisKey key, RedisValue hashField, double value, CommandFlags flags = CommandFlags.None)
        {
            var msg = value == 0 && (flags & CommandFlags.FireAndForget) != 0
                ? null : Message.Create(Db, flags, RedisCommand.HINCRBYFLOAT, key, hashField, value);
            return ExecuteSync(msg, ResultProcessor.Double);
        }

        public Task<long> HashIncrementAsync(RedisKey key, RedisValue hashField, long value = 1, CommandFlags flags = CommandFlags.None)
        {
            var msg = value == 0 && (flags & CommandFlags.FireAndForget) != 0
                ? null : Message.Create(Db, flags, RedisCommand.HINCRBY, key, hashField, value);
            return ExecuteAsync(msg, ResultProcessor.Int64);
        }

        public Task<double> HashIncrementAsync(RedisKey key, RedisValue hashField, double value, CommandFlags flags = CommandFlags.None)
        {
            var msg = value == 0 && (flags & CommandFlags.FireAndForget) != 0
                ? null : Message.Create(Db, flags, RedisCommand.HINCRBYFLOAT, key, hashField, value);
            return ExecuteAsync(msg, ResultProcessor.Double);
        }

        public RedisValue[] HashKeys(RedisKey key, CommandFlags flags = CommandFlags.None)
        {
            var msg = Message.Create(Db, flags, RedisCommand.HKEYS, key);
            return ExecuteSync(msg, ResultProcessor.RedisValueArray);
        }

        public Task<RedisValue[]> HashKeysAsync(RedisKey key, CommandFlags flags = CommandFlags.None)
        {
            var msg = Message.Create(Db, flags, RedisCommand.HKEYS, key);
            return ExecuteAsync(msg, ResultProcessor.RedisValueArray);
        }

        public long HashLength(RedisKey key, CommandFlags flags = CommandFlags.None)
        {
            var msg = Message.Create(Db, flags, RedisCommand.HLEN, key);
            return ExecuteSync(msg, ResultProcessor.Int64);
        }

        public Task<long> HashLengthAsync(RedisKey key, CommandFlags flags = CommandFlags.None)
        {
            var msg = Message.Create(Db, flags, RedisCommand.HLEN, key);
            return ExecuteAsync(msg, ResultProcessor.Int64);
        }

        IEnumerable<HashEntry> IDatabase.HashScan(RedisKey key, RedisValue pattern, int pageSize, CommandFlags flags)
        {
            return HashScan(key, pattern, pageSize, CursorUtils.Origin, 0, flags);
        }

        public IEnumerable<HashEntry> HashScan(RedisKey key, RedisValue pattern = default(RedisValue), int pageSize = CursorUtils.DefaultPageSize, long cursor = CursorUtils.Origin, int pageOffset = 0, CommandFlags flags = CommandFlags.None)
        {
            var scan = TryScan<HashEntry>(key, pattern, pageSize, cursor, pageOffset, flags, RedisCommand.HSCAN, HashScanResultProcessor.Default);
            if (scan != null) return scan;

            if (cursor != 0 || pageOffset != 0) throw ExceptionFactory.NoCursor(RedisCommand.HGETALL);
            if (pattern.IsNull) return HashGetAll(key, flags);
            throw ExceptionFactory.NotSupported(true, RedisCommand.HSCAN);
        }

        public bool HashSet(RedisKey key, RedisValue hashField, RedisValue value, When when = When.Always, CommandFlags flags = CommandFlags.None)
        {
            WhenAlwaysOrNotExists(when);
            var msg = value.IsNull
                ? Message.Create(Db, flags, RedisCommand.HDEL, key, hashField)
                : Message.Create(Db, flags, when == When.Always ? RedisCommand.HSET : RedisCommand.HSETNX, key, hashField, value);
            return ExecuteSync(msg, ResultProcessor.Boolean);
        }

        public void HashSet(RedisKey key, HashEntry[] hashFields, CommandFlags flags = CommandFlags.None)
        {
            var msg = GetHashSetMessage(key, hashFields, flags);
            if (msg == null) return;
            ExecuteSync(msg, ResultProcessor.DemandOK);
        }

        public Task<bool> HashSetAsync(RedisKey key, RedisValue hashField, RedisValue value, When when = When.Always, CommandFlags flags = CommandFlags.None)
        {
            WhenAlwaysOrNotExists(when);
            var msg = value.IsNull
                ? Message.Create(Db, flags, RedisCommand.HDEL, key, hashField)
                : Message.Create(Db, flags, when == When.Always ? RedisCommand.HSET : RedisCommand.HSETNX, key, hashField, value);
            return ExecuteAsync(msg, ResultProcessor.Boolean);
        }

        public Task HashSetAsync(RedisKey key, HashEntry[] hashFields, CommandFlags flags = CommandFlags.None)
        {
            var msg = GetHashSetMessage(key, hashFields, flags);
            return ExecuteAsync(msg, ResultProcessor.DemandOK);
        }

        public Task<bool> HashSetIfNotExistsAsync(RedisKey key, RedisValue hashField, RedisValue value, CommandFlags flags)
        {
            var msg = Message.Create(Db, flags, RedisCommand.HSETNX, key, hashField, value);
            return ExecuteAsync(msg, ResultProcessor.Boolean);
        }

        public RedisValue[] HashValues(RedisKey key, CommandFlags flags = CommandFlags.None)
        {
            var msg = Message.Create(Db, flags, RedisCommand.HVALS, key);
            return ExecuteSync(msg, ResultProcessor.RedisValueArray);
        }

        public Task<RedisValue[]> HashValuesAsync(RedisKey key, CommandFlags flags = CommandFlags.None)
        {
            var msg = Message.Create(Db, flags, RedisCommand.HVALS, key);
            return ExecuteAsync(msg, ResultProcessor.RedisValueArray);
        }

        public bool HyperLogLogAdd(RedisKey key, RedisValue value, CommandFlags flags = CommandFlags.None)
        {
            var cmd = Message.Create(Db, flags, RedisCommand.PFADD, key, value);
            return ExecuteSync(cmd, ResultProcessor.Boolean);
        }

        public bool HyperLogLogAdd(RedisKey key, RedisValue[] values, CommandFlags flags = CommandFlags.None)
        {
            var cmd = Message.Create(Db, flags, RedisCommand.PFADD, key, values);
            return ExecuteSync(cmd, ResultProcessor.Boolean);
        }

        public Task<bool> HyperLogLogAddAsync(RedisKey key, RedisValue value, CommandFlags flags = CommandFlags.None)
        {
            var cmd = Message.Create(Db, flags, RedisCommand.PFADD, key, value);
            return ExecuteAsync(cmd, ResultProcessor.Boolean);
        }

        public Task<bool> HyperLogLogAddAsync(RedisKey key, RedisValue[] values, CommandFlags flags = CommandFlags.None)
        {
            var cmd = Message.Create(Db, flags, RedisCommand.PFADD, key, values);
            return ExecuteAsync(cmd, ResultProcessor.Boolean);
        }

        public long HyperLogLogLength(RedisKey key, CommandFlags flags = CommandFlags.None)
        {
            var cmd = Message.Create(Db, flags, RedisCommand.PFCOUNT, key);
            return ExecuteSync(cmd, ResultProcessor.Int64);
        }

        public long HyperLogLogLength(RedisKey[] keys, CommandFlags flags = CommandFlags.None)
        {
            if (keys == null) throw new ArgumentNullException("keys");

            var cmd = Message.Create(Db, flags, RedisCommand.PFCOUNT, keys);
            return ExecuteSync(cmd, ResultProcessor.Int64);
        }

        public Task<long> HyperLogLogLengthAsync(RedisKey key, CommandFlags flags = CommandFlags.None)
        {
            var cmd = Message.Create(Db, flags, RedisCommand.PFCOUNT, key);
            return ExecuteAsync(cmd, ResultProcessor.Int64);
        }

        public Task<long> HyperLogLogLengthAsync(RedisKey[] keys, CommandFlags flags = CommandFlags.None)
        {
            if (keys == null) throw new ArgumentNullException("keys");

            var cmd = Message.Create(Db, flags, RedisCommand.PFCOUNT, keys);
            return ExecuteAsync(cmd, ResultProcessor.Int64);
        }

        public void HyperLogLogMerge(RedisKey destination, RedisKey first, RedisKey second, CommandFlags flags = CommandFlags.None)
        {
            var cmd = Message.Create(Db, flags, RedisCommand.PFMERGE, destination, first, second);
            ExecuteSync(cmd, ResultProcessor.DemandOK);
        }

        public void HyperLogLogMerge(RedisKey destination, RedisKey[] sourceKeys, CommandFlags flags = CommandFlags.None)
        {
            var cmd = Message.Create(Db, flags, RedisCommand.PFMERGE, destination, sourceKeys);
            ExecuteSync(cmd, ResultProcessor.DemandOK);
        }

        public Task HyperLogLogMergeAsync(RedisKey destination, RedisKey first, RedisKey second, CommandFlags flags = CommandFlags.None)
        {
            var cmd = Message.Create(Db, flags, RedisCommand.PFMERGE, destination, first, second);
            return ExecuteAsync(cmd, ResultProcessor.DemandOK);
        }

        public Task HyperLogLogMergeAsync(RedisKey destination, RedisKey[] sourceKeys, CommandFlags flags = CommandFlags.None)
        {
            var cmd = Message.Create(Db, flags, RedisCommand.PFMERGE, destination, sourceKeys);
            return ExecuteAsync(cmd, ResultProcessor.DemandOK);
        }

        public EndPoint IdentifyEndpoint(RedisKey key = default(RedisKey), CommandFlags flags = CommandFlags.None)
        {
            var msg = key.IsNull ? Message.Create(Db, flags, RedisCommand.PING) : Message.Create(Db, flags, RedisCommand.EXISTS, key);
            return ExecuteSync(msg, ResultProcessor.ConnectionIdentity);
        }

        public Task<EndPoint> IdentifyEndpointAsync(RedisKey key = default(RedisKey), CommandFlags flags = CommandFlags.None)
        {
            var msg = key.IsNull ? Message.Create(Db, flags, RedisCommand.PING) : Message.Create(Db, flags, RedisCommand.EXISTS, key);
            return ExecuteAsync(msg, ResultProcessor.ConnectionIdentity);
        }

        public bool IsConnected(RedisKey key, CommandFlags flags = CommandFlags.None)
        {
            var server = multiplexer.SelectServer(Db, RedisCommand.PING, flags, key);
            return server != null && server.IsConnected;
        }

        public bool KeyDelete(RedisKey key, CommandFlags flags = CommandFlags.None)
        {
            var msg = Message.Create(Db, flags, RedisCommand.DEL, key);
            return ExecuteSync(msg, ResultProcessor.DemandZeroOrOne);
        }

        public long KeyDelete(RedisKey[] keys, CommandFlags flags = CommandFlags.None)
        {
            if (keys == null) throw new ArgumentNullException("keys");
            var msg = keys.Length == 0 ? null : Message.Create(Db, flags, RedisCommand.DEL, keys);
            return ExecuteSync(msg, ResultProcessor.Int64);
        }

        public Task<bool> KeyDeleteAsync(RedisKey key, CommandFlags flags = CommandFlags.None)
        {
            var msg = Message.Create(Db, flags, RedisCommand.DEL, key);
            return ExecuteAsync(msg, ResultProcessor.DemandZeroOrOne);
        }

        public Task<long> KeyDeleteAsync(RedisKey[] keys, CommandFlags flags = CommandFlags.None)
        {
            if (keys == null) throw new ArgumentNullException("keys");

            var msg = keys.Length == 0 ? null : Message.Create(Db, flags, RedisCommand.DEL, keys);
            return ExecuteAsync(msg, ResultProcessor.Int64);
        }

        public byte[] KeyDump(RedisKey key, CommandFlags flags = CommandFlags.None)
        {
            var msg = Message.Create(Db, flags, RedisCommand.DUMP, key);
            return ExecuteSync(msg, ResultProcessor.ByteArray);
        }

        public Task<byte[]> KeyDumpAsync(RedisKey key, CommandFlags flags = CommandFlags.None)
        {
            var msg = Message.Create(Db, flags, RedisCommand.DUMP, key);
            return ExecuteAsync(msg, ResultProcessor.ByteArray);
        }

        public bool KeyExists(RedisKey key, CommandFlags flags = CommandFlags.None)
        {
            var msg = Message.Create(Db, flags, RedisCommand.EXISTS, key);
            return ExecuteSync(msg, ResultProcessor.Boolean);
        }

        public Task<bool> KeyExistsAsync(RedisKey key, CommandFlags flags = CommandFlags.None)
        {
            var msg = Message.Create(Db, flags, RedisCommand.EXISTS, key);
            return ExecuteAsync(msg, ResultProcessor.Boolean);
        }

        public bool KeyExpire(RedisKey key, TimeSpan? expiry, CommandFlags flags = CommandFlags.None)
        {
            ServerEndPoint server;
            var msg = GetExpiryMessage(key, flags, expiry, out server);
            return ExecuteSync(msg, ResultProcessor.Boolean, server: server);
        }

        public bool KeyExpire(RedisKey key, DateTime? expiry, CommandFlags flags = CommandFlags.None)
        {
            ServerEndPoint server;
            var msg = GetExpiryMessage(key, flags, expiry, out server);
            return ExecuteSync(msg, ResultProcessor.Boolean, server: server);
        }

        public Task<bool> KeyExpireAsync(RedisKey key, TimeSpan? expiry, CommandFlags flags = CommandFlags.None)
        {
            ServerEndPoint server;
            var msg = GetExpiryMessage(key, flags, expiry, out server);
            return ExecuteAsync(msg, ResultProcessor.Boolean, server: server);
        }

        public Task<bool> KeyExpireAsync(RedisKey key, DateTime? expiry, CommandFlags flags = CommandFlags.None)
        {
            ServerEndPoint server;
            var msg = GetExpiryMessage(key, flags, expiry, out server);
            return ExecuteAsync(msg, ResultProcessor.Boolean, server: server);
        }

        public void KeyMigrate(RedisKey key, EndPoint toServer, int toDatabase = 0, int timeoutMilliseconds = 0, MigrateOptions migrateOptions = MigrateOptions.None, CommandFlags flags = CommandFlags.None)
        {
            if (timeoutMilliseconds <= 0) timeoutMilliseconds = multiplexer.TimeoutMilliseconds;
            var msg = new KeyMigrateCommandMessage(Db, key, toServer, toDatabase, timeoutMilliseconds, migrateOptions, flags);
            ExecuteSync(msg, ResultProcessor.DemandOK);
        }
        public Task KeyMigrateAsync(RedisKey key, EndPoint toServer, int toDatabase = 0, int timeoutMilliseconds = 0, MigrateOptions migrateOptions = MigrateOptions.None, CommandFlags flags = CommandFlags.None)
        {
            if (timeoutMilliseconds <= 0) timeoutMilliseconds = multiplexer.TimeoutMilliseconds;
            var msg = new KeyMigrateCommandMessage(Db, key, toServer, toDatabase, timeoutMilliseconds, migrateOptions, flags);
            return ExecuteAsync(msg, ResultProcessor.DemandOK);
        }

        sealed class KeyMigrateCommandMessage : Message.CommandKeyBase // MIGRATE is atypical
        {
            private MigrateOptions migrateOptions;
            private int timeoutMilliseconds;
            private int toDatabase;
            RedisValue toHost, toPort;

            public KeyMigrateCommandMessage(int db, RedisKey key, EndPoint toServer, int toDatabase, int timeoutMilliseconds, MigrateOptions migrateOptions, CommandFlags flags)
                : base(db, flags, RedisCommand.MIGRATE, key)
            {
                if (toServer == null) throw new ArgumentNullException("server");
                string toHost;
                int toPort;
                if (!Format.TryGetHostPort(toServer, out toHost, out toPort)) throw new ArgumentException("toServer");
                this.toHost = toHost;
                this.toPort = toPort;
                if (toDatabase < 0) throw new ArgumentOutOfRangeException("toDatabase");
                this.toDatabase = toDatabase;
                this.timeoutMilliseconds = timeoutMilliseconds;
                this.migrateOptions = migrateOptions;
            }

            internal override void WriteImpl(PhysicalConnection physical)
            {
                bool isCopy = (migrateOptions & MigrateOptions.Copy) != 0;
                bool isReplace = (migrateOptions & MigrateOptions.Replace) != 0;
                physical.WriteHeader(Command, 5 + (isCopy ? 1 : 0) + (isReplace ? 1 : 0));
                physical.Write(toHost);
                physical.Write(toPort);
                physical.Write(Key);
                physical.Write(toDatabase);
                physical.Write(timeoutMilliseconds);
                if (isCopy) physical.Write(RedisLiterals.COPY);
                if (isReplace) physical.Write(RedisLiterals.REPLACE);
            }
        }

        public bool KeyMove(RedisKey key, int database, CommandFlags flags = CommandFlags.None)
        {
            var msg = Message.Create(Db, flags, RedisCommand.MOVE, key, database);
            return ExecuteSync(msg, ResultProcessor.Boolean);
        }

        public Task<bool> KeyMoveAsync(RedisKey key, int database, CommandFlags flags = CommandFlags.None)
        {
            var msg = Message.Create(Db, flags, RedisCommand.MOVE, key, database);
            return ExecuteAsync(msg, ResultProcessor.Boolean);
        }

        public bool KeyPersist(RedisKey key, CommandFlags flags = CommandFlags.None)
        {
            var msg = Message.Create(Db, flags, RedisCommand.PERSIST, key);
            return ExecuteSync(msg, ResultProcessor.Boolean);
        }

        public Task<bool> KeyPersistAsync(RedisKey key, CommandFlags flags = CommandFlags.None)
        {
            var msg = Message.Create(Db, flags, RedisCommand.PERSIST, key);
            return ExecuteAsync(msg, ResultProcessor.Boolean);
        }

        public RedisKey KeyRandom(CommandFlags flags = CommandFlags.None)
        {
            var msg = Message.Create(Db, flags, RedisCommand.RANDOMKEY);
            return ExecuteSync(msg, ResultProcessor.RedisKey);
        }

        public Task<RedisKey> KeyRandomAsync(CommandFlags flags = CommandFlags.None)
        {
            var msg = Message.Create(Db, flags, RedisCommand.RANDOMKEY);
            return ExecuteAsync(msg, ResultProcessor.RedisKey);
        }

        public bool KeyRename(RedisKey key, RedisKey newKey, When when = When.Always, CommandFlags flags = CommandFlags.None)
        {
            WhenAlwaysOrNotExists(when);
            var msg = Message.Create(Db, flags, when == When.Always ? RedisCommand.RENAME : RedisCommand.RENAMENX, key, newKey);
            return ExecuteSync(msg, ResultProcessor.DemandOK);
        }

        public Task<bool> KeyRenameAsync(RedisKey key, RedisKey newKey, When when = When.Always, CommandFlags flags = CommandFlags.None)
        {
            WhenAlwaysOrNotExists(when);
            var msg = Message.Create(Db, flags, when == When.Always ? RedisCommand.RENAME : RedisCommand.RENAMENX, key, newKey);
            return ExecuteAsync(msg, ResultProcessor.DemandOK);
        }

        public void KeyRestore(RedisKey key, byte[] value, TimeSpan? expiry = null, CommandFlags flags = CommandFlags.None)
        {
            var msg = GetRestoreMessage(key, value, expiry, flags);
            ExecuteSync(msg, ResultProcessor.DemandOK);
        }

        public Task KeyRestoreAsync(RedisKey key, byte[] value, TimeSpan? expiry = null, CommandFlags flags = CommandFlags.None)
        {
            var msg = GetRestoreMessage(key, value, expiry, flags);
            return ExecuteAsync(msg, ResultProcessor.DemandOK);
        }

        public TimeSpan? KeyTimeToLive(RedisKey key, CommandFlags flags = CommandFlags.None)
        {
            ServerEndPoint server;
            var features = GetFeatures(Db, key, flags, out server);
            Message msg;
            if (server != null && features.MillisecondExpiry && multiplexer.CommandMap.IsAvailable(RedisCommand.PTTL))
            {
                msg = Message.Create(Db, flags, RedisCommand.PTTL, key);
                return ExecuteSync(msg, ResultProcessor.TimeSpanFromMilliseconds);
            }
            msg = Message.Create(Db, flags, RedisCommand.TTL, key);
            return ExecuteSync(msg, ResultProcessor.TimeSpanFromSeconds);
        }

        public Task<TimeSpan?> KeyTimeToLiveAsync(RedisKey key, CommandFlags flags = CommandFlags.None)
        {
            ServerEndPoint server;
            var features = GetFeatures(Db, key, flags, out server);
            Message msg;
            if (server != null && features.MillisecondExpiry && multiplexer.CommandMap.IsAvailable(RedisCommand.PTTL))
            {
                msg = Message.Create(Db, flags, RedisCommand.PTTL, key);
                return ExecuteAsync(msg, ResultProcessor.TimeSpanFromMilliseconds);
            }
            msg = Message.Create(Db, flags, RedisCommand.TTL, key);
            return ExecuteAsync(msg, ResultProcessor.TimeSpanFromSeconds);

        }

        public RedisType KeyType(RedisKey key, CommandFlags flags = CommandFlags.None)
        {
            var msg = Message.Create(Db, flags, RedisCommand.TYPE, key);
            return ExecuteSync(msg, ResultProcessor.RedisType);
        }

        public Task<RedisType> KeyTypeAsync(RedisKey key, CommandFlags flags)
        {
            var msg = Message.Create(Db, flags, RedisCommand.TYPE, key);
            return ExecuteAsync(msg, ResultProcessor.RedisType);
        }

        public RedisValue ListGetByIndex(RedisKey key, long index, CommandFlags flags = CommandFlags.None)
        {
            var msg = Message.Create(Db, flags, RedisCommand.LINDEX, key, index);
            return ExecuteSync(msg, ResultProcessor.RedisValue);
        }

        public Task<RedisValue> ListGetByIndexAsync(RedisKey key, long index, CommandFlags flags = CommandFlags.None)
        {
            var msg = Message.Create(Db, flags, RedisCommand.LINDEX, key, index);
            return ExecuteAsync(msg, ResultProcessor.RedisValue);
        }

        public long ListInsertAfter(RedisKey key, RedisValue pivot, RedisValue value, CommandFlags flags = CommandFlags.None)
        {
            var msg = Message.Create(Db, flags, RedisCommand.LINSERT, key, RedisLiterals.AFTER, pivot, value);
            return ExecuteSync(msg, ResultProcessor.Int64);
        }

        public Task<long> ListInsertAfterAsync(RedisKey key, RedisValue pivot, RedisValue value, CommandFlags flags = CommandFlags.None)
        {
            var msg = Message.Create(Db, flags, RedisCommand.LINSERT, key, RedisLiterals.AFTER, pivot, value);
            return ExecuteAsync(msg, ResultProcessor.Int64);
        }

        public long ListInsertBefore(RedisKey key, RedisValue pivot, RedisValue value, CommandFlags flags = CommandFlags.None)
        {
            var msg = Message.Create(Db, flags, RedisCommand.LINSERT, key, RedisLiterals.BEFORE, pivot, value);
            return ExecuteSync(msg, ResultProcessor.Int64);
        }

        public Task<long> ListInsertBeforeAsync(RedisKey key, RedisValue pivot, RedisValue value, CommandFlags flags = CommandFlags.None)
        {
            var msg = Message.Create(Db, flags, RedisCommand.LINSERT, key, RedisLiterals.BEFORE, pivot, value);
            return ExecuteAsync(msg, ResultProcessor.Int64);
        }

        public RedisValue ListLeftPop(RedisKey key, CommandFlags flags = CommandFlags.None)
        {
            var msg = Message.Create(Db, flags, RedisCommand.LPOP, key);
            return ExecuteSync(msg, ResultProcessor.RedisValue);
        }

        public Task<RedisValue> ListLeftPopAsync(RedisKey key, CommandFlags flags = CommandFlags.None)
        {
            var msg = Message.Create(Db, flags, RedisCommand.LPOP, key);
            return ExecuteAsync(msg, ResultProcessor.RedisValue);
        }

        public long ListLeftPush(RedisKey key, RedisValue value, When when = When.Always, CommandFlags flags = CommandFlags.None)
        {
            WhenAlwaysOrExists(when);
            var msg = Message.Create(Db, flags, when == When.Always ? RedisCommand.LPUSH : RedisCommand.LPUSHX, key, value);
            return ExecuteSync(msg, ResultProcessor.Int64);
        }

        public long ListLeftPush(RedisKey key, RedisValue[] values, CommandFlags flags = CommandFlags.None)
        {
            if (values == null) throw new ArgumentNullException("values");
            var msg = values.Length == 0 ? Message.Create(Db, flags, RedisCommand.LLEN, key) : Message.Create(Db, flags, RedisCommand.LPUSH, key, values);
            return ExecuteSync(msg, ResultProcessor.Int64);
        }

        public Task<long> ListLeftPushAsync(RedisKey key, RedisValue value, When when = When.Always, CommandFlags flags = CommandFlags.None)
        {
            WhenAlwaysOrExists(when);
            var msg = Message.Create(Db, flags, when == When.Always ? RedisCommand.LPUSH : RedisCommand.LPUSHX, key, value);
            return ExecuteAsync(msg, ResultProcessor.Int64);
        }

        public Task<long> ListLeftPushAsync(RedisKey key, RedisValue[] values, CommandFlags flags = CommandFlags.None)
        {
            if (values == null) throw new ArgumentNullException("values");
            var msg = values.Length == 0 ? Message.Create(Db, flags, RedisCommand.LLEN, key) : Message.Create(Db, flags, RedisCommand.LPUSH, key, values);
            return ExecuteAsync(msg, ResultProcessor.Int64);
        }

        public long ListLength(RedisKey key, CommandFlags flags = CommandFlags.None)
        {
            var msg = Message.Create(Db, flags, RedisCommand.LLEN, key);
            return ExecuteSync(msg, ResultProcessor.Int64);
        }

        public Task<long> ListLengthAsync(RedisKey key, CommandFlags flags = CommandFlags.None)
        {
            var msg = Message.Create(Db, flags, RedisCommand.LLEN, key);
            return ExecuteAsync(msg, ResultProcessor.Int64);
        }

        public RedisValue[] ListRange(RedisKey key, long start = 0, long stop = -1, CommandFlags flags = CommandFlags.None)
        {
            var msg = Message.Create(Db, flags, RedisCommand.LRANGE, key, start, stop);
            return ExecuteSync(msg, ResultProcessor.RedisValueArray);
        }

        public Task<RedisValue[]> ListRangeAsync(RedisKey key, long start = 0, long stop = -1, CommandFlags flags = CommandFlags.None)
        {
            var msg = Message.Create(Db, flags, RedisCommand.LRANGE, key, start, stop);
            return ExecuteAsync(msg, ResultProcessor.RedisValueArray);
        }

        public long ListRemove(RedisKey key, RedisValue value, long count = 0, CommandFlags flags = CommandFlags.None)
        {
            var msg = Message.Create(Db, flags, RedisCommand.LREM, key, count, value);
            return ExecuteSync(msg, ResultProcessor.Int64);
        }

        public Task<long> ListRemoveAsync(RedisKey key, RedisValue value, long count = 0, CommandFlags flags = CommandFlags.None)
        {
            var msg = Message.Create(Db, flags, RedisCommand.LREM, key, count, value);
            return ExecuteAsync(msg, ResultProcessor.Int64);
        }

        public RedisValue ListRightPop(RedisKey key, CommandFlags flags = CommandFlags.None)
        {
            var msg = Message.Create(Db, flags, RedisCommand.RPOP, key);
            return ExecuteSync(msg, ResultProcessor.RedisValue);
        }

        public Task<RedisValue> ListRightPopAsync(RedisKey key, CommandFlags flags = CommandFlags.None)
        {
            var msg = Message.Create(Db, flags, RedisCommand.RPOP, key);
            return ExecuteAsync(msg, ResultProcessor.RedisValue);
        }

        public RedisValue ListRightPopLeftPush(RedisKey source, RedisKey destination, CommandFlags flags = CommandFlags.None)
        {
            var msg = Message.Create(Db, flags, RedisCommand.RPOPLPUSH, source, destination);
            return ExecuteSync(msg, ResultProcessor.RedisValue);
        }

        public Task<RedisValue> ListRightPopLeftPushAsync(RedisKey source, RedisKey destination, CommandFlags flags = CommandFlags.None)
        {
            var msg = Message.Create(Db, flags, RedisCommand.RPOPLPUSH, source, destination);
            return ExecuteAsync(msg, ResultProcessor.RedisValue);
        }

        public long ListRightPush(RedisKey key, RedisValue value, When when = When.Always, CommandFlags flags = CommandFlags.None)
        {
            WhenAlwaysOrExists(when);
            var msg = Message.Create(Db, flags, when == When.Always ? RedisCommand.RPUSH : RedisCommand.RPUSHX, key, value);
            return ExecuteSync(msg, ResultProcessor.Int64);
        }

        public long ListRightPush(RedisKey key, RedisValue[] values, CommandFlags flags = CommandFlags.None)
        {
            if (values == null) throw new ArgumentNullException("values");
            var msg = values.Length == 0 ? Message.Create(Db, flags, RedisCommand.LLEN, key) : Message.Create(Db, flags, RedisCommand.RPUSH, key, values);
            return ExecuteSync(msg, ResultProcessor.Int64);
        }

        public Task<long> ListRightPushAsync(RedisKey key, RedisValue value, When when = When.Always, CommandFlags flags = CommandFlags.None)
        {
            WhenAlwaysOrExists(when);
            var msg = Message.Create(Db, flags, when == When.Always ? RedisCommand.RPUSH : RedisCommand.RPUSHX, key, value);
            return ExecuteAsync(msg, ResultProcessor.Int64);
        }

        public Task<long> ListRightPushAsync(RedisKey key, RedisValue[] values, CommandFlags flags = CommandFlags.None)
        {
            if (values == null) throw new ArgumentNullException("values");
            var msg = values.Length == 0 ? Message.Create(Db, flags, RedisCommand.LLEN, key) : Message.Create(Db, flags, RedisCommand.RPUSH, key, values);
            return ExecuteAsync(msg, ResultProcessor.Int64);
        }

        public void ListSetByIndex(RedisKey key, long index, RedisValue value, CommandFlags flags = CommandFlags.None)
        {
            var msg = Message.Create(Db, flags, RedisCommand.LSET, key, index, value);
            ExecuteSync(msg, ResultProcessor.DemandOK);
        }

        public Task ListSetByIndexAsync(RedisKey key, long index, RedisValue value, CommandFlags flags = CommandFlags.None)
        {
            var msg = Message.Create(Db, flags, RedisCommand.LSET, key, index, value);
            return ExecuteAsync(msg, ResultProcessor.DemandOK);
        }

        public void ListTrim(RedisKey key, long start, long stop, CommandFlags flags = CommandFlags.None)
        {
            var msg = Message.Create(Db, flags, RedisCommand.LTRIM, key, start, stop);
            ExecuteSync(msg, ResultProcessor.DemandOK);
        }

        public Task ListTrimAsync(RedisKey key, long start, long stop, CommandFlags flags = CommandFlags.None)
        {
            var msg = Message.Create(Db, flags, RedisCommand.LTRIM, key, start, stop);
            return ExecuteAsync(msg, ResultProcessor.DemandOK);
        }

        public bool LockExtend(RedisKey key, RedisValue value, TimeSpan expiry, CommandFlags flags = CommandFlags.None)
        {
            if (value.IsNull) throw new ArgumentNullException("value");
            var tran = GetLockExtendTransaction(key, value, expiry);
            if(tran != null) return tran.Execute(flags);

            // without transactions (twemproxy etc), we can't enforce the "value" part
            return KeyExpire(key, expiry, flags);
        }

        public Task<bool> LockExtendAsync(RedisKey key, RedisValue value, TimeSpan expiry, CommandFlags flags = CommandFlags.None)
        {
            if (value.IsNull) throw new ArgumentNullException("value");
            var tran = GetLockExtendTransaction(key, value, expiry);
            if(tran != null) return tran.ExecuteAsync(flags);

            // without transactions (twemproxy etc), we can't enforce the "value" part
            return KeyExpireAsync(key, expiry, flags);
        }
        
        public RedisValue LockQuery(RedisKey key, CommandFlags flags = CommandFlags.None)
        {
            return StringGet(key, flags);
        }

        public Task<RedisValue> LockQueryAsync(RedisKey key, CommandFlags flags = CommandFlags.None)
        {
            return StringGetAsync(key, flags);
        }

        public bool LockRelease(RedisKey key, RedisValue value, CommandFlags flags = CommandFlags.None)
        {
            if (value.IsNull) throw new ArgumentNullException("value");
            var tran = GetLockReleaseTransaction(key, value);
            if(tran != null) return tran.Execute(flags);

            // without transactions (twemproxy etc), we can't enforce the "value" part
            return KeyDelete(key, flags);
        }

        public Task<bool> LockReleaseAsync(RedisKey key, RedisValue value, CommandFlags flags = CommandFlags.None)
        {
            if (value.IsNull) throw new ArgumentNullException("value");
            var tran = GetLockReleaseTransaction(key, value);
            if(tran != null) return tran.ExecuteAsync(flags);

            // without transactions (twemproxy etc), we can't enforce the "value" part
            return KeyDeleteAsync(key, flags);
        }

        public bool LockTake(RedisKey key, RedisValue value, TimeSpan expiry, CommandFlags flags = CommandFlags.None)
        {
            if (value.IsNull) throw new ArgumentNullException("value");
            return StringSet(key, value, expiry, When.NotExists, flags);
        }

        public Task<bool> LockTakeAsync(RedisKey key, RedisValue value, TimeSpan expiry, CommandFlags flags = CommandFlags.None)
        {
            if (value.IsNull) throw new ArgumentNullException("value");
            return StringSetAsync(key, value, expiry, When.NotExists, flags);
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
        public RedisResult ScriptEvaluate(string script, RedisKey[] keys = null, RedisValue[] values = null, CommandFlags flags = CommandFlags.None)
        {
            var msg = new ScriptEvalMessage(Db, flags, script, keys, values);
            try
            {
                return ExecuteSync(msg, ResultProcessor.ScriptResult);
            } catch(RedisServerException)
            {
                // could be a NOSCRIPT; for a sync call, we can re-issue that without problem
                if(msg.IsScriptUnavailable) return ExecuteSync(msg, ResultProcessor.ScriptResult);
                throw;
            }
        }
        public RedisResult ScriptEvaluate(byte[] hash, RedisKey[] keys = null, RedisValue[] values = null, CommandFlags flags = CommandFlags.None)
        {
            var msg = new ScriptEvalMessage(Db, flags, hash, keys, values);
            return ExecuteSync(msg, ResultProcessor.ScriptResult);
        }

        public Task<RedisResult> ScriptEvaluateAsync(string script, RedisKey[] keys = null, RedisValue[] values = null, CommandFlags flags = CommandFlags.None)
        {
            var msg = new ScriptEvalMessage(Db, flags, script, keys, values);
            return ExecuteAsync(msg, ResultProcessor.ScriptResult);
        }
        public Task<RedisResult> ScriptEvaluateAsync(byte[] hash, RedisKey[] keys = null, RedisValue[] values = null, CommandFlags flags = CommandFlags.None)
        {
            var msg = new ScriptEvalMessage(Db, flags, hash, keys, values);
            return ExecuteAsync(msg, ResultProcessor.ScriptResult);
        }

        public bool SetAdd(RedisKey key, RedisValue value, CommandFlags flags = CommandFlags.None)
        {
            var msg = Message.Create(Db, flags, RedisCommand.SADD, key, value);
            return ExecuteSync(msg, ResultProcessor.Boolean);
        }

        public long SetAdd(RedisKey key, RedisValue[] values, CommandFlags flags = CommandFlags.None)
        {
            var msg = Message.Create(Db, flags, RedisCommand.SADD, key, values);
            return ExecuteSync(msg, ResultProcessor.Int64);
        }

        public Task<bool> SetAddAsync(RedisKey key, RedisValue value, CommandFlags flags = CommandFlags.None)
        {
            var msg = Message.Create(Db, flags, RedisCommand.SADD, key, value);
            return ExecuteAsync(msg, ResultProcessor.Boolean);
        }

        public Task<long> SetAddAsync(RedisKey key, RedisValue[] values, CommandFlags flags = CommandFlags.None)
        {
            var msg = Message.Create(Db, flags, RedisCommand.SADD, key, values);
            return ExecuteAsync(msg, ResultProcessor.Int64);
        }

        public RedisValue[] SetCombine(SetOperation operation, RedisKey first, RedisKey second, CommandFlags flags = CommandFlags.None)
        {
            var msg = Message.Create(Db, flags, SetOperationCommand(operation, false), first, second);
            return ExecuteSync(msg, ResultProcessor.RedisValueArray);
        }

        public RedisValue[] SetCombine(SetOperation operation, RedisKey[] keys, CommandFlags flags = CommandFlags.None)
        {
            var msg = Message.Create(Db, flags, SetOperationCommand(operation, false), keys);
            return ExecuteSync(msg, ResultProcessor.RedisValueArray);
        }

        public long SetCombineAndStore(SetOperation operation, RedisKey destination, RedisKey first, RedisKey second, CommandFlags flags = CommandFlags.None)
        {
            var msg = Message.Create(Db, flags, SetOperationCommand(operation, true), destination, first, second);
            return ExecuteSync(msg, ResultProcessor.Int64);
        }

        public long SetCombineAndStore(SetOperation operation, RedisKey destination, RedisKey[] keys, CommandFlags flags = CommandFlags.None)
        {
            var msg = Message.Create(Db, flags, SetOperationCommand(operation, true), destination, keys);
            return ExecuteSync(msg, ResultProcessor.Int64);
        }

        public Task<long> SetCombineAndStoreAsync(SetOperation operation, RedisKey destination, RedisKey first, RedisKey second, CommandFlags flags = CommandFlags.None)
        {
            var msg = Message.Create(Db, flags, SetOperationCommand(operation, true), destination, first, second);
            return ExecuteAsync(msg, ResultProcessor.Int64);
        }

        public Task<long> SetCombineAndStoreAsync(SetOperation operation, RedisKey destination, RedisKey[] keys, CommandFlags flags = CommandFlags.None)
        {
            var msg = Message.Create(Db, flags, SetOperationCommand(operation, true), destination, keys);
            return ExecuteAsync(msg, ResultProcessor.Int64);
        }

        public Task<RedisValue[]> SetCombineAsync(SetOperation operation, RedisKey first, RedisKey second, CommandFlags flags = CommandFlags.None)
        {
            var msg = Message.Create(Db, flags, SetOperationCommand(operation, false), first, second);
            return ExecuteAsync(msg, ResultProcessor.RedisValueArray);
        }

        public Task<RedisValue[]> SetCombineAsync(SetOperation operation, RedisKey[] keys, CommandFlags flags = CommandFlags.None)
        {
            var msg = Message.Create(Db, flags, SetOperationCommand(operation, false), keys);
            return ExecuteAsync(msg, ResultProcessor.RedisValueArray);
        }

        public bool SetContains(RedisKey key, RedisValue value, CommandFlags flags = CommandFlags.None)
        {
            var msg = Message.Create(Db, flags, RedisCommand.SISMEMBER, key, value);
            return ExecuteSync(msg, ResultProcessor.Boolean);
        }

        public Task<bool> SetContainsAsync(RedisKey key, RedisValue value, CommandFlags flags = CommandFlags.None)
        {
            var msg = Message.Create(Db, flags, RedisCommand.SISMEMBER, key, value);
            return ExecuteAsync(msg, ResultProcessor.Boolean);
        }


        public long SetLength(RedisKey key, CommandFlags flags = CommandFlags.None)
        {
            var msg = Message.Create(Db, flags, RedisCommand.SCARD, key);
            return ExecuteSync(msg, ResultProcessor.Int64);
        }

        public Task<long> SetLengthAsync(RedisKey key, CommandFlags flags = CommandFlags.None)
        {
            var msg = Message.Create(Db, flags, RedisCommand.SCARD, key);
            return ExecuteAsync(msg, ResultProcessor.Int64);
        }

        public RedisValue[] SetMembers(RedisKey key, CommandFlags flags = CommandFlags.None)
        {
            var msg = Message.Create(Db, flags, RedisCommand.SMEMBERS, key);
            return ExecuteSync(msg, ResultProcessor.RedisValueArray);
        }

        public Task<RedisValue[]> SetMembersAsync(RedisKey key, CommandFlags flags = CommandFlags.None)
        {
            var msg = Message.Create(Db, flags, RedisCommand.SMEMBERS, key);
            return ExecuteAsync(msg, ResultProcessor.RedisValueArray);
        }

        public bool SetMove(RedisKey source, RedisKey destination, RedisValue value, CommandFlags flags = CommandFlags.None)
        {
            var msg = Message.Create(Db, flags, RedisCommand.SMOVE, source, destination, value);
            return ExecuteSync(msg, ResultProcessor.Boolean);
        }

        public Task<bool> SetMoveAsync(RedisKey source, RedisKey destination, RedisValue value, CommandFlags flags = CommandFlags.None)
        {
            var msg = Message.Create(Db, flags, RedisCommand.SMOVE, source, destination, value);
            return ExecuteAsync(msg, ResultProcessor.Boolean);
        }

        public RedisValue SetPop(RedisKey key, CommandFlags flags = CommandFlags.None)
        {
            var msg = Message.Create(Db, flags, RedisCommand.SPOP, key);
            return ExecuteSync(msg, ResultProcessor.RedisValue);
        }

        public Task<RedisValue> SetPopAsync(RedisKey key, CommandFlags flags = CommandFlags.None)
        {
            var msg = Message.Create(Db, flags, RedisCommand.SPOP, key);
            return ExecuteAsync(msg, ResultProcessor.RedisValue);
        }

        public RedisValue SetRandomMember(RedisKey key, CommandFlags flags = CommandFlags.None)
        {
            var msg = Message.Create(Db, flags, RedisCommand.SRANDMEMBER, key);
            return ExecuteSync(msg, ResultProcessor.RedisValue);
        }

        public Task<RedisValue> SetRandomMemberAsync(RedisKey key, CommandFlags flags = CommandFlags.None)
        {
            var msg = Message.Create(Db, flags, RedisCommand.SRANDMEMBER, key);
            return ExecuteAsync(msg, ResultProcessor.RedisValue);
        }

        public RedisValue[] SetRandomMembers(RedisKey key, long count, CommandFlags flags = CommandFlags.None)
        {
            var msg = Message.Create(Db, flags, RedisCommand.SRANDMEMBER, key, count);
            return ExecuteSync(msg, ResultProcessor.RedisValueArray);
        }

        public Task<RedisValue[]> SetRandomMembersAsync(RedisKey key, long count, CommandFlags flags = CommandFlags.None)
        {
            var msg = Message.Create(Db, flags, RedisCommand.SRANDMEMBER, key, count);
            return ExecuteAsync(msg, ResultProcessor.RedisValueArray);
        }

        public bool SetRemove(RedisKey key, RedisValue value, CommandFlags flags = CommandFlags.None)
        {
            var msg = Message.Create(Db, flags, RedisCommand.SREM, key, value);
            return ExecuteSync(msg, ResultProcessor.Boolean);
        }

        public long SetRemove(RedisKey key, RedisValue[] values, CommandFlags flags = CommandFlags.None)
        {
            var msg = Message.Create(Db, flags, RedisCommand.SREM, key, values);
            return ExecuteSync(msg, ResultProcessor.Int64);
        }

        public Task<bool> SetRemoveAsync(RedisKey key, RedisValue value, CommandFlags flags = CommandFlags.None)
        {
            var msg = Message.Create(Db, flags, RedisCommand.SREM, key, value);
            return ExecuteAsync(msg, ResultProcessor.Boolean);
        }

        public Task<long> SetRemoveAsync(RedisKey key, RedisValue[] values, CommandFlags flags = CommandFlags.None)
        {
            var msg = Message.Create(Db, flags, RedisCommand.SREM, key, values);
            return ExecuteAsync(msg, ResultProcessor.Int64);
        }

        IEnumerable<RedisValue> IDatabase.SetScan(RedisKey key, RedisValue pattern, int pageSize, CommandFlags flags)
        {
            return SetScan(key, pattern, pageSize, CursorUtils.Origin, 0, flags);
        }

        public IEnumerable<RedisValue> SetScan(RedisKey key, RedisValue pattern = default(RedisValue), int pageSize = CursorUtils.DefaultPageSize, long cursor = CursorUtils.Origin, int pageOffset = 0, CommandFlags flags = CommandFlags.None)
        {
            var scan = TryScan<RedisValue>(key, pattern, pageSize, cursor, pageOffset, flags, RedisCommand.SSCAN, SetScanResultProcessor.Default);
            if (scan != null) return scan;

            if(cursor != 0 || pageOffset != 0) throw ExceptionFactory.NoCursor(RedisCommand.SMEMBERS);
            if (pattern.IsNull) return SetMembers(key, flags);
            throw ExceptionFactory.NotSupported(true, RedisCommand.SSCAN);
        }
        public RedisValue[] Sort(RedisKey key, long skip = 0, long take = -1, Order order = Order.Ascending, SortType sortType = SortType.Numeric, RedisValue by = default(RedisValue), RedisValue[] get = null, CommandFlags flags = CommandFlags.None)
        {
            var msg = GetSortedSetAddMessage(default(RedisKey), key, skip, take, order, sortType, by, get, flags);
            return ExecuteSync(msg, ResultProcessor.RedisValueArray);
        }

        public long SortAndStore(RedisKey destination, RedisKey key, long skip = 0, long take = -1, Order order = Order.Ascending, SortType sortType = SortType.Numeric, RedisValue by = default(RedisValue), RedisValue[] get = null, CommandFlags flags = CommandFlags.None)
        {
            var msg = GetSortedSetAddMessage(destination, key, skip, take, order, sortType, by, get, flags);
            return ExecuteSync(msg, ResultProcessor.Int64);
        }

        public Task<long> SortAndStoreAsync(RedisKey destination, RedisKey key, long skip = 0, long take = -1, Order order = Order.Ascending, SortType sortType = SortType.Numeric, RedisValue by = default(RedisValue), RedisValue[] get = null, CommandFlags flags = CommandFlags.None)
        {
            var msg = GetSortedSetAddMessage(destination, key, skip, take, order, sortType, by, get, flags);
            return ExecuteAsync(msg, ResultProcessor.Int64);
        }

        public Task<RedisValue[]> SortAsync(RedisKey key, long skip = 0, long take = -1, Order order = Order.Ascending, SortType sortType = SortType.Numeric, RedisValue by = default(RedisValue), RedisValue[] get = null, CommandFlags flags = CommandFlags.None)
        {
            var msg = GetSortedSetAddMessage(default(RedisKey), key, skip, take, order, sortType, by, get, flags);
            return ExecuteAsync(msg, ResultProcessor.RedisValueArray);
        }

        public bool SortedSetAdd(RedisKey key, RedisValue member, double score, CommandFlags flags = CommandFlags.None)
        {
            var msg = Message.Create(Db, flags, RedisCommand.ZADD, key, score, member);
            return ExecuteSync(msg, ResultProcessor.Boolean);
        }

        public long SortedSetAdd(RedisKey key, SortedSetEntry[] values, CommandFlags flags = CommandFlags.None)
        {
            var msg = GetSortedSetAddMessage(key, values, flags);
            return ExecuteSync(msg, ResultProcessor.Int64);
        }

        public Task<bool> SortedSetAddAsync(RedisKey key, RedisValue member, double score, CommandFlags flags = CommandFlags.None)
        {
            var msg = Message.Create(Db, flags, RedisCommand.ZADD, key, score, member);
            return ExecuteAsync(msg, ResultProcessor.Boolean);
        }

        public Task<long> SortedSetAddAsync(RedisKey key, SortedSetEntry[] values, CommandFlags flags = CommandFlags.None)
        {
            var msg = GetSortedSetAddMessage(key, values, flags);
            return ExecuteAsync(msg, ResultProcessor.Int64);
        }

        public long SortedSetCombineAndStore(SetOperation operation, RedisKey destination, RedisKey first, RedisKey second, Aggregate aggregate = Aggregate.Sum, CommandFlags flags = CommandFlags.None)
        {
            var msg = GetSortedSetCombineAndStoreCommandMessage(operation, destination, new[] { first, second }, null, aggregate, flags);
            return ExecuteSync(msg, ResultProcessor.Int64);
        }

        public long SortedSetCombineAndStore(SetOperation operation, RedisKey destination, RedisKey[] keys, double[] weights = null, Aggregate aggregate = Aggregate.Sum, CommandFlags flags = CommandFlags.None)
        {
            var msg = GetSortedSetCombineAndStoreCommandMessage(operation, destination, keys, weights, aggregate, flags);
            return ExecuteSync(msg, ResultProcessor.Int64);
        }

        public Task<long> SortedSetCombineAndStoreAsync(SetOperation operation, RedisKey destination, RedisKey first, RedisKey second, Aggregate aggregate = Aggregate.Sum, CommandFlags flags = CommandFlags.None)
        {
            var msg = GetSortedSetCombineAndStoreCommandMessage(operation, destination, new[] { first, second }, null, aggregate, flags);
            return ExecuteAsync(msg, ResultProcessor.Int64);
        }

        public Task<long> SortedSetCombineAndStoreAsync(SetOperation operation, RedisKey destination, RedisKey[] keys, double[] weights = null, Aggregate aggregate = Aggregate.Sum, CommandFlags flags = CommandFlags.None)
        {
            var msg = GetSortedSetCombineAndStoreCommandMessage(operation, destination, keys, weights, aggregate, flags);
            return ExecuteAsync(msg, ResultProcessor.Int64);
        }

        public double SortedSetDecrement(RedisKey key, RedisValue member, double value, CommandFlags flags = CommandFlags.None)
        {
            return SortedSetIncrement(key, member, -value, flags);
        }

        public Task<double> SortedSetDecrementAsync(RedisKey key, RedisValue member, double value, CommandFlags flags = CommandFlags.None)
        {
            return SortedSetIncrementAsync(key, member, -value, flags);
        }

        public double SortedSetIncrement(RedisKey key, RedisValue member, double value, CommandFlags flags = CommandFlags.None)
        {
            var msg = Message.Create(Db, flags, RedisCommand.ZINCRBY, key, value, member);
            return ExecuteSync(msg, ResultProcessor.Double);
        }

        public Task<double> SortedSetIncrementAsync(RedisKey key, RedisValue member, double value, CommandFlags flags = CommandFlags.None)
        {
            var msg = Message.Create(Db, flags, RedisCommand.ZINCRBY, key, value, member);
            return ExecuteAsync(msg, ResultProcessor.Double);
        }

        public long SortedSetLength(RedisKey key, double min = double.NegativeInfinity, double max = double.PositiveInfinity, Exclude exclude = Exclude.None, CommandFlags flags = CommandFlags.None)
        {
            var msg = GetSortedSetLengthMessage(key, min, max, exclude, flags);
            return ExecuteSync(msg, ResultProcessor.Int64);
        }

        public Task<long> SortedSetLengthAsync(RedisKey key, double min = double.NegativeInfinity, double max = double.PositiveInfinity, Exclude exclude = Exclude.None, CommandFlags flags = CommandFlags.None)
        {
            var msg = GetSortedSetLengthMessage(key, min, max, exclude, flags);
            return ExecuteAsync(msg, ResultProcessor.Int64);
        }

        public RedisValue[] SortedSetRangeByRank(RedisKey key, long start = 0, long stop = -1, Order order = Order.Ascending, CommandFlags flags = CommandFlags.None)
        {
            var msg = Message.Create(Db, flags, order == Order.Descending ? RedisCommand.ZREVRANGE : RedisCommand.ZRANGE, key, start, stop);
            return ExecuteSync(msg, ResultProcessor.RedisValueArray);
        }

        public Task<RedisValue[]> SortedSetRangeByRankAsync(RedisKey key, long start = 0, long stop = -1, Order order = Order.Ascending, CommandFlags flags = CommandFlags.None)
        {
            var msg = Message.Create(Db, flags, order == Order.Descending ? RedisCommand.ZREVRANGE : RedisCommand.ZRANGE, key, start, stop);
            return ExecuteAsync(msg, ResultProcessor.RedisValueArray);
        }

        public SortedSetEntry[] SortedSetRangeByRankWithScores(RedisKey key, long start = 0, long stop = -1, Order order = Order.Ascending, CommandFlags flags = CommandFlags.None)
        {
            var msg = Message.Create(Db, flags, order == Order.Descending ? RedisCommand.ZREVRANGE : RedisCommand.ZRANGE, key, start, stop, RedisLiterals.WITHSCORES);
            return ExecuteSync(msg, ResultProcessor.SortedSetWithScores);
        }

        public Task<SortedSetEntry[]> SortedSetRangeByRankWithScoresAsync(RedisKey key, long start = 0, long stop = -1, Order order = Order.Ascending, CommandFlags flags = CommandFlags.None)
        {
            var msg = Message.Create(Db, flags, order == Order.Descending ? RedisCommand.ZREVRANGE : RedisCommand.ZRANGE, key, start, stop, RedisLiterals.WITHSCORES);
            return ExecuteAsync(msg, ResultProcessor.SortedSetWithScores);
        }

        public RedisValue[] SortedSetRangeByScore(RedisKey key, double start = double.NegativeInfinity, double stop = double.PositiveInfinity, Exclude exclude = Exclude.None, Order order = Order.Ascending, long skip = 0, long take = -1, CommandFlags flags = CommandFlags.None)
        {
            var msg = GetSortedSetRangeByScoreMessage(key, start, stop, exclude, order, skip, take, flags, false);
            return ExecuteSync(msg, ResultProcessor.RedisValueArray);
        }

        public Task<RedisValue[]> SortedSetRangeByScoreAsync(RedisKey key, double start = double.NegativeInfinity, double stop = double.PositiveInfinity, Exclude exclude = Exclude.None, Order order = Order.Ascending, long skip = 0, long take = -1, CommandFlags flags = CommandFlags.None)
        {
            var msg = GetSortedSetRangeByScoreMessage(key, start, stop, exclude, order, skip, take, flags, false);
            return ExecuteAsync(msg, ResultProcessor.RedisValueArray);
        }

        public SortedSetEntry[] SortedSetRangeByScoreWithScores(RedisKey key, double start = double.NegativeInfinity, double stop = double.PositiveInfinity, Exclude exclude = Exclude.None, Order order = Order.Ascending, long skip = 0, long take = -1, CommandFlags flags = CommandFlags.None)
        {
            var msg = GetSortedSetRangeByScoreMessage(key, start, stop, exclude, order, skip, take, flags, true);
            return ExecuteSync(msg, ResultProcessor.SortedSetWithScores);
        }

        public Task<SortedSetEntry[]> SortedSetRangeByScoreWithScoresAsync(RedisKey key, double start = double.NegativeInfinity, double stop = double.PositiveInfinity, Exclude exclude = Exclude.None, Order order = Order.Ascending, long skip = 0, long take = -1, CommandFlags flags = CommandFlags.None)
        {
            var msg = GetSortedSetRangeByScoreMessage(key, start, stop, exclude, order, skip, take, flags, true);
            return ExecuteAsync(msg, ResultProcessor.SortedSetWithScores);
        }

        public long? SortedSetRank(RedisKey key, RedisValue member, Order order = Order.Ascending, CommandFlags flags = CommandFlags.None)
        {
            var msg = Message.Create(Db, flags, order == Order.Descending ? RedisCommand.ZREVRANK : RedisCommand.ZRANK, key, member);
            return ExecuteSync(msg, ResultProcessor.NullableInt64);
        }

        public Task<long?> SortedSetRankAsync(RedisKey key, RedisValue member, Order order = Order.Ascending, CommandFlags flags = CommandFlags.None)
        {
            var msg = Message.Create(Db, flags, order == Order.Descending ? RedisCommand.ZREVRANK : RedisCommand.ZRANK, key, member);
            return ExecuteAsync(msg, ResultProcessor.NullableInt64);
        }

        public bool SortedSetRemove(RedisKey key, RedisValue member, CommandFlags flags = CommandFlags.None)
        {
            var msg = Message.Create(Db, flags, RedisCommand.ZREM, key, member);
            return ExecuteSync(msg, ResultProcessor.Boolean);
        }

        public long SortedSetRemove(RedisKey key, RedisValue[] members, CommandFlags flags = CommandFlags.None)
        {
            var msg = Message.Create(Db, flags, RedisCommand.ZREM, key, members);
            return ExecuteSync(msg, ResultProcessor.Int64);
        }

        public Task<bool> SortedSetRemoveAsync(RedisKey key, RedisValue member, CommandFlags flags = CommandFlags.None)
        {
            var msg = Message.Create(Db, flags, RedisCommand.ZREM, key, member);
            return ExecuteAsync(msg, ResultProcessor.Boolean);
        }

        public Task<long> SortedSetRemoveAsync(RedisKey key, RedisValue[] members, CommandFlags flags = CommandFlags.None)
        {
            var msg = Message.Create(Db, flags, RedisCommand.ZREM, key, members);
            return ExecuteAsync(msg, ResultProcessor.Int64);
        }

        public long SortedSetRemoveRangeByRank(RedisKey key, long start, long stop, CommandFlags flags = CommandFlags.None)
        {
            var msg = Message.Create(Db, flags, RedisCommand.ZREMRANGEBYRANK, key, start, stop);
            return ExecuteSync(msg, ResultProcessor.Int64);
        }

        public Task<long> SortedSetRemoveRangeByRankAsync(RedisKey key, long start, long stop, CommandFlags flags = CommandFlags.None)
        {
            var msg = Message.Create(Db, flags, RedisCommand.ZREMRANGEBYRANK, key, start, stop);
            return ExecuteAsync(msg, ResultProcessor.Int64);
        }

        public long SortedSetRemoveRangeByScore(RedisKey key, double start, double stop, Exclude exclude = Exclude.None, CommandFlags flags = CommandFlags.None)
        {
            var msg = GetSortedSetRemoveRangeByScoreMessage(key, start, stop, exclude, flags);
            return ExecuteSync(msg, ResultProcessor.Int64);
        }

        public Task<long> SortedSetRemoveRangeByScoreAsync(RedisKey key, double start, double stop, Exclude exclude = Exclude.None, CommandFlags flags = CommandFlags.None)
        {
            var msg = GetSortedSetRemoveRangeByScoreMessage(key, start, stop, exclude, flags);
            return ExecuteAsync(msg, ResultProcessor.Int64);
        }

        IEnumerable<SortedSetEntry> IDatabase.SortedSetScan(RedisKey key, RedisValue pattern, int pageSize, CommandFlags flags)
        {
            return SortedSetScan(key, pattern, pageSize, CursorUtils.Origin, 0, flags);
        }

        public IEnumerable<SortedSetEntry> SortedSetScan(RedisKey key, RedisValue pattern = default(RedisValue), int pageSize = CursorUtils.DefaultPageSize, long cursor = CursorUtils.Origin, int pageOffset = 0, CommandFlags flags = CommandFlags.None)
        {
            var scan = TryScan<SortedSetEntry>(key, pattern, pageSize, cursor, pageOffset, flags, RedisCommand.ZSCAN, SortedSetScanResultProcessor.Default);
            if (scan != null) return scan;

            if (cursor != 0 || pageOffset != 0) throw ExceptionFactory.NoCursor(RedisCommand.ZRANGE);
            if (pattern.IsNull) return SortedSetRangeByRankWithScores(key, flags: flags);
            throw ExceptionFactory.NotSupported(true, RedisCommand.ZSCAN);
        }

        public double? SortedSetScore(RedisKey key, RedisValue member, CommandFlags flags = CommandFlags.None)
        {
            var msg = Message.Create(Db, flags, RedisCommand.ZSCORE, key, member);
            return ExecuteSync(msg, ResultProcessor.NullableDouble);
        }

        public Task<double?> SortedSetScoreAsync(RedisKey key, RedisValue member, CommandFlags flags = CommandFlags.None)
        {
            var msg = Message.Create(Db, flags, RedisCommand.ZSCORE, key, member);
            return ExecuteAsync(msg, ResultProcessor.NullableDouble);
        }

        public long StringAppend(RedisKey key, RedisValue value, CommandFlags flags = CommandFlags.None)
        {
            var msg = Message.Create(Db, flags, RedisCommand.APPEND, key, value);
            return ExecuteSync(msg, ResultProcessor.Int64);
        }

        public Task<long> StringAppendAsync(RedisKey key, RedisValue value, CommandFlags flags = CommandFlags.None)
        {
            var msg = Message.Create(Db, flags, RedisCommand.APPEND, key, value);
            return ExecuteAsync(msg, ResultProcessor.Int64);
        }

        public long StringBitCount(RedisKey key, long start = 0, long end = -1, CommandFlags flags = CommandFlags.None)
        {
            var msg = Message.Create(Db, flags, RedisCommand.BITCOUNT, key, start, end);
            return ExecuteSync(msg, ResultProcessor.Int64);
        }

        public Task<long> StringBitCountAsync(RedisKey key, long start = 0, long end = -1, CommandFlags flags = CommandFlags.None)
        {
            var msg = Message.Create(Db, flags, RedisCommand.BITCOUNT, key, start, end);
            return ExecuteAsync(msg, ResultProcessor.Int64);
        }

        public long StringBitOperation(Bitwise operation, RedisKey destination, RedisKey first, RedisKey second, CommandFlags flags = CommandFlags.None)
        {
            var msg = GetStringBitOperationMessage(operation, destination, first, second, flags);
            return ExecuteSync(msg, ResultProcessor.Int64);
        }

        public long StringBitOperation(Bitwise operation, RedisKey destination, RedisKey[] keys, CommandFlags flags = CommandFlags.None)
        {
            var msg = GetStringBitOperationMessage(operation, destination, keys, flags);
            return ExecuteSync(msg, ResultProcessor.Int64);
        }

        public Task<long> StringBitOperationAsync(Bitwise operation, RedisKey destination, RedisKey first, RedisKey second, CommandFlags flags = CommandFlags.None)
        {
            var msg = GetStringBitOperationMessage(operation, destination, first, second, flags);
            return ExecuteAsync(msg, ResultProcessor.Int64);
        }

        public Task<long> StringBitOperationAsync(Bitwise operation, RedisKey destination, RedisKey[] keys, CommandFlags flags = CommandFlags.None)
        {
            var msg = GetStringBitOperationMessage(operation, destination, keys, flags);
            return ExecuteAsync(msg, ResultProcessor.Int64);
        }

        public long StringBitPosition(RedisKey key, bool value, long start = 0, long end = -1, CommandFlags flags = CommandFlags.None)
        {
            var msg = Message.Create(Db, flags, RedisCommand.BITPOS, key, value, start, end);
            return ExecuteSync(msg, ResultProcessor.Int64);
        }

        public Task<long> StringBitPositionAsync(RedisKey key, bool value, long start = 0, long end = -1, CommandFlags flags = CommandFlags.None)
        {
            var msg = Message.Create(Db, flags, RedisCommand.BITPOS, key, value, start, end);
            return ExecuteAsync(msg, ResultProcessor.Int64);
        }

        public long StringDecrement(RedisKey key, long value = 1, CommandFlags flags = CommandFlags.None)
        {
            return StringIncrement(key, -value, flags);
        }

        public double StringDecrement(RedisKey key, double value, CommandFlags flags = CommandFlags.None)
        {
            return StringIncrement(key, -value, flags);
        }

        public Task<long> StringDecrementAsync(RedisKey key, long value = 1, CommandFlags flags = CommandFlags.None)
        {
            return StringIncrementAsync(key, -value, flags);
        }

        public Task<double> StringDecrementAsync(RedisKey key, double value, CommandFlags flags = CommandFlags.None)
        {
            return StringIncrementAsync(key, -value, flags);
        }

        public RedisValue StringGet(RedisKey key, CommandFlags flags = CommandFlags.None)
        {
            var msg = Message.Create(Db, flags, RedisCommand.GET, key);
            return ExecuteSync(msg, ResultProcessor.RedisValue);
        }

        public RedisValue[] StringGet(RedisKey[] keys, CommandFlags flags = CommandFlags.None)
        {
            var msg = Message.Create(Db, flags, RedisCommand.MGET, keys);
            return ExecuteSync(msg, ResultProcessor.RedisValueArray);
        }

        public Task<RedisValue> StringGetAsync(RedisKey key, CommandFlags flags = CommandFlags.None)
        {
            var msg = Message.Create(Db, flags, RedisCommand.GET, key);
            return ExecuteAsync(msg, ResultProcessor.RedisValue);
        }

        public Task<RedisValue[]> StringGetAsync(RedisKey[] keys, CommandFlags flags = CommandFlags.None)
        {
            var msg = Message.Create(Db, flags, RedisCommand.MGET, keys);
            return ExecuteAsync(msg, ResultProcessor.RedisValueArray);
        }

        public bool StringGetBit(RedisKey key, long offset, CommandFlags flags = CommandFlags.None)
        {
            var msg = Message.Create(Db, flags, RedisCommand.GETBIT, key, offset);
            return ExecuteSync(msg, ResultProcessor.Boolean);
        }

        public Task<bool> StringGetBitAsync(RedisKey key, long offset, CommandFlags flags = CommandFlags.None)
        {
            var msg = Message.Create(Db, flags, RedisCommand.GETBIT, key, offset);
            return ExecuteAsync(msg, ResultProcessor.Boolean);
        }

        public RedisValue StringGetRange(RedisKey key, long start, long end, CommandFlags flags = CommandFlags.None)
        {
            var msg = Message.Create(Db, flags, RedisCommand.GETRANGE, key, start, end);
            return ExecuteSync(msg, ResultProcessor.RedisValue);
        }

        public Task<RedisValue> StringGetRangeAsync(RedisKey key, long start, long end, CommandFlags flags = CommandFlags.None)
        {
            var msg = Message.Create(Db, flags, RedisCommand.GETRANGE, key, start, end);
            return ExecuteAsync(msg, ResultProcessor.RedisValue);
        }

        public RedisValue StringGetSet(RedisKey key, RedisValue value, CommandFlags flags = CommandFlags.None)
        {
            var msg = Message.Create(Db, flags, RedisCommand.GETSET, key, value);
            return ExecuteSync(msg, ResultProcessor.RedisValue);
        }

        public Task<RedisValue> StringGetSetAsync(RedisKey key, RedisValue value, CommandFlags flags = CommandFlags.None)
        {
            var msg = Message.Create(Db, flags, RedisCommand.GETSET, key, value);
            return ExecuteAsync(msg, ResultProcessor.RedisValue);
        }

        public RedisValueWithExpiry StringGetWithExpiry(RedisKey key, CommandFlags flags = CommandFlags.None)
        {
            ServerEndPoint server;
            ResultProcessor<RedisValueWithExpiry> processor;
            var msg = GetStringGetWithExpiryMessage(key, flags, out processor, out server);
            return ExecuteSync(msg, processor, server);
        }

        public Task<RedisValueWithExpiry> StringGetWithExpiryAsync(RedisKey key, CommandFlags flags = CommandFlags.None)
        {
            ServerEndPoint server;
            ResultProcessor<RedisValueWithExpiry> processor;
            var msg = GetStringGetWithExpiryMessage(key, flags, out processor, out server);
            return ExecuteAsync(msg, processor, server);
        }

        public long StringIncrement(RedisKey key, long value = 1, CommandFlags flags = CommandFlags.None)
        {
            var msg = IncrMessage(key, value, flags);
            return ExecuteSync(msg, ResultProcessor.Int64);
        }

        public double StringIncrement(RedisKey key, double value, CommandFlags flags = CommandFlags.None)
        {
            var msg = value == 0 && (flags & CommandFlags.FireAndForget) != 0
                ? null : Message.Create(Db, flags, RedisCommand.INCRBYFLOAT, key, value);
            return ExecuteSync(msg, ResultProcessor.Double);
        }

        public Task<long> StringIncrementAsync(RedisKey key, long value = 1, CommandFlags flags = CommandFlags.None)
        {
            var msg = IncrMessage(key, value, flags);
            return ExecuteAsync(msg, ResultProcessor.Int64);
        }

        public Task<double> StringIncrementAsync(RedisKey key, double value, CommandFlags flags = CommandFlags.None)
        {
            var msg = value == 0 && (flags & CommandFlags.FireAndForget) != 0
                ? null : Message.Create(Db, flags, RedisCommand.INCRBYFLOAT, key, value);
            return ExecuteAsync(msg, ResultProcessor.Double);
        }

        public long StringLength(RedisKey key, CommandFlags flags = CommandFlags.None)
        {
            var msg = Message.Create(Db, flags, RedisCommand.STRLEN, key);
            return ExecuteSync(msg, ResultProcessor.Int64);
        }

        public Task<long> StringLengthAsync(RedisKey key, CommandFlags flags = CommandFlags.None)
        {
            var msg = Message.Create(Db, flags, RedisCommand.STRLEN, key);
            return ExecuteAsync(msg, ResultProcessor.Int64);
        }

        public bool StringSet(RedisKey key, RedisValue value, TimeSpan? expiry = null, When when = When.Always, CommandFlags flags = CommandFlags.None)
        {
            var msg = GetStringSetMessage(key, value, expiry, when, flags);
            return ExecuteSync(msg, ResultProcessor.Boolean);
        }

        public bool StringSet(KeyValuePair<RedisKey, RedisValue>[] values, When when = When.Always, CommandFlags flags = CommandFlags.None)
        {
            var msg = GetStringSetMessage(values, when, flags);
            return ExecuteSync(msg, ResultProcessor.Boolean);
        }

        public Task<bool> StringSetAsync(RedisKey key, RedisValue value, TimeSpan? expiry = null, When when = When.Always, CommandFlags flags = CommandFlags.None)
        {
            var msg = GetStringSetMessage(key, value, expiry, when, flags);
            return ExecuteAsync(msg, ResultProcessor.Boolean);
        }

        public Task<bool> StringSetAsync(KeyValuePair<RedisKey, RedisValue>[] values, When when = When.Always, CommandFlags flags = CommandFlags.None)
        {
            var msg = GetStringSetMessage(values, when, flags);
            return ExecuteAsync(msg, ResultProcessor.Boolean);
        }

        public bool StringSetBit(RedisKey key, long offset, bool value, CommandFlags flags = CommandFlags.None)
        {
            var msg = Message.Create(Db, flags, RedisCommand.SETBIT, key, offset, value);
            return ExecuteSync(msg, ResultProcessor.Boolean);
        }

        public Task<bool> StringSetBitAsync(RedisKey key, long offset, bool value, CommandFlags flags = CommandFlags.None)
        {
            var msg = Message.Create(Db, flags, RedisCommand.SETBIT, key, offset, value);
            return ExecuteAsync(msg, ResultProcessor.Boolean);
        }

        public RedisValue StringSetRange(RedisKey key, long offset, RedisValue value, CommandFlags flags = CommandFlags.None)
        {
            var msg = Message.Create(Db, flags, RedisCommand.SETRANGE, key, offset, value);
            return ExecuteSync(msg, ResultProcessor.RedisValue);
        }

        public Task<RedisValue> StringSetRangeAsync(RedisKey key, long offset, RedisValue value, CommandFlags flags = CommandFlags.None)
        {
            var msg = Message.Create(Db, flags, RedisCommand.SETRANGE, key, offset, value);
            return ExecuteAsync(msg, ResultProcessor.RedisValue);
        }

        Message GetExpiryMessage(RedisKey key, CommandFlags flags, TimeSpan? expiry, out ServerEndPoint server)
        {
            TimeSpan duration;
            if (expiry == null || (duration = expiry.Value) == TimeSpan.MaxValue)
            {
                server = null;
                return Message.Create(Db, flags, RedisCommand.PERSIST, key);
            }
            long milliseconds = duration.Ticks / TimeSpan.TicksPerMillisecond;
            if ((milliseconds % 1000) != 0)
            {
                var features = GetFeatures(Db, key, flags, out server);
                if (server != null && features.MillisecondExpiry && multiplexer.CommandMap.IsAvailable(RedisCommand.PEXPIRE))
                {
                    return Message.Create(Db, flags, RedisCommand.PEXPIRE, key, milliseconds);
                }
            }
            server = null;
            long seconds = milliseconds / 1000;
            return Message.Create(Db, flags, RedisCommand.EXPIRE, key, seconds);
        }

        Message GetExpiryMessage(RedisKey key, CommandFlags flags, DateTime? expiry, out ServerEndPoint server)
        {
            DateTime when;
            if (expiry == null || (when = expiry.Value) == DateTime.MaxValue)
            {
                server = null;
                return Message.Create(Db, flags, RedisCommand.PERSIST, key);
            }
            switch (when.Kind)
            {
                case DateTimeKind.Local:
                case DateTimeKind.Utc:
                    break; // fine, we can work with that
                default:
                    throw new ArgumentException("Expiry time must be either Utc or Local", "expiry");
            }
            long milliseconds = (when - RedisBase.UnixEpoch).Ticks / TimeSpan.TicksPerMillisecond;

            if ((milliseconds % 1000) != 0)
            {
                var features = GetFeatures(Db, key, flags, out server);
                if (server != null && features.MillisecondExpiry && multiplexer.CommandMap.IsAvailable(RedisCommand.PEXPIREAT))
                {
                    return Message.Create(Db, flags, RedisCommand.PEXPIREAT, key, milliseconds);
                }
            }
            server = null;
            long seconds = milliseconds / 1000;
            return Message.Create(Db, flags, RedisCommand.EXPIREAT, key, seconds);
        }

        private Message GetHashSetMessage(RedisKey key, HashEntry[] hashFields, CommandFlags flags)
        {
            if (hashFields == null) throw new ArgumentNullException("hashFields");
            switch (hashFields.Length)
            {
                case 0: return null;
                case 1: return Message.Create(Db, flags, RedisCommand.HMSET, key,
                        hashFields[0].name, hashFields[0].value);
                case 2:
                    return Message.Create(Db, flags, RedisCommand.HMSET, key,
                        hashFields[0].name, hashFields[0].value,
                        hashFields[1].name, hashFields[1].value);
                default:
                    var arr = new RedisValue[hashFields.Length * 2];
                    int offset = 0;
                    for (int i = 0; i < hashFields.Length; i++)
                    {
                        arr[offset++] = hashFields[i].name;
                        arr[offset++] = hashFields[i].value;
                    }
                    return Message.Create(Db, flags, RedisCommand.HMSET, key, arr);
            }
        }

        ITransaction GetLockExtendTransaction(RedisKey key, RedisValue value, TimeSpan expiry)
        {
            var tran = CreateTransactionIfAvailable(asyncState);
            if (tran != null)
            {
                tran.AddCondition(Condition.StringEqual(key, value));
                tran.KeyExpireAsync(key, expiry, CommandFlags.FireAndForget);
            }
            return tran;
        }

        ITransaction GetLockReleaseTransaction(RedisKey key, RedisValue value)
        {
            var tran = CreateTransactionIfAvailable(asyncState);
            if (tran != null)
            {
                tran.AddCondition(Condition.StringEqual(key, value));
                tran.KeyDeleteAsync(key, CommandFlags.FireAndForget);
            }
            return tran;
        }

        private RedisValue GetLexRange(RedisValue value, Exclude exclude, bool isStart)
        {
            if(value.IsNull)
            {
                return isStart? RedisLiterals.MinusSymbol : RedisLiterals.PlusSumbol;
            }
            byte[] orig = value;

            byte[] result = new byte[orig.Length + 1];
            // no defaults here; must always explicitly specify [ / (
            result[0] = (exclude & (isStart ? Exclude.Start : Exclude.Stop)) == 0 ? (byte)'[' : (byte)'(';
            Buffer.BlockCopy(orig, 0, result, 1, orig.Length);            
            return result;
        }
        private RedisValue GetRange(double value, Exclude exclude, bool isStart)
        {
            if (isStart)
            {
                if ((exclude & Exclude.Start) == 0) return value; // inclusive is default
            }
            else
            {
                if ((exclude & Exclude.Stop) == 0) return value; // inclusive is default
            }
            return "(" + Format.ToString(value); // '(' prefix means exclusive
        }

        Message GetRestoreMessage(RedisKey key, byte[] value, TimeSpan? expiry, CommandFlags flags)
        {
            long pttl = (expiry == null || expiry.Value == TimeSpan.MaxValue) ? 0 : (expiry.Value.Ticks / TimeSpan.TicksPerMillisecond);
            return Message.Create(Db, flags, RedisCommand.RESTORE, key, pttl, value);
        }

        private Message GetSortedSetAddMessage(RedisKey key, SortedSetEntry[] values, CommandFlags flags)
        {
            if (values == null) throw new ArgumentNullException("values");
            switch (values.Length)
            {
                case 0: return null;
                case 1:
                    return Message.Create(Db, flags, RedisCommand.ZADD, key,
                        values[0].score, values[0].element);
                case 2:
                    return Message.Create(Db, flags, RedisCommand.ZADD, key,
                        values[0].score, values[0].element,
                        values[1].score, values[1].element);
                default:
                    var arr = new RedisValue[values.Length * 2];
                    int index = 0;
                    for (int i = 0; i < values.Length; i++)
                    {
                        arr[index++] = values[i].score;
                        arr[index++] = values[i].element;
                    }
                    return Message.Create(Db, flags, RedisCommand.ZADD, key, arr);
            }
        }

        private Message GetSortedSetAddMessage(RedisKey destination, RedisKey key, long skip, long take, Order order, SortType sortType, RedisValue by, RedisValue[] get, CommandFlags flags)
        {
            // most common cases; no "get", no "by", no "destination", no "skip", no "take"
            if (destination.IsNull && skip == 0 && take == -1 && by.IsNull && (get == null || get.Length == 0))
            {
                switch (order)
                {
                    case Order.Ascending:
                        switch (sortType)
                        {
                            case SortType.Numeric: return Message.Create(Db, flags, RedisCommand.SORT, key);
                            case SortType.Alphabetic: return Message.Create(Db, flags, RedisCommand.SORT, key, RedisLiterals.ALPHA);
                        }
                        break;
                    case Order.Descending:
                        switch (sortType)
                        {
                            case SortType.Numeric: return Message.Create(Db, flags, RedisCommand.SORT, key, RedisLiterals.DESC);
                            case SortType.Alphabetic: return Message.Create(Db, flags, RedisCommand.SORT, key, RedisLiterals.DESC, RedisLiterals.ALPHA);
                        }
                        break;
                }
            }

            // and now: more complicated scenarios...
            List<RedisValue> values = new List<RedisValue>();
            if (!by.IsNull)
            {
                values.Add(RedisLiterals.BY);
                values.Add(by);
            }
            if (skip != 0 || take != -1)// these are our defaults that mean "everything"; anything else needs to be sent explicitly
            {
                values.Add(RedisLiterals.LIMIT);
                values.Add(skip);
                values.Add(take);
            }
            switch (order)
            {
                case Order.Ascending:
                    break; // default
                case Order.Descending:
                    values.Add(RedisLiterals.DESC);
                    break;
                default:
                    throw new ArgumentOutOfRangeException("order");
            }
            switch (sortType)
            {
                case SortType.Numeric:
                    break; // default
                case SortType.Alphabetic:
                    values.Add(RedisLiterals.ALPHA);
                    break;
                default:
                    throw new ArgumentOutOfRangeException("sortType");
            }
            if (get != null && get.Length != 0)
            {
                foreach (var item in get)
                {
                    values.Add(RedisLiterals.GET);
                    values.Add(item);
                }
            }
            if (destination.IsNull) return Message.Create(Db, flags, RedisCommand.SORT, key, values.ToArray());

            // because we are using STORE, we need to push this to a master
            if (Message.GetMasterSlaveFlags(flags) == CommandFlags.DemandSlave)
            {
                throw ExceptionFactory.MasterOnly(multiplexer.IncludeDetailInExceptions, RedisCommand.SORT, null, null);
            }
            flags = Message.SetMasterSlaveFlags(flags, CommandFlags.DemandMaster);
            values.Add(RedisLiterals.STORE);
            return Message.Create(Db, flags, RedisCommand.SORT, key, values.ToArray(), destination);
        }

        private Message GetSortedSetCombineAndStoreCommandMessage(SetOperation operation, RedisKey destination, RedisKey[] keys, double[] weights, Aggregate aggregate, CommandFlags flags)
        {
            RedisCommand command;
            switch (operation)
            {
                case SetOperation.Intersect: command = RedisCommand.ZINTERSTORE; break;
                case SetOperation.Union: command = RedisCommand.ZUNIONSTORE; break;
                default: throw new ArgumentOutOfRangeException("operation");
            }
            if (keys == null) throw new ArgumentNullException("keys");

            List<RedisValue> values = null;
            if (weights != null && weights.Length != 0)
            {
                (values ?? (values = new List<RedisValue>())).Add(RedisLiterals.WEIGHTS);
                foreach (var weight in weights)
                    values.Add(weight);
            }
            switch (aggregate)
            {
                case Aggregate.Sum: break; // default
                case Aggregate.Min:
                    (values ?? (values = new List<RedisValue>())).Add(RedisLiterals.AGGREGATE);
                    values.Add(RedisLiterals.MIN);
                    break;
                case Aggregate.Max:
                    (values ?? (values = new List<RedisValue>())).Add(RedisLiterals.AGGREGATE);
                    values.Add(RedisLiterals.MAX);
                    break;
                default:
                    throw new ArgumentOutOfRangeException("aggregate");
            }
            return new SortedSetCombineAndStoreCommandMessage(Db, flags, command, destination, keys, values == null ? RedisValue.EmptyArray : values.ToArray());

        }

        private Message GetSortedSetLengthMessage(RedisKey key, double min, double max, Exclude exclude, CommandFlags flags)
        {
            if (double.IsNegativeInfinity(min) && double.IsPositiveInfinity(max))
                return Message.Create(Db, flags, RedisCommand.ZCARD, key);

            var from = GetRange(min, exclude, true);
            var to = GetRange(max, exclude, false);
            return Message.Create(Db, flags, RedisCommand.ZCOUNT, key, from, to);
        }

        private Message GetSortedSetRangeByScoreMessage(RedisKey key, double start, double stop, Exclude exclude, Order order, long skip, long take, CommandFlags flags, bool withScores)
        {
            // usage: {ZRANGEBYSCORE|ZREVRANGEBYSCORE} key from to [WITHSCORES] [LIMIT offset count]
            // there's basically only 4 layouts; with/without each of scores/limit
            var command = order == Order.Descending ? RedisCommand.ZREVRANGEBYSCORE : RedisCommand.ZRANGEBYSCORE;
            bool unlimited = skip == 0 && take == -1; // these are our defaults that mean "everything"; anything else needs to be sent explicitly

            bool reverseLimits = (order == Order.Ascending) == (start > stop);
            if (reverseLimits)
            {
                var tmp = start;
                start = stop;
                stop = tmp;
                switch(exclude)
                {
                    case Exclude.Start: exclude = Exclude.Stop; break;
                    case Exclude.Stop: exclude = Exclude.Start; break;
                }
            }

            RedisValue from = GetRange(start, exclude, true), to = GetRange(stop, exclude, false);
            if (withScores)
            {
                return unlimited ? Message.Create(Db, flags, command, key, from, to, RedisLiterals.WITHSCORES)
                    : Message.Create(Db, flags, command, key, new[] { from, to, RedisLiterals.WITHSCORES, RedisLiterals.LIMIT, skip, take });
            }
            else
            {
                return unlimited ? Message.Create(Db, flags, command, key, from, to)
                    : Message.Create(Db, flags, command, key, new[] { from, to, RedisLiterals.LIMIT, skip, take });
            }
        }

        private Message GetSortedSetRemoveRangeByScoreMessage(RedisKey key, double start, double stop, Exclude exclude, CommandFlags flags)
        {
            return Message.Create(Db, flags, RedisCommand.ZREMRANGEBYSCORE, key,
                    GetRange(start, exclude, true), GetRange(stop, exclude, false));
        }

        private Message GetStringBitOperationMessage(Bitwise operation, RedisKey destination, RedisKey[] keys, CommandFlags flags)
        {
            if (keys == null) throw new ArgumentNullException("keys");
            if (keys.Length == 0) return null;

            // these ones are too bespoke to warrant custom Message implementations
            var serverSelectionStrategy = multiplexer.ServerSelectionStrategy;
            int slot = serverSelectionStrategy.HashSlot(destination);
            var values = new RedisValue[keys.Length + 2];
            values[0] = RedisLiterals.Get(operation);
            values[1] = destination.AsRedisValue();
            for (int i = 0; i < keys.Length; i++)
            {
                values[i + 2] = keys[i].AsRedisValue();
                slot = serverSelectionStrategy.CombineSlot(slot, keys[i]);
            }
            return Message.CreateInSlot(Db, slot, flags, RedisCommand.BITOP, values);
        }

        private Message GetStringBitOperationMessage(Bitwise operation, RedisKey destination, RedisKey first, RedisKey second, CommandFlags flags)
        {
            // these ones are too bespoke to warrant custom Message implementations
            var op = RedisLiterals.Get(operation);
            var serverSelectionStrategy = multiplexer.ServerSelectionStrategy;
            int slot = serverSelectionStrategy.HashSlot(destination);
            slot = serverSelectionStrategy.CombineSlot(slot, first);
            if (second.IsNull || operation == Bitwise.Not)
            { // unary
                return Message.CreateInSlot(Db, slot, flags, RedisCommand.BITOP, new[] { op, destination.AsRedisValue(), first.AsRedisValue() });
            }
            // binary
            slot = serverSelectionStrategy.CombineSlot(slot, second);
            return Message.CreateInSlot(Db, slot, flags, RedisCommand.BITOP, new[] { op, destination.AsRedisValue(), first.AsRedisValue(), second.AsRedisValue() });
        }

        Message GetStringGetWithExpiryMessage(RedisKey key, CommandFlags flags, out ResultProcessor<RedisValueWithExpiry> processor, out ServerEndPoint server)
        {
            if (this is IBatch)
            {
                throw new NotSupportedException("This operation is not possible inside a transaction or batch; please issue separate GetString and KeyTimeToLive requests");
            }
            var features = GetFeatures(Db, key, flags, out server);
            processor = StringGetWithExpiryProcessor.Default;
            if (server != null && features.MillisecondExpiry && multiplexer.CommandMap.IsAvailable(RedisCommand.PTTL))
            {
                return new StringGetWithExpiryMessage(Db, flags, RedisCommand.PTTL, key);
            }
            // if we use TTL, it doesn't matter which server
            server = null;
            return new StringGetWithExpiryMessage(Db, flags, RedisCommand.TTL, key);
        }

        private Message GetStringSetMessage(KeyValuePair<RedisKey, RedisValue>[] values, When when = When.Always, CommandFlags flags = CommandFlags.None)
        {
            if (values == null) throw new ArgumentNullException("values");
            switch (values.Length)
            {
                case 0: return null;
                case 1: return GetStringSetMessage(values[0].Key, values[0].Value, null, when, flags);
                default:
                    WhenAlwaysOrNotExists(when);
                    int slot = ServerSelectionStrategy.NoSlot, offset = 0;
                    var args = new RedisValue[values.Length * 2];
                    var serverSelectionStrategy = multiplexer.ServerSelectionStrategy;
                    for (int i = 0; i < values.Length; i++)
                    {
                        args[offset++] = values[i].Key.AsRedisValue();
                        args[offset++] = values[i].Value;
                        slot = serverSelectionStrategy.CombineSlot(slot, values[i].Key);
                    }
                    return Message.CreateInSlot(Db, slot, flags, when == When.NotExists ? RedisCommand.MSETNX : RedisCommand.MSET, args);
            }
        }

        Message GetStringSetMessage(RedisKey key, RedisValue value, TimeSpan? expiry = null, When when = When.Always, CommandFlags flags = CommandFlags.None)
        {
            WhenAlwaysOrExistsOrNotExists(when);
            if (value.IsNull) return Message.Create(Db, flags, RedisCommand.DEL, key);

            if (expiry == null || expiry.Value == TimeSpan.MaxValue)
            { // no expiry
                switch (when)
                {
                    case When.Always: return Message.Create(Db, flags, RedisCommand.SET, key, value);
                    case When.NotExists: return Message.Create(Db, flags, RedisCommand.SETNX, key, value);
                    case When.Exists: return Message.Create(Db, flags, RedisCommand.SET, key, value, RedisLiterals.XX);
                }
            }
            long milliseconds = expiry.Value.Ticks / TimeSpan.TicksPerMillisecond;

            if ((milliseconds % 1000) == 0)
            {
                // a nice round number of seconds
                long seconds = milliseconds / 1000;
                switch (when)
                {
                    case When.Always: return Message.Create(Db, flags, RedisCommand.SETEX, key, seconds, value);
                    case When.Exists: return Message.Create(Db, flags, RedisCommand.SET, key, value, RedisLiterals.EX, seconds, RedisLiterals.XX);
                    case When.NotExists: return Message.Create(Db, flags, RedisCommand.SET, key, value, RedisLiterals.EX, seconds, RedisLiterals.NX);
                }
            }

            switch (when)
            {
                case When.Always: return Message.Create(Db, flags, RedisCommand.PSETEX, key, milliseconds, value);
                case When.Exists: return Message.Create(Db, flags, RedisCommand.SET, key, value, RedisLiterals.PX, milliseconds, RedisLiterals.XX);
                case When.NotExists: return Message.Create(Db, flags, RedisCommand.SET, key, value, RedisLiterals.PX, milliseconds, RedisLiterals.NX);
            }
            throw new NotSupportedException();
        }

        Message IncrMessage(RedisKey key, long value, CommandFlags flags)
        {
            switch (value)
            {
                case 0:
                    if ((flags & CommandFlags.FireAndForget) != 0) return null;
                    return Message.Create(Db, flags, RedisCommand.INCRBY, key, value);
                case 1:
                    return Message.Create(Db, flags, RedisCommand.INCR, key);
                case -1:
                    return Message.Create(Db, flags, RedisCommand.DECR, key);
                default:
                    return value > 0
                        ? Message.Create(Db, flags, RedisCommand.INCRBY, key, value)
                        : Message.Create(Db, flags, RedisCommand.DECRBY, key, -value);
            }
        }

        private RedisCommand SetOperationCommand(SetOperation operation, bool store)
        {
            switch (operation)
            {
                case SetOperation.Difference: return store ? RedisCommand.SDIFFSTORE : RedisCommand.SDIFF;
                case SetOperation.Intersect: return store ? RedisCommand.SINTERSTORE : RedisCommand.SINTER;
                case SetOperation.Union: return store ? RedisCommand.SUNIONSTORE : RedisCommand.SUNION;
                default: throw new ArgumentOutOfRangeException("operation");
            }
        }

        private IEnumerable<T> TryScan<T>(RedisKey key, RedisValue pattern, int pageSize, long cursor, int pageOffset, CommandFlags flags, RedisCommand command, ResultProcessor<ScanIterator<T>.ScanResult> processor)
        {
            if (pageSize <= 0) throw new ArgumentOutOfRangeException("pageSize");
            if (!multiplexer.CommandMap.IsAvailable(command)) return null;

            ServerEndPoint server;
            var features = GetFeatures(Db, key, flags, out server);
            if (!features.Scan) return null;

            if (CursorUtils.IsNil(pattern)) pattern = (byte[])null;
            return new ScanIterator<T>(this, server, key, pattern, pageSize, cursor, pageOffset, flags, command, processor);
        }

        private Message GetLexMessage(RedisCommand command, RedisKey key, RedisValue min, RedisValue max, Exclude exclude, long skip, long take, CommandFlags flags)
        {
            RedisValue start = GetLexRange(min, exclude, true), stop = GetLexRange(max, exclude, false);

            if (skip == 0 && take == -1)
                return Message.Create(Db, flags, command, key, start, stop);

            return Message.Create(Db, flags, command, key, new[] { start, stop, RedisLiterals.LIMIT, skip, take });
        }
        public long SortedSetLengthByValue(RedisKey key, RedisValue min, RedisValue max, Exclude exclude = Exclude.None, CommandFlags flags = CommandFlags.None)
        {
            var msg = GetLexMessage(RedisCommand.ZLEXCOUNT, key, min, max, exclude, 0, -1, flags);
            return ExecuteSync(msg, ResultProcessor.Int64);
        }

        public RedisValue[] SortedSetRangeByValue(RedisKey key, RedisValue min = default(RedisValue), RedisValue max = default(RedisValue), Exclude exclude = Exclude.None, long skip = 0, long take = -1, CommandFlags flags = CommandFlags.None)
        {
            var msg = GetLexMessage(RedisCommand.ZRANGEBYLEX, key, min, max, exclude, skip, take, flags);
            return ExecuteSync(msg, ResultProcessor.RedisValueArray);
        }

        public long SortedSetRemoveRangeByValue(RedisKey key, RedisValue min, RedisValue max, Exclude exclude = Exclude.None, CommandFlags flags = CommandFlags.None)
        {
            var msg = GetLexMessage(RedisCommand.ZREMRANGEBYLEX, key, min, max, exclude, 0, -1, flags);
            return ExecuteSync(msg, ResultProcessor.Int64);
        }

        public Task<long> SortedSetLengthByValueAsync(RedisKey key, RedisValue min, RedisValue max, Exclude exclude = Exclude.None, CommandFlags flags = CommandFlags.None)
        {
            var msg = GetLexMessage(RedisCommand.ZLEXCOUNT, key, min, max, exclude, 0, -1, flags);
            return ExecuteAsync(msg, ResultProcessor.Int64);
        }

        public Task<RedisValue[]> SortedSetRangeByValueAsync(RedisKey key, RedisValue min = default(RedisValue), RedisValue max = default(RedisValue), Exclude exclude = Exclude.None, long skip = 0, long take = -1, CommandFlags flags = CommandFlags.None)
        {
            var msg = GetLexMessage(RedisCommand.ZRANGEBYLEX, key, min, max, exclude, skip, take, flags);
            return ExecuteAsync(msg, ResultProcessor.RedisValueArray);
        }

        public Task<long> SortedSetRemoveRangeByValueAsync(RedisKey key, RedisValue min, RedisValue max, Exclude exclude = Exclude.None, CommandFlags flags = CommandFlags.None)
        {
            var msg = GetLexMessage(RedisCommand.ZREMRANGEBYLEX, key, min, max, exclude, 0, -1, flags);
            return ExecuteAsync(msg, ResultProcessor.Int64);
        }


        internal class ScanIterator<T> : CursorEnumerable<T>
        {
            private readonly RedisKey key;
            private readonly RedisValue pattern;
            private readonly RedisCommand command;
            private readonly ResultProcessor<ScanResult> processor;

            public ScanIterator(RedisDatabase database, ServerEndPoint server, RedisKey key, RedisValue pattern, int pageSize, long cursor, int pageOffset, CommandFlags flags,
                RedisCommand command, ResultProcessor<ScanResult> processor)
                : base(database, server, database.Database, pageSize, cursor, pageOffset, flags)
            {
                this.key = key;
                this.pattern = pattern;
                this.command = command;
                this.processor = processor;
            }
            protected override ResultProcessor<CursorEnumerable<T>.ScanResult> Processor
            {
                get { return processor; }
            }
            protected override Message CreateMessage(long cursor)
            {
                if (CursorUtils.IsNil(pattern))
                {
                    if (pageSize == CursorUtils.DefaultPageSize)
                    {
                        return Message.Create(db, flags, command, key, cursor);
                    }
                    else
                    {
                        return Message.Create(db, flags, command, key, cursor, RedisLiterals.COUNT, pageSize);
                    }
                }
                else
                {
                    if (pageSize == CursorUtils.DefaultPageSize)
                    {
                        return Message.Create(db, flags, command, key, cursor, RedisLiterals.MATCH, pattern);
                    }
                    else
                    {
                        return Message.Create(db, flags, command, key, new RedisValue[] { cursor, RedisLiterals.MATCH, pattern, RedisLiterals.COUNT, pageSize });
                    }
                }
            }
        }

        internal sealed class ScriptLoadMessage : Message
        {
            internal readonly string Script;
            public ScriptLoadMessage(CommandFlags flags, string script) : base(-1, flags, RedisCommand.SCRIPT)
            {
                if (script == null) throw new ArgumentNullException("script");
                this.Script = script;
            }
            internal override void WriteImpl(PhysicalConnection physical)
            {
                physical.WriteHeader(Command, 2);
                physical.Write(RedisLiterals.LOAD);
                physical.Write((RedisValue)Script);
            }
        }
        sealed class HashScanResultProcessor : ScanResultProcessor<HashEntry>
        {
            public static readonly ResultProcessor<ScanIterator<HashEntry>.ScanResult> Default = new HashScanResultProcessor();
            private HashScanResultProcessor() { }
            protected override HashEntry[] Parse(RawResult result)
            {
                HashEntry[] pairs;
                if (!HashEntryArray.TryParse(result, out pairs)) pairs = null;
                return pairs;
            }
        }

        private abstract class ScanResultProcessor<T> : ResultProcessor<ScanIterator<T>.ScanResult>
        {
            protected abstract T[] Parse(RawResult result);

            protected override bool SetResultCore(PhysicalConnection connection, Message message, RawResult result)
            {
                switch (result.Type)
                {
                    case ResultType.MultiBulk:
                        var arr = result.GetItems();
                        long i64;
                        if (arr.Length == 2 && arr[1].Type == ResultType.MultiBulk && arr[0].TryGetInt64(out i64))
                        {
                            var sscanResult = new ScanIterator<T>.ScanResult(i64, Parse(arr[1]));
                            SetResult(message, sscanResult);
                            return true;
                        }
                        break;
                }
                return false;
            }
        }
        private sealed class ScriptEvalMessage : Message, IMultiMessage
        {
            private readonly RedisKey[] keys;
            private readonly string script;
            private readonly RedisValue[] values;
            private byte[] asciiHash, hexHash;
            public ScriptEvalMessage(int db, CommandFlags flags, string script, RedisKey[] keys, RedisValue[] values)
                : this(db, flags, ResultProcessor.ScriptLoadProcessor.IsSHA1(script) ? RedisCommand.EVALSHA : RedisCommand.EVAL, script, null, keys, values)
            {
                if (script == null) throw new ArgumentNullException("script");
            }
            public ScriptEvalMessage(int db, CommandFlags flags, byte[] hash, RedisKey[] keys, RedisValue[] values)
                : this(db, flags, RedisCommand.EVAL, null, hash, keys, values)
            {
                if (hash == null) throw new ArgumentNullException("hash");
            }

            private ScriptEvalMessage(int db, CommandFlags flags, RedisCommand command, string script, byte[] hexHash, RedisKey[] keys, RedisValue[] values) : base(db, flags, command)
            {
                this.script = script;
                this.hexHash = hexHash;

                if (keys == null) keys = RedisKey.EmptyArray;
                if (values == null) values = RedisValue.EmptyArray;
                for (int i = 0; i < keys.Length; i++)
                    keys[i].AssertNotNull();
                this.keys = keys;
                for (int i = 0; i < values.Length; i++)
                    values[i].AssertNotNull();
                this.values = values;
            }

            public override int GetHashSlot(ServerSelectionStrategy serverSelectionStrategy)
            {
                int slot = ServerSelectionStrategy.NoSlot;
                for (int i = 0; i < keys.Length; i++)
                    slot = serverSelectionStrategy.CombineSlot(slot, keys[i]);
                return slot;
            }

            public IEnumerable<Message> GetMessages(PhysicalConnection connection)
            {
                if (script != null) // a script was provided (rather than a hash); check it is known
                {
                    asciiHash = connection.Bridge.ServerEndPoint.GetScriptHash(script, command);

                    if (asciiHash == null)
                    {
                        var msg = new ScriptLoadMessage(Flags, script);
                        msg.SetInternalCall();
                        msg.SetSource(ResultProcessor.ScriptLoad, null);
                        yield return msg;
                    }
                }
                yield return this;
            }

            internal override void WriteImpl(PhysicalConnection physical)
            {
                if(hexHash != null)
                {
                    physical.WriteHeader(RedisCommand.EVALSHA, 2 + keys.Length + values.Length);
                    physical.WriteAsHex(hexHash);
                }
                else if (asciiHash != null)
                {
                    physical.WriteHeader(RedisCommand.EVALSHA, 2 + keys.Length + values.Length);
                    physical.Write((RedisValue)asciiHash);
                }
                else
                {
                    physical.WriteHeader(RedisCommand.EVAL, 2 + keys.Length + values.Length);
                    physical.Write((RedisValue)script);
                }
                physical.Write(keys.Length);
                for (int i = 0; i < keys.Length; i++)
                    physical.Write(keys[i]);
                for (int i = 0; i < values.Length; i++)
                    physical.Write(values[i]);
            }
        }

        sealed class SetScanResultProcessor : ScanResultProcessor<RedisValue>
        {
            public static readonly ResultProcessor<ScanIterator<RedisValue>.ScanResult> Default = new SetScanResultProcessor();
            private SetScanResultProcessor() { }
            protected override RedisValue[] Parse(RawResult result)
            {
                return result.GetItemsAsValues();
            }
        }
        sealed class SortedSetCombineAndStoreCommandMessage : Message.CommandKeyBase // ZINTERSTORE and ZUNIONSTORE have a very unusual signature
        {
            private readonly RedisKey[] keys;
            private readonly RedisValue[] values;
            public SortedSetCombineAndStoreCommandMessage(int db, CommandFlags flags, RedisCommand command, RedisKey destination, RedisKey[] keys, RedisValue[] values) : base(db, flags, command, destination)
            {
                for (int i = 0; i < keys.Length; i++)
                    keys[i].AssertNotNull();
                this.keys = keys;
                for (int i = 0; i < values.Length; i++)
                    values[i].AssertNotNull();
                this.values = values;
            }
            public override int GetHashSlot(ServerSelectionStrategy serverSelectionStrategy)
            {
                int slot = base.GetHashSlot(serverSelectionStrategy);
                for (int i = 0; i < keys.Length; i++)
                    slot = serverSelectionStrategy.CombineSlot(slot, keys[i]);
                return slot;
            }
            internal override void WriteImpl(PhysicalConnection physical)
            {
                physical.WriteHeader(Command, 2 + keys.Length + values.Length);
                physical.Write(Key);
                physical.Write(keys.Length);
                for (int i = 0; i < keys.Length; i++)
                    physical.Write(keys[i]);
                for (int i = 0; i < values.Length; i++)
                    physical.Write(values[i]);
            }
        }

        sealed class SortedSetScanResultProcessor : ScanResultProcessor<SortedSetEntry>
        {
            public static readonly ResultProcessor<ScanIterator<SortedSetEntry>.ScanResult> Default = new SortedSetScanResultProcessor();
            private SortedSetScanResultProcessor() { }
            protected override SortedSetEntry[] Parse(RawResult result)
            {
                SortedSetEntry[] pairs;
                if (!SortedSetWithScores.TryParse(result, out pairs)) pairs = null;
                return pairs;
            }
        }
        private class StringGetWithExpiryMessage : Message.CommandKeyBase, IMultiMessage
        {
            private readonly RedisCommand ttlCommand;
            private ResultBox<TimeSpan?> box;

            public StringGetWithExpiryMessage(int db, CommandFlags flags, RedisCommand ttlCommand, RedisKey key)
                            : base(db, flags | CommandFlags.NoRedirect /* <== not implemented/tested */, RedisCommand.GET, key)
            {
                this.ttlCommand = ttlCommand;
            }
            public override string CommandAndKey { get { return ttlCommand + "+" + RedisCommand.GET + " " + (string)Key; } }

            public IEnumerable<Message> GetMessages(PhysicalConnection connection)
            {
                box = ResultBox<TimeSpan?>.Get(null);
                var ttl = Message.Create(Db, Flags, ttlCommand, Key);
                var proc = ttlCommand == RedisCommand.PTTL ? ResultProcessor.TimeSpanFromMilliseconds : ResultProcessor.TimeSpanFromSeconds;
                ttl.SetSource(proc, box);
                yield return ttl;
                yield return this;
            }

            public bool UnwrapValue(out TimeSpan? value, out Exception ex)
            {
                if (box != null)
                {
                    ResultBox<TimeSpan?>.UnwrapAndRecycle(box, out value, out ex);
                    box = null;
                    return ex == null;
                }
                value = null;
                ex = null;
                return false;
            }
            internal override void WriteImpl(PhysicalConnection physical)
            {
                physical.WriteHeader(command, 1);
                physical.Write(Key);
            }
        }

        private class StringGetWithExpiryProcessor : ResultProcessor<RedisValueWithExpiry>
        {
            public static readonly ResultProcessor<RedisValueWithExpiry> Default = new StringGetWithExpiryProcessor();
            private StringGetWithExpiryProcessor() { }
            protected  override bool SetResultCore(PhysicalConnection connection, Message message, RawResult result)
            {
                switch(result.Type)
                {
                    case ResultType.Integer:
                    case ResultType.SimpleString:
                    case ResultType.BulkString:
                        RedisValue value = result.AsRedisValue();
                        var sgwem = message as StringGetWithExpiryMessage;
                        TimeSpan? expiry;
                        Exception ex;
                        if (sgwem != null && sgwem.UnwrapValue(out expiry, out ex))
                        {
                            if (ex == null)
                            {
                                SetResult(message, new RedisValueWithExpiry(value, expiry));
                            }
                            else
                            {
                                SetException(message, ex);
                            }
                            return true;
                        }
                        break;
                }
                return false;
            }
        }
    }
}
