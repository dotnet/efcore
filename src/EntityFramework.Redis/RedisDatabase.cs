// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
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

        private IDatabase _redisDatabase;
        private IServer _redisServer;

        public RedisDatabase([NotNull] DbContextConfiguration configuration)
            : base(configuration)
        {
        }

        public virtual IDatabase UnderlyingDatabase
        {
            get
            {
                if (_redisDatabase == null)
                {
                    var connection = (RedisConnection)Configuration.Connection;
                    var connectionMultiplexer =
                        ConnectionMultiplexer.Connect(connection.ConnectionString);
                    _redisDatabase =
                        connectionMultiplexer.GetDatabase(connection.Database);
                }

                return _redisDatabase;
            }

            // For test purposes only
            internal set { _redisDatabase = value; }
        }

        public virtual IServer UnderlyingServer
        {
            get
            {
                if (_redisServer == null)
                {
                    var connection = (RedisConnection)Configuration.Connection;
                    var configOptions = ConfigurationOptions.Parse(connection.ConnectionString);
                    configOptions.AllowAdmin = true; // require Admin access for e.g. FlushDatabase
                    var connectionMultiplexer =
                        ConnectionMultiplexer.Connect(configOptions);
                    _redisServer =
                        connectionMultiplexer.GetServer(connection.ConnectionString);
                }

                return _redisServer;
            }

            // For test purposes only
            internal set { _redisServer = value; }
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
            var txn = UnderlyingDatabase.CreateTransaction();
            foreach (var entry in stateEntries)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    return 0;
                }

                switch (entry.EntityState)
                {
                    case EntityState.Added:
                        AddInsertEntryCommands(txn, entry);
                        break;

                    case EntityState.Deleted:
                        AddDeleteEntryCommands(txn, entry);
                        break;

                    case EntityState.Modified:
                        AddModifyEntryCommands(txn, entry);
                        break;
                }

                if (await txn.ExecuteAsync())
                {
                    entitiesProcessed = stateEntries.Count;
                }
            }

            return entitiesProcessed;
        }

        /// <summary>
        ///     Deletes all keys in the database
        /// </summary>
        public virtual void FlushDatabase()
        {
            var connection = (RedisConnection)Configuration.Connection;
            UnderlyingServer.FlushDatabase(connection.Database);
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
            await UnderlyingServer.FlushDatabaseAsync(connection.Database);
        }

        private void AddInsertEntryCommands(ITransaction txn, StateEntry stateEntry)
        {
            var compositePrimaryKeyValues = string.Join(
                PropertyValueSeparator,
                stateEntry.EntityType.GetKey().Properties.Select(p => EncodeKeyValue(stateEntry, p)));

            var redisDataKeyName = string.Format(CultureInfo.InvariantCulture, DataHashNameFormat, stateEntry.EntityType.Name, compositePrimaryKeyValues);


            // Note: null entries are stored as the absence of the property_name-property_value pair in the hash
            var entries =
                stateEntry.EntityType.Properties
                    .Where(p => stateEntry[p] != null)
                    .Select(p => new HashEntry(p.Name, EncodeAsBytes(stateEntry[p]))).ToArray();
            txn.HashSetAsync(redisDataKeyName, entries);

            var redisPrimaryKeyIndexKeyName = string.Format(CultureInfo.InvariantCulture, PrimaryKeyIndexNameFormat, stateEntry.EntityType.Name);
            txn.SetAddAsync(redisPrimaryKeyIndexKeyName, compositePrimaryKeyValues);
        }

        private void AddDeleteEntryCommands(ITransaction txn, StateEntry stateEntry)
        {
            var compositePrimaryKeyValues = string.Join(
                PropertyValueSeparator,
                stateEntry.EntityType.GetKey().Properties.Select(p => EncodeKeyValue(stateEntry, p)));

            var redisDataKeyName = string.Format(CultureInfo.InvariantCulture, DataHashNameFormat, stateEntry.EntityType.Name, compositePrimaryKeyValues);
            txn.KeyDeleteAsync(redisDataKeyName);

            var redisPrimaryKeyIndexKeyName = string.Format(CultureInfo.InvariantCulture, PrimaryKeyIndexNameFormat, stateEntry.EntityType.Name);
            txn.SetRemoveAsync(redisPrimaryKeyIndexKeyName, compositePrimaryKeyValues);
        }

        private void AddModifyEntryCommands(ITransaction txn, StateEntry stateEntry)
        {
            var compositePrimaryKeyValues =
                string.Join(
                    PropertyValueSeparator,
                    stateEntry.EntityType.GetKey().Properties.Select(p => EncodeKeyValue(stateEntry, p)));

            var redisPrimaryKeyIndexKeyName = string.Format(CultureInfo.InvariantCulture, PrimaryKeyIndexNameFormat, stateEntry.EntityType.Name);
            var redisDataKeyName = string.Format(CultureInfo.InvariantCulture, DataHashNameFormat, stateEntry.EntityType.Name, compositePrimaryKeyValues);
            txn.AddCondition(Condition.KeyExists(redisPrimaryKeyIndexKeyName));

            // first delete all the hash entries which have changed to null
            var changingToNullEntries = stateEntry.EntityType.Properties
                .Where(p => stateEntry.IsPropertyModified(p) && stateEntry[p] == null)
                .Select(p => (RedisValue)p.Name).ToArray();
            txn.HashDeleteAsync(redisDataKeyName, changingToNullEntries);

            // now update all the other entries
            var updatedEntries = stateEntry.EntityType.Properties
                .Where(p => stateEntry.IsPropertyModified(p) && stateEntry[p] != null)
                .Select(p => new HashEntry(p.Name, EncodeAsBytes(stateEntry[p]))).ToArray();
            txn.HashSetAsync(redisDataKeyName, updatedEntries);
        }

        private static string EncodeKeyValue(
            StateEntry stateEntry, IPropertyBase p)
        {
            var value = stateEntry[p];
            if (value == null)
            {
                throw new ArgumentException(
                    string.Format(
                        CultureInfo.InvariantCulture,
                        Strings.InvalidPrimaryKeyValue,
                        stateEntry.EntityType.Name,
                        p.Name));
            }

            return Convert.ToString(value).Replace(KeyNameSeparator, EscapedKeyNameSeparator);
        }

        private static byte[] EncodeAsBytes(object value)
        {
            Check.NotNull(value, "value");

            return value as byte[] ??
                   Encoding.UTF8.GetBytes(Convert.ToString(value));
        }

        private static object DecodeBytes(byte[] bytes, IProperty property)
        {
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

            var propertyType = property.PropertyType;

            if (typeof(string) == propertyType)
            {
                return value;
            }
            else if (typeof(Int32) == propertyType)
            {
                return MaybeNullable<Int32>(Convert.ToInt32(value), property);
            }
            else if (typeof(Int64) == propertyType)
            {
                return MaybeNullable<Int64>(Convert.ToInt64(value), property);
            }
            else if (typeof(Double) == propertyType)
            {
                return MaybeNullable<Double>(Convert.ToDouble(value), property);
            }
            else if (typeof(DateTime) == propertyType)
            {
                return MaybeNullable<DateTime>(DateTime.Parse(value), property);
            }
            else if (typeof(Single) == propertyType)
            {
                return MaybeNullable<Single>(Convert.ToSingle(value), property);
            }
            else if (typeof(Boolean) == propertyType)
            {
                return MaybeNullable<Boolean>(Convert.ToBoolean(value), property);
            }
            else if (typeof(Byte) == propertyType)
            {
                return MaybeNullable<Byte>(Convert.ToByte(value), property);
            }
            else if (typeof(UInt32) == propertyType)
            {
                return MaybeNullable<UInt32>(Convert.ToUInt32(value), property);
            }
            else if (typeof(UInt64) == propertyType)
            {
                return MaybeNullable<UInt64>(Convert.ToUInt64(value), property);
            }
            else if (typeof(Int16) == propertyType)
            {
                return MaybeNullable<Int16>(Convert.ToInt16(value), property);
            }
            else if (typeof(UInt16) == propertyType)
            {
                return MaybeNullable<UInt16>(Convert.ToUInt16(value), property);
            }
            else if (typeof(Char) == propertyType)
            {
                return MaybeNullable<Char>(Convert.ToChar(value), property);
            }
            else if (typeof(SByte) == propertyType)
            {
                return MaybeNullable<SByte>(Convert.ToSByte(value), property);
            }
            else
            {
                throw new ArgumentOutOfRangeException("property",
                    string.Format(
                        CultureInfo.InvariantCulture,
                        Strings.UnableToDecodeProperty,
                        property.Name,
                        propertyType.FullName));
            }
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
