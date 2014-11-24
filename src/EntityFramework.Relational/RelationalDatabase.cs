// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Storage;
using Microsoft.Data.Entity.Utilities;
using Microsoft.Framework.Logging;

namespace Microsoft.Data.Entity.Relational
{
    public abstract class RelationalDatabase : Database
    {
        protected RelationalDatabase(
                   [NotNull] DbContextService<IModel> model,
                   [NotNull] DataStoreCreator dataStoreCreator,
                   [NotNull] DataStoreConnection connection,
                   [NotNull] ILoggerFactory loggerFactory)
            : base(model, dataStoreCreator, connection, loggerFactory)
        {
        }
        public new virtual RelationalConnection Connection
        {
            get { return (RelationalConnection)base.Connection; }
        }

        public virtual void Create()
        {
            Logger.CreatingDatabase(Connection.DbConnection.Database);

            RelationalDataStoreCreator.Create();
        }

        public virtual Task CreateAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            Logger.CreatingDatabase(Connection.DbConnection.Database);

            return RelationalDataStoreCreator.CreateAsync(cancellationToken);
        }

        public virtual void CreateTables()
        {
            RelationalDataStoreCreator.CreateTables(Model);
        }

        public virtual Task CreateTablesAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            return RelationalDataStoreCreator.CreateTablesAsync(Model, cancellationToken);
        }

        public virtual void Delete()
        {
            RelationalDataStoreCreator.Delete();
        }

        public virtual Task DeleteAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            return RelationalDataStoreCreator.DeleteAsync(cancellationToken);
        }

        public virtual bool Exists()
        {
            return RelationalDataStoreCreator.Exists();
        }

        public virtual Task<bool> ExistsAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            return RelationalDataStoreCreator.ExistsAsync(cancellationToken);
        }

        public virtual bool HasTables()
        {
            return RelationalDataStoreCreator.HasTables();
        }

        public virtual Task<bool> HasTablesAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            return RelationalDataStoreCreator.HasTablesAsync(cancellationToken);
        }

        private RelationalDataStoreCreator RelationalDataStoreCreator
        {
            get { return (RelationalDataStoreCreator)base.DataStoreCreator; }
        }
    }
}
