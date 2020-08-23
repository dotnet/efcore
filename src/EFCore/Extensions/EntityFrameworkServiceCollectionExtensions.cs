// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Utilities;
using Microsoft.Extensions.DependencyInjection.Extensions;

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
        ///         Registers the given context as a service in the <see cref="IServiceCollection" />.
        ///     </para>
        ///     <para>
        ///         Use this method when using dependency injection in your application, such as with ASP.NET Core.
        ///         For applications that don't use dependency injection, consider creating <see cref="DbContext" />
        ///         instances directly with its constructor. The <see cref="DbContext.OnConfiguring" /> method can then be
        ///         overridden to configure a connection string and other options.
        ///     </para>
        ///     <para>
        ///         For more information on how to use this method, see the Entity Framework Core documentation at https://aka.ms/efdocs.
        ///         For more information on using dependency injection, see https://go.microsoft.com/fwlink/?LinkId=526890.
        ///     </para>
        /// </summary>
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
        ///         In order for the options to be passed into your context, you need to expose a constructor on your context that takes
        ///         <see cref="DbContextOptions{TContext}" /> and passes it to the base constructor of <see cref="DbContext" />.
        ///     </para>
        /// </param>
        /// <param name="contextLifetime"> The lifetime with which to register the DbContext service in the container. </param>
        /// <param name="optionsLifetime"> The lifetime with which to register the DbContextOptions service in the container. </param>
        /// <returns>
        ///     The same service collection so that multiple calls can be chained.
        /// </returns>
        public static IServiceCollection AddDbContext<TContext>(
            [NotNull] this IServiceCollection serviceCollection,
            [CanBeNull] Action<DbContextOptionsBuilder> optionsAction = null,
            ServiceLifetime contextLifetime = ServiceLifetime.Scoped,
            ServiceLifetime optionsLifetime = ServiceLifetime.Scoped)
            where TContext : DbContext
            => AddDbContext<TContext, TContext>(serviceCollection, optionsAction, contextLifetime, optionsLifetime);

        /// <summary>
        ///     <para>
        ///         Registers the given context as a service in the <see cref="IServiceCollection" />.
        ///     </para>
        ///     <para>
        ///         Use this method when using dependency injection in your application, such as with ASP.NET Core.
        ///         For applications that don't use dependency injection, consider creating <see cref="DbContext" />
        ///         instances directly with its constructor. The <see cref="DbContext.OnConfiguring" /> method can then be
        ///         overridden to configure a connection string and other options.
        ///     </para>
        ///     <para>
        ///         For more information on how to use this method, see the Entity Framework Core documentation at https://aka.ms/efdocs.
        ///         For more information on using dependency injection, see https://go.microsoft.com/fwlink/?LinkId=526890.
        ///     </para>
        /// </summary>
        /// <typeparam name="TContextService"> The class or interface that will be used to resolve the context from the container. </typeparam>
        /// <typeparam name="TContextImplementation"> The concrete implementation type to create. </typeparam>
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
        ///         In order for the options to be passed into your context, you need to expose a constructor on your context that takes
        ///         <see cref="DbContextOptions{TContext}" /> and passes it to the base constructor of <see cref="DbContext" />.
        ///     </para>
        /// </param>
        /// <param name="contextLifetime"> The lifetime with which to register the DbContext service in the container. </param>
        /// <param name="optionsLifetime"> The lifetime with which to register the DbContextOptions service in the container. </param>
        /// <returns>
        ///     The same service collection so that multiple calls can be chained.
        /// </returns>
        public static IServiceCollection AddDbContext<TContextService, TContextImplementation>(
            [NotNull] this IServiceCollection serviceCollection,
            [CanBeNull] Action<DbContextOptionsBuilder> optionsAction = null,
            ServiceLifetime contextLifetime = ServiceLifetime.Scoped,
            ServiceLifetime optionsLifetime = ServiceLifetime.Scoped)
            where TContextImplementation : DbContext, TContextService
            => AddDbContext<TContextService, TContextImplementation>(
                serviceCollection,
                optionsAction == null
                    ? (Action<IServiceProvider, DbContextOptionsBuilder>)null
                    : (p, b) => optionsAction(b), contextLifetime, optionsLifetime);

        /// <summary>
        ///     <para>
        ///         Registers the given <see cref="DbContext" /> as a service in the <see cref="IServiceCollection" />,
        ///         and enables DbContext pooling for this registration.
        ///     </para>
        ///     <para>
        ///         DbContext pooling can increase performance in high-throughput scenarios by re-using context instances.
        ///         However, for most application this performance gain is very small.
        ///         Note that when using pooling, the context configuration cannot change between uses, and scoped services
        ///         injected into the context will only be resolved once from the initial scope.
        ///         Only consider using DbContext pooling when performance testing indicates it provides a real boost.
        ///     </para>
        ///     <para>
        ///         Use this method when using dependency injection in your application, such as with ASP.NET Core.
        ///         For applications that don't use dependency injection, consider creating <see cref="DbContext" />
        ///         instances directly with its constructor. The <see cref="DbContext.OnConfiguring" /> method can then be
        ///         overridden to configure a connection string and other options.
        ///     </para>
        ///     <para>
        ///         For more information on how to use this method, see the Entity Framework Core documentation at https://aka.ms/efdocs.
        ///         For more information on using dependency injection, see https://go.microsoft.com/fwlink/?LinkId=526890.
        ///     </para>
        /// </summary>
        /// <typeparam name="TContext"> The type of context to be registered. </typeparam>
        /// <param name="serviceCollection"> The <see cref="IServiceCollection" /> to add services to. </param>
        /// <param name="optionsAction">
        ///     <para>
        ///         A required action to configure the <see cref="DbContextOptions" /> for the context. When using
        ///         context pooling, options configuration must be performed externally; <see cref="DbContext.OnConfiguring" />
        ///         will not be called.
        ///     </para>
        /// </param>
        /// <param name="poolSize">
        ///     Sets the maximum number of instances retained by the pool.
        /// </param>
        /// <returns>
        ///     The same service collection so that multiple calls can be chained.
        /// </returns>
        public static IServiceCollection AddDbContextPool<TContext>(
            [NotNull] this IServiceCollection serviceCollection,
            [NotNull] Action<DbContextOptionsBuilder> optionsAction,
            int poolSize = 128)
            where TContext : DbContext
            => AddDbContextPool<TContext, TContext>(serviceCollection, optionsAction, poolSize);

        /// <summary>
        ///     <para>
        ///         Registers the given <see cref="DbContext" /> as a service in the <see cref="IServiceCollection" />,
        ///         and enables DbContext pooling for this registration.
        ///     </para>
        ///     <para>
        ///         DbContext pooling can increase performance in high-throughput scenarios by re-using context instances.
        ///         However, for most application this performance gain is very small.
        ///         Note that when using pooling, the context configuration cannot change between uses, and scoped services
        ///         injected into the context will only be resolved once from the initial scope.
        ///         Only consider using DbContext pooling when performance testing indicates it provides a real boost.
        ///     </para>
        ///     <para>
        ///         Use this method when using dependency injection in your application, such as with ASP.NET Core.
        ///         For applications that don't use dependency injection, consider creating <see cref="DbContext" />
        ///         instances directly with its constructor. The <see cref="DbContext.OnConfiguring" /> method can then be
        ///         overridden to configure a connection string and other options.
        ///     </para>
        ///     <para>
        ///         For more information on how to use this method, see the Entity Framework Core documentation at https://aka.ms/efdocs.
        ///         For more information on using dependency injection, see https://go.microsoft.com/fwlink/?LinkId=526890.
        ///     </para>
        /// </summary>
        /// <typeparam name="TContextService"> The class or interface that will be used to resolve the context from the container. </typeparam>
        /// <typeparam name="TContextImplementation"> The concrete implementation type to create. </typeparam>
        /// <param name="serviceCollection"> The <see cref="IServiceCollection" /> to add services to. </param>
        /// <param name="optionsAction">
        ///     <para>
        ///         A required action to configure the <see cref="DbContextOptions" /> for the context. When using
        ///         context pooling, options configuration must be performed externally; <see cref="DbContext.OnConfiguring" />
        ///         will not be called.
        ///     </para>
        /// </param>
        /// <param name="poolSize">
        ///     Sets the maximum number of instances retained by the pool.
        /// </param>
        /// <returns>
        ///     The same service collection so that multiple calls can be chained.
        /// </returns>
        public static IServiceCollection AddDbContextPool<TContextService, TContextImplementation>(
            [NotNull] this IServiceCollection serviceCollection,
            [NotNull] Action<DbContextOptionsBuilder> optionsAction,
            int poolSize = 128)
            where TContextImplementation : DbContext, TContextService
            where TContextService : class
        {
            Check.NotNull(optionsAction, nameof(optionsAction));

            return AddDbContextPool<TContextService, TContextImplementation>(serviceCollection, (_, ob) => optionsAction(ob), poolSize);
        }

        /// <summary>
        ///     <para>
        ///         Registers the given <see cref="DbContext" /> as a service in the <see cref="IServiceCollection" />,
        ///         and enables DbContext pooling for this registration.
        ///     </para>
        ///     <para>
        ///         DbContext pooling can increase performance in high-throughput scenarios by re-using context instances.
        ///         However, for most application this performance gain is very small.
        ///         Note that when using pooling, the context configuration cannot change between uses, and scoped services
        ///         injected into the context will only be resolved once from the initial scope.
        ///         Only consider using DbContext pooling when performance testing indicates it provides a real boost.
        ///     </para>
        ///     <para>
        ///         Use this method when using dependency injection in your application, such as with ASP.NET Core.
        ///         For applications that don't use dependency injection, consider creating <see cref="DbContext" />
        ///         instances directly with its constructor. The <see cref="DbContext.OnConfiguring" /> method can then be
        ///         overridden to configure a connection string and other options.
        ///     </para>
        ///     <para>
        ///         For more information on how to use this method, see the Entity Framework Core documentation at https://aka.ms/efdocs.
        ///         For more information on using dependency injection, see https://go.microsoft.com/fwlink/?LinkId=526890.
        ///     </para>
        ///     <para>
        ///         This overload has an <paramref name="optionsAction" /> that provides the application's
        ///         <see cref="IServiceProvider" />. This is useful if you want to setup Entity Framework Core to resolve
        ///         its internal services from the primary application service provider.
        ///         By default, we recommend using
        ///         <see cref="AddDbContextPool{TContext}(IServiceCollection,Action{DbContextOptionsBuilder},int)" /> which allows
        ///         Entity Framework to create and maintain its own <see cref="IServiceProvider" /> for internal Entity Framework services.
        ///     </para>
        /// </summary>
        /// <typeparam name="TContext"> The type of context to be registered. </typeparam>
        /// <param name="serviceCollection"> The <see cref="IServiceCollection" /> to add services to. </param>
        /// <param name="optionsAction">
        ///     <para>
        ///         A required action to configure the <see cref="DbContextOptions" /> for the context. When using
        ///         context pooling, options configuration must be performed externally; <see cref="DbContext.OnConfiguring" />
        ///         will not be called.
        ///     </para>
        /// </param>
        /// <param name="poolSize">
        ///     Sets the maximum number of instances retained by the pool.
        /// </param>
        /// <returns>
        ///     The same service collection so that multiple calls can be chained.
        /// </returns>
        public static IServiceCollection AddDbContextPool<TContext>(
            [NotNull] this IServiceCollection serviceCollection,
            [NotNull] Action<IServiceProvider, DbContextOptionsBuilder> optionsAction,
            int poolSize = 128)
            where TContext : DbContext
            => AddDbContextPool<TContext, TContext>(serviceCollection, optionsAction, poolSize);

        /// <summary>
        ///     <para>
        ///         Registers the given <see cref="DbContext" /> as a service in the <see cref="IServiceCollection" />,
        ///         and enables DbContext pooling for this registration.
        ///     </para>
        ///     <para>
        ///         DbContext pooling can increase performance in high-throughput scenarios by re-using context instances.
        ///         However, for most application this performance gain is very small.
        ///         Note that when using pooling, the context configuration cannot change between uses, and scoped services
        ///         injected into the context will only be resolved once from the initial scope.
        ///         Only consider using DbContext pooling when performance testing indicates it provides a real boost.
        ///     </para>
        ///     <para>
        ///         Use this method when using dependency injection in your application, such as with ASP.NET Core.
        ///         For applications that don't use dependency injection, consider creating <see cref="DbContext" />
        ///         instances directly with its constructor. The <see cref="DbContext.OnConfiguring" /> method can then be
        ///         overridden to configure a connection string and other options.
        ///     </para>
        ///     <para>
        ///         For more information on how to use this method, see the Entity Framework Core documentation at https://aka.ms/efdocs.
        ///         For more information on using dependency injection, see https://go.microsoft.com/fwlink/?LinkId=526890.
        ///     </para>
        ///     <para>
        ///         This overload has an <paramref name="optionsAction" /> that provides the application's
        ///         <see cref="IServiceProvider" />. This is useful if you want to setup Entity Framework Core to resolve
        ///         its internal services from the primary application service provider.
        ///         By default, we recommend using
        ///         <see cref="AddDbContextPool{TContext,TContextImplementation}(IServiceCollection,Action{DbContextOptionsBuilder},int)" />
        ///         which allows Entity Framework to create and maintain its own <see cref="IServiceProvider" /> for internal
        ///         Entity Framework services.
        ///     </para>
        /// </summary>
        /// <typeparam name="TContextService"> The class or interface that will be used to resolve the context from the container. </typeparam>
        /// <typeparam name="TContextImplementation"> The concrete implementation type to create. </typeparam>
        /// <param name="serviceCollection"> The <see cref="IServiceCollection" /> to add services to. </param>
        /// <param name="optionsAction">
        ///     <para>
        ///         A required action to configure the <see cref="DbContextOptions" /> for the context. When using
        ///         context pooling, options configuration must be performed externally; <see cref="DbContext.OnConfiguring" />
        ///         will not be called.
        ///     </para>
        /// </param>
        /// <param name="poolSize">
        ///     Sets the maximum number of instances retained by the pool.
        /// </param>
        /// <returns>
        ///     The same service collection so that multiple calls can be chained.
        /// </returns>
        public static IServiceCollection AddDbContextPool<TContextService, TContextImplementation>(
            [NotNull] this IServiceCollection serviceCollection,
            [NotNull] Action<IServiceProvider, DbContextOptionsBuilder> optionsAction,
            int poolSize = 128)
            where TContextImplementation : DbContext, TContextService
            where TContextService : class
        {
            Check.NotNull(serviceCollection, nameof(serviceCollection));
            Check.NotNull(optionsAction, nameof(optionsAction));

            AddPoolingOptions<TContextImplementation>(serviceCollection, optionsAction, poolSize);

            serviceCollection.TryAddSingleton<IDbContextPool<TContextImplementation>, DbContextPool<TContextImplementation>>();
            serviceCollection.AddScoped<IScopedDbContextLease<TContextImplementation>, ScopedDbContextLease<TContextImplementation>>();

            serviceCollection.AddScoped<TContextService>(
                sp => sp.GetRequiredService<IScopedDbContextLease<TContextImplementation>>().Context);

            return serviceCollection;
        }

        private static void AddPoolingOptions<TContext>(
            IServiceCollection serviceCollection,
            Action<IServiceProvider, DbContextOptionsBuilder> optionsAction,
            int poolSize)
            where TContext : DbContext
        {
            if (poolSize <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(poolSize), CoreStrings.InvalidPoolSize);
            }

            CheckContextConstructors<TContext>();

            AddCoreServices<TContext>(
                serviceCollection,
                (sp, ob) =>
                {
                    optionsAction(sp, ob);

                    var extension = (ob.Options.FindExtension<CoreOptionsExtension>() ?? new CoreOptionsExtension())
                        .WithMaxPoolSize(poolSize);

                    ((IDbContextOptionsBuilderInfrastructure)ob).AddOrUpdateExtension(extension);
                },
                ServiceLifetime.Singleton);
        }

        /// <summary>
        ///     <para>
        ///         Registers the given context as a service in the <see cref="IServiceCollection" />.
        ///     </para>
        ///     <para>
        ///         Use this method when using dependency injection in your application, such as with ASP.NET Core.
        ///         For applications that don't use dependency injection, consider creating <see cref="DbContext" />
        ///         instances directly with its constructor. The <see cref="DbContext.OnConfiguring" /> method can then be
        ///         overridden to configure a connection string and other options.
        ///     </para>
        ///     <para>
        ///         For more information on how to use this method, see the Entity Framework Core documentation at https://aka.ms/efdocs.
        ///         For more information on using dependency injection, see https://go.microsoft.com/fwlink/?LinkId=526890.
        ///     </para>
        /// </summary>
        /// <typeparam name="TContext"> The type of context to be registered. </typeparam>
        /// <param name="serviceCollection"> The <see cref="IServiceCollection" /> to add services to. </param>
        /// <param name="contextLifetime"> The lifetime with which to register the DbContext service in the container. </param>
        /// <param name="optionsLifetime"> The lifetime with which to register the DbContextOptions service in the container. </param>
        /// <returns>
        ///     The same service collection so that multiple calls can be chained.
        /// </returns>
        public static IServiceCollection AddDbContext<TContext>(
            [NotNull] this IServiceCollection serviceCollection,
            ServiceLifetime contextLifetime,
            ServiceLifetime optionsLifetime = ServiceLifetime.Scoped)
            where TContext : DbContext
            => AddDbContext<TContext, TContext>(serviceCollection, contextLifetime, optionsLifetime);

        /// <summary>
        ///     <para>
        ///         Registers the given context as a service in the <see cref="IServiceCollection" />.
        ///     </para>
        ///     <para>
        ///         Use this method when using dependency injection in your application, such as with ASP.NET Core.
        ///         For applications that don't use dependency injection, consider creating <see cref="DbContext" />
        ///         instances directly with its constructor. The <see cref="DbContext.OnConfiguring" /> method can then be
        ///         overridden to configure a connection string and other options.
        ///     </para>
        ///     <para>
        ///         For more information on how to use this method, see the Entity Framework Core documentation at https://aka.ms/efdocs.
        ///         For more information on using dependency injection, see https://go.microsoft.com/fwlink/?LinkId=526890.
        ///     </para>
        /// </summary>
        /// <typeparam name="TContextService"> The class or interface that will be used to resolve the context from the container. </typeparam>
        /// <typeparam name="TContextImplementation"> The concrete implementation type to create. </typeparam>
        /// <param name="serviceCollection"> The <see cref="IServiceCollection" /> to add services to. </param>
        /// <param name="contextLifetime"> The lifetime with which to register the DbContext service in the container. </param>
        /// <param name="optionsLifetime"> The lifetime with which to register the DbContextOptions service in the container. </param>
        /// <returns>
        ///     The same service collection so that multiple calls can be chained.
        /// </returns>
        public static IServiceCollection AddDbContext<TContextService, TContextImplementation>(
            [NotNull] this IServiceCollection serviceCollection,
            ServiceLifetime contextLifetime,
            ServiceLifetime optionsLifetime = ServiceLifetime.Scoped)
            where TContextImplementation : DbContext, TContextService
            where TContextService : class
            => AddDbContext<TContextService, TContextImplementation>(
                serviceCollection,
                (Action<IServiceProvider, DbContextOptionsBuilder>)null,
                contextLifetime,
                optionsLifetime);

        /// <summary>
        ///     <para>
        ///         Registers the given context as a service in the <see cref="IServiceCollection" />.
        ///     </para>
        ///     <para>
        ///         Use this method when using dependency injection in your application, such as with ASP.NET Core.
        ///         For applications that don't use dependency injection, consider creating <see cref="DbContext" />
        ///         instances directly with its constructor. The <see cref="DbContext.OnConfiguring" /> method can then be
        ///         overridden to configure a connection string and other options.
        ///     </para>
        ///     <para>
        ///         For more information on how to use this method, see the Entity Framework Core documentation at https://aka.ms/efdocs.
        ///         For more information on using dependency injection, see https://go.microsoft.com/fwlink/?LinkId=526890.
        ///     </para>
        ///     <para>
        ///         This overload has an <paramref name="optionsAction" /> that provides the application's
        ///         <see cref="IServiceProvider" />. This is useful if you want to setup Entity Framework Core to resolve
        ///         its internal services from the primary application service provider.
        ///         By default, we recommend using
        ///         <see cref="AddDbContext{TContext}(IServiceCollection,Action{DbContextOptionsBuilder},ServiceLifetime,ServiceLifetime)" />
        ///         which allows Entity Framework to create and maintain its own <see cref="IServiceProvider" /> for internal
        ///         Entity Framework services.
        ///     </para>
        /// </summary>
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
        ///         In order for the options to be passed into your context, you need to expose a constructor on your context that takes
        ///         <see cref="DbContextOptions{TContext}" /> and passes it to the base constructor of <see cref="DbContext" />.
        ///     </para>
        /// </param>
        /// <param name="contextLifetime"> The lifetime with which to register the DbContext service in the container. </param>
        /// <param name="optionsLifetime"> The lifetime with which to register the DbContextOptions service in the container. </param>
        /// <returns>
        ///     The same service collection so that multiple calls can be chained.
        /// </returns>
        public static IServiceCollection AddDbContext<TContext>(
            [NotNull] this IServiceCollection serviceCollection,
            [CanBeNull] Action<IServiceProvider, DbContextOptionsBuilder> optionsAction,
            ServiceLifetime contextLifetime = ServiceLifetime.Scoped,
            ServiceLifetime optionsLifetime = ServiceLifetime.Scoped)
            where TContext : DbContext
            => AddDbContext<TContext, TContext>(serviceCollection, optionsAction, contextLifetime, optionsLifetime);

        /// <summary>
        ///     <para>
        ///         Registers the given context as a service in the <see cref="IServiceCollection" />.
        ///     </para>
        ///     <para>
        ///         Use this method when using dependency injection in your application, such as with ASP.NET Core.
        ///         For applications that don't use dependency injection, consider creating <see cref="DbContext" />
        ///         instances directly with its constructor. The <see cref="DbContext.OnConfiguring" /> method can then be
        ///         overridden to configure a connection string and other options.
        ///     </para>
        ///     <para>
        ///         For more information on how to use this method, see the Entity Framework Core documentation at https://aka.ms/efdocs.
        ///         For more information on using dependency injection, see https://go.microsoft.com/fwlink/?LinkId=526890.
        ///     </para>
        ///     <para>
        ///         This overload has an <paramref name="optionsAction" /> that provides the application's
        ///         <see cref="IServiceProvider" />. This is useful if you want to setup Entity Framework Core to resolve
        ///         its internal services from the primary application service provider.
        ///         By default, we recommend using
        ///         <see
        ///             cref="AddDbContext{TContext,TContextImplementation}(IServiceCollection,Action{DbContextOptionsBuilder},ServiceLifetime,ServiceLifetime)" />
        ///         which allows Entity Framework to create and maintain its own <see cref="IServiceProvider" /> for internal
        ///         Entity Framework services.
        ///     </para>
        /// </summary>
        /// <typeparam name="TContextService"> The class or interface that will be used to resolve the context from the container. </typeparam>
        /// <typeparam name="TContextImplementation"> The concrete implementation type to create. </typeparam>
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
        ///         In order for the options to be passed into your context, you need to expose a constructor on your context that takes
        ///         <see cref="DbContextOptions{TContext}" /> and passes it to the base constructor of <see cref="DbContext" />.
        ///     </para>
        /// </param>
        /// <param name="contextLifetime"> The lifetime with which to register the DbContext service in the container. </param>
        /// <param name="optionsLifetime"> The lifetime with which to register the DbContextOptions service in the container. </param>
        /// <returns>
        ///     The same service collection so that multiple calls can be chained.
        /// </returns>
        public static IServiceCollection AddDbContext<TContextService, TContextImplementation>(
            [NotNull] this IServiceCollection serviceCollection,
            [CanBeNull] Action<IServiceProvider, DbContextOptionsBuilder> optionsAction,
            ServiceLifetime contextLifetime = ServiceLifetime.Scoped,
            ServiceLifetime optionsLifetime = ServiceLifetime.Scoped)
            where TContextImplementation : DbContext, TContextService
        {
            Check.NotNull(serviceCollection, nameof(serviceCollection));

            if (contextLifetime == ServiceLifetime.Singleton)
            {
                optionsLifetime = ServiceLifetime.Singleton;
            }

            if (optionsAction != null)
            {
                CheckContextConstructors<TContextImplementation>();
            }

            AddCoreServices<TContextImplementation>(serviceCollection, optionsAction, optionsLifetime);

            serviceCollection.TryAdd(new ServiceDescriptor(typeof(TContextService), typeof(TContextImplementation), contextLifetime));

            return serviceCollection;
        }

        /// <summary>
        ///     <para>
        ///         Registers an <see cref="IDbContextFactory{TContext}" /> in the <see cref="IServiceCollection" /> to create instances
        ///         of given <see cref="DbContext" /> type.
        ///     </para>
        ///     <para>
        ///         Registering a factory instead of registering the context type directly allows for easy creation of new
        ///         <see cref="DbContext" /> instances.
        ///         Registering a factory is recommended for Blazor applications and other situations where the dependency
        ///         injection scope is not aligned with the context lifetime.
        ///     </para>
        ///     <para>
        ///         Use this method when using dependency injection in your application, such as with Blazor.
        ///         For applications that don't use dependency injection, consider creating <see cref="DbContext" />
        ///         instances directly with its constructor. The <see cref="DbContext.OnConfiguring" /> method can then be
        ///         overridden to configure a connection string and other options.
        ///     </para>
        ///     <para>
        ///         For more information on how to use this method, see the Entity Framework Core documentation at https://aka.ms/efdocs.
        ///         For more information on using dependency injection, see https://go.microsoft.com/fwlink/?LinkId=526890.
        ///     </para>
        /// </summary>
        /// <typeparam name="TContext"> The type of <see cref="DbContext" /> to be created by the factory. </typeparam>
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
        ///         In order for the options to be passed into your context, you need to expose a constructor on your context that takes
        ///         <see cref="DbContextOptions{TContext}" /> and passes it to the base constructor of <see cref="DbContext" />.
        ///     </para>
        /// </param>
        /// <param name="lifetime">
        ///     The lifetime with which to register the factory and options.
        ///     The default is <see cref="ServiceLifetime.Singleton" />
        /// </param>
        /// <returns>
        ///     The same service collection so that multiple calls can be chained.
        /// </returns>
        public static IServiceCollection AddDbContextFactory<TContext>(
            [NotNull] this IServiceCollection serviceCollection,
            [CanBeNull] Action<DbContextOptionsBuilder> optionsAction = null,
            ServiceLifetime lifetime = ServiceLifetime.Singleton)
            where TContext : DbContext
            => AddDbContextFactory<TContext, DbContextFactory<TContext>>(serviceCollection, optionsAction, lifetime);

        /// <summary>
        ///     <para>
        ///         Registers an <see cref="IDbContextFactory{TContext}" /> in the <see cref="IServiceCollection" /> to create instances
        ///         of given <see cref="DbContext" /> type.
        ///     </para>
        ///     <para>
        ///         Registering a factory instead of registering the context type directly allows for easy creation of new
        ///         <see cref="DbContext" /> instances.
        ///         Registering a factory is recommended for Blazor applications and other situations where the dependency
        ///         injection scope is not aligned with the context lifetime.
        ///     </para>
        ///     <para>
        ///         Use this method when using dependency injection in your application, such as with Blazor.
        ///         For applications that don't use dependency injection, consider creating <see cref="DbContext" />
        ///         instances directly with its constructor. The <see cref="DbContext.OnConfiguring" /> method can then be
        ///         overridden to configure a connection string and other options.
        ///     </para>
        ///     <para>
        ///         This overload allows a specific implementation of <see cref="IDbContextFactory{TContext}" /> to be registered
        ///         instead of using the default factory shipped with EF Core.
        ///     </para>
        ///     <para>
        ///         For more information on how to use this method, see the Entity Framework Core documentation at https://aka.ms/efdocs.
        ///         For more information on using dependency injection, see https://go.microsoft.com/fwlink/?LinkId=526890.
        ///     </para>
        /// </summary>
        /// <typeparam name="TContext"> The type of <see cref="DbContext" /> to be created by the factory. </typeparam>
        /// <typeparam name="TFactory"> The type of <see cref="IDbContextFactory{TContext}" /> to register. </typeparam>
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
        ///         In order for the options to be passed into your context, you need to expose a constructor on your context that takes
        ///         <see cref="DbContextOptions{TContext}" /> and passes it to the base constructor of <see cref="DbContext" />.
        ///     </para>
        /// </param>
        /// <param name="lifetime">
        ///     The lifetime with which to register the factory and options.
        ///     The default is <see cref="ServiceLifetime.Singleton" />
        /// </param>
        /// <returns>
        ///     The same service collection so that multiple calls can be chained.
        /// </returns>
        public static IServiceCollection AddDbContextFactory<TContext, TFactory>(
            [NotNull] this IServiceCollection serviceCollection,
            [CanBeNull] Action<DbContextOptionsBuilder> optionsAction = null,
            ServiceLifetime lifetime = ServiceLifetime.Singleton)
            where TContext : DbContext
            where TFactory : IDbContextFactory<TContext>
            => AddDbContextFactory<TContext, TFactory>(
                serviceCollection,
                optionsAction == null
                    ? (Action<IServiceProvider, DbContextOptionsBuilder>)null
                    : (p, b) => optionsAction(b),
                lifetime);

        /// <summary>
        ///     <para>
        ///         Registers an <see cref="IDbContextFactory{TContext}" /> in the <see cref="IServiceCollection" /> to create instances
        ///         of given <see cref="DbContext" /> type.
        ///     </para>
        ///     <para>
        ///         Registering a factory instead of registering the context type directly allows for easy creation of new
        ///         <see cref="DbContext" /> instances.
        ///         Registering a factory is recommended for Blazor applications and other situations where the dependency
        ///         injection scope is not aligned with the context lifetime.
        ///     </para>
        ///     <para>
        ///         Use this method when using dependency injection in your application, such as with Blazor.
        ///         For applications that don't use dependency injection, consider creating <see cref="DbContext" />
        ///         instances directly with its constructor. The <see cref="DbContext.OnConfiguring" /> method can then be
        ///         overridden to configure a connection string and other options.
        ///     </para>
        ///     <para>
        ///         This overload has an <paramref name="optionsAction" /> that provides the application's
        ///         <see cref="IServiceProvider" />. This is useful if you want to setup Entity Framework Core to resolve
        ///         its internal services from the primary application service provider.
        ///         By default, we recommend using
        ///         <see cref="AddDbContextFactory{TContext}(IServiceCollection,Action{DbContextOptionsBuilder},ServiceLifetime)" /> which allows
        ///         Entity Framework to create and maintain its own <see cref="IServiceProvider" /> for internal Entity Framework services.
        ///     </para>
        ///     <para>
        ///         For more information on how to use this method, see the Entity Framework Core documentation at https://aka.ms/efdocs.
        ///         For more information on using dependency injection, see https://go.microsoft.com/fwlink/?LinkId=526890.
        ///     </para>
        /// </summary>
        /// <typeparam name="TContext"> The type of <see cref="DbContext" /> to be created by the factory. </typeparam>
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
        ///         In order for the options to be passed into your context, you need to expose a constructor on your context that takes
        ///         <see cref="DbContextOptions{TContext}" /> and passes it to the base constructor of <see cref="DbContext" />.
        ///     </para>
        /// </param>
        /// <param name="lifetime">
        ///     The lifetime with which to register the factory and options.
        ///     The default is <see cref="ServiceLifetime.Singleton" />
        /// </param>
        /// <returns>
        ///     The same service collection so that multiple calls can be chained.
        /// </returns>
        public static IServiceCollection AddDbContextFactory<TContext>(
            [NotNull] this IServiceCollection serviceCollection,
            [CanBeNull] Action<IServiceProvider, DbContextOptionsBuilder> optionsAction,
            ServiceLifetime lifetime = ServiceLifetime.Singleton)
            where TContext : DbContext
            => AddDbContextFactory<TContext, DbContextFactory<TContext>>(serviceCollection, optionsAction, lifetime);

        /// <summary>
        ///     <para>
        ///         Registers an <see cref="IDbContextFactory{TContext}" /> in the <see cref="IServiceCollection" /> to create instances
        ///         of given <see cref="DbContext" /> type.
        ///     </para>
        ///     <para>
        ///         Registering a factory instead of registering the context type directly allows for easy creation of new
        ///         <see cref="DbContext" /> instances.
        ///         Registering a factory is recommended for Blazor applications and other situations where the dependency
        ///         injection scope is not aligned with the context lifetime.
        ///     </para>
        ///     <para>
        ///         Use this method when using dependency injection in your application, such as with Blazor.
        ///         For applications that don't use dependency injection, consider creating <see cref="DbContext" />
        ///         instances directly with its constructor. The <see cref="DbContext.OnConfiguring" /> method can then be
        ///         overridden to configure a connection string and other options.
        ///     </para>
        ///     <para>
        ///         This overload allows a specific implementation of <see cref="IDbContextFactory{TContext}" /> to be registered
        ///         instead of using the default factory shipped with EF Core.
        ///     </para>
        ///     <para>
        ///         This overload has an <paramref name="optionsAction" /> that provides the application's
        ///         <see cref="IServiceProvider" />. This is useful if you want to setup Entity Framework Core to resolve
        ///         its internal services from the primary application service provider.
        ///         By default, we recommend using
        ///         <see cref="AddDbContextFactory{TContext}(IServiceCollection,Action{DbContextOptionsBuilder},ServiceLifetime)" /> which allows
        ///         Entity Framework to create and maintain its own <see cref="IServiceProvider" /> for internal Entity Framework services.
        ///     </para>
        ///     <para>
        ///         For more information on how to use this method, see the Entity Framework Core documentation at https://aka.ms/efdocs.
        ///         For more information on using dependency injection, see https://go.microsoft.com/fwlink/?LinkId=526890.
        ///     </para>
        /// </summary>
        /// <typeparam name="TContext"> The type of <see cref="DbContext" /> to be created by the factory. </typeparam>
        /// <typeparam name="TFactory"> The type of <see cref="IDbContextFactory{TContext}" /> to register. </typeparam>
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
        ///         In order for the options to be passed into your context, you need to expose a constructor on your context that takes
        ///         <see cref="DbContextOptions{TContext}" /> and passes it to the base constructor of <see cref="DbContext" />.
        ///     </para>
        /// </param>
        /// <param name="lifetime">
        ///     The lifetime with which to register the factory and options.
        ///     The default is <see cref="ServiceLifetime.Singleton" />
        /// </param>
        /// <returns>
        ///     The same service collection so that multiple calls can be chained.
        /// </returns>
        public static IServiceCollection AddDbContextFactory<TContext, TFactory>(
            [NotNull] this IServiceCollection serviceCollection,
            [CanBeNull] Action<IServiceProvider, DbContextOptionsBuilder> optionsAction,
            ServiceLifetime lifetime = ServiceLifetime.Singleton)
            where TContext : DbContext
            where TFactory : IDbContextFactory<TContext>
        {
            Check.NotNull(serviceCollection, nameof(serviceCollection));

            AddCoreServices<TContext>(serviceCollection, optionsAction, lifetime);

            serviceCollection.AddSingleton<IDbContextFactorySource<TContext>, DbContextFactorySource<TContext>>();

            serviceCollection.TryAdd(
                new ServiceDescriptor(
                    typeof(IDbContextFactory<TContext>),
                    typeof(TFactory),
                    lifetime));

            return serviceCollection;
        }

        /// <summary>
        ///     <para>
        ///         Registers an <see cref="IDbContextFactory{TContext}" /> in the <see cref="IServiceCollection" /> to create instances
        ///         of given <see cref="DbContext" /> type where instances are pooled for reuse.
        ///     </para>
        ///     <para>
        ///         Registering a factory instead of registering the context type directly allows for easy creation of new
        ///         <see cref="DbContext" /> instances.
        ///         Registering a factory is recommended for Blazor applications and other situations where the dependency
        ///         injection scope is not aligned with the context lifetime.
        ///     </para>
        ///     <para>
        ///         Use this method when using dependency injection in your application, such as with Blazor.
        ///         For applications that don't use dependency injection, consider creating <see cref="DbContext" />
        ///         instances directly with its constructor. The <see cref="DbContext.OnConfiguring" /> method can then be
        ///         overridden to configure a connection string and other options.
        ///     </para>
        ///     <para>
        ///         For more information on how to use this method, see the Entity Framework Core documentation at https://aka.ms/efdocs.
        ///         For more information on using dependency injection, see https://go.microsoft.com/fwlink/?LinkId=526890.
        ///     </para>
        /// </summary>
        /// <typeparam name="TContext"> The type of <see cref="DbContext" /> to be created by the factory. </typeparam>
        /// <param name="serviceCollection"> The <see cref="IServiceCollection" /> to add services to. </param>
        /// <param name="optionsAction">
        ///     <para>
        ///         A required action to configure the <see cref="DbContextOptions" /> for the context. When using
        ///         context pooling, options configuration must be performed externally; <see cref="DbContext.OnConfiguring" />
        ///         will not be called.
        ///     </para>
        /// </param>
        /// <param name="poolSize">
        ///     Sets the maximum number of instances retained by the pool.
        /// </param>
        /// <returns>
        ///     The same service collection so that multiple calls can be chained.
        /// </returns>
        public static IServiceCollection AddPooledDbContextFactory<TContext>(
            [NotNull] this IServiceCollection serviceCollection,
            [NotNull] Action<DbContextOptionsBuilder> optionsAction,
            int poolSize = 128)
            where TContext : DbContext
        {
            Check.NotNull(optionsAction, nameof(optionsAction));

            return AddPooledDbContextFactory<TContext>(serviceCollection, (_, ob) => optionsAction(ob));
        }

        /// <summary>
        ///     <para>
        ///         Registers an <see cref="IDbContextFactory{TContext}" /> in the <see cref="IServiceCollection" /> to create instances
        ///         of given <see cref="DbContext" /> type where instances are pooled for reuse.
        ///     </para>
        ///     <para>
        ///         Registering a factory instead of registering the context type directly allows for easy creation of new
        ///         <see cref="DbContext" /> instances.
        ///         Registering a factory is recommended for Blazor applications and other situations where the dependency
        ///         injection scope is not aligned with the context lifetime.
        ///     </para>
        ///     <para>
        ///         Use this method when using dependency injection in your application, such as with Blazor.
        ///         For applications that don't use dependency injection, consider creating <see cref="DbContext" />
        ///         instances directly with its constructor. The <see cref="DbContext.OnConfiguring" /> method can then be
        ///         overridden to configure a connection string and other options.
        ///     </para>
        ///     <para>
        ///         For more information on how to use this method, see the Entity Framework Core documentation at https://aka.ms/efdocs.
        ///         For more information on using dependency injection, see https://go.microsoft.com/fwlink/?LinkId=526890.
        ///     </para>
        /// </summary>
        /// <typeparam name="TContext"> The type of <see cref="DbContext" /> to be created by the factory. </typeparam>
        /// <param name="serviceCollection"> The <see cref="IServiceCollection" /> to add services to. </param>
        /// <param name="optionsAction">
        ///     <para>
        ///         A required action to configure the <see cref="DbContextOptions" /> for the context. When using
        ///         context pooling, options configuration must be performed externally; <see cref="DbContext.OnConfiguring" />
        ///         will not be called.
        ///     </para>
        /// </param>
        /// <param name="poolSize">
        ///     Sets the maximum number of instances retained by the pool.
        /// </param>
        /// <returns>
        ///     The same service collection so that multiple calls can be chained.
        /// </returns>
        public static IServiceCollection AddPooledDbContextFactory<TContext>(
            [NotNull] this IServiceCollection serviceCollection,
            [NotNull] Action<IServiceProvider, DbContextOptionsBuilder> optionsAction,
            int poolSize = 128)
            where TContext : DbContext
        {
            Check.NotNull(serviceCollection, nameof(serviceCollection));
            Check.NotNull(optionsAction, nameof(optionsAction));

            AddPoolingOptions<TContext>(serviceCollection, optionsAction, poolSize);

            serviceCollection.TryAddSingleton<IDbContextPool<TContext>, DbContextPool<TContext>>();
            serviceCollection.TryAddSingleton<IDbContextFactory<TContext>, PooledDbContextFactory<TContext>>();

            return serviceCollection;
        }

        private static void AddCoreServices<TContextImplementation>(
            IServiceCollection serviceCollection,
            Action<IServiceProvider, DbContextOptionsBuilder> optionsAction,
            ServiceLifetime optionsLifetime)
            where TContextImplementation : DbContext
        {
            serviceCollection.TryAdd(
                new ServiceDescriptor(
                    typeof(DbContextOptions<TContextImplementation>),
                    p => CreateDbContextOptions<TContextImplementation>(p, optionsAction),
                    optionsLifetime));

            serviceCollection.Add(
                new ServiceDescriptor(
                    typeof(DbContextOptions),
                    p => p.GetRequiredService<DbContextOptions<TContextImplementation>>(),
                    optionsLifetime));
        }

        private static DbContextOptions<TContext> CreateDbContextOptions<TContext>(
            [NotNull] IServiceProvider applicationServiceProvider,
            [CanBeNull] Action<IServiceProvider, DbContextOptionsBuilder> optionsAction)
            where TContext : DbContext
        {
            var builder = new DbContextOptionsBuilder<TContext>(
                new DbContextOptions<TContext>(new Dictionary<Type, IDbContextOptionsExtension>()));

            builder.UseApplicationServiceProvider(applicationServiceProvider);

            optionsAction?.Invoke(applicationServiceProvider, builder);

            return builder.Options;
        }

        private static void CheckContextConstructors<TContext>()
            where TContext : DbContext
        {
            var declaredConstructors = typeof(TContext).GetTypeInfo().DeclaredConstructors.ToList();
            if (declaredConstructors.Count == 1
                && declaredConstructors[0].GetParameters().Length == 0)
            {
                throw new ArgumentException(CoreStrings.DbContextMissingConstructor(typeof(TContext).ShortDisplayName()));
            }
        }
    }
}
