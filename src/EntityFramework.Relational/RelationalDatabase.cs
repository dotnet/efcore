// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Relational.Migrations;
using Microsoft.Data.Entity.Storage;
using Microsoft.Data.Entity.Utilities;
using Microsoft.Framework.Logging;

namespace Microsoft.Data.Entity.Relational
{
    public class RelationalDatabase : Database, IAccessor<IMigrator>
    {
        private readonly IMigrator _migrator;

        public RelationalDatabase(
            [NotNull] DbContext context,
            [NotNull] IRelationalDataStoreCreator dataStoreCreator,
            [NotNull] IRelationalConnection connection,
            [NotNull] IMigrator migrator,
            [NotNull] ILoggerFactory loggerFactory)
            : base(context, dataStoreCreator, loggerFactory)
        {
            Check.NotNull(migrator, nameof(migrator));
            Check.NotNull(connection, nameof(connection));

            _migrator = migrator;
            Connection = connection;
        }

        public virtual void ApplyMigrations() => _migrator.ApplyMigrations();

        IMigrator IAccessor<IMigrator>.Service => _migrator;

        public virtual IRelationalConnection Connection { get; }

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

        public virtual void CreateTables() => RelationalDataStoreCreator.CreateTables(Model);

        public virtual Task CreateTablesAsync(CancellationToken cancellationToken = default(CancellationToken))
            => RelationalDataStoreCreator.CreateTablesAsync(Model, cancellationToken);

        public virtual void Delete() => RelationalDataStoreCreator.Delete();

        public virtual Task DeleteAsync(CancellationToken cancellationToken = default(CancellationToken))
            => RelationalDataStoreCreator.DeleteAsync(cancellationToken);

        public virtual bool Exists()
            => RelationalDataStoreCreator.Exists();

        public virtual Task<bool> ExistsAsync(CancellationToken cancellationToken = default(CancellationToken))
            => RelationalDataStoreCreator.ExistsAsync(cancellationToken);

        public virtual bool HasTables() => RelationalDataStoreCreator.HasTables();

        public virtual Task<bool> HasTablesAsync(CancellationToken cancellationToken = default(CancellationToken))
            => RelationalDataStoreCreator.HasTablesAsync(cancellationToken);

        private IRelationalDataStoreCreator RelationalDataStoreCreator => (IRelationalDataStoreCreator)((IAccessor<IDataStoreCreator>)this).Service;

        private ILogger Logger => ((IAccessor<ILogger>)this).Service;

        private IModel Model => ((IAccessor<IModel>)this).Service;
    }
}
