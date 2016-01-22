// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Storage
{
    public abstract class RelationalDatabaseCreator : IRelationalDatabaseCreator
    {
        private readonly IMigrationsModelDiffer _modelDiffer;
        private readonly IMigrationsSqlGenerator _migrationsSqlGenerator;

        protected RelationalDatabaseCreator(
            [NotNull] IModel model,
            [NotNull] IRelationalConnection connection,
            [NotNull] IMigrationsModelDiffer modelDiffer,
            [NotNull] IMigrationsSqlGenerator migrationsSqlGenerator)
        {
            Check.NotNull(model, nameof(model));
            Check.NotNull(connection, nameof(connection));
            Check.NotNull(modelDiffer, nameof(modelDiffer));
            Check.NotNull(migrationsSqlGenerator, nameof(migrationsSqlGenerator));

            Model = model;
            Connection = connection;
            _modelDiffer = modelDiffer;
            _migrationsSqlGenerator = migrationsSqlGenerator;
        }

        protected virtual IModel Model { get; }
        protected virtual IRelationalConnection Connection { get; }

        public abstract bool Exists();

        public virtual Task<bool> ExistsAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();

            return Task.FromResult(Exists());
        }

        public abstract void Create();

        public virtual Task CreateAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();

            Create();

            return Task.FromResult(0);
        }

        public abstract void Delete();

        public virtual Task DeleteAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();

            Delete();

            return Task.FromResult(0);
        }

        public virtual void CreateTables()
        {
            var commands = GetCreateTablesCommands();

            using (var transaction = Connection.BeginTransaction())
            {
                commands.ExecuteNonQuery(Connection);

                transaction.Commit();
            }
        }

        public virtual async Task CreateTablesAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            var commands = GetCreateTablesCommands();

            using (var transaction = await Connection.BeginTransactionAsync(cancellationToken))
            {
                await commands.ExecuteNonQueryAsync(Connection, cancellationToken);

                transaction.Commit();
            }
        }

        protected virtual IEnumerable<IRelationalCommand> GetCreateTablesCommands()
            => _migrationsSqlGenerator.Generate(_modelDiffer.GetDifferences(null, Model), Model);

        protected abstract bool HasTables();

        protected virtual Task<bool> HasTablesAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();

            return Task.FromResult(HasTables());
        }

        public virtual bool EnsureDeleted()
        {
            if (Exists())
            {
                Delete();
                return true;
            }
            return false;
        }

        public virtual async Task<bool> EnsureDeletedAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            if (await ExistsAsync(cancellationToken))
            {
                await DeleteAsync(cancellationToken);

                return true;
            }
            return false;
        }

        public virtual bool EnsureCreated()
        {
            if (!Exists())
            {
                Create();
                CreateTables();
                return true;
            }

            if (!HasTables())
            {
                CreateTables();
                return true;
            }

            return false;
        }

        public virtual async Task<bool> EnsureCreatedAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            if (!await ExistsAsync(cancellationToken))
            {
                await CreateAsync(cancellationToken);
                await CreateTablesAsync(cancellationToken);

                return true;
            }

            if (!await HasTablesAsync(cancellationToken))
            {
                await CreateTablesAsync(cancellationToken);

                return true;
            }

            return false;
        }
    }
}
