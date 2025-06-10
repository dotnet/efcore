// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Text;
using Microsoft.EntityFrameworkCore.SqlServer.Internal;

namespace Microsoft.EntityFrameworkCore.SqlServer.Infrastructure.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class SqlServerOptionsExtension : RelationalOptionsExtension, IDbContextOptionsExtension
{
    private DbContextOptionsExtensionInfo? _info;
    private SqlServerEngineType _engineType;
    private int? _sqlServerCompatibilityLevel;
    private int? _azureSqlCompatibilityLevel;
    private int? _azureSynapseCompatibilityLevel;
    private bool _useRetryingStrategyByDefault;

    // For the SQL Server/Azure SQL compatibility levels, see
    // https://learn.microsoft.com/sql/t-sql/statements/alter-database-transact-sql-compatibility-level
    // SQL Server 2025 (17.x): compatibility level 170 (default for Azure SQL, currently preview for on-prem)
    // SQL Server 2022 (16.x): compatibility level 160, start date 2022-11-16, mainstream end date 2028-01-11, extended end date 2033-01-11
    // SQL Server 2019 (15.x): compatibility level 150, start date 2019-11-04, mainstream end date 2025-02-28, extended end date 2030-01-08
    // SQL Server 2017 (14.x): compatibility level 140, start date 2017-09-29, mainstream end date 2022-10-11, extended end date 2027-10-12
    // SQL Server 2016 (13.x): compatibility level 130, start date 2016-06-01, mainstream end date 2021-07-13, extended end date 2026-07-14
    // SQL Server 2014 (12.x): compatibility level 120, start date 2014-06-05, mainstream end date 2019-07-09, extended end date 2024-07-09

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static readonly int SqlServerDefaultCompatibilityLevel = 150;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    // Azure SQL compatibility levels are the same as SQL Server compatibility levels, see table above
    public static readonly int AzureSqlDefaultCompatibilityLevel = 170;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    // See https://learn.microsoft.com/en-us/sql/t-sql/statements/alter-database-scoped-configuration-transact-sql
    public static readonly int AzureSynapseDefaultCompatibilityLevel = 30;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public SqlServerOptionsExtension()
    {
    }

    // NB: When adding new options, make sure to update the copy ctor below.

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected SqlServerOptionsExtension(SqlServerOptionsExtension copyFrom)
        : base(copyFrom)
    {
        _engineType = copyFrom._engineType;
        _sqlServerCompatibilityLevel = copyFrom._sqlServerCompatibilityLevel;
        _azureSqlCompatibilityLevel = copyFrom._azureSqlCompatibilityLevel;
        _azureSynapseCompatibilityLevel = copyFrom._azureSynapseCompatibilityLevel;
        _useRetryingStrategyByDefault = copyFrom._useRetryingStrategyByDefault;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override DbContextOptionsExtensionInfo Info
        => _info ??= new ExtensionInfo(this);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override RelationalOptionsExtension Clone()
        => new SqlServerOptionsExtension(this);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual SqlServerEngineType EngineType
        => _engineType;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual int SqlServerCompatibilityLevel
        => _sqlServerCompatibilityLevel ?? SqlServerDefaultCompatibilityLevel;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual int AzureSqlCompatibilityLevel
        => _azureSqlCompatibilityLevel ?? AzureSqlDefaultCompatibilityLevel;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual int AzureSynapseCompatibilityLevel
        => _azureSynapseCompatibilityLevel ?? AzureSynapseDefaultCompatibilityLevel;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual bool UseRetryingStrategyByDefault
        => _useRetryingStrategyByDefault;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual SqlServerOptionsExtension WithEngineType(SqlServerEngineType engineType)
    {
        if (EngineType != SqlServerEngineType.Unknown && EngineType != engineType)
        {
            throw new InvalidOperationException(SqlServerStrings.AlreadyConfiguredEngineType(engineType, EngineType));
        }

        var clone = (SqlServerOptionsExtension)Clone();

        clone._engineType = engineType;

        return clone;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual SqlServerOptionsExtension WithLegacyAzureSql(bool enable)
    {
        var clone = (SqlServerOptionsExtension)Clone();

        clone._engineType = SqlServerEngineType.SqlServer;
        clone._useRetryingStrategyByDefault = enable;

        return clone;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual SqlServerOptionsExtension WithSqlServerCompatibilityLevel(int? sqlServerCompatibilityLevel)
    {
        var clone = (SqlServerOptionsExtension)Clone();

        clone._sqlServerCompatibilityLevel = sqlServerCompatibilityLevel;

        return clone;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual SqlServerOptionsExtension WithAzureSqlCompatibilityLevel(int? azureSqlCompatibilityLevel)
    {
        var clone = (SqlServerOptionsExtension)Clone();

        clone._azureSqlCompatibilityLevel = azureSqlCompatibilityLevel;

        return clone;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual SqlServerOptionsExtension WithAzureSynapseCompatibilityLevel(int? azureSynapseCompatibilityLevel)
    {
        var clone = (SqlServerOptionsExtension)Clone();

        clone._azureSynapseCompatibilityLevel = _azureSynapseCompatibilityLevel;

        return clone;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual SqlServerOptionsExtension WithUseRetryingStrategyByDefault(bool enable)
    {
        var clone = (SqlServerOptionsExtension)Clone();

        clone._useRetryingStrategyByDefault = enable;

        return clone;
    }

    /// <inheritdoc />
    public virtual IDbContextOptionsExtension ApplyDefaults(IDbContextOptions options)
    {
        if (ExecutionStrategyFactory == null
            && (EngineType == SqlServerEngineType.AzureSql
                || EngineType == SqlServerEngineType.AzureSynapse
                || UseRetryingStrategyByDefault))
        {
            return WithExecutionStrategyFactory(c => new SqlServerRetryingExecutionStrategy(c));
        }

        return this;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override void ApplyServices(IServiceCollection services)
    {
        switch (_engineType)
        {
            case SqlServerEngineType.SqlServer:
                services.AddEntityFrameworkSqlServer();
                break;
            case SqlServerEngineType.AzureSql:
                services.AddEntityFrameworkAzureSql();
                break;
            case SqlServerEngineType.AzureSynapse:
                services.AddEntityFrameworkAzureSynapse();
                break;
        }
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override void Validate(IDbContextOptions options)
    {
        base.Validate(options);
        if (EngineType == SqlServerEngineType.Unknown)
        {
            throw new InvalidOperationException(
                SqlServerStrings.InvalidEngineType(
                    $"{nameof(SqlServerDbContextOptionsExtensions.UseSqlServer)}/{nameof(SqlServerDbContextOptionsExtensions.UseAzureSql)}/{nameof(SqlServerDbContextOptionsExtensions.UseAzureSynapse)}"));
        }
    }

    private sealed class ExtensionInfo(IDbContextOptionsExtension extension) : RelationalExtensionInfo(extension)
    {
        private string? _logFragment;

        private new SqlServerOptionsExtension Extension
            => (SqlServerOptionsExtension)base.Extension;

        public override bool ShouldUseSameServiceProvider(DbContextOptionsExtensionInfo other)
            => other is ExtensionInfo otherInfo
                && Extension.EngineType == otherInfo.Extension.EngineType
                && Extension.SqlServerCompatibilityLevel == otherInfo.Extension.SqlServerCompatibilityLevel
                && Extension.AzureSqlCompatibilityLevel == otherInfo.Extension.AzureSqlCompatibilityLevel
                && Extension.AzureSynapseCompatibilityLevel == otherInfo.Extension.AzureSynapseCompatibilityLevel;

        public override string LogFragment
        {
            get
            {
                if (_logFragment == null)
                {
                    var builder = new StringBuilder();

                    builder.Append(base.LogFragment);

                    builder
                        .Append("EngineType=")
                        .Append(Extension._engineType)
                        .Append(' ');

                    if (Extension._useRetryingStrategyByDefault)
                    {
                        builder
                            .Append("UseRetryingStrategyByDefault=")
                            .Append(Extension._useRetryingStrategyByDefault)
                            .Append(' ');
                    }

                    if (Extension._sqlServerCompatibilityLevel != null)
                    {
                        builder
                            .Append("SqlServerCompatibilityLevel=")
                            .Append(Extension._sqlServerCompatibilityLevel)
                            .Append(' ');
                    }

                    if (Extension._azureSqlCompatibilityLevel != null)
                    {
                        builder
                            .Append("AzureSqlCompatibilityLevel=")
                            .Append(Extension._azureSqlCompatibilityLevel)
                            .Append(' ');
                    }

                    if (Extension._azureSynapseCompatibilityLevel != null)
                    {
                        builder
                            .Append("AzureSynapseCompatibilityLevel=")
                            .Append(Extension._azureSynapseCompatibilityLevel)
                            .Append(' ');
                    }

                    _logFragment = builder.ToString();
                }

                return _logFragment;
            }
        }

        public override void PopulateDebugInfo(IDictionary<string, string> debugInfo)
        {
            debugInfo["EngineType"] = Extension.EngineType.ToString();
            debugInfo["UseRetryingStrategyByDefault"] = Extension.UseRetryingStrategyByDefault.ToString();

            if (Extension.SqlServerCompatibilityLevel is int sqlServerCompatibilityLevel)
            {
                debugInfo["SqlServerCompatibilityLevel"] = sqlServerCompatibilityLevel.ToString();
            }

            if (Extension.AzureSqlCompatibilityLevel is int azureSqlCompatibilityLevel)
            {
                debugInfo["AzureSqlCompatibilityLevel"] = azureSqlCompatibilityLevel.ToString();
            }

            if (Extension.AzureSynapseCompatibilityLevel is int azureSynapseCompatibilityLevel)
            {
                debugInfo["AzureSynapseCompatibilityLevel"] = azureSynapseCompatibilityLevel.ToString();
            }
        }
    }
}
