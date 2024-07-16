// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Text;
using Microsoft.Data.SqlClient;

namespace Microsoft.EntityFrameworkCore.SqlServer.Migrations.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class SqlServerHistoryRepository : HistoryRepository
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public SqlServerHistoryRepository(HistoryRepositoryDependencies dependencies)
        : base(dependencies)
    {
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override string ExistsSql
    {
        get
        {
            var stringTypeMapping = Dependencies.TypeMappingSource.GetMapping(typeof(string));

            return "SELECT OBJECT_ID("
                + stringTypeMapping.GenerateSqlLiteral(
                    SqlGenerationHelper.DelimitIdentifier(TableName, TableSchema))
                + ")"
                + SqlGenerationHelper.StatementTerminator;
        }
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override bool InterpretExistsResult(object? value)
        => value != DBNull.Value;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override IDisposable GetDatabaseLock(TimeSpan timeout)
    {
        var dbLock = CreateMigrationDatabaseLock();
        int result;
        try
        {
            result = (int)CreateGetLockCommand(timeout).ExecuteScalar(CreateRelationalCommandParameters())!;
        }
        catch
        {
            try
            {
                dbLock.Dispose();
            }
            catch
            {
            }

            throw;
        }

        return result < 0
            ? throw new TimeoutException()
            : dbLock;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override async Task<IAsyncDisposable> GetDatabaseLockAsync(TimeSpan timeout, CancellationToken cancellationToken = default)
    {
        var dbLock = CreateMigrationDatabaseLock();
        int result;
        try
        {
            result = (int)(await CreateGetLockCommand(timeout).ExecuteScalarAsync(CreateRelationalCommandParameters(), cancellationToken)
                .ConfigureAwait(false))!;
        }
        catch
        {
            try
            {
                await dbLock.DisposeAsync().ConfigureAwait(false);
            }
            catch
            {
            }

            throw;
        }

        return result < 0
            ? throw new TimeoutException()
            : dbLock;
    }

    private IRelationalCommand CreateGetLockCommand(TimeSpan timeout)
        => Dependencies.RawSqlCommandBuilder.Build("""
DECLARE @result int;
EXEC @result = sp_getapplock @Resource = '__EFMigrationsLock', @LockOwner = 'Session', @LockMode = 'Exclusive', @LockTimeout = @LockTimeout;
SELECT @result
""",
            [new SqlParameter("@LockTimeout", timeout.TotalMilliseconds)]).RelationalCommand;

    private SqlServerMigrationDatabaseLock CreateMigrationDatabaseLock()
        => new(
            Dependencies.RawSqlCommandBuilder.Build("""
DECLARE @result int;
EXEC @result = sp_releaseapplock @Resource = '__EFMigrationsLock', @LockOwner = 'Session';
SELECT @result
"""),
            CreateRelationalCommandParameters());

    private RelationalCommandParameterObject CreateRelationalCommandParameters()
        => new(
            Dependencies.Connection,
            null,
            null,
            Dependencies.CurrentContext.Context,
            Dependencies.CommandLogger, CommandSource.Migrations);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override string GetCreateIfNotExistsScript()
    {
        var stringTypeMapping = Dependencies.TypeMappingSource.GetMapping(typeof(string));

        var builder = new StringBuilder()
            .Append("IF OBJECT_ID(")
            .Append(
                stringTypeMapping.GenerateSqlLiteral(
                    SqlGenerationHelper.DelimitIdentifier(TableName, TableSchema)))
            .AppendLine(") IS NULL")
            .AppendLine("BEGIN");

        using (var reader = new StringReader(GetCreateScript()))
        {
            var first = true;
            string? line;
            while ((line = reader.ReadLine()) != null)
            {
                if (first)
                {
                    first = false;
                }
                else
                {
                    builder.AppendLine();
                }

                if (line.Length != 0)
                {
                    builder
                        .Append("    ")
                        .Append(line);
                }
            }
        }

        builder
            .AppendLine()
            .Append("END")
            .AppendLine(SqlGenerationHelper.StatementTerminator);

        return builder.ToString();
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override string GetBeginIfNotExistsScript(string migrationId)
    {
        var stringTypeMapping = Dependencies.TypeMappingSource.GetMapping(typeof(string));

        return new StringBuilder()
            .AppendLine("IF NOT EXISTS (")
            .Append("    SELECT * FROM ")
            .AppendLine(SqlGenerationHelper.DelimitIdentifier(TableName, TableSchema))
            .Append("    WHERE ")
            .Append(SqlGenerationHelper.DelimitIdentifier(MigrationIdColumnName))
            .Append(" = ").AppendLine(stringTypeMapping.GenerateSqlLiteral(migrationId))
            .AppendLine(")")
            .Append("BEGIN")
            .ToString();
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override string GetBeginIfExistsScript(string migrationId)
    {
        var stringTypeMapping = Dependencies.TypeMappingSource.GetMapping(typeof(string));

        return new StringBuilder()
            .AppendLine("IF EXISTS (")
            .Append("    SELECT * FROM ")
            .AppendLine(SqlGenerationHelper.DelimitIdentifier(TableName, TableSchema))
            .Append("    WHERE ")
            .Append(SqlGenerationHelper.DelimitIdentifier(MigrationIdColumnName))
            .Append(" = ")
            .AppendLine(stringTypeMapping.GenerateSqlLiteral(migrationId))
            .AppendLine(")")
            .Append("BEGIN")
            .ToString();
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override string GetEndIfScript()
        => new StringBuilder()
            .Append("END")
            .AppendLine(SqlGenerationHelper.StatementTerminator)
            .ToString();
}
