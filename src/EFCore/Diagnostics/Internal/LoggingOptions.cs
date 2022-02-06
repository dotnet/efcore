// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Diagnostics.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class LoggingOptions : ILoggingOptions
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual void Initialize(IDbContextOptions options)
    {
        var coreOptions = options.FindExtension<CoreOptionsExtension>() ?? new CoreOptionsExtension();

        IsSensitiveDataLoggingEnabled = coreOptions.IsSensitiveDataLoggingEnabled;
        WarningsConfiguration = coreOptions.WarningsConfiguration;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual void Validate(IDbContextOptions options)
    {
        var coreOptions = options.FindExtension<CoreOptionsExtension>() ?? new CoreOptionsExtension();

        if (IsSensitiveDataLoggingEnabled != coreOptions.IsSensitiveDataLoggingEnabled)
        {
            Check.DebugAssert(coreOptions.InternalServiceProvider != null, "InternalServiceProvider is null");

            throw new InvalidOperationException(
                CoreStrings.SingletonOptionChanged(
                    nameof(DbContextOptionsBuilder.EnableSensitiveDataLogging),
                    nameof(DbContextOptionsBuilder.UseInternalServiceProvider)));
        }

        if (WarningsConfiguration.GetServiceProviderHashCode() != coreOptions.WarningsConfiguration.GetServiceProviderHashCode())
        {
            Check.DebugAssert(coreOptions.InternalServiceProvider != null, "InternalServiceProvider is null");

            throw new InvalidOperationException(
                CoreStrings.SingletonOptionChanged(
                    nameof(DbContextOptionsBuilder.ConfigureWarnings),
                    nameof(DbContextOptionsBuilder.UseInternalServiceProvider)));
        }
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual bool IsSensitiveDataLoggingEnabled { get; private set; }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual bool IsSensitiveDataLoggingWarned { get; set; }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual WarningsConfiguration WarningsConfiguration { get; private set; } = null!;
}
