// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Data.Entity.AzureTableStorage.Utilities;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Metadata;

namespace Microsoft.Data.Entity.AzureTableStorage
{
    public class AtsDatabase : Database
    {
        private readonly AtsConnection _connection;
        private readonly IModel _model;

        public AtsDatabase([NotNull] DbContextConfiguration configuration, [NotNull] AtsConnection connection)
            : base(configuration)
        {
            Check.NotNull(connection, "connection");
            _connection = connection;
            _model = configuration.Model;
        }

        public virtual void CreateTables()
        {
            foreach (var type in _model.EntityTypes)
            {
                var table = _connection.GetTableReference(type.StorageName);
                if (table != null)
                {
                    table.CreateIfNotExists();
                }
            }
        }

        public virtual async Task CreateTablesAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            foreach (var type in _model.EntityTypes)
            {
                var table = _connection.GetTableReference(type.StorageName);
                if (table != null)
                {
                    await table.CreateIfNotExistsAsync(cancellationToken);
                }
            }
        }

        public virtual bool HasTables()
        {
            return _connection.Account.CreateCloudTableClient().ListTables().Any();
        }

        public virtual Task<bool> HasTablesAsync(CancellationToken cancellationToken = new CancellationToken())
        {
            return Task.Run(() => HasTables(), cancellationToken);
        }

        public virtual void DeleteTables()
        {
            foreach (var type in _model.EntityTypes)
            {
                var table = _connection.GetTableReference(type.StorageName);
                if (table != null)
                {
                    table.DeleteIfExists();
                }
            }
        }
    }
}
