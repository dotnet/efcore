// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Diagnostics.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class ScopedLoggerFactory : ILoggerFactory
{
    private readonly ILoggerFactory _underlyingFactory;
    private readonly bool _dispose;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public ScopedLoggerFactory(
        ILoggerFactory loggerFactory,
        bool dispose)
    {
        _underlyingFactory = loggerFactory;
        _dispose = dispose;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static ScopedLoggerFactory Create(
        IServiceProvider internalServiceProvider,
        IDbContextOptions? contextOptions)
    {
        var coreOptions
            = (contextOptions ?? internalServiceProvider.GetService<IDbContextOptions>())
            ?.FindExtension<CoreOptionsExtension>();

        if (coreOptions != null)
        {
            if (coreOptions.LoggerFactory != null)
            {
                return new ScopedLoggerFactory(coreOptions.LoggerFactory, dispose: false);
            }

            var applicationServiceProvider = coreOptions.ApplicationServiceProvider;
            if (applicationServiceProvider != null
                && applicationServiceProvider != internalServiceProvider)
            {
                var loggerFactory = applicationServiceProvider.GetService<ILoggerFactory>();
                if (loggerFactory != null)
                {
                    return new ScopedLoggerFactory(loggerFactory, dispose: false);
                }
            }
        }

        return new ScopedLoggerFactory(new LoggerFactory(), dispose: true);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual void Dispose()
    {
        if (_dispose)
        {
            _underlyingFactory.Dispose();
        }
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual ILogger CreateLogger(string categoryName)
        => _underlyingFactory.CreateLogger(categoryName);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual void AddProvider(ILoggerProvider provider)
        => _underlyingFactory.AddProvider(provider);
}
