// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

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
        ///     <para>
        ///         Adds the services required for proxy support in Entity Framework. You use this method when
        ///         using dependency injection in your application, such as with ASP.NET. For more information
        ///         on setting up dependency injection, see http://go.microsoft.com/fwlink/?LinkId=526890.
        ///     </para>
        ///     <para>
        ///         You only need to use this functionality when you want Entity Framework to resolve the services it uses
        ///         from an external dependency injection container. If you are not using an external
        ///         dependency injection container, Entity Framework will take care of creating the services it requires.
        ///     </para>
        /// </summary>
        /// <param name="serviceCollection"> The <see cref="IServiceCollection" /> to add services to. </param>
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
