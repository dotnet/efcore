// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Text;

namespace Microsoft.EntityFrameworkCore.Migrations;

/// <summary>
///     A base class for the repository used to access the '__EFMigrationsHistory' table that tracks metadata
///     about EF Core Migrations such as which migrations have been applied.
/// </summary>
/// <remarks>
///     <para>
///         Database providers must inherit from this class to implement provider-specific functionality.
///     </para>
///     <para>
///         The service lifetime is <see cref="ServiceLifetime.Scoped" />. This means that each
///         <see cref="DbContext" /> instance will use its own instance of this service.
///         The implementation may depend on other services registered with any lifetime.
///         The implementation does not need to be thread-safe.
///     </para>
///     <para>
///         See <see href="https://aka.ms/efcore-docs-migrations">Database migrations</see> for more information and examples.
///     </para>
/// </remarks>
// TODO: Leverage query pipeline for GetAppliedMigrations
// TODO: Leverage update pipeline for GetInsertScript & GetDeleteScript
public abstract class HistoryRepository : IHistoryRepository
{
    /// <summary>
    ///     The default name for the Migrations history table.
    /// </summary>
    public const string DefaultTableName = "__EFMigrationsHistory";

    private IModel? _model;
    private string? _migrationIdColumnName;
    private string? _productVersionColumnName;

    /// <summary>
    ///     Initializes a new instance of this class.
    /// </summary>
    /// <param name="dependencies">Parameter object containing dependencies for this service.</param>
    protected HistoryRepository(HistoryRepositoryDependencies dependencies)
    {
        Dependencies = dependencies;

        var relationalOptions = RelationalOptionsExtension.Extract(dependencies.Options);
        TableName = relationalOptions.MigrationsHistoryTableName ?? DefaultTableName;
        TableSchema = relationalOptions.MigrationsHistoryTableSchema;
    }

    /// <summary>
    ///     Relational provider-specific dependencies for this service.
    /// </summary>
    protected virtual HistoryRepositoryDependencies Dependencies { get; }

    /// <summary>
    ///     A helper class for generation of SQL.
    /// </summary>
    protected virtual ISqlGenerationHelper SqlGenerationHelper
        => Dependencies.SqlGenerationHelper;

    /// <summary>
    ///     THe history table name.
    /// </summary>
    protected virtual string TableName { get; }

    /// <summary>
    ///     The schema that contains the history table, or <see langword="null" /> if the default schema should be used.
    /// </summary>
    protected virtual string? TableSchema { get; }

    /// <summary>
    ///     The name of the column that holds the Migration identifier.
    /// </summary>
    protected virtual string MigrationIdColumnName
        => _migrationIdColumnName ??= EnsureModel()
            .FindEntityType(typeof(HistoryRow))!
            .FindProperty(nameof(HistoryRow.MigrationId))!
            .GetColumnName();

    private IModel EnsureModel()
    {
        if (_model == null)
        {
            var conventionSet = Dependencies.ConventionSetBuilder.CreateConventionSet();

            conventionSet.Remove(typeof(DbSetFindingConvention));
            conventionSet.Remove(typeof(RelationalDbFunctionAttributeConvention));

            var modelBuilder = new ModelBuilder(conventionSet);
            modelBuilder.Entity<HistoryRow>(
                x =>
                {
                    ConfigureTable(x);
                    x.ToTable(TableName, TableSchema);
                });

            _model = Dependencies.ModelRuntimeInitializer.Initialize(
                (IModel)modelBuilder.Model, designTime: true, validationLogger: null);
        }

        return _model;
    }

    /// <summary>
    ///     The name of the column that contains the Entity Framework product version.
    /// </summary>
    protected virtual string ProductVersionColumnName
        => _productVersionColumnName ??= EnsureModel()
            .FindEntityType(typeof(HistoryRow))!
            .FindProperty(nameof(HistoryRow.ProductVersion))!
            .GetColumnName();

    /// <summary>
    ///     Overridden by database providers to generate SQL that tests for existence of the history table.
    /// </summary>
    protected abstract string ExistsSql { get; }

    /// <summary>
    ///     Checks whether or not the history table exists.
    /// </summary>
    /// <returns><see langword="true" /> if the table already exists, <see langword="false" /> otherwise.</returns>
    public virtual bool Exists()
        => Dependencies.DatabaseCreator.Exists()
            && InterpretExistsResult(
                Dependencies.RawSqlCommandBuilder.Build(ExistsSql).ExecuteScalar(
                    new RelationalCommandParameterObject(
                        Dependencies.Connection,
                        null,
                        null,
                        Dependencies.CurrentContext.Context,
                        Dependencies.CommandLogger, CommandSource.Migrations)));

    /// <summary>
    ///     Checks whether or not the history table exists.
    /// </summary>
    /// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
    /// <returns>
    ///     A task that represents the asynchronous operation. The task result contains
    ///     <see langword="true" /> if the table already exists, <see langword="false" /> otherwise.
    /// </returns>
    /// <exception cref="OperationCanceledException">If the <see cref="CancellationToken" /> is canceled.</exception>
    public virtual async Task<bool> ExistsAsync(CancellationToken cancellationToken = default)
        => await Dependencies.DatabaseCreator.ExistsAsync(cancellationToken).ConfigureAwait(false)
            && InterpretExistsResult(
                await Dependencies.RawSqlCommandBuilder.Build(ExistsSql).ExecuteScalarAsync(
                    new RelationalCommandParameterObject(
                        Dependencies.Connection,
                        null,
                        null,
                        Dependencies.CurrentContext.Context,
                        Dependencies.CommandLogger, CommandSource.Migrations),
                    cancellationToken).ConfigureAwait(false));

    /// <summary>
    ///     Interprets the result of executing <see cref="ExistsSql" />.
    /// </summary>
    /// <returns><see langword="true" /> if the table already exists, <see langword="false" /> otherwise.</returns>
    protected abstract bool InterpretExistsResult(object? value);

    /// <summary>
    ///     Overridden by a database provider to generate a SQL script that will create the history table
    ///     if and only if it does not already exist.
    /// </summary>
    /// <returns>The SQL script.</returns>
    public abstract string GetCreateIfNotExistsScript();

    /// <summary>
    ///     Generates a SQL script that will create the history table.
    /// </summary>
    /// <returns>The SQL script.</returns>
    public virtual string GetCreateScript()
    {
        var model = EnsureModel();

        var operations = Dependencies.ModelDiffer.GetDifferences(null, model.GetRelationalModel());
        var commandList = Dependencies.MigrationsSqlGenerator.Generate(operations, model);

        return string.Concat(commandList.Select(c => c.CommandText));
    }

    /// <summary>
    ///     Configures the entity type mapped to the history table.
    /// </summary>
    /// <remarks>
    ///     Database providers can override this to add or replace configuration.
    /// </remarks>
    /// <param name="history">A builder for the <see cref="HistoryRow" /> entity type.</param>
    protected virtual void ConfigureTable(EntityTypeBuilder<HistoryRow> history)
    {
        history.ToTable(DefaultTableName);
        history.HasKey(h => h.MigrationId);
        history.Property(h => h.MigrationId).HasMaxLength(150);
        history.Property(h => h.ProductVersion).HasMaxLength(32).IsRequired();
    }

    /// <summary>
    ///     Queries the history table for all migrations that have been applied.
    /// </summary>
    /// <returns>The list of applied migrations, as <see cref="HistoryRow" /> entities.</returns>
    public virtual IReadOnlyList<HistoryRow> GetAppliedMigrations()
    {
        var rows = new List<HistoryRow>();

        if (Exists())
        {
            var command = Dependencies.RawSqlCommandBuilder.Build(GetAppliedMigrationsSql);

            using var reader = command.ExecuteReader(
                new RelationalCommandParameterObject(
                    Dependencies.Connection,
                    null,
                    null,
                    Dependencies.CurrentContext.Context,
                    Dependencies.CommandLogger, CommandSource.Migrations));
            while (reader.Read())
            {
                rows.Add(new HistoryRow(reader.DbDataReader.GetString(0), reader.DbDataReader.GetString(1)));
            }
        }

        return rows;
    }

    /// <summary>
    ///     Queries the history table for all migrations that have been applied.
    /// </summary>
    /// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
    /// <returns>
    ///     A task that represents the asynchronous operation. The task result contains
    ///     the list of applied migrations, as <see cref="HistoryRow" /> entities.
    /// </returns>
    /// <exception cref="OperationCanceledException">If the <see cref="CancellationToken" /> is canceled.</exception>
    public virtual async Task<IReadOnlyList<HistoryRow>> GetAppliedMigrationsAsync(
        CancellationToken cancellationToken = default)
    {
        var rows = new List<HistoryRow>();

        if (await ExistsAsync(cancellationToken).ConfigureAwait(false))
        {
            var command = Dependencies.RawSqlCommandBuilder.Build(GetAppliedMigrationsSql);

            var reader = await command.ExecuteReaderAsync(
                new RelationalCommandParameterObject(
                    Dependencies.Connection,
                    null,
                    null,
                    Dependencies.CurrentContext.Context,
                    Dependencies.CommandLogger, CommandSource.Migrations),
                cancellationToken).ConfigureAwait(false);

            await using var _ = reader.ConfigureAwait(false);

            while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
            {
                rows.Add(new HistoryRow(reader.DbDataReader.GetString(0), reader.DbDataReader.GetString(1)));
            }
        }

        return rows;
    }

    /// <summary>
    ///     Generates SQL to query for the migrations that have been applied.
    /// </summary>
    protected virtual string GetAppliedMigrationsSql
        => new StringBuilder()
            .Append("SELECT ")
            .Append(SqlGenerationHelper.DelimitIdentifier(MigrationIdColumnName))
            .Append(", ")
            .AppendLine(SqlGenerationHelper.DelimitIdentifier(ProductVersionColumnName))
            .Append("FROM ")
            .AppendLine(SqlGenerationHelper.DelimitIdentifier(TableName, TableSchema))
            .Append("ORDER BY ")
            .Append(SqlGenerationHelper.DelimitIdentifier(MigrationIdColumnName))
            .Append(SqlGenerationHelper.StatementTerminator)
            .ToString();

    /// <summary>
    ///     Generates a SQL script to insert a row into the history table.
    /// </summary>
    /// <param name="row">The row to insert, represented as a <see cref="HistoryRow" /> entity.</param>
    /// <returns>The generated SQL.</returns>
    public virtual string GetInsertScript(HistoryRow row)
    {
        var stringTypeMapping = Dependencies.TypeMappingSource.GetMapping(typeof(string));

        return new StringBuilder().Append("INSERT INTO ")
            .Append(SqlGenerationHelper.DelimitIdentifier(TableName, TableSchema))
            .Append(" (")
            .Append(SqlGenerationHelper.DelimitIdentifier(MigrationIdColumnName))
            .Append(", ")
            .Append(SqlGenerationHelper.DelimitIdentifier(ProductVersionColumnName))
            .AppendLine(")")
            .Append("VALUES (")
            .Append(stringTypeMapping.GenerateSqlLiteral(row.MigrationId))
            .Append(", ")
            .Append(stringTypeMapping.GenerateSqlLiteral(row.ProductVersion))
            .Append(')')
            .AppendLine(SqlGenerationHelper.StatementTerminator)
            .ToString();
    }

    /// <summary>
    ///     Generates a SQL script to delete a row from the history table.
    /// </summary>
    /// <param name="migrationId">The migration identifier of the row to delete.</param>
    /// <returns>The generated SQL.</returns>
    public virtual string GetDeleteScript(string migrationId)
    {
        var stringTypeMapping = Dependencies.TypeMappingSource.GetMapping(typeof(string));

        return new StringBuilder().Append("DELETE FROM ")
            .AppendLine(SqlGenerationHelper.DelimitIdentifier(TableName, TableSchema))
            .Append("WHERE ")
            .Append(SqlGenerationHelper.DelimitIdentifier(MigrationIdColumnName))
            .Append(" = ")
            .Append(stringTypeMapping.GenerateSqlLiteral(migrationId))
            .AppendLine(SqlGenerationHelper.StatementTerminator)
            .ToString();
    }

    /// <summary>
    ///     Overridden by database providers to generate a SQL Script that will <c>BEGIN</c> a block
    ///     of SQL if and only if the migration with the given identifier does not already exist in the history table.
    /// </summary>
    /// <param name="migrationId">The migration identifier.</param>
    /// <returns>The generated SQL.</returns>
    public abstract string GetBeginIfNotExistsScript(string migrationId);

    /// <summary>
    ///     Overridden by database providers to generate a SQL Script that will <c>BEGIN</c> a block
    ///     of SQL if and only if the migration with the given identifier already exists in the history table.
    /// </summary>
    /// <param name="migrationId">The migration identifier.</param>
    /// <returns>The generated SQL.</returns>
    public abstract string GetBeginIfExistsScript(string migrationId);

    /// <summary>
    ///     Overridden by database providers to generate a SQL script to <c>END</c> the SQL block.
    /// </summary>
    /// <returns>The generated SQL.</returns>
    public abstract string GetEndIfScript();
}
