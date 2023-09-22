// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Proxies.Internal;

namespace Microsoft.EntityFrameworkCore;

/// <summary>
///     Extension methods related to use of proxies with Entity Framework Core.
/// </summary>
public static class ProxiesExtensions
{
    /// <summary>
    ///     Turns on the creation of change tracking proxies.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         Note that this requires appropriate services to be available in the EF internal service provider. Normally this
    ///         will happen automatically, but if the application is controlling the service provider, then a call to
    ///         <see cref="ProxiesServiceCollectionExtensions.AddEntityFrameworkProxies" /> may be needed.
    ///     </para>
    ///     <para>
    ///         See <see href="https://aka.ms/efcore-docs-notification-entities">Notification entities</see> for more information and examples.
    ///     </para>
    /// </remarks>
    /// <param name="optionsBuilder">
    ///     The options builder, as passed to <see cref="DbContext.OnConfiguring" />
    ///     or exposed AddDbContext.
    /// </param>
    /// <param name="useChangeTrackingProxies">
    ///     <see langword="true" /> to use change tracking proxies; <see langword="false" /> to prevent their
    ///     use.
    /// </param>
    /// <param name="checkEquality">
    ///     <see langword="true" /> if proxy change detection should check if the incoming value is equal to the current
    ///     value before notifying. Defaults to <see langword="true" />.
    /// </param>
    /// <returns>The same builder to allow method calls to be chained.</returns>
    public static DbContextOptionsBuilder UseChangeTrackingProxies(
        this DbContextOptionsBuilder optionsBuilder,
        bool useChangeTrackingProxies = true,
        bool checkEquality = true)
    {
        var extension = optionsBuilder.Options.FindExtension<ProxiesOptionsExtension>()
            ?? new ProxiesOptionsExtension();

        extension = extension.WithChangeTracking(useChangeTrackingProxies, checkEquality);

        ((IDbContextOptionsBuilderInfrastructure)optionsBuilder).AddOrUpdateExtension(extension);

        return optionsBuilder;
    }

    /// <summary>
    ///     Turns on the creation of change tracking proxies.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         Note that this requires appropriate services to be available in the EF internal service provider. Normally this
    ///         will happen automatically, but if the application is controlling the service provider, then a call to
    ///         <see cref="ProxiesServiceCollectionExtensions.AddEntityFrameworkProxies" /> may be needed.
    ///     </para>
    ///     <para>
    ///         See <see href="https://aka.ms/efcore-docs-notification-entities">Notification entities</see> for more information and examples.
    ///     </para>
    /// </remarks>
    /// <typeparam name="TContext">The <see cref="DbContext" /> type.</typeparam>
    /// <param name="optionsBuilder">
    ///     The options builder, as passed to <see cref="DbContext.OnConfiguring" />
    ///     or exposed AddDbContext.
    /// </param>
    /// <param name="useChangeTrackingProxies">
    ///     <see langword="true" /> to use change tracking proxies; <see langword="false" /> to prevent their
    ///     use.
    /// </param>
    /// <param name="checkEquality">
    ///     <see langword="true" /> if proxy change detection should check if the incoming value is equal to the current
    ///     value before notifying. Defaults to <see langword="true" />.
    /// </param>
    /// <returns>The same builder to allow method calls to be chained.</returns>
    public static DbContextOptionsBuilder<TContext> UseChangeTrackingProxies<TContext>(
        this DbContextOptionsBuilder<TContext> optionsBuilder,
        bool useChangeTrackingProxies = true,
        bool checkEquality = true)
        where TContext : DbContext
        => (DbContextOptionsBuilder<TContext>)UseChangeTrackingProxies(
            (DbContextOptionsBuilder)optionsBuilder, useChangeTrackingProxies, checkEquality);

    /// <summary>
    ///     Turns on the creation of lazy loading proxies.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         Note that this requires appropriate services to be available in the EF internal service provider. Normally this
    ///         will happen automatically, but if the application is controlling the service provider, then a call to
    ///         <see cref="ProxiesServiceCollectionExtensions.AddEntityFrameworkProxies" /> may be needed.
    ///     </para>
    ///     <para>
    ///         See <see href="https://aka.ms/efcore-docs-lazy-loading">Lazy loading</see> for more information and examples.
    ///     </para>
    /// </remarks>
    /// <param name="optionsBuilder">
    ///     The options builder, as passed to <see cref="DbContext.OnConfiguring" />
    ///     or exposed AddDbContext.
    /// </param>
    /// <param name="useLazyLoadingProxies"><see langword="true" /> to use lazy loading proxies; <see langword="false" /> to prevent their use.</param>
    /// <returns>The same builder to allow method calls to be chained.</returns>
    public static DbContextOptionsBuilder UseLazyLoadingProxies(
        this DbContextOptionsBuilder optionsBuilder,
        bool useLazyLoadingProxies = true)
    {
        var extension = optionsBuilder.Options.FindExtension<ProxiesOptionsExtension>()
            ?? new ProxiesOptionsExtension();

        extension = extension.WithLazyLoading(useLazyLoadingProxies);

        ((IDbContextOptionsBuilderInfrastructure)optionsBuilder).AddOrUpdateExtension(extension);

        return optionsBuilder;
    }

    /// <summary>
    ///     Turns on the creation of lazy loading proxies.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         Note that this requires appropriate services to be available in the EF internal service provider. Normally this
    ///         will happen automatically, but if the application is controlling the service provider, then a call to
    ///         <see cref="ProxiesServiceCollectionExtensions.AddEntityFrameworkProxies" /> may be needed.
    ///     </para>
    ///     <para>
    ///         See <see href="https://aka.ms/efcore-docs-lazy-loading">Lazy loading</see> for more information and examples.
    ///     </para>
    /// </remarks>
    /// <param name="optionsBuilder">
    ///     The options builder, as passed to <see cref="DbContext.OnConfiguring" />
    ///     or exposed AddDbContext.
    /// </param>
    /// <param name="lazyLoadingProxiesOptionsAction">An optional action to allow additional proxy-specific configuration.</param>
    /// <returns>The same builder to allow method calls to be chained.</returns>
    public static DbContextOptionsBuilder UseLazyLoadingProxies(
        this DbContextOptionsBuilder optionsBuilder,
        Action<LazyLoadingProxiesOptionsBuilder> lazyLoadingProxiesOptionsAction)
    {
        Check.NotNull(lazyLoadingProxiesOptionsAction, nameof(lazyLoadingProxiesOptionsAction));

        var extension = optionsBuilder.Options.FindExtension<ProxiesOptionsExtension>()
            ?? new ProxiesOptionsExtension();

        ((IDbContextOptionsBuilderInfrastructure)optionsBuilder).AddOrUpdateExtension(
            extension.WithLazyLoading(useLazyLoadingProxies: true));

        lazyLoadingProxiesOptionsAction.Invoke(new LazyLoadingProxiesOptionsBuilder(optionsBuilder));

        return optionsBuilder;
    }

    /// <summary>
    ///     Turns on the creation of lazy loading proxies.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         Note that this requires appropriate services to be available in the EF internal service provider. Normally this
    ///         will happen automatically, but if the application is controlling the service provider, then a call to
    ///         <see cref="ProxiesServiceCollectionExtensions.AddEntityFrameworkProxies" /> may be needed.
    ///     </para>
    ///     <para>
    ///         See <see href="https://aka.ms/efcore-docs-lazy-loading">Lazy loading</see> for more information and examples.
    ///     </para>
    /// </remarks>
    /// <typeparam name="TContext">The <see cref="DbContext" /> type.</typeparam>
    /// <param name="optionsBuilder">
    ///     The options builder, as passed to <see cref="DbContext.OnConfiguring" />
    ///     or exposed AddDbContext.
    /// </param>
    /// <param name="useLazyLoadingProxies"><see langword="true" /> to use lazy loading proxies; <see langword="false" /> to prevent their use.</param>
    /// <returns>The same builder to allow method calls to be chained.</returns>
    public static DbContextOptionsBuilder<TContext> UseLazyLoadingProxies<TContext>(
        this DbContextOptionsBuilder<TContext> optionsBuilder,
        bool useLazyLoadingProxies = true)
        where TContext : DbContext
        => (DbContextOptionsBuilder<TContext>)UseLazyLoadingProxies((DbContextOptionsBuilder)optionsBuilder, useLazyLoadingProxies);

    /// <summary>
    ///     Turns on the creation of lazy loading proxies.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         Note that this requires appropriate services to be available in the EF internal service provider. Normally this
    ///         will happen automatically, but if the application is controlling the service provider, then a call to
    ///         <see cref="ProxiesServiceCollectionExtensions.AddEntityFrameworkProxies" /> may be needed.
    ///     </para>
    ///     <para>
    ///         See <see href="https://aka.ms/efcore-docs-lazy-loading">Lazy loading</see> for more information and examples.
    ///     </para>
    /// </remarks>
    /// <typeparam name="TContext">The <see cref="DbContext" /> type.</typeparam>
    /// <param name="optionsBuilder">
    ///     The options builder, as passed to <see cref="DbContext.OnConfiguring" />
    ///     or exposed AddDbContext.
    /// </param>
    /// <param name="lazyLoadingProxiesOptionsAction">An optional action to allow additional proxy-specific configuration.</param>
    /// <returns>The same builder to allow method calls to be chained.</returns>
    public static DbContextOptionsBuilder<TContext> UseLazyLoadingProxies<TContext>(
        this DbContextOptionsBuilder<TContext> optionsBuilder,
        Action<LazyLoadingProxiesOptionsBuilder> lazyLoadingProxiesOptionsAction)
        where TContext : DbContext
        => (DbContextOptionsBuilder<TContext>)UseLazyLoadingProxies(
            (DbContextOptionsBuilder)optionsBuilder, lazyLoadingProxiesOptionsAction);

    /// <summary>
    ///     Creates a proxy instance for an entity type if proxy creation has been turned on.
    /// </summary>
    /// <param name="context">The <see cref="DbContext" />.</param>
    /// <param name="entityType">The entity type for which a proxy is needed.</param>
    /// <param name="constructorArguments">Arguments to pass to the entity type constructor.</param>
    /// <returns>The proxy instance.</returns>
    public static object CreateProxy(
        this DbContext context,
        Type entityType,
        params object[] constructorArguments)
    {
        Check.NotNull(context, nameof(context));
        Check.NotNull(entityType, nameof(entityType));
        Check.NotNull(constructorArguments, nameof(constructorArguments));

        return context.GetInfrastructure().CreateProxy(entityType, constructorArguments);
    }

    /// <summary>
    ///     Creates a proxy instance for an entity type if proxy creation has been turned on.
    /// </summary>
    /// <typeparam name="TEntity">The entity type for which a proxy is needed.</typeparam>
    /// <param name="context">The <see cref="DbContext" />.</param>
    /// <param name="constructorArguments">Arguments to pass to the entity type constructor.</param>
    /// <returns>The proxy instance.</returns>
    public static TEntity CreateProxy<TEntity>(
        this DbContext context,
        params object[] constructorArguments)
        => CreateProxy<TEntity>(context, null, constructorArguments);

    /// <summary>
    ///     Creates a proxy instance for an entity type if proxy creation has been turned on.
    /// </summary>
    /// <typeparam name="TEntity">The entity type for which a proxy is needed.</typeparam>
    /// <param name="context">The <see cref="DbContext" />.</param>
    /// <param name="configureEntity">Called after the entity is created to set property values, etc.</param>
    /// <param name="constructorArguments">Arguments to pass to the entity type constructor.</param>
    /// <returns>The proxy instance.</returns>
    public static TEntity CreateProxy<TEntity>(
        this DbContext context,
        Action<TEntity>? configureEntity,
        params object[] constructorArguments)
    {
        var entity = (TEntity)context.CreateProxy(typeof(TEntity), constructorArguments);

        configureEntity?.Invoke(entity);

        return entity;
    }

    /// <summary>
    ///     Creates a proxy instance for an entity type if proxy creation has been turned on.
    /// </summary>
    /// <typeparam name="TEntity">The entity type for which a proxy is needed.</typeparam>
    /// <param name="set">The <see cref="DbSet{TEntity}" />.</param>
    /// <param name="constructorArguments">Arguments to pass to the entity type constructor.</param>
    /// <returns>The proxy instance.</returns>
    public static TEntity CreateProxy<TEntity>(
        this DbSet<TEntity> set,
        params object[] constructorArguments)
        where TEntity : class
        => CreateProxy(set, null, constructorArguments);

    /// <summary>
    ///     Creates a proxy instance for an entity type if proxy creation has been turned on.
    /// </summary>
    /// <typeparam name="TEntity">The entity type for which a proxy is needed.</typeparam>
    /// <param name="set">The <see cref="DbSet{TEntity}" />.</param>
    /// <param name="configureEntity">Called after the entity is created to set property values, etc.</param>
    /// <param name="constructorArguments">Arguments to pass to the entity type constructor.</param>
    /// <returns>The proxy instance.</returns>
    public static TEntity CreateProxy<TEntity>(
        this DbSet<TEntity> set,
        Action<TEntity>? configureEntity,
        params object[] constructorArguments)
        where TEntity : class
    {
        Check.NotNull(set, nameof(set));
        Check.NotNull(constructorArguments, nameof(constructorArguments));

        var entity = (TEntity)set.GetInfrastructure().CreateProxy(set.EntityType, constructorArguments);

        configureEntity?.Invoke(entity);

        return entity;
    }

    private static object CreateProxy(
        this IServiceProvider serviceProvider,
        IEntityType entityType,
        params object[] constructorArguments)
    {
        CheckProxyOptions(serviceProvider, entityType.DisplayName());

        return serviceProvider.GetRequiredService<IProxyFactory>().CreateProxy(
            serviceProvider.GetRequiredService<ICurrentDbContext>().Context,
            entityType,
            constructorArguments);
    }

    private static object CreateProxy(
        this IServiceProvider serviceProvider,
        Type entityType,
        params object[] constructorArguments)
    {
        CheckProxyOptions(serviceProvider, entityType.ShortDisplayName());

        return serviceProvider.GetRequiredService<IProxyFactory>().Create(
            serviceProvider.GetRequiredService<ICurrentDbContext>().Context,
            entityType,
            constructorArguments);
    }

    private static void CheckProxyOptions(IServiceProvider serviceProvider, string entityTypeName)
    {
        var options = serviceProvider.GetRequiredService<IDbContextOptions>().FindExtension<ProxiesOptionsExtension>();

        if (options?.UseProxies != true)
        {
            throw new InvalidOperationException(ProxiesStrings.ProxiesNotEnabled(entityTypeName));
        }
    }
}
