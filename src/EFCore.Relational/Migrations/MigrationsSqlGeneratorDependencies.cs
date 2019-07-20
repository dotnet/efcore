// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
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
    ///         The service lifetime is <see cref="ServiceLifetime.Scoped"/>. This means that each
    ///         <see cref="DbContext"/> instance will use its own instance of this service.
    ///         The implementation may depend on other services registered with any lifetime.
    ///         The implementation does not need to be thread-safe.
    ///     </para>
    /// </summary>
    public sealed class MigrationsSqlGeneratorDependencies
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
        /// <param name="commandBuilderFactory"> The command builder factory. </param>
        /// <param name="updateSqlGenerator"> High level SQL generator. </param>
        /// <param name="sqlGenerationHelper"> Helpers for SQL generation. </param>
        /// <param name="typeMappingSource"> The type mapper. </param>
        /// <param name="currentContext"> Contains the <see cref="DbContext"/> currently in use. </param>
        /// <param name="logger"> A logger. </param>
        [EntityFrameworkInternal]
        public MigrationsSqlGeneratorDependencies(
            [NotNull] IRelationalCommandBuilderFactory commandBuilderFactory,
            [NotNull] IUpdateSqlGenerator updateSqlGenerator,
            [NotNull] ISqlGenerationHelper sqlGenerationHelper,
            [NotNull] IRelationalTypeMappingSource typeMappingSource,
            [NotNull] ICurrentDbContext currentContext,
            [NotNull] IDiagnosticsLogger<DbLoggerCategory.Database.Command> logger)
        {
            Check.NotNull(commandBuilderFactory, nameof(commandBuilderFactory));
            Check.NotNull(updateSqlGenerator, nameof(updateSqlGenerator));
            Check.NotNull(sqlGenerationHelper, nameof(sqlGenerationHelper));
            Check.NotNull(typeMappingSource, nameof(typeMappingSource));
            Check.NotNull(currentContext, nameof(currentContext));
            Check.NotNull(logger, nameof(logger));

            CommandBuilderFactory = commandBuilderFactory;
            SqlGenerationHelper = sqlGenerationHelper;
            UpdateSqlGenerator = updateSqlGenerator;
            TypeMappingSource = typeMappingSource;
            CurrentContext = currentContext;
            Logger = logger;
        }

        /// <summary>
        ///     The command builder factory.
        /// </summary>
        public IRelationalCommandBuilderFactory CommandBuilderFactory { get; }

        /// <summary>
        ///     High level SQL generator.
        /// </summary>
        public IUpdateSqlGenerator UpdateSqlGenerator { get; }

        /// <summary>
        ///     Helpers for SQL generation.
        /// </summary>
        public ISqlGenerationHelper SqlGenerationHelper { get; }

        /// <summary>
        ///     The type mapper.
        /// </summary>
        public IRelationalTypeMappingSource TypeMappingSource { get; }

        /// <summary>
        ///    Contains the <see cref="DbContext"/> currently in use.
        /// </summary>
        public ICurrentDbContext CurrentContext { get; }

        /// <summary>
        ///     A logger.
        /// </summary>
        public IDiagnosticsLogger<DbLoggerCategory.Database.Command> Logger { get; }

        /// <summary>
        ///     Clones this dependency parameter object with one service replaced.
        /// </summary>
        /// <param name="commandBuilderFactory"> A replacement for the current dependency of this type. </param>
        /// <returns> A new parameter object with the given service replaced. </returns>
        public MigrationsSqlGeneratorDependencies With([NotNull] IRelationalCommandBuilderFactory commandBuilderFactory)
            => new MigrationsSqlGeneratorDependencies(
                commandBuilderFactory,
                UpdateSqlGenerator,
                SqlGenerationHelper,
                TypeMappingSource,
                CurrentContext,
                Logger);

        /// <summary>
        ///     Clones this dependency parameter object with one service replaced.
        /// </summary>
        /// <param name="updateSqlGenerator"> A replacement for the current dependency of this type. </param>
        /// <returns> A new parameter object with the given service replaced. </returns>
        public MigrationsSqlGeneratorDependencies With([NotNull] IUpdateSqlGenerator updateSqlGenerator)
            => new MigrationsSqlGeneratorDependencies(
                CommandBuilderFactory,
                updateSqlGenerator,
                SqlGenerationHelper,
                TypeMappingSource,
                CurrentContext,
                Logger);

        /// <summary>
        ///     Clones this dependency parameter object with one service replaced.
        /// </summary>
        /// <param name="sqlGenerationHelper"> A replacement for the current dependency of this type. </param>
        /// <returns> A new parameter object with the given service replaced. </returns>
        public MigrationsSqlGeneratorDependencies With([NotNull] ISqlGenerationHelper sqlGenerationHelper)
            => new MigrationsSqlGeneratorDependencies(
                CommandBuilderFactory,
                UpdateSqlGenerator,
                sqlGenerationHelper,
                TypeMappingSource,
                CurrentContext,
                Logger);

        /// <summary>
        ///     Clones this dependency parameter object with one service replaced.
        /// </summary>
        /// <param name="typeMappingSource"> A replacement for the current dependency of this type. </param>
        /// <returns> A new parameter object with the given service replaced. </returns>
        public MigrationsSqlGeneratorDependencies With([NotNull] IRelationalTypeMappingSource typeMappingSource)
            => new MigrationsSqlGeneratorDependencies(
                CommandBuilderFactory,
                UpdateSqlGenerator,
                SqlGenerationHelper,
                typeMappingSource,
                CurrentContext,
                Logger);

        /// <summary>
        ///     Clones this dependency parameter object with one service replaced.
        /// </summary>
        /// <param name="currentContext"> A replacement for the current dependency of this type. </param>
        /// <returns> A new parameter object with the given service replaced. </returns>
        public MigrationsSqlGeneratorDependencies With([NotNull] ICurrentDbContext currentContext)
            => new MigrationsSqlGeneratorDependencies(
                CommandBuilderFactory,
                UpdateSqlGenerator,
                SqlGenerationHelper,
                TypeMappingSource,
                currentContext,
                Logger);

        /// <summary>
        ///     Clones this dependency parameter object with one service replaced.
        /// </summary>
        /// <param name="logger"> A replacement for the current dependency of this type. </param>
        /// <returns> A new parameter object with the given service replaced. </returns>
        public MigrationsSqlGeneratorDependencies With([NotNull] IDiagnosticsLogger<DbLoggerCategory.Database.Command> logger)
            => new MigrationsSqlGeneratorDependencies(
                CommandBuilderFactory,
                UpdateSqlGenerator,
                SqlGenerationHelper,
                TypeMappingSource,
                CurrentContext,
                logger);
    }
}
