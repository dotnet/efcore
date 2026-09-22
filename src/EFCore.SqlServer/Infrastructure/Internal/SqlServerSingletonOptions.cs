// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.SqlServer.Infrastructure.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class SqlServerSingletonOptions : ISqlServerSingletonOptions
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual SqlServerEngineType EngineType { get; private set; }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual int SqlServerCompatibilityLevel { get; private set; } = SqlServerOptionsExtension.SqlServerDefaultCompatibilityLevel;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual int AzureSqlCompatibilityLevel { get; private set; } = SqlServerOptionsExtension.AzureSqlDefaultCompatibilityLevel;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual int AzureSynapseCompatibilityLevel { get; private set; } =
        SqlServerOptionsExtension.AzureSynapseDefaultCompatibilityLevel;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual void Initialize(IDbContextOptions options)
    {
        var sqlServerOptions = options.FindExtension<SqlServerOptionsExtension>();
        if (sqlServerOptions != null)
        {
            EngineType = sqlServerOptions.EngineType;
            SqlServerCompatibilityLevel = sqlServerOptions.SqlServerCompatibilityLevel;
            AzureSqlCompatibilityLevel = sqlServerOptions.AzureSqlCompatibilityLevel;
            AzureSynapseCompatibilityLevel = sqlServerOptions.AzureSynapseCompatibilityLevel;
        }
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual void Validate(IDbContextOptions options)
    {
        var sqlServerOptions = options.FindExtension<SqlServerOptionsExtension>();

        if (sqlServerOptions != null)
        {
            if (EngineType == SqlServerEngineType.SqlServer
                && (EngineType != sqlServerOptions.EngineType
                    || SqlServerCompatibilityLevel != sqlServerOptions.SqlServerCompatibilityLevel))
            {
                throw new InvalidOperationException(
                    CoreStrings.SingletonOptionChanged(
                        $"{nameof(SqlServerDbContextOptionsExtensions.UseSqlServer)}",
                        nameof(DbContextOptionsBuilder.UseInternalServiceProvider)));
            }

            if (EngineType == SqlServerEngineType.AzureSql
                && (EngineType != sqlServerOptions.EngineType || AzureSqlCompatibilityLevel != sqlServerOptions.AzureSqlCompatibilityLevel))
            {
                throw new InvalidOperationException(
                    CoreStrings.SingletonOptionChanged(
                        $"{nameof(SqlServerDbContextOptionsExtensions.UseAzureSql)}",
                        nameof(DbContextOptionsBuilder.UseInternalServiceProvider)));
            }

            if (EngineType == SqlServerEngineType.AzureSynapse
                && (EngineType != sqlServerOptions.EngineType
                    || AzureSynapseCompatibilityLevel != sqlServerOptions.AzureSynapseCompatibilityLevel))
            {
                throw new InvalidOperationException(
                    CoreStrings.SingletonOptionChanged(
                        $"{nameof(SqlServerDbContextOptionsExtensions.UseAzureSynapse)}",
                        nameof(DbContextOptionsBuilder.UseInternalServiceProvider)));
            }
        }
    }
}
