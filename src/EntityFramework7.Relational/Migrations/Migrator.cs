// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Internal;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Migrations.Builders;
using Microsoft.Data.Entity.Migrations.History;
using Microsoft.Data.Entity.Migrations.Infrastructure;
using Microsoft.Data.Entity.Migrations.Sql;
using Microsoft.Data.Entity.Storage;
using Microsoft.Data.Entity.Utilities;
using Microsoft.Framework.Logging;
using Strings = Microsoft.Data.Entity.Relational.Internal.Strings;

namespace Microsoft.Data.Entity.Migrations
{
    public class Migrator : IMigrator
    {
        private const string InitialDatabase = "0";

        private readonly IMigrationAssembly _migrationAssembly;
        private readonly IHistoryRepository _historyRepository;
        private readonly IRelationalDatabaseCreator _databaseCreator;
        private readonly IMigrationSqlGenerator _migrationSqlGenerator;
        private readonly ISqlStatementExecutor _executor;
        private readonly IRelationalConnection _connection;
        private readonly IModelDiffer _modelDiffer;
        private readonly IModel _model;
        private readonly IMigrationIdGenerator _idGenerator;
        private readonly IUpdateSqlGenerator _sqlGenerator;
        private readonly LazyRef<ILogger> _logger;
        private readonly IMigrationModelFactory _modelFactory;

        public Migrator(
            [NotNull] IMigrationAssembly migrationAssembly,
            [NotNull] IHistoryRepository historyRepository,
            [NotNull] IDatabaseCreator databaseCreator,
            [NotNull] IMigrationSqlGenerator migrationSqlGenerator,
            [NotNull] ISqlStatementExecutor executor,
            [NotNull] IRelationalConnection connection,
            [NotNull] IModelDiffer modelDiffer,
            [NotNull] IModel model,
            [NotNull] IMigrationIdGenerator idGenerator,
            [NotNull] IUpdateSqlGenerator sqlGenerator,
            [NotNull] ILoggerFactory loggerFactory,
            [NotNull] IMigrationModelFactory modelFactory)
        {
            Check.NotNull(migrationAssembly, nameof(migrationAssembly));
            Check.NotNull(historyRepository, nameof(historyRepository));
            Check.NotNull(databaseCreator, nameof(databaseCreator));
            Check.NotNull(migrationSqlGenerator, nameof(migrationSqlGenerator));
            Check.NotNull(executor, nameof(executor));
            Check.NotNull(connection, nameof(connection));
            Check.NotNull(modelDiffer, nameof(modelDiffer));
            Check.NotNull(model, nameof(model));
            Check.NotNull(idGenerator, nameof(idGenerator));
            Check.NotNull(sqlGenerator, nameof(sqlGenerator));
            Check.NotNull(loggerFactory, nameof(loggerFactory));
            Check.NotNull(modelFactory, nameof(modelFactory));

            _migrationAssembly = migrationAssembly;
            _historyRepository = historyRepository;
            _databaseCreator = (IRelationalDatabaseCreator)databaseCreator;
            _migrationSqlGenerator = migrationSqlGenerator;
            _executor = executor;
            _connection = connection;
            _modelDiffer = modelDiffer;
            _model = model;
            _idGenerator = idGenerator;
            _sqlGenerator = sqlGenerator;
            _logger = new LazyRef<ILogger>(loggerFactory.CreateLogger<Migrator>);
            _modelFactory = modelFactory;
        }

        protected virtual string ProductVersion =>
            typeof(Migrator).GetTypeInfo().Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()
                ?.InformationalVersion;

        public virtual IReadOnlyList<Migration> GetUnappliedMigrations()
        {
            var appliedMigrations = _historyRepository.GetAppliedMigrations();

            return _migrationAssembly.Migrations.Where(
                m => !appliedMigrations.Any(
                    e => string.Equals(e.MigrationId, m.Id, StringComparison.OrdinalIgnoreCase)))
                .ToList();
        }

        public virtual bool HasPendingModelChanges() => _modelDiffer.HasDifferences(_migrationAssembly.LastModel, _model);

        public async Task ApplyMigrationsAsync(string targetMigration = null)
        {
            var connection = _connection.DbConnection;
            _logger.Value.LogVerbose(Strings.UsingConnection(connection.Database, connection.DataSource));

            var appliedMigrationEntries = await _historyRepository.GetAppliedMigrationsAsync();
            var migrations = _migrationAssembly.Migrations;
            var migrationToExecute = GetMigrationsToExecute(targetMigration, migrations, appliedMigrationEntries);

            for (int index = 0; index < migrationToExecute.Count; index++)
            {
                var migration = migrationToExecute[index];
                var batches = GetSqlBatches(migration, index, migrations);

                await ExecuteAsync(batches, migration.Migration == migrations[0]);
            }
        }

        public virtual void ApplyMigrations(string targetMigration = null)
        {
            var connection = _connection.DbConnection;
            _logger.Value.LogVerbose(Strings.UsingConnection(connection.Database, connection.DataSource));

            var appliedMigrationEntries = _historyRepository.GetAppliedMigrations();
            var migrations = _migrationAssembly.Migrations;
            var migrationToExecute = GetMigrationsToExecute(targetMigration, migrations, appliedMigrationEntries);

            for (int index = 0; index < migrationToExecute.Count; index++)
            {
                var migration = migrationToExecute[index];
                var batches = GetSqlBatches(migration, index, migrations);

                Execute(batches, migration.Migration == migrations[0]);
            }
        }

        public virtual string ScriptMigrations(
            string fromMigrationName,
            string toMigrationName,
            bool idempotent = false)
        {
            var migrations = _migrationAssembly.Migrations;

            if (string.IsNullOrEmpty(fromMigrationName))
            {
                fromMigrationName = InitialDatabase;
            }
            else if (fromMigrationName != InitialDatabase)
            {
                fromMigrationName = _idGenerator.ResolveId(fromMigrationName, migrations);
            }

            if (string.IsNullOrEmpty(toMigrationName))
            {
                toMigrationName = migrations.Last().Id;
            }
            else if (toMigrationName != InitialDatabase)
            {
                toMigrationName = _idGenerator.ResolveId(toMigrationName, migrations);
            }

            var builder = new IndentedStringBuilder();

            // If going up...
            if (string.Compare(fromMigrationName, toMigrationName, StringComparison.OrdinalIgnoreCase) <= 0)
            {
                var migrationsToApply = migrations.Where(
                    m => string.Compare(m.Id, fromMigrationName, StringComparison.OrdinalIgnoreCase) > 0
                         && string.Compare(m.Id, toMigrationName, StringComparison.OrdinalIgnoreCase) <= 0);
                var checkFirst = true;
                foreach (var migration in migrationsToApply)
                {
                    if (checkFirst)
                    {
                        if (migration == migrations[0])
                        {
                            builder.AppendLine(_historyRepository.Create(ifNotExists: true));
                            builder.AppendLine(_sqlGenerator.BatchSeparator);
                            builder.AppendLine();
                        }

                        checkFirst = false;
                    }

                    _logger.Value.LogVerbose(Strings.GeneratingUp(migration.Id));

                    foreach (var batch in ApplyMigration(migration))
                    {
                        if (idempotent)
                        {
                            builder.AppendLine(_historyRepository.BeginIfNotExists(migration.Id));
                            using (builder.Indent())
                            {
                                var lines = batch.Sql.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
                                foreach (var line in lines)
                                {
                                    builder.AppendLine(line);
                                }
                            }
                            builder.AppendLine(_historyRepository.EndIf());
                        }
                        else
                        {
                            builder.Append(batch.Sql);
                        }

                        builder.AppendLine(_sqlGenerator.BatchSeparator);
                        builder.AppendLine();
                    }
                }
            }
            else // If going down...
            {
                var migrationsToRevert = migrations
                    .Where(
                        m => string.Compare(m.Id, toMigrationName, StringComparison.OrdinalIgnoreCase) > 0
                             && string.Compare(m.Id, fromMigrationName, StringComparison.OrdinalIgnoreCase) <= 0)
                    .OrderByDescending(m => m.Id);
                foreach (var migration in migrationsToRevert)
                {
                    _logger.Value.LogVerbose(Strings.GeneratingDown(migration.Id));

                    foreach (var batch in RevertMigration(migration))
                    {
                        if (idempotent)
                        {
                            builder.AppendLine(_historyRepository.BeginIfExists(migration.Id));
                            using (builder.Indent())
                            {
                                var lines = batch.Sql.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
                                foreach (var line in lines)
                                {
                                    builder.AppendLine(line);
                                }
                            }
                            builder.AppendLine(_historyRepository.EndIf());
                        }
                        else
                        {
                            builder.Append(batch.Sql);
                        }

                        builder.AppendLine(_sqlGenerator.BatchSeparator);
                        builder.AppendLine();
                    }
                }
            }

            return builder.ToString();
        }

        protected virtual IReadOnlyList<SqlBatch> ApplyMigration([NotNull] Migration migration)
        {
            Check.NotNull(migration, nameof(migration));

            var migrationBuilder = new MigrationBuilder();
            migration.Up(migrationBuilder);

            var operations = migrationBuilder.Operations.ToList();
            operations.Add(_historyRepository.GetInsertOperation(new HistoryRow(migration.Id, ProductVersion)));

            var targetModel = _modelFactory.Create(migration.BuildTargetModel);

            return _migrationSqlGenerator.Generate(operations, targetModel);
        }

        protected virtual IReadOnlyList<SqlBatch> RevertMigration([NotNull] Migration migration)
        {
            Check.NotNull(migration, nameof(migration));

            var migrationBuilder = new MigrationBuilder();
            migration.Down(migrationBuilder);
            var operations = migrationBuilder.Operations.ToList();

            operations.Add(_historyRepository.GetDeleteOperation(migration.Id));

            // TODO: Pass source model?
            return _migrationSqlGenerator.Generate(operations);
        }

        protected virtual async Task ExecuteAsync([NotNull] IEnumerable<SqlBatch> sqlBatches, bool ensureDatabase = false)
        {
            if (ensureDatabase && !await _databaseCreator.ExistsAsync())
            {
                await _databaseCreator.CreateAsync();
            }

            using (var transaction = await _connection.BeginTransactionAsync())
            {
                await _executor.ExecuteNonQueryAsync(_connection, transaction.DbTransaction, sqlBatches);
                transaction.Commit();
            }
        }

        protected virtual void Execute([NotNull] IEnumerable<SqlBatch> sqlBatches, bool ensureDatabase = false)
        {
            Check.NotNull(sqlBatches, nameof(sqlBatches));

            if (ensureDatabase && !_databaseCreator.Exists())
            {
                _databaseCreator.Create();
            }

            using (var transaction = _connection.BeginTransaction())
            {
                _executor.ExecuteNonQuery(_connection, transaction.DbTransaction, sqlBatches);
                transaction.Commit();
            }
        }

        protected virtual List<ExecutableMigration> GetMigrationsToExecute(string targetMigration, IReadOnlyList<Migration> migrations, IReadOnlyList<IHistoryRow> appliedMigrationEntries)
        {
            var migrationToExecute = new List<ExecutableMigration>();

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

            if (string.IsNullOrEmpty(targetMigration))
            {
                migrationToExecute = unappliedMigrations
                    .Select(t => new ExecutableMigration
                    {
                        Migration = t,
                        ShouldApply = true
                    }).ToList();
            }
            else if (targetMigration == InitialDatabase)
            {
                migrationToExecute = appliedMigrations
                    .Select(t => new ExecutableMigration
                    {
                        Migration = t,
                        ShouldApply = false
                    }).ToList();
            }
            else
            {
                targetMigration = _idGenerator.ResolveId(targetMigration, migrations);
                migrationToExecute
                    .AddRange(
                        unappliedMigrations
                            .Where(m => string.Compare(m.Id, targetMigration, StringComparison.OrdinalIgnoreCase) <= 0)
                            .Select(t => new ExecutableMigration
                            {
                                Migration = t,
                                ShouldApply = true
                            }));

                migrationToExecute
                    .AddRange(
                        appliedMigrations
                            .Where(m => string.Compare(m.Id, targetMigration, StringComparison.OrdinalIgnoreCase) > 0)
                            .OrderByDescending(m => m.Id)
                            .Select(t => new ExecutableMigration
                            {
                                Migration = t,
                                ShouldApply = false
                            }));
            }

            return migrationToExecute;
        }

        protected virtual IList<SqlBatch> GetSqlBatches(ExecutableMigration migration, int index, IReadOnlyList<Migration> migrations)
        {
            IList<SqlBatch> batches;
            if (migration.ShouldApply)
            {
                batches = ApplyMigration(migration.Migration).ToList();

                if (index == 0)
                {
                    if (migration.Migration == migrations[0])
                    {
                        batches.Insert(0, new SqlBatch(_historyRepository.Create(ifNotExists: true)));
                    }
                }

                _logger.Value.LogInformation(Strings.ApplyingMigration(migration.Migration.Id));
            }
            else
            {
                batches = RevertMigration(migration.Migration).ToList();
                _logger.Value.LogInformation(Strings.RevertingMigration(migration.Migration.Id));
            }

            return batches;
        }

        protected class ExecutableMigration
        {
            public Migration Migration { get; set; }
            public bool ShouldApply { get; set; }
        }
    }
}
