// Copyright (c) Microsoft Open Technologies, Inc.
// All Rights Reserved
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
// http://www.apache.org/licenses/LICENSE-2.0
// 
// THIS CODE IS PROVIDED *AS IS* BASIS, WITHOUT WARRANTIES OR
// CONDITIONS OF ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING
// WITHOUT LIMITATION ANY IMPLIED WARRANTIES OR CONDITIONS OF
// TITLE, FITNESS FOR A PARTICULAR PURPOSE, MERCHANTABLITY OR
// NON-INFRINGEMENT.
// See the Apache 2 License for the specific language governing
// permissions and limitations under the License.

using System;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Infrastructure;
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

        protected RelationalConnection([NotNull] DbContextConfiguration configuration)
        {
            Check.NotNull(configuration, "configuration");

            var storeConfigs = configuration.ContextOptions.Extensions
                .OfType<RelationalConfigurationExtension>()
                .ToArray();

            if (storeConfigs.Length == 0)
            {
                throw new InvalidOperationException(Strings.FormatNoDataStoreConfigured());
            }

            if (storeConfigs.Length > 1)
            {
                throw new InvalidOperationException(Strings.FormatMultipleDataStoresConfigured());
            }

            var storeConfig = storeConfigs[0];

            if (storeConfig.Connection != null)
            {
                if (!string.IsNullOrWhiteSpace(storeConfig.ConnectionString))
                {
                    throw new InvalidOperationException(Strings.FormatConnectionAndConnectionString());
                }

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
                throw new InvalidOperationException(Strings.FormatNoConnectionOrConnectionString());
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
            if (_openedCount == 0)
            {
                _connection.Value.Open();
            }
            _openedCount++;
        }

        public virtual async Task OpenAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            if (_openedCount == 0)
            {
                await _connection.Value.OpenAsync(cancellationToken);
            }
            _openedCount++;
        }

        // Sporadic failure when running Async query tests
        public virtual void Close()
        {
            // TODO: Consider how to handle open/closing to make sure that a connection that is passed in
            // as open is never erroneously closed without placing undue burdon on users of the connection.
            // Disabled: See GitHub #141
            // Contract.Assert(_openedCount > 0);
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
                _openedCount = 0;
            }
        }
    }
}
