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
using Microsoft.Data.Entity.Query;
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

        public virtual async Task<int> SaveChangesAsync(
            [NotNull] IReadOnlyList<StateEntry> stateEntries,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            Check.NotNull(stateEntries, "stateEntries");

            if (cancellationToken.IsCancellationRequested)
            {
                return 0;
            }

            var entitiesProcessed = 0;
            var transaction = GetUnderlyingDatabase().CreateTransaction();
            foreach (var entry in stateEntries)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    return 0;
                }

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

                if (await transaction.ExecuteAsync().ConfigureAwait(continueOnCapturedContext: false))
                {
                    entitiesProcessed = stateEntries.Count;
                }
            }

            return entitiesProcessed;
        }

        /// <summary>
        ///     Gets values from database and materializes new EntityTypes
        /// </summary>
        /// <typeparam name="TResult">type of expected result</typeparam>
        /// <param name="entityType">EntityType of </param>
        /// <param name="queryBuffer"></param>
        /// <returns>An Enumerable of materialized EntityType objects</returns>
        public virtual IEnumerable<TResult> GetMaterializedResults<TResult>(
            [NotNull] IEntityType entityType,
            [NotNull] IQueryBuffer queryBuffer)
        {
            Check.NotNull(entityType, "entityType");
            Check.NotNull(queryBuffer, "queryBuffer");

            var redisPrimaryKeyIndexKeyName
                = ConstructRedisPrimaryKeyIndexKeyName(entityType);

            var allKeysForEntity
                = GetUnderlyingDatabase().SetMembers(redisPrimaryKeyIndexKeyName);

            return allKeysForEntity
                .Select(compositePrimaryKeyValues
                    => GetEntityQueryObjectsFromDatabase(compositePrimaryKeyValues, entityType, DecodeBytes))
                .Select(objectArrayFromHash
                    => (TResult)queryBuffer
                        .GetEntity(entityType, new ObjectArrayValueReader(objectArrayFromHash)));
        }

        /// <summary>
        ///     Gets non-materialized values from database
        /// </summary>
        /// <param name="redisQuery">Query data to decide what is selected from the database</param>
        /// <returns>An Enumerable of non-materialized object[]'s each of which represents one primary key</returns>
        public virtual IEnumerable<object[]> GetResults([NotNull] RedisQuery redisQuery)
        {
            Check.NotNull(redisQuery, "redisQuery");

            var redisPrimaryKeyIndexKeyName =
                ConstructRedisPrimaryKeyIndexKeyName(redisQuery.EntityType);

            var allKeysForEntity = GetUnderlyingDatabase().SetMembers(redisPrimaryKeyIndexKeyName).AsEnumerable();
            return allKeysForEntity
                .Select(compositePrimaryKeyValue =>
                    GetProjectionQueryObjectsFromDatabase(compositePrimaryKeyValue, redisQuery.EntityType, redisQuery.SelectedProperties, DecodeBytes));
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
            if (cancellationToken.IsCancellationRequested)
            {
                return;
            }

            var connection = (RedisConnection)Configuration.Connection;

            await GetUnderlyingServer().FlushDatabaseAsync(connection.Database).ConfigureAwait(continueOnCapturedContext: false);
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

        private static string ConstructKeyValue(
            StateEntry stateEntry, Func<StateEntry, IProperty, object> propertyValueSelector)
        {
            return string.Join(
                PropertyValueSeparator,
                stateEntry.EntityType.GetPrimaryKey().Properties.Select(p => EncodeKeyValue(propertyValueSelector(stateEntry, p))));
        }

        // returns the object array representing all the properties
        // from an EntityType with a particular primary key
        private object[] GetEntityQueryObjectsFromDatabase(
            string compositePrimaryKeyValues, IEntityType entityType, Func<byte[], IProperty, object> decoder)
        {
            var results = new object[entityType.Properties.Count];

            // HGETALL
            var redisHashEntries = GetUnderlyingDatabase().HashGetAll(
                ConstructRedisDataKeyName(entityType, compositePrimaryKeyValues))
                .ToDictionary(he => he.Name, he => he.Value);

            foreach (var property in entityType.Properties)
            {
                // Note: since null's are stored in the database as the absence of the column name in the hash
                // need to insert null's into the objectArray at the appropriate places.
                RedisValue propertyRedisValue;
                if (redisHashEntries.TryGetValue(property.Name, out propertyRedisValue))
                {
                    results[property.Index] = decoder(propertyRedisValue, property);
                }
                else
                {
                    results[property.Index] = null;
                }
            }

            return results;
        }

        /// <returns>
        ///     returns the object[] representing the set of selected properties from
        ///     an EntityType with a particular primary key
        /// </returns>
        private object[] GetProjectionQueryObjectsFromDatabase(
            string primaryKey, IEntityType entityType,
            IEnumerable<IProperty> selectedProperties, Func<byte[], IProperty, object> decoder)
        {
            var selectedPropertiesArray = selectedProperties.ToArray();
            var results = new object[selectedPropertiesArray.Length];

            // HMGET
            var fields = selectedPropertiesArray.Select(p => (RedisValue)p.Name).ToArray();
            var redisHashEntries = GetUnderlyingDatabase().HashGet(
                ConstructRedisDataKeyName(entityType, primaryKey), fields);
            for (var i = 0; i < selectedPropertiesArray.Length; i++)
            {
                results[i] =
                    redisHashEntries[i].IsNull
                        ? null
                        : decoder(redisHashEntries[i], selectedPropertiesArray[i]);
            }

            return results;
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

            if (typeof(string) == underlyingType)
            {
                return value;
            }
            if (typeof(Int32) == underlyingType)
            {
                return MaybeNullable(Convert.ToInt32(value), property);
            }
            if (typeof(Int64) == underlyingType)
            {
                return MaybeNullable(Convert.ToInt64(value), property);
            }
            if (typeof(Double) == underlyingType)
            {
                return MaybeNullable(Convert.ToDouble(value), property);
            }
            if (typeof(Decimal) == underlyingType)
            {
                return MaybeNullable<Decimal>(Convert.ToDecimal(value), property);
            }
            if (typeof(DateTime) == underlyingType)
            {
                return MaybeNullable(DateTime.Parse(value), property);
            }
            if (typeof(Single) == underlyingType)
            {
                return MaybeNullable(Convert.ToSingle(value), property);
            }
            if (typeof(Boolean) == underlyingType)
            {
                return MaybeNullable(Convert.ToBoolean(value), property);
            }
            if (typeof(Byte) == underlyingType)
            {
                return MaybeNullable(Convert.ToByte(value), property);
            }
            if (typeof(UInt32) == underlyingType)
            {
                return MaybeNullable(Convert.ToUInt32(value), property);
            }
            if (typeof(UInt64) == underlyingType)
            {
                return MaybeNullable(Convert.ToUInt64(value), property);
            }
            if (typeof(Int16) == underlyingType)
            {
                return MaybeNullable(Convert.ToInt16(value), property);
            }
            if (typeof(UInt16) == underlyingType)
            {
                return MaybeNullable(Convert.ToUInt16(value), property);
            }
            if (typeof(Char) == underlyingType)
            {
                return MaybeNullable(Convert.ToChar(value), property);
            }
            if (typeof(SByte) == underlyingType)
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
    }
}
