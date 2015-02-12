// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Relational.Migrations.Builders;
using Microsoft.Data.Entity.Relational.Migrations.History;
using Microsoft.Data.Entity.Relational.Migrations.Infrastructure;
using Microsoft.Data.Entity.Relational.Migrations.Sql;
using Microsoft.Data.Entity.Storage;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Relational.Migrations
{
    // TODO: Log
    public class Migrator
    {
        private const string InitialDatabase = "0";

        private readonly MigrationAssembly _migrationAssembly;
        private readonly DbContextService<IHistoryRepository> _historyRepository;
        private readonly LazyRef<RelationalDataStoreCreator> _dataStoreCreator;
        private readonly DbContextService<MigrationSqlGenerator> _sqlGenerator;
        private readonly SqlStatementExecutor _executor;
        private readonly LazyRef<RelationalConnection> _connection;
        private readonly DbContextService<ModelDiffer> _modelDiffer;
        private readonly DbContextService<IModel> _model;
        private readonly MigrationIdGenerator _idGenerator;

        /// <summary>
        ///     This constructor is intended only for use when creating test doubles that will override members
        ///     with mocked or faked behavior. Use of this constructor for other purposes may result in unexpected
        ///     behavior including but not limited to throwing <see cref="NullReferenceException" />.
        /// </summary>
        protected Migrator()
        {
        }

        public Migrator(
            [NotNull] MigrationAssembly migrationAssembly,
            [NotNull] DbContextService<IHistoryRepository> historyRepository,
            [NotNull] DbContextService<DataStoreCreator> dataStoreCreator,
            [NotNull] DbContextService<MigrationSqlGenerator> sqlGenerator,
            [NotNull] SqlStatementExecutor executor,
            [NotNull] DbContextService<DataStoreConnection> connection,
            [NotNull] DbContextService<ModelDiffer> modelDiffer,
            [NotNull] DbContextService<IModel> model,
            [NotNull] MigrationIdGenerator idGenerator)
        {
            Check.NotNull(migrationAssembly, nameof(migrationAssembly));
            Check.NotNull(historyRepository, nameof(historyRepository));
            Check.NotNull(dataStoreCreator, nameof(dataStoreCreator));
            Check.NotNull(sqlGenerator, nameof(sqlGenerator));
            Check.NotNull(executor, nameof(executor));
            Check.NotNull(connection, nameof(connection));
            Check.NotNull(modelDiffer, nameof(modelDiffer));
            Check.NotNull(model, nameof(model));
            Check.NotNull(idGenerator, nameof(idGenerator));

            _migrationAssembly = migrationAssembly;
            _historyRepository = historyRepository;
            _dataStoreCreator = new LazyRef<RelationalDataStoreCreator>(
                () => (RelationalDataStoreCreator)dataStoreCreator.Service);
            _sqlGenerator = sqlGenerator;
            _executor = executor;
            _connection = new LazyRef<RelationalConnection>(() => (RelationalConnection)connection.Service);
            _modelDiffer = modelDiffer;
            _model = model;
            _idGenerator = idGenerator;
        }

        protected virtual string ProductVersion =>
            typeof(Migrator).GetTypeInfo().Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()
                ?.InformationalVersion;

        public virtual IReadOnlyList<Migration> GetUnappliedMigrations()
        {
            var appliedMigrations = _historyRepository.Service.GetAppliedMigrations();

            return _migrationAssembly.Migrations.Where(
                m => !appliedMigrations.Any(
                    e => string.Equals(e.MigrationId, m.Id, StringComparison.OrdinalIgnoreCase)))
                .ToList();
        }

        public virtual bool HasPendingModelChanges() =>
            _modelDiffer.Service.HasDifferences(_migrationAssembly.ModelSnapshot?.Model, _model.Service);

        public virtual void ApplyMigrations([CanBeNull] string targetMigration = null)
        {
            var migrations = _migrationAssembly.Migrations;
            var appliedMigrationEntries = _historyRepository.Service.GetAppliedMigrations();

            var appliedMigrations = new List<Migration>();
            var unappliedMigrations = new List<Migration>();
            foreach (var migraion in migrations)
            {
                if (appliedMigrationEntries.Any(
                    e => string.Equals(e.MigrationId, migraion.Id, StringComparison.OrdinalIgnoreCase)))
                {
                    appliedMigrations.Add(migraion);
                }
                else
                {
                    unappliedMigrations.Add(migraion);
                }
            }

            IEnumerable<Migration> migrationsToApply;
            IEnumerable<Migration> migrationsToRevert;
            if (string.IsNullOrEmpty(targetMigration))
            {
                migrationsToApply = unappliedMigrations;
                migrationsToRevert = Enumerable.Empty<Migration>();
            }
            else if (targetMigration == InitialDatabase)
            {
                migrationsToApply = Enumerable.Empty<Migration>();
                migrationsToRevert = appliedMigrations;
            }
            else
            {
                if (!_idGenerator.IsValidId(targetMigration))
                {
                    var candidate = migrations.Where(m => _idGenerator.GetName(m.Id) == targetMigration)
                        .Concat(migrations.Where(m => string.Equals(_idGenerator.GetName(m.Id), targetMigration, StringComparison.OrdinalIgnoreCase)))
                        .Select(m => m.Id)
                        .FirstOrDefault();
                    if (candidate == null)
                    {
                        throw new InvalidOperationException(Strings.TargetMigrationNotFound(targetMigration));
                    }

                    targetMigration = candidate;
                }

                migrationsToApply = unappliedMigrations
                    .Where(m => string.Compare(m.Id, targetMigration, StringComparison.OrdinalIgnoreCase) <= 0);
                migrationsToRevert = appliedMigrations
                    .Where(m => string.Compare(m.Id, targetMigration, StringComparison.OrdinalIgnoreCase) > 0)
                    .OrderByDescending(m => m.Id);
            }

            var checkFirst = true;
            foreach (var migration in migrationsToApply)
            {
                var first = false;
                if (checkFirst)
                {
                    first = migration == migrations[0];
                    checkFirst = false;
                }

                Execute(ApplyMigration(migration, first), first);
            }

            foreach (var migration in migrationsToRevert)
            {
                Execute(RevertMigration(migration));
            }
        }

        public virtual string ScriptMigrations(
            [CanBeNull] string fromMigrationName,
            [CanBeNull] string toMigrationName,
            bool idempotent = false) =>
                "-- TODO: Generate a script ;)";

        protected virtual IReadOnlyList<SqlBatch> ApplyMigration([NotNull] Migration migration, bool first)
        {
            Check.NotNull(migration, nameof(migration));

            var migrationBuilder = new MigrationBuilder();
            migration.Up(migrationBuilder);
            var operations = migrationBuilder.Operations.ToList();

            if (first && !_historyRepository.Service.Exists())
            {
                operations.Add(_historyRepository.Service.GetCreateOperation());
            }

            operations.Add(_historyRepository.Service.GetInsertOperation(new HistoryRow(migration.Id, ProductVersion)));

            return _sqlGenerator.Service.Generate(operations, migration.Target);
        }

        protected virtual IReadOnlyList<SqlBatch> RevertMigration([NotNull] Migration migration)
        {
            Check.NotNull(migration, nameof(migration));

            var migrationBuilder = new MigrationBuilder();
            migration.Down(migrationBuilder);
            var operations = migrationBuilder.Operations.ToList();

            operations.Add(_historyRepository.Service.GetDeleteOperation(migration.Id));

            // TODO: Pass source model?
            return _sqlGenerator.Service.Generate(operations);
        }

        protected virtual void Execute([NotNull] IEnumerable<SqlBatch> sqlBatches, bool first = false)
        {
            Check.NotNull(sqlBatches, nameof(sqlBatches));

            if (first && !_dataStoreCreator.Value.Exists())
            {
                _dataStoreCreator.Value.Create();
            }

            using (var transaction = _connection.Value.BeginTransaction())
            {
                _executor.ExecuteNonQuery(_connection.Value, transaction.DbTransaction, sqlBatches);
                transaction.Commit();
            }
        }
    }
}
