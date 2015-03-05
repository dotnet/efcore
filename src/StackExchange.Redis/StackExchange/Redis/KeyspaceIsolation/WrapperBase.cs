using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Threading.Tasks;

namespace StackExchange.Redis.KeyspaceIsolation
{
    internal class WrapperBase<TInner> : IDatabaseAsync where TInner : IDatabaseAsync
    {
        private readonly TInner _inner;
        private readonly byte[] _keyPrefix;

        internal WrapperBase(TInner inner, byte[] keyPrefix)
        {
            _inner = inner;
            _keyPrefix = keyPrefix;
        }

        public ConnectionMultiplexer Multiplexer
        {
            get { return this.Inner.Multiplexer; }
        }

        internal TInner Inner
        {
            get { return _inner; }
        }

        internal byte[] Prefix
        {
            get { return _keyPrefix; }
        }

        public Task<RedisValue> DebugObjectAsync(RedisKey key, CommandFlags flags = CommandFlags.None)
        {
            return this.Inner.DebugObjectAsync(this.ToInner(key), flags);
        }

        public Task<double> HashDecrementAsync(RedisKey key, RedisValue hashField, double value, CommandFlags flags = CommandFlags.None)
        {
            return this.Inner.HashDecrementAsync(this.ToInner(key), hashField, value, flags);
        }

        public Task<long> HashDecrementAsync(RedisKey key, RedisValue hashField, long value = 1, CommandFlags flags = CommandFlags.None)
        {
            return this.Inner.HashDecrementAsync(this.ToInner(key), hashField, value, flags);
        }

        public Task<long> HashDeleteAsync(RedisKey key, RedisValue[] hashFields, CommandFlags flags = CommandFlags.None)
        {
            return this.Inner.HashDeleteAsync(this.ToInner(key), hashFields, flags);
        }

        public Task<bool> HashDeleteAsync(RedisKey key, RedisValue hashField, CommandFlags flags = CommandFlags.None)
        {
            return this.Inner.HashDeleteAsync(this.ToInner(key), hashField, flags);
        }

        public Task<bool> HashExistsAsync(RedisKey key, RedisValue hashField, CommandFlags flags = CommandFlags.None)
        {
            return this.Inner.HashExistsAsync(this.ToInner(key), hashField, flags);
        }

        public Task<HashEntry[]> HashGetAllAsync(RedisKey key, CommandFlags flags = CommandFlags.None)
        {
            return this.Inner.HashGetAllAsync(this.ToInner(key), flags);
        }

        public Task<RedisValue[]> HashGetAsync(RedisKey key, RedisValue[] hashFields, CommandFlags flags = CommandFlags.None)
        {
            return this.Inner.HashGetAsync(this.ToInner(key), hashFields, flags);
        }

        public Task<RedisValue> HashGetAsync(RedisKey key, RedisValue hashField, CommandFlags flags = CommandFlags.None)
        {
            return this.Inner.HashGetAsync(this.ToInner(key), hashField, flags);
        }

        public Task<double> HashIncrementAsync(RedisKey key, RedisValue hashField, double value, CommandFlags flags = CommandFlags.None)
        {
            return this.Inner.HashIncrementAsync(this.ToInner(key), hashField, value, flags);
        }

        public Task<long> HashIncrementAsync(RedisKey key, RedisValue hashField, long value = 1, CommandFlags flags = CommandFlags.None)
        {
            return this.Inner.HashIncrementAsync(this.ToInner(key), hashField, value, flags);
        }

        public Task<RedisValue[]> HashKeysAsync(RedisKey key, CommandFlags flags = CommandFlags.None)
        {
            return this.Inner.HashKeysAsync(this.ToInner(key), flags);
        }

        public Task<long> HashLengthAsync(RedisKey key, CommandFlags flags = CommandFlags.None)
        {
            return this.Inner.HashLengthAsync(this.ToInner(key), flags);
        }

        public Task<bool> HashSetAsync(RedisKey key, RedisValue hashField, RedisValue value, When when = When.Always, CommandFlags flags = CommandFlags.None)
        {
            return this.Inner.HashSetAsync(this.ToInner(key), hashField, value, when, flags);
        }

        public Task HashSetAsync(RedisKey key, HashEntry[] hashFields, CommandFlags flags = CommandFlags.None)
        {
            return this.Inner.HashSetAsync(this.ToInner(key), hashFields, flags);
        }

        public Task<RedisValue[]> HashValuesAsync(RedisKey key, CommandFlags flags = CommandFlags.None)
        {
            return this.Inner.HashValuesAsync(this.ToInner(key), flags);
        }

        public Task<bool> HyperLogLogAddAsync(RedisKey key, RedisValue[] values, CommandFlags flags = CommandFlags.None)
        {
            return this.Inner.HyperLogLogAddAsync(this.ToInner(key), values, flags);
        }

        public Task<bool> HyperLogLogAddAsync(RedisKey key, RedisValue value, CommandFlags flags = CommandFlags.None)
        {
            return this.Inner.HyperLogLogAddAsync(this.ToInner(key), value, flags);
        }

        public Task<long> HyperLogLogLengthAsync(RedisKey key, CommandFlags flags = CommandFlags.None)
        {
            return this.Inner.HyperLogLogLengthAsync(this.ToInner(key), flags);
        }

        public Task<long> HyperLogLogLengthAsync(RedisKey[] keys, CommandFlags flags = CommandFlags.None)
        {
            return this.Inner.HyperLogLogLengthAsync(this.ToInner(keys), flags);
        }

        public Task HyperLogLogMergeAsync(RedisKey destination, RedisKey[] sourceKeys, CommandFlags flags = CommandFlags.None)
        {
            return this.Inner.HyperLogLogMergeAsync(this.ToInner(destination), this.ToInner(sourceKeys), flags);
        }

        public Task HyperLogLogMergeAsync(RedisKey destination, RedisKey first, RedisKey second, CommandFlags flags = CommandFlags.None)
        {
            return this.Inner.HyperLogLogMergeAsync(this.ToInner(destination), this.ToInner(first), this.ToInner(second), flags);
        }

        public Task<EndPoint> IdentifyEndpointAsync(RedisKey key = default(RedisKey), CommandFlags flags = CommandFlags.None)
        {
            return this.Inner.IdentifyEndpointAsync(this.ToInner(key), flags);
        }

        public bool IsConnected(RedisKey key, CommandFlags flags = CommandFlags.None)
        {
            return this.Inner.IsConnected(this.ToInner(key), flags);
        }

        public Task<long> KeyDeleteAsync(RedisKey[] keys, CommandFlags flags = CommandFlags.None)
        {
            return this.Inner.KeyDeleteAsync(this.ToInner(keys), flags);
        }

        public Task<bool> KeyDeleteAsync(RedisKey key, CommandFlags flags = CommandFlags.None)
        {
            return this.Inner.KeyDeleteAsync(this.ToInner(key), flags);
        }

        public Task<byte[]> KeyDumpAsync(RedisKey key, CommandFlags flags = CommandFlags.None)
        {
            return this.Inner.KeyDumpAsync(this.ToInner(key), flags);
        }

        public Task<bool> KeyExistsAsync(RedisKey key, CommandFlags flags = CommandFlags.None)
        {
            return this.Inner.KeyExistsAsync(this.ToInner(key), flags);
        }

        public Task<bool> KeyExpireAsync(RedisKey key, DateTime? expiry, CommandFlags flags = CommandFlags.None)
        {
            return this.Inner.KeyExpireAsync(this.ToInner(key), expiry, flags);
        }

        public Task<bool> KeyExpireAsync(RedisKey key, TimeSpan? expiry, CommandFlags flags = CommandFlags.None)
        {
            return this.Inner.KeyExpireAsync(this.ToInner(key), expiry, flags);
        }

        public Task KeyMigrateAsync(RedisKey key, EndPoint toServer, int toDatabase = 0, int timeoutMilliseconds = 0, MigrateOptions migrateOptions = MigrateOptions.None, CommandFlags flags = CommandFlags.None)
        {
            return this.Inner.KeyMigrateAsync(this.ToInner(key), toServer, toDatabase, timeoutMilliseconds, migrateOptions, flags);
        }

        public Task<bool> KeyMoveAsync(RedisKey key, int database, CommandFlags flags = CommandFlags.None)
        {
            return this.Inner.KeyMoveAsync(this.ToInner(key), database, flags);
        }

        public Task<bool> KeyPersistAsync(RedisKey key, CommandFlags flags = CommandFlags.None)
        {
            return this.Inner.KeyPersistAsync(this.ToInner(key), flags);
        }

        public Task<RedisKey> KeyRandomAsync(CommandFlags flags = CommandFlags.None)
        {
            throw new NotSupportedException("RANDOMKEY is not supported when a key-prefix is specified");
        }

        public Task<bool> KeyRenameAsync(RedisKey key, RedisKey newKey, When when = When.Always, CommandFlags flags = CommandFlags.None)
        {
            return this.Inner.KeyRenameAsync(this.ToInner(key), this.ToInner(newKey), when, flags);
        }

        public Task KeyRestoreAsync(RedisKey key, byte[] value, TimeSpan? expiry = null, CommandFlags flags = CommandFlags.None)
        {
            return this.Inner.KeyRestoreAsync(this.ToInner(key), value, expiry, flags);
        }

        public Task<TimeSpan?> KeyTimeToLiveAsync(RedisKey key, CommandFlags flags = CommandFlags.None)
        {
            return this.Inner.KeyTimeToLiveAsync(this.ToInner(key), flags);
        }

        public Task<RedisType> KeyTypeAsync(RedisKey key, CommandFlags flags = CommandFlags.None)
        {
            return this.Inner.KeyTypeAsync(this.ToInner(key), flags);
        }

        public Task<RedisValue> ListGetByIndexAsync(RedisKey key, long index, CommandFlags flags = CommandFlags.None)
        {
            return this.Inner.ListGetByIndexAsync(this.ToInner(key), index, flags);
        }

        public Task<long> ListInsertAfterAsync(RedisKey key, RedisValue pivot, RedisValue value, CommandFlags flags = CommandFlags.None)
        {
            return this.Inner.ListInsertAfterAsync(this.ToInner(key), pivot, value, flags);
        }

        public Task<long> ListInsertBeforeAsync(RedisKey key, RedisValue pivot, RedisValue value, CommandFlags flags = CommandFlags.None)
        {
            return this.Inner.ListInsertBeforeAsync(this.ToInner(key), pivot, value, flags);
        }

        public Task<RedisValue> ListLeftPopAsync(RedisKey key, CommandFlags flags = CommandFlags.None)
        {
            return this.Inner.ListLeftPopAsync(this.ToInner(key), flags);
        }

        public Task<long> ListLeftPushAsync(RedisKey key, RedisValue[] values, CommandFlags flags = CommandFlags.None)
        {
            return this.Inner.ListLeftPushAsync(this.ToInner(key), values, flags);
        }

        public Task<long> ListLeftPushAsync(RedisKey key, RedisValue value, When when = When.Always, CommandFlags flags = CommandFlags.None)
        {
            return this.Inner.ListLeftPushAsync(this.ToInner(key), value, when, flags);
        }

        public Task<long> ListLengthAsync(RedisKey key, CommandFlags flags = CommandFlags.None)
        {
            return this.Inner.ListLengthAsync(this.ToInner(key), flags);
        }

        public Task<RedisValue[]> ListRangeAsync(RedisKey key, long start = 0, long stop = -1, CommandFlags flags = CommandFlags.None)
        {
            return this.Inner.ListRangeAsync(this.ToInner(key), start, stop, flags);
        }

        public Task<long> ListRemoveAsync(RedisKey key, RedisValue value, long count = 0, CommandFlags flags = CommandFlags.None)
        {
            return this.Inner.ListRemoveAsync(this.ToInner(key), value, count, flags);
        }

        public Task<RedisValue> ListRightPopAsync(RedisKey key, CommandFlags flags = CommandFlags.None)
        {
            return this.Inner.ListRightPopAsync(this.ToInner(key), flags);
        }

        public Task<RedisValue> ListRightPopLeftPushAsync(RedisKey source, RedisKey destination, CommandFlags flags = CommandFlags.None)
        {
            return this.Inner.ListRightPopLeftPushAsync(this.ToInner(source), this.ToInner(destination), flags);
        }

        public Task<long> ListRightPushAsync(RedisKey key, RedisValue[] values, CommandFlags flags = CommandFlags.None)
        {
            return this.Inner.ListRightPushAsync(this.ToInner(key), values, flags);
        }

        public Task<long> ListRightPushAsync(RedisKey key, RedisValue value, When when = When.Always, CommandFlags flags = CommandFlags.None)
        {
            return this.Inner.ListRightPushAsync(this.ToInner(key), value, when, flags);
        }

        public Task ListSetByIndexAsync(RedisKey key, long index, RedisValue value, CommandFlags flags = CommandFlags.None)
        {
            return this.Inner.ListSetByIndexAsync(this.ToInner(key), index, value, flags);
        }

        public Task ListTrimAsync(RedisKey key, long start, long stop, CommandFlags flags = CommandFlags.None)
        {
            return this.Inner.ListTrimAsync(this.ToInner(key), start, stop, flags);
        }

        public Task<bool> LockExtendAsync(RedisKey key, RedisValue value, TimeSpan expiry, CommandFlags flags = CommandFlags.None)
        {
            return this.Inner.LockExtendAsync(this.ToInner(key), value, expiry, flags);
        }

        public Task<RedisValue> LockQueryAsync(RedisKey key, CommandFlags flags = CommandFlags.None)
        {
            return this.Inner.LockQueryAsync(this.ToInner(key), flags);
        }

        public Task<bool> LockReleaseAsync(RedisKey key, RedisValue value, CommandFlags flags = CommandFlags.None)
        {
            return this.Inner.LockReleaseAsync(this.ToInner(key), value, flags);
        }

        public Task<bool> LockTakeAsync(RedisKey key, RedisValue value, TimeSpan expiry, CommandFlags flags = CommandFlags.None)
        {
            return this.Inner.LockTakeAsync(this.ToInner(key), value, expiry, flags);
        }

        public Task<long> PublishAsync(RedisChannel channel, RedisValue message, CommandFlags flags = CommandFlags.None)
        {
            return this.Inner.PublishAsync(this.ToInner(channel), message, flags);
        }

        public Task<RedisResult> ScriptEvaluateAsync(byte[] hash, RedisKey[] keys = null, RedisValue[] values = null, CommandFlags flags = CommandFlags.None)
        {
            // TODO: The return value could contain prefixed keys. It might make sense to 'unprefix' those?
            return this.Inner.ScriptEvaluateAsync(hash, this.ToInner(keys), values, flags);
        }

        public Task<RedisResult> ScriptEvaluateAsync(string script, RedisKey[] keys = null, RedisValue[] values = null, CommandFlags flags = CommandFlags.None)
        {
            // TODO: The return value could contain prefixed keys. It might make sense to 'unprefix' those?
            return this.Inner.ScriptEvaluateAsync(script, this.ToInner(keys), values, flags);
        }

        public Task<long> SetAddAsync(RedisKey key, RedisValue[] values, CommandFlags flags = CommandFlags.None)
        {
            return this.Inner.SetAddAsync(this.ToInner(key), values, flags);
        }

        public Task<bool> SetAddAsync(RedisKey key, RedisValue value, CommandFlags flags = CommandFlags.None)
        {
            return this.Inner.SetAddAsync(this.ToInner(key), value, flags);
        }

        public Task<long> SetCombineAndStoreAsync(SetOperation operation, RedisKey destination, RedisKey[] keys, CommandFlags flags = CommandFlags.None)
        {
            return this.Inner.SetCombineAndStoreAsync(operation, this.ToInner(destination), this.ToInner(keys), flags);
        }

        public Task<long> SetCombineAndStoreAsync(SetOperation operation, RedisKey destination, RedisKey first, RedisKey second, CommandFlags flags = CommandFlags.None)
        {
            return this.Inner.SetCombineAndStoreAsync(operation, this.ToInner(destination), this.ToInner(first), this.ToInner(second), flags);
        }

        public Task<RedisValue[]> SetCombineAsync(SetOperation operation, RedisKey[] keys, CommandFlags flags = CommandFlags.None)
        {
            return this.Inner.SetCombineAsync(operation, this.ToInner(keys), flags);
        }

        public Task<RedisValue[]> SetCombineAsync(SetOperation operation, RedisKey first, RedisKey second, CommandFlags flags = CommandFlags.None)
        {
            return this.Inner.SetCombineAsync(operation, this.ToInner(first), this.ToInner(second), flags);
        }

        public Task<bool> SetContainsAsync(RedisKey key, RedisValue value, CommandFlags flags = CommandFlags.None)
        {
            return this.Inner.SetContainsAsync(this.ToInner(key), value, flags);
        }

        public Task<long> SetLengthAsync(RedisKey key, CommandFlags flags = CommandFlags.None)
        {
            return this.Inner.SetLengthAsync(this.ToInner(key), flags);
        }

        public Task<RedisValue[]> SetMembersAsync(RedisKey key, CommandFlags flags = CommandFlags.None)
        {
            return this.Inner.SetMembersAsync(this.ToInner(key), flags);
        }

        public Task<bool> SetMoveAsync(RedisKey source, RedisKey destination, RedisValue value, CommandFlags flags = CommandFlags.None)
        {
            return this.Inner.SetMoveAsync(this.ToInner(source), this.ToInner(destination), value, flags);
        }

        public Task<RedisValue> SetPopAsync(RedisKey key, CommandFlags flags = CommandFlags.None)
        {
            return this.Inner.SetPopAsync(this.ToInner(key), flags);
        }

        public Task<RedisValue> SetRandomMemberAsync(RedisKey key, CommandFlags flags = CommandFlags.None)
        {
            return this.Inner.SetRandomMemberAsync(this.ToInner(key), flags);
        }

        public Task<RedisValue[]> SetRandomMembersAsync(RedisKey key, long count, CommandFlags flags = CommandFlags.None)
        {
            return this.Inner.SetRandomMembersAsync(this.ToInner(key), count, flags);
        }

        public Task<long> SetRemoveAsync(RedisKey key, RedisValue[] values, CommandFlags flags = CommandFlags.None)
        {
            return this.Inner.SetRemoveAsync(this.ToInner(key), values, flags);
        }

        public Task<bool> SetRemoveAsync(RedisKey key, RedisValue value, CommandFlags flags = CommandFlags.None)
        {
            return this.Inner.SetRemoveAsync(this.ToInner(key), value, flags);
        }

        public Task<long> SortAndStoreAsync(RedisKey destination, RedisKey key, long skip = 0, long take = -1, Order order = Order.Ascending, SortType sortType = SortType.Numeric, RedisValue by = default(RedisValue), RedisValue[] get = null, CommandFlags flags = CommandFlags.None)
        {
            return this.Inner.SortAndStoreAsync(this.ToInner(destination), this.ToInner(key), skip, take, order, sortType, this.SortByToInner(by), this.SortGetToInner(get), flags);
        }

        public Task<RedisValue[]> SortAsync(RedisKey key, long skip = 0, long take = -1, Order order = Order.Ascending, SortType sortType = SortType.Numeric, RedisValue by = default(RedisValue), RedisValue[] get = null, CommandFlags flags = CommandFlags.None)
        {
            return this.Inner.SortAsync(this.ToInner(key), skip, take, order, sortType, this.SortByToInner(by), this.SortGetToInner(get), flags);
        }

        public Task<long> SortedSetAddAsync(RedisKey key, SortedSetEntry[] values, CommandFlags flags = CommandFlags.None)
        {
            return this.Inner.SortedSetAddAsync(this.ToInner(key), values, flags);
        }

        public Task<bool> SortedSetAddAsync(RedisKey key, RedisValue member, double score, CommandFlags flags = CommandFlags.None)
        {
            return this.Inner.SortedSetAddAsync(this.ToInner(key), member, score, flags);
        }

        public Task<long> SortedSetCombineAndStoreAsync(SetOperation operation, RedisKey destination, RedisKey[] keys, double[] weights = null, Aggregate aggregate = Aggregate.Sum, CommandFlags flags = CommandFlags.None)
        {
            return this.Inner.SortedSetCombineAndStoreAsync(operation, this.ToInner(destination), this.ToInner(keys), weights, aggregate, flags);
        }

        public Task<long> SortedSetCombineAndStoreAsync(SetOperation operation, RedisKey destination, RedisKey first, RedisKey second, Aggregate aggregate = Aggregate.Sum, CommandFlags flags = CommandFlags.None)
        {
            return this.Inner.SortedSetCombineAndStoreAsync(operation, this.ToInner(destination), this.ToInner(first), this.ToInner(second), aggregate, flags);
        }

        public Task<double> SortedSetDecrementAsync(RedisKey key, RedisValue member, double value, CommandFlags flags = CommandFlags.None)
        {
            return this.Inner.SortedSetDecrementAsync(this.ToInner(key), member, value, flags);
        }

        public Task<double> SortedSetIncrementAsync(RedisKey key, RedisValue member, double value, CommandFlags flags = CommandFlags.None)
        {
            return this.Inner.SortedSetIncrementAsync(this.ToInner(key), member, value, flags);
        }

        public Task<long> SortedSetLengthAsync(RedisKey key, double min = -1.0 / 0.0, double max = 1.0 / 0.0, Exclude exclude = Exclude.None, CommandFlags flags = CommandFlags.None)
        {
            return this.Inner.SortedSetLengthAsync(this.ToInner(key), min, max, exclude, flags);
        }

        public Task<long> SortedSetLengthByValueAsync(RedisKey key, RedisValue min, RedisValue max, Exclude exclude = Exclude.None, CommandFlags flags = CommandFlags.None)
        {
            return this.Inner.SortedSetLengthByValueAsync(this.ToInner(key), min, max, exclude, flags);
        }

        public Task<RedisValue[]> SortedSetRangeByRankAsync(RedisKey key, long start = 0, long stop = -1, Order order = Order.Ascending, CommandFlags flags = CommandFlags.None)
        {
            return this.Inner.SortedSetRangeByRankAsync(this.ToInner(key), start, stop, order, flags);
        }

        public Task<SortedSetEntry[]> SortedSetRangeByRankWithScoresAsync(RedisKey key, long start = 0, long stop = -1, Order order = Order.Ascending, CommandFlags flags = CommandFlags.None)
        {
            return this.Inner.SortedSetRangeByRankWithScoresAsync(this.ToInner(key), start, stop, order, flags);
        }

        public Task<RedisValue[]> SortedSetRangeByScoreAsync(RedisKey key, double start = -1.0 / 0.0, double stop = 1.0 / 0.0, Exclude exclude = Exclude.None, Order order = Order.Ascending, long skip = 0, long take = -1, CommandFlags flags = CommandFlags.None)
        {
            return this.Inner.SortedSetRangeByScoreAsync(this.ToInner(key), start, stop, exclude, order, skip, take, flags);
        }

        public Task<SortedSetEntry[]> SortedSetRangeByScoreWithScoresAsync(RedisKey key, double start = -1.0 / 0.0, double stop = 1.0 / 0.0, Exclude exclude = Exclude.None, Order order = Order.Ascending, long skip = 0, long take = -1, CommandFlags flags = CommandFlags.None)
        {
            return this.Inner.SortedSetRangeByScoreWithScoresAsync(this.ToInner(key), start, stop, exclude, order, skip, take, flags);
        }

        public Task<RedisValue[]> SortedSetRangeByValueAsync(RedisKey key, RedisValue min = default(RedisValue), RedisValue max = default(RedisValue), Exclude exclude = Exclude.None, long skip = 0, long take = -1, CommandFlags flags = CommandFlags.None)
        {
            return this.Inner.SortedSetRangeByValueAsync(this.ToInner(key), min, max, exclude, skip, take, flags);
        }

        public Task<long?> SortedSetRankAsync(RedisKey key, RedisValue member, Order order = Order.Ascending, CommandFlags flags = CommandFlags.None)
        {
            return this.Inner.SortedSetRankAsync(this.ToInner(key), member, order, flags);
        }

        public Task<long> SortedSetRemoveAsync(RedisKey key, RedisValue[] members, CommandFlags flags = CommandFlags.None)
        {
            return this.Inner.SortedSetRemoveAsync(this.ToInner(key), members, flags);
        }

        public Task<bool> SortedSetRemoveAsync(RedisKey key, RedisValue member, CommandFlags flags = CommandFlags.None)
        {
            return this.Inner.SortedSetRemoveAsync(this.ToInner(key), member, flags);
        }

        public Task<long> SortedSetRemoveRangeByRankAsync(RedisKey key, long start, long stop, CommandFlags flags = CommandFlags.None)
        {
            return this.Inner.SortedSetRemoveRangeByRankAsync(this.ToInner(key), start, stop, flags);
        }

        public Task<long> SortedSetRemoveRangeByScoreAsync(RedisKey key, double start, double stop, Exclude exclude = Exclude.None, CommandFlags flags = CommandFlags.None)
        {
            return this.Inner.SortedSetRemoveRangeByScoreAsync(this.ToInner(key), start, stop, exclude, flags);
        }

        public Task<long> SortedSetRemoveRangeByValueAsync(RedisKey key, RedisValue min, RedisValue max, Exclude exclude = Exclude.None, CommandFlags flags = CommandFlags.None)
        {
            return this.Inner.SortedSetRemoveRangeByValueAsync(this.ToInner(key), min, max, exclude, flags);
        }

        public Task<double?> SortedSetScoreAsync(RedisKey key, RedisValue member, CommandFlags flags = CommandFlags.None)
        {
            return this.Inner.SortedSetScoreAsync(this.ToInner(key), member, flags);
        }

        public Task<long> StringAppendAsync(RedisKey key, RedisValue value, CommandFlags flags = CommandFlags.None)
        {
            return this.Inner.StringAppendAsync(this.ToInner(key), value, flags);
        }

        public Task<long> StringBitCountAsync(RedisKey key, long start = 0, long end = -1, CommandFlags flags = CommandFlags.None)
        {
            return this.Inner.StringBitCountAsync(this.ToInner(key), start, end, flags);
        }

        public Task<long> StringBitOperationAsync(Bitwise operation, RedisKey destination, RedisKey[] keys, CommandFlags flags = CommandFlags.None)
        {
            return this.Inner.StringBitOperationAsync(operation, this.ToInner(destination), this.ToInner(keys), flags);
        }

        public Task<long> StringBitOperationAsync(Bitwise operation, RedisKey destination, RedisKey first, RedisKey second = default(RedisKey), CommandFlags flags = CommandFlags.None)
        {
            return this.Inner.StringBitOperationAsync(operation, this.ToInner(destination), this.ToInner(first), this.ToInnerOrDefault(second), flags);
        }

        public Task<long> StringBitPositionAsync(RedisKey key, bool bit, long start = 0, long end = -1, CommandFlags flags = CommandFlags.None)
        {
            return this.Inner.StringBitPositionAsync(this.ToInner(key), bit, start, end, flags);
        }

        public Task<double> StringDecrementAsync(RedisKey key, double value, CommandFlags flags = CommandFlags.None)
        {
            return this.Inner.StringDecrementAsync(this.ToInner(key), value, flags);
        }

        public Task<long> StringDecrementAsync(RedisKey key, long value = 1, CommandFlags flags = CommandFlags.None)
        {
            return this.Inner.StringDecrementAsync(this.ToInner(key), value, flags);
        }

        public Task<RedisValue[]> StringGetAsync(RedisKey[] keys, CommandFlags flags = CommandFlags.None)
        {
            return this.Inner.StringGetAsync(this.ToInner(keys), flags);
        }

        public Task<RedisValue> StringGetAsync(RedisKey key, CommandFlags flags = CommandFlags.None)
        {
            return this.Inner.StringGetAsync(this.ToInner(key), flags);
        }

        public Task<bool> StringGetBitAsync(RedisKey key, long offset, CommandFlags flags = CommandFlags.None)
        {
            return this.Inner.StringGetBitAsync(this.ToInner(key), offset, flags);
        }

        public Task<RedisValue> StringGetRangeAsync(RedisKey key, long start, long end, CommandFlags flags = CommandFlags.None)
        {
            return this.Inner.StringGetRangeAsync(this.ToInner(key), start, end, flags);
        }

        public Task<RedisValue> StringGetSetAsync(RedisKey key, RedisValue value, CommandFlags flags = CommandFlags.None)
        {
            return this.Inner.StringGetSetAsync(this.ToInner(key), value, flags);
        }

        public Task<RedisValueWithExpiry> StringGetWithExpiryAsync(RedisKey key, CommandFlags flags = CommandFlags.None)
        {
            return this.Inner.StringGetWithExpiryAsync(this.ToInner(key), flags);
        }

        public Task<double> StringIncrementAsync(RedisKey key, double value, CommandFlags flags = CommandFlags.None)
        {
            return this.Inner.StringIncrementAsync(this.ToInner(key), value, flags);
        }

        public Task<long> StringIncrementAsync(RedisKey key, long value = 1, CommandFlags flags = CommandFlags.None)
        {
            return this.Inner.StringIncrementAsync(this.ToInner(key), value, flags);
        }

        public Task<long> StringLengthAsync(RedisKey key, CommandFlags flags = CommandFlags.None)
        {
            return this.Inner.StringLengthAsync(this.ToInner(key), flags);
        }

        public Task<bool> StringSetAsync(KeyValuePair<RedisKey, RedisValue>[] values, When when = When.Always, CommandFlags flags = CommandFlags.None)
        {
            return this.Inner.StringSetAsync(this.ToInner(values), when, flags);
        }

        public Task<bool> StringSetAsync(RedisKey key, RedisValue value, TimeSpan? expiry = null, When when = When.Always, CommandFlags flags = CommandFlags.None)
        {
            return this.Inner.StringSetAsync(this.ToInner(key), value, expiry, when, flags);
        }

        public Task<bool> StringSetBitAsync(RedisKey key, long offset, bool bit, CommandFlags flags = CommandFlags.None)
        {
            return this.Inner.StringSetBitAsync(this.ToInner(key), offset, bit, flags);
        }

        public Task<RedisValue> StringSetRangeAsync(RedisKey key, long offset, RedisValue value, CommandFlags flags = CommandFlags.None)
        {
            return this.Inner.StringSetRangeAsync(this.ToInner(key), offset, value, flags);
        }

        public Task<TimeSpan> PingAsync(CommandFlags flags = CommandFlags.None)
        {
            return this.Inner.PingAsync(flags);
        }

        public bool TryWait(Task task)
        {
            return this.Inner.TryWait(task);
        }

        public TResult Wait<TResult>(Task<TResult> task)
        {
            return this.Inner.Wait(task);
        }

        public void Wait(Task task)
        {
            this.Inner.Wait(task);
        }

        public void WaitAll(params Task[] tasks)
        {
            this.Inner.WaitAll(tasks);
        }

#if DEBUG
        public Task<string> ClientGetNameAsync(CommandFlags flags = CommandFlags.None)
        {
            return this.Inner.ClientGetNameAsync(flags);
        }
#endif

        protected internal RedisKey ToInner(RedisKey outer)
        {
            return RedisKey.WithPrefix(_keyPrefix, outer);
        }

        protected RedisKey ToInnerOrDefault(RedisKey outer)
        {
            if (outer == default(RedisKey))
            {
                return outer;
            }
            else
            {
                return this.ToInner(outer);
            }
        }

        protected RedisKey[] ToInner(RedisKey[] outer)
        {
            if (outer == null || outer.Length == 0)
            {
                return outer;
            }
            else
            {
                RedisKey[] inner = new RedisKey[outer.Length];

                for (int i = 0; i < outer.Length; ++i)
                {
                    inner[i] = this.ToInner(outer[i]);
                }

                return inner;
            }
        }

        protected KeyValuePair<RedisKey, RedisValue> ToInner(KeyValuePair<RedisKey, RedisValue> outer)
        {
            return new KeyValuePair<RedisKey, RedisValue>(this.ToInner(outer.Key), outer.Value);
        }

        protected KeyValuePair<RedisKey, RedisValue>[] ToInner(KeyValuePair<RedisKey, RedisValue>[] outer)
        {
            if (outer == null || outer.Length == 0)
            {
                return outer;
            }
            else
            {
                KeyValuePair<RedisKey, RedisValue>[] inner = new KeyValuePair<RedisKey, RedisValue>[outer.Length];

                for (int i = 0; i < outer.Length; ++i)
                {
                    inner[i] = this.ToInner(outer[i]);
                }

                return inner;
            }
        }

        protected RedisValue ToInner(RedisValue outer)
        {
            return RedisKey.ConcatenateBytes(this.Prefix, null, (byte[])outer);
        }

        protected RedisValue SortByToInner(RedisValue outer)
        {
            if (outer == "nosort")
            {
                return outer;
            }
            else
            {
                return this.ToInner(outer);
            }
        }

        protected RedisValue SortGetToInner(RedisValue outer)
        {
            if (outer == "#")
            {
                return outer;
            }
            else
            {
                return this.ToInner(outer);
            }
        }

        protected RedisValue[] SortGetToInner(RedisValue[] outer)
        {
            if (outer == null || outer.Length == 0)
            {
                return outer;
            }
            else
            {
                RedisValue[] inner = new RedisValue[outer.Length];

                for (int i = 0; i < outer.Length; ++i)
                {
                    inner[i] = this.SortGetToInner(outer[i]);
                }

                return inner;
            }
        }

        protected RedisChannel ToInner(RedisChannel outer)
        {
            return RedisKey.ConcatenateBytes(this.Prefix, null, (byte[])outer);
        }

        private Func<RedisKey, RedisKey> mapFunction;
        protected Func<RedisKey, RedisKey> GetMapFunction()
        {
            // create as a delegate when first required, then re-use
            return mapFunction ?? (mapFunction = new Func<RedisKey, RedisKey>(this.ToInner)); 
        }
    }
}
