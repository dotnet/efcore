// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Threading;
using Microsoft.EntityFrameworkCore.Diagnostics.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace Microsoft.EntityFrameworkCore.Migrations.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
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
    private readonly IModelRuntimeInitializer _modelRuntimeInitializer;
    private readonly IDiagnosticsLogger<DbLoggerCategory.Migrations> _logger;
    private readonly IRelationalCommandDiagnosticsLogger _commandLogger;
    private readonly IMigrationsModelDiffer _migrationsModelDiffer;
    private readonly IDesignTimeModel _designTimeModel;
    private readonly string _activeProvider;
    private readonly IDbContextOptions _contextOptions;
    private readonly IExecutionStrategy _executionStrategy;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public Migrator(
        IMigrationsAssembly migrationsAssembly,
        IHistoryRepository historyRepository,
        IDatabaseCreator databaseCreator,
        IMigrationsSqlGenerator migrationsSqlGenerator,
        IRawSqlCommandBuilder rawSqlCommandBuilder,
        IMigrationCommandExecutor migrationCommandExecutor,
        IRelationalConnection connection,
        ISqlGenerationHelper sqlGenerationHelper,
        ICurrentDbContext currentContext,
        IModelRuntimeInitializer modelRuntimeInitializer,
        IDiagnosticsLogger<DbLoggerCategory.Migrations> logger,
        IRelationalCommandDiagnosticsLogger commandLogger,
        IDatabaseProvider databaseProvider,
        IMigrationsModelDiffer migrationsModelDiffer,
        IDesignTimeModel designTimeModel,
        IDbContextOptions contextOptions,
        IExecutionStrategy executionStrategy)
    {
        _migrationsAssembly = migrationsAssembly;
        _historyRepository = historyRepository;
        _databaseCreator = (IRelationalDatabaseCreator)databaseCreator;
        _migrationsSqlGenerator = migrationsSqlGenerator;
        _rawSqlCommandBuilder = rawSqlCommandBuilder;
        _migrationCommandExecutor = migrationCommandExecutor;
        _connection = connection;
        _sqlGenerationHelper = sqlGenerationHelper;
        _currentContext = currentContext;
        _modelRuntimeInitializer = modelRuntimeInitializer;
        _logger = logger;
        _commandLogger = commandLogger;
        _migrationsModelDiffer = migrationsModelDiffer;
        _designTimeModel = designTimeModel;
        _activeProvider = databaseProvider.Name;
        _contextOptions = contextOptions;
        _executionStrategy = executionStrategy;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual void Migrate(string? targetMigration)
    {
        if (_connection.CurrentTransaction is not null
            && _executionStrategy.RetriesOnFailure)
        {
            throw new NotSupportedException(RelationalStrings.TransactionSuppressedMigrationInUserTransaction);
        }

        if (RelationalResources.LogPendingModelChanges(_logger).WarningBehavior != WarningBehavior.Ignore
            && HasPendingModelChanges())
        {
            _logger.PendingModelChangesWarning(_currentContext.Context.GetType());
        }

        _logger.MigrateUsingConnection(this, _connection);

        if (!_databaseCreator.Exists())
        {
            _databaseCreator.Create();
        }

        try
        {
            _connection.Open();

            if (!_historyRepository.Exists())
            {
                _historyRepository.Create();
            }

            _executionStrategy.Execute(
                (Migrator: this, TargetMigration: targetMigration, State: new MigrationExecutionState()),
                (_, s) => s.Migrator.MigrateImplementation(s.TargetMigration, s.State), verifySucceeded: null);
        }
        finally
        {
            _connection.Close();
        }
    }

    private bool MigrateImplementation(string? targetMigration, MigrationExecutionState state)
    {
        _connection.Open();
        var context = _currentContext.Context;
        using var transaction = context.Database.BeginTransaction();
        state.DatabaseLock = state.DatabaseLock == null
            ? _historyRepository.AcquireDatabaseLock(transaction)
            : state.DatabaseLock.Reacquire(transaction);

        PopulateMigrations(
            _historyRepository.GetAppliedMigrations().Select(t => t.MigrationId),
            targetMigration,
            out var migratorData);

        var commandLists = GetMigrationCommandLists(migratorData);
        foreach (var commandList in commandLists)
        {
            var (id, getCommands) = commandList;

            if (id != state.CurrentMigrationId)
            {
                state.CurrentMigrationId = id;
                state.LastCommittedCommandIndex = 0;
            }

            _migrationCommandExecutor.ExecuteNonQuery(getCommands(), _connection);
        }

        var coreOptionsExtension =
            _contextOptions.FindExtension<CoreOptionsExtension>()
            ?? new CoreOptionsExtension();

        var seed = coreOptionsExtension.Seeder;
        if (seed != null)
        {
            var operationsPerformed = migratorData.AppliedMigrations.Count != 0
                || migratorData.RevertedMigrations.Count != 0;
            seed(context, operationsPerformed);
        }
        else if (coreOptionsExtension.AsyncSeeder != null)
        {
            throw new InvalidOperationException(CoreStrings.MissingSeeder);
        }

        transaction.Commit();
        _connection.Close();
        return true;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual async Task MigrateAsync(
        string? targetMigration,
        CancellationToken cancellationToken = default)
    {
        if (_connection.CurrentTransaction is not null
            && _executionStrategy.RetriesOnFailure)
        {
            throw new NotSupportedException(RelationalStrings.TransactionSuppressedMigrationInUserTransaction);
        }

        if (RelationalResources.LogPendingModelChanges(_logger).WarningBehavior != WarningBehavior.Ignore
            && HasPendingModelChanges())
        {
            _logger.PendingModelChangesWarning(_currentContext.Context.GetType());
        }

        _logger.MigrateUsingConnection(this, _connection);

        if (!await _databaseCreator.ExistsAsync(cancellationToken).ConfigureAwait(false))
        {
            await _databaseCreator.CreateAsync(cancellationToken).ConfigureAwait(false);
        }

        try
        {
            await _connection.OpenAsync(cancellationToken).ConfigureAwait(false);

            if (!await _historyRepository.ExistsAsync(cancellationToken).ConfigureAwait(false))
            {
                await _historyRepository.CreateAsync(cancellationToken).ConfigureAwait(false);
            }

            await _executionStrategy.ExecuteAsync(
                (Migrator: this, TargetMigration: targetMigration, State: new MigrationExecutionState()),
                async (_, s, ct) => await s.Migrator.MigrateImplementationAsync(s.TargetMigration, s.State, ct).ConfigureAwait(false), verifySucceeded: null, cancellationToken)
                .ConfigureAwait(false);
        }
        finally
        {
            _connection.Close();
        }
    }

    private async Task<bool> MigrateImplementationAsync(string? targetMigration, MigrationExecutionState state, CancellationToken cancellationToken = default)
    {
        await _connection.OpenAsync(cancellationToken).ConfigureAwait(false);
        var context = _currentContext.Context;
        using var transaction = await context.Database.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);
        await using var _ = transaction.ConfigureAwait(false);

        state.DatabaseLock = state.DatabaseLock == null
            ? await _historyRepository.AcquireDatabaseLockAsync(transaction, cancellationToken).ConfigureAwait(false)
            : await state.DatabaseLock.ReacquireAsync(transaction, cancellationToken).ConfigureAwait(false);

        PopulateMigrations(
            (await _historyRepository.GetAppliedMigrationsAsync(cancellationToken).ConfigureAwait(false)).Select(t => t.MigrationId),
            targetMigration,
            out var migratorData);

        var commandLists = GetMigrationCommandLists(migratorData);
        foreach (var commandList in commandLists)
        {
            var (id, getCommands) = commandList;

            if (id != state.CurrentMigrationId)
            {
                state.CurrentMigrationId = id;
                state.LastCommittedCommandIndex = 0;
            }

            await _migrationCommandExecutor.ExecuteNonQueryAsync(getCommands(), _connection, cancellationToken)
                .ConfigureAwait(false);
        }

        var coreOptionsExtension =
            _contextOptions.FindExtension<CoreOptionsExtension>()
            ?? new CoreOptionsExtension();

        var seedAsync = coreOptionsExtension.AsyncSeeder;
        if (seedAsync != null)
        {
            var operationsPerformed = migratorData.AppliedMigrations.Count != 0
                || migratorData.RevertedMigrations.Count != 0;
            await seedAsync(context, operationsPerformed, cancellationToken).ConfigureAwait(false);
        }
        else if (coreOptionsExtension.Seeder != null)
        {
            throw new InvalidOperationException(CoreStrings.MissingSeeder);
        }

        await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
        _connection.Close();
        return true;
    }

    private IEnumerable<(string, Func<IReadOnlyList<MigrationCommand>>)> GetMigrationCommandLists(MigratorData parameters)
    {
        var migrationsToApply = parameters.AppliedMigrations;
        var migrationsToRevert = parameters.RevertedMigrations;
        var actualTargetMigration = parameters.TargetMigration;

        for (var i = 0; i < migrationsToRevert.Count; i++)
        {
            var migration = migrationsToRevert[i];

            var index = i;
            yield return (migration.GetId(), () =>
            {
                _logger.MigrationReverting(this, migration);

                var commands = GenerateDownSql(
                    migration,
                    index != migrationsToRevert.Count - 1
                        ? migrationsToRevert[index + 1]
                        : actualTargetMigration);
                if (migration.DownOperations.Count > 1
                    && commands.FirstOrDefault(c => c.TransactionSuppressed) is MigrationCommand nonTransactionalCommand)
                {
                    _logger.NonTransactionalMigrationOperationWarning(this, migration, nonTransactionalCommand);
                }

                return commands;
            });
        }

        foreach (var migration in migrationsToApply)
        {
            yield return (migration.GetId(), () =>
            {
                _logger.MigrationApplying(this, migration);

                var commands = GenerateUpSql(migration);
                if (migration.UpOperations.Count > 1
                    && commands.FirstOrDefault(c => c.TransactionSuppressed) is MigrationCommand nonTransactionalCommand)
                {
                    _logger.NonTransactionalMigrationOperationWarning(this, migration, nonTransactionalCommand);
                }

                return commands;
            });
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
        string? targetMigration,
        out MigratorData parameters)
    {
        var appliedMigrations = new Dictionary<string, TypeInfo>();
        var unappliedMigrations = new Dictionary<string, TypeInfo>();
        var appliedMigrationEntrySet = new HashSet<string>(appliedMigrationEntries, StringComparer.OrdinalIgnoreCase);
        if (_migrationsAssembly.Migrations.Count == 0)
        {
            _logger.MigrationsNotFound(this, _migrationsAssembly);
        }

        foreach (var (key, typeInfo) in _migrationsAssembly.Migrations)
        {
            if (appliedMigrationEntrySet.Contains(key))
            {
                appliedMigrations.Add(key, typeInfo);
            }
            else
            {
                unappliedMigrations.Add(key, typeInfo);
            }
        }

        IReadOnlyList<Migration> migrationsToApply;
        IReadOnlyList<Migration> migrationsToRevert;
        Migration? actualTargetMigration = null;
        if (string.IsNullOrEmpty(targetMigration))
        {
            migrationsToApply = unappliedMigrations
                .OrderBy(m => m.Key)
                .Select(p => _migrationsAssembly.CreateMigration(p.Value, _activeProvider))
                .ToList();
            migrationsToRevert = [];
        }
        else if (targetMigration == Migration.InitialDatabase)
        {
            migrationsToApply = [];
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

        parameters = new MigratorData(migrationsToApply, migrationsToRevert, actualTargetMigration);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual string GenerateScript(
        string? fromMigration = null,
        string? toMigration = null,
        MigrationsSqlGenerationOptions options = MigrationsSqlGenerationOptions.Default)
    {
        options |= MigrationsSqlGenerationOptions.Script;

        var idempotent = options.HasFlag(MigrationsSqlGenerationOptions.Idempotent);
        var noTransactions = options.HasFlag(MigrationsSqlGenerationOptions.NoTransactions);

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

        PopulateMigrations(appliedMigrations, toMigration, out var migratorData);

        var builder = new IndentedStringBuilder();

        if (fromMigration == Migration.InitialDatabase
            || string.IsNullOrEmpty(fromMigration))
        {
            builder
                .Append(_historyRepository.GetCreateIfNotExistsScript())
                .Append(_sqlGenerationHelper.BatchTerminator);
        }

        var idempotencyEnd = idempotent
            ? _historyRepository.GetEndIfScript()
            : null;
        var migrationsToApply = migratorData.AppliedMigrations;
        var migrationsToRevert = migratorData.RevertedMigrations;
        var actualTargetMigration = migratorData.TargetMigration;
        var transactionStarted = false;
        for (var i = 0; i < migrationsToRevert.Count; i++)
        {
            var migration = migrationsToRevert[i];
            var previousMigration = i != migrationsToRevert.Count - 1
                ? migrationsToRevert[i + 1]
                : actualTargetMigration;

            _logger.MigrationGeneratingDownScript(this, migration, fromMigration, toMigration, idempotent);

            var idempotencyCondition = idempotent
                ? _historyRepository.GetBeginIfExistsScript(migration.GetId())
                : null;

            GenerateSqlScript(
                GenerateDownSql(migration, previousMigration, options),
                builder, _sqlGenerationHelper, ref transactionStarted, noTransactions, idempotencyCondition, idempotencyEnd);
        }

        foreach (var migration in migrationsToApply)
        {
            _logger.MigrationGeneratingUpScript(this, migration, fromMigration, toMigration, idempotent);

            var idempotencyCondition = idempotent
                ? _historyRepository.GetBeginIfNotExistsScript(migration.GetId())
                : null;

            GenerateSqlScript(
                GenerateUpSql(migration, options),
                builder, _sqlGenerationHelper, ref transactionStarted, noTransactions, idempotencyCondition, idempotencyEnd);
        }

        if (!noTransactions && transactionStarted)
        {
            builder
                .AppendLine(_sqlGenerationHelper.CommitTransactionStatement)
                .Append(_sqlGenerationHelper.BatchTerminator);
        }

        return builder.ToString();
    }

    private static void GenerateSqlScript(
        IEnumerable<MigrationCommand> commands,
        IndentedStringBuilder builder,
        ISqlGenerationHelper sqlGenerationHelper,
        ref bool transactionStarted,
        bool noTransactions = false,
        string? idempotencyCondition = null,
        string? idempotencyEnd = null)
    {
        foreach (var command in commands)
        {
            if (!noTransactions)
            {
                if (!transactionStarted && !command.TransactionSuppressed)
                {
                    builder
                        .AppendLine(sqlGenerationHelper.StartTransactionStatement)
                        .Append(sqlGenerationHelper.BatchTerminator);
                    transactionStarted = true;
                }

                if (transactionStarted && command.TransactionSuppressed)
                {
                    builder
                        .AppendLine(sqlGenerationHelper.CommitTransactionStatement)
                        .Append(sqlGenerationHelper.BatchTerminator);
                    transactionStarted = false;
                }
            }

            if (idempotencyCondition != null
                && idempotencyEnd != null)
            {
                builder.AppendLine(idempotencyCondition);
                using (builder.Indent())
                {
                    builder.AppendLines(command.CommandText);
                }

                builder.Append(idempotencyEnd);
            }
            else
            {
                builder.Append(command.CommandText);
            }

            builder.Append(sqlGenerationHelper.BatchTerminator);
        }
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected virtual IReadOnlyList<MigrationCommand> GenerateUpSql(
        Migration migration,
        MigrationsSqlGenerationOptions options = MigrationsSqlGenerationOptions.Default)
    {
        var insertCommand = _rawSqlCommandBuilder.Build(
            _historyRepository.GetInsertScript(new HistoryRow(migration.GetId(), ProductInfo.GetVersion())));

        return _migrationsSqlGenerator
            .Generate(migration.UpOperations, FinalizeModel(migration.TargetModel), options)
            .Concat([new MigrationCommand(insertCommand, _currentContext.Context, _commandLogger)])
            .ToList();
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected virtual IReadOnlyList<MigrationCommand> GenerateDownSql(
        Migration migration,
        Migration? previousMigration,
        MigrationsSqlGenerationOptions options = MigrationsSqlGenerationOptions.Default)
    {
        var deleteCommand = _rawSqlCommandBuilder.Build(
            _historyRepository.GetDeleteScript(migration.GetId()));

        return _migrationsSqlGenerator
            .Generate(
                migration.DownOperations, previousMigration == null ? null : FinalizeModel(previousMigration.TargetModel), options)
            .Concat([new MigrationCommand(deleteCommand, _currentContext.Context, _commandLogger)])
            .ToList();
    }

    private IModel? FinalizeModel(IModel? model)
        => model == null
            ? null
            : _modelRuntimeInitializer.Initialize(model);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public bool HasPendingModelChanges()
        => _migrationsModelDiffer.HasDifferences(
            FinalizeModel(_migrationsAssembly.ModelSnapshot?.Model)?.GetRelationalModel(),
            _designTimeModel.Model.GetRelationalModel());
}
