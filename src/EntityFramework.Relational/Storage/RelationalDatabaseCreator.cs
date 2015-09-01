// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Migrations;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Storage
{
    public abstract class RelationalDatabaseCreator : IRelationalDatabaseCreator
    {
        private readonly IMigrationsModelDiffer _modelDiffer;
        private readonly IMigrationsSqlGenerator _migrationsSqlGenerator;
        private readonly IRelationalCommandBuilderFactory _commandBuilderFactory;

        protected RelationalDatabaseCreator(
            [NotNull] IModel model,
            [NotNull] IRelationalConnection connection,
            [NotNull] IMigrationsModelDiffer modelDiffer,
            [NotNull] IMigrationsSqlGenerator migrationsSqlGenerator,
            [NotNull] IRelationalCommandBuilderFactory commandBuilderFactory)
        {
            Check.NotNull(model, nameof(model));
            Check.NotNull(connection, nameof(connection));
            Check.NotNull(modelDiffer, nameof(modelDiffer));
            Check.NotNull(migrationsSqlGenerator, nameof(migrationsSqlGenerator));
            Check.NotNull(commandBuilderFactory, nameof(commandBuilderFactory));

            Model = model;
            Connection = connection;
            _modelDiffer = modelDiffer;
            _migrationsSqlGenerator = migrationsSqlGenerator;
            _commandBuilderFactory = commandBuilderFactory;
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
            foreach (var command in GetCreateTablesCommands())
            {
                command.ExecuteNonQuery(Connection);
            }
        }

        public virtual async Task CreateTablesAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            foreach (var command in GetCreateTablesCommands())
            {
                await command.ExecuteNonQueryAsync(Connection, cancellationToken);
            }
        }

        protected virtual IEnumerable<IRelationalCommand> GetCreateTablesCommands()
            => _migrationsSqlGenerator.Generate(_modelDiffer.GetDifferences(null, Model), Model);

        protected virtual bool HasTables()
            => EvaluateHasTablesResult(GetHasTablesCommand().ExecuteScalar(Connection));

        protected virtual async Task<bool> HasTablesAsync(CancellationToken cancellationToken = default(CancellationToken))
            => EvaluateHasTablesResult(await GetHasTablesCommand().ExecuteScalarAsync(Connection, cancellationToken));

        protected abstract bool EvaluateHasTablesResult([CanBeNull] object result);

        protected virtual IRelationalCommand GetHasTablesCommand()
            => _commandBuilderFactory
                .Create()
                .Append(GetHasTablesSql())
                .BuildRelationalCommand();

        protected abstract string GetHasTablesSql();

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
