// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.Caching.Memory;
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
        public new virtual DbContextOptions<TContext> Options => (DbContextOptions<TContext>)base.Options;

        /// <summary>
        ///     Sets the model to be used for the context. If the model is set, then <see cref="DbContext.OnModelCreating(ModelBuilder)" />
        ///     will not be run.
        /// </summary>
        /// <param name="model"> The model to be used. </param>
        /// <returns> The same builder instance so that multiple calls can be chained. </returns>
        public new virtual DbContextOptionsBuilder<TContext> UseModel([NotNull] IModel model)
            => (DbContextOptionsBuilder<TContext>)base.UseModel(model);

        public new virtual DbContextOptionsBuilder<TContext> UseLoggerFactory([CanBeNull] ILoggerFactory loggerFactory)
            => (DbContextOptionsBuilder<TContext>)base.UseLoggerFactory(loggerFactory);

        public new virtual DbContextOptionsBuilder<TContext> UseMemoryCache([CanBeNull] IMemoryCache memoryCache)
            => (DbContextOptionsBuilder<TContext>)base.UseMemoryCache(memoryCache);

        public new virtual DbContextOptionsBuilder<TContext> UseInternalServiceProvider([CanBeNull] IServiceProvider serviceProvider)
            => (DbContextOptionsBuilder<TContext>)base.UseInternalServiceProvider(serviceProvider);

        /// <summary>
        ///     Enables application data to be included in exception messages, logging, etc. This can include the values assigned to properties
        ///     of your entity instances, parameter values for commands being sent to the database, and other such data. You should only enable
        ///     this flag if you have the appropriate security measures in place based on the sensitivity of this data.
        /// </summary>
        /// <returns> The same builder instance so that multiple calls can be chained. </returns>
        public new virtual DbContextOptionsBuilder<TContext> EnableSensitiveDataLogging()
            => (DbContextOptionsBuilder<TContext>)base.EnableSensitiveDataLogging();

        public new virtual DbContextOptionsBuilder<TContext> ConfigureWarnings(
            [NotNull] Action<WarningsConfigurationBuilder> warningsConfigurationBuilderAction)
            => (DbContextOptionsBuilder<TContext>)base.ConfigureWarnings(warningsConfigurationBuilderAction);
    }
}
