// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Data.Entity.ChangeTracking;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Redis.Query;
using Microsoft.Data.Entity.Redis.Utilities;
using StackExchange.Redis;

namespace Microsoft.Data.Entity.Redis
{
    public class RedisDatabase : Database
    {
        private const string KeyNameSeparator = ":";
        private const string EscapedKeyNameSeparator = @"\x3A";
        private const string PropertyValueSeparator = "::";

        private const string EntityFrameworkPrefix = "EF" + KeyNameSeparator;

        private const string IndexPrefix =
            EntityFrameworkPrefix + "Index" + KeyNameSeparator;

        private const string PrimaryKeyIndexPrefix =
            IndexPrefix + "PK" + KeyNameSeparator;

        private const string PrimaryKeyIndexNameFormat = // argument is EntityType
            PrimaryKeyIndexPrefix + "{0}";

        private const string DataPrefix =
            EntityFrameworkPrefix + "Data" + KeyNameSeparator;

        private const string DataHashNameFormat = // 1st arg is EntityType, 2nd is value of the PK
            DataPrefix + "{0}" + KeyNameSeparator + "{1}";

        private const string ValueGeneratorPrefix =
            EntityFrameworkPrefix + "ValueGenerator" + KeyNameSeparator;

        private const string ValueGeneratorKeyNameFormat = // 1st arg is EntityType, 2nd is name of the property
            ValueGeneratorPrefix + "{0}" + KeyNameSeparator + "{1}";

        private static readonly ConcurrentDictionary<string, ConnectionMultiplexer> _connectionMultiplexers
            = new ConcurrentDictionary<string, ConnectionMultiplexer>(); // key = ConfigurationOptions.ToString()

        public RedisDatabase([NotNull] DbContextConfiguration configuration)
            : base(configuration)
        {
        }

        private ConnectionMultiplexer ConnectionMultiplexer
        {
            get
            {
                var connection = (RedisConnection)Configuration.Connection;
                var configurationOptions = ConfigurationOptions.Parse(connection.ConnectionString);

                configurationOptions.AllowAdmin = true; // require Admin access for Server commands

                var connectionMultiplexerKey = configurationOptions.ToString();

                ConnectionMultiplexer connectionMultiplexer;

                if (!_connectionMultiplexers.TryGetValue(connectionMultiplexerKey, out connectionMultiplexer))
                {
                    connectionMultiplexer = ConnectionMultiplexer.Connect(configurationOptions);

                    if (!_connectionMultiplexers.TryAdd(connectionMultiplexerKey, connectionMultiplexer))
                    {
                        connectionMultiplexer = _connectionMultiplexers[connectionMultiplexerKey];
                    }
                }

                return connectionMultiplexer;
            }
        }

        public virtual IDatabase GetUnderlyingDatabase()
        {
            return ConnectionMultiplexer
                .GetDatabase(((RedisConnection)Configuration.Connection).Database);
        }

        public virtual IServer GetUnderlyingServer()
        {
            return ConnectionMultiplexer
                .GetServer(((RedisConnection)Configuration.Connection).ConnectionString);
        }

        public virtual int SaveChanges(
            [NotNull] IReadOnlyList<StateEntry> stateEntries)
        {
            Check.NotNull(stateEntries, "stateEntries");

            var transaction = PrepareTransactionForSaveChanges(stateEntries);

            var entitiesProcessed = 0;
            if (transaction.Execute())
            {
                entitiesProcessed = stateEntries.Count;
            }

            return entitiesProcessed;
        }

        public virtual async Task<int> SaveChangesAsync(
            [NotNull] IReadOnlyList<StateEntry> stateEntries,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            Check.NotNull(stateEntries, "stateEntries");

            cancellationToken.ThrowIfCancellationRequested();

            var transaction = PrepareTransactionForSaveChanges(stateEntries);

            var entitiesProcessed = 0;
            if (await transaction.ExecuteAsync().WithCurrentCulture())
            {
                entitiesProcessed = stateEntries.Count;
            }

            return entitiesProcessed;
        }

        private ITransaction PrepareTransactionForSaveChanges(IReadOnlyList<StateEntry> stateEntries)
        {
            var transaction = GetUnderlyingDatabase().CreateTransaction();
            foreach (var entry in stateEntries)
            {
                switch (entry.EntityState)
                {
                    case EntityState.Added:
                        AddInsertEntryCommands(transaction, entry);
                        break;

                    case EntityState.Deleted:
                        AddDeleteEntryCommands(transaction, entry);
                        break;

                    case EntityState.Modified:
                        AddModifyEntryCommands(transaction, entry);
                        break;
                }
            }
            return transaction;
        }

        /// <summary>
        ///     Gets a set of object[] values from database each of which represents the values
        ///     of the Properties required by the query for a particular EntityType
        /// </summary>
        /// <param name="redisQuery">An object representing the parameters of the query</param>
        /// <returns>
        ///     An Enumerable of object[] values from database each of which represents
        ///     the values of the Properties for the EntityType (either all the propoerties
        ///     or the selected properties as defined by the query)
        /// </returns>
        public virtual IEnumerable<object[]> GetResultsEnumerable([NotNull] RedisQuery redisQuery)
        {
            Check.NotNull(redisQuery, "redisQuery");

            var redisPrimaryKeyIndexKeyName
                = ConstructRedisPrimaryKeyIndexKeyName(redisQuery.EntityType);

            var allKeysForEntity
                = GetUnderlyingDatabase().SetMembers(redisPrimaryKeyIndexKeyName);

            return allKeysForEntity
                .Select(compositePrimaryKey
                    => GetQueryObjectsFromDatabase(
                        compositePrimaryKey, redisQuery, DecodeBytes));
        }

        /// <summary>
        ///     Gets a set of object[] values from database each of which represents the values
        ///     of the Properties required by the query for a particular EntityType
        /// </summary>
        /// <param name="redisQuery">An object representing the parameters of the query</param>
        /// <returns>
        ///     An Enumerable of object[] values from database each of which represents
        ///     the values of the Properties for the EntityType (either all the propoerties
        ///     or the selected properties as defined by the query)
        /// </returns>
        public virtual IAsyncEnumerable<object[]> GetResultsAsyncEnumerable([NotNull] RedisQuery redisQuery)
        {
            Check.NotNull(redisQuery, "redisQuery");

            return new AsyncEnumerable(this, redisQuery);
        }

        /// <summary>
        ///     Deletes all keys in the database
        /// </summary>
        public virtual void FlushDatabase()
        {
            var connection = (RedisConnection)Configuration.Connection;
            GetUnderlyingServer().FlushDatabase(connection.Database);
        }

        /// <summary>
        ///     Deletes all keys in the database
        /// </summary>
        public virtual async Task FlushDatabaseAsync(
            CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();

            var connection = (RedisConnection)Configuration.Connection;

            await GetUnderlyingServer().FlushDatabaseAsync(connection.Database).WithCurrentCulture();
        }

        private void AddInsertEntryCommands(ITransaction transaction, StateEntry stateEntry)
        {
            var compositePrimaryKeyValues =
                ConstructKeyValue(stateEntry, (se, prop) => stateEntry[prop]);

            var redisDataKeyName =
                ConstructRedisDataKeyName(stateEntry.EntityType, compositePrimaryKeyValues);

            // Note: null entries are stored as the absence of the property_name-property_value pair in the hash
            var entries =
                stateEntry.EntityType.Properties
                    .Where(p => stateEntry[p] != null)
                    .Select(p => new HashEntry(p.Name, EncodeAsBytes(stateEntry[p]))).ToArray();
            transaction.HashSetAsync(redisDataKeyName, entries);

            var redisPrimaryKeyIndexKeyName = ConstructRedisPrimaryKeyIndexKeyName(stateEntry.EntityType);
            transaction.SetAddAsync(redisPrimaryKeyIndexKeyName, compositePrimaryKeyValues);
        }

        private void AddDeleteEntryCommands(ITransaction transaction, StateEntry stateEntry)
        {
            var compositePrimaryKeyValues =
                ConstructKeyValue(stateEntry, (se, prop) => stateEntry.OriginalValues[prop]);

            var redisDataKeyName = ConstructRedisDataKeyName(stateEntry.EntityType, compositePrimaryKeyValues);
            transaction.KeyDeleteAsync(redisDataKeyName);

            var redisPrimaryKeyIndexKeyName = ConstructRedisPrimaryKeyIndexKeyName(stateEntry.EntityType);
            transaction.SetRemoveAsync(redisPrimaryKeyIndexKeyName, compositePrimaryKeyValues);
        }

        private void AddModifyEntryCommands(ITransaction transaction, StateEntry stateEntry)
        {
            var compositePrimaryKeyValues =
                ConstructKeyValue(stateEntry, (se, prop) => stateEntry.OriginalValues[prop]);

            var redisPrimaryKeyIndexKeyName = ConstructRedisPrimaryKeyIndexKeyName(stateEntry.EntityType);
            var redisDataKeyName = ConstructRedisDataKeyName(stateEntry.EntityType, compositePrimaryKeyValues);
            transaction.AddCondition(Condition.KeyExists(redisPrimaryKeyIndexKeyName));

            // first delete all the hash entries which have changed to null
            var changingToNullEntries = stateEntry.EntityType.Properties
                .Where(p => stateEntry.IsPropertyModified(p) && stateEntry[p] == null)
                .Select(p => (RedisValue)p.Name).ToArray();
            transaction.HashDeleteAsync(redisDataKeyName, changingToNullEntries);

            // now update all the other entries
            var updatedEntries = stateEntry.EntityType.Properties
                .Where(p => stateEntry.IsPropertyModified(p) && stateEntry[p] != null)
                .Select(p => new HashEntry(p.Name, EncodeAsBytes(stateEntry[p]))).ToArray();
            transaction.HashSetAsync(redisDataKeyName, updatedEntries);
        }

        private static string ConstructRedisPrimaryKeyIndexKeyName(IEntityType entityType)
        {
            return string.Format(CultureInfo.InvariantCulture,
                PrimaryKeyIndexNameFormat, Escape(entityType.SimpleName));
        }

        private static string ConstructRedisDataKeyName(
            IEntityType entityType, string compositePrimaryKeyValues)
        {
            return string.Format(CultureInfo.InvariantCulture,
                DataHashNameFormat, Escape(entityType.SimpleName), compositePrimaryKeyValues);
        }

        public static string ConstructRedisValueGeneratorKeyName([NotNull] IProperty property)
        {
            Check.NotNull(property, "property");

            return string.Format(CultureInfo.InvariantCulture,
                ValueGeneratorKeyNameFormat, Escape(property.EntityType.SimpleName), Escape(property.Name));
        }

        private static string ConstructKeyValue(
            StateEntry stateEntry, Func<StateEntry, IProperty, object> propertyValueSelector)
        {
            return string.Join(
                PropertyValueSeparator,
                stateEntry.EntityType.GetPrimaryKey().Properties.Select(p => EncodeKeyValue(propertyValueSelector(stateEntry, p))));
        }

        // returns the object array representing all the properties
        // required by the RedisQuery. Note: if SelectedProperties is
        // null or empty then return all properties.
        private object[] GetQueryObjectsFromDatabase(
            string primaryKey, RedisQuery redisQuery, Func<byte[], IProperty, object> decoder)
        {
            object[] results = null;
            var dataKeyName = ConstructRedisDataKeyName(redisQuery.EntityType, primaryKey);

            if (redisQuery.SelectedProperties == null
                || !redisQuery.SelectedProperties.Any())
            {
                results = new object[redisQuery.EntityType.Properties.Count];

                // HGETALL (all properties)
                var redisHashEntries = GetUnderlyingDatabase().HashGetAll(dataKeyName)
                    .ToDictionary(he => he.Name, he => he.Value);

                foreach (var property in redisQuery.EntityType.Properties)
                {
                    // Note: since null's are stored in the database as the absence of the column name in the hash
                    // need to insert null's into the objectArray at the appropriate places.
                    RedisValue propertyRedisValue;
                    results[property.Index] =
                        redisHashEntries.TryGetValue(property.Name, out propertyRedisValue)
                            ? decoder(propertyRedisValue, property)
                            : null;
                }
            }
            else
            {
                var selectedPropertiesArray = redisQuery.SelectedProperties.ToArray();
                results = new object[selectedPropertiesArray.Length];

                // HMGET (selected properties)
                var fields = selectedPropertiesArray.Select(p => (RedisValue)p.Name).ToArray();
                var redisHashEntries = GetUnderlyingDatabase().HashGet(dataKeyName, fields);
                for (var i = 0; i < selectedPropertiesArray.Length; i++)
                {
                    results[i] =
                        redisHashEntries[i].IsNull
                            ? null
                            : decoder(redisHashEntries[i], selectedPropertiesArray[i]);
                }
            }

            return results;
        }

        // returns the object array representing all the properties
        // from an EntityType with a particular primary key
        private async Task<object[]> GetQueryObjectsFromDatabaseAsync(
            string primaryKey, RedisQuery redisQuery, Func<byte[], IProperty, object> decoder)
        {
            object[] results = null;
            var dataKeyName = ConstructRedisDataKeyName(redisQuery.EntityType, primaryKey);

            if (redisQuery.SelectedProperties == null
                || !redisQuery.SelectedProperties.Any())
            {
                results = new object[redisQuery.EntityType.Properties.Count];

                // Async HGETALL
                var redisHashEntries = await GetUnderlyingDatabase().HashGetAllAsync(dataKeyName);

                foreach (var property in redisQuery.EntityType.Properties)
                {
                    var redisHashEntriesDictionary = redisHashEntries.ToDictionary(he => he.Name, he => he.Value);
                    // Note: since null's are stored in the database as the absence of the column name in the hash
                    // need to insert null's into the objectArray at the appropriate places.
                    RedisValue propertyRedisValue;
                    if (redisHashEntriesDictionary.TryGetValue(property.Name, out propertyRedisValue))
                    {
                        results[property.Index] = decoder(propertyRedisValue, property);
                    }
                    else
                    {
                        results[property.Index] = null;
                    }
                }
            }
            else
            {
                var selectedPropertiesArray = redisQuery.SelectedProperties.ToArray();
                results = new object[selectedPropertiesArray.Length];

                // Async HMGET
                var fields = selectedPropertiesArray.Select(p => (RedisValue)p.Name).ToArray();
                var redisHashEntries = await GetUnderlyingDatabase().HashGetAsync(dataKeyName, fields);
                for (var i = 0; i < selectedPropertiesArray.Length; i++)
                {
                    results[i] =
                        redisHashEntries[i].IsNull
                            ? null
                            : decoder(redisHashEntries[i], selectedPropertiesArray[i]);
                }
            }

            return results;
        }

        /// <summary>
        ///     Get the next generated value for the given property
        /// </summary>
        /// <param name="property">the property for which to get the next generated value</param>
        /// <param name="incrementBy">when getting blocks of values, set this to the block size, otherwise use 1</param>
        /// <param name="sequenceName">
        ///     the name under which the generated sequence is kept on the underlying database, can be null
        ///     to use default name
        /// </param>
        /// <returns>The next generated value</returns>
        public virtual long GetNextGeneratedValue([NotNull] IProperty property, long incrementBy, [CanBeNull] string sequenceName)
        {
            Check.NotNull(property, "property");

            if (sequenceName == null)
            {
                sequenceName = ConstructRedisValueGeneratorKeyName(property);
            }

            // INCRBY
            return GetUnderlyingDatabase().StringIncrement(sequenceName, incrementBy);
        }

        /// <summary>
        ///     Get the next generated value for the given property
        /// </summary>
        /// <param name="property">the property for which to get the next generated value</param>
        /// <param name="incrementBy">when getting blocks of values, set this to the block size, otherwise use 1</param>
        /// <param name="sequenceName">
        ///     the name under which the generated sequence is kept on the underlying database, can be null
        ///     to use default name
        /// </param>
        /// <param name="cancellationToken">propagates notification that operations should be canceled</param>
        /// <returns>The next generated value</returns>
        public virtual Task<long> GetNextGeneratedValueAsync(
            [NotNull] IProperty property, long incrementBy,
            [CanBeNull] string sequenceName, CancellationToken cancellationToken)
        {
            Check.NotNull(property, "property");

            cancellationToken.ThrowIfCancellationRequested();

            if (sequenceName == null)
            {
                sequenceName = ConstructRedisValueGeneratorKeyName(property);
            }

            // Async INCRBY
            return GetUnderlyingDatabase().StringIncrementAsync(sequenceName, incrementBy);
        }

        private static string EncodeKeyValue([NotNull] object propertyValue)
        {
            Check.NotNull(propertyValue, "propertyValue");

            return Escape(Convert.ToString(propertyValue));
        }

        private static string Escape(string s)
        {
            return s.Replace(KeyNameSeparator, EscapedKeyNameSeparator);
        }

        private static byte[] EncodeAsBytes(object value)
        {
            Check.NotNull(value, "value");

            return value as byte[] ??
                   Encoding.UTF8.GetBytes(Convert.ToString(value));
        }

        private static object DecodeBytes([NotNull] byte[] bytes, [NotNull] IProperty property)
        {
            Check.NotNull(bytes, "bytes");
            Check.NotNull(property, "property");

            if (property.PropertyType == typeof(byte[]))
            {
                return bytes;
            }

            var value = Encoding.UTF8.GetString(bytes, 0, bytes.Length);

            if (value == null)
            {
                throw new ArgumentException(
                    string.Format(
                        CultureInfo.InvariantCulture,
                        Strings.InvalidDatabaseValue,
                        "[" + string.Join(",", bytes.AsEnumerable()) + "]"));
            }

            var underlyingType = property.UnderlyingType;

            if (underlyingType == typeof(string))
            {
                return value;
            }
            if (underlyingType == typeof(Int32))
            {
                return MaybeNullable(Convert.ToInt32(value), property);
            }
            if (underlyingType == typeof(Int64))
            {
                return MaybeNullable(Convert.ToInt64(value), property);
            }
            if (underlyingType == typeof(Double))
            {
                return MaybeNullable(Convert.ToDouble(value), property);
            }
            if (underlyingType == typeof(Decimal))
            {
                return MaybeNullable(Convert.ToDecimal(value), property);
            }
            if (underlyingType == typeof(DateTime))
            {
                return MaybeNullable(DateTime.Parse(value), property);
            }
            if (underlyingType == typeof(DateTimeOffset))
            {
                return MaybeNullable(DateTimeOffset.Parse(value), property);
            }
            if (underlyingType == typeof(Single))
            {
                return MaybeNullable(Convert.ToSingle(value), property);
            }
            if (underlyingType == typeof(Boolean))
            {
                return MaybeNullable(Convert.ToBoolean(value), property);
            }
            if (underlyingType == typeof(Byte))
            {
                return MaybeNullable(Convert.ToByte(value), property);
            }
            if (underlyingType == typeof(UInt32))
            {
                return MaybeNullable(Convert.ToUInt32(value), property);
            }
            if (underlyingType == typeof(UInt64))
            {
                return MaybeNullable(Convert.ToUInt64(value), property);
            }
            if (underlyingType == typeof(Int16))
            {
                return MaybeNullable(Convert.ToInt16(value), property);
            }
            if (underlyingType == typeof(UInt16))
            {
                return MaybeNullable(Convert.ToUInt16(value), property);
            }
            if (underlyingType == typeof(Char))
            {
                return MaybeNullable(Convert.ToChar(value), property);
            }
            if (underlyingType == typeof(SByte))
            {
                return MaybeNullable(Convert.ToSByte(value), property);
            }

            throw new ArgumentOutOfRangeException("property",
                string.Format(
                    CultureInfo.InvariantCulture,
                    Strings.UnableToDecodeProperty,
                    property.Name,
                    property.PropertyType.FullName,
                    property.EntityType.Name));
        }

        private static object MaybeNullable<T>(T value, IProperty property)
            where T : struct
        {
            if (property.IsNullable)
            {
                return (T?)value;
            }

            return value;
        }

        private sealed class AsyncEnumerable : IAsyncEnumerable<object[]>
        {
            private readonly RedisDatabase _redisDatabase;
            private readonly RedisQuery _redisQuery;

            public AsyncEnumerable(
                RedisDatabase redisDatabase,
                RedisQuery redisQuery)
            {
                _redisDatabase = redisDatabase;
                _redisQuery = redisQuery;
            }

            public IAsyncEnumerator<object[]> GetEnumerator()
            {
                return new AsyncEnumerator(this);
            }

            private sealed class AsyncEnumerator : IAsyncEnumerator<object[]>
            {
                private readonly AsyncEnumerable _enumerable;
                private RedisValue[] _entityKeysForQuery;
                private int _currentOffset = -1;
                private object[] _current;
                private bool _disposed;

                public AsyncEnumerator(AsyncEnumerable enumerable)
                {
                    _enumerable = enumerable;
                }

                public async Task<bool> MoveNext(CancellationToken cancellationToken)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    if (_entityKeysForQuery == null)
                    {
                        await InitializeRedisKeys(cancellationToken);
                    }

                    var hasNext = (++_currentOffset < _entityKeysForQuery.Length);
                    if (!hasNext)
                    {
                        _current = null;
                        // H.A.C.K.: Workaround https://github.com/Reactive-Extensions/Rx.NET/issues/5
                        Dispose();
                        return false;
                    }

                    _current = await _enumerable._redisDatabase.GetQueryObjectsFromDatabaseAsync(
                        _entityKeysForQuery[_currentOffset],
                        _enumerable._redisQuery,
                        DecodeBytes);

                    return true;
                }

                private async Task InitializeRedisKeys(CancellationToken cancellationToken)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    var redisPrimaryKeyIndexKeyName =
                        ConstructRedisPrimaryKeyIndexKeyName(_enumerable._redisQuery.EntityType);

                    _entityKeysForQuery = await _enumerable
                        ._redisDatabase.GetUnderlyingDatabase().SetMembersAsync(redisPrimaryKeyIndexKeyName);
                }

                public object[] Current
                {
                    get
                    {
                        if (_current == null)
                        {
                            throw new InvalidOperationException();
                        }

                        return _current;
                    }
                }

                public void Dispose()
                {
                    if (!_disposed)
                    {
                        _disposed = true;
                    }
                }
            }
        }
    }
}
