// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Utilities;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore.Storage
{
    /// <summary>
    ///     <para>
    ///         Service dependencies parameter class for <see cref="RelationalDatabaseCreator" />
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
    public sealed class RelationalDatabaseCreatorDependencies
    {
        /// <summary>
        ///     <para>
        ///         Creates the service dependencies parameter object for a <see cref="RelationalDatabaseCreator" />.
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
        /// <param name="model"> The <see cref="IModel" /> for the context this creator is being used with. </param>
        /// <param name="connection"> The <see cref="IRelationalConnection" /> to be used. </param>
        /// <param name="modelDiffer"> The <see cref="IMigrationsModelDiffer" /> to be used. </param>
        /// <param name="migrationsSqlGenerator"> The <see cref="IMigrationsSqlGenerator" /> to be used. </param>
        /// <param name="migrationCommandExecutor"> The <see cref="IMigrationCommandExecutor" /> to be used. </param>
        /// <param name="sqlGenerationHelper"> The <see cref="ISqlGenerationHelper" /> to be used. </param>
        /// <param name="executionStrategyFactory">The <see cref="IExecutionStrategyFactory" /> to be used. </param>
        /// <param name="currentContext"> Contains the <see cref="DbContext"/> currently in use. </param>
        /// <param name="commandLogger"> The command logger. </param>
        [EntityFrameworkInternal]
        public RelationalDatabaseCreatorDependencies(
            [NotNull] IModel model,
            [NotNull] IRelationalConnection connection,
            [NotNull] IMigrationsModelDiffer modelDiffer,
            [NotNull] IMigrationsSqlGenerator migrationsSqlGenerator,
            [NotNull] IMigrationCommandExecutor migrationCommandExecutor,
            [NotNull] ISqlGenerationHelper sqlGenerationHelper,
            [NotNull] IExecutionStrategyFactory executionStrategyFactory,
            [NotNull] ICurrentDbContext currentContext,
            [NotNull] IDiagnosticsLogger<DbLoggerCategory.Database.Command> commandLogger)
        {
            Check.NotNull(model, nameof(model));
            Check.NotNull(connection, nameof(connection));
            Check.NotNull(modelDiffer, nameof(modelDiffer));
            Check.NotNull(migrationsSqlGenerator, nameof(migrationsSqlGenerator));
            Check.NotNull(migrationCommandExecutor, nameof(migrationCommandExecutor));
            Check.NotNull(sqlGenerationHelper, nameof(sqlGenerationHelper));
            Check.NotNull(executionStrategyFactory, nameof(executionStrategyFactory));
            Check.NotNull(currentContext, nameof(currentContext));
            Check.NotNull(commandLogger, nameof(commandLogger));

            Model = model;
            Connection = connection;
            ModelDiffer = modelDiffer;
            MigrationsSqlGenerator = migrationsSqlGenerator;
            MigrationCommandExecutor = migrationCommandExecutor;
            SqlGenerationHelper = sqlGenerationHelper;
            ExecutionStrategyFactory = executionStrategyFactory;
            CurrentContext = currentContext;
            CommandLogger = commandLogger;
        }

        /// <summary>
        ///     The model differ.
        /// </summary>
        public IMigrationsModelDiffer ModelDiffer { get; }

        /// <summary>
        ///     The Migrations SQL generator.
        /// </summary>
        public IMigrationsSqlGenerator MigrationsSqlGenerator { get; }

        /// <summary>
        ///     Gets the model for the context this creator is being used with.
        /// </summary>
        public IModel Model { get; }

        /// <summary>
        ///     Gets the connection for the database.
        /// </summary>
        public IRelationalConnection Connection { get; }

        /// <summary>
        ///     Gets the <see cref="IMigrationCommandExecutor" /> to be used.
        /// </summary>
        public IMigrationCommandExecutor MigrationCommandExecutor { get; }

        /// <summary>
        ///     Gets the <see cref="ISqlGenerationHelper" /> to be used.
        /// </summary>
        public ISqlGenerationHelper SqlGenerationHelper { get; }

        /// <summary>
        ///     Gets the <see cref="IExecutionStrategyFactory" /> to be used.
        /// </summary>
        public IExecutionStrategyFactory ExecutionStrategyFactory { get; }

        /// <summary>
        ///     The command logger.
        /// </summary>
        public IDiagnosticsLogger<DbLoggerCategory.Database.Command> CommandLogger { get; }

        /// <summary>
        ///    Contains the <see cref="DbContext"/> currently in use.
        /// </summary>
        public ICurrentDbContext CurrentContext { get; }

        /// <summary>
        ///     Clones this dependency parameter object with one service replaced.
        /// </summary>
        /// <param name="model"> A replacement for the current dependency of this type. </param>
        /// <returns> A new parameter object with the given service replaced. </returns>
        public RelationalDatabaseCreatorDependencies With([NotNull] IModel model)
            => new RelationalDatabaseCreatorDependencies(
                model,
                Connection,
                ModelDiffer,
                MigrationsSqlGenerator,
                MigrationCommandExecutor,
                SqlGenerationHelper,
                ExecutionStrategyFactory,
                CurrentContext,
                CommandLogger);

        /// <summary>
        ///     Clones this dependency parameter object with one service replaced.
        /// </summary>
        /// <param name="connection"> A replacement for the current dependency of this type. </param>
        /// <returns> A new parameter object with the given service replaced. </returns>
        public RelationalDatabaseCreatorDependencies With([NotNull] IRelationalConnection connection)
            => new RelationalDatabaseCreatorDependencies(
                Model,
                connection,
                ModelDiffer,
                MigrationsSqlGenerator,
                MigrationCommandExecutor,
                SqlGenerationHelper,
                ExecutionStrategyFactory,
                CurrentContext,
                CommandLogger);

        /// <summary>
        ///     Clones this dependency parameter object with one service replaced.
        /// </summary>
        /// <param name="modelDiffer"> A replacement for the current dependency of this type. </param>
        /// <returns> A new parameter object with the given service replaced. </returns>
        public RelationalDatabaseCreatorDependencies With([NotNull] IMigrationsModelDiffer modelDiffer)
            => new RelationalDatabaseCreatorDependencies(
                Model,
                Connection,
                modelDiffer,
                MigrationsSqlGenerator,
                MigrationCommandExecutor,
                SqlGenerationHelper,
                ExecutionStrategyFactory,
                CurrentContext,
                CommandLogger);

        /// <summary>
        ///     Clones this dependency parameter object with one service replaced.
        /// </summary>
        /// <param name="migrationsSqlGenerator"> A replacement for the current dependency of this type. </param>
        /// <returns> A new parameter object with the given service replaced. </returns>
        public RelationalDatabaseCreatorDependencies With([NotNull] IMigrationsSqlGenerator migrationsSqlGenerator)
            => new RelationalDatabaseCreatorDependencies(
                Model,
                Connection,
                ModelDiffer,
                migrationsSqlGenerator,
                MigrationCommandExecutor,
                SqlGenerationHelper,
                ExecutionStrategyFactory,
                CurrentContext,
                CommandLogger);

        /// <summary>
        ///     Clones this dependency parameter object with one service replaced.
        /// </summary>
        /// <param name="migrationCommandExecutor"> A replacement for the current dependency of this type. </param>
        /// <returns> A new parameter object with the given service replaced. </returns>
        public RelationalDatabaseCreatorDependencies With([NotNull] IMigrationCommandExecutor migrationCommandExecutor)
            => new RelationalDatabaseCreatorDependencies(
                Model,
                Connection,
                ModelDiffer,
                MigrationsSqlGenerator,
                migrationCommandExecutor,
                SqlGenerationHelper,
                ExecutionStrategyFactory,
                CurrentContext,
                CommandLogger);

        /// <summary>
        ///     Clones this dependency parameter object with one service replaced.
        /// </summary>
        /// <param name="sqlGenerationHelper"> A replacement for the current dependency of this type. </param>
        /// <returns> A new parameter object with the given service replaced. </returns>
        public RelationalDatabaseCreatorDependencies With([NotNull] ISqlGenerationHelper sqlGenerationHelper)
            => new RelationalDatabaseCreatorDependencies(
                Model,
                Connection,
                ModelDiffer,
                MigrationsSqlGenerator,
                MigrationCommandExecutor,
                sqlGenerationHelper,
                ExecutionStrategyFactory,
                CurrentContext,
                CommandLogger);

        /// <summary>
        ///     Clones this dependency parameter object with one service replaced.
        /// </summary>
        /// <param name="executionStrategyFactory"> A replacement for the current dependency of this type. </param>
        /// <returns> A new parameter object with the given service replaced. </returns>
        public RelationalDatabaseCreatorDependencies With([NotNull] IExecutionStrategyFactory executionStrategyFactory)
            => new RelationalDatabaseCreatorDependencies(
                Model,
                Connection,
                ModelDiffer,
                MigrationsSqlGenerator,
                MigrationCommandExecutor,
                SqlGenerationHelper,
                executionStrategyFactory,
                CurrentContext,
                CommandLogger);

        /// <summary>
        ///     Clones this dependency parameter object with one service replaced.
        /// </summary>
        /// <param name="currentContext"> A replacement for the current dependency of this type. </param>
        /// <returns> A new parameter object with the given service replaced. </returns>
        public RelationalDatabaseCreatorDependencies With([NotNull] ICurrentDbContext currentContext)
            => new RelationalDatabaseCreatorDependencies(
                Model,
                Connection,
                ModelDiffer,
                MigrationsSqlGenerator,
                MigrationCommandExecutor,
                SqlGenerationHelper,
                ExecutionStrategyFactory,
                currentContext,
                CommandLogger);

        /// <summary>
        ///     Clones this dependency parameter object with one service replaced.
        /// </summary>
        /// <param name="commandLogger"> A replacement for the current dependency of this type. </param>
        /// <returns> A new parameter object with the given service replaced. </returns>
        public RelationalDatabaseCreatorDependencies With([NotNull] IDiagnosticsLogger<DbLoggerCategory.Database.Command> commandLogger)
            => new RelationalDatabaseCreatorDependencies(
                Model,
                Connection,
                ModelDiffer,
                MigrationsSqlGenerator,
                MigrationCommandExecutor,
                SqlGenerationHelper,
                ExecutionStrategyFactory,
                CurrentContext,
                commandLogger);
    }
}
