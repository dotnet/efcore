// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    ///     Extension methods for setting up Entity Framework related services in an <see cref="IServiceCollection" />.
    /// </summary>
    public static class EntityFrameworkServiceCollectionExtensions
    {
        /// <summary>
        ///     <para>
        ///         Registers the given context as a service in the <see cref="IServiceCollection" /> and ensures services that the context
        ///         uses are resolved from the <see cref="IServiceProvider" />.
        ///     </para>
        ///     <para>
        ///         You use this method when using dependency injection in your application, such as with ASP.NET.
        ///         For more information on setting up dependency injection, see http://go.microsoft.com/fwlink/?LinkId=526890.
        ///     </para>
        /// </summary>
        /// <example>
        ///     <code>
        ///         public void ConfigureServices(IServiceCollection services) 
        ///         {
        ///             var connectionString = "connection string to database";
        /// 
        ///             services.AddDbContext&lt;MyContext&gt;(options => options.UseSqlServer(connectionString)); 
        ///         }
        ///     </code>
        /// </example>
        /// <typeparam name="TContext"> The type of context to be registered. </typeparam>
        /// <param name="serviceCollection"> The <see cref="IServiceCollection" /> to add services to. </param>
        /// <param name="optionsAction">
        ///     <para>
        ///         An optional action to configure the <see cref="DbContextOptions" /> for the context. This provides an
        ///         alternative to performing configuration of the context by overriding the
        ///         <see cref="DbContext.OnConfiguring" /> method in your derived context.
        ///     </para>
        ///     <para>
        ///         If an action is supplied here, the <see cref="DbContext.OnConfiguring" /> method will still be run if it has
        ///         been overridden on the derived context. <see cref="DbContext.OnConfiguring" /> configuration will be applied
        ///         in addition to configuration performed here.
        ///     </para>
        ///     <para>
        ///         You do not need to expose a <see cref="DbContextOptions" /> constructor parameter for the options to be passed to the
        ///         context. If you choose to expose a constructor parameter, we recommend typing it as the generic
        ///         <see cref="DbContextOptions{TContext}" />. You can use the non-generic <see cref="DbContextOptions" /> but this will only
        ///         work if you have one derived context type registered in your <see cref="IServiceProvider" />.
        ///     </para>
        /// </param>
        /// <param name="contextLifetime"> The lifetime with which to register the DbContext service in the container. </param>
        /// <returns>
        ///     A builder that allows further Entity Framework specific setup of the <see cref="IServiceCollection" />.
        /// </returns>
        public static IServiceCollection AddDbContext<TContext>(
            [NotNull] this IServiceCollection serviceCollection,
            [CanBeNull] Action<DbContextOptionsBuilder> optionsAction = null,
            ServiceLifetime contextLifetime = ServiceLifetime.Scoped)
            where TContext : DbContext
            => AddDbContext<TContext>(serviceCollection, (p, b) => optionsAction?.Invoke(b), contextLifetime);

        public static IServiceCollection AddDbContext<TContext>(
            [NotNull] this IServiceCollection serviceCollection,
            ServiceLifetime contextLifetime)
            where TContext : DbContext
            => AddDbContext<TContext>(serviceCollection, (Action<IServiceProvider, DbContextOptionsBuilder>)null, contextLifetime);

        public static IServiceCollection AddDbContext<TContext>(
            [NotNull] this IServiceCollection serviceCollection,
            [CanBeNull] Action<IServiceProvider, DbContextOptionsBuilder> optionsAction,
            ServiceLifetime contextLifetime = ServiceLifetime.Scoped)
            where TContext : DbContext
        {
            serviceCollection
                .AddMemoryCache()
                .AddLogging();

            serviceCollection.TryAddSingleton(p => DbContextOptionsFactory<TContext>(p, optionsAction));
            serviceCollection.AddSingleton<DbContextOptions>(p => p.GetRequiredService<DbContextOptions<TContext>>());

            serviceCollection.TryAdd(new ServiceDescriptor(typeof(TContext), typeof(TContext), contextLifetime));

            return serviceCollection;
        }

        private static DbContextOptions<TContext> DbContextOptionsFactory<TContext>(
            [NotNull] IServiceProvider applicationServiceProvider,
            [CanBeNull] Action<IServiceProvider, DbContextOptionsBuilder> optionsAction)
            where TContext : DbContext
        {
            var builder = new DbContextOptionsBuilder<TContext>(
                new DbContextOptions<TContext>(new Dictionary<Type, IDbContextOptionsExtension>()))
                .UseMemoryCache(applicationServiceProvider.GetService<IMemoryCache>())
                .UseLoggerFactory(applicationServiceProvider.GetService<ILoggerFactory>());

            optionsAction?.Invoke(applicationServiceProvider, builder);

            return builder.Options;
        }
    }
}
