// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Internal;
using Microsoft.Data.Entity.Migrations.Operations;
using Microsoft.Data.Entity.Storage;
using Microsoft.Data.Entity.Storage.Commands;
using Microsoft.Data.Entity.Update;
using Microsoft.Data.Entity.Utilities;
using Microsoft.Framework.Logging;
using Strings = Microsoft.Data.Entity.Relational.Internal.Strings;

namespace Microsoft.Data.Entity.Migrations.Internal
{
    public class Migrator : IMigrator
    {
        private readonly IMigrationsAssembly _migrationAssembly;
        private readonly IHistoryRepository _historyRepository;
        private readonly IRelationalDatabaseCreator _databaseCreator;
        private readonly IMigrationsSqlGenerator _sqlGenerator;
        private readonly ISqlStatementExecutor _executor;
        private readonly IRelationalConnection _connection;
        private readonly IUpdateSqlGenerator _sql;
        private readonly LazyRef<ILogger> _logger;

        public Migrator(
            [NotNull] IMigrationsAssembly migrationAssembly,
            [NotNull] IHistoryRepository historyRepository,
            [NotNull] IDatabaseCreator databaseCreator,
            [NotNull] IMigrationsSqlGenerator sqlGenerator,
            [NotNull] ISqlStatementExecutor executor,
            [NotNull] IRelationalConnection connection,
            [NotNull] IUpdateSqlGenerator sql,
            [NotNull] ILoggerFactory loggerFactory)
        {
            Check.NotNull(migrationAssembly, nameof(migrationAssembly));
            Check.NotNull(historyRepository, nameof(historyRepository));
            Check.NotNull(databaseCreator, nameof(databaseCreator));
            Check.NotNull(sqlGenerator, nameof(sqlGenerator));
            Check.NotNull(executor, nameof(executor));
            Check.NotNull(connection, nameof(connection));
            Check.NotNull(sql, nameof(sql));
            Check.NotNull(loggerFactory, nameof(loggerFactory));

            _migrationAssembly = migrationAssembly;
            _historyRepository = historyRepository;
            _databaseCreator = (IRelationalDatabaseCreator)databaseCreator;
            _sqlGenerator = sqlGenerator;
            _executor = executor;
            _connection = connection;
            _sql = sql;
            _logger = new LazyRef<ILogger>(loggerFactory.CreateLogger<Migrator>);
        }

        public virtual void Migrate(string targetMigration = null)
        {
            var connection = _connection.DbConnection;
            _logger.Value.LogVerbose(Strings.UsingConnection(connection.Database, connection.DataSource));

            var migrations = _migrationAssembly.Migrations;
            var appliedMigrationEntries = _historyRepository.GetAppliedMigrations();

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

            IReadOnlyList<Migration> migrationsToApply;
            IReadOnlyList<Migration> migrationsToRevert;
            if (string.IsNullOrEmpty(targetMigration))
            {
                migrationsToApply = unappliedMigrations;
                migrationsToRevert = new Migration[0];
            }
            else if (targetMigration == Migration.InitialDatabase)
            {
                migrationsToApply = new Migration[0];
                migrationsToRevert = appliedMigrations.OrderByDescending(m => m.Id).ToList();
            }
            else
            {
                targetMigration = _migrationAssembly.GetMigration(targetMigration).Id;
                migrationsToApply = unappliedMigrations
                    .Where(m => string.Compare(m.Id, targetMigration, StringComparison.OrdinalIgnoreCase) <= 0)
                    .ToList();
                migrationsToRevert = appliedMigrations
                    .Where(m => string.Compare(m.Id, targetMigration, StringComparison.OrdinalIgnoreCase) > 0)
                    .OrderByDescending(m => m.Id)
                    .ToList();
            }

            bool first;
            var checkFirst = true;
            foreach (var migration in migrationsToApply)
            {
                var batches = new List<RelationalCommand>(GenerateUpSql(migration));

                first = false;
                if (checkFirst)
                {
                    first = migration == migrations[0];
                    if (first && !_historyRepository.Exists())
                    {
                        // TODO: Prepend to first batch instead
                        batches.Insert(0, new RelationalCommand(_historyRepository.GetCreateScript()));
                    }

                    checkFirst = false;
                }

                _logger.Value.LogInformation(Strings.ApplyingMigration(migration.Id));

                Execute(batches, first);
            }

            for (var i = 0; i < migrationsToRevert.Count; i++)
            {
                var migration = migrationsToRevert[i];

                _logger.Value.LogInformation(Strings.RevertingMigration(migration.Id));

                Execute(GenerateDownSql(
                    migration,
                    i != migrationsToRevert.Count - 1
                        ? migrationsToRevert[i + 1]
                        : null));
            }
        }

        public virtual string GenerateScript(
            string fromMigration,
            string toMigration,
            bool idempotent = false)
        {
            var migrations = _migrationAssembly.Migrations;

            if (string.IsNullOrEmpty(fromMigration))
            {
                fromMigration = Migration.InitialDatabase;
            }
            else if (fromMigration != Migration.InitialDatabase)
            {
                fromMigration = _migrationAssembly.GetMigration(fromMigration).Id;
            }

            if (string.IsNullOrEmpty(toMigration))
            {
                toMigration = migrations.Last().Id;
            }
            else if (toMigration != Migration.InitialDatabase)
            {
                toMigration = _migrationAssembly.GetMigration(toMigration).Id;
            }

            var builder = new IndentedStringBuilder();

            // If going up...
            if (string.Compare(fromMigration, toMigration, StringComparison.OrdinalIgnoreCase) <= 0)
            {
                var migrationsToApply = migrations.Where(
                    m => string.Compare(m.Id, fromMigration, StringComparison.OrdinalIgnoreCase) > 0
                         && string.Compare(m.Id, toMigration, StringComparison.OrdinalIgnoreCase) <= 0);
                var checkFirst = true;
                foreach (var migration in migrationsToApply)
                {
                    if (checkFirst)
                    {
                        if (migration == migrations[0])
                        {
                            builder.AppendLine(_historyRepository.GetCreateIfNotExistsScript());
                            builder.AppendLine(_sql.BatchSeparator);
                            builder.AppendLine();
                        }

                        checkFirst = false;
                    }

                    _logger.Value.LogVerbose(Strings.GeneratingUp(migration.Id));

                    foreach (var command in GenerateUpSql(migration))
                    {
                        if (idempotent)
                        {
                            builder.AppendLine(_historyRepository.GetBeginIfNotExistsScript(migration.Id));
                            using (builder.Indent())
                            {
                                builder.AppendLines(command.CommandText);
                            }
                            builder.AppendLine(_historyRepository.GetEndIfScript());
                        }
                        else
                        {
                            builder.Append(command.CommandText);
                        }

                        builder.AppendLine(_sql.BatchSeparator);
                        builder.AppendLine();
                    }
                }
            }
            else // If going down...
            {
                var migrationsToRevert = migrations
                    .Where(
                        m => string.Compare(m.Id, toMigration, StringComparison.OrdinalIgnoreCase) > 0
                             && string.Compare(m.Id, fromMigration, StringComparison.OrdinalIgnoreCase) <= 0)
                    .OrderByDescending(m => m.Id)
                    .ToList();
                for (var i = 0; i < migrationsToRevert.Count; i++)
                {
                    var migration = migrationsToRevert[i];
                    var previousMigration = i != migrationsToRevert.Count - 1
                        ? migrationsToRevert[i + 1]
                        : null;

                    _logger.Value.LogVerbose(Strings.GeneratingDown(migration.Id));

                    foreach (var command in GenerateDownSql(migration, previousMigration))
                    {
                        if (idempotent)
                        {
                            builder.AppendLine(_historyRepository.GetBeginIfExistsScript(migration.Id));
                            using (builder.Indent())
                            {
                                builder.AppendLines(command.CommandText);
                            }
                            builder.AppendLine(_historyRepository.GetEndIfScript());
                        }
                        else
                        {
                            builder.Append(command.CommandText);
                        }

                        builder.AppendLine(_sql.BatchSeparator);
                        builder.AppendLine();
                    }
                }
            }

            return builder.ToString();
        }

        protected virtual IReadOnlyList<RelationalCommand> GenerateUpSql([NotNull] Migration migration)
        {
            Check.NotNull(migration, nameof(migration));

            var operations = new List<MigrationOperation>(migration.UpOperations);
            // TODO: Append to batch instead
            operations.Add(
                new SqlOperation
                {
                    Sql = _historyRepository.GetInsertScript(new HistoryRow(migration.Id, ProductInfo.GetVersion()))
                });

            return _sqlGenerator.Generate(operations, migration.TargetModel);
        }

        protected virtual IReadOnlyList<RelationalCommand> GenerateDownSql(
            [NotNull] Migration migration,
            [CanBeNull] Migration previousMigration)
        {
            Check.NotNull(migration, nameof(migration));

            var operations = new List<MigrationOperation>(migration.DownOperations);

            // TODO: Append to batch instead
            operations.Add(new SqlOperation { Sql = _historyRepository.GetDeleteScript(migration.Id) });

            var targetModel = previousMigration != null
                ? previousMigration.TargetModel
                : null;

            return _sqlGenerator.Generate(operations, targetModel);
        }

        private void Execute([NotNull] IEnumerable<RelationalCommand> relationalCommands, bool ensureDatabase = false)
        {
            Check.NotNull(relationalCommands, nameof(relationalCommands));

            if (ensureDatabase && !_databaseCreator.Exists())
            {
                _databaseCreator.Create();
            }

            using (var transaction = _connection.BeginTransaction())
            {
                _executor.ExecuteNonQuery(_connection, relationalCommands);
                transaction.Commit();
            }
        }
    }
}
