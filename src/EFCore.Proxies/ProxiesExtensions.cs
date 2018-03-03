// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Proxies.Internal;
using Microsoft.EntityFrameworkCore.Utilities;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore
{
    /// <summary>
    ///     Extension methods related to use of proxies with Entity Framework Core.
    /// </summary>
    public static class ProxiesExtensions
    {
        /// <summary>
        ///     <para>
        ///         Turns on the creation of lazy-loading proxies.
        ///     </para>
        ///     <para>
        ///         Note that this requires appropriate services to be available in the EF internal service provider. Normally this
        ///         will happen automatically, but if the application is controlling the service provider, then a call to
        ///         <see cref="ProxiesServiceCollectionExtensions.AddEntityFrameworkProxies" /> may be needed.
        ///     </para>
        /// </summary>
        /// <param name="optionsBuilder">
        ///     The options builder, as passed to <see cref="DbContext.OnConfiguring" />
        ///     or exposed AddDbContext.
        /// </param>
        /// <param name="useLazyLoadingProxies"> <c>True</c> to use lazy-loading proxies; false to prevent their use. </param>
        /// <returns> The same builder to allow method calls to be chained. </returns>
        public static DbContextOptionsBuilder UseLazyLoadingProxies(
            [NotNull] this DbContextOptionsBuilder optionsBuilder,
            bool useLazyLoadingProxies = true)
        {
            Check.NotNull(optionsBuilder, nameof(optionsBuilder));

            var extension = optionsBuilder.Options.FindExtension<ProxiesOptionsExtension>()
                            ?? new ProxiesOptionsExtension();

            extension = extension.WithLazyLoading(useLazyLoadingProxies);

            ((IDbContextOptionsBuilderInfrastructure)optionsBuilder).AddOrUpdateExtension(extension);

            return optionsBuilder;
        }

        /// <summary>
        ///     <para>
        ///         Turns on the creation of lazy-loading proxies.
        ///     </para>
        ///     <para>
        ///         Note that this requires appropriate services to be available in the EF internal service provider. Normally this
        ///         will happen automatically, but if the application is controlling the service provider, then a call to
        ///         <see cref="ProxiesServiceCollectionExtensions.AddEntityFrameworkProxies" /> may be needed.
        ///     </para>
        /// </summary>
        /// <typeparam name="TContext"> The <see cref="DbContext" /> type. </typeparam>
        /// <param name="optionsBuilder">
        ///     The options builder, as passed to <see cref="DbContext.OnConfiguring" />
        ///     or exposed AddDbContext.
        /// </param>
        /// <param name="useLazyLoadingProxies"> <c>True</c> to use lazy-loading proxies; false to prevent their use. </param>
        /// <returns> The same builder to allow method calls to be chained. </returns>
        public static DbContextOptionsBuilder<TContext> UseLazyLoadingProxies<TContext>(
            [NotNull] this DbContextOptionsBuilder<TContext> optionsBuilder,
            bool useLazyLoadingProxies = true)
            where TContext : DbContext
            => (DbContextOptionsBuilder<TContext>)UseLazyLoadingProxies((DbContextOptionsBuilder)optionsBuilder, useLazyLoadingProxies);

        /// <summary>
        ///     Creates a proxy instance for an entity type if proxy creation has been turned on.
        /// </summary>
        /// <param name="context"> The <see cref="DbContext" />. </param>
        /// <param name="entityType"> The entity type for which a proxy is needed. </param>
        /// <param name="constructorArguments"> Arguments to pass to the entity type constructor. </param>
        /// <returns> The proxy instance. </returns>
        public static object CreateProxy(
            [NotNull] this DbContext context,
            [NotNull] Type entityType,
            [NotNull] params object[] constructorArguments)
        {
            Check.NotNull(context, nameof(context));
            Check.NotNull(entityType, nameof(entityType));
            Check.NotNull(constructorArguments, nameof(constructorArguments));

            return context.GetInfrastructure().CreateProxy(entityType, constructorArguments);
        }

        /// <summary>
        ///     Creates a proxy instance for an entity type if proxy creation has been turned on.
        /// </summary>
        /// <typeparam name="TEntity"> The entity type for which a proxy is needed. </typeparam>
        /// <param name="context"> The <see cref="DbContext" />. </param>
        /// <param name="constructorArguments"> Arguments to pass to the entity type constructor. </param>
        /// <returns> The proxy instance. </returns>
        public static TEntity CreateProxy<TEntity>(
            [NotNull] this DbContext context,
            [NotNull] params object[] constructorArguments)
            => (TEntity)context.CreateProxy(typeof(TEntity), constructorArguments);

        /// <summary>
        ///     Creates a proxy instance for an entity type if proxy creation has been turned on.
        /// </summary>
        /// <typeparam name="TEntity"> The entity type for which a proxy is needed. </typeparam>
        /// <param name="set"> The <see cref="DbSet{TEntity}" />. </param>
        /// <param name="constructorArguments"> Arguments to pass to the entity type constructor. </param>
        /// <returns> The proxy instance. </returns>
        public static TEntity CreateProxy<TEntity>(
            [NotNull] this DbSet<TEntity> set,
            [NotNull] params object[] constructorArguments)
            where TEntity : class
        {
            Check.NotNull(set, nameof(set));
            Check.NotNull(constructorArguments, nameof(constructorArguments));

            return (TEntity)set.GetInfrastructure().CreateProxy(typeof(TEntity), constructorArguments);
        }

        private static object CreateProxy(
            this IServiceProvider serviceProvider,
            Type entityType,
            params object[] constructorArguments)
        {
            var options = serviceProvider.GetService<IDbContextOptions>().FindExtension<ProxiesOptionsExtension>();

            if (options?.UseLazyLoadingProxies != true)
            {
                throw new InvalidOperationException(ProxiesStrings.ProxiesNotEnabled(entityType.ShortDisplayName()));
            }

            return serviceProvider.GetService<IProxyFactory>().Create(
                serviceProvider.GetService<ICurrentDbContext>().Context,
                entityType,
                constructorArguments);
        }
    }
}
