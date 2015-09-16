// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Internal;
using Microsoft.Data.Entity.Storage;
using Microsoft.Data.Entity.Update;
using Microsoft.Data.Entity.Utilities;
using Microsoft.Framework.Logging;
using Strings = Microsoft.Data.Entity.Relational.Internal.Strings;

namespace Microsoft.Data.Entity.Migrations.Internal
{
    public class Migrator : IMigrator
    {
        private readonly IMigrationsAssembly _migrationsAssembly;
        private readonly IHistoryRepository _historyRepository;
        private readonly IRelationalDatabaseCreator _databaseCreator;
        private readonly IMigrationsSqlGenerator _sqlGenerator;
        private readonly ISqlStatementExecutor _executor;
        private readonly IRelationalConnection _connection;
        private readonly IUpdateSqlGenerator _sql;
        private readonly LazyRef<ILogger> _logger;
        private readonly string _activeProvider;

        public Migrator(
            [NotNull] IMigrationsAssembly migrationsAssembly,
            [NotNull] IHistoryRepository historyRepository,
            [NotNull] IDatabaseCreator databaseCreator,
            [NotNull] IMigrationsSqlGenerator sqlGenerator,
            [NotNull] ISqlStatementExecutor executor,
            [NotNull] IRelationalConnection connection,
            [NotNull] IUpdateSqlGenerator sql,
            [NotNull] ILoggerFactory loggerFactory,
            [NotNull] IDatabaseProviderServices providerServices)
        {
            Check.NotNull(migrationsAssembly, nameof(migrationsAssembly));
            Check.NotNull(historyRepository, nameof(historyRepository));
            Check.NotNull(databaseCreator, nameof(databaseCreator));
            Check.NotNull(sqlGenerator, nameof(sqlGenerator));
            Check.NotNull(executor, nameof(executor));
            Check.NotNull(connection, nameof(connection));
            Check.NotNull(sql, nameof(sql));
            Check.NotNull(loggerFactory, nameof(loggerFactory));
            Check.NotNull(providerServices, nameof(providerServices));

            _migrationsAssembly = migrationsAssembly;
            _historyRepository = historyRepository;
            _databaseCreator = (IRelationalDatabaseCreator)databaseCreator;
            _sqlGenerator = sqlGenerator;
            _executor = executor;
            _connection = connection;
            _sql = sql;
            _logger = new LazyRef<ILogger>(loggerFactory.CreateLogger<Migrator>);
            _activeProvider = providerServices.InvariantName;
        }

        public virtual void Migrate(string targetMigration = null)
        {
            var connection = _connection.DbConnection;
            _logger.Value.LogVerbose(Strings.UsingConnection(connection.Database, connection.DataSource));

            if (!_historyRepository.Exists())
            {
                if (!_databaseCreator.Exists())
                {
                    _databaseCreator.Create();
                }

                Execute(new[] { new RelationalCommand(_historyRepository.GetCreateScript()) });
            }

            var commands = GetMigrationCommands(_historyRepository.GetAppliedMigrations(), targetMigration);
            foreach (var command in commands)
            {
                Execute(command());
            }
        }

        public virtual async Task MigrateAsync(
            string targetMigration = null,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            var connection = _connection.DbConnection;
            _logger.Value.LogVerbose(Strings.UsingConnection(connection.Database, connection.DataSource));

            if (!await _historyRepository.ExistsAsync(cancellationToken))
            {
                if (!await _databaseCreator.ExistsAsync(cancellationToken))
                {
                    await _databaseCreator.CreateAsync(cancellationToken);
                }

                await ExecuteAsync(
                    new[] { new RelationalCommand(_historyRepository.GetCreateScript()) },
                    cancellationToken);
            }

            var commands = GetMigrationCommands(
                await _historyRepository.GetAppliedMigrationsAsync(cancellationToken),
                targetMigration);
            foreach (var command in commands)
            {
                await ExecuteAsync(command(), cancellationToken);
            }
        }

        private IEnumerable<Func<IReadOnlyList<RelationalCommand>>> GetMigrationCommands(
            IReadOnlyList<HistoryRow> appliedMigrationEntries,
            string targetMigration = null)
        {
            var appliedMigrations = new Dictionary<string, TypeInfo>();
            var unappliedMigrations = new Dictionary<string, TypeInfo>();
            foreach (var migration in _migrationsAssembly.Migrations)
            {
                if (appliedMigrationEntries.Any(
                    e => string.Equals(e.MigrationId, migration.Key, StringComparison.OrdinalIgnoreCase)))
                {
                    appliedMigrations.Add(migration.Key, migration.Value);
                }
                else
                {
                    unappliedMigrations.Add(migration.Key, migration.Value);
                }
            }

            IReadOnlyList<Migration> migrationsToApply;
            IReadOnlyList<Migration> migrationsToRevert;
            if (string.IsNullOrEmpty(targetMigration))
            {
                migrationsToApply = unappliedMigrations
                    .Select(p => _migrationsAssembly.CreateMigration(p.Value, _activeProvider))
                    .ToList();
                migrationsToRevert = new Migration[0];
            }
            else if (targetMigration == Migration.InitialDatabase)
            {
                migrationsToApply = new Migration[0];
                migrationsToRevert = appliedMigrations
                    .OrderByDescending(m => m.Key)
                    .Select(p => _migrationsAssembly.CreateMigration(p.Value, _activeProvider))
                    .ToList();
            }
            else
            {
                targetMigration = _migrationsAssembly.GetMigrationId(targetMigration);
                migrationsToApply = unappliedMigrations
                    .Where(m => string.Compare(m.Key, targetMigration, StringComparison.OrdinalIgnoreCase) <= 0)
                    .Select(p => _migrationsAssembly.CreateMigration(p.Value, _activeProvider))
                    .ToList();
                migrationsToRevert = appliedMigrations
                    .Where(m => string.Compare(m.Key, targetMigration, StringComparison.OrdinalIgnoreCase) > 0)
                    .OrderByDescending(m => m.Key)
                    .Select(p => _migrationsAssembly.CreateMigration(p.Value, _activeProvider))
                    .ToList();
            }

            for (var i = 0; i < migrationsToRevert.Count; i++)
            {
                var migration = migrationsToRevert[i];

                yield return () =>
                {
                    _logger.Value.LogInformation(Strings.RevertingMigration(migration.GetId()));

                    return GenerateDownSql(
                        migration,
                        i != migrationsToRevert.Count - 1
                            ? migrationsToRevert[i + 1]
                            : null);
                };
            }

            foreach (var migration in migrationsToApply)
            {
                yield return () =>
                {
                    _logger.Value.LogInformation(Strings.ApplyingMigration(migration.GetId()));

                    return GenerateUpSql(migration);
                };
            }
        }

        public virtual string GenerateScript(
            string fromMigration = null,
            string toMigration = null,
            bool idempotent = false)
        {
            var migrations = _migrationsAssembly.Migrations;

            if (string.IsNullOrEmpty(fromMigration))
            {
                fromMigration = Migration.InitialDatabase;
            }
            else if (fromMigration != Migration.InitialDatabase)
            {
                fromMigration = _migrationsAssembly.GetMigrationId(fromMigration);
            }

            if (string.IsNullOrEmpty(toMigration))
            {
                toMigration = migrations.Keys.Last();
            }
            else if (toMigration != Migration.InitialDatabase)
            {
                toMigration = _migrationsAssembly.GetMigrationId(toMigration);
            }

            var builder = new IndentedStringBuilder();

            // If going up...
            if (string.Compare(fromMigration, toMigration, StringComparison.OrdinalIgnoreCase) <= 0)
            {
                var migrationsToApply = migrations.Where(
                    m => string.Compare(m.Key, fromMigration, StringComparison.OrdinalIgnoreCase) > 0
                        && string.Compare(m.Key, toMigration, StringComparison.OrdinalIgnoreCase) <= 0)
                    .Select(m => _migrationsAssembly.CreateMigration(m.Value, _activeProvider));
                var checkFirst = true;
                foreach (var migration in migrationsToApply)
                {
                    if (checkFirst)
                    {
                        if (migration.GetId() == migrations.Keys.First())
                        {
                            builder.AppendLine(_historyRepository.GetCreateIfNotExistsScript());
                            builder.Append(_sql.BatchSeparator);
                        }

                        checkFirst = false;
                    }

                    _logger.Value.LogVerbose(Strings.GeneratingUp(migration.GetId()));

                    foreach (var command in GenerateUpSql(migration))
                    {
                        if (idempotent)
                        {
                            builder.AppendLine(_historyRepository.GetBeginIfNotExistsScript(migration.GetId()));
                            using (builder.Indent())
                            {
                                builder.AppendLines(command.CommandText);
                            }
                            builder.AppendLine(_historyRepository.GetEndIfScript());
                        }
                        else
                        {
                            builder.AppendLine(command.CommandText);
                        }

                        builder.Append(_sql.BatchSeparator);
                    }
                }
            }
            else // If going down...
            {
                var migrationsToRevert = migrations
                        .Where(
                            m => string.Compare(m.Key, toMigration, StringComparison.OrdinalIgnoreCase) > 0
                                && string.Compare(m.Key, fromMigration, StringComparison.OrdinalIgnoreCase) <= 0)
                        .OrderByDescending(m => m.Key)
                        .Select(m => _migrationsAssembly.CreateMigration(m.Value, _activeProvider))
                        .ToList();
                for (var i = 0; i < migrationsToRevert.Count; i++)
                {
                    var migration = migrationsToRevert[i];
                    var previousMigration = i != migrationsToRevert.Count - 1
                        ? migrationsToRevert[i + 1]
                        : null;

                    _logger.Value.LogVerbose(Strings.GeneratingDown(migration.GetId()));

                    foreach (var command in GenerateDownSql(migration, previousMigration))
                    {
                        if (idempotent)
                        {
                            builder.AppendLine(_historyRepository.GetBeginIfExistsScript(migration.GetId()));
                            using (builder.Indent())
                            {
                                builder.AppendLines(command.CommandText);
                            }
                            builder.AppendLine(_historyRepository.GetEndIfScript());
                        }
                        else
                        {
                            builder.AppendLine(command.CommandText);
                        }

                        builder.Append(_sql.BatchSeparator);
                    }
                }
            }

            return builder.ToString();
        }

        protected virtual IReadOnlyList<RelationalCommand> GenerateUpSql([NotNull] Migration migration)
        {
            Check.NotNull(migration, nameof(migration));

            var commands = new List<RelationalCommand>();
            commands.AddRange(_sqlGenerator.Generate(migration.UpOperations, migration.TargetModel));
            commands.Add(
                new RelationalCommand(
                    _historyRepository.GetInsertScript(new HistoryRow(migration.GetId(), ProductInfo.GetVersion()))));

            return commands;
        }

        protected virtual IReadOnlyList<RelationalCommand> GenerateDownSql(
            [NotNull] Migration migration,
            [CanBeNull] Migration previousMigration)
        {
            Check.NotNull(migration, nameof(migration));

            var commands = new List<RelationalCommand>();
            commands.AddRange(_sqlGenerator.Generate(migration.DownOperations, previousMigration?.TargetModel));
            commands.Add(new RelationalCommand(_historyRepository.GetDeleteScript(migration.GetId())));

            return commands;
        }

        private void Execute(IEnumerable<RelationalCommand> relationalCommands)
        {
            using (var transaction = _connection.BeginTransaction())
            {
                _executor.ExecuteNonQuery(_connection, relationalCommands);
                transaction.Commit();
            }
        }

        private async Task ExecuteAsync(
            IEnumerable<RelationalCommand> relationalCommands,
            CancellationToken cancellationToken = default(CancellationToken))
        {

            using (var transaction = await _connection.BeginTransactionAsync(cancellationToken))
            {
                await _executor.ExecuteNonQueryAsync(_connection, relationalCommands, cancellationToken);
                transaction.Commit();
            }
        }
    }
}
