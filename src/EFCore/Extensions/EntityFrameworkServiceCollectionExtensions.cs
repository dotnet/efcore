// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore.Infrastructure.Internal;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.DependencyInjection.Extensions;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
///     Extension methods for setting up Entity Framework related services in an <see cref="IServiceCollection" />.
/// </summary>
public static class EntityFrameworkServiceCollectionExtensions
{
    /// <summary>
    ///     Registers the given context as a service in the <see cref="IServiceCollection" />.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         Use this method when using dependency injection in your application, such as with ASP.NET Core.
    ///         For applications that don't use dependency injection, consider creating <see cref="DbContext" />
    ///         instances directly with its constructor. The <see cref="DbContext.OnConfiguring" /> method can then be
    ///         overridden to configure a connection string and other options.
    ///     </para>
    ///     <para>
    ///         Entity Framework Core does not support multiple parallel operations being run on the same <see cref="DbContext" />
    ///         instance. This includes both parallel execution of async queries and any explicit concurrent use from multiple threads.
    ///         Therefore, always await async calls immediately, or use separate DbContext instances for operations that execute
    ///         in parallel. See <see href="https://aka.ms/efcore-docs-threading">Avoiding DbContext threading issues</see> for more information
    ///         and examples.
    ///     </para>
    ///     <para>
    ///         See <see href="https://aka.ms/efcore-docs-di">Using DbContext with dependency injection</see> for more information and examples.
    ///     </para>
    /// </remarks>
    /// <typeparam name="TContext">The type of context to be registered.</typeparam>
    /// <param name="serviceCollection">The <see cref="IServiceCollection" /> to add services to.</param>
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
    /// <param name="contextLifetime">The lifetime with which to register the DbContext service in the container.</param>
    /// <param name="optionsLifetime">The lifetime with which to register the DbContextOptions service in the container.</param>
    /// <returns>The same service collection so that multiple calls can be chained.</returns>
    public static IServiceCollection AddDbContext
        <[DynamicallyAccessedMembers(DbContext.DynamicallyAccessedMemberTypes)] TContext>(
            this IServiceCollection serviceCollection,
            Action<DbContextOptionsBuilder>? optionsAction = null,
            ServiceLifetime contextLifetime = ServiceLifetime.Scoped,
            ServiceLifetime optionsLifetime = ServiceLifetime.Scoped)
        where TContext : DbContext
        => AddDbContext<TContext, TContext>(serviceCollection, optionsAction, contextLifetime, optionsLifetime);

    /// <summary>
    ///     Registers the given context as a service in the <see cref="IServiceCollection" />.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         Use this method when using dependency injection in your application, such as with ASP.NET Core.
    ///         For applications that don't use dependency injection, consider creating <see cref="DbContext" />
    ///         instances directly with its constructor. The <see cref="DbContext.OnConfiguring" /> method can then be
    ///         overridden to configure a connection string and other options.
    ///     </para>
    ///     <para>
    ///         Entity Framework Core does not support multiple parallel operations being run on the same <see cref="DbContext" />
    ///         instance. This includes both parallel execution of async queries and any explicit concurrent use from multiple threads.
    ///         Therefore, always await async calls immediately, or use separate DbContext instances for operations that execute
    ///         in parallel. See <see href="https://aka.ms/efcore-docs-threading">Avoiding DbContext threading issues</see> for more information
    ///         and examples.
    ///     </para>
    ///     <para>
    ///         See <see href="https://aka.ms/efcore-docs-di">Using DbContext with dependency injection</see> for more information and examples.
    ///     </para>
    /// </remarks>
    /// <typeparam name="TContextService">The class or interface that will be used to resolve the context from the container.</typeparam>
    /// <typeparam name="TContextImplementation">The concrete implementation type to create.</typeparam>
    /// <param name="serviceCollection">The <see cref="IServiceCollection" /> to add services to.</param>
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
    /// <param name="contextLifetime">The lifetime with which to register the DbContext service in the container.</param>
    /// <param name="optionsLifetime">The lifetime with which to register the DbContextOptions service in the container.</param>
    /// <returns>The same service collection so that multiple calls can be chained.</returns>
    public static IServiceCollection AddDbContext
        <TContextService, [DynamicallyAccessedMembers(DbContext.DynamicallyAccessedMemberTypes)] TContextImplementation>(
            this IServiceCollection serviceCollection,
            Action<DbContextOptionsBuilder>? optionsAction = null,
            ServiceLifetime contextLifetime = ServiceLifetime.Scoped,
            ServiceLifetime optionsLifetime = ServiceLifetime.Scoped)
        where TContextImplementation : DbContext, TContextService
        => AddDbContext<TContextService, TContextImplementation>(
            serviceCollection,
            optionsAction == null
                ? null
                : (_, b) => optionsAction(b), contextLifetime, optionsLifetime);

    /// <summary>
    ///     Registers the given <see cref="DbContext" /> as a service in the <see cref="IServiceCollection" />,
    ///     and enables DbContext pooling for this registration.
    /// </summary>
    /// <remarks>
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
    ///         Entity Framework Core does not support multiple parallel operations being run on the same <see cref="DbContext" />
    ///         instance. This includes both parallel execution of async queries and any explicit concurrent use from multiple threads.
    ///         Therefore, always await async calls immediately, or use separate DbContext instances for operations that execute
    ///         in parallel. See <see href="https://aka.ms/efcore-docs-threading">Avoiding DbContext threading issues</see> for more information
    ///         and examples.
    ///     </para>
    ///     <para>
    ///         See <see href="https://aka.ms/efcore-docs-di">Using DbContext with dependency injection</see> and
    ///         <see href="https://aka.ms/efcore-docs-dbcontext-pooling">Using DbContext pooling</see> for more information and examples.
    ///     </para>
    /// </remarks>
    /// <typeparam name="TContext">The type of context to be registered.</typeparam>
    /// <param name="serviceCollection">The <see cref="IServiceCollection" /> to add services to.</param>
    /// <param name="optionsAction">
    ///     A required action to configure the <see cref="DbContextOptions" /> for the context. When using
    ///     context pooling, options configuration must be performed externally; <see cref="DbContext.OnConfiguring" />
    ///     will not be called.
    /// </param>
    /// <param name="poolSize">Sets the maximum number of instances retained by the pool. Defaults to 1024.</param>
    /// <returns>The same service collection so that multiple calls can be chained.</returns>
    public static IServiceCollection AddDbContextPool
        <[DynamicallyAccessedMembers(DbContext.DynamicallyAccessedMemberTypes)] TContext>(
            this IServiceCollection serviceCollection,
            Action<DbContextOptionsBuilder> optionsAction,
            int poolSize = DbContextPool<DbContext>.DefaultPoolSize)
        where TContext : DbContext
        => AddDbContextPool<TContext, TContext>(serviceCollection, optionsAction, poolSize);

    /// <summary>
    ///     Registers the given <see cref="DbContext" /> as a service in the <see cref="IServiceCollection" />,
    ///     and enables DbContext pooling for this registration.
    /// </summary>
    /// <remarks>
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
    ///         Entity Framework Core does not support multiple parallel operations being run on the same <see cref="DbContext" />
    ///         instance. This includes both parallel execution of async queries and any explicit concurrent use from multiple threads.
    ///         Therefore, always await async calls immediately, or use separate DbContext instances for operations that execute
    ///         in parallel. See <see href="https://aka.ms/efcore-docs-threading">Avoiding DbContext threading issues</see> for more information
    ///         and examples.
    ///     </para>
    ///     <para>
    ///         See <see href="https://aka.ms/efcore-docs-di">Using DbContext with dependency injection</see> and
    ///         <see href="https://aka.ms/efcore-docs-dbcontext-pooling">Using DbContext pooling</see> for more information and examples.
    ///     </para>
    /// </remarks>
    /// <typeparam name="TContextService">The class or interface that will be used to resolve the context from the container.</typeparam>
    /// <typeparam name="TContextImplementation">The concrete implementation type to create.</typeparam>
    /// <param name="serviceCollection">The <see cref="IServiceCollection" /> to add services to.</param>
    /// <param name="optionsAction">
    ///     A required action to configure the <see cref="DbContextOptions" /> for the context. When using
    ///     context pooling, options configuration must be performed externally; <see cref="DbContext.OnConfiguring" />
    ///     will not be called.
    /// </param>
    /// <param name="poolSize">Sets the maximum number of instances retained by the pool. Defaults to 1024.</param>
    /// <returns>The same service collection so that multiple calls can be chained.</returns>
    public static IServiceCollection AddDbContextPool
        <TContextService, [DynamicallyAccessedMembers(DbContext.DynamicallyAccessedMemberTypes)] TContextImplementation>(
            this IServiceCollection serviceCollection,
            Action<DbContextOptionsBuilder> optionsAction,
            int poolSize = DbContextPool<DbContext>.DefaultPoolSize)
        where TContextImplementation : DbContext, TContextService
        where TContextService : class
    {
        Check.NotNull(optionsAction, nameof(optionsAction));

        return AddDbContextPool<TContextService, TContextImplementation>(serviceCollection, (_, ob) => optionsAction(ob), poolSize);
    }

    /// <summary>
    ///     Registers the given <see cref="DbContext" /> as a service in the <see cref="IServiceCollection" />,
    ///     and enables DbContext pooling for this registration.
    /// </summary>
    /// <remarks>
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
    ///         Entity Framework Core does not support multiple parallel operations being run on the same <see cref="DbContext" />
    ///         instance. This includes both parallel execution of async queries and any explicit concurrent use from multiple threads.
    ///         Therefore, always await async calls immediately, or use separate DbContext instances for operations that execute
    ///         in parallel. See <see href="https://aka.ms/efcore-docs-threading">Avoiding DbContext threading issues</see> for more information
    ///         and examples.
    ///     </para>
    ///     <para>
    ///         See <see href="https://aka.ms/efcore-docs-di">Using DbContext with dependency injection</see> and
    ///         <see href="https://aka.ms/efcore-docs-dbcontext-pooling">Using DbContext pooling</see> for more information and examples.
    ///     </para>
    ///     <para>
    ///         This overload has an <paramref name="optionsAction" /> that provides the application's
    ///         <see cref="IServiceProvider" />. This is useful if you want to setup Entity Framework Core to resolve
    ///         its internal services from the primary application service provider.
    ///         By default, we recommend using
    ///         <see cref="AddDbContextPool{TContext}(IServiceCollection,Action{DbContextOptionsBuilder},int)" /> which allows
    ///         Entity Framework to create and maintain its own <see cref="IServiceProvider" /> for internal Entity Framework services.
    ///     </para>
    /// </remarks>
    /// <typeparam name="TContext">The type of context to be registered.</typeparam>
    /// <param name="serviceCollection">The <see cref="IServiceCollection" /> to add services to.</param>
    /// <param name="optionsAction">
    ///     A required action to configure the <see cref="DbContextOptions" /> for the context. When using
    ///     context pooling, options configuration must be performed externally; <see cref="DbContext.OnConfiguring" />
    ///     will not be called.
    /// </param>
    /// <param name="poolSize">Sets the maximum number of instances retained by the pool. Defaults to 1024.</param>
    /// <returns>The same service collection so that multiple calls can be chained.</returns>
    public static IServiceCollection AddDbContextPool
        <[DynamicallyAccessedMembers(DbContext.DynamicallyAccessedMemberTypes)] TContext>(
            this IServiceCollection serviceCollection,
            Action<IServiceProvider, DbContextOptionsBuilder> optionsAction,
            int poolSize = DbContextPool<DbContext>.DefaultPoolSize)
        where TContext : DbContext
        => AddDbContextPool<TContext, TContext>(serviceCollection, optionsAction, poolSize);

    /// <summary>
    ///     Registers the given <see cref="DbContext" /> as a service in the <see cref="IServiceCollection" />,
    ///     and enables DbContext pooling for this registration.
    /// </summary>
    /// <remarks>
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
    ///         Entity Framework Core does not support multiple parallel operations being run on the same <see cref="DbContext" />
    ///         instance. This includes both parallel execution of async queries and any explicit concurrent use from multiple threads.
    ///         Therefore, always await async calls immediately, or use separate DbContext instances for operations that execute
    ///         in parallel. See <see href="https://aka.ms/efcore-docs-threading">Avoiding DbContext threading issues</see> for more information
    ///         and examples.
    ///     </para>
    ///     <para>
    ///         See <see href="https://aka.ms/efcore-docs-di">Using DbContext with dependency injection</see> and
    ///         <see href="https://aka.ms/efcore-docs-dbcontext-pooling">Using DbContext pooling</see> for more information and examples.
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
    /// </remarks>
    /// <typeparam name="TContextService">The class or interface that will be used to resolve the context from the container.</typeparam>
    /// <typeparam name="TContextImplementation">The concrete implementation type to create.</typeparam>
    /// <param name="serviceCollection">The <see cref="IServiceCollection" /> to add services to.</param>
    /// <param name="optionsAction">
    ///     A required action to configure the <see cref="DbContextOptions" /> for the context. When using
    ///     context pooling, options configuration must be performed externally; <see cref="DbContext.OnConfiguring" />
    ///     will not be called.
    /// </param>
    /// <param name="poolSize">Sets the maximum number of instances retained by the pool. Defaults to 1024.</param>
    /// <returns>The same service collection so that multiple calls can be chained.</returns>
    public static IServiceCollection AddDbContextPool
        <TContextService, [DynamicallyAccessedMembers(DbContext.DynamicallyAccessedMemberTypes)] TContextImplementation>(
            this IServiceCollection serviceCollection,
            Action<IServiceProvider, DbContextOptionsBuilder> optionsAction,
            int poolSize = DbContextPool<DbContext>.DefaultPoolSize)
        where TContextImplementation : DbContext, TContextService
        where TContextService : class
    {
        Check.NotNull(optionsAction, nameof(optionsAction));

        AddPoolingOptions<TContextImplementation>(serviceCollection, optionsAction, poolSize);

        serviceCollection.TryAddSingleton<IDbContextPool<TContextImplementation>, DbContextPool<TContextImplementation>>();
        serviceCollection.TryAddScoped<IScopedDbContextLease<TContextImplementation>, ScopedDbContextLease<TContextImplementation>>();

        serviceCollection.TryAddScoped<TContextService>(
            sp => sp.GetRequiredService<IScopedDbContextLease<TContextImplementation>>().Context);

        if (typeof(TContextService) != typeof(TContextImplementation))
        {
            serviceCollection.TryAddScoped(p => (TContextImplementation)p.GetService<TContextService>()!);
        }

        return serviceCollection;
    }

    private static void AddPoolingOptions<[DynamicallyAccessedMembers(DbContext.DynamicallyAccessedMemberTypes)] TContext>(
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
    ///     Registers the given context as a service in the <see cref="IServiceCollection" />.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         Use this method when using dependency injection in your application, such as with ASP.NET Core.
    ///         For applications that don't use dependency injection, consider creating <see cref="DbContext" />
    ///         instances directly with its constructor. The <see cref="DbContext.OnConfiguring" /> method can then be
    ///         overridden to configure a connection string and other options.
    ///     </para>
    ///     <para>
    ///         Entity Framework Core does not support multiple parallel operations being run on the same <see cref="DbContext" />
    ///         instance. This includes both parallel execution of async queries and any explicit concurrent use from multiple threads.
    ///         Therefore, always await async calls immediately, or use separate DbContext instances for operations that execute
    ///         in parallel. See <see href="https://aka.ms/efcore-docs-threading">Avoiding DbContext threading issues</see> for more information
    ///         and examples.
    ///     </para>
    ///     <para>
    ///         See <see href="https://aka.ms/efcore-docs-di">Using DbContext with dependency injection</see> for more information and examples.
    ///     </para>
    /// </remarks>
    /// <typeparam name="TContext">The type of context to be registered.</typeparam>
    /// <param name="serviceCollection">The <see cref="IServiceCollection" /> to add services to.</param>
    /// <param name="contextLifetime">The lifetime with which to register the DbContext service in the container.</param>
    /// <param name="optionsLifetime">The lifetime with which to register the DbContextOptions service in the container.</param>
    /// <returns>The same service collection so that multiple calls can be chained.</returns>
    public static IServiceCollection AddDbContext
        <[DynamicallyAccessedMembers(DbContext.DynamicallyAccessedMemberTypes)] TContext>(
            this IServiceCollection serviceCollection,
            ServiceLifetime contextLifetime,
            ServiceLifetime optionsLifetime = ServiceLifetime.Scoped)
        where TContext : DbContext
        => AddDbContext<TContext, TContext>(serviceCollection, contextLifetime, optionsLifetime);

    /// <summary>
    ///     Registers the given context as a service in the <see cref="IServiceCollection" />.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         Use this method when using dependency injection in your application, such as with ASP.NET Core.
    ///         For applications that don't use dependency injection, consider creating <see cref="DbContext" />
    ///         instances directly with its constructor. The <see cref="DbContext.OnConfiguring" /> method can then be
    ///         overridden to configure a connection string and other options.
    ///     </para>
    ///     <para>
    ///         Entity Framework Core does not support multiple parallel operations being run on the same <see cref="DbContext" />
    ///         instance. This includes both parallel execution of async queries and any explicit concurrent use from multiple threads.
    ///         Therefore, always await async calls immediately, or use separate DbContext instances for operations that execute
    ///         in parallel. See <see href="https://aka.ms/efcore-docs-threading">Avoiding DbContext threading issues</see> for more information
    ///         and examples.
    ///     </para>
    ///     <para>
    ///         See <see href="https://aka.ms/efcore-docs-di">Using DbContext with dependency injection</see> for more information and examples.
    ///     </para>
    /// </remarks>
    /// <typeparam name="TContextService">The class or interface that will be used to resolve the context from the container.</typeparam>
    /// <typeparam name="TContextImplementation">The concrete implementation type to create.</typeparam>
    /// <param name="serviceCollection">The <see cref="IServiceCollection" /> to add services to.</param>
    /// <param name="contextLifetime">The lifetime with which to register the DbContext service in the container.</param>
    /// <param name="optionsLifetime">The lifetime with which to register the DbContextOptions service in the container.</param>
    /// <returns>The same service collection so that multiple calls can be chained.</returns>
    public static IServiceCollection AddDbContext
        <TContextService, [DynamicallyAccessedMembers(DbContext.DynamicallyAccessedMemberTypes)] TContextImplementation>(
            this IServiceCollection serviceCollection,
            ServiceLifetime contextLifetime,
            ServiceLifetime optionsLifetime = ServiceLifetime.Scoped)
        where TContextImplementation : DbContext, TContextService
        where TContextService : class
        => AddDbContext<TContextService, TContextImplementation>(
            serviceCollection,
            (Action<IServiceProvider, DbContextOptionsBuilder>?)null,
            contextLifetime,
            optionsLifetime);

    /// <summary>
    ///     Registers the given context as a service in the <see cref="IServiceCollection" />.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         Use this method when using dependency injection in your application, such as with ASP.NET Core.
    ///         For applications that don't use dependency injection, consider creating <see cref="DbContext" />
    ///         instances directly with its constructor. The <see cref="DbContext.OnConfiguring" /> method can then be
    ///         overridden to configure a connection string and other options.
    ///     </para>
    ///     <para>
    ///         Entity Framework Core does not support multiple parallel operations being run on the same <see cref="DbContext" />
    ///         instance. This includes both parallel execution of async queries and any explicit concurrent use from multiple threads.
    ///         Therefore, always await async calls immediately, or use separate DbContext instances for operations that execute
    ///         in parallel. See <see href="https://aka.ms/efcore-docs-threading">Avoiding DbContext threading issues</see> for more information
    ///         and examples.
    ///     </para>
    ///     <para>
    ///         Entity Framework Core does not support multiple parallel operations being run on the same <see cref="DbContext" />
    ///         instance. This includes both parallel execution of async queries and any explicit concurrent use from multiple threads.
    ///         Therefore, always await async calls immediately, or use separate DbContext instances for operations that execute
    ///         in parallel. See <see href="https://aka.ms/efcore-docs-threading">Avoiding DbContext threading issues</see> for more information
    ///         and examples.
    ///     </para>
    ///     <para>
    ///         See <see href="https://aka.ms/efcore-docs-di">Using DbContext with dependency injection</see> for more information and examples.
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
    /// </remarks>
    /// <typeparam name="TContext">The type of context to be registered.</typeparam>
    /// <param name="serviceCollection">The <see cref="IServiceCollection" /> to add services to.</param>
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
    /// <param name="contextLifetime">The lifetime with which to register the DbContext service in the container.</param>
    /// <param name="optionsLifetime">The lifetime with which to register the DbContextOptions service in the container.</param>
    /// <returns>The same service collection so that multiple calls can be chained.</returns>
    public static IServiceCollection AddDbContext
        <[DynamicallyAccessedMembers(DbContext.DynamicallyAccessedMemberTypes)] TContext>(
            this IServiceCollection serviceCollection,
            Action<IServiceProvider, DbContextOptionsBuilder>? optionsAction,
            ServiceLifetime contextLifetime = ServiceLifetime.Scoped,
            ServiceLifetime optionsLifetime = ServiceLifetime.Scoped)
        where TContext : DbContext
        => AddDbContext<TContext, TContext>(serviceCollection, optionsAction, contextLifetime, optionsLifetime);

    /// <summary>
    ///     Registers the given context as a service in the <see cref="IServiceCollection" />.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         Use this method when using dependency injection in your application, such as with ASP.NET Core.
    ///         For applications that don't use dependency injection, consider creating <see cref="DbContext" />
    ///         instances directly with its constructor. The <see cref="DbContext.OnConfiguring" /> method can then be
    ///         overridden to configure a connection string and other options.
    ///     </para>
    ///     <para>
    ///         Entity Framework Core does not support multiple parallel operations being run on the same <see cref="DbContext" />
    ///         instance. This includes both parallel execution of async queries and any explicit concurrent use from multiple threads.
    ///         Therefore, always await async calls immediately, or use separate DbContext instances for operations that execute
    ///         in parallel. See <see href="https://aka.ms/efcore-docs-threading">Avoiding DbContext threading issues</see> for more information
    ///         and examples.
    ///     </para>
    ///     <para>
    ///         See <see href="https://aka.ms/efcore-docs-di">Using DbContext with dependency injection</see> for more information and examples.
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
    /// </remarks>
    /// <typeparam name="TContextService">The class or interface that will be used to resolve the context from the container.</typeparam>
    /// <typeparam name="TContextImplementation">The concrete implementation type to create.</typeparam>
    /// <param name="serviceCollection">The <see cref="IServiceCollection" /> to add services to.</param>
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
    /// <param name="contextLifetime">The lifetime with which to register the DbContext service in the container.</param>
    /// <param name="optionsLifetime">The lifetime with which to register the DbContextOptions service in the container.</param>
    /// <returns>The same service collection so that multiple calls can be chained.</returns>
    public static IServiceCollection AddDbContext
        <TContextService, [DynamicallyAccessedMembers(DbContext.DynamicallyAccessedMemberTypes)] TContextImplementation>(
            this IServiceCollection serviceCollection,
            Action<IServiceProvider, DbContextOptionsBuilder>? optionsAction,
            ServiceLifetime contextLifetime = ServiceLifetime.Scoped,
            ServiceLifetime optionsLifetime = ServiceLifetime.Scoped)
        where TContextImplementation : DbContext, TContextService
    {
        if (contextLifetime == ServiceLifetime.Singleton)
        {
            optionsLifetime = ServiceLifetime.Singleton;
        }

        if (optionsAction != null)
        {
            CheckContextConstructors<TContextImplementation>();
        }

        AddCoreServices<TContextImplementation>(serviceCollection, optionsAction, optionsLifetime);

        if (serviceCollection.Any(d => d.ServiceType == typeof(IDbContextFactorySource<TContextImplementation>)))
        {
            // Override registration made by AddDbContextFactory
            var serviceDescriptor = serviceCollection.FirstOrDefault(d => d.ServiceType == typeof(TContextImplementation));
            if (serviceDescriptor != null)
            {
                serviceCollection.Remove(serviceDescriptor);
            }
        }

        serviceCollection.TryAdd(new ServiceDescriptor(typeof(TContextService), typeof(TContextImplementation), contextLifetime));

        if (typeof(TContextService) != typeof(TContextImplementation))
        {
            serviceCollection.TryAdd(
                new ServiceDescriptor(
                    typeof(TContextImplementation),
                    p => (TContextImplementation)p.GetService<TContextService>()!,
                    contextLifetime));
        }

        return serviceCollection;
    }

    /// <summary>
    ///     Registers an <see cref="IDbContextFactory{TContext}" /> in the <see cref="IServiceCollection" /> to create instances
    ///     of given <see cref="DbContext" /> type.
    /// </summary>
    /// <remarks>
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
    ///         For convenience, this method also registers the context type itself as a scoped service. This allows a context
    ///         instance to be resolved from a dependency injection scope directly or created by the factory, as appropriate.
    ///     </para>
    ///     <para>
    ///         Entity Framework Core does not support multiple parallel operations being run on the same <see cref="DbContext" />
    ///         instance. This includes both parallel execution of async queries and any explicit concurrent use from multiple threads.
    ///         Therefore, always await async calls immediately, or use separate DbContext instances for operations that execute
    ///         in parallel. See <see href="https://aka.ms/efcore-docs-threading">Avoiding DbContext threading issues</see> for more information
    ///         and examples.
    ///     </para>
    ///     <para>
    ///         See <see href="https://aka.ms/efcore-docs-di">Using DbContext with dependency injection</see> and
    ///         <see href="https://aka.ms/efcore-docs-dbcontext-factory">Using DbContext factories</see> for more information and examples.
    ///     </para>
    /// </remarks>
    /// <typeparam name="TContext">The type of <see cref="DbContext" /> to be created by the factory.</typeparam>
    /// <param name="serviceCollection">The <see cref="IServiceCollection" /> to add services to.</param>
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
    /// <returns>The same service collection so that multiple calls can be chained.</returns>
    public static IServiceCollection AddDbContextFactory
        <[DynamicallyAccessedMembers(DbContext.DynamicallyAccessedMemberTypes)] TContext>(
            this IServiceCollection serviceCollection,
            Action<DbContextOptionsBuilder>? optionsAction = null,
            ServiceLifetime lifetime = ServiceLifetime.Singleton)
        where TContext : DbContext
        => AddDbContextFactory<TContext, DbContextFactory<TContext>>(serviceCollection, optionsAction, lifetime);

    /// <summary>
    ///     Registers an <see cref="IDbContextFactory{TContext}" /> in the <see cref="IServiceCollection" /> to create instances
    ///     of given <see cref="DbContext" /> type.
    /// </summary>
    /// <remarks>
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
    ///         For convenience, this method also registers the context type itself as a scoped service. This allows a context
    ///         instance to be resolved from a dependency injection scope directly or created by the factory, as appropriate.
    ///     </para>
    ///     <para>
    ///         This overload allows a specific implementation of <see cref="IDbContextFactory{TContext}" /> to be registered
    ///         instead of using the default factory shipped with EF Core.
    ///     </para>
    ///     <para>
    ///         Entity Framework Core does not support multiple parallel operations being run on the same <see cref="DbContext" />
    ///         instance. This includes both parallel execution of async queries and any explicit concurrent use from multiple threads.
    ///         Therefore, always await async calls immediately, or use separate DbContext instances for operations that execute
    ///         in parallel. See <see href="https://aka.ms/efcore-docs-threading">Avoiding DbContext threading issues</see> for more information
    ///         and examples.
    ///     </para>
    ///     <para>
    ///         See <see href="https://aka.ms/efcore-docs-di">Using DbContext with dependency injection</see> and
    ///         <see href="https://aka.ms/efcore-docs-dbcontext-factory">Using DbContext factories</see> for more information and examples.
    ///     </para>
    /// </remarks>
    /// <typeparam name="TContext">The type of <see cref="DbContext" /> to be created by the factory.</typeparam>
    /// <typeparam name="TFactory">The type of <see cref="IDbContextFactory{TContext}" /> to register.</typeparam>
    /// <param name="serviceCollection">The <see cref="IServiceCollection" /> to add services to.</param>
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
    /// <returns>The same service collection so that multiple calls can be chained.</returns>
    public static IServiceCollection AddDbContextFactory
    <[DynamicallyAccessedMembers(DbContext.DynamicallyAccessedMemberTypes)] TContext,
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TFactory>(
        this IServiceCollection serviceCollection,
        Action<DbContextOptionsBuilder>? optionsAction = null,
        ServiceLifetime lifetime = ServiceLifetime.Singleton)
        where TContext : DbContext
        where TFactory : IDbContextFactory<TContext>
        => AddDbContextFactory<TContext, TFactory>(
            serviceCollection,
            optionsAction == null
                ? null
                : (_, b) => optionsAction(b),
            lifetime);

    /// <summary>
    ///     Registers an <see cref="IDbContextFactory{TContext}" /> in the <see cref="IServiceCollection" /> to create instances
    ///     of given <see cref="DbContext" /> type.
    /// </summary>
    /// <remarks>
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
    ///         For convenience, this method also registers the context type itself as a scoped service. This allows a context
    ///         instance to be resolved from a dependency injection scope directly or created by the factory, as appropriate.
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
    ///         Entity Framework Core does not support multiple parallel operations being run on the same <see cref="DbContext" />
    ///         instance. This includes both parallel execution of async queries and any explicit concurrent use from multiple threads.
    ///         Therefore, always await async calls immediately, or use separate DbContext instances for operations that execute
    ///         in parallel. See <see href="https://aka.ms/efcore-docs-threading">Avoiding DbContext threading issues</see> for more information
    ///         and examples.
    ///     </para>
    ///     <para>
    ///         See <see href="https://aka.ms/efcore-docs-di">Using DbContext with dependency injection</see> and
    ///         <see href="https://aka.ms/efcore-docs-dbcontext-factory">Using DbContext factories</see> for more information and examples.
    ///     </para>
    /// </remarks>
    /// <typeparam name="TContext">The type of <see cref="DbContext" /> to be created by the factory.</typeparam>
    /// <param name="serviceCollection">The <see cref="IServiceCollection" /> to add services to.</param>
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
    /// <returns>The same service collection so that multiple calls can be chained.</returns>
    public static IServiceCollection AddDbContextFactory
        <[DynamicallyAccessedMembers(DbContext.DynamicallyAccessedMemberTypes)] TContext>(
            this IServiceCollection serviceCollection,
            Action<IServiceProvider, DbContextOptionsBuilder> optionsAction,
            ServiceLifetime lifetime = ServiceLifetime.Singleton)
        where TContext : DbContext
        => AddDbContextFactory<TContext, DbContextFactory<TContext>>(serviceCollection, optionsAction, lifetime);

    /// <summary>
    ///     Registers an <see cref="IDbContextFactory{TContext}" /> in the <see cref="IServiceCollection" /> to create instances
    ///     of given <see cref="DbContext" /> type.
    /// </summary>
    /// <remarks>
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
    ///         For convenience, this method also registers the context type itself as a scoped service. This allows a context
    ///         instance to be resolved from a dependency injection scope directly or created by the factory, as appropriate.
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
    ///         Entity Framework Core does not support multiple parallel operations being run on the same <see cref="DbContext" />
    ///         instance. This includes both parallel execution of async queries and any explicit concurrent use from multiple threads.
    ///         Therefore, always await async calls immediately, or use separate DbContext instances for operations that execute
    ///         in parallel. See <see href="https://aka.ms/efcore-docs-threading">Avoiding DbContext threading issues</see> for more information
    ///         and examples.
    ///     </para>
    ///     <para>
    ///         See <see href="https://aka.ms/efcore-docs-di">Using DbContext with dependency injection</see> and
    ///         <see href="https://aka.ms/efcore-docs-dbcontext-factory">Using DbContext factories</see> for more information and examples.
    ///     </para>
    /// </remarks>
    /// <typeparam name="TContext">The type of <see cref="DbContext" /> to be created by the factory.</typeparam>
    /// <typeparam name="TFactory">The type of <see cref="IDbContextFactory{TContext}" /> to register.</typeparam>
    /// <param name="serviceCollection">The <see cref="IServiceCollection" /> to add services to.</param>
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
    /// <returns>The same service collection so that multiple calls can be chained.</returns>
    public static IServiceCollection AddDbContextFactory
    <[DynamicallyAccessedMembers(DbContext.DynamicallyAccessedMemberTypes)] TContext,
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TFactory>(
        this IServiceCollection serviceCollection,
        Action<IServiceProvider, DbContextOptionsBuilder>? optionsAction,
        ServiceLifetime lifetime = ServiceLifetime.Singleton)
        where TContext : DbContext
        where TFactory : IDbContextFactory<TContext>
    {
        AddCoreServices<TContext>(serviceCollection, optionsAction, lifetime);

        serviceCollection.AddSingleton<IDbContextFactorySource<TContext>, DbContextFactorySource<TContext>>();

        serviceCollection.TryAdd(
            new ServiceDescriptor(
                typeof(IDbContextFactory<TContext>),
                typeof(TFactory),
                lifetime));

        serviceCollection.TryAdd(
            new ServiceDescriptor(
                typeof(TContext),
                typeof(TContext),
                lifetime == ServiceLifetime.Transient
                    ? ServiceLifetime.Transient
                    : ServiceLifetime.Scoped));

        return serviceCollection;
    }

    /// <summary>
    ///     Registers an <see cref="IDbContextFactory{TContext}" /> in the <see cref="IServiceCollection" /> to create instances
    ///     of given <see cref="DbContext" /> type where instances are pooled for reuse.
    /// </summary>
    /// <remarks>
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
    ///         Entity Framework Core does not support multiple parallel operations being run on the same <see cref="DbContext" />
    ///         instance. This includes both parallel execution of async queries and any explicit concurrent use from multiple threads.
    ///         Therefore, always await async calls immediately, or use separate DbContext instances for operations that execute
    ///         in parallel. See <see href="https://aka.ms/efcore-docs-threading">Avoiding DbContext threading issues</see> for more information
    ///         and examples.
    ///     </para>
    ///     <para>
    ///         See <see href="https://aka.ms/efcore-docs-di">Using DbContext with dependency injection</see>,
    ///         <see href="https://aka.ms/efcore-docs-dbcontext-factory">Using DbContext factories</see>, and
    ///         <see href="https://aka.ms/efcore-docs-dbcontext-pooling">Using DbContext pooling</see> for more information and examples.
    ///     </para>
    /// </remarks>
    /// <typeparam name="TContext">The type of <see cref="DbContext" /> to be created by the factory.</typeparam>
    /// <param name="serviceCollection">The <see cref="IServiceCollection" /> to add services to.</param>
    /// <param name="optionsAction">
    ///     A required action to configure the <see cref="DbContextOptions" /> for the context. When using
    ///     context pooling, options configuration must be performed externally; <see cref="DbContext.OnConfiguring" />
    ///     will not be called.
    /// </param>
    /// <param name="poolSize">Sets the maximum number of instances retained by the pool. Defaults to 1024.</param>
    /// <returns>The same service collection so that multiple calls can be chained.</returns>
    public static IServiceCollection AddPooledDbContextFactory
        <[DynamicallyAccessedMembers(DbContext.DynamicallyAccessedMemberTypes)] TContext>(
            this IServiceCollection serviceCollection,
            Action<DbContextOptionsBuilder> optionsAction,
            int poolSize = DbContextPool<DbContext>.DefaultPoolSize)
        where TContext : DbContext
    {
        Check.NotNull(optionsAction, nameof(optionsAction));

        return AddPooledDbContextFactory<TContext>(serviceCollection, (_, ob) => optionsAction(ob), poolSize);
    }

    /// <summary>
    ///     Registers an <see cref="IDbContextFactory{TContext}" /> in the <see cref="IServiceCollection" /> to create instances
    ///     of given <see cref="DbContext" /> type where instances are pooled for reuse.
    /// </summary>
    /// <remarks>
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
    ///         Entity Framework Core does not support multiple parallel operations being run on the same <see cref="DbContext" />
    ///         instance. This includes both parallel execution of async queries and any explicit concurrent use from multiple threads.
    ///         Therefore, always await async calls immediately, or use separate DbContext instances for operations that execute
    ///         in parallel. See <see href="https://aka.ms/efcore-docs-threading">Avoiding DbContext threading issues</see> for more information
    ///         and examples.
    ///     </para>
    ///     <para>
    ///         See <see href="https://aka.ms/efcore-docs-di">Using DbContext with dependency injection</see>,
    ///         <see href="https://aka.ms/efcore-docs-dbcontext-factory">Using DbContext factories</see>, and
    ///         <see href="https://aka.ms/efcore-docs-dbcontext-pooling">Using DbContext pooling</see> for more information and examples.
    ///     </para>
    /// </remarks>
    /// <typeparam name="TContext">The type of <see cref="DbContext" /> to be created by the factory.</typeparam>
    /// <param name="serviceCollection">The <see cref="IServiceCollection" /> to add services to.</param>
    /// <param name="optionsAction">
    ///     A required action to configure the <see cref="DbContextOptions" /> for the context. When using
    ///     context pooling, options configuration must be performed externally; <see cref="DbContext.OnConfiguring" />
    ///     will not be called.
    /// </param>
    /// <param name="poolSize">Sets the maximum number of instances retained by the pool. Defaults to 1024.</param>
    /// <returns>The same service collection so that multiple calls can be chained.</returns>
    public static IServiceCollection AddPooledDbContextFactory
        <[DynamicallyAccessedMembers(DbContext.DynamicallyAccessedMemberTypes)] TContext>(
            this IServiceCollection serviceCollection,
            Action<IServiceProvider, DbContextOptionsBuilder> optionsAction,
            int poolSize = DbContextPool<DbContext>.DefaultPoolSize)
        where TContext : DbContext
    {
        Check.NotNull(optionsAction, nameof(optionsAction));

        AddPoolingOptions<TContext>(serviceCollection, optionsAction, poolSize);

        serviceCollection.TryAddSingleton<IDbContextPool<TContext>, DbContextPool<TContext>>();
        serviceCollection.TryAddSingleton<IDbContextFactory<TContext>>(
            sp => new PooledDbContextFactory<TContext>(sp.GetRequiredService<IDbContextPool<TContext>>()));

        return serviceCollection;
    }

    /// <summary>
    ///     Configures the given context type in the <see cref="IServiceCollection" />.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         <see cref="AddDbContext{TContext}(IServiceCollection,Action{DbContextOptionsBuilder},ServiceLifetime,ServiceLifetime)" />,
    ///         <see cref="AddDbContextPool{TContext}(IServiceCollection,Action{DbContextOptionsBuilder},int)" />, 
    ///         <see cref="AddDbContextFactory{TContext, TFactory}(IServiceCollection,Action{DbContextOptionsBuilder}?,ServiceLifetime)" /> or
    ///         <see cref="AddPooledDbContextFactory{TContext}(IServiceCollection,Action{DbContextOptionsBuilder},int)" />
    ///         must also be called for the specified configuration to take effect.
    ///         Calling this method after any of the above will ovewrite conflicting configuration.
    ///         For non-pooled contexts <see cref="DbContext.OnConfiguring" /> configuration will be applied
    ///         in addition to configuration performed here.
    ///     </para>
    ///     <para>
    ///         This method can be invoked multiple times and the configuration will be applied in the given order.
    ///     </para>
    ///     <para>
    ///         See <see href="https://aka.ms/efcore-docs-di">Using DbContext with dependency injection</see> for more information and examples.
    ///     </para>
    /// </remarks>
    /// <typeparam name="TContext">The type of context to be registered.</typeparam>
    /// <param name="serviceCollection">The <see cref="IServiceCollection" /> to add services to.</param>
    /// <param name="optionsAction">An action to configure the <see cref="DbContextOptions" /> for the context.</param>
    /// <param name="optionsLifetime">
    ///     The lifetime with which the <see cref="DbContextOptions" /> service will be registered in the container.
    /// </param>
    /// <returns>The same service collection so that multiple calls can be chained.</returns>
    public static IServiceCollection ConfigureDbContext
        <[DynamicallyAccessedMembers(DbContext.DynamicallyAccessedMemberTypes)] TContext>(
            this IServiceCollection serviceCollection,
            Action<DbContextOptionsBuilder> optionsAction,
            ServiceLifetime optionsLifetime = ServiceLifetime.Singleton)
        where TContext : DbContext
        => ConfigureDbContext<TContext>(serviceCollection, (_, b) => optionsAction(b), optionsLifetime);

    /// <summary>
    ///     Configures the given context type in the <see cref="IServiceCollection" />.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         <see cref="AddDbContext{TContext}(IServiceCollection,Action{DbContextOptionsBuilder},ServiceLifetime,ServiceLifetime)" />,
    ///         <see cref="AddDbContextPool{TContext}(IServiceCollection,Action{DbContextOptionsBuilder},int)" />, 
    ///         <see cref="AddDbContextFactory{TContext, TFactory}(IServiceCollection,Action{DbContextOptionsBuilder}?,ServiceLifetime)" /> or
    ///         <see cref="AddPooledDbContextFactory{TContext}(IServiceCollection,Action{DbContextOptionsBuilder},int)" />
    ///         must also be called for the specified configuration to take effect.
    ///         Calling this method after any of the above will ovewrite conflicting configuration.
    ///         For non-pooled contexts <see cref="DbContext.OnConfiguring" /> configuration will be applied
    ///         in addition to configuration performed here.
    ///     </para>
    ///     <para>
    ///         This method can be invoked multiple times and the configuration will be applied in the given order.
    ///     </para>
    ///     <para>
    ///         See <see href="https://aka.ms/efcore-docs-di">Using DbContext with dependency injection</see> for more information and examples.
    ///     </para>
    /// </remarks>
    /// <typeparam name="TContext">The type of context to be registered.</typeparam>
    /// <param name="serviceCollection">The <see cref="IServiceCollection" /> to add services to.</param>
    /// <param name="optionsAction">An action to configure the <see cref="DbContextOptions" /> for the context.</param>
    /// <param name="optionsLifetime">
    ///     The lifetime with which the <see cref="DbContextOptions" /> service will be registered in the container.
    /// </param>
    /// <returns>The same service collection so that multiple calls can be chained.</returns>
    public static IServiceCollection ConfigureDbContext
        <[DynamicallyAccessedMembers(DbContext.DynamicallyAccessedMemberTypes)] TContext>(
            this IServiceCollection serviceCollection,
            Action<IServiceProvider, DbContextOptionsBuilder> optionsAction,
            ServiceLifetime optionsLifetime = ServiceLifetime.Singleton)
        where TContext : DbContext
    {
        serviceCollection.Add(
            new ServiceDescriptor(
                typeof(IDbContextOptionsConfiguration<TContext>),
                p => new DbContextOptionsConfiguration<TContext>(optionsAction),
                optionsLifetime));

        return serviceCollection;
    }

    private static void AddCoreServices<TContextImplementation>(
        IServiceCollection serviceCollection,
        Action<IServiceProvider, DbContextOptionsBuilder>? optionsAction,
        ServiceLifetime optionsLifetime)
        where TContextImplementation : DbContext
    {
        serviceCollection.TryAddSingleton<ServiceProviderAccessor>();

        if (optionsAction != null)
        {
            serviceCollection.ConfigureDbContext<TContextImplementation>(optionsAction, optionsLifetime);
        }

        serviceCollection.TryAdd(
            new ServiceDescriptor(
                typeof(DbContextOptions<TContextImplementation>),
                CreateDbContextOptions<TContextImplementation>,
                optionsLifetime));

        serviceCollection.Add(
            new ServiceDescriptor(
                typeof(DbContextOptions),
                p => p.GetRequiredService<DbContextOptions<TContextImplementation>>(),
                optionsLifetime));
    }

    private static DbContextOptions<TContext> CreateDbContextOptions<TContext>(
        IServiceProvider applicationServiceProvider)
        where TContext : DbContext
    {
        var builder = new DbContextOptionsBuilder<TContext>(
            new DbContextOptions<TContext>(new Dictionary<Type, IDbContextOptionsExtension>()));

        builder.UseApplicationServiceProvider(applicationServiceProvider);

        foreach (var configuration in applicationServiceProvider.GetServices<IDbContextOptionsConfiguration<TContext>>())
        {
            configuration.Configure(applicationServiceProvider, builder);
        }

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
