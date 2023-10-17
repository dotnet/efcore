// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Text;

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
    private int? _compatibilityLevel;
    private bool? _azureSql;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    // See https://learn.microsoft.com/sql/t-sql/statements/alter-database-transact-sql-compatibility-level
    // SQL Server 2022 (16.x): compatibility level 160, start date 2022-11-16, mainstream end date 2028-01-11, extended end date 2033-01-11
    // SQL Server 2019 (15.x): compatibility level 150, start date 2019-11-04, mainstream end date 2025-02-28, extended end date 2030-01-08
    // SQL Server 2017 (14.x): compatibility level 140, start date 2017-09-29, mainstream end date 2022-10-11, extended end date 2027-10-12
    // SQL Server 2016 (13.x): compatibility level 130, start date 2016-06-01, mainstream end date 2021-07-13, extended end date 2026-07-14
    // SQL Server 2014 (12.x): compatibility level 120, start date 2014-06-05, mainstream end date 2019-07-09, extended end date 2024-07-09
    public static readonly int DefaultCompatibilityLevel = 160;

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
        _compatibilityLevel = copyFrom._compatibilityLevel;
        _azureSql = copyFrom._azureSql;
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
    public virtual int CompatibilityLevel
        => _compatibilityLevel ?? DefaultCompatibilityLevel;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual int? CompatibilityLevelWithoutDefault
        => _compatibilityLevel;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual SqlServerOptionsExtension WithCompatibilityLevel(int? compatibilityLevel)
    {
        var clone = (SqlServerOptionsExtension)Clone();

        clone._compatibilityLevel = compatibilityLevel;

        return clone;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual bool IsAzureSql
        => _azureSql ?? false;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual SqlServerOptionsExtension WithAzureSql(bool enable)
    {
        var clone = (SqlServerOptionsExtension)Clone();

        clone._azureSql = enable;

        return clone;
    }

    /// <inheritdoc />
    public virtual IDbContextOptionsExtension ApplyDefaults(IDbContextOptions options)
    {
        if (!IsAzureSql)
        {
            return this;
        }

        if (ExecutionStrategyFactory == null)
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
        => services.AddEntityFrameworkSqlServer();

    private sealed class ExtensionInfo : RelationalExtensionInfo
    {
        private string? _logFragment;

        public ExtensionInfo(IDbContextOptionsExtension extension)
            : base(extension)
        {
        }

        private new SqlServerOptionsExtension Extension
            => (SqlServerOptionsExtension)base.Extension;

        public override bool IsDatabaseProvider
            => true;

        public override bool ShouldUseSameServiceProvider(DbContextOptionsExtensionInfo other)
            => other is ExtensionInfo otherInfo
                && Extension.CompatibilityLevel == otherInfo.Extension.CompatibilityLevel;

        public override string LogFragment
        {
            get
            {
                if (_logFragment == null)
                {
                    var builder = new StringBuilder();

                    builder.Append(base.LogFragment);

                    if (Extension._compatibilityLevel is int compatibilityLevel)
                    {
                        builder
                            .Append("CompatibilityLevel=")
                            .Append(compatibilityLevel);
                    }

                    _logFragment = builder.ToString();
                }

                return _logFragment;
            }
        }

        public override void PopulateDebugInfo(IDictionary<string, string> debugInfo)
        {
            debugInfo["SqlServer"] = "1";

            if (Extension.CompatibilityLevel is int compatibilityLevel)
            {
                debugInfo["CompatibilityLevel"] = compatibilityLevel.ToString();
            }
        }
    }
}
