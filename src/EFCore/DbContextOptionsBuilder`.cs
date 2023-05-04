// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.Extensions.Caching.Memory;

namespace Microsoft.EntityFrameworkCore;

/// <summary>
///     Provides a simple API surface for configuring <see cref="DbContextOptions{TContext}" />. Databases (and other extensions)
///     typically define extension methods on this object that allow you to configure the database connection (and other
///     options) to be used for a context.
/// </summary>
/// <remarks>
///     <para>
///         You can use <see cref="DbContextOptionsBuilder" /> to configure a context by overriding
///         <see cref="DbContext.OnConfiguring(DbContextOptionsBuilder)" /> or creating a <see cref="DbContextOptions" />
///         externally and passing it to the context constructor.
///     </para>
///     <para>
///         See <see href="https://aka.ms/efcore-docs-dbcontext-options">Using DbContextOptions</see> for more information and examples.
///     </para>
/// </remarks>
/// <typeparam name="TContext">The type of context to be configured.</typeparam>
public class DbContextOptionsBuilder<TContext> : DbContextOptionsBuilder
    where TContext : DbContext
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="DbContextOptionsBuilder{TContext}" /> class with no options set.
    /// </summary>
    public DbContextOptionsBuilder()
        : this(new DbContextOptions<TContext>())
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="DbContextOptionsBuilder{TContext}" /> class to further configure
    ///     a given <see cref="DbContextOptions" />.
    /// </summary>
    /// <param name="options">The options to be configured.</param>
    public DbContextOptionsBuilder(DbContextOptions<TContext> options)
        : base(options)
    {
    }

    /// <summary>
    ///     Gets the options being configured.
    /// </summary>
    public new virtual DbContextOptions<TContext> Options
        => (DbContextOptions<TContext>)base.Options;

    /// <summary>
    ///     Sets the model to be used for the context. If the model is set, then <see cref="DbContext.OnModelCreating(ModelBuilder)" />
    ///     will not be run.
    /// </summary>
    /// <param name="model">The model to be used.</param>
    /// <remarks>
    ///     <para>
    ///         If setting an externally created model <see cref="ModelBuilder.FinalizeModel()" /> should be called first.
    ///     </para>
    ///     <para>
    ///         See <see href="https://aka.ms/efcore-docs-dbcontext-options">Using DbContextOptions</see> and
    ///         <see href="https://aka.ms/efcore-docs-modeling">Model Building</see> for more information and examples.
    ///     </para>
    /// </remarks>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public new virtual DbContextOptionsBuilder<TContext> UseModel(IModel model)
        => (DbContextOptionsBuilder<TContext>)base.UseModel(model);

    /// <summary>
    ///     Sets the <see cref="ILoggerFactory" /> that will be used to create <see cref="ILogger" /> instances
    ///     for logging done by this context.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         There is no need to call this method when using one of the <see cref="O:EntityFrameworkServiceCollectionExtensions.AddDbContext" />
    ///         methods. 'AddDbContext' will ensure that the <see cref="ILoggerFactory" /> used by EF is obtained from the
    ///         application service provider.
    ///     </para>
    ///     <para>
    ///         This method cannot be used if the application is setting the internal service provider
    ///         through a call to <see cref="UseInternalServiceProvider" />. In this case, the <see cref="ILoggerFactory" />
    ///         should be configured directly in that service provider.
    ///     </para>
    ///     <para>
    ///         See <see href="https://aka.ms/efcore-docs-dbcontext-options">Using DbContextOptions</see> and
    ///         <see href="https://aka.ms/efcore-docs-logging">Logging</see> for more information and examples.
    ///     </para>
    /// </remarks>
    /// <param name="loggerFactory">The logger factory to be used.</param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public new virtual DbContextOptionsBuilder<TContext> UseLoggerFactory(ILoggerFactory? loggerFactory)
        => (DbContextOptionsBuilder<TContext>)base.UseLoggerFactory(loggerFactory);

    /// <summary>
    ///     Logs using the supplied action. For example, use <c>optionsBuilder.LogTo(Console.WriteLine)</c> to
    ///     log to the console.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         This overload allows the minimum level of logging and the log formatting to be controlled.
    ///         Use the
    ///         <see
    ///             cref="LogTo(Action{string},System.Collections.Generic.IEnumerable{Microsoft.Extensions.Logging.EventId},LogLevel,DbContextLoggerOptions?)" />
    ///         overload to log only specific events.
    ///         Use the <see cref="LogTo(Action{string},IEnumerable{string},LogLevel,DbContextLoggerOptions?)" />
    ///         overload to log only events in specific categories.
    ///         Use the <see cref="LogTo(Action{string},Func{EventId,LogLevel,bool},DbContextLoggerOptions?)" />
    ///         overload to use a custom filter for events.
    ///         Use the <see cref="LogTo(Func{EventId,LogLevel,bool},Action{EventData})" /> overload to log to a fully custom logger.
    ///     </para>
    ///     <para>
    ///         See <see href="https://aka.ms/efcore-docs-dbcontext-options">Using DbContextOptions</see> and
    ///         <see href="https://aka.ms/efcore-docs-logging">Logging</see> for more information and examples.
    ///     </para>
    /// </remarks>
    /// <param name="action">Delegate called when there is a message to log.</param>
    /// <param name="minimumLevel">The minimum level of logging event to log. Defaults to <see cref="LogLevel.Debug" /></param>
    /// <param name="options">
    ///     Formatting options for log messages. Passing null (the default) means use <see cref="DbContextLoggerOptions.DefaultWithLocalTime" />
    /// </param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public new virtual DbContextOptionsBuilder<TContext> LogTo(
        Action<string> action,
        LogLevel minimumLevel = LogLevel.Debug,
        DbContextLoggerOptions? options = null)
        => (DbContextOptionsBuilder<TContext>)base.LogTo(action, minimumLevel, options);

    /// <summary>
    ///     Logs the specified events using the supplied action. For example, use
    ///     <c>optionsBuilder.LogTo(Console.WriteLine, new[] { CoreEventId.ContextInitialized })</c> to log the
    ///     <see cref="CoreEventId.ContextInitialized" /> event to the console.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         Use the <see cref="LogTo(Action{string},LogLevel,DbContextLoggerOptions?)" /> overload for default logging of
    ///         all events.
    ///         Use the <see cref="LogTo(Action{string},IEnumerable{string},LogLevel,DbContextLoggerOptions?)" />
    ///         overload to log only events in specific categories.
    ///         Use the <see cref="LogTo(Action{string},Func{EventId,LogLevel,bool},DbContextLoggerOptions?)" />
    ///         overload to use a custom filter for events.
    ///         Use the <see cref="LogTo(Func{EventId,LogLevel,bool},Action{EventData})" /> overload to log to a fully custom logger.
    ///     </para>
    ///     <para>
    ///         See <see href="https://aka.ms/efcore-docs-dbcontext-options">Using DbContextOptions</see> and
    ///         <see href="https://aka.ms/efcore-docs-logging">Logging</see> for more information and examples.
    ///     </para>
    /// </remarks>
    /// <param name="action">Delegate called when there is a message to log.</param>
    /// <param name="events">The <see cref="EventId" /> of each event to log.</param>
    /// <param name="minimumLevel">The minimum level of logging event to log. Defaults to <see cref="LogLevel.Debug" /></param>
    /// <param name="options">
    ///     Formatting options for log messages. Passing null (the default) means use <see cref="DbContextLoggerOptions.DefaultWithLocalTime" />
    /// </param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public new virtual DbContextOptionsBuilder<TContext> LogTo(
        Action<string> action,
        IEnumerable<EventId> events,
        LogLevel minimumLevel = LogLevel.Debug,
        DbContextLoggerOptions? options = null)
        => (DbContextOptionsBuilder<TContext>)base.LogTo(action, events, minimumLevel, options);

    /// <summary>
    ///     Logs all events in the specified categories using the supplied action. For example, use
    ///     <c>optionsBuilder.LogTo(Console.WriteLine, new[] { DbLoggerCategory.Infrastructure.Name })</c> to log all
    ///     events in the <see cref="DbLoggerCategory.Infrastructure" /> category.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         Use the <see cref="LogTo(Action{string},LogLevel,DbContextLoggerOptions?)" /> overload for default logging of
    ///         all events.
    ///         Use the <see cref="LogTo(Action{string},IEnumerable{EventId},LogLevel,DbContextLoggerOptions?)" />
    ///         overload to log only specific events.
    ///         Use the <see cref="LogTo(Action{string},Func{EventId,LogLevel,bool},DbContextLoggerOptions?)" />
    ///         overload to use a custom filter for events.
    ///         Use the <see cref="LogTo(Func{EventId,LogLevel,bool},Action{EventData})" /> overload to log to a fully custom logger.
    ///     </para>
    ///     <para>
    ///         See <see href="https://aka.ms/efcore-docs-dbcontext-options">Using DbContextOptions</see> and
    ///         <see href="https://aka.ms/efcore-docs-logging">Logging</see> for more information and examples.
    ///     </para>
    /// </remarks>
    /// <param name="action">Delegate called when there is a message to log.</param>
    /// <param name="categories">The <see cref="DbLoggerCategory" /> of each event to log.</param>
    /// <param name="minimumLevel">The minimum level of logging event to log. Defaults to <see cref="LogLevel.Debug" /></param>
    /// <param name="options">
    ///     Formatting options for log messages. Passing null (the default) means use <see cref="DbContextLoggerOptions.DefaultWithLocalTime" />
    /// </param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public new virtual DbContextOptionsBuilder<TContext> LogTo(
        Action<string> action,
        IEnumerable<string> categories,
        LogLevel minimumLevel = LogLevel.Debug,
        DbContextLoggerOptions? options = null)
        => (DbContextOptionsBuilder<TContext>)base.LogTo(action, categories, minimumLevel, options);

    /// <summary>
    ///     Logs events filtered by a supplied custom filter delegate. The filter should return true to
    ///     log a message, or false to filter it out of the log.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         Use the <see cref="LogTo(Action{string},LogLevel,DbContextLoggerOptions?)" /> overload for default logging of
    ///         all events.
    ///         Use the <see cref="LogTo(Action{string},IEnumerable{EventId},LogLevel,DbContextLoggerOptions?)" />
    ///         Use the <see cref="LogTo(Action{string},IEnumerable{string},LogLevel,DbContextLoggerOptions?)" />
    ///         overload to log only events in specific categories.
    ///         Use the <see cref="LogTo(Func{EventId,LogLevel,bool},Action{EventData})" /> overload to log to a fully custom logger.
    ///     </para>
    ///     <para>
    ///         See <see href="https://aka.ms/efcore-docs-dbcontext-options">Using DbContextOptions</see> and
    ///         <see href="https://aka.ms/efcore-docs-logging">Logging</see> for more information and examples.
    ///     </para>
    /// </remarks>
    /// <param name="action">Delegate called when there is a message to log.</param>
    /// <param name="filter">Delegate that returns true to log the message or false to ignore it.</param>
    /// <param name="options">
    ///     Formatting options for log messages. Passing null (the default) means use <see cref="DbContextLoggerOptions.DefaultWithLocalTime" />
    /// </param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public new virtual DbContextOptionsBuilder<TContext> LogTo(
        Action<string> action,
        Func<EventId, LogLevel, bool> filter,
        DbContextLoggerOptions? options = null)
        => (DbContextOptionsBuilder<TContext>)base.LogTo(action, filter, options);

    /// <summary>
    ///     Logs events to a custom logger delegate filtered by a custom filter delegate. The filter should return true to
    ///     log a message, or false to filter it out of the log.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         Use the <see cref="LogTo(Action{string},LogLevel,DbContextLoggerOptions?)" /> overload for default logging of
    ///         all events.
    ///         Use the <see cref="LogTo(Action{string},IEnumerable{EventId},LogLevel,DbContextLoggerOptions?)" />
    ///         Use the <see cref="LogTo(Action{string},IEnumerable{string},LogLevel,DbContextLoggerOptions?)" />
    ///         overload to log only events in specific categories.
    ///         Use the <see cref="LogTo(Action{string},Func{EventId,LogLevel,bool},DbContextLoggerOptions?)" />
    ///         overload to use a custom filter for events.
    ///     </para>
    ///     <para>
    ///         See <see href="https://aka.ms/efcore-docs-dbcontext-options">Using DbContextOptions</see> and
    ///         <see href="https://aka.ms/efcore-docs-logging">Logging</see> for more information and examples.
    ///     </para>
    /// </remarks>
    /// <param name="filter">Delegate that returns true to log the message or false to ignore it.</param>
    /// <param name="logger">Delegate called when there is a message to log.</param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    // Filter comes first, logger second, otherwise it's hard to get the correct overload to resolve
    public new virtual DbContextOptionsBuilder<TContext> LogTo(
        Func<EventId, LogLevel, bool> filter,
        Action<EventData> logger)
        => (DbContextOptionsBuilder<TContext>)base.LogTo(filter, logger);

    /// <summary>
    ///     Disables concurrency detection, which detects many cases of erroneous concurrent usage of a <see cref="DbContext" />
    ///     instance and causes an informative exception to be thrown. This provides a minor performance improvement, but if a
    ///     <see cref="DbContext" /> instance is used concurrently, the behavior will be undefined and the program may fail in
    ///     unpredictable ways.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         Only disable concurrency detection after confirming that the performance gains are considerable, and the application has
    ///         been thoroughly tested against concurrency bugs.
    ///     </para>
    ///     <para>
    ///         Note that if the application is setting the internal service provider through a call to
    ///         <see cref="UseInternalServiceProvider" />, then this option must configured the same way
    ///         for all uses of that service provider. Consider instead not calling <see cref="UseInternalServiceProvider" />
    ///         so that EF will manage the service providers and can create new instances as required.
    ///     </para>
    ///     <para>
    ///         See <see href="https://aka.ms/efcore-docs-dbcontext-options">Using DbContextOptions</see> for more information and examples.
    ///     </para>
    /// </remarks>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public new virtual DbContextOptionsBuilder<TContext> EnableThreadSafetyChecks(bool checksEnabled = true)
        => (DbContextOptionsBuilder<TContext>)base.EnableThreadSafetyChecks(checksEnabled);

    /// <summary>
    ///     Enables detailed errors when handling data value exceptions that occur during processing of store query results. Such errors
    ///     most often occur due to misconfiguration of entity properties. E.g. If a property is configured to be of type
    ///     'int', but the underlying data in the store is actually of type 'string', then an exception will be generated
    ///     at runtime during processing of the data value. When this option is enabled and a data error is encountered, the
    ///     generated exception will include details of the specific entity property that generated the error.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         Enabling this option incurs a small performance overhead during query execution.
    ///     </para>
    ///     <para>
    ///         Note that if the application is setting the internal service provider through a call to
    ///         <see cref="UseInternalServiceProvider" />, then this option must configured the same way
    ///         for all uses of that service provider. Consider instead not calling <see cref="UseInternalServiceProvider" />
    ///         so that EF will manage the service providers and can create new instances as required.
    ///     </para>
    ///     <para>
    ///         See <see href="https://aka.ms/efcore-docs-dbcontext-options">Using DbContextOptions</see> and
    ///         <see href="https://aka.ms/efcore-docs-logging">Logging</see> for more information and examples.
    ///     </para>
    /// </remarks>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public new virtual DbContextOptionsBuilder<TContext> EnableDetailedErrors(bool detailedErrorsEnabled = true)
        => (DbContextOptionsBuilder<TContext>)base.EnableDetailedErrors(detailedErrorsEnabled);

    /// <summary>
    ///     Sets the <see cref="IMemoryCache" /> to be used for query caching by this context.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         Note that changing the memory cache can cause EF to build a new internal service provider, which
    ///         may cause issues with performance. Generally it is expected that no more than one or two different
    ///         instances will be used for a given application.
    ///     </para>
    ///     <para>
    ///         This method cannot be used if the application is setting the internal service provider
    ///         through a call to <see cref="UseInternalServiceProvider" />. In this case, the <see cref="IMemoryCache" />
    ///         should be configured directly in that service provider.
    ///     </para>
    ///     <para>
    ///         See <see href="https://aka.ms/efcore-docs-dbcontext-options">Using DbContextOptions</see>  and
    ///         <see href="https://learn.microsoft.com/dotnet/core/extensions/caching">Caching in .NET</see> for more information.
    ///     </para>
    /// </remarks>
    /// <param name="memoryCache">The memory cache to be used.</param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public new virtual DbContextOptionsBuilder<TContext> UseMemoryCache(IMemoryCache? memoryCache)
        => (DbContextOptionsBuilder<TContext>)base.UseMemoryCache(memoryCache);

    /// <summary>
    ///     Sets the <see cref="IServiceProvider" /> that the context should resolve all of its services from. EF will
    ///     create and manage a service provider if none is specified.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         The service provider must contain all the services required by Entity Framework (and the database being
    ///         used). The Entity Framework services can be registered using an extension method on <see cref="IServiceCollection" />.
    ///         For example, the Microsoft SQL Server provider includes an AddEntityFrameworkSqlServer() method to add
    ///         the required services.
    ///     </para>
    ///     <para>
    ///         If the <see cref="IServiceProvider" /> has a <see cref="DbContextOptions" /> or
    ///         <see cref="DbContextOptions{TContext}" /> registered, then this will be used as the options for
    ///         this context instance.
    ///     </para>
    ///     <para>
    ///         See <see href="https://aka.ms/efcore-docs-dbcontext-options">Using DbContextOptions</see> for more information and examples.
    ///     </para>
    /// </remarks>
    /// <param name="serviceProvider">The service provider to be used.</param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public new virtual DbContextOptionsBuilder<TContext> UseInternalServiceProvider(IServiceProvider? serviceProvider)
        => (DbContextOptionsBuilder<TContext>)base.UseInternalServiceProvider(serviceProvider);

    /// <summary>
    ///     Sets the <see cref="IServiceProvider" /> from which application services will be obtained. This
    ///     is done automatically when using 'AddDbContext', so it is rare that this method needs to be called.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         See <see href="https://aka.ms/efcore-docs-dbcontext-options">Using DbContextOptions</see> for more information and examples.
    ///     </para>
    /// </remarks>
    /// <param name="serviceProvider">The service provider to be used.</param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public new virtual DbContextOptionsBuilder<TContext> UseApplicationServiceProvider(IServiceProvider? serviceProvider)
        => (DbContextOptionsBuilder<TContext>)base.UseApplicationServiceProvider(serviceProvider);

    /// <summary>
    ///     Sets the root <see cref="IServiceProvider" /> from which singleton application services can be obtained from singleton
    ///     internal services.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         This is an advanced option that is rarely needed by normal applications. Calling this method will result in a new internal
    ///         service provider being created for every different root application service provider.
    ///     </para>
    ///     <para>
    ///         See <see href="https://aka.ms/efcore-docs-dbcontext-options">Using DbContextOptions</see> for more information and examples.
    ///     </para>
    /// </remarks>
    /// <param name="rootServiceProvider">The service provider to be used.</param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public new virtual DbContextOptionsBuilder<TContext> UseRootApplicationServiceProvider(IServiceProvider? rootServiceProvider)
        => (DbContextOptionsBuilder<TContext>)base.UseRootApplicationServiceProvider(rootServiceProvider);

    /// <summary>
    ///     Resolves the root <see cref="IServiceProvider" /> from from the scoped application service provider. The root provider can
    ///     be used to obtain singleton application services from singleton internal services.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         This is an advanced option that is rarely needed by normal applications. Calling this method will result in a new internal
    ///         service provider being created for every different root application service provider.
    ///     </para>
    ///     <para>
    ///         See <see href="https://aka.ms/efcore-docs-dbcontext-options">Using DbContextOptions</see> for more information and examples.
    ///     </para>
    /// </remarks>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public new virtual DbContextOptionsBuilder<TContext> UseRootApplicationServiceProvider()
        => (DbContextOptionsBuilder<TContext>)base.UseRootApplicationServiceProvider();

    /// <summary>
    ///     Enables application data to be included in exception messages, logging, etc. This can include the
    ///     values assigned to properties of your entity instances, parameter values for commands being sent
    ///     to the database, and other such data. You should only enable this flag if you have the appropriate
    ///     security measures in place based on the sensitivity of this data.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         Note that if the application is setting the internal service provider through a call to
    ///         <see cref="UseInternalServiceProvider" />, then this option must configured the same way
    ///         for all uses of that service provider. Consider instead not calling <see cref="UseInternalServiceProvider" />
    ///         so that EF will manage the service providers and can create new instances as required.
    ///     </para>
    ///     <para>
    ///         See <see href="https://aka.ms/efcore-docs-dbcontext-options">Using DbContextOptions</see> and
    ///         <see href="https://aka.ms/efcore-docs-logging">Logging</see> for more information and examples.
    ///     </para>
    /// </remarks>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public new virtual DbContextOptionsBuilder<TContext> EnableSensitiveDataLogging(bool sensitiveDataLoggingEnabled = true)
        => (DbContextOptionsBuilder<TContext>)base.EnableSensitiveDataLogging(sensitiveDataLoggingEnabled);

    /// <summary>
    ///     Enables or disables caching of internal service providers. Disabling caching can
    ///     massively impact performance and should only be used in testing scenarios that
    ///     build many service providers for test isolation.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         Note that if the application is setting the internal service provider through a call to
    ///         <see cref="UseInternalServiceProvider" />, then setting this option will have no effect.
    ///     </para>
    ///     <para>
    ///         See <see href="https://aka.ms/efcore-docs-dbcontext-options">Using DbContextOptions</see> for more information and examples.
    ///     </para>
    /// </remarks>
    /// <param name="cacheServiceProvider">If <see langword="true" />, then the internal service provider is cached.</param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public new virtual DbContextOptionsBuilder<TContext> EnableServiceProviderCaching(bool cacheServiceProvider = true)
        => (DbContextOptionsBuilder<TContext>)base.EnableServiceProviderCaching(cacheServiceProvider);

    /// <summary>
    ///     Sets the tracking behavior for LINQ queries run against the context. Disabling change tracking
    ///     is useful for read-only scenarios because it avoids the overhead of setting up change tracking for each
    ///     entity instance. You should not disable change tracking if you want to manipulate entity instances and
    ///     persist those changes to the database using <see cref="DbContext.SaveChanges()" />.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         This method sets the default behavior for all contexts created with these options, but you can override this
    ///         behavior for a context instance using <see cref="ChangeTracker.QueryTrackingBehavior" /> or on individual
    ///         queries using the <see cref="EntityFrameworkQueryableExtensions.AsNoTracking{TEntity}(IQueryable{TEntity})" />
    ///         and <see cref="EntityFrameworkQueryableExtensions.AsTracking{TEntity}(IQueryable{TEntity})" /> methods.
    ///     </para>
    ///     <para>
    ///         The default value is <see cref="QueryTrackingBehavior.TrackAll" />. This means the
    ///         change tracker will keep track of changes for all entities that are returned from a LINQ query.
    ///     </para>
    ///     <para>
    ///         See <see href="https://aka.ms/efcore-docs-dbcontext-options">Using DbContextOptions</see> and
    ///         <see href="https://aka.ms/efcore-docs-query">Querying data with EF Core</see> for more information and examples.
    ///     </para>
    /// </remarks>
    public new virtual DbContextOptionsBuilder<TContext> UseQueryTrackingBehavior(QueryTrackingBehavior queryTrackingBehavior)
        => (DbContextOptionsBuilder<TContext>)base.UseQueryTrackingBehavior(queryTrackingBehavior);

    /// <summary>
    ///     Configures the runtime behavior of warnings generated by Entity Framework. You can set a default
    ///     behavior and behaviors for each warning type.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         Note that changing this configuration can cause EF to build a new internal service provider, which
    ///         may cause issues with performance. Generally it is expected that no more than one or two different
    ///         configurations will be used for a given application.
    ///     </para>
    ///     <para>
    ///         Note that if the application is setting the internal service provider through a call to
    ///         <see cref="UseInternalServiceProvider" />, then this option must configured the same way
    ///         for all uses of that service provider. Consider instead not calling <see cref="UseInternalServiceProvider" />
    ///         so that EF will manage the service providers and can create new instances as required.
    ///     </para>
    ///     <para>
    ///         See <see href="https://aka.ms/efcore-docs-dbcontext-options">Using DbContextOptions</see> and
    ///         <see href="https://aka.ms/efcore-docs-logging">Logging</see> for more information and examples.
    ///     </para>
    /// </remarks>
    /// <param name="warningsConfigurationBuilderAction">
    ///     An action to configure the warning behavior.
    /// </param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public new virtual DbContextOptionsBuilder<TContext> ConfigureWarnings(
        Action<WarningsConfigurationBuilder> warningsConfigurationBuilderAction)
        => (DbContextOptionsBuilder<TContext>)base.ConfigureWarnings(warningsConfigurationBuilderAction);

    /// <summary>
    ///     Replaces all internal Entity Framework implementations of a service contract with a different
    ///     implementation.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         This method can only be used when EF is building and managing its internal service provider.
    ///         If the service provider is being built externally and passed to
    ///         <see cref="UseInternalServiceProvider" />, then replacement services should be configured on
    ///         that service provider before it is passed to EF.
    ///     </para>
    ///     <para>
    ///         The replacement service gets the same scope as the EF service that it is replacing.
    ///     </para>
    ///     <para>
    ///         See <see href="https://aka.ms/efcore-docs-dbcontext-options">Using DbContextOptions</see> for more information and examples.
    ///     </para>
    /// </remarks>
    /// <typeparam name="TService">The type (usually an interface) that defines the contract of the service to replace.</typeparam>
    /// <typeparam name="TImplementation">The new implementation type for the service.</typeparam>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public new virtual DbContextOptionsBuilder<TContext> ReplaceService<TService, TImplementation>()
        where TImplementation : TService
        => (DbContextOptionsBuilder<TContext>)base.ReplaceService<TService, TImplementation>();

    /// <summary>
    ///     Replaces the internal Entity Framework implementation of a specific implementation of a service contract
    ///     with a different implementation.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         This method is useful for replacing a single instance of services that can be legitimately registered
    ///         multiple times in the EF internal service provider.
    ///     </para>
    ///     <para>
    ///         This method can only be used when EF is building and managing its internal service provider.
    ///         If the service provider is being built externally and passed to
    ///         <see cref="UseInternalServiceProvider" />, then replacement services should be configured on
    ///         that service provider before it is passed to EF.
    ///     </para>
    ///     <para>
    ///         The replacement service gets the same scope as the EF service that it is replacing.
    ///     </para>
    ///     <para>
    ///         See <see href="https://aka.ms/efcore-docs-dbcontext-options">Using DbContextOptions</see> for more information and examples.
    ///     </para>
    /// </remarks>
    /// <typeparam name="TService">The type (usually an interface) that defines the contract of the service to replace.</typeparam>
    /// <typeparam name="TCurrentImplementation">The current implementation type for the service.</typeparam>
    /// <typeparam name="TNewImplementation">The new implementation type for the service.</typeparam>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public new virtual DbContextOptionsBuilder<TContext> ReplaceService<TService, TCurrentImplementation, TNewImplementation>()
        where TCurrentImplementation : TService
        where TNewImplementation : TService
        => (DbContextOptionsBuilder<TContext>)base.ReplaceService<TService, TCurrentImplementation, TNewImplementation>();

    /// <summary>
    ///     Adds <see cref="IInterceptor" /> instances to those registered on the context.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         Interceptors can be used to view, change, or suppress operations taken by Entity Framework.
    ///         See the specific implementations of <see cref="IInterceptor" /> for details. For example, 'IDbCommandInterceptor'.
    ///     </para>
    ///     <para>
    ///         A single interceptor instance can implement multiple different interceptor interfaces. It will be registered as
    ///         an interceptor for all interfaces that it implements.
    ///     </para>
    ///     <para>
    ///         Extensions can also register multiple <see cref="IInterceptor" />s in the internal service provider.
    ///         If both injected and application interceptors are found, then the injected interceptors are run in the
    ///         order that they are resolved from the service provider, and then the application interceptors are run
    ///         in the order that they were added to the context.
    ///     </para>
    ///     <para>
    ///         Calling this method multiple times will result in all interceptors in every call being added to the context.
    ///         Interceptors added in a previous call are not overridden by interceptors added in a later call.
    ///     </para>
    ///     <para>
    ///         See <see href="https://aka.ms/efcore-docs-dbcontext-options">Using DbContextOptions</see> and
    ///         <see href="https://aka.ms/efcore-docs-interceptors">EF Core interceptors</see> for more information and examples.
    ///     </para>
    /// </remarks>
    /// <param name="interceptors">The interceptors to add.</param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public new virtual DbContextOptionsBuilder<TContext> AddInterceptors(IEnumerable<IInterceptor> interceptors)
        => (DbContextOptionsBuilder<TContext>)base.AddInterceptors(interceptors);

    /// <summary>
    ///     Adds <see cref="IInterceptor" /> instances to those registered on the context.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         Interceptors can be used to view, change, or suppress operations taken by Entity Framework.
    ///         See the specific implementations of <see cref="IInterceptor" /> for details. For example, 'IDbCommandInterceptor'.
    ///     </para>
    ///     <para>
    ///         Extensions can also register multiple <see cref="IInterceptor" />s in the internal service provider.
    ///         If both injected and application interceptors are found, then the injected interceptors are run in the
    ///         order that they are resolved from the service provider, and then the application interceptors are run
    ///         in the order that they were added to the context.
    ///     </para>
    ///     <para>
    ///         Calling this method multiple times will result in all interceptors in every call being added to the context.
    ///         Interceptors added in a previous call are not overridden by interceptors added in a later call.
    ///     </para>
    ///     <para>
    ///         See <see href="https://aka.ms/efcore-docs-dbcontext-options">Using DbContextOptions</see> and
    ///         <see href="https://aka.ms/efcore-docs-interceptors">EF Core interceptors</see> for more information and examples.
    ///     </para>
    /// </remarks>
    /// <param name="interceptors">The interceptors to add.</param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public new virtual DbContextOptionsBuilder<TContext> AddInterceptors(params IInterceptor[] interceptors)
        => (DbContextOptionsBuilder<TContext>)base.AddInterceptors(interceptors);

    /// <summary>
    ///     Configures how long EF Core will cache logging configuration in certain high-performance paths. This makes
    ///     EF Core skip potentially costly logging checks, but means that runtime logging changes (e.g. registering a
    ///     new <see cref="DiagnosticListener" /> may not be taken into account right away).
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         Defaults to one second.
    ///     </para>
    ///     <para>
    ///         See <see href="https://aka.ms/efcore-docs-dbcontext-options">Using DbContextOptions</see> and
    ///         <see href="https://aka.ms/efcore-docs-logging">Logging</see> for more information and examples.
    ///     </para>
    /// </remarks>
    /// <param name="timeSpan">The maximum time period over which to skip logging checks before checking again.</param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public new virtual DbContextOptionsBuilder<TContext> ConfigureLoggingCacheTime(TimeSpan timeSpan)
        => (DbContextOptionsBuilder<TContext>)base.ConfigureLoggingCacheTime(timeSpan);
}
