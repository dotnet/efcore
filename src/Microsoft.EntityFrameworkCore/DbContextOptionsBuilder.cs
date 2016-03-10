// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Utilities;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace Microsoft.EntityFrameworkCore
{
    /// <summary>
    ///     <para>
    ///         Provides a simple API surface for configuring <see cref="DbContextOptions" />. Databases (and other extensions)
    ///         typically define extension methods on this object that allow you to configure the database connection (and other
    ///         options) to be used for a context.
    ///     </para>
    ///     <para>
    ///         You can use <see cref="DbContextOptionsBuilder" /> to configure a context by overriding
    ///         <see cref="DbContext.OnConfiguring(DbContextOptionsBuilder)" /> or creating a <see cref="DbContextOptions" />
    ///         externally and passing it to the context constructor.
    ///     </para>
    /// </summary>
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
        /// <param name="options"> The options to be configured. </param>
        public DbContextOptionsBuilder([NotNull] DbContextOptions options)
        {
            Check.NotNull(options, nameof(options));

            _options = options;
        }

        /// <summary>
        ///     Gets the options being configured.
        /// </summary>
        public virtual DbContextOptions Options => _options;

        /// <summary>
        ///     <para>
        ///         Gets a value indicating whether any options have been configured.
        ///     </para>
        ///     <para>
        ///         This can be useful when you have overridden <see cref="DbContext.OnConfiguring(DbContextOptionsBuilder)" /> to configure
        ///         the context, but in some cases you also externally provide options via the context constructor. This property can be
        ///         used to determine if the options have already been set, and skip some or all of the logic in
        ///         <see cref="DbContext.OnConfiguring(DbContextOptionsBuilder)" />.
        ///     </para>
        /// </summary>
        public virtual bool IsConfigured => _options.Extensions.Any();

        /// <summary>
        ///     Sets the model to be used for the context. If the model is set, then <see cref="DbContext.OnModelCreating(ModelBuilder)" />
        ///     will not be run.
        /// </summary>
        /// <param name="model"> The model to be used. </param>
        /// <returns> The same builder instance so that multiple calls can be chained. </returns>
        public virtual DbContextOptionsBuilder UseModel([NotNull] IModel model) 
            => SetOption(e => e.Model = Check.NotNull(model, nameof(model)));

        public virtual DbContextOptionsBuilder UseLoggerFactory([CanBeNull] ILoggerFactory loggerFactory) 
            => SetOption(e => e.LoggerFactory = loggerFactory);

        public virtual DbContextOptionsBuilder UseMemoryCache([CanBeNull] IMemoryCache memoryCache)
            => SetOption(e => e.MemoryCache = memoryCache);

        public virtual DbContextOptionsBuilder UseInternalServiceProvider([CanBeNull] IServiceProvider serviceProvider)
            => SetOption(e => e.InternalServiceProvider = serviceProvider);

        /// <summary>
        ///     Enables application data to be included in exception messages, logging, etc. This can include the values assigned to properties
        ///     of your entity instances, parameter values for commands being sent to the database, and other such data. You should only enable
        ///     this flag if you have the appropriate security measures in place based on the sensitivity of this data.
        /// </summary>
        /// <returns> The same builder instance so that multiple calls can be chained. </returns>
        public virtual DbContextOptionsBuilder EnableSensitiveDataLogging()
            => SetOption(e => e.IsSensitiveDataLoggingEnabled = true);

        /// <summary>
        ///     <para>
        ///         Adds the given extension to the options. If an existing extension of the same type already exists, it will be replaced.
        ///     </para>
        ///     <para>
        ///         This property is intended for use by extension methods to configure the context. It is not intended to be used in
        ///         application code.
        ///     </para>
        /// </summary>
        /// <typeparam name="TExtension"> The type of extension to be added. </typeparam>
        /// <param name="extension"> The extension to be added. </param>
        void IDbContextOptionsBuilderInfrastructure.AddOrUpdateExtension<TExtension>(TExtension extension)
        {
            Check.NotNull(extension, nameof(extension));

            _options = _options.WithExtension(extension);
        }

        private DbContextOptionsBuilder SetOption(Action<CoreOptionsExtension> setAction)
        {
            var existingExtension = Options.FindExtension<CoreOptionsExtension>();

            var extension
                = existingExtension != null
                    ? new CoreOptionsExtension(existingExtension)
                    : new CoreOptionsExtension();

            setAction(extension);

            ((IDbContextOptionsBuilderInfrastructure)this).AddOrUpdateExtension(extension);

            return this;
        }
    }
}
