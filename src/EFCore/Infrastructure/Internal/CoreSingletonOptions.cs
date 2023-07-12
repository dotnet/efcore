// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Infrastructure.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class CoreSingletonOptions : ICoreSingletonOptions
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

        AreDetailedErrorsEnabled = coreOptions.DetailedErrorsEnabled;
        AreThreadSafetyChecksEnabled = coreOptions.ThreadSafetyChecksEnabled;
        RootApplicationServiceProvider = coreOptions.RootApplicationServiceProvider;
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

        if (AreDetailedErrorsEnabled != coreOptions.DetailedErrorsEnabled)
        {
            Check.DebugAssert(coreOptions.InternalServiceProvider != null, "InternalServiceProvider is null");

            throw new InvalidOperationException(
                CoreStrings.SingletonOptionChanged(
                    nameof(DbContextOptionsBuilder.EnableDetailedErrors),
                    nameof(DbContextOptionsBuilder.UseInternalServiceProvider)));
        }

        if (AreThreadSafetyChecksEnabled != coreOptions.ThreadSafetyChecksEnabled)
        {
            Check.DebugAssert(coreOptions.InternalServiceProvider != null, "InternalServiceProvider is null");

            throw new InvalidOperationException(
                CoreStrings.SingletonOptionChanged(
                    nameof(DbContextOptionsBuilder.EnableThreadSafetyChecks),
                    nameof(DbContextOptionsBuilder.UseInternalServiceProvider)));
        }

        if (RootApplicationServiceProvider != coreOptions.RootApplicationServiceProvider)
        {
            throw new InvalidOperationException(
                CoreStrings.SingletonOptionChanged(
                    nameof(DbContextOptionsBuilder.UseRootApplicationServiceProvider),
                    nameof(DbContextOptionsBuilder.UseInternalServiceProvider)));
        }
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual bool AreDetailedErrorsEnabled { get; private set; }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual bool AreThreadSafetyChecksEnabled { get; private set; }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual IServiceProvider? RootApplicationServiceProvider { get; private set; }
}
