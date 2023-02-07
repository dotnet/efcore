// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

// ReSharper disable once CheckNamespace

namespace Microsoft.EntityFrameworkCore.Infrastructure.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public static class InfrastructureExtensions
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static TService GetService<TService>(IInfrastructure<IServiceProvider> accessor)
        where TService : class
        => (TService)GetService(accessor, typeof(TService));

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static object GetService(IInfrastructure<IServiceProvider> accessor, Type serviceType)
    {
        var internalServiceProvider = accessor.Instance;

        var service = internalServiceProvider.GetService(serviceType)
            ?? internalServiceProvider.GetService<IDbContextOptions>()
                ?.Extensions.OfType<CoreOptionsExtension>().FirstOrDefault()
                ?.ApplicationServiceProvider
                ?.GetService(serviceType);

        if (service == null)
        {
            throw new InvalidOperationException(
                CoreStrings.NoProviderConfiguredFailedToResolveService(serviceType.DisplayName()));
        }

        return service;
    }
}
