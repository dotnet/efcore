// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
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
    ///         The service lifetime is <see cref="ServiceLifetime.Scoped" />. This means that each
    ///         <see cref="DbContext" /> instance will use its own instance of this service.
    ///         The implementation may depend on other services registered with any lifetime.
    ///         The implementation does not need to be thread-safe.
    ///     </para>
    /// </summary>
    public sealed record RelationalDatabaseCreatorDependencies
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
        ///     <para>
        ///         This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///         the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///         any release. You should only use it directly in your code with extreme caution and knowing that
        ///         doing so can result in application failures when updating to a new Entity Framework Core release.
        ///     </para>
        /// </summary>
        [EntityFrameworkInternal]
        public RelationalDatabaseCreatorDependencies(
            IModel model,
            IRelationalConnection connection,
            IMigrationsModelDiffer modelDiffer,
            IMigrationsSqlGenerator migrationsSqlGenerator,
            IMigrationCommandExecutor migrationCommandExecutor,
            ISqlGenerationHelper sqlGenerationHelper,
            IExecutionStrategyFactory executionStrategyFactory,
            ICurrentDbContext currentContext,
            IRelationalCommandDiagnosticsLogger commandLogger)
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

#pragma warning disable CS0618 // Type or member is obsolete
            Model = model;
#pragma warning restore CS0618 // Type or member is obsolete
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
        public IMigrationsModelDiffer ModelDiffer { get; init; }

        /// <summary>
        ///     The Migrations SQL generator.
        /// </summary>
        public IMigrationsSqlGenerator MigrationsSqlGenerator { get; init; }

        /// <summary>
        ///     Gets the model for the context this creator is being used with.
        /// </summary>
        [Obsolete("Use CurrentContext.Context.GetService<IDesignTimeModel>().Model instead")]
        public IModel Model { get; init; }

        /// <summary>
        ///     Gets the connection for the database.
        /// </summary>
        public IRelationalConnection Connection { get; init; }

        /// <summary>
        ///     Gets the <see cref="IMigrationCommandExecutor" /> to be used.
        /// </summary>
        public IMigrationCommandExecutor MigrationCommandExecutor { get; init; }

        /// <summary>
        ///     Gets the <see cref="ISqlGenerationHelper" /> to be used.
        /// </summary>
        public ISqlGenerationHelper SqlGenerationHelper { get; init; }

        /// <summary>
        ///     Gets the <see cref="IExecutionStrategyFactory" /> to be used.
        /// </summary>
        public IExecutionStrategyFactory ExecutionStrategyFactory { get; init; }

        /// <summary>
        ///     The command logger.
        /// </summary>
        public IRelationalCommandDiagnosticsLogger CommandLogger { get; init; }

        /// <summary>
        ///     Contains the <see cref="DbContext" /> currently in use.
        /// </summary>
        public ICurrentDbContext CurrentContext { get; init; }
    }
}
