// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Utilities;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore.Update
{
    /// <summary>
    ///     <para>
    ///         Service dependencies parameter class for <see cref="IModificationCommandBatchFactory" />
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
    public sealed class ModificationCommandBatchFactoryDependencies
    {
        /// <summary>
        ///     <para>
        ///         Creates the service dependencies parameter object for a <see cref="IModificationCommandBatchFactory" />.
        ///     </para>
        ///     <para>
        ///         Do not call this constructor directly from either provider or application code as it may change
        ///         as new dependencies are added. Instead, use this type in your constructor so that an instance
        ///         will be created and injected automatically by the dependency injection container. To create
        ///         an instance with some dependent services replaced, first resolve the object from the dependency
        ///         injection container, then replace selected services using the 'With...' methods. Do not call
        ///         the constructor at any point in this process.
        ///     </para>
        /// </summary>
        /// <param name="valueBufferFactoryFactory"> The value buffer factory. </param>
        /// <param name="commandBuilderFactory"> The command builder factory. </param>
        /// <param name="sqlGenerationHelper"> The sql generator. </param>
        /// <param name="updateSqlGenerator"> The update generator. </param>
        /// <param name="currentContext"> Contains the <see cref="DbContext"/> currently in use. </param>
        /// <param name="logger"> A logger. </param>
        public ModificationCommandBatchFactoryDependencies(
            [NotNull] IRelationalCommandBuilderFactory commandBuilderFactory,
            [NotNull] ISqlGenerationHelper sqlGenerationHelper,
            [NotNull] IUpdateSqlGenerator updateSqlGenerator,
            [NotNull] IRelationalValueBufferFactoryFactory valueBufferFactoryFactory,
            [NotNull] ICurrentDbContext currentContext,
            [NotNull] IDiagnosticsLogger<DbLoggerCategory.Database.Command> logger)
        {
            Check.NotNull(commandBuilderFactory, nameof(commandBuilderFactory));
            Check.NotNull(sqlGenerationHelper, nameof(sqlGenerationHelper));
            Check.NotNull(updateSqlGenerator, nameof(updateSqlGenerator));
            Check.NotNull(valueBufferFactoryFactory, nameof(valueBufferFactoryFactory));
            Check.NotNull(logger, nameof(logger));

            CommandBuilderFactory = commandBuilderFactory;
            SqlGenerationHelper = sqlGenerationHelper;
            UpdateSqlGenerator = updateSqlGenerator;
            ValueBufferFactoryFactory = valueBufferFactoryFactory;
            CurrentContext = currentContext;
            Logger = logger;
        }

        /// <summary>
        ///     A logger.
        /// </summary>
        public IDiagnosticsLogger<DbLoggerCategory.Database.Command> Logger { get; }

        /// <summary>
        ///     The command builder factory.
        /// </summary>
        public IRelationalCommandBuilderFactory CommandBuilderFactory { get; }

        /// <summary>
        ///     The SQL generator helper.
        /// </summary>
        public ISqlGenerationHelper SqlGenerationHelper { get; }

        /// <summary>
        ///     The update SQL generator.
        /// </summary>
        public IUpdateSqlGenerator UpdateSqlGenerator { get; }

        /// <summary>
        ///     The value buffer factory.
        /// </summary>
        public IRelationalValueBufferFactoryFactory ValueBufferFactoryFactory { get; }

        /// <summary>
        ///    Contains the <see cref="DbContext"/> currently in use.
        /// </summary>
        public ICurrentDbContext CurrentContext { get; }

        /// <summary>
        ///     Clones this dependency parameter object with one service replaced.
        /// </summary>
        /// <param name="logger"> A replacement for the current dependency of this type. </param>
        /// <returns> A new parameter object with the given service replaced. </returns>
        public ModificationCommandBatchFactoryDependencies With([NotNull] IDiagnosticsLogger<DbLoggerCategory.Database.Command> logger)
            => new ModificationCommandBatchFactoryDependencies(
                CommandBuilderFactory,
                SqlGenerationHelper,
                UpdateSqlGenerator,
                ValueBufferFactoryFactory,
                CurrentContext,
                logger);

        /// <summary>
        ///     Clones this dependency parameter object with one service replaced.
        /// </summary>
        /// <param name="valueBufferFactoryFactory"> A replacement for the current dependency of this type. </param>
        /// <returns> A new parameter object with the given service replaced. </returns>
        public ModificationCommandBatchFactoryDependencies With([NotNull] IRelationalValueBufferFactoryFactory valueBufferFactoryFactory)
            => new ModificationCommandBatchFactoryDependencies(
                CommandBuilderFactory,
                SqlGenerationHelper,
                UpdateSqlGenerator,
                valueBufferFactoryFactory,
                CurrentContext,
                Logger);

        /// <summary>
        ///     Clones this dependency parameter object with one service replaced.
        /// </summary>
        /// <param name="commandBuilderFactory"> A replacement for the current dependency of this type. </param>
        /// <returns> A new parameter object with the given service replaced. </returns>
        public ModificationCommandBatchFactoryDependencies With([NotNull] IRelationalCommandBuilderFactory commandBuilderFactory)
            => new ModificationCommandBatchFactoryDependencies(
                commandBuilderFactory,
                SqlGenerationHelper,
                UpdateSqlGenerator,
                ValueBufferFactoryFactory,
                CurrentContext,
                Logger);

        /// <summary>
        ///     Clones this dependency parameter object with one service replaced.
        /// </summary>
        /// <param name="sqlGenerationHelper"> A replacement for the current dependency of this type. </param>
        /// <returns> A new parameter object with the given service replaced. </returns>
        public ModificationCommandBatchFactoryDependencies With([NotNull] ISqlGenerationHelper sqlGenerationHelper)
            => new ModificationCommandBatchFactoryDependencies(
                CommandBuilderFactory,
                sqlGenerationHelper,
                UpdateSqlGenerator,
                ValueBufferFactoryFactory,
                CurrentContext,
                Logger);

        /// <summary>
        ///     Clones this dependency parameter object with one service replaced.
        /// </summary>
        /// <param name="updateSqlGenerator"> A replacement for the current dependency of this type. </param>
        /// <returns> A new parameter object with the given service replaced. </returns>
        public ModificationCommandBatchFactoryDependencies With([NotNull] IUpdateSqlGenerator updateSqlGenerator)
            => new ModificationCommandBatchFactoryDependencies(
                CommandBuilderFactory,
                SqlGenerationHelper,
                updateSqlGenerator,
                ValueBufferFactoryFactory,
                CurrentContext,
                Logger);

        /// <summary>
        ///     Clones this dependency parameter object with one service replaced.
        /// </summary>
        /// <param name="currentContext"> A replacement for the current dependency of this type. </param>
        /// <returns> A new parameter object with the given service replaced. </returns>
        public ModificationCommandBatchFactoryDependencies With([NotNull] ICurrentDbContext currentContext)
            => new ModificationCommandBatchFactoryDependencies(
                CommandBuilderFactory,
                SqlGenerationHelper,
                UpdateSqlGenerator,
                ValueBufferFactoryFactory,
                currentContext,
                Logger);
    }
}
