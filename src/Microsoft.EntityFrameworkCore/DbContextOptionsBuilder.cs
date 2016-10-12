// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Utilities;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
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

        /// <summary>
        ///     Sets the <see cref="ILoggerFactory" /> that will be used to create <see cref="ILogger" /> instances
        ///     for logging done by this context.
        /// </summary>
        /// <param name="loggerFactory"> The logger factory to be used. </param>
        /// <returns> The same builder instance so that multiple calls can be chained. </returns>
        public virtual DbContextOptionsBuilder UseLoggerFactory([CanBeNull] ILoggerFactory loggerFactory)
            => SetOption(e => e.LoggerFactory = loggerFactory);

        /// <summary>
        ///     Sets the <see cref="IMemoryCache" /> to be used for query caching by this context. EF will
        ///     create and manage a memory cache if none is specified.
        /// </summary>
        /// <param name="memoryCache"> The memory cache to be used. </param>
        /// <returns> The same builder instance so that multiple calls can be chained. </returns>
        public virtual DbContextOptionsBuilder UseMemoryCache([CanBeNull] IMemoryCache memoryCache)
            => SetOption(e => e.MemoryCache = memoryCache);

        /// <summary>
        ///     <para>
        ///         Sets the <see cref="IServiceProvider" /> that the context should resolve all of its services from. EF will
        ///         create and manage a service provider if none is specified.
        ///     </para>
        ///     <para>
        ///         The service provider must contain all the services required by Entity Framework (and the database being
        ///         used). The Entity Framework services can be registered using the
        ///         <see
        ///             cref="Microsoft.EntityFrameworkCore.Infrastructure.EntityFrameworkServiceCollectionExtensions.AddEntityFramework(IServiceCollection)" />
        ///         method. Most databases also provide an extension method on <see cref="IServiceCollection" /> to register the
        ///         services required. For example, the Microsoft SQL Server provider includes an AddEntityFrameworkSqlServer() method
        ///         to add the required services.
        ///     </para>
        ///     <para>
        ///         If the <see cref="IServiceProvider" /> has a <see cref="DbContextOptions" /> or
        ///         <see cref="DbContextOptions{TContext}" /> registered, then this will be used as the options for
        ///         this context instance.
        ///     </para>
        /// </summary>
        /// <param name="serviceProvider"> The service provider to be used. </param>
        /// <returns> The same builder instance so that multiple calls can be chained. </returns>
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
        ///         The default value is <see cref="EntityFrameworkCore.QueryTrackingBehavior.TrackAll" />. This means the change tracker will
        ///         keep track of changes for all entities that are returned from a LINQ query.
        ///     </para>
        /// </summary>
        public virtual DbContextOptionsBuilder UseQueryTrackingBehavior(QueryTrackingBehavior queryTrackingBehavior)
            => SetOption(e => e.QueryTrackingBehavior = queryTrackingBehavior);

        /// <summary>
        ///     Configures the runtime behavior of warnings generated by Entity Framework. You can set a default behavior and behaviors for
        ///     each warning type.
        /// </summary>
        /// <example>
        ///     <code>
        ///         optionsBuilder.ConfigureWarnings(warnings => 
        ///             warnings.Default(WarningBehavior.Ignore)
        ///                     .Log(CoreEventId.IncludeIgnoredWarning, CoreEventId.ModelValidationWarning)
        ///                     .Throw(RelationalEventId.QueryClientEvaluationWarning))
        ///     </code>
        /// </example>
        /// <param name="warningsConfigurationBuilderAction">
        ///     An action to configure the warning behavior.
        /// </param>
        /// <returns> The same builder instance so that multiple calls can be chained. </returns>
        public virtual DbContextOptionsBuilder ConfigureWarnings(
            [NotNull] Action<WarningsConfigurationBuilder> warningsConfigurationBuilderAction)
        {
            Check.NotNull(warningsConfigurationBuilderAction, nameof(warningsConfigurationBuilderAction));

            var warningConfigurationBuilder
                = new WarningsConfigurationBuilder(
                    Options.FindExtension<CoreOptionsExtension>()?.WarningsConfiguration);

            warningsConfigurationBuilderAction(warningConfigurationBuilder);

            return SetOption(e => e.WarningsConfiguration = warningConfigurationBuilder.Configuration);
        }

        /// <summary>
        ///     <para>
        ///         Replaces the internal Entity Framework implementation of a service contract with a different
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
        public virtual DbContextOptionsBuilder ReplaceService<TService, TImplementation>() where TImplementation : TService
            => SetOption(e => e.ReplaceService(typeof(TService), typeof(TImplementation)));

        /// <summary>
        ///     <para>
        ///         Adds the given extension to the options. If an existing extension of the same type already exists, it will be replaced.
        ///     </para>
        ///     <para>
        ///         This method is intended for use by extension methods to configure the context. It is not intended to be used in
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
