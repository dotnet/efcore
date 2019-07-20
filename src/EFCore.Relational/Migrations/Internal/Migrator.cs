// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Utilities;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore.Migrations.Internal
{
    /// <summary>
    ///     <para>
    ///         This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///         the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///         any release. You should only use it directly in your code with extreme caution and knowing that
    ///         doing so can result in application failures when updating to a new Entity Framework Core release.
    ///     </para>
    ///     <para>
    ///         The service lifetime is <see cref="ServiceLifetime.Scoped"/>. This means that each
    ///         <see cref="DbContext"/> instance will use its own instance of this service.
    ///         The implementation may depend on other services registered with any lifetime.
    ///         The implementation does not need to be thread-safe.
    ///     </para>
    /// </summary>
    public class Migrator : IMigrator
    {
        private readonly IMigrationsAssembly _migrationsAssembly;
        private readonly IHistoryRepository _historyRepository;
        private readonly IRelationalDatabaseCreator _databaseCreator;
        private readonly IMigrationsSqlGenerator _migrationsSqlGenerator;
        private readonly IRawSqlCommandBuilder _rawSqlCommandBuilder;
        private readonly IMigrationCommandExecutor _migrationCommandExecutor;
        private readonly IRelationalConnection _connection;
        private readonly ISqlGenerationHelper _sqlGenerationHelper;
        private readonly ICurrentDbContext _currentContext;
        private readonly IDiagnosticsLogger<DbLoggerCategory.Migrations> _logger;
        private readonly IDiagnosticsLogger<DbLoggerCategory.Database.Command> _commandLogger;
        private readonly string _activeProvider;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public Migrator(
            [NotNull] IMigrationsAssembly migrationsAssembly,
            [NotNull] IHistoryRepository historyRepository,
            [NotNull] IDatabaseCreator databaseCreator,
            [NotNull] IMigrationsSqlGenerator migrationsSqlGenerator,
            [NotNull] IRawSqlCommandBuilder rawSqlCommandBuilder,
            [NotNull] IMigrationCommandExecutor migrationCommandExecutor,
            [NotNull] IRelationalConnection connection,
            [NotNull] ISqlGenerationHelper sqlGenerationHelper,
            [NotNull] ICurrentDbContext currentContext,
            [NotNull] IDiagnosticsLogger<DbLoggerCategory.Migrations> logger,
            [NotNull] IDiagnosticsLogger<DbLoggerCategory.Database.Command> commandLogger,
            [NotNull] IDatabaseProvider databaseProvider)
        {
            Check.NotNull(migrationsAssembly, nameof(migrationsAssembly));
            Check.NotNull(historyRepository, nameof(historyRepository));
            Check.NotNull(databaseCreator, nameof(databaseCreator));
            Check.NotNull(migrationsSqlGenerator, nameof(migrationsSqlGenerator));
            Check.NotNull(rawSqlCommandBuilder, nameof(rawSqlCommandBuilder));
            Check.NotNull(migrationCommandExecutor, nameof(migrationCommandExecutor));
            Check.NotNull(connection, nameof(connection));
            Check.NotNull(sqlGenerationHelper, nameof(sqlGenerationHelper));
            Check.NotNull(currentContext, nameof(currentContext));
            Check.NotNull(logger, nameof(logger));
            Check.NotNull(commandLogger, nameof(commandLogger));
            Check.NotNull(databaseProvider, nameof(databaseProvider));

            _migrationsAssembly = migrationsAssembly;
            _historyRepository = historyRepository;
            _databaseCreator = (IRelationalDatabaseCreator)databaseCreator;
            _migrationsSqlGenerator = migrationsSqlGenerator;
            _rawSqlCommandBuilder = rawSqlCommandBuilder;
            _migrationCommandExecutor = migrationCommandExecutor;
            _connection = connection;
            _sqlGenerationHelper = sqlGenerationHelper;
            _currentContext = currentContext;
            _logger = logger;
            _commandLogger = commandLogger;
            _activeProvider = databaseProvider.Name;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual void Migrate(string targetMigration = null)
        {
            _logger.MigrateUsingConnection(this, _connection);

            if (!_historyRepository.Exists())
            {
                if (!_databaseCreator.Exists())
                {
                    _databaseCreator.Create();
                }

                var command = _rawSqlCommandBuilder.Build(
                    _historyRepository.GetCreateScript());

                command.ExecuteNonQuery(
                    new RelationalCommandParameterObject(
                        _connection,
                        null,
                        _currentContext.Context,
                        _commandLogger));
            }

            var commandLists = GetMigrationCommandLists(_historyRepository.GetAppliedMigrations(), targetMigration);
            foreach (var commandList in commandLists)
            {
                _migrationCommandExecutor.ExecuteNonQuery(commandList(), _connection);
            }
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual async Task MigrateAsync(
            string targetMigration = null,
            CancellationToken cancellationToken = default)
        {
            _logger.MigrateUsingConnection(this, _connection);

            if (!await _historyRepository.ExistsAsync(cancellationToken))
            {
                if (!await _databaseCreator.ExistsAsync(cancellationToken))
                {
                    await _databaseCreator.CreateAsync(cancellationToken);
                }

                var command = _rawSqlCommandBuilder.Build(
                    _historyRepository.GetCreateScript());

                await command.ExecuteNonQueryAsync(
                    new RelationalCommandParameterObject(
                        _connection,
                        null,
                        _currentContext.Context,
                        _commandLogger),
                    cancellationToken);
            }

            var commandLists = GetMigrationCommandLists(
                await _historyRepository.GetAppliedMigrationsAsync(cancellationToken),
                targetMigration);

            foreach (var commandList in commandLists)
            {
                await _migrationCommandExecutor.ExecuteNonQueryAsync(commandList(), _connection, cancellationToken);
            }
        }

        private IEnumerable<Func<IReadOnlyList<MigrationCommand>>> GetMigrationCommandLists(
            IReadOnlyList<HistoryRow> appliedMigrationEntries,
            string targetMigration = null)
        {
            PopulateMigrations(
                appliedMigrationEntries.Select(t => t.MigrationId),
                targetMigration,
                out var migrationsToApply,
                out var migrationsToRevert,
                out var actualTargetMigration);

            for (var i = 0; i < migrationsToRevert.Count; i++)
            {
                var migration = migrationsToRevert[i];

                var index = i;
                yield return () =>
                {
                    _logger.MigrationReverting(this, migration);

                    return GenerateDownSql(
                        migration,
                        index != migrationsToRevert.Count - 1
                            ? migrationsToRevert[index + 1]
                            : actualTargetMigration);
                };
            }

            foreach (var migration in migrationsToApply)
            {
                yield return () =>
                {
                    _logger.MigrationApplying(this, migration);

                    return GenerateUpSql(migration);
                };
            }

            if (migrationsToRevert.Count + migrationsToApply.Count == 0)
            {
                _logger.MigrationsNotApplied(this);
            }
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected virtual void PopulateMigrations(
            IEnumerable<string> appliedMigrationEntries,
            string targetMigration,
            out IReadOnlyList<Migration> migrationsToApply,
            out IReadOnlyList<Migration> migrationsToRevert,
            out Migration actualTargetMigration)
        {
            var appliedMigrations = new Dictionary<string, TypeInfo>();
            var unappliedMigrations = new Dictionary<string, TypeInfo>();
            var appliedMigrationEntrySet = new HashSet<string>(appliedMigrationEntries, StringComparer.OrdinalIgnoreCase);
            if (_migrationsAssembly.Migrations.Count == 0)
            {
                _logger.MigrationsNotFound(this, _migrationsAssembly);
            }

            foreach (var migration in _migrationsAssembly.Migrations)
            {
                if (appliedMigrationEntrySet.Contains(migration.Key))
                {
                    appliedMigrations.Add(migration.Key, migration.Value);
                }
                else
                {
                    unappliedMigrations.Add(migration.Key, migration.Value);
                }
            }

            if (string.IsNullOrEmpty(targetMigration))
            {
                migrationsToApply = unappliedMigrations
                    .OrderBy(m => m.Key)
                    .Select(p => _migrationsAssembly.CreateMigration(p.Value, _activeProvider))
                    .ToList();
                migrationsToRevert = Array.Empty<Migration>();
                actualTargetMigration = null;
            }
            else if (targetMigration == Migration.InitialDatabase)
            {
                migrationsToApply = Array.Empty<Migration>();
                migrationsToRevert = appliedMigrations
                    .OrderByDescending(m => m.Key)
                    .Select(p => _migrationsAssembly.CreateMigration(p.Value, _activeProvider))
                    .ToList();
                actualTargetMigration = null;
            }
            else
            {
                targetMigration = _migrationsAssembly.GetMigrationId(targetMigration);
                migrationsToApply = unappliedMigrations
                    .Where(m => string.Compare(m.Key, targetMigration, StringComparison.OrdinalIgnoreCase) <= 0)
                    .OrderBy(m => m.Key)
                    .Select(p => _migrationsAssembly.CreateMigration(p.Value, _activeProvider))
                    .ToList();
                migrationsToRevert = appliedMigrations
                    .Where(m => string.Compare(m.Key, targetMigration, StringComparison.OrdinalIgnoreCase) > 0)
                    .OrderByDescending(m => m.Key)
                    .Select(p => _migrationsAssembly.CreateMigration(p.Value, _activeProvider))
                    .ToList();
                actualTargetMigration = appliedMigrations
                    .Where(m => string.Compare(m.Key, targetMigration, StringComparison.OrdinalIgnoreCase) == 0)
                    .Select(p => _migrationsAssembly.CreateMigration(p.Value, _activeProvider))
                    .SingleOrDefault();
            }
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual string GenerateScript(
            string fromMigration = null,
            string toMigration = null,
            bool idempotent = false)
        {
            IEnumerable<string> appliedMigrations;
            if (string.IsNullOrEmpty(fromMigration)
                || fromMigration == Migration.InitialDatabase)
            {
                appliedMigrations = Enumerable.Empty<string>();
            }
            else
            {
                var fromMigrationId = _migrationsAssembly.GetMigrationId(fromMigration);
                appliedMigrations = _migrationsAssembly.Migrations
                    .Where(t => string.Compare(t.Key, fromMigrationId, StringComparison.OrdinalIgnoreCase) <= 0)
                    .Select(t => t.Key);
            }

            PopulateMigrations(
                appliedMigrations,
                toMigration,
                out var migrationsToApply,
                out var migrationsToRevert,
                out var actualTargetMigration);

            var builder = new IndentedStringBuilder();

            if (fromMigration == Migration.InitialDatabase
                || string.IsNullOrEmpty(fromMigration))
            {
                builder.AppendLine(_historyRepository.GetCreateIfNotExistsScript());
                builder.Append(_sqlGenerationHelper.BatchTerminator);
            }

            for (var i = 0; i < migrationsToRevert.Count; i++)
            {
                var migration = migrationsToRevert[i];
                var previousMigration = i != migrationsToRevert.Count - 1
                    ? migrationsToRevert[i + 1]
                    : actualTargetMigration;

                _logger.MigrationGeneratingDownScript(this, migration, fromMigration, toMigration, idempotent);

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

                    builder.Append(_sqlGenerationHelper.BatchTerminator);
                }
            }

            foreach (var migration in migrationsToApply)
            {
                _logger.MigrationGeneratingUpScript(this, migration, fromMigration, toMigration, idempotent);

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

                    builder.Append(_sqlGenerationHelper.BatchTerminator);
                }
            }

            return builder.ToString();
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected virtual IReadOnlyList<MigrationCommand> GenerateUpSql([NotNull] Migration migration)
        {
            Check.NotNull(migration, nameof(migration));

            var insertCommand = _rawSqlCommandBuilder.Build(
                _historyRepository.GetInsertScript(new HistoryRow(migration.GetId(), ProductInfo.GetVersion())));

            return _migrationsSqlGenerator
                .Generate(migration.UpOperations, migration.TargetModel)
                .Concat(new[] { new MigrationCommand(insertCommand, _currentContext.Context, _commandLogger) })
                .ToList();
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected virtual IReadOnlyList<MigrationCommand> GenerateDownSql(
            [NotNull] Migration migration,
            [CanBeNull] Migration previousMigration)
        {
            Check.NotNull(migration, nameof(migration));

            var deleteCommand = _rawSqlCommandBuilder.Build(
                _historyRepository.GetDeleteScript(migration.GetId()));

            return _migrationsSqlGenerator
                .Generate(migration.DownOperations, previousMigration?.TargetModel)
                .Concat(new[] { new MigrationCommand(deleteCommand, _currentContext.Context, _commandLogger) })
                .ToList();
        }
    }
}
