// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Microsoft.EntityFrameworkCore
{
    /// <summary>
    ///     <para>
    ///         Provides a simple API surface for configuring <see cref="DbContextOptions{TContext}" />. Databases (and other extensions)
    ///         typically define extension methods on this object that allow you to configure the database connection (and other
    ///         options) to be used for a context.
    ///     </para>
    ///     <para>
    ///         You can use <see cref="DbContextOptionsBuilder" /> to configure a context by overriding
    ///         <see cref="DbContext.OnConfiguring(DbContextOptionsBuilder)" /> or creating a <see cref="DbContextOptions" />
    ///         externally and passing it to the context constructor.
    ///     </para>
    /// </summary>
    /// <typeparam name="TContext"> The type of context to be configured. </typeparam>
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
        /// <param name="options"> The options to be configured. </param>
        public DbContextOptionsBuilder([NotNull] DbContextOptions<TContext> options)
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
        /// <param name="model"> The model to be used. </param>
        /// <returns> The same builder instance so that multiple calls can be chained. </returns>
        public new virtual DbContextOptionsBuilder<TContext> UseModel([NotNull] IModel model)
            => (DbContextOptionsBuilder<TContext>)base.UseModel(model);

        /// <summary>
        ///     <para>
        ///         Sets the <see cref="ILoggerFactory" /> that will be used to create <see cref="ILogger" /> instances
        ///         for logging done by this context.
        ///     </para>
        ///     <para>
        ///         There is no need to call this method when using one of the <see cref="M:EntityFrameworkServiceCollectionExtensions.AddDbContext" />
        ///         methods. 'AddDbContext' will ensure that the <see cref="ILoggerFactory" /> used by EF is obtained from the
        ///         application service provider.
        ///     </para>
        ///     <para>
        ///         This method cannot be used if the application is setting the internal service provider
        ///         through a call to <see cref="UseInternalServiceProvider" />. In this case, the <see cref="ILoggerFactory" />
        ///         should be configured directly in that service provider.
        ///     </para>
        /// </summary>
        /// <param name="loggerFactory"> The logger factory to be used. </param>
        /// <returns> The same builder instance so that multiple calls can be chained. </returns>
        public new virtual DbContextOptionsBuilder<TContext> UseLoggerFactory([CanBeNull] ILoggerFactory loggerFactory)
            => (DbContextOptionsBuilder<TContext>)base.UseLoggerFactory(loggerFactory);

        /// <summary>
        ///     <para>
        ///         Logs using the supplied action. For example, use <c>optionsBuilder.LogTo(Console.WriteLine)</c> to
        ///         log to the console.
        ///     </para>
        ///     <para>
        ///         This overload allows the minimum level of logging and the log formatting to be controlled.
        ///         Use the
        ///         <see
        ///             cref="LogTo(Action{string},IEnumerable{EventId},LogLevel,DbContextLoggerOptions?)" />
        ///         overload to log only specific events.
        ///         Use the <see cref="LogTo(Action{string},IEnumerable{string},LogLevel,DbContextLoggerOptions?)" />
        ///         overload to log only events in specific categories.
        ///         Use the <see cref="LogTo(Action{string},Func{EventId,LogLevel,bool},DbContextLoggerOptions?)" />
        ///         overload to use a custom filter for events.
        ///         Use the <see cref="LogTo(Func{EventId,LogLevel,bool},Action{EventData})" /> overload to log to a fully custom logger.
        ///     </para>
        /// </summary>
        /// <param name="action"> Delegate called when there is a message to log. </param>
        /// <param name="minimumLevel"> The minimum level of logging event to log. Defaults to <see cref="LogLevel.Debug" /> </param>
        /// <param name="options">
        ///     Formatting options for log messages. Passing null (the default) means use <see cref="DbContextLoggerOptions.DefaultWithLocalTime" />
        /// </param>
        /// <returns> The same builder instance so that multiple calls can be chained. </returns>
        public new virtual DbContextOptionsBuilder<TContext> LogTo(
            [NotNull] Action<string> action,
            LogLevel minimumLevel = LogLevel.Debug,
            DbContextLoggerOptions? options = null)
            => (DbContextOptionsBuilder<TContext>)base.LogTo(action, minimumLevel, options);

        /// <summary>
        ///     <para>
        ///         Logs the specified events using the supplied action. For example, use
        ///         <c>optionsBuilder.LogTo(Console.WriteLine, new[] { CoreEventId.ContextInitialized })</c> to log the
        ///         <see cref="CoreEventId.ContextInitialized" /> event to the console.
        ///     </para>
        ///     <para>
        ///         Use the <see cref="LogTo(Action{string},LogLevel,DbContextLoggerOptions?)" /> overload for default logging of
        ///         all events.
        ///         Use the <see cref="LogTo(Action{string},IEnumerable{string},LogLevel,DbContextLoggerOptions?)" />
        ///         overload to log only events in specific categories.
        ///         Use the <see cref="LogTo(Action{string},Func{EventId,LogLevel,bool},DbContextLoggerOptions?)" />
        ///         overload to use a custom filter for events.
        ///         Use the <see cref="LogTo(Func{EventId,LogLevel,bool},Action{EventData})" /> overload to log to a fully custom logger.
        ///     </para>
        /// </summary>
        /// <param name="action"> Delegate called when there is a message to log. </param>
        /// <param name="events"> The <see cref="EventId" /> of each event to log. </param>
        /// <param name="minimumLevel"> The minimum level of logging event to log. Defaults to <see cref="LogLevel.Debug" /> </param>
        /// <param name="options">
        ///     Formatting options for log messages. Passing null (the default) means use <see cref="DbContextLoggerOptions.DefaultWithLocalTime" />
        /// </param>
        /// <returns> The same builder instance so that multiple calls can be chained. </returns>
        public new virtual DbContextOptionsBuilder<TContext> LogTo(
            [NotNull] Action<string> action,
            [NotNull] IEnumerable<EventId> events,
            LogLevel minimumLevel = LogLevel.Debug,
            DbContextLoggerOptions? options = null)
            => (DbContextOptionsBuilder<TContext>)base.LogTo(action, events, minimumLevel, options);

        /// <summary>
        ///     <para>
        ///         Logs all events in the specified categories using the supplied action. For example, use
        ///         <c>optionsBuilder.LogTo(Console.WriteLine, new[] { DbLoggerCategory.Infrastructure.Name })</c> to log all
        ///         events in the <see cref="DbLoggerCategory.Infrastructure" /> category.
        ///     </para>
        ///     <para>
        ///         Use the <see cref="LogTo(Action{string},LogLevel,DbContextLoggerOptions?)" /> overload for default logging of
        ///         all events.
        ///         Use the <see cref="LogTo(Action{string},IEnumerable{EventId},LogLevel,DbContextLoggerOptions?)" />
        ///         overload to log only specific events.
        ///         Use the <see cref="LogTo(Action{string},Func{EventId,LogLevel,bool},DbContextLoggerOptions?)" />
        ///         overload to use a custom filter for events.
        ///         Use the <see cref="LogTo(Func{EventId,LogLevel,bool},Action{EventData})" /> overload to log to a fully custom logger.
        ///     </para>
        /// </summary>
        /// <param name="action"> Delegate called when there is a message to log. </param>
        /// <param name="categories"> The <see cref="DbLoggerCategory" /> of each event to log. </param>
        /// <param name="minimumLevel"> The minimum level of logging event to log. Defaults to <see cref="LogLevel.Debug" /> </param>
        /// <param name="options">
        ///     Formatting options for log messages. Passing null (the default) means use <see cref="DbContextLoggerOptions.DefaultWithLocalTime" />
        /// </param>
        /// <returns> The same builder instance so that multiple calls can be chained. </returns>
        public new virtual DbContextOptionsBuilder<TContext> LogTo(
            [NotNull] Action<string> action,
            [NotNull] IEnumerable<string> categories,
            LogLevel minimumLevel = LogLevel.Debug,
            DbContextLoggerOptions? options = null)
            => (DbContextOptionsBuilder<TContext>)base.LogTo(action, categories, minimumLevel, options);

        /// <summary>
        ///     <para>
        ///         Logs events filtered by a supplied custom filter delegate. The filter should return true to
        ///         log a message, or false to filter it out of the log.
        ///     </para>
        ///     <para>
        ///         Use the <see cref="LogTo(Action{string},LogLevel,DbContextLoggerOptions?)" /> overload for default logging of
        ///         all events.
        ///         Use the <see cref="LogTo(Action{string},IEnumerable{EventId},LogLevel,DbContextLoggerOptions?)" />
        ///         Use the <see cref="LogTo(Action{string},IEnumerable{string},LogLevel,DbContextLoggerOptions?)" />
        ///         overload to log only events in specific categories.
        ///         Use the <see cref="LogTo(Func{EventId,LogLevel,bool},Action{EventData})" /> overload to log to a fully custom logger.
        ///     </para>
        /// </summary>
        /// <param name="action"> Delegate called when there is a message to log. </param>
        /// <param name="filter"> Delegate that returns true to log the message or false to ignore it. </param>
        /// <param name="options">
        ///     Formatting options for log messages. Passing null (the default) means use <see cref="DbContextLoggerOptions.DefaultWithLocalTime" />
        /// </param>
        /// <returns> The same builder instance so that multiple calls can be chained. </returns>
        public new virtual DbContextOptionsBuilder<TContext> LogTo(
            [NotNull] Action<string> action,
            [NotNull] Func<EventId, LogLevel, bool> filter,
            DbContextLoggerOptions? options = null)
            => (DbContextOptionsBuilder<TContext>)base.LogTo(action, filter, options);

        /// <summary>
        ///     <para>
        ///         Logs events to a custom logger delegate filtered by a custom filter delegate. The filter should return true to
        ///         log a message, or false to filter it out of the log.
        ///     </para>
        ///     <para>
        ///         Use the <see cref="LogTo(Action{string},LogLevel,DbContextLoggerOptions?)" /> overload for default logging of
        ///         all events.
        ///         Use the <see cref="LogTo(Action{string},IEnumerable{EventId},LogLevel,DbContextLoggerOptions?)" />
        ///         Use the <see cref="LogTo(Action{string},IEnumerable{string},LogLevel,DbContextLoggerOptions?)" />
        ///         overload to log only events in specific categories.
        ///         Use the <see cref="LogTo(Action{string},Func{EventId,LogLevel,bool},DbContextLoggerOptions?)" />
        ///         overload to use a custom filter for events.
        ///     </para>
        /// </summary>
        /// <param name="filter"> Delegate that returns true to log the message or false to ignore it. </param>
        /// <param name="logger"> Delegate called when there is a message to log. </param>
        /// <returns> The same builder instance so that multiple calls can be chained. </returns>
        // Filter comes first, logger second, otherwise it's hard to get the correct overload to resolve
        public new virtual DbContextOptionsBuilder<TContext> LogTo(
            [NotNull] Func<EventId, LogLevel, bool> filter,
            [NotNull] Action<EventData> logger)
            => (DbContextOptionsBuilder<TContext>)base.LogTo(filter, logger);

        /// <summary>
        ///     <para>
        ///         Disables concurrency detection, which detects many cases of erroneous concurrent usage of a <see cref="DbContext" />
        ///         instance and causes an informative exception to be thrown. This provides a minor performance improvement, but if a
        ///         <see cref="DbContext" /> instance is used concurrently, the behavior will be undefined and the program may fail in
        ///         unpredictable ways.
        ///     </para>
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
        /// </summary>
        /// <returns> The same builder instance so that multiple calls can be chained. </returns>
        public new virtual DbContextOptionsBuilder<TContext> DisableConcurrencyDetection(bool concurrencyDetectionDisabled = true)
            => (DbContextOptionsBuilder<TContext>)base.DisableConcurrencyDetection(concurrencyDetectionDisabled);

        /// <summary>
        ///     <para>
        ///         Enables detailed errors when handling data value exceptions that occur during processing of store query results. Such errors
        ///         most often occur due to misconfiguration of entity properties. E.g. If a property is configured to be of type
        ///         'int', but the underlying data in the store is actually of type 'string', then an exception will be generated
        ///         at runtime during processing of the data value. When this option is enabled and a data error is encountered, the
        ///         generated exception will include details of the specific entity property that generated the error.
        ///     </para>
        ///     <para>
        ///         Enabling this option incurs a small performance overhead during query execution.
        ///     </para>
        ///     <para>
        ///         Note that if the application is setting the internal service provider through a call to
        ///         <see cref="UseInternalServiceProvider" />, then this option must configured the same way
        ///         for all uses of that service provider. Consider instead not calling <see cref="UseInternalServiceProvider" />
        ///         so that EF will manage the service providers and can create new instances as required.
        ///     </para>
        /// </summary>
        /// <returns> The same builder instance so that multiple calls can be chained. </returns>
        public new virtual DbContextOptionsBuilder<TContext> EnableDetailedErrors(bool detailedErrorsEnabled = true)
            => (DbContextOptionsBuilder<TContext>)base.EnableDetailedErrors(detailedErrorsEnabled);

        /// <summary>
        ///     <para>
        ///         Sets the <see cref="IMemoryCache" /> to be used for query caching by this context.
        ///     </para>
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
        /// </summary>
        /// <param name="memoryCache"> The memory cache to be used. </param>
        /// <returns> The same builder instance so that multiple calls can be chained. </returns>
        public new virtual DbContextOptionsBuilder<TContext> UseMemoryCache([CanBeNull] IMemoryCache memoryCache)
            => (DbContextOptionsBuilder<TContext>)base.UseMemoryCache(memoryCache);

        /// <summary>
        ///     <para>
        ///         Sets the <see cref="IServiceProvider" /> that the context should resolve all of its services from. EF will
        ///         create and manage a service provider if none is specified.
        ///     </para>
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
        /// </summary>
        /// <param name="serviceProvider"> The service provider to be used. </param>
        /// <returns> The same builder instance so that multiple calls can be chained. </returns>
        public new virtual DbContextOptionsBuilder<TContext> UseInternalServiceProvider([CanBeNull] IServiceProvider serviceProvider)
            => (DbContextOptionsBuilder<TContext>)base.UseInternalServiceProvider(serviceProvider);

        /// <summary>
        ///     Sets the <see cref="IServiceProvider" /> from which application services will be obtained. This
        ///     is done automatically when using 'AddDbContext', so it is rare that this method needs to be called.
        /// </summary>
        /// <param name="serviceProvider"> The service provider to be used. </param>
        /// <returns> The same builder instance so that multiple calls can be chained. </returns>
        public new virtual DbContextOptionsBuilder<TContext> UseApplicationServiceProvider([CanBeNull] IServiceProvider serviceProvider)
            => (DbContextOptionsBuilder<TContext>)base.UseApplicationServiceProvider(serviceProvider);

        /// <summary>
        ///     <para>
        ///         Enables application data to be included in exception messages, logging, etc. This can include the
        ///         values assigned to properties of your entity instances, parameter values for commands being sent
        ///         to the database, and other such data. You should only enable this flag if you have the appropriate
        ///         security measures in place based on the sensitivity of this data.
        ///     </para>
        ///     <para>
        ///         Note that if the application is setting the internal service provider through a call to
        ///         <see cref="UseInternalServiceProvider" />, then this option must configured the same way
        ///         for all uses of that service provider. Consider instead not calling <see cref="UseInternalServiceProvider" />
        ///         so that EF will manage the service providers and can create new instances as required.
        ///     </para>
        /// </summary>
        /// <returns> The same builder instance so that multiple calls can be chained. </returns>
        public new virtual DbContextOptionsBuilder<TContext> EnableSensitiveDataLogging(bool sensitiveDataLoggingEnabled = true)
            => (DbContextOptionsBuilder<TContext>)base.EnableSensitiveDataLogging(sensitiveDataLoggingEnabled);

        /// <summary>
        ///     <para>
        ///         Enables or disables caching of internal service providers. Disabling caching can
        ///         massively impact performance and should only be used in testing scenarios that
        ///         build many service providers for test isolation.
        ///     </para>
        ///     <para>
        ///         Note that if the application is setting the internal service provider through a call to
        ///         <see cref="UseInternalServiceProvider" />, then setting this option wil have no effect.
        ///     </para>
        /// </summary>
        /// <param name="cacheServiceProvider"> If <see langword="true" />, then the internal service provider is cached. </param>
        /// <returns> The same builder instance so that multiple calls can be chained. </returns>
        public new virtual DbContextOptionsBuilder<TContext> EnableServiceProviderCaching(bool cacheServiceProvider = true)
            => (DbContextOptionsBuilder<TContext>)base.EnableServiceProviderCaching(cacheServiceProvider);

        /// <summary>
        ///     <para>
        ///         Sets the tracking behavior for LINQ queries run against the context. Disabling change tracking
        ///         is useful for read-only scenarios because it avoids the overhead of setting up change tracking for each
        ///         entity instance. You should not disable change tracking if you want to manipulate entity instances and
        ///         persist those changes to the database using <see cref="DbContext.SaveChanges()" />.
        ///     </para>
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
        /// </summary>
        public new virtual DbContextOptionsBuilder<TContext> UseQueryTrackingBehavior(QueryTrackingBehavior queryTrackingBehavior)
            => (DbContextOptionsBuilder<TContext>)base.UseQueryTrackingBehavior(queryTrackingBehavior);

        /// <summary>
        ///     <para>
        ///         Configures the runtime behavior of warnings generated by Entity Framework. You can set a default
        ///         behavior and behaviors for each warning type.
        ///     </para>
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
        /// </summary>
        /// <example>
        ///     <code>
        /// optionsBuilder.ConfigureWarnings(warnings =>
        ///     warnings.Default(WarningBehavior.Ignore)
        ///         .Log(CoreEventId.IncludeIgnoredWarning, CoreEventId.ModelValidationWarning)
        ///         .Throw(RelationalEventId.BoolWithDefaultWarning));
        ///     </code>
        /// </example>
        /// <param name="warningsConfigurationBuilderAction">
        ///     An action to configure the warning behavior.
        /// </param>
        /// <returns> The same builder instance so that multiple calls can be chained. </returns>
        public new virtual DbContextOptionsBuilder<TContext> ConfigureWarnings(
            [NotNull] Action<WarningsConfigurationBuilder> warningsConfigurationBuilderAction)
            => (DbContextOptionsBuilder<TContext>)base.ConfigureWarnings(warningsConfigurationBuilderAction);

        /// <summary>
        ///     <para>
        ///         Replaces all internal Entity Framework implementations of a service contract with a different
        ///         implementation.
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
        /// </summary>
        /// <typeparam name="TService"> The type (usually an interface) that defines the contract of the service to replace. </typeparam>
        /// <typeparam name="TImplementation"> The new implementation type for the service. </typeparam>
        /// <returns> The same builder instance so that multiple calls can be chained. </returns>
        public new virtual DbContextOptionsBuilder<TContext> ReplaceService<TService, TImplementation>()
            where TImplementation : TService
            => (DbContextOptionsBuilder<TContext>)base.ReplaceService<TService, TImplementation>();

        /// <summary>
        ///     <para>
        ///         Replaces the internal Entity Framework implementation of a specific implementation of a service contract
        ///         with a different implementation.
        ///     </para>
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
        /// </summary>
        /// <typeparam name="TService"> The type (usually an interface) that defines the contract of the service to replace. </typeparam>
        /// <typeparam name="TCurrentImplementation"> The current implementation type for the service. </typeparam>
        /// <typeparam name="TNewImplementation"> The new implementation type for the service. </typeparam>
        /// <returns> The same builder instance so that multiple calls can be chained. </returns>
        public new virtual DbContextOptionsBuilder<TContext> ReplaceService<TService, TCurrentImplementation, TNewImplementation>()
            where TCurrentImplementation : TService
            where TNewImplementation : TService
            => (DbContextOptionsBuilder<TContext>)base.ReplaceService<TService, TCurrentImplementation, TNewImplementation>();

        /// <summary>
        ///     <para>
        ///         Adds <see cref="IInterceptor" /> instances to those registered on the context.
        ///     </para>
        ///     <para>
        ///         Interceptors can be used to view, change, or suppress operations taken by Entity Framework.
        ///         See the specific implementations of <see cref="IInterceptor" /> for details. For example, 'IDbCommandInterceptor'.
        ///     </para>
        ///     <para>
        ///         A single interceptor instance can implement multiple different interceptor interfaces. I will be registered as
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
        /// </summary>
        /// <param name="interceptors"> The interceptors to add. </param>
        /// <returns> The same builder instance so that multiple calls can be chained. </returns>
        public new virtual DbContextOptionsBuilder<TContext> AddInterceptors([NotNull] IEnumerable<IInterceptor> interceptors)
            => (DbContextOptionsBuilder<TContext>)base.AddInterceptors(interceptors);

        /// <summary>
        ///     <para>
        ///         Adds <see cref="IInterceptor" /> instances to those registered on the context.
        ///     </para>
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
        /// </summary>
        /// <param name="interceptors"> The interceptors to add. </param>
        /// <returns> The same builder instance so that multiple calls can be chained. </returns>
        public new virtual DbContextOptionsBuilder<TContext> AddInterceptors([NotNull] params IInterceptor[] interceptors)
            => (DbContextOptionsBuilder<TContext>)base.AddInterceptors(interceptors);
    }
}
