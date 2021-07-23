// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Update;
using Microsoft.EntityFrameworkCore.Utilities;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore.Migrations
{
    /// <summary>
    ///     <para>
    ///         Service dependencies parameter class for <see cref="MigrationsSqlGenerator" />
    ///     </para>
    ///     <para>
    ///         This type is typically used by database providers (and other extensions). It is generally
    ///         not used in application code.
    ///     </para>
    ///     <para>
    ///         Do not construct instances of this class directly from either provider or application code as the
    ///         constructor signature may change as new dependencies are added. Instead, use this type in
    ///         your constructor so that an instance will be created and injected automatically by the
    ///         dependency injection container. To create an instance with some dependent services replaced,
    ///         first resolve the object from the dependency injection container, then replace selected
    ///         services using the 'With...' methods. Do not call the constructor at any point in this process.
    ///     </para>
    ///     <para>
    ///         The service lifetime is <see cref="ServiceLifetime.Scoped" />. This means that each
    ///         <see cref="DbContext" /> instance will use its own instance of this service.
    ///         The implementation may depend on other services registered with any lifetime.
    ///         The implementation does not need to be thread-safe.
    ///     </para>
    /// </summary>
    public sealed record MigrationsSqlGeneratorDependencies
    {
        /// <summary>
        ///     <para>
        ///         Creates the service dependencies parameter object for a <see cref="MigrationsSqlGenerator" />.
        ///     </para>
        ///     <para>
        ///         Do not call this constructor directly from either provider or application code as it may change
        ///         as new dependencies are added. Instead, use this type in your constructor so that an instance
        ///         will be created and injected automatically by the dependency injection container. To create
        ///         an instance with some dependent services replaced, first resolve the object from the dependency
        ///         injection container, then replace selected services using the 'With...' methods. Do not call
        ///         the constructor at any point in this process.
        ///     </para>
        ///     <para>
        ///         This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///         the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///         any release. You should only use it directly in your code with extreme caution and knowing that
        ///         doing so can result in application failures when updating to a new Entity Framework Core release.
        ///     </para>
        /// </summary>
        [EntityFrameworkInternal]
        public MigrationsSqlGeneratorDependencies(
            IRelationalCommandBuilderFactory commandBuilderFactory,
            IUpdateSqlGenerator updateSqlGenerator,
            ISqlGenerationHelper sqlGenerationHelper,
            IRelationalTypeMappingSource typeMappingSource,
            ICurrentDbContext currentContext,
            ILoggingOptions loggingOptions,
            IRelationalCommandDiagnosticsLogger logger,
            IDiagnosticsLogger<DbLoggerCategory.Migrations> migrationsLogger)
        {
            Check.NotNull(commandBuilderFactory, nameof(commandBuilderFactory));
            Check.NotNull(updateSqlGenerator, nameof(updateSqlGenerator));
            Check.NotNull(sqlGenerationHelper, nameof(sqlGenerationHelper));
            Check.NotNull(typeMappingSource, nameof(typeMappingSource));
            Check.NotNull(currentContext, nameof(currentContext));
            Check.NotNull(loggingOptions, nameof(loggingOptions));
            Check.NotNull(logger, nameof(logger));
            Check.NotNull(migrationsLogger, nameof(migrationsLogger));

            CommandBuilderFactory = commandBuilderFactory;
            SqlGenerationHelper = sqlGenerationHelper;
            UpdateSqlGenerator = updateSqlGenerator;
            TypeMappingSource = typeMappingSource;
            CurrentContext = currentContext;
            LoggingOptions = loggingOptions;
            Logger = logger;
            MigrationsLogger = migrationsLogger;
        }

        /// <summary>
        ///     The command builder factory.
        /// </summary>
        public IRelationalCommandBuilderFactory CommandBuilderFactory { get; init; }

        /// <summary>
        ///     High level SQL generator.
        /// </summary>
        public IUpdateSqlGenerator UpdateSqlGenerator { get; init; }

        /// <summary>
        ///     Helpers for SQL generation.
        /// </summary>
        public ISqlGenerationHelper SqlGenerationHelper { get; init; }

        /// <summary>
        ///     The type mapper.
        /// </summary>
        public IRelationalTypeMappingSource TypeMappingSource { get; init; }

        /// <summary>
        ///     Contains the <see cref="DbContext" /> currently in use.
        /// </summary>
        public ICurrentDbContext CurrentContext { get; init; }

        /// <summary>
        ///     The logging options.
        /// </summary>
        public ILoggingOptions LoggingOptions { get; init; }

        /// <summary>
        ///     The database command logger.
        /// </summary>
        public IRelationalCommandDiagnosticsLogger Logger { get; init; }

        /// <summary>
        ///     The database command logger.
        /// </summary>
        public IDiagnosticsLogger<DbLoggerCategory.Migrations> MigrationsLogger { get; init; }
    }
}
