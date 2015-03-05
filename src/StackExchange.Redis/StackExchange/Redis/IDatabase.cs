using System;
using System.Collections.Generic;
using System.Net;

namespace StackExchange.Redis
{

    /// <summary>
    /// Describes functionality that is common to both standalone redis servers and redis clusters
    /// </summary>
    public interface IDatabase : IRedis, IDatabaseAsync
    {

        /// <summary>
        /// The numeric identifier of this database
        /// </summary>
        int Database { get; }

        /// <summary>
        /// Allows creation of a group of operations that will be sent to the server as a single unit,
        /// but which may or may not be processed on the server contiguously.
        /// </summary>
        [IgnoreNamePrefix]
        IBatch CreateBatch(object asyncState = null);


        /// <summary>
        /// Atomically transfer a key from a source Redis instance to a destination Redis instance. On success the key is deleted from the original instance by default, and is guaranteed to exist in the target instance.
        /// </summary>
        /// <remarks>http://redis.io/commands/MIGRATE</remarks>
        void KeyMigrate(RedisKey key, EndPoint toServer, int toDatabase = 0, int timeoutMilliseconds = 0, MigrateOptions migrateOptions = MigrateOptions.None, CommandFlags flags = CommandFlags.None);


        /// <summary>
        /// Allows creation of a group of operations that will be sent to the server as a single unit,
        /// and processed on the server as a single unit.
        /// </summary>
        [IgnoreNamePrefix]
        ITransaction CreateTransaction(object asyncState = null);

        /// <summary>
        /// Returns the raw DEBUG OBJECT output for a key; this command is not fully documented and should be avoided unless you have good reason, and then avoided anyway.
        /// </summary>
        /// <remarks>http://redis.io/commands/debug-object</remarks>
        RedisValue DebugObject(RedisKey key, CommandFlags flags = CommandFlags.None);

        /// <summary>
        /// Decrements the number stored at field in the hash stored at key by decrement. If key does not exist, a new key holding a hash is created. If field does not exist or holds a string that cannot be interpreted as integer, the value is set to 0 before the operation is performed.
        /// </summary>
        /// <remarks>The range of values supported by HINCRBY is limited to 64 bit signed integers.</remarks>
        /// <returns>the value at field after the decrement operation.</returns>
        /// <remarks>http://redis.io/commands/hincrby</remarks>
        long HashDecrement(RedisKey key, RedisValue hashField, long value = 1, CommandFlags flags = CommandFlags.None);

        /// <summary>
        /// Decrement the specified field of an hash stored at key, and representing a floating point number, by the specified decrement. If the field does not exist, it is set to 0 before performing the operation.
        /// </summary>
        /// <remarks>The precision of the output is fixed at 17 digits after the decimal point regardless of the actual internal precision of the computation.</remarks>
        /// <returns>the value at field after the decrement operation.</returns>
        /// <remarks>http://redis.io/commands/hincrbyfloat</remarks>
        double HashDecrement(RedisKey key, RedisValue hashField, double value, CommandFlags flags = CommandFlags.None);

        /// <summary>
        /// Removes the specified fields from the hash stored at key. Non-existing fields are ignored. Non-existing keys are treated as empty hashes and this command returns 0.
        /// </summary>
        /// <remarks>http://redis.io/commands/hdel</remarks>
        /// <returns>The number of fields that were removed.</returns>
        bool HashDelete(RedisKey key, RedisValue hashField, CommandFlags flags = CommandFlags.None);

        /// <summary>
        /// Removes the specified fields from the hash stored at key. Non-existing fields are ignored. Non-existing keys are treated as empty hashes and this command returns 0.
        /// </summary>
        /// <remarks>http://redis.io/commands/hdel</remarks>
        /// <returns>The number of fields that were removed.</returns>
        long HashDelete(RedisKey key, RedisValue[] hashFields, CommandFlags flags = CommandFlags.None);

        /// <summary>
        /// Returns if field is an existing field in the hash stored at key.
        /// </summary>
        /// <returns>1 if the hash contains field. 0 if the hash does not contain field, or key does not exist.</returns>
        /// <remarks>http://redis.io/commands/hexists</remarks>
        bool HashExists(RedisKey key, RedisValue hashField, CommandFlags flags = CommandFlags.None);

        /// <summary>
        /// Returns the value associated with field in the hash stored at key.
        /// </summary>
        /// <returns>the value associated with field, or nil when field is not present in the hash or key does not exist.</returns>
        /// <remarks>http://redis.io/commands/hget</remarks>
        RedisValue HashGet(RedisKey key, RedisValue hashField, CommandFlags flags = CommandFlags.None);

        /// <summary>
        /// Returns the values associated with the specified fields in the hash stored at key.
        /// For every field that does not exist in the hash, a nil value is returned.Because a non-existing keys are treated as empty hashes, running HMGET against a non-existing key will return a list of nil values.
        /// </summary>
        /// <returns>list of values associated with the given fields, in the same order as they are requested.</returns>
        /// <remarks>http://redis.io/commands/hmget</remarks>
        RedisValue[] HashGet(RedisKey key, RedisValue[] hashFields, CommandFlags flags = CommandFlags.None);

        /// <summary>
        /// Returns all fields and values of the hash stored at key. 
        /// </summary>
        /// <returns>list of fields and their values stored in the hash, or an empty list when key does not exist.</returns>
        /// <remarks>http://redis.io/commands/hgetall</remarks>
        HashEntry[] HashGetAll(RedisKey key, CommandFlags flags = CommandFlags.None);

        /// <summary>
        /// Increments the number stored at field in the hash stored at key by increment. If key does not exist, a new key holding a hash is created. If field does not exist or holds a string that cannot be interpreted as integer, the value is set to 0 before the operation is performed.
        /// </summary>
        /// <remarks>The range of values supported by HINCRBY is limited to 64 bit signed integers.</remarks>
        /// <returns>the value at field after the increment operation.</returns>
        /// <remarks>http://redis.io/commands/hincrby</remarks>
        long HashIncrement(RedisKey key, RedisValue hashField, long value = 1, CommandFlags flags = CommandFlags.None);

        /// <summary>
        /// Increment the specified field of an hash stored at key, and representing a floating point number, by the specified increment. If the field does not exist, it is set to 0 before performing the operation.
        /// </summary>
        /// <remarks>The precision of the output is fixed at 17 digits after the decimal point regardless of the actual internal precision of the computation.</remarks>
        /// <returns>the value at field after the increment operation.</returns>
        /// <remarks>http://redis.io/commands/hincrbyfloat</remarks>
        double HashIncrement(RedisKey key, RedisValue hashField, double value, CommandFlags flags = CommandFlags.None);

        /// <summary>
        /// Returns all field names in the hash stored at key.
        /// </summary>
        /// <returns>list of fields in the hash, or an empty list when key does not exist.</returns>
        /// <remarks>http://redis.io/commands/hkeys</remarks>
        RedisValue[] HashKeys(RedisKey key, CommandFlags flags = CommandFlags.None);

        /// <summary>
        /// Returns the number of fields contained in the hash stored at key.
        /// </summary>
        /// <returns>number of fields in the hash, or 0 when key does not exist.</returns>
        /// <remarks>http://redis.io/commands/hlen</remarks>
        long HashLength(RedisKey key, CommandFlags flags = CommandFlags.None);

        /// <summary>
        /// The HSCAN command is used to incrementally iterate over a hash
        /// </summary>
        /// <returns>yields all elements of the hash.</returns>
        /// <remarks>http://redis.io/commands/hscan</remarks>
        IEnumerable<HashEntry> HashScan(RedisKey key, RedisValue pattern, int pageSize, CommandFlags flags);

        /// <summary>
        /// The HSCAN command is used to incrementally iterate over a hash; note: to resume an iteration via <i>cursor</i>, cast the original enumerable or enumerator to <i>IScanningCursor</i>.
        /// </summary>
        /// <returns>yields all elements of the hash.</returns>
        /// <remarks>http://redis.io/commands/hscan</remarks>
        IEnumerable<HashEntry> HashScan(RedisKey key, RedisValue pattern = default(RedisValue), int pageSize = RedisBase.CursorUtils.DefaultPageSize, long cursor = RedisBase.CursorUtils.Origin, int pageOffset = 0, CommandFlags flags = CommandFlags.None);

        /// <summary>
        /// Sets the specified fields to their respective values in the hash stored at key. This command overwrites any existing fields in the hash. If key does not exist, a new key holding a hash is created.
        /// </summary>
        /// <remarks>http://redis.io/commands/hmset</remarks>
        void HashSet(RedisKey key, HashEntry[] hashFields, CommandFlags flags = CommandFlags.None);

        /// <summary>
        /// Sets field in the hash stored at key to value. If key does not exist, a new key holding a hash is created. If field already exists in the hash, it is overwritten.
        /// </summary>
        /// <returns>1 if field is a new field in the hash and value was set. 0 if field already exists in the hash and the value was updated.</returns>
        /// <remarks>http://redis.io/commands/hset</remarks>
        /// <remarks>http://redis.io/commands/hsetnx</remarks>
        bool HashSet(RedisKey key, RedisValue hashField, RedisValue value, When when = When.Always, CommandFlags flags = CommandFlags.None);

        /// <summary>
        /// Returns all values in the hash stored at key.
        /// </summary>
        /// <returns>list of values in the hash, or an empty list when key does not exist.</returns>
        /// <remarks>http://redis.io/commands/hvals</remarks>
        RedisValue[] HashValues(RedisKey key, CommandFlags flags = CommandFlags.None);

        /// <summary>
        /// Adds the element to the HyperLogLog data structure stored at the variable name specified as first argument.
        /// </summary>
        /// <returns>true if at least 1 HyperLogLog internal register was altered. false otherwise.</returns>
        /// <remarks>http://redis.io/commands/pfadd</remarks>
        bool HyperLogLogAdd(RedisKey key, RedisValue value, CommandFlags flags = CommandFlags.None);

        /// <summary>
        /// Adds all the element arguments to the HyperLogLog data structure stored at the variable name specified as first argument.
        /// </summary>
        /// <returns>true if at least 1 HyperLogLog internal register was altered. false otherwise.</returns>
        /// <remarks>http://redis.io/commands/pfadd</remarks>
        bool HyperLogLogAdd(RedisKey key, RedisValue[] values, CommandFlags flags = CommandFlags.None);

        /// <summary>
        /// Returns the approximated cardinality computed by the HyperLogLog data structure stored at the specified variable, or 0 if the variable does not exist.
        /// </summary>
        /// <returns>The approximated number of unique elements observed via HyperLogLogAdd.</returns>
        /// <remarks>http://redis.io/commands/pfcount</remarks>
        long HyperLogLogLength(RedisKey key, CommandFlags flags = CommandFlags.None);

        /// <summary>
        /// Returns the approximated cardinality of the union of the HyperLogLogs passed, by internally merging the HyperLogLogs stored at the provided keys into a temporary hyperLogLog, or 0 if the variable does not exist.
        /// </summary>
        /// <returns>The approximated number of unique elements observed via HyperLogLogAdd.</returns>
        /// <remarks>http://redis.io/commands/pfcount</remarks>
        long HyperLogLogLength(RedisKey[] keys, CommandFlags flags = CommandFlags.None);

        /// <summary>
        /// Merge multiple HyperLogLog values into an unique value that will approximate the cardinality of the union of the observed Sets of the source HyperLogLog structures.
        /// </summary>
        /// <remarks>http://redis.io/commands/pfmerge</remarks>
        void HyperLogLogMerge(RedisKey destination, RedisKey first, RedisKey second, CommandFlags flags = CommandFlags.None);

        /// <summary>
        /// Merge multiple HyperLogLog values into an unique value that will approximate the cardinality of the union of the observed Sets of the source HyperLogLog structures.
        /// </summary>
        /// <remarks>http://redis.io/commands/pfmerge</remarks>
        void HyperLogLogMerge(RedisKey destination, RedisKey[] sourceKeys, CommandFlags flags = CommandFlags.None);

        /// <summary>
        /// Inidicate exactly which redis server we are talking to
        /// </summary>
        [IgnoreNamePrefix]
        EndPoint IdentifyEndpoint(RedisKey key = default(RedisKey), CommandFlags flags = CommandFlags.None);

        /// <summary>
        /// Removes the specified key. A key is ignored if it does not exist.
        /// </summary>
        /// <returns>True if the key was removed.</returns>
        /// <remarks>http://redis.io/commands/del</remarks>
        bool KeyDelete(RedisKey key, CommandFlags flags = CommandFlags.None);

        /// <summary>
        /// Removes the specified keys. A key is ignored if it does not exist.
        /// </summary>
        /// <returns>The number of keys that were removed.</returns>
        /// <remarks>http://redis.io/commands/del</remarks>
        long KeyDelete(RedisKey[] keys, CommandFlags flags = CommandFlags.None);

        /// <summary>
        /// Serialize the value stored at key in a Redis-specific format and return it to the user. The returned value can be synthesized back into a Redis key using the RESTORE command.
        /// </summary>
        /// <returns>the serialized value.</returns>
        /// <remarks>http://redis.io/commands/dump</remarks>
        byte[] KeyDump(RedisKey key, CommandFlags flags = CommandFlags.None);

        /// <summary>
        /// Returns if key exists.
        /// </summary>
        /// <returns>1 if the key exists. 0 if the key does not exist.</returns>
        /// <remarks>http://redis.io/commands/exists</remarks>
        bool KeyExists(RedisKey key, CommandFlags flags = CommandFlags.None);

        /// <summary>
        /// Set a timeout on key. After the timeout has expired, the key will automatically be deleted. A key with an associated timeout is said to be volatile in Redis terminology.
        /// </summary>
        /// <remarks>If key is updated before the timeout has expired, then the timeout is removed as if the PERSIST command was invoked on key.
        /// For Redis versions &lt; 2.1.3, existing timeouts cannot be overwritten. So, if key already has an associated timeout, it will do nothing and return 0. Since Redis 2.1.3, you can update the timeout of a key. It is also possible to remove the timeout using the PERSIST command. See the page on key expiry for more information.</remarks>
        /// <returns>1 if the timeout was set. 0 if key does not exist or the timeout could not be set.</returns>
        /// <remarks>http://redis.io/commands/expire</remarks>
        /// <remarks>http://redis.io/commands/pexpire</remarks>
        /// <remarks>http://redis.io/commands/persist</remarks>
        bool KeyExpire(RedisKey key, TimeSpan? expiry, CommandFlags flags = CommandFlags.None);

        /// <summary>
        /// Set a timeout on key. After the timeout has expired, the key will automatically be deleted. A key with an associated timeout is said to be volatile in Redis terminology.
        /// </summary>
        /// <remarks>If key is updated before the timeout has expired, then the timeout is removed as if the PERSIST command was invoked on key.
        /// For Redis versions &lt; 2.1.3, existing timeouts cannot be overwritten. So, if key already has an associated timeout, it will do nothing and return 0. Since Redis 2.1.3, you can update the timeout of a key. It is also possible to remove the timeout using the PERSIST command. See the page on key expiry for more information.</remarks>
        /// <returns>1 if the timeout was set. 0 if key does not exist or the timeout could not be set.</returns>
        /// <remarks>http://redis.io/commands/expireat</remarks>
        /// <remarks>http://redis.io/commands/pexpireat</remarks>
        /// <remarks>http://redis.io/commands/persist</remarks>
        bool KeyExpire(RedisKey key, DateTime? expiry, CommandFlags flags = CommandFlags.None);

        /// <summary>
        /// Move key from the currently selected database (see SELECT) to the specified destination database. When key already exists in the destination database, or it does not exist in the source database, it does nothing. It is possible to use MOVE as a locking primitive because of this.
        /// </summary>
        /// <returns>1 if key was moved; 0 if key was not moved.</returns>
        /// <remarks>http://redis.io/commands/move</remarks>
        bool KeyMove(RedisKey key, int database, CommandFlags flags = CommandFlags.None);

        /// <summary>Remove the existing timeout on key, turning the key from volatile (a key with an expire set) to persistent (a key that will never expire as no timeout is associated).</summary>
        /// <returns>1 if the timeout was removed. 0 if key does not exist or does not have an associated timeout.</returns>
        /// <remarks>http://redis.io/commands/persist</remarks>
        bool KeyPersist(RedisKey key, CommandFlags flags = CommandFlags.None);

        /// <summary>
        /// Return a random key from the currently selected database.
        /// </summary>
        /// <returns>the random key, or nil when the database is empty.</returns>
        /// <remarks>http://redis.io/commands/randomkey</remarks>
        RedisKey KeyRandom(CommandFlags flags = CommandFlags.None);

        /// <summary>
        /// Renames key to newkey. It returns an error when the source and destination names are the same, or when key does not exist. 
        /// </summary>
        /// <returns>http://redis.io/commands/rename</returns>
        /// <remarks>http://redis.io/commands/renamenx</remarks>
        bool KeyRename(RedisKey key, RedisKey newKey, When when = When.Always, CommandFlags flags = CommandFlags.None);

        /// <summary>
        /// Create a key associated with a value that is obtained by deserializing the provided serialized value (obtained via DUMP).
        /// If ttl is 0 the key is created without any expire, otherwise the specified expire time(in milliseconds) is set.
        /// </summary>
        /// <remarks>http://redis.io/commands/restore</remarks>
        void KeyRestore(RedisKey key, byte[] value, TimeSpan? expiry = null, CommandFlags flags = CommandFlags.None);

        /// <summary>
        /// Returns the remaining time to live of a key that has a timeout.  This introspection capability allows a Redis client to check how many seconds a given key will continue to be part of the dataset.
        /// </summary>
        /// <returns>TTL, or nil when key does not exist or does not have a timeout.</returns>
        /// <remarks>http://redis.io/commands/ttl</remarks>
        TimeSpan? KeyTimeToLive(RedisKey key, CommandFlags flags = CommandFlags.None);

        /// <summary>
        /// Returns the string representation of the type of the value stored at key. The different types that can be returned are: string, list, set, zset and hash.
        /// </summary>
        /// <returns>type of key, or none when key does not exist.</returns>
        /// <remarks>http://redis.io/commands/type</remarks>
        RedisType KeyType(RedisKey key, CommandFlags flags = CommandFlags.None);

        /// <summary>
        /// Returns the element at index index in the list stored at key. The index is zero-based, so 0 means the first element, 1 the second element and so on. Negative indices can be used to designate elements starting at the tail of the list. Here, -1 means the last element, -2 means the penultimate and so forth.
        /// </summary>
        /// <returns>the requested element, or nil when index is out of range.</returns>
        /// <remarks>http://redis.io/commands/lindex</remarks>
        RedisValue ListGetByIndex(RedisKey key, long index, CommandFlags flags = CommandFlags.None);

        /// <summary>
        /// Inserts value in the list stored at key either before or after the reference value pivot.
        /// When key does not exist, it is considered an empty list and no operation is performed.
        /// </summary>
        /// <returns>the length of the list after the insert operation, or -1 when the value pivot was not found.</returns>
        /// <remarks>http://redis.io/commands/linsert</remarks>
        long ListInsertAfter(RedisKey key, RedisValue pivot, RedisValue value, CommandFlags flags = CommandFlags.None);

        /// <summary>
        /// Inserts value in the list stored at key either before or after the reference value pivot.
        /// When key does not exist, it is considered an empty list and no operation is performed.
        /// </summary>
        /// <returns>the length of the list after the insert operation, or -1 when the value pivot was not found.</returns>
        /// <remarks>http://redis.io/commands/linsert</remarks>
        long ListInsertBefore(RedisKey key, RedisValue pivot, RedisValue value, CommandFlags flags = CommandFlags.None);

        /// <summary>
        /// Removes and returns the first element of the list stored at key.
        /// </summary>
        /// <returns>the value of the first element, or nil when key does not exist.</returns>
        /// <remarks>http://redis.io/commands/lpop</remarks>
        RedisValue ListLeftPop(RedisKey key, CommandFlags flags = CommandFlags.None);

        /// <summary>
        /// Insert the specified value at the head of the list stored at key. If key does not exist, it is created as empty list before performing the push operations.
        /// </summary>
        /// <returns>the length of the list after the push operations.</returns>
        /// <remarks>http://redis.io/commands/lpush</remarks>
        /// <remarks>http://redis.io/commands/lpushx</remarks>
        long ListLeftPush(RedisKey key, RedisValue value, When when = When.Always, CommandFlags flags = CommandFlags.None);

        /// <summary>
        /// Insert all the specified values at the head of the list stored at key. If key does not exist, it is created as empty list before performing the push operations.
        /// Elements are inserted one after the other to the head of the list, from the leftmost element to the rightmost element. So for instance the command LPUSH mylist a b c will result into a list containing c as first element, b as second element and a as third element.
        /// </summary>
        /// <returns>the length of the list after the push operations.</returns>
        /// <remarks>http://redis.io/commands/lpush</remarks>
        long ListLeftPush(RedisKey key, RedisValue[] values, CommandFlags flags = CommandFlags.None);

        /// <summary>
        /// Returns the length of the list stored at key. If key does not exist, it is interpreted as an empty list and 0 is returned. 
        /// </summary>
        /// <returns>the length of the list at key.</returns>
        /// <remarks>http://redis.io/commands/llen</remarks>
        long ListLength(RedisKey key, CommandFlags flags = CommandFlags.None);

        /// <summary>
        /// Returns the specified elements of the list stored at key. The offsets start and stop are zero-based indexes, with 0 being the first element of the list (the head of the list), 1 being the next element and so on.
        /// These offsets can also be negative numbers indicating offsets starting at the end of the list.For example, -1 is the last element of the list, -2 the penultimate, and so on.
        /// Note that if you have a list of numbers from 0 to 100, LRANGE list 0 10 will return 11 elements, that is, the rightmost item is included. 
        /// </summary>
        /// <returns>list of elements in the specified range.</returns>
        /// <remarks>http://redis.io/commands/lrange</remarks>
        RedisValue[] ListRange(RedisKey key, long start = 0, long stop = -1, CommandFlags flags = CommandFlags.None);

        /// <summary>
        /// Removes the first count occurrences of elements equal to value from the list stored at key. The count argument influences the operation in the following ways:
        /// count &gt; 0: Remove elements equal to value moving from head to tail.
        /// count &lt; 0: Remove elements equal to value moving from tail to head.
        /// count = 0: Remove all elements equal to value.
        /// </summary>
        /// <returns>the number of removed elements.</returns>
        /// <remarks>http://redis.io/commands/lrem</remarks>
        long ListRemove(RedisKey key, RedisValue value, long count = 0, CommandFlags flags = CommandFlags.None);

        /// <summary>
        /// Removes and returns the last element of the list stored at key.
        /// </summary>
        /// <remarks>http://redis.io/commands/rpop</remarks>
        RedisValue ListRightPop(RedisKey key, CommandFlags flags = CommandFlags.None);

        /// <summary>
        /// Atomically returns and removes the last element (tail) of the list stored at source, and pushes the element at the first element (head) of the list stored at destination.
        /// </summary>
        /// <returns>the element being popped and pushed.</returns>
        /// <remarks>http://redis.io/commands/rpoplpush</remarks>
        RedisValue ListRightPopLeftPush(RedisKey source, RedisKey destination, CommandFlags flags = CommandFlags.None);

        /// <summary>
        /// Insert the specified value at the tail of the list stored at key. If key does not exist, it is created as empty list before performing the push operation.
        /// </summary>
        /// <returns>the length of the list after the push operation.</returns>
        /// <remarks>http://redis.io/commands/rpush</remarks>
        /// <remarks>http://redis.io/commands/rpushx</remarks>
        long ListRightPush(RedisKey key, RedisValue value, When when = When.Always, CommandFlags flags = CommandFlags.None);

        /// <summary>
        /// Insert all the specified values at the tail of the list stored at key. If key does not exist, it is created as empty list before performing the push operation. 
        /// Elements are inserted one after the other to the tail of the list, from the leftmost element to the rightmost element. So for instance the command RPUSH mylist a b c will result into a list containing a as first element, b as second element and c as third element.
        /// </summary>
        /// <returns>the length of the list after the push operation.</returns>
        /// <remarks>http://redis.io/commands/rpush</remarks>
        long ListRightPush(RedisKey key, RedisValue[] values, CommandFlags flags = CommandFlags.None);

        /// <summary>
        /// Sets the list element at index to value. For more information on the index argument, see ListGetByIndex. An error is returned for out of range indexes.
        /// </summary>
        /// <remarks>http://redis.io/commands/lset</remarks>
        void ListSetByIndex(RedisKey key, long index, RedisValue value, CommandFlags flags = CommandFlags.None);

        /// <summary>
        /// Trim an existing list so that it will contain only the specified range of elements specified. Both start and stop are zero-based indexes, where 0 is the first element of the list (the head), 1 the next element and so on.
        /// For example: LTRIM foobar 0 2 will modify the list stored at foobar so that only the first three elements of the list will remain.
        /// start and end can also be negative numbers indicating offsets from the end of the list, where -1 is the last element of the list, -2 the penultimate element and so on.
        /// </summary>
        /// <remarks>http://redis.io/commands/ltrim</remarks>
        void ListTrim(RedisKey key, long start, long stop, CommandFlags flags = CommandFlags.None);

        /// <summary>
        /// Extends a lock, if the token value is correct
        /// </summary>
        bool LockExtend(RedisKey key, RedisValue value, TimeSpan expiry, CommandFlags flags = CommandFlags.None);

        /// <summary>
        /// Queries the token held against a lock
        /// </summary>
        RedisValue LockQuery(RedisKey key, CommandFlags flags = CommandFlags.None);

        /// <summary>
        /// Releases a lock, if the token value is correct
        /// </summary>
        bool LockRelease(RedisKey key, RedisValue value, CommandFlags flags = CommandFlags.None);

        /// <summary>
        /// Takes a lock (specifying a token value) if it is not already taken
        /// </summary>
        bool LockTake(RedisKey key, RedisValue value, TimeSpan expiry, CommandFlags flags = CommandFlags.None);

        /// <summary>
        /// Posts a message to the given channel.
        /// </summary>
        /// <returns>the number of clients that received the message.</returns>
        /// <remarks>http://redis.io/commands/publish</remarks>
        long Publish(RedisChannel channel, RedisValue message, CommandFlags flags = CommandFlags.None);
        /// <summary>
        /// Execute a Lua script against the server
        /// </summary>
        /// <remarks>http://redis.io/commands/eval, http://redis.io/commands/evalsha</remarks>
        /// <returns>A dynamic representation of the script's result</returns>
        RedisResult ScriptEvaluate(string script, RedisKey[] keys = null, RedisValue[] values = null, CommandFlags flags = CommandFlags.None);

        /// <summary>
        /// Execute a Lua script against the server using just the SHA1 hash
        /// </summary>
        /// <remarks>http://redis.io/commands/evalsha</remarks>
        /// <returns>A dynamic representation of the script's result</returns>
        RedisResult ScriptEvaluate(byte[] hash, RedisKey[] keys = null, RedisValue[] values = null, CommandFlags flags = CommandFlags.None);

        /// <summary>
        /// Add the specified member to the set stored at key. Specified members that are already a member of this set are ignored. If key does not exist, a new set is created before adding the specified members.
        /// </summary>
        /// <returns>True if the specified member was not already present in the set, else False</returns>
        /// <remarks>http://redis.io/commands/sadd</remarks>
        bool SetAdd(RedisKey key, RedisValue value, CommandFlags flags = CommandFlags.None);

        /// <summary>
        /// Add the specified members to the set stored at key. Specified members that are already a member of this set are ignored. If key does not exist, a new set is created before adding the specified members.
        /// </summary>
        /// <returns>the number of elements that were added to the set, not including all the elements already present into the set.</returns>
        /// <remarks>http://redis.io/commands/sadd</remarks>
        long SetAdd(RedisKey key, RedisValue[] values, CommandFlags flags = CommandFlags.None);

        /// <summary>
        /// Returns the members of the set resulting from the specified operation against the given sets.
        /// </summary>
        /// <returns>list with members of the resulting set.</returns>
        /// <remarks>http://redis.io/commands/sunion</remarks>
        /// <remarks>http://redis.io/commands/sinter</remarks>
        /// <remarks>http://redis.io/commands/sdiff</remarks>
        RedisValue[] SetCombine(SetOperation operation, RedisKey first, RedisKey second, CommandFlags flags = CommandFlags.None);

        /// <summary>
        /// Returns the members of the set resulting from the specified operation against the given sets.
        /// </summary>
        /// <returns>list with members of the resulting set.</returns>
        /// <remarks>http://redis.io/commands/sunion</remarks>
        /// <remarks>http://redis.io/commands/sinter</remarks>
        /// <remarks>http://redis.io/commands/sdiff</remarks>
        RedisValue[] SetCombine(SetOperation operation, RedisKey[] keys, CommandFlags flags = CommandFlags.None);

        /// <summary>
        /// This command is equal to SetCombine, but instead of returning the resulting set, it is stored in destination. If destination already exists, it is overwritten.
        /// </summary>
        /// <returns>the number of elements in the resulting set.</returns>
        /// <remarks>http://redis.io/commands/sunionstore</remarks>
        /// <remarks>http://redis.io/commands/sinterstore</remarks>
        /// <remarks>http://redis.io/commands/sdiffstore</remarks>
        long SetCombineAndStore(SetOperation operation, RedisKey destination, RedisKey first, RedisKey second, CommandFlags flags = CommandFlags.None);

        /// <summary>
        /// This command is equal to SetCombine, but instead of returning the resulting set, it is stored in destination. If destination already exists, it is overwritten.
        /// </summary>
        /// <returns>the number of elements in the resulting set.</returns>
        /// <remarks>http://redis.io/commands/sunionstore</remarks>
        /// <remarks>http://redis.io/commands/sinterstore</remarks>
        /// <remarks>http://redis.io/commands/sdiffstore</remarks>
        long SetCombineAndStore(SetOperation operation, RedisKey destination, RedisKey[] keys, CommandFlags flags = CommandFlags.None);

        /// <summary>
        /// Returns if member is a member of the set stored at key.
        /// </summary>
        /// <returns>1 if the element is a member of the set. 0 if the element is not a member of the set, or if key does not exist.</returns>
        /// <remarks>http://redis.io/commands/sismember</remarks>
        bool SetContains(RedisKey key, RedisValue value, CommandFlags flags = CommandFlags.None);

        /// <summary>
        /// Returns the set cardinality (number of elements) of the set stored at key.
        /// </summary>
        /// <returns>the cardinality (number of elements) of the set, or 0 if key does not exist.</returns>
        /// <remarks>http://redis.io/commands/scard</remarks>
        long SetLength(RedisKey key, CommandFlags flags = CommandFlags.None);

        /// <summary>
        /// Returns all the members of the set value stored at key.
        /// </summary>
        /// <returns>all elements of the set.</returns>
        /// <remarks>http://redis.io/commands/smembers</remarks>
        RedisValue[] SetMembers(RedisKey key, CommandFlags flags = CommandFlags.None);

        /// <summary>
        /// Move member from the set at source to the set at destination. This operation is atomic. In every given moment the element will appear to be a member of source or destination for other clients.
        /// When the specified element already exists in the destination set, it is only removed from the source set.
        /// </summary>
        /// <returns>1 if the element is moved. 0 if the element is not a member of source and no operation was performed.</returns>
        /// <remarks>http://redis.io/commands/smove</remarks>
        bool SetMove(RedisKey source, RedisKey destination, RedisValue value, CommandFlags flags = CommandFlags.None);

        /// <summary>
        /// Removes and returns a random element from the set value stored at key.
        /// </summary>
        /// <returns>the removed element, or nil when key does not exist.</returns>
        /// <remarks>http://redis.io/commands/spop</remarks>
        RedisValue SetPop(RedisKey key, CommandFlags flags = CommandFlags.None);

        /// <summary>
        /// Return a random element from the set value stored at key.
        /// </summary>
        /// <returns>the randomly selected element, or nil when key does not exist</returns>
        /// <remarks>http://redis.io/commands/srandmember</remarks>
        RedisValue SetRandomMember(RedisKey key, CommandFlags flags = CommandFlags.None);

        /// <summary>
        /// Return an array of count distinct elements if count is positive. If called with a negative count the behavior changes and the command is allowed to return the same element multiple times.
        /// In this case the numer of returned elements is the absolute value of the specified count.
        /// </summary>
        /// <returns>an array of elements, or an empty array when key does not exist</returns>
        /// <remarks>http://redis.io/commands/srandmember</remarks>
        RedisValue[] SetRandomMembers(RedisKey key, long count, CommandFlags flags = CommandFlags.None);

        /// <summary>
        /// Remove the specified member from the set stored at key.  Specified members that are not a member of this set are ignored.
        /// </summary>
        /// <returns>True if the specified member was already present in the set, else False</returns>
        /// <remarks>http://redis.io/commands/srem</remarks>
        bool SetRemove(RedisKey key, RedisValue value, CommandFlags flags = CommandFlags.None);

        /// <summary>
        /// Remove the specified members from the set stored at key. Specified members that are not a member of this set are ignored.
        /// </summary>
        /// <returns>the number of members that were removed from the set, not including non existing members.</returns>
        /// <remarks>http://redis.io/commands/srem</remarks>
        long SetRemove(RedisKey key, RedisValue[] values, CommandFlags flags = CommandFlags.None);

        /// <summary>
        /// The SSCAN command is used to incrementally iterate over set
        /// </summary>
        /// <returns>yields all elements of the set.</returns>
        /// <remarks>http://redis.io/commands/sscan</remarks>
        IEnumerable<RedisValue> SetScan(RedisKey key, RedisValue pattern, int pageSize, CommandFlags flags);

        /// <summary>
        /// The SSCAN command is used to incrementally iterate over set; note: to resume an iteration via <i>cursor</i>, cast the original enumerable or enumerator to <i>IScanningCursor</i>.
        /// </summary>
        /// <returns>yields all elements of the set.</returns>
        /// <remarks>http://redis.io/commands/sscan</remarks>
        IEnumerable<RedisValue> SetScan(RedisKey key, RedisValue pattern = default(RedisValue), int pageSize = RedisBase.CursorUtils.DefaultPageSize, long cursor = RedisBase.CursorUtils.Origin, int pageOffset = 0, CommandFlags flags = CommandFlags.None);

        /// <summary>
        /// Sorts a list, set or sorted set (numerically or alphabetically, ascending by default); By default, the elements themselves are compared, but the values can also be
        /// used to perform external key-lookups using the <c>by</c> parameter. By default, the elements themselves are returned, but external key-lookups (one or many) can
        /// be performed instead by specifying the <c>get</c> parameter (note that <c>#</c> specifies the element itself, when used in <c>get</c>).
        /// Referring to the <a href="http://redis.io/commands/sort">redis SORT documentation </a> for examples is recommended. When used in hashes, <c>by</c> and <c>get</c>
        /// can be used to specify fields using <c>-&gt;</c> notation (again, refer to redis documentation).
        /// </summary>
        /// <remarks>http://redis.io/commands/sort</remarks>
        /// <returns>Returns the sorted elements, or the external values if <c>get</c> is specified</returns>
        [IgnoreNamePrefix]
        RedisValue[] Sort(RedisKey key, long skip = 0, long take = -1, Order order = Order.Ascending, SortType sortType = SortType.Numeric, RedisValue by = default(RedisValue), RedisValue[] get = null, CommandFlags flags = CommandFlags.None);

        /// <summary>
        /// Sorts a list, set or sorted set (numerically or alphabetically, ascending by default); By default, the elements themselves are compared, but the values can also be
        /// used to perform external key-lookups using the <c>by</c> parameter. By default, the elements themselves are returned, but external key-lookups (one or many) can
        /// be performed instead by specifying the <c>get</c> parameter (note that <c>#</c> specifies the element itself, when used in <c>get</c>).
        /// Referring to the <a href="http://redis.io/commands/sort">redis SORT documentation </a> for examples is recommended. When used in hashes, <c>by</c> and <c>get</c>
        /// can be used to specify fields using <c>-&gt;</c> notation (again, refer to redis documentation).
        /// </summary>
        /// <remarks>http://redis.io/commands/sort</remarks>
        /// <returns>Returns the number of elements stored in the new list</returns>
        [IgnoreNamePrefix]
        long SortAndStore(RedisKey destination, RedisKey key, long skip = 0, long take = -1, Order order = Order.Ascending, SortType sortType = SortType.Numeric, RedisValue by = default(RedisValue), RedisValue[] get = null, CommandFlags flags = CommandFlags.None);

        /// <summary>
        /// Adds the specified member with the specified score to the sorted set stored at key. If the specified member is already a member of the sorted set, the score is updated and the element reinserted at the right position to ensure the correct ordering.
        /// </summary>
        /// <returns>True if the value was added, False if it already existed (the score is still updated)</returns>
        /// <remarks>http://redis.io/commands/zadd</remarks>
        bool SortedSetAdd(RedisKey key, RedisValue member, double score, CommandFlags flags = CommandFlags.None);

        /// <summary>
        /// Adds all the specified members with the specified scores to the sorted set stored at key. If a specified member is already a member of the sorted set, the score is updated and the element reinserted at the right position to ensure the correct ordering.
        /// </summary>
        /// <returns>The number of elements added to the sorted sets, not including elements already existing for which the score was updated.</returns>
        /// <remarks>http://redis.io/commands/zadd</remarks>
        long SortedSetAdd(RedisKey key, SortedSetEntry[] values, CommandFlags flags = CommandFlags.None);

        /// <summary>
        /// Computes a set operation over two sorted sets, and stores the result in destination, optionally performing 
        /// a specific aggregation (defaults to sum)
        /// </summary>
        /// <remarks>http://redis.io/commands/zunionstore</remarks>
        /// <remarks>http://redis.io/commands/zinterstore</remarks>
        /// <returns>the number of elements in the resulting sorted set at destination</returns>
        long SortedSetCombineAndStore(SetOperation operation, RedisKey destination, RedisKey first, RedisKey second, Aggregate aggregate = Aggregate.Sum, CommandFlags flags = CommandFlags.None);

        /// <summary>
        /// Computes a set operation over multiple sorted sets (optionally using per-set weights), and stores the result in destination, optionally performing 
        /// a specific aggregation (defaults to sum)
        /// </summary>
        /// <remarks>http://redis.io/commands/zunionstore</remarks>
        /// <remarks>http://redis.io/commands/zinterstore</remarks>
        /// <returns>the number of elements in the resulting sorted set at destination</returns>
        long SortedSetCombineAndStore(SetOperation operation, RedisKey destination, RedisKey[] keys, double[] weights = null, Aggregate aggregate = Aggregate.Sum, CommandFlags flags = CommandFlags.None);

        /// <summary>
        /// Decrements the score of member in the sorted set stored at key by decrement. If member does not exist in the sorted set, it is added with -decrement as its score (as if its previous score was 0.0).
        /// </summary>
        /// <returns>the new score of member</returns>
        /// <remarks>http://redis.io/commands/zincrby</remarks>
        double SortedSetDecrement(RedisKey key, RedisValue member, double value, CommandFlags flags = CommandFlags.None);

        /// <summary>
        /// Increments the score of member in the sorted set stored at key by increment. If member does not exist in the sorted set, it is added with increment as its score (as if its previous score was 0.0).
        /// </summary>
        /// <returns>the new score of member</returns>
        /// <remarks>http://redis.io/commands/zincrby</remarks>
        double SortedSetIncrement(RedisKey key, RedisValue member, double value, CommandFlags flags = CommandFlags.None);

        /// <summary>
        /// Returns the sorted set cardinality (number of elements) of the sorted set stored at key.
        /// </summary>
        /// <returns>the cardinality (number of elements) of the sorted set, or 0 if key does not exist.</returns>
        /// <remarks>http://redis.io/commands/zcard</remarks>
        long SortedSetLength(RedisKey key, double min = double.NegativeInfinity, double max = double.PositiveInfinity, Exclude exclude = Exclude.None, CommandFlags flags = CommandFlags.None);

        /// <summary>
        /// When all the elements in a sorted set are inserted with the same score, in order to force lexicographical ordering, this command returns the number of elements in the sorted set at key with a value between min and max.
        /// </summary>
        /// <returns>the number of elements in the specified score range.</returns>
        /// <remarks>When all the elements in a sorted set are inserted with the same score, in order to force lexicographical ordering, this command returns all the elements in the sorted set at key with a value between min and max.</remarks>
        long SortedSetLengthByValue(RedisKey key, RedisValue min, RedisValue max, Exclude exclude = Exclude.None, CommandFlags flags = CommandFlags.None);

        /// <summary>
        /// Returns the specified range of elements in the sorted set stored at key. By default the elements are considered to be ordered from the lowest to the highest score. Lexicographical order is used for elements with equal score.
        /// Both start and stop are zero-based indexes, where 0 is the first element, 1 is the next element and so on. They can also be negative numbers indicating offsets from the end of the sorted set, with -1 being the last element of the sorted set, -2 the penultimate element and so on.
        /// </summary>
        /// <returns>list of elements in the specified range</returns>
        /// <remarks>http://redis.io/commands/zrange</remarks>
        /// <remarks>http://redis.io/commands/zrevrange</remarks>
        RedisValue[] SortedSetRangeByRank(RedisKey key, long start = 0, long stop = -1, Order order = Order.Ascending, CommandFlags flags = CommandFlags.None);

        /// <summary>
        /// Returns the specified range of elements in the sorted set stored at key. By default the elements are considered to be ordered from the lowest to the highest score. Lexicographical order is used for elements with equal score.
        /// Both start and stop are zero-based indexes, where 0 is the first element, 1 is the next element and so on. They can also be negative numbers indicating offsets from the end of the sorted set, with -1 being the last element of the sorted set, -2 the penultimate element and so on.
        /// </summary>
        /// <returns>list of elements in the specified range</returns>
        /// <remarks>http://redis.io/commands/zrange</remarks>
        /// <remarks>http://redis.io/commands/zrevrange</remarks>
        SortedSetEntry[] SortedSetRangeByRankWithScores(RedisKey key, long start = 0, long stop = -1, Order order = Order.Ascending, CommandFlags flags = CommandFlags.None);



        /// <summary>
        /// Returns the specified range of elements in the sorted set stored at key. By default the elements are considered to be ordered from the lowest to the highest score. Lexicographical order is used for elements with equal score.
        /// Both start and stop are zero-based indexes, where 0 is the first element, 1 is the next element and so on. They can also be negative numbers indicating offsets from the end of the sorted set, with -1 being the last element of the sorted set, -2 the penultimate element and so on.
        /// </summary>
        /// <returns>list of elements in the specified score range</returns>
        /// <remarks>http://redis.io/commands/zrangebyscore</remarks>
        /// <remarks>http://redis.io/commands/zrevrangebyscore</remarks>
        RedisValue[] SortedSetRangeByScore(RedisKey key,
            double start = double.NegativeInfinity, double stop = double.PositiveInfinity,
            Exclude exclude = Exclude.None, Order order = Order.Ascending, long skip = 0, long take = -1,
            CommandFlags flags = CommandFlags.None);

        /// <summary>
        /// Returns the specified range of elements in the sorted set stored at key. By default the elements are considered to be ordered from the lowest to the highest score. Lexicographical order is used for elements with equal score.
        /// Both start and stop are zero-based indexes, where 0 is the first element, 1 is the next element and so on. They can also be negative numbers indicating offsets from the end of the sorted set, with -1 being the last element of the sorted set, -2 the penultimate element and so on.
        /// </summary>
        /// <returns>list of elements in the specified score range</returns>
        /// <remarks>http://redis.io/commands/zrangebyscore</remarks>
        /// <remarks>http://redis.io/commands/zrevrangebyscore</remarks>
        SortedSetEntry[] SortedSetRangeByScoreWithScores(RedisKey key,
            double start = double.NegativeInfinity, double stop = double.PositiveInfinity,
            Exclude exclude = Exclude.None, Order order = Order.Ascending, long skip = 0, long take = -1,
            CommandFlags flags = CommandFlags.None);

        /// <summary>
        /// When all the elements in a sorted set are inserted with the same score, in order to force lexicographical ordering, this command returns all the elements in the sorted set at key with a value between min and max.
        /// </summary>
        /// <remarks>http://redis.io/commands/zrangebylex</remarks>
        /// <returns>list of elements in the specified score range.</returns>
        RedisValue[] SortedSetRangeByValue(RedisKey key, RedisValue min = default(RedisValue), RedisValue max = default(RedisValue),
            Exclude exclude = Exclude.None, long skip = 0, long take = -1,
            CommandFlags flags = CommandFlags.None);

        /// <summary>
        /// Returns the rank of member in the sorted set stored at key, by default with the scores ordered from low to high. The rank (or index) is 0-based, which means that the member with the lowest score has rank 0.
        /// </summary>
        /// <returns>If member exists in the sorted set, the rank of member; If member does not exist in the sorted set or key does not exist, null</returns>
        /// <remarks>http://redis.io/commands/zrank</remarks>
        /// <remarks>http://redis.io/commands/zrevrank</remarks>
        long? SortedSetRank(RedisKey key, RedisValue member, Order order = Order.Ascending, CommandFlags flags = CommandFlags.None);

        /// <summary>
        /// Removes the specified member from the sorted set stored at key. Non existing members are ignored.
        /// </summary>
        /// <returns>True if the member existed in the sorted set and was removed; False otherwise.</returns>
        /// <remarks>http://redis.io/commands/zrem</remarks>
        bool SortedSetRemove(RedisKey key, RedisValue member, CommandFlags flags = CommandFlags.None);

        /// <summary>
        /// Removes the specified members from the sorted set stored at key. Non existing members are ignored.
        /// </summary>
        /// <returns>The number of members removed from the sorted set, not including non existing members.</returns>
        /// <remarks>http://redis.io/commands/zrem</remarks>
        long SortedSetRemove(RedisKey key, RedisValue[] members, CommandFlags flags = CommandFlags.None);

        /// <summary>
        /// Removes all elements in the sorted set stored at key with rank between start and stop. Both start and stop are 0 -based indexes with 0 being the element with the lowest score. These indexes can be negative numbers, where they indicate offsets starting at the element with the highest score. For example: -1 is the element with the highest score, -2 the element with the second highest score and so forth.
        /// </summary>
        /// <returns> the number of elements removed.</returns>
        /// <remarks>http://redis.io/commands/zremrangebyrank</remarks>
        long SortedSetRemoveRangeByRank(RedisKey key, long start, long stop, CommandFlags flags = CommandFlags.None);

        /// <summary>
        /// Removes all elements in the sorted set stored at key with a score between min and max (inclusive by default).
        /// </summary>
        /// <returns> the number of elements removed.</returns>
        /// <remarks>http://redis.io/commands/zremrangebyscore</remarks>
        long SortedSetRemoveRangeByScore(RedisKey key, double start, double stop, Exclude exclude = Exclude.None, CommandFlags flags = CommandFlags.None);

        /// <summary>
        /// When all the elements in a sorted set are inserted with the same score, in order to force lexicographical ordering, this command removes all elements in the sorted set stored at key between the lexicographical range specified by min and max.
        /// </summary>
        /// <remarks>http://redis.io/commands/zremrangebylex</remarks>
        /// <returns>the number of elements removed.</returns>
        long SortedSetRemoveRangeByValue(RedisKey key, RedisValue min, RedisValue max, Exclude exclude = Exclude.None, CommandFlags flags = CommandFlags.None);
        /// <summary>
        /// The ZSCAN command is used to incrementally iterate over a sorted set
        /// </summary>
        /// <returns>yields all elements of the sorted set.</returns>
        /// <remarks>http://redis.io/commands/zscan</remarks>
        IEnumerable<SortedSetEntry> SortedSetScan(RedisKey key, RedisValue pattern, int pageSize, CommandFlags flags);

        /// <summary>
        /// The ZSCAN command is used to incrementally iterate over a sorted set; note: to resume an iteration via <i>cursor</i>, cast the original enumerable or enumerator to <i>IScanningCursor</i>.
        /// </summary>
        /// <returns>yields all elements of the sorted set.</returns>
        /// <remarks>http://redis.io/commands/zscan</remarks>
        IEnumerable<SortedSetEntry> SortedSetScan(RedisKey key, RedisValue pattern = default(RedisValue), int pageSize = RedisBase.CursorUtils.DefaultPageSize, long cursor = RedisBase.CursorUtils.Origin, int pageOffset = 0, CommandFlags flags = CommandFlags.None);
        /// <summary>
        /// Returns the score of member in the sorted set at key; If member does not exist in the sorted set, or key does not exist, nil is returned.
        /// </summary>
        /// <returns>the score of member</returns>
        /// <remarks>http://redis.io/commands/zscore</remarks>
        double? SortedSetScore(RedisKey key, RedisValue member, CommandFlags flags = CommandFlags.None);





        /// <summary>
        /// If key already exists and is a string, this command appends the value at the end of the string. If key does not exist it is created and set as an empty string,
        /// so APPEND will be similar to SET in this special case.
        /// </summary>
        /// <returns>the length of the string after the append operation.</returns>
        /// <remarks>http://redis.io/commands/append</remarks>
        long StringAppend(RedisKey key, RedisValue value, CommandFlags flags = CommandFlags.None);

        /// <summary>
        /// Count the number of set bits (population counting) in a string.
        /// By default all the bytes contained in the string are examined.It is possible to specify the counting operation only in an interval passing the additional arguments start and end.
        /// Like for the GETRANGE command start and end can contain negative values in order to index bytes starting from the end of the string, where -1 is the last byte, -2 is the penultimate, and so forth.
        /// </summary>
        /// <returns>The number of bits set to 1</returns>
        /// <remarks>http://redis.io/commands/bitcount</remarks>
        long StringBitCount(RedisKey key, long start = 0, long end = -1, CommandFlags flags = CommandFlags.None);

        /// <summary>
        /// Perform a bitwise operation between multiple keys (containing string values) and store the result in the destination key.
        /// The BITOP command supports four bitwise operations; note that NOT is a unary operator: the second key should be omitted in this case
        /// and only the first key will be considered.
        /// The result of the operation is always stored at destkey.
        /// </summary>
        /// <returns>The size of the string stored in the destination key, that is equal to the size of the longest input string.</returns>
        /// <remarks>http://redis.io/commands/bitop</remarks>
        long StringBitOperation(Bitwise operation, RedisKey destination, RedisKey first, RedisKey second = default(RedisKey), CommandFlags flags = CommandFlags.None);

        /// <summary>
        /// Perform a bitwise operation between multiple keys (containing string values) and store the result in the destination key.
        /// The BITOP command supports four bitwise operations; note that NOT is a unary operator.
        /// The result of the operation is always stored at destkey.
        /// </summary>
        /// <returns>The size of the string stored in the destination key, that is equal to the size of the longest input string.</returns>
        /// <remarks>http://redis.io/commands/bitop</remarks>
        long StringBitOperation(Bitwise operation, RedisKey destination, RedisKey[] keys, CommandFlags flags = CommandFlags.None);

        /// <summary>
        /// Return the position of the first bit set to 1 or 0 in a string.
        /// The position is returned thinking at the string as an array of bits from left to right where the first byte most significant bit is at position 0, the second byte most significant bit is at position 8 and so forth.
        /// An start and end may be specified; these are in bytes, not bits; start and end can contain negative values in order to index bytes starting from the end of the string, where -1 is the last byte, -2 is the penultimate, and so forth.
        /// </summary>
        /// <returns>The command returns the position of the first bit set to 1 or 0 according to the request.
        /// If we look for set bits(the bit argument is 1) and the string is empty or composed of just zero bytes, -1 is returned.</returns>
        /// <remarks>http://redis.io/commands/bitpos</remarks>
        long StringBitPosition(RedisKey key, bool bit, long start = 0, long end = -1, CommandFlags flags = CommandFlags.None);

        /// <summary>
        /// Decrements the number stored at key by decrement. If the key does not exist, it is set to 0 before performing the operation.
        /// An error is returned if the key contains a value of the wrong type or contains a string that is not representable as integer. This operation is limited to 64 bit signed integers.
        /// </summary>
        /// <returns> the value of key after the decrement</returns>
        /// <remarks>http://redis.io/commands/decrby</remarks>
        /// <remarks>http://redis.io/commands/decr</remarks>
        long StringDecrement(RedisKey key, long value = 1, CommandFlags flags = CommandFlags.None);

        /// <summary>
        /// Decrements the string representing a floating point number stored at key by the specified decrement. If the key does not exist, it is set to 0 before performing the operation. The precision of the output is fixed at 17 digits after the decimal point regardless of the actual internal precision of the computation.
        /// </summary>
        /// <returns>the value of key after the decrement</returns>
        /// <remarks>http://redis.io/commands/incrbyfloat</remarks>
        double StringDecrement(RedisKey key, double value, CommandFlags flags = CommandFlags.None);

        /// <summary>
        /// Get the value of key. If the key does not exist the special value nil is returned. An error is returned if the value stored at key is not a string, because GET only handles string values.
        /// </summary>
        /// <returns>the value of key, or nil when key does not exist.</returns>
        /// <remarks>http://redis.io/commands/get</remarks>
        RedisValue StringGet(RedisKey key, CommandFlags flags = CommandFlags.None);
        /// <summary>
        /// Returns the values of all specified keys. For every key that does not hold a string value or does not exist, the special value nil is returned.
        /// </summary>
        /// <remarks>http://redis.io/commands/mget</remarks>
        RedisValue[] StringGet(RedisKey[] keys, CommandFlags flags = CommandFlags.None);

        /// <summary>
        /// Returns the bit value at offset in the string value stored at key.
        /// When offset is beyond the string length, the string is assumed to be a contiguous space with 0 bits.
        /// </summary>
        /// <returns>the bit value stored at offset.</returns>
        /// <remarks>http://redis.io/commands/getbit</remarks>
        bool StringGetBit(RedisKey key, long offset, CommandFlags flags = CommandFlags.None);

        /// <summary>
        /// Returns the substring of the string value stored at key, determined by the offsets start and end (both are inclusive). Negative offsets can be used in order to provide an offset starting from the end of the string. So -1 means the last character, -2 the penultimate and so forth.
        /// </summary>
        /// <returns>the substring of the string value stored at key</returns>
        /// <remarks>http://redis.io/commands/getrange</remarks>
        RedisValue StringGetRange(RedisKey key, long start, long end, CommandFlags flags = CommandFlags.None);

        /// <summary>
        /// Atomically sets key to value and returns the old value stored at key.
        /// </summary>
        /// <remarks>http://redis.io/commands/getset</remarks>
        /// <returns> the old value stored at key, or nil when key did not exist.</returns>
        RedisValue StringGetSet(RedisKey key, RedisValue value, CommandFlags flags = CommandFlags.None);

        /// <summary>
        /// Get the value of key. If the key does not exist the special value nil is returned. An error is returned if the value stored at key is not a string, because GET only handles string values.
        /// </summary>
        /// <returns>the value of key, or nil when key does not exist.</returns>
        /// <remarks>http://redis.io/commands/get</remarks>
        RedisValueWithExpiry StringGetWithExpiry(RedisKey key, CommandFlags flags = CommandFlags.None);

        /// <summary>
        /// Increments the number stored at key by increment. If the key does not exist, it is set to 0 before performing the operation. An error is returned if the key contains a value of the wrong type or contains a string that is not representable as integer. This operation is limited to 64 bit signed integers.
        /// </summary>
        /// <returns> the value of key after the increment</returns>
        /// <remarks>http://redis.io/commands/incrby</remarks>
        /// <remarks>http://redis.io/commands/incr</remarks>
        long StringIncrement(RedisKey key, long value = 1, CommandFlags flags = CommandFlags.None);

        /// <summary>
        /// Increments the string representing a floating point number stored at key by the specified increment. If the key does not exist, it is set to 0 before performing the operation. The precision of the output is fixed at 17 digits after the decimal point regardless of the actual internal precision of the computation.
        /// </summary>
        /// <returns>the value of key after the increment</returns>
        /// <remarks>http://redis.io/commands/incrbyfloat</remarks>
        double StringIncrement(RedisKey key, double value, CommandFlags flags = CommandFlags.None);

        /// <summary>
        /// Returns the length of the string value stored at key.
        /// </summary>
        /// <returns>the length of the string at key, or 0 when key does not exist.</returns>
        /// <remarks>http://redis.io/commands/strlen</remarks>
        long StringLength(RedisKey key, CommandFlags flags = CommandFlags.None);

        /// <summary>
        /// Set key to hold the string value. If key already holds a value, it is overwritten, regardless of its type.
        /// </summary>
        /// <remarks>http://redis.io/commands/set</remarks>
        bool StringSet(RedisKey key, RedisValue value, TimeSpan? expiry = null, When when = When.Always, CommandFlags flags = CommandFlags.None);
        /// <summary>
        /// Sets the given keys to their respective values. If "not exists" is specified, this will not perform any operation at all even if just a single key already exists.
        /// </summary>
        /// <returns>True if the keys were set, else False</returns>
        /// <remarks>http://redis.io/commands/mset</remarks>
        /// <remarks>http://redis.io/commands/msetnx</remarks>
        bool StringSet(KeyValuePair<RedisKey, RedisValue>[] values, When when = When.Always, CommandFlags flags = CommandFlags.None);

        /// <summary>
        /// Sets or clears the bit at offset in the string value stored at key.
        /// The bit is either set or cleared depending on value, which can be either 0 or 1. When key does not exist, a new string value is created.The string is grown to make sure it can hold a bit at offset.
        /// </summary>
        /// <returns>the original bit value stored at offset.</returns>
        /// <remarks>http://redis.io/commands/setbit</remarks>
        bool StringSetBit(RedisKey key, long offset, bool bit, CommandFlags flags = CommandFlags.None);
        /// <summary>
        /// Overwrites part of the string stored at key, starting at the specified offset, for the entire length of value. If the offset is larger than the current length of the string at key, the string is padded with zero-bytes to make offset fit. Non-existing keys are considered as empty strings, so this command will make sure it holds a string large enough to be able to set value at offset.
        /// </summary>
        /// <returns>the length of the string after it was modified by the command.</returns>
        /// <remarks>http://redis.io/commands/setrange</remarks>
        RedisValue StringSetRange(RedisKey key, long offset, RedisValue value, CommandFlags flags = CommandFlags.None);
    }
}
