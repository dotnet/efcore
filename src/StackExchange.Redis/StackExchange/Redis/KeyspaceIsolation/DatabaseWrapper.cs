using System;
using System.Collections.Generic;
using System.Net;

namespace StackExchange.Redis.KeyspaceIsolation
{
    internal sealed class DatabaseWrapper : WrapperBase<IDatabase>, IDatabase
    {
        public DatabaseWrapper(IDatabase inner, byte[] prefix)
            : base(inner, prefix)
        {
        }

        public IBatch CreateBatch(object asyncState = null)
        {
            return new BatchWrapper(this.Inner.CreateBatch(asyncState), this.Prefix);
        }

        public ITransaction CreateTransaction(object asyncState = null)
        {
            return new TransactionWrapper(this.Inner.CreateTransaction(asyncState), this.Prefix);
        }

        public int Database
        {
            get { return this.Inner.Database; }
        }

        public RedisValue DebugObject(RedisKey key, CommandFlags flags = CommandFlags.None)
        {
            return this.Inner.DebugObject(this.ToInner(key), flags);
        }

        public double HashDecrement(RedisKey key, RedisValue hashField, double value, CommandFlags flags = CommandFlags.None)
        {
            return this.Inner.HashDecrement(this.ToInner(key), hashField, value, flags);
        }

        public long HashDecrement(RedisKey key, RedisValue hashField, long value = 1, CommandFlags flags = CommandFlags.None)
        {
            return this.Inner.HashDecrement(this.ToInner(key), hashField, value, flags);
        }

        public long HashDelete(RedisKey key, RedisValue[] hashFields, CommandFlags flags = CommandFlags.None)
        {
            return this.Inner.HashDelete(this.ToInner(key), hashFields, flags);
        }

        public bool HashDelete(RedisKey key, RedisValue hashField, CommandFlags flags = CommandFlags.None)
        {
            return this.Inner.HashDelete(this.ToInner(key), hashField, flags);
        }

        public bool HashExists(RedisKey key, RedisValue hashField, CommandFlags flags = CommandFlags.None)
        {
            return this.Inner.HashExists(this.ToInner(key), hashField, flags);
        }

        public HashEntry[] HashGetAll(RedisKey key, CommandFlags flags = CommandFlags.None)
        {
            return this.Inner.HashGetAll(this.ToInner(key), flags);
        }

        public RedisValue[] HashGet(RedisKey key, RedisValue[] hashFields, CommandFlags flags = CommandFlags.None)
        {
            return this.Inner.HashGet(this.ToInner(key), hashFields, flags);
        }

        public RedisValue HashGet(RedisKey key, RedisValue hashField, CommandFlags flags = CommandFlags.None)
        {
            return this.Inner.HashGet(this.ToInner(key), hashField, flags);
        }

        public double HashIncrement(RedisKey key, RedisValue hashField, double value, CommandFlags flags = CommandFlags.None)
        {
            return this.Inner.HashIncrement(this.ToInner(key), hashField, value, flags);
        }

        public long HashIncrement(RedisKey key, RedisValue hashField, long value = 1, CommandFlags flags = CommandFlags.None)
        {
            return this.Inner.HashIncrement(this.ToInner(key), hashField, value, flags);
        }

        public RedisValue[] HashKeys(RedisKey key, CommandFlags flags = CommandFlags.None)
        {
            return this.Inner.HashKeys(this.ToInner(key), flags);
        }

        public long HashLength(RedisKey key, CommandFlags flags = CommandFlags.None)
        {
            return this.Inner.HashLength(this.ToInner(key), flags);
        }

        public bool HashSet(RedisKey key, RedisValue hashField, RedisValue value, When when = When.Always, CommandFlags flags = CommandFlags.None)
        {
            return this.Inner.HashSet(this.ToInner(key), hashField, value, when, flags);
        }

        public void HashSet(RedisKey key, HashEntry[] hashFields, CommandFlags flags = CommandFlags.None)
        {
            this.Inner.HashSet(this.ToInner(key), hashFields, flags);
        }

        public RedisValue[] HashValues(RedisKey key, CommandFlags flags = CommandFlags.None)
        {
            return this.Inner.HashValues(this.ToInner(key), flags);
        }

        public bool HyperLogLogAdd(RedisKey key, RedisValue[] values, CommandFlags flags = CommandFlags.None)
        {
            return this.Inner.HyperLogLogAdd(this.ToInner(key), values, flags);
        }

        public bool HyperLogLogAdd(RedisKey key, RedisValue value, CommandFlags flags = CommandFlags.None)
        {
            return this.Inner.HyperLogLogAdd(this.ToInner(key), value, flags);
        }

        public long HyperLogLogLength(RedisKey key, CommandFlags flags = CommandFlags.None)
        {
            return this.Inner.HyperLogLogLength(this.ToInner(key), flags);
        }

        public long HyperLogLogLength(RedisKey[] keys, CommandFlags flags = CommandFlags.None)
        {
            return this.Inner.HyperLogLogLength(this.ToInner(keys), flags);
        }

        public void HyperLogLogMerge(RedisKey destination, RedisKey[] sourceKeys, CommandFlags flags = CommandFlags.None)
        {
            this.Inner.HyperLogLogMerge(this.ToInner(destination), this.ToInner(sourceKeys), flags);
        }

        public void HyperLogLogMerge(RedisKey destination, RedisKey first, RedisKey second, CommandFlags flags = CommandFlags.None)
        {
            this.Inner.HyperLogLogMerge(this.ToInner(destination), this.ToInner(first), this.ToInner(second), flags);
        }

        public EndPoint IdentifyEndpoint(RedisKey key = default(RedisKey), CommandFlags flags = CommandFlags.None)
        {
            return this.Inner.IdentifyEndpoint(this.ToInner(key), flags);
        }

        public long KeyDelete(RedisKey[] keys, CommandFlags flags = CommandFlags.None)
        {
            return this.Inner.KeyDelete(this.ToInner(keys), flags);
        }

        public bool KeyDelete(RedisKey key, CommandFlags flags = CommandFlags.None)
        {
            return this.Inner.KeyDelete(this.ToInner(key), flags);
        }

        public byte[] KeyDump(RedisKey key, CommandFlags flags = CommandFlags.None)
        {
            return this.Inner.KeyDump(this.ToInner(key), flags);
        }

        public bool KeyExists(RedisKey key, CommandFlags flags = CommandFlags.None)
        {
            return this.Inner.KeyExists(this.ToInner(key), flags);
        }

        public bool KeyExpire(RedisKey key, DateTime? expiry, CommandFlags flags = CommandFlags.None)
        {
            return this.Inner.KeyExpire(this.ToInner(key), expiry, flags);
        }

        public bool KeyExpire(RedisKey key, TimeSpan? expiry, CommandFlags flags = CommandFlags.None)
        {
            return this.Inner.KeyExpire(this.ToInner(key), expiry, flags);
        }

        public void KeyMigrate(RedisKey key, EndPoint toServer, int toDatabase = 0, int timeoutMilliseconds = 0, MigrateOptions migrateOptions = MigrateOptions.None, CommandFlags flags = CommandFlags.None)
        {
            this.Inner.KeyMigrate(this.ToInner(key), toServer, toDatabase, timeoutMilliseconds, migrateOptions, flags);
        }

        public bool KeyMove(RedisKey key, int database, CommandFlags flags = CommandFlags.None)
        {
            return this.Inner.KeyMove(this.ToInner(key), database, flags);
        }

        public bool KeyPersist(RedisKey key, CommandFlags flags = CommandFlags.None)
        {
            return this.Inner.KeyPersist(this.ToInner(key), flags);
        }

        public RedisKey KeyRandom(CommandFlags flags = CommandFlags.None)
        {
            throw new NotSupportedException("RANDOMKEY is not supported when a key-prefix is specified");
        }

        public bool KeyRename(RedisKey key, RedisKey newKey, When when = When.Always, CommandFlags flags = CommandFlags.None)
        {
            return this.Inner.KeyRename(this.ToInner(key), this.ToInner(newKey), when, flags);
        }

        public void KeyRestore(RedisKey key, byte[] value, TimeSpan? expiry = null, CommandFlags flags = CommandFlags.None)
        {
            this.Inner.KeyRestore(this.ToInner(key), value, expiry, flags);
        }

        public TimeSpan? KeyTimeToLive(RedisKey key, CommandFlags flags = CommandFlags.None)
        {
            return this.Inner.KeyTimeToLive(this.ToInner(key), flags);
        }

        public RedisType KeyType(RedisKey key, CommandFlags flags = CommandFlags.None)
        {
            return this.Inner.KeyType(this.ToInner(key), flags);
        }

        public RedisValue ListGetByIndex(RedisKey key, long index, CommandFlags flags = CommandFlags.None)
        {
            return this.Inner.ListGetByIndex(this.ToInner(key), index, flags);
        }

        public long ListInsertAfter(RedisKey key, RedisValue pivot, RedisValue value, CommandFlags flags = CommandFlags.None)
        {
            return this.Inner.ListInsertAfter(this.ToInner(key), pivot, value, flags);
        }

        public long ListInsertBefore(RedisKey key, RedisValue pivot, RedisValue value, CommandFlags flags = CommandFlags.None)
        {
            return this.Inner.ListInsertBefore(this.ToInner(key), pivot, value, flags);
        }

        public RedisValue ListLeftPop(RedisKey key, CommandFlags flags = CommandFlags.None)
        {
            return this.Inner.ListLeftPop(this.ToInner(key), flags);
        }

        public long ListLeftPush(RedisKey key, RedisValue[] values, CommandFlags flags = CommandFlags.None)
        {
            return this.Inner.ListLeftPush(this.ToInner(key), values, flags);
        }

        public long ListLeftPush(RedisKey key, RedisValue value, When when = When.Always, CommandFlags flags = CommandFlags.None)
        {
            return this.Inner.ListLeftPush(this.ToInner(key), value, when, flags);
        }

        public long ListLength(RedisKey key, CommandFlags flags = CommandFlags.None)
        {
            return this.Inner.ListLength(this.ToInner(key), flags);
        }

        public RedisValue[] ListRange(RedisKey key, long start = 0, long stop = -1, CommandFlags flags = CommandFlags.None)
        {
            return this.Inner.ListRange(this.ToInner(key), start, stop, flags);
        }

        public long ListRemove(RedisKey key, RedisValue value, long count = 0, CommandFlags flags = CommandFlags.None)
        {
            return this.Inner.ListRemove(this.ToInner(key), value, count, flags);
        }

        public RedisValue ListRightPop(RedisKey key, CommandFlags flags = CommandFlags.None)
        {
            return this.Inner.ListRightPop(this.ToInner(key), flags);
        }

        public RedisValue ListRightPopLeftPush(RedisKey source, RedisKey destination, CommandFlags flags = CommandFlags.None)
        {
            return this.Inner.ListRightPopLeftPush(this.ToInner(source), this.ToInner(destination), flags);
        }

        public long ListRightPush(RedisKey key, RedisValue[] values, CommandFlags flags = CommandFlags.None)
        {
            return this.Inner.ListRightPush(this.ToInner(key), values, flags);
        }

        public long ListRightPush(RedisKey key, RedisValue value, When when = When.Always, CommandFlags flags = CommandFlags.None)
        {
            return this.Inner.ListRightPush(this.ToInner(key), value, when, flags);
        }

        public void ListSetByIndex(RedisKey key, long index, RedisValue value, CommandFlags flags = CommandFlags.None)
        {
            this.Inner.ListSetByIndex(this.ToInner(key), index, value, flags);
        }

        public void ListTrim(RedisKey key, long start, long stop, CommandFlags flags = CommandFlags.None)
        {
            this.Inner.ListTrim(this.ToInner(key), start, stop, flags);
        }

        public bool LockExtend(RedisKey key, RedisValue value, TimeSpan expiry, CommandFlags flags = CommandFlags.None)
        {
            return this.Inner.LockExtend(this.ToInner(key), value, expiry, flags);
        }

        public RedisValue LockQuery(RedisKey key, CommandFlags flags = CommandFlags.None)
        {
            return this.Inner.LockQuery(this.ToInner(key), flags);
        }

        public bool LockRelease(RedisKey key, RedisValue value, CommandFlags flags = CommandFlags.None)
        {
            return this.Inner.LockRelease(this.ToInner(key), value, flags);
        }

        public bool LockTake(RedisKey key, RedisValue value, TimeSpan expiry, CommandFlags flags = CommandFlags.None)
        {
            return this.Inner.LockTake(this.ToInner(key), value, expiry, flags);
        }

        public long Publish(RedisChannel channel, RedisValue message, CommandFlags flags = CommandFlags.None)
        {
            return this.Inner.Publish(this.ToInner(channel), message, flags);
        }

        public RedisResult ScriptEvaluate(byte[] hash, RedisKey[] keys = null, RedisValue[] values = null, CommandFlags flags = CommandFlags.None)
        {
            // TODO: The return value could contain prefixed keys. It might make sense to 'unprefix' those?
            return this.Inner.ScriptEvaluate(hash, this.ToInner(keys), values, flags);
        }

        public RedisResult ScriptEvaluate(string script, RedisKey[] keys = null, RedisValue[] values = null, CommandFlags flags = CommandFlags.None)
        {
            // TODO: The return value could contain prefixed keys. It might make sense to 'unprefix' those?
            return this.Inner.ScriptEvaluate(script, this.ToInner(keys), values, flags);
        }

        public long SetAdd(RedisKey key, RedisValue[] values, CommandFlags flags = CommandFlags.None)
        {
            return this.Inner.SetAdd(this.ToInner(key), values, flags);
        }

        public bool SetAdd(RedisKey key, RedisValue value, CommandFlags flags = CommandFlags.None)
        {
            return this.Inner.SetAdd(this.ToInner(key), value, flags);
        }

        public long SetCombineAndStore(SetOperation operation, RedisKey destination, RedisKey[] keys, CommandFlags flags = CommandFlags.None)
        {
            return this.Inner.SetCombineAndStore(operation, this.ToInner(destination), this.ToInner(keys), flags);
        }

        public long SetCombineAndStore(SetOperation operation, RedisKey destination, RedisKey first, RedisKey second, CommandFlags flags = CommandFlags.None)
        {
            return this.Inner.SetCombineAndStore(operation, this.ToInner(destination), this.ToInner(first), this.ToInner(second), flags);
        }

        public RedisValue[] SetCombine(SetOperation operation, RedisKey[] keys, CommandFlags flags = CommandFlags.None)
        {
            return this.Inner.SetCombine(operation, this.ToInner(keys), flags);
        }

        public RedisValue[] SetCombine(SetOperation operation, RedisKey first, RedisKey second, CommandFlags flags = CommandFlags.None)
        {
            return this.Inner.SetCombine(operation, this.ToInner(first), this.ToInner(second), flags);
        }

        public bool SetContains(RedisKey key, RedisValue value, CommandFlags flags = CommandFlags.None)
        {
            return this.Inner.SetContains(this.ToInner(key), value, flags);
        }

        public long SetLength(RedisKey key, CommandFlags flags = CommandFlags.None)
        {
            return this.Inner.SetLength(this.ToInner(key), flags);
        }

        public RedisValue[] SetMembers(RedisKey key, CommandFlags flags = CommandFlags.None)
        {
            return this.Inner.SetMembers(this.ToInner(key), flags);
        }

        public bool SetMove(RedisKey source, RedisKey destination, RedisValue value, CommandFlags flags = CommandFlags.None)
        {
            return this.Inner.SetMove(this.ToInner(source), this.ToInner(destination), value, flags);
        }

        public RedisValue SetPop(RedisKey key, CommandFlags flags = CommandFlags.None)
        {
            return this.Inner.SetPop(this.ToInner(key), flags);
        }

        public RedisValue SetRandomMember(RedisKey key, CommandFlags flags = CommandFlags.None)
        {
            return this.Inner.SetRandomMember(this.ToInner(key), flags);
        }

        public RedisValue[] SetRandomMembers(RedisKey key, long count, CommandFlags flags = CommandFlags.None)
        {
            return this.Inner.SetRandomMembers(this.ToInner(key), count, flags);
        }

        public long SetRemove(RedisKey key, RedisValue[] values, CommandFlags flags = CommandFlags.None)
        {
            return this.Inner.SetRemove(this.ToInner(key), values, flags);
        }

        public bool SetRemove(RedisKey key, RedisValue value, CommandFlags flags = CommandFlags.None)
        {
            return this.Inner.SetRemove(this.ToInner(key), value, flags);
        }

        public long SortAndStore(RedisKey destination, RedisKey key, long skip = 0, long take = -1, Order order = Order.Ascending, SortType sortType = SortType.Numeric, RedisValue by = default(RedisValue), RedisValue[] get = null, CommandFlags flags = CommandFlags.None)
        {
            return this.Inner.SortAndStore(this.ToInner(destination), this.ToInner(key), skip, take, order, sortType, this.SortByToInner(by), this.SortGetToInner(get), flags);
        }

        public RedisValue[] Sort(RedisKey key, long skip = 0, long take = -1, Order order = Order.Ascending, SortType sortType = SortType.Numeric, RedisValue by = default(RedisValue), RedisValue[] get = null, CommandFlags flags = CommandFlags.None)
        {
            return this.Inner.Sort(this.ToInner(key), skip, take, order, sortType, this.SortByToInner(by), this.SortGetToInner(get), flags);
        }

        public long SortedSetAdd(RedisKey key, SortedSetEntry[] values, CommandFlags flags = CommandFlags.None)
        {
            return this.Inner.SortedSetAdd(this.ToInner(key), values, flags);
        }

        public bool SortedSetAdd(RedisKey key, RedisValue member, double score, CommandFlags flags = CommandFlags.None)
        {
            return this.Inner.SortedSetAdd(this.ToInner(key), member, score, flags);
        }

        public long SortedSetCombineAndStore(SetOperation operation, RedisKey destination, RedisKey[] keys, double[] weights = null, Aggregate aggregate = Aggregate.Sum, CommandFlags flags = CommandFlags.None)
        {
            return this.Inner.SortedSetCombineAndStore(operation, this.ToInner(destination), this.ToInner(keys), weights, aggregate, flags);
        }

        public long SortedSetCombineAndStore(SetOperation operation, RedisKey destination, RedisKey first, RedisKey second, Aggregate aggregate = Aggregate.Sum, CommandFlags flags = CommandFlags.None)
        {
            return this.Inner.SortedSetCombineAndStore(operation, this.ToInner(destination), this.ToInner(first), this.ToInner(second), aggregate, flags);
        }

        public double SortedSetDecrement(RedisKey key, RedisValue member, double value, CommandFlags flags = CommandFlags.None)
        {
            return this.Inner.SortedSetDecrement(this.ToInner(key), member, value, flags);
        }

        public double SortedSetIncrement(RedisKey key, RedisValue member, double value, CommandFlags flags = CommandFlags.None)
        {
            return this.Inner.SortedSetIncrement(this.ToInner(key), member, value, flags);
        }

        public long SortedSetLength(RedisKey key, double min = -1.0 / 0.0, double max = 1.0 / 0.0, Exclude exclude = Exclude.None, CommandFlags flags = CommandFlags.None)
        {
            return this.Inner.SortedSetLength(this.ToInner(key), min, max, exclude, flags);
        }

        public long SortedSetLengthByValue(RedisKey key, RedisValue min, RedisValue max, Exclude exclude = Exclude.None, CommandFlags flags = CommandFlags.None)
        {
            return this.Inner.SortedSetLengthByValue(this.ToInner(key), min, max, exclude, flags);
        }

        public RedisValue[] SortedSetRangeByRank(RedisKey key, long start = 0, long stop = -1, Order order = Order.Ascending, CommandFlags flags = CommandFlags.None)
        {
            return this.Inner.SortedSetRangeByRank(this.ToInner(key), start, stop, order, flags);
        }

        public SortedSetEntry[] SortedSetRangeByRankWithScores(RedisKey key, long start = 0, long stop = -1, Order order = Order.Ascending, CommandFlags flags = CommandFlags.None)
        {
            return this.Inner.SortedSetRangeByRankWithScores(this.ToInner(key), start, stop, order, flags);
        }

        public RedisValue[] SortedSetRangeByScore(RedisKey key, double start = -1.0 / 0.0, double stop = 1.0 / 0.0, Exclude exclude = Exclude.None, Order order = Order.Ascending, long skip = 0, long take = -1, CommandFlags flags = CommandFlags.None)
        {
            return this.Inner.SortedSetRangeByScore(this.ToInner(key), start, stop, exclude, order, skip, take, flags);
        }

        public SortedSetEntry[] SortedSetRangeByScoreWithScores(RedisKey key, double start = -1.0 / 0.0, double stop = 1.0 / 0.0, Exclude exclude = Exclude.None, Order order = Order.Ascending, long skip = 0, long take = -1, CommandFlags flags = CommandFlags.None)
        {
            return this.Inner.SortedSetRangeByScoreWithScores(this.ToInner(key), start, stop, exclude, order, skip, take, flags);
        }

        public RedisValue[] SortedSetRangeByValue(RedisKey key, RedisValue min = default(RedisValue), RedisValue max = default(RedisValue), Exclude exclude = Exclude.None, long skip = 0, long take = -1, CommandFlags flags = CommandFlags.None)
        {
            return this.Inner.SortedSetRangeByValue(this.ToInner(key), min, max, exclude, skip, take, flags);
        }

        public long? SortedSetRank(RedisKey key, RedisValue member, Order order = Order.Ascending, CommandFlags flags = CommandFlags.None)
        {
            return this.Inner.SortedSetRank(this.ToInner(key), member, order, flags);
        }

        public long SortedSetRemove(RedisKey key, RedisValue[] members, CommandFlags flags = CommandFlags.None)
        {
            return this.Inner.SortedSetRemove(this.ToInner(key), members, flags);
        }

        public bool SortedSetRemove(RedisKey key, RedisValue member, CommandFlags flags = CommandFlags.None)
        {
            return this.Inner.SortedSetRemove(this.ToInner(key), member, flags);
        }

        public long SortedSetRemoveRangeByRank(RedisKey key, long start, long stop, CommandFlags flags = CommandFlags.None)
        {
            return this.Inner.SortedSetRemoveRangeByRank(this.ToInner(key), start, stop, flags);
        }

        public long SortedSetRemoveRangeByScore(RedisKey key, double start, double stop, Exclude exclude = Exclude.None, CommandFlags flags = CommandFlags.None)
        {
            return this.Inner.SortedSetRemoveRangeByScore(this.ToInner(key), start, stop, exclude, flags);
        }

        public long SortedSetRemoveRangeByValue(RedisKey key, RedisValue min, RedisValue max, Exclude exclude = Exclude.None, CommandFlags flags = CommandFlags.None)
        {
            return this.Inner.SortedSetRemoveRangeByValue(this.ToInner(key), min, max, exclude, flags);
        }

        public double? SortedSetScore(RedisKey key, RedisValue member, CommandFlags flags = CommandFlags.None)
        {
            return this.Inner.SortedSetScore(this.ToInner(key), member, flags);
        }

        public long StringAppend(RedisKey key, RedisValue value, CommandFlags flags = CommandFlags.None)
        {
            return this.Inner.StringAppend(this.ToInner(key), value, flags);
        }

        public long StringBitCount(RedisKey key, long start = 0, long end = -1, CommandFlags flags = CommandFlags.None)
        {
            return this.Inner.StringBitCount(this.ToInner(key), start, end, flags);
        }

        public long StringBitOperation(Bitwise operation, RedisKey destination, RedisKey[] keys, CommandFlags flags = CommandFlags.None)
        {
            return this.Inner.StringBitOperation(operation, this.ToInner(destination), this.ToInner(keys), flags);
        }

        public long StringBitOperation(Bitwise operation, RedisKey destination, RedisKey first, RedisKey second = default(RedisKey), CommandFlags flags = CommandFlags.None)
        {
            return this.Inner.StringBitOperation(operation, this.ToInner(destination), this.ToInner(first), this.ToInnerOrDefault(second), flags);
        }

        public long StringBitPosition(RedisKey key, bool bit, long start = 0, long end = -1, CommandFlags flags = CommandFlags.None)
        {
            return this.Inner.StringBitPosition(this.ToInner(key), bit, start, end, flags);
        }

        public double StringDecrement(RedisKey key, double value, CommandFlags flags = CommandFlags.None)
        {
            return this.Inner.StringDecrement(this.ToInner(key), value, flags);
        }

        public long StringDecrement(RedisKey key, long value = 1, CommandFlags flags = CommandFlags.None)
        {
            return this.Inner.StringDecrement(this.ToInner(key), value, flags);
        }

        public RedisValue[] StringGet(RedisKey[] keys, CommandFlags flags = CommandFlags.None)
        {
            return this.Inner.StringGet(this.ToInner(keys), flags);
        }

        public RedisValue StringGet(RedisKey key, CommandFlags flags = CommandFlags.None)
        {
            return this.Inner.StringGet(this.ToInner(key), flags);
        }

        public bool StringGetBit(RedisKey key, long offset, CommandFlags flags = CommandFlags.None)
        {
            return this.Inner.StringGetBit(this.ToInner(key), offset, flags);
        }

        public RedisValue StringGetRange(RedisKey key, long start, long end, CommandFlags flags = CommandFlags.None)
        {
            return this.Inner.StringGetRange(this.ToInner(key), start, end, flags);
        }

        public RedisValue StringGetSet(RedisKey key, RedisValue value, CommandFlags flags = CommandFlags.None)
        {
            return this.Inner.StringGetSet(this.ToInner(key), value, flags);
        }

        public RedisValueWithExpiry StringGetWithExpiry(RedisKey key, CommandFlags flags = CommandFlags.None)
        {
            return this.Inner.StringGetWithExpiry(this.ToInner(key), flags);
        }

        public double StringIncrement(RedisKey key, double value, CommandFlags flags = CommandFlags.None)
        {
            return this.Inner.StringIncrement(this.ToInner(key), value, flags);
        }

        public long StringIncrement(RedisKey key, long value = 1, CommandFlags flags = CommandFlags.None)
        {
            return this.Inner.StringIncrement(this.ToInner(key), value, flags);
        }

        public long StringLength(RedisKey key, CommandFlags flags = CommandFlags.None)
        {
            return this.Inner.StringLength(this.ToInner(key), flags);
        }

        public bool StringSet(KeyValuePair<RedisKey, RedisValue>[] values, When when = When.Always, CommandFlags flags = CommandFlags.None)
        {
            return this.Inner.StringSet(this.ToInner(values), when, flags);
        }

        public bool StringSet(RedisKey key, RedisValue value, TimeSpan? expiry = null, When when = When.Always, CommandFlags flags = CommandFlags.None)
        {
            return this.Inner.StringSet(this.ToInner(key), value, expiry, when, flags);
        }

        public bool StringSetBit(RedisKey key, long offset, bool bit, CommandFlags flags = CommandFlags.None)
        {
            return this.Inner.StringSetBit(this.ToInner(key), offset, bit, flags);
        }

        public RedisValue StringSetRange(RedisKey key, long offset, RedisValue value, CommandFlags flags = CommandFlags.None)
        {
            return this.Inner.StringSetRange(this.ToInner(key), offset, value, flags);
        }

        public TimeSpan Ping(CommandFlags flags = CommandFlags.None)
        {
            return this.Inner.Ping(flags);
        }


        IEnumerable<HashEntry> IDatabase.HashScan(RedisKey key, RedisValue pattern, int pageSize, CommandFlags flags)
        {
            return HashScan(key, pattern, pageSize, RedisBase.CursorUtils.Origin, 0, flags);
        }
        public IEnumerable<HashEntry> HashScan(RedisKey key, RedisValue pattern = default(RedisValue), int pageSize = RedisBase.CursorUtils.DefaultPageSize, long cursor = RedisBase.CursorUtils.Origin, int pageOffset = 0, CommandFlags flags = CommandFlags.None)
        {
            return this.Inner.HashScan(this.ToInner(key), pattern, pageSize, cursor, pageOffset, flags);
        }

        IEnumerable<RedisValue> IDatabase.SetScan(RedisKey key, RedisValue pattern, int pageSize, CommandFlags flags)
        {
            return SetScan(key, pattern, pageSize, RedisBase.CursorUtils.Origin, 0, flags);
        }
        public IEnumerable<RedisValue> SetScan(RedisKey key, RedisValue pattern = default(RedisValue), int pageSize = RedisBase.CursorUtils.DefaultPageSize, long cursor = RedisBase.CursorUtils.Origin, int pageOffset = 0, CommandFlags flags = CommandFlags.None)
        {
            return this.Inner.SetScan(this.ToInner(key), pattern, pageSize, cursor, pageOffset, flags);
        }

        IEnumerable<SortedSetEntry> IDatabase.SortedSetScan(RedisKey key, RedisValue pattern, int pageSize, CommandFlags flags)
        {
            return SortedSetScan(key, pattern, pageSize, RedisBase.CursorUtils.Origin, 0, flags);
        }
        public IEnumerable<SortedSetEntry> SortedSetScan(RedisKey key, RedisValue pattern = default(RedisValue), int pageSize = RedisBase.CursorUtils.DefaultPageSize, long cursor = RedisBase.CursorUtils.Origin, int pageOffset = 0, CommandFlags flags = CommandFlags.None)
        {
            return this.Inner.SortedSetScan(this.ToInner(key), pattern, pageSize, cursor, pageOffset, flags);
        }


#if DEBUG
        public string ClientGetName(CommandFlags flags = CommandFlags.None)
        {
            return this.Inner.ClientGetName(flags);
        }

        public void Quit(CommandFlags flags = CommandFlags.None)
        {
            this.Inner.Quit(flags);
        }
#endif
    }
}
