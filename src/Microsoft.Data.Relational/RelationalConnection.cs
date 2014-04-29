// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Data.Entity;
using Microsoft.Data.Entity.Storage;
using Microsoft.Data.Entity.Utilities;
using Microsoft.Data.Relational.Utilities;

namespace Microsoft.Data.Relational
{
    public abstract class RelationalConnection : DataStoreConnection, IDisposable
    {
        private readonly string _connectionString;
        private readonly LazyRef<DbConnection> _connection;
        private readonly bool _connectionOwned;
        private int _openedCount;

        /// <summary>
        ///     This constructor is intended only for use when creating test doubles that will override members
        ///     with mocked or faked behavior. Use of this constructor for other purposes may result in unexpected
        ///     behavior including but not limited to throwing <see cref="NullReferenceException" />.
        /// </summary>
        protected RelationalConnection()
        {
        }

        protected RelationalConnection([NotNull] ContextConfiguration configuration)
        {
            Check.NotNull(configuration, "configuration");

            var storeConfigs = configuration.EntityConfiguration.Extensions
                .OfType<RelationalConfigurationExtension>()
                .ToArray();

            if (storeConfigs.Length == 0)
            {
                // TODO: Proper message
                throw new InvalidOperationException("Configuration not found.");
            }

            if (storeConfigs.Length > 1)
            {
                // TODO: Proper message
                throw new InvalidOperationException("Multiple configurations found.");
            }

            var storeConfig = storeConfigs[0];

            if (storeConfig.Connection != null)
            {
                _connection = new LazyRef<DbConnection>(() => storeConfig.Connection);
                _connectionOwned = false;
                _openedCount = storeConfig.Connection.State == ConnectionState.Open ? 1 : 0;
            }
            else if (!string.IsNullOrWhiteSpace(storeConfig.ConnectionString))
            {
                _connectionString = storeConfig.ConnectionString;
                _connection = new LazyRef<DbConnection>(CreateDbConnection);
                _connectionOwned = true;
            }
            else
            {
                // TODO: Proper message
                throw new InvalidOperationException("No connection.");
            }
        }

        protected abstract DbConnection CreateDbConnection();

        public virtual string ConnectionString
        {
            get { return _connectionString ?? _connection.Value.ConnectionString; }
        }

        public virtual DbConnection DbConnection
        {
            get { return _connection.Value; }
        }

        public virtual void Open()
        {
            if (_openedCount++ == 0)
            {
                _connection.Value.Open();
            }
        }

        public virtual async Task OpenAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            if (_openedCount == 0)
            {
                await _connection.Value.OpenAsync(cancellationToken);
                // Only increment count if Open call succeeds
                _openedCount++;
            }
        }

        public virtual void Close()
        {
            if (--_openedCount == 0)
            {
                _connection.Value.Close();
            }
        }

        public virtual void Dispose()
        {
            if (_connectionOwned && _connection.HasValue)
            {
                _connection.Value.Dispose();
                _connection.Reset(CreateDbConnection);
            }
        }
    }
}
