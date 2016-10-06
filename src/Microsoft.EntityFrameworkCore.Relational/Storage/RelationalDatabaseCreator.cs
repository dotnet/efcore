// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Utilities;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore.Storage
{
    /// <summary>
    ///     <para>
    ///         Performs database/schema creation, and other related operations.
    ///     </para>
    ///     <para>
    ///         This type is typically used by database providers (and other extensions). It is generally
    ///         not used in application code.
    ///     </para>
    /// </summary>
    public abstract class RelationalDatabaseCreator : IRelationalDatabaseCreator, IServiceInjectionSite
    {
        private readonly IMigrationsModelDiffer _modelDiffer;
        private readonly IMigrationsSqlGenerator _migrationsSqlGenerator;

        /// <summary>
        ///     Initializes a new instance of the <see cref="RelationalDatabaseCreator" /> class.
        /// </summary>
        /// <param name="model"> The <see cref="IModel" /> for the context this creator is being used with. </param>
        /// <param name="connection"> The <see cref="IRelationalConnection" /> to be used. </param>
        /// <param name="modelDiffer"> The <see cref="IMigrationsModelDiffer" /> to be used. </param>
        /// <param name="migrationsSqlGenerator"> The <see cref="IMigrationsSqlGenerator" /> to be used. </param>
        /// <param name="migrationCommandExecutor"> The <see cref="IMigrationCommandExecutor" /> to be used. </param>
        [Obsolete("Derived classes must be updated to call the new constructor with additional parameters.")]
        protected RelationalDatabaseCreator(
            [NotNull] IModel model,
            [NotNull] IRelationalConnection connection,
            [NotNull] IMigrationsModelDiffer modelDiffer,
            [NotNull] IMigrationsSqlGenerator migrationsSqlGenerator,
            [NotNull] IMigrationCommandExecutor migrationCommandExecutor)
        {
            Check.NotNull(model, nameof(model));
            Check.NotNull(connection, nameof(connection));
            Check.NotNull(modelDiffer, nameof(modelDiffer));
            Check.NotNull(migrationsSqlGenerator, nameof(migrationsSqlGenerator));
            Check.NotNull(migrationCommandExecutor, nameof(migrationCommandExecutor));

            Model = model;
            Connection = connection;
            _modelDiffer = modelDiffer;
            _migrationsSqlGenerator = migrationsSqlGenerator;
            MigrationCommandExecutor = migrationCommandExecutor;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="RelationalDatabaseCreator" /> class.
        /// </summary>
        /// <param name="model"> The <see cref="IModel" /> for the context this creator is being used with. </param>
        /// <param name="connection"> The <see cref="IRelationalConnection" /> to be used. </param>
        /// <param name="modelDiffer"> The <see cref="IMigrationsModelDiffer" /> to be used. </param>
        /// <param name="migrationsSqlGenerator"> The <see cref="IMigrationsSqlGenerator" /> to be used. </param>
        /// <param name="migrationCommandExecutor"> The <see cref="IMigrationCommandExecutor" /> to be used. </param>
        /// <param name="executionStrategyFactory">The <see cref="IExecutionStrategyFactory" /> to be used. </param>
        protected RelationalDatabaseCreator(
            [NotNull] IModel model,
            [NotNull] IRelationalConnection connection,
            [NotNull] IMigrationsModelDiffer modelDiffer,
            [NotNull] IMigrationsSqlGenerator migrationsSqlGenerator,
            [NotNull] IMigrationCommandExecutor migrationCommandExecutor,
            [NotNull] IExecutionStrategyFactory executionStrategyFactory)
        {
            Check.NotNull(model, nameof(model));
            Check.NotNull(connection, nameof(connection));
            Check.NotNull(modelDiffer, nameof(modelDiffer));
            Check.NotNull(migrationsSqlGenerator, nameof(migrationsSqlGenerator));
            Check.NotNull(migrationCommandExecutor, nameof(migrationCommandExecutor));
            Check.NotNull(executionStrategyFactory, nameof(executionStrategyFactory));

            Model = model;
            Connection = connection;
            _modelDiffer = modelDiffer;
            _migrationsSqlGenerator = migrationsSqlGenerator;
            MigrationCommandExecutor = migrationCommandExecutor;
            ExecutionStrategyFactory = executionStrategyFactory;
        }

        /// <summary>
        ///     Gets the model for the context this creator is being used with.
        /// </summary>
        protected virtual IModel Model { get; }

        /// <summary>
        ///     Gets the connection for the database.
        /// </summary>
        protected virtual IRelationalConnection Connection { get; }

        /// <summary>
        ///     Gets the <see cref="IMigrationCommandExecutor" /> to be used.
        /// </summary>
        protected virtual IMigrationCommandExecutor MigrationCommandExecutor { get; }

        /// <summary>
        ///     Gets the <see cref="IExecutionStrategyFactory" /> to be used.
        /// </summary>
        protected virtual IExecutionStrategyFactory ExecutionStrategyFactory { get; private set; }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        void IServiceInjectionSite.InjectServices(IServiceProvider serviceProvider)
            => ExecutionStrategyFactory = ExecutionStrategyFactory ?? serviceProvider.GetService<IExecutionStrategyFactory>();

        /// <summary>
        ///     Determines whether the physical database exists. No attempt is made to determine if the database
        ///     contains the schema for the current model.
        /// </summary>
        /// <returns>
        ///     True if the database exists; otherwise false.
        /// </returns>
        public abstract bool Exists();

        /// <summary>
        ///     Asynchronously determines whether the physical database exists. No attempt is made to determine if
        ///     the database contains the schema for the current model.
        /// </summary>
        /// <param name="cancellationToken">
        ///     A <see cref="CancellationToken" /> to observe while waiting for the task to complete.
        /// </param>
        /// <returns>
        ///     A task that represents the asynchronous operation. The task result contains
        ///     true if the database exists; otherwise false.
        /// </returns>
        public virtual Task<bool> ExistsAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();

            return Task.FromResult(Exists());
        }

        /// <summary>
        ///     Creates the physical database. Does not attempt to populate it with any schema.
        /// </summary>
        public abstract void Create();

        /// <summary>
        ///     Asynchronously creates the physical database. Does not attempt to populate it with any schema.
        /// </summary>
        /// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
        /// <returns>
        ///     A task that represents the asynchronous operation.
        /// </returns>
        public virtual Task CreateAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();

            Create();

            return Task.FromResult(0);
        }

        /// <summary>
        ///     Deletes the physical database.
        /// </summary>
        public abstract void Delete();

        /// <summary>
        ///     Asynchronously deletes the physical database.
        /// </summary>
        /// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
        /// <returns>
        ///     A task that represents the asynchronous operation.
        /// </returns>
        public virtual Task DeleteAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();

            Delete();

            return Task.FromResult(0);
        }

        /// <summary>
        ///     Creates all tables for the current model in the database. No attempt is made
        ///     to incrementally update the schema. It is assumed that none of the tables exist in the database.
        /// </summary>
        public virtual void CreateTables()
            => MigrationCommandExecutor.ExecuteNonQuery(GetCreateTablesCommands(), Connection);

        /// <summary>
        ///     Asynchronously creates all tables for the current model in the database. No attempt is made
        ///     to incrementally update the schema. It is assumed that none of the tables exist in the database.
        /// </summary>
        /// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
        /// <returns>
        ///     A task that represents the asynchronous operation.
        /// </returns>
        public virtual async Task CreateTablesAsync(CancellationToken cancellationToken = default(CancellationToken))
            => await MigrationCommandExecutor.ExecuteNonQueryAsync(GetCreateTablesCommands(), Connection, cancellationToken);

        /// <summary>
        ///     Gets the commands that will create all tables from the model.
        /// </summary>
        /// <returns> The generated commands. </returns>
        protected virtual IReadOnlyList<MigrationCommand> GetCreateTablesCommands()
            => _migrationsSqlGenerator.Generate(_modelDiffer.GetDifferences(null, Model), Model);

        /// <summary>
        ///     Determines whether the database contains any tables. No attempt is made to determine if
        ///     tables belong to the current model or not.
        /// </summary>
        /// <returns> A value indicating whether any tables are present in the database. </returns>
        protected abstract bool HasTables();

        /// <summary>
        ///     Asynchronously determines whether the database contains any tables. No attempt is made to determine if
        ///     tables belong to the current model or not.
        /// </summary>
        /// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
        /// <returns>
        ///     A task that represents the asynchronous operation. The task result contains
        ///     a value indicating whether any tables are present in the database.
        /// </returns>
        protected virtual Task<bool> HasTablesAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();

            return Task.FromResult(HasTables());
        }

        /// <summary>
        ///     <para>
        ///         Ensures that the database for the context does not exist. If it does not exist, no action is taken. If it does
        ///         exist then the database is deleted.
        ///     </para>
        ///     <para>
        ///         Warning: The entire database is deleted an no effort is made to remove just the database objects that are used by
        ///         the model for this context.
        ///     </para>
        /// </summary>
        /// <returns>
        ///     True if the database is deleted, false if it did not exist.
        /// </returns>
        public virtual bool EnsureDeleted()
        {
            if (Exists())
            {
                Delete();
                return true;
            }
            return false;
        }

        /// <summary>
        ///     <para>
        ///         Asynchronously ensures that the database for the context does not exist. If it does not exist, no action is taken. If it does
        ///         exist then the database is deleted.
        ///     </para>
        ///     <para>
        ///         Warning: The entire database is deleted an no effort is made to remove just the database objects that are used by
        ///         the model for this context.
        ///     </para>
        /// </summary>
        /// <param name="cancellationToken">A <see cref="T:System.Threading.CancellationToken" /> to observe while waiting for the task to complete.</param>
        /// <returns>
        ///     A task that represents the asynchronous save operation. The task result contains true if the database is deleted,
        ///     false if it did not exist.
        /// </returns>
        public virtual async Task<bool> EnsureDeletedAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            if (await ExistsAsync(cancellationToken))
            {
                await DeleteAsync(cancellationToken);

                return true;
            }
            return false;
        }

        /// <summary>
        ///     Ensures that the database for the context exists. If it exists, no action is taken. If it does not
        ///     exist then the database and all its schema are created. If the database exists, then no effort is made
        ///     to ensure it is compatible with the model for this context.
        /// </summary>
        /// <returns>
        ///     True if the database is created, false if it already existed.
        /// </returns>
        public virtual bool EnsureCreated()
        {
            if (!Exists())
            {
                Create();
                CreateTables();
                return true;
            }

            if (!HasTables())
            {
                CreateTables();
                return true;
            }

            return false;
        }

        /// <summary>
        ///     Asynchronously ensures that the database for the context exists. If it exists, no action is taken. If it does not
        ///     exist then the database and all its schema are created. If the database exists, then no effort is made
        ///     to ensure it is compatible with the model for this context.
        /// </summary>
        /// <param name="cancellationToken">A <see cref="T:System.Threading.CancellationToken" /> to observe while waiting for the task to complete.</param>
        /// <returns>
        ///     A task that represents the asynchronous save operation. The task result contains true if the database is created,
        ///     false if it already existed.
        /// </returns>
        public virtual async Task<bool> EnsureCreatedAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            if (!await ExistsAsync(cancellationToken))
            {
                await CreateAsync(cancellationToken);
                await CreateTablesAsync(cancellationToken);

                return true;
            }

            if (!await HasTablesAsync(cancellationToken))
            {
                await CreateTablesAsync(cancellationToken);

                return true;
            }

            return false;
        }
    }
}
