// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.ComponentModel;
using Microsoft.EntityFrameworkCore.Diagnostics.Internal;
using Microsoft.Extensions.Caching.Memory;

namespace Microsoft.EntityFrameworkCore;

/// <summary>
///     Provides a simple API surface for configuring <see cref="DbContextOptions" />. Databases (and other extensions)
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
public class DbContextOptionsBuilder : IDbContextOptionsBuilderInfrastructure
{
    private DbContextOptions _options;

    /// <summary>
    ///     Initializes a new instance of the <see cref="DbContextOptionsBuilder" /> class with no options set.
    /// </summary>
    public DbContextOptionsBuilder()
        : this(new DbContextOptions<DbContext>())
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="DbContextOptionsBuilder" /> class to further configure
    ///     a given <see cref="DbContextOptions" />.
    /// </summary>
    /// <param name="options">The options to be configured.</param>
    public DbContextOptionsBuilder(DbContextOptions options)
    {
        Check.NotNull(options, nameof(options));

        _options = options;
    }

    /// <summary>
    ///     Gets the options being configured.
    /// </summary>
    public virtual DbContextOptions Options
        => _options;

    /// <summary>
    ///     Gets a value indicating whether any options have been configured.
    /// </summary>
    /// <remarks>
    ///     This can be useful when you have overridden <see cref="DbContext.OnConfiguring(DbContextOptionsBuilder)" /> to configure
    ///     the context, but in some cases you also externally provide options via the context constructor. This property can be
    ///     used to determine if the options have already been set, and skip some or all of the logic in
    ///     <see cref="DbContext.OnConfiguring(DbContextOptionsBuilder)" />.
    /// </remarks>
    public virtual bool IsConfigured
        => _options.Extensions.Any(e => e.Info.IsDatabaseProvider);

    /// <summary>
    ///     Sets the model to be used for the context. If the model is set, then <see cref="DbContext.OnModelCreating(ModelBuilder)" />
    ///     will not be run.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         If setting an externally created model <see cref="ModelBuilder.FinalizeModel()" /> should be called first.
    ///     </para>
    ///     <para>
    ///         See <see href="https://aka.ms/efcore-docs-dbcontext-options">Using DbContextOptions</see> and
    ///         <see href="https://aka.ms/efcore-docs-modeling">Model Building</see> for more information and examples.
    ///     </para>
    /// </remarks>
    /// <param name="model">The model to be used.</param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public virtual DbContextOptionsBuilder UseModel(IModel model)
        => WithOption(e => e.WithModel(Check.NotNull(model, nameof(model))));

    /// <summary>
    ///     Sets the <see cref="ILoggerFactory" /> that will be used to create <see cref="ILogger" /> instances
    ///     for logging done by this context.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         There is no need to call this method when using one of the 'AddDbContext' methods, including 'AddDbContextPool'.
    ///         These methods ensure that the <see cref="ILoggerFactory" /> used by EF is obtained from the application service provider.
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
    public virtual DbContextOptionsBuilder UseLoggerFactory(ILoggerFactory? loggerFactory)
        => WithOption(e => e.WithLoggerFactory(loggerFactory));

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
    public virtual DbContextOptionsBuilder LogTo(
        Action<string> action,
        LogLevel minimumLevel = LogLevel.Debug,
        DbContextLoggerOptions? options = null)
        => LogTo(action, (_, l) => l >= minimumLevel, options);

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
    public virtual DbContextOptionsBuilder LogTo(
        Action<string> action,
        IEnumerable<EventId> events,
        LogLevel minimumLevel = LogLevel.Debug,
        DbContextLoggerOptions? options = null)
    {
        Check.NotNull(events, nameof(events));

        var eventsArray = events.ToArray();

        return eventsArray.Length switch
        {
            0 => this,
            1 => LogTo(action, (eventId, level) => level >= minimumLevel && eventId == eventsArray[0], options),
            < 6 => LogTo(action, (eventId, level) => level >= minimumLevel && eventsArray.Contains(eventId), options),
            _ => LogTo(action, (eventId, level) => level >= minimumLevel && eventsArray.ToHashSet().Contains(eventId), options)
        };
    }

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
    public virtual DbContextOptionsBuilder LogTo(
        Action<string> action,
        IEnumerable<string> categories,
        LogLevel minimumLevel = LogLevel.Debug,
        DbContextLoggerOptions? options = null)
    {
        Check.NotNull(categories, nameof(categories));

        var categoriesArray = categories.ToArray();

        if (categoriesArray.Length == 0)
        {
            return this;
        }

        if (categoriesArray.Length != 1)
        {
            // One category is common, but even when there are more the number should be low because
            // the number of available categories is low. So no HashSet here.
            return LogTo(
                action,
                (eventId, level) =>
                {
                    if (level >= minimumLevel)
                    {
                        for (var i = 0; i < categoriesArray.Length; i++)
                        {
                            if (eventId.Name!.StartsWith(categoriesArray[i], StringComparison.OrdinalIgnoreCase))
                            {
                                return true;
                            }
                        }
                    }

                    return false;
                },
                options);
        }

        var singleCategory = categoriesArray[0];
        return LogTo(
            action,
            (eventId, level) => level >= minimumLevel
                && eventId.Name!.StartsWith(singleCategory, StringComparison.OrdinalIgnoreCase),
            options);
    }

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
    public virtual DbContextOptionsBuilder LogTo(
        Action<string> action,
        Func<EventId, LogLevel, bool> filter,
        DbContextLoggerOptions? options = null)
    {
        Check.NotNull(action, nameof(action));
        Check.NotNull(filter, nameof(filter));

        return LogTo(new FormattingDbContextLogger(action, filter, options ?? DbContextLoggerOptions.DefaultWithLocalTime));
    }

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
    public virtual DbContextOptionsBuilder LogTo(
        Func<EventId, LogLevel, bool> filter,
        Action<EventData> logger)
    {
        Check.NotNull(logger, nameof(logger));
        Check.NotNull(filter, nameof(filter));

        return LogTo(new DelegatingDbContextLogger(logger, filter));
    }

    private DbContextOptionsBuilder LogTo(IDbContextLogger logger)
        => WithOption(e => e.WithDbContextLogger(logger));

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
    public virtual DbContextOptionsBuilder EnableThreadSafetyChecks(bool enableChecks = true)
        => WithOption(e => e.WithThreadSafetyChecksEnabled(enableChecks));

    /// <summary>
    ///     Enables detailed errors when handling of data value exceptions that occur during processing of store query results. Such errors
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
    public virtual DbContextOptionsBuilder EnableDetailedErrors(bool detailedErrorsEnabled = true)
        => WithOption(e => e.WithDetailedErrorsEnabled(detailedErrorsEnabled));

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
    public virtual DbContextOptionsBuilder UseMemoryCache(IMemoryCache? memoryCache)
        => WithOption(e => e.WithMemoryCache(memoryCache));

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
    public virtual DbContextOptionsBuilder UseInternalServiceProvider(IServiceProvider? serviceProvider)
        => WithOption(e => e.WithInternalServiceProvider(serviceProvider));

    /// <summary>
    ///     Sets the <see cref="IServiceProvider" /> from which application services will be obtained. This
    ///     is done automatically when using 'AddDbContext' or 'AddDbContextPool',
    ///     so it is rare that this method needs to be called.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         See <see href="https://aka.ms/efcore-docs-dbcontext-options">Using DbContextOptions</see> for more information and examples.
    ///     </para>
    /// </remarks>
    /// <param name="serviceProvider">The service provider to be used.</param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public virtual DbContextOptionsBuilder UseApplicationServiceProvider(IServiceProvider? serviceProvider)
        => WithOption(e => e.WithApplicationServiceProvider(serviceProvider));

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
    public virtual DbContextOptionsBuilder UseRootApplicationServiceProvider(IServiceProvider? rootServiceProvider)
        => WithOption(e => e.WithRootApplicationServiceProvider(rootServiceProvider));

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
    public virtual DbContextOptionsBuilder UseRootApplicationServiceProvider()
        => WithOption(e => e.WithRootApplicationServiceProvider());

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
    /// <param name="sensitiveDataLoggingEnabled">If <see langword="true" />, then sensitive data is logged.</param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public virtual DbContextOptionsBuilder EnableSensitiveDataLogging(bool sensitiveDataLoggingEnabled = true)
        => WithOption(e => e.WithSensitiveDataLoggingEnabled(sensitiveDataLoggingEnabled));

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
    public virtual DbContextOptionsBuilder EnableServiceProviderCaching(bool cacheServiceProvider = true)
        => WithOption(e => e.WithServiceProviderCachingEnabled(cacheServiceProvider));

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
    ///         The default value is <see cref="QueryTrackingBehavior.TrackAll" />. This means
    ///         the change tracker will keep track of changes for all entities that are returned from a LINQ query.
    ///     </para>
    ///     <para>
    ///         See <see href="https://aka.ms/efcore-docs-dbcontext-options">Using DbContextOptions</see> and
    ///         <see href="https://aka.ms/efcore-docs-query">Querying data with EF Core</see> for more information and examples.
    ///     </para>
    /// </remarks>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public virtual DbContextOptionsBuilder UseQueryTrackingBehavior(QueryTrackingBehavior queryTrackingBehavior)
        => WithOption(e => e.WithQueryTrackingBehavior(queryTrackingBehavior));

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
    public virtual DbContextOptionsBuilder ConfigureWarnings(
        Action<WarningsConfigurationBuilder> warningsConfigurationBuilderAction)
    {
        Check.NotNull(warningsConfigurationBuilderAction, nameof(warningsConfigurationBuilderAction));

        warningsConfigurationBuilderAction(new WarningsConfigurationBuilder(this));

        return this;
    }

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
    public virtual DbContextOptionsBuilder ReplaceService<TService, TImplementation>()
        where TImplementation : TService
        => WithOption(e => e.WithReplacedService(typeof(TService), typeof(TImplementation)));

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
    public virtual DbContextOptionsBuilder ReplaceService<TService, TCurrentImplementation, TNewImplementation>()
        where TCurrentImplementation : TService
        where TNewImplementation : TService
        => WithOption(e => e.WithReplacedService(typeof(TService), typeof(TNewImplementation), typeof(TCurrentImplementation)));

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
    public virtual DbContextOptionsBuilder AddInterceptors(IEnumerable<IInterceptor> interceptors)
    {
        Check.NotNull(interceptors, nameof(interceptors));

        var singletonInterceptors = interceptors.OfType<ISingletonInterceptor>().ToList();
        var builder = this;
        if (singletonInterceptors.Count > 0)
        {
            builder = WithOption(e => e.WithSingletonInterceptors(singletonInterceptors));
        }

        return builder.WithOption(e => e.WithInterceptors(interceptors));
    }

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
    public virtual DbContextOptionsBuilder AddInterceptors(params IInterceptor[] interceptors)
        => AddInterceptors((IEnumerable<IInterceptor>)interceptors);

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
    public virtual DbContextOptionsBuilder ConfigureLoggingCacheTime(TimeSpan timeSpan)
        => WithOption(e => e.WithLoggingCacheTime(timeSpan));

    /// <summary>
    ///     Adds the given extension to the options. If an existing extension of the same type already exists, it will be replaced.
    /// </summary>
    /// <remarks>
    ///     This method is intended for use by extension methods to configure the context. It is not intended to be used in
    ///     application code.
    /// </remarks>
    /// <typeparam name="TExtension">The type of extension to be added.</typeparam>
    /// <param name="extension">The extension to be added.</param>
    void IDbContextOptionsBuilderInfrastructure.AddOrUpdateExtension<TExtension>(TExtension extension)
        => _options = _options.WithExtension(extension);

    private DbContextOptionsBuilder WithOption(Func<CoreOptionsExtension, CoreOptionsExtension> withFunc)
    {
        ((IDbContextOptionsBuilderInfrastructure)this).AddOrUpdateExtension(
            withFunc(Options.FindExtension<CoreOptionsExtension>() ?? new CoreOptionsExtension()));

        return this;
    }

    #region Hidden System.Object members

    /// <summary>
    ///     Returns a string that represents the current object.
    /// </summary>
    /// <returns>A string that represents the current object.</returns>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public override string? ToString()
        => base.ToString();

    /// <summary>
    ///     Determines whether the specified object is equal to the current object.
    /// </summary>
    /// <param name="obj">The object to compare with the current object.</param>
    /// <returns><see langword="true" /> if the specified object is equal to the current object; otherwise, <see langword="false" />.</returns>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public override bool Equals(object? obj)
        => base.Equals(obj);

    /// <summary>
    ///     Serves as the default hash function.
    /// </summary>
    /// <returns>A hash code for the current object.</returns>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public override int GetHashCode()
        => base.GetHashCode();

    #endregion
}
