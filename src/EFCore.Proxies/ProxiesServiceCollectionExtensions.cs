// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Infrastructure;
using Microsoft.EntityFrameworkCore.Proxies.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    ///     EntityFrameworkCore.Proxies extension methods for <see cref="IServiceCollection" />.
    /// </summary>
    public static class ProxiesServiceCollectionExtensions
    {
        /// <summary>
        ///     Adds the services required for proxy support in Entity Framework.
        /// </summary>
        /// <remarks>
        ///     Calling this method is no longer necessary when building most applications, including those that
        ///     use dependency injection in ASP.NET or elsewhere.
        ///     It is only needed when building the internal service provider for use with
        ///     the <see cref="DbContextOptionsBuilder.UseInternalServiceProvider" /> method.
        ///     This is not recommend other than for some advanced scenarios.
        /// </remarks>
        /// <param name="serviceCollection">The <see cref="IServiceCollection" /> to add services to.</param>
        /// <returns>
        ///     The same service collection so that multiple calls can be chained.
        /// </returns>
        public static IServiceCollection AddEntityFrameworkProxies(
            this IServiceCollection serviceCollection)
        {
            Check.NotNull(serviceCollection, nameof(serviceCollection));

            new EntityFrameworkServicesBuilder(serviceCollection)
                .TryAdd<IConventionSetPlugin, ProxiesConventionSetPlugin>()
                .TryAddProviderSpecificServices(
                    b => b.TryAddSingleton<IProxyFactory, ProxyFactory>());

            return serviceCollection;
        }
    }
}
