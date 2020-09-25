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

        /// <inheritdoc cref="DbContextOptionsBuilder.Options" />
        public new virtual DbContextOptions<TContext> Options
            => (DbContextOptions<TContext>)base.Options;

        /// <inheritdoc cref="DbContextOptionsBuilder.UseModel" />
        public new virtual DbContextOptionsBuilder<TContext> UseModel([NotNull] IModel model)
            => (DbContextOptionsBuilder<TContext>)base.UseModel(model);

        /// <inheritdoc cref="DbContextOptionsBuilder.UseLoggerFactory" />
        public new virtual DbContextOptionsBuilder<TContext> UseLoggerFactory([CanBeNull] ILoggerFactory loggerFactory)
            => (DbContextOptionsBuilder<TContext>)base.UseLoggerFactory(loggerFactory);

        /// <inheritdoc cref="DbContextOptionsBuilder.LogTo(Action{string},LogLevel,DbContextLoggerOptions?)" />
        public new virtual DbContextOptionsBuilder<TContext> LogTo(
            [NotNull] Action<string> action,
            LogLevel minimumLevel = LogLevel.Debug,
            DbContextLoggerOptions? options = null)
            => (DbContextOptionsBuilder<TContext>)base.LogTo(action, minimumLevel, options);

        /// <inheritdoc cref="DbContextOptionsBuilder.LogTo(Action{string},IEnumerable{EventId},LogLevel,DbContextLoggerOptions?)" />
        public new virtual DbContextOptionsBuilder<TContext> LogTo(
            [NotNull] Action<string> action,
            [NotNull] IEnumerable<EventId> events,
            LogLevel minimumLevel = LogLevel.Debug,
            DbContextLoggerOptions? options = null)
            => (DbContextOptionsBuilder<TContext>)base.LogTo(action, events, minimumLevel, options);

        /// <inheritdoc cref="DbContextOptionsBuilder.LogTo(Action{string},IEnumerable{string},LogLevel,DbContextLoggerOptions?)" />
        public new virtual DbContextOptionsBuilder<TContext> LogTo(
            [NotNull] Action<string> action,
            [NotNull] IEnumerable<string> categories,
            LogLevel minimumLevel = LogLevel.Debug,
            DbContextLoggerOptions? options = null)
            => (DbContextOptionsBuilder<TContext>)base.LogTo(action, categories, minimumLevel, options);

        /// <inheritdoc cref="DbContextOptionsBuilder.LogTo(Action{string},Func{EventId,LogLevel,bool},DbContextLoggerOptions?)" />
        public new virtual DbContextOptionsBuilder<TContext> LogTo(
            [NotNull] Action<string> action,
            [NotNull] Func<EventId, LogLevel, bool> filter,
            DbContextLoggerOptions? options = null)
            => (DbContextOptionsBuilder<TContext>)base.LogTo(action, filter, options);

        /// <inheritdoc cref="DbContextOptionsBuilder.LogTo(Func{EventId,LogLevel,bool},Action{EventData})" />
        public new virtual DbContextOptionsBuilder<TContext> LogTo(
            [NotNull] Func<EventId, LogLevel, bool> filter,
            [NotNull] Action<EventData> logger)
            => (DbContextOptionsBuilder<TContext>)base.LogTo(filter, logger);

        /// <inheritdoc cref="DbContextOptionsBuilder.EnableDetailedErrors" />
        public new virtual DbContextOptionsBuilder<TContext> EnableDetailedErrors(bool detailedErrorsEnabled = true)
            => (DbContextOptionsBuilder<TContext>)base.EnableDetailedErrors(detailedErrorsEnabled);

        /// <inheritdoc cref="DbContextOptionsBuilder.UseMemoryCache" />
        public new virtual DbContextOptionsBuilder<TContext> UseMemoryCache([CanBeNull] IMemoryCache memoryCache)
            => (DbContextOptionsBuilder<TContext>)base.UseMemoryCache(memoryCache);

        /// <inheritdoc cref="DbContextOptionsBuilder.UseInternalServiceProvider" />
        public new virtual DbContextOptionsBuilder<TContext> UseInternalServiceProvider([CanBeNull] IServiceProvider serviceProvider)
            => (DbContextOptionsBuilder<TContext>)base.UseInternalServiceProvider(serviceProvider);

        /// <inheritdoc cref="DbContextOptionsBuilder.UseApplicationServiceProvider" />
        public new virtual DbContextOptionsBuilder<TContext> UseApplicationServiceProvider([CanBeNull] IServiceProvider serviceProvider)
            => (DbContextOptionsBuilder<TContext>)base.UseApplicationServiceProvider(serviceProvider);

        /// <inheritdoc cref="DbContextOptionsBuilder.EnableSensitiveDataLogging" />
        public new virtual DbContextOptionsBuilder<TContext> EnableSensitiveDataLogging(bool sensitiveDataLoggingEnabled = true)
            => (DbContextOptionsBuilder<TContext>)base.EnableSensitiveDataLogging(sensitiveDataLoggingEnabled);

        /// <inheritdoc cref="DbContextOptionsBuilder.EnableServiceProviderCaching" />
        public new virtual DbContextOptionsBuilder<TContext> EnableServiceProviderCaching(bool cacheServiceProvider = true)
            => (DbContextOptionsBuilder<TContext>)base.EnableServiceProviderCaching(cacheServiceProvider);

        /// <inheritdoc cref="DbContextOptionsBuilder.UseQueryTrackingBehavior" />
        public new virtual DbContextOptionsBuilder<TContext> UseQueryTrackingBehavior(QueryTrackingBehavior queryTrackingBehavior)
            => (DbContextOptionsBuilder<TContext>)base.UseQueryTrackingBehavior(queryTrackingBehavior);

        /// <inheritdoc cref="DbContextOptionsBuilder.ConfigureWarnings" />
        public new virtual DbContextOptionsBuilder<TContext> ConfigureWarnings(
            [NotNull] Action<WarningsConfigurationBuilder> warningsConfigurationBuilderAction)
            => (DbContextOptionsBuilder<TContext>)base.ConfigureWarnings(warningsConfigurationBuilderAction);

        /// <inheritdoc cref="DbContextOptionsBuilder.ReplaceService{TService,TImplementation}" />
        public new virtual DbContextOptionsBuilder<TContext> ReplaceService<TService, TImplementation>()
            where TImplementation : TService
            => (DbContextOptionsBuilder<TContext>)base.ReplaceService<TService, TImplementation>();

        /// <inheritdoc cref="DbContextOptionsBuilder.ReplaceService{TService,TCurrentImplementation,TNewImplementation}" />
        public new virtual DbContextOptionsBuilder<TContext> ReplaceService<TService, TCurrentImplementation, TNewImplementation>()
            where TCurrentImplementation : TService
            where TNewImplementation : TService
            => (DbContextOptionsBuilder<TContext>)base.ReplaceService<TService, TCurrentImplementation, TNewImplementation>();

        /// <inheritdoc cref="DbContextOptionsBuilder.AddInterceptors(IEnumerable{IInterceptor})" />
        public new virtual DbContextOptionsBuilder<TContext> AddInterceptors([NotNull] IEnumerable<IInterceptor> interceptors)
            => (DbContextOptionsBuilder<TContext>)base.AddInterceptors(interceptors);

        /// <inheritdoc cref="DbContextOptionsBuilder.AddInterceptors(IInterceptor[])" />
        public new virtual DbContextOptionsBuilder<TContext> AddInterceptors([NotNull] params IInterceptor[] interceptors)
            => (DbContextOptionsBuilder<TContext>)base.AddInterceptors(interceptors);
    }
}
