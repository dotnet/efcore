// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Design.Internal;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Migrations.Design
{
    /// <summary>
    ///     <para>
    ///         Service dependencies parameter class for <see cref="MigrationsScaffolder" />
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
    /// </summary>
    public sealed class MigrationsScaffolderDependencies
    {
        /// <summary>
        ///     <para>
        ///         Creates the service dependencies parameter object for a <see cref="MigrationsScaffolder" />.
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
        ///         This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///         directly from your code. This API may change or be removed in future releases.
        ///     </para>
        /// </summary>
        /// <param name="currentDbContext"> The current DbContext. </param>
        /// <param name="model"> The model. </param>
        /// <param name="migrationsAssembly"> The migrations assembly. </param>
        /// <param name="migrationsModelDiffer"> The migrations model differ. </param>
        /// <param name="migrationsIdGenerator"> The migrations ID generator. </param>
        /// <param name="migrationCodeGenerator"> The migrations code generator. </param>
        /// <param name="historyRepository"> The history repository. </param>
        /// <param name="operationReporter"> The operation reporter. </param>
        /// <param name="databaseProvider"> The database provider. </param>
        /// <param name="snapshotModelProcessor"> The snapshot model processor. </param>
        public MigrationsScaffolderDependencies(
            [NotNull] ICurrentDbContext currentDbContext,
            [NotNull] IModel model,
            [NotNull] IMigrationsAssembly migrationsAssembly,
            [NotNull] IMigrationsModelDiffer migrationsModelDiffer,
            [NotNull] IMigrationsIdGenerator migrationsIdGenerator,
            [NotNull] IMigrationsCodeGenerator migrationCodeGenerator,
            [NotNull] IHistoryRepository historyRepository,
            [NotNull] IOperationReporter operationReporter,
            [NotNull] IDatabaseProvider databaseProvider,
            [NotNull] ISnapshotModelProcessor snapshotModelProcessor)
        {
            Check.NotNull(currentDbContext, nameof(currentDbContext));
            Check.NotNull(model, nameof(model));
            Check.NotNull(migrationsAssembly, nameof(migrationsAssembly));
            Check.NotNull(migrationsModelDiffer, nameof(migrationsModelDiffer));
            Check.NotNull(migrationsIdGenerator, nameof(migrationsIdGenerator));
            Check.NotNull(migrationCodeGenerator, nameof(migrationCodeGenerator));
            Check.NotNull(historyRepository, nameof(historyRepository));
            Check.NotNull(operationReporter, nameof(operationReporter));
            Check.NotNull(databaseProvider, nameof(databaseProvider));
            Check.NotNull(snapshotModelProcessor, nameof(snapshotModelProcessor));

            CurrentDbContext = currentDbContext;
            Model = model;
            MigrationsAssembly = migrationsAssembly;
            MigrationsModelDiffer = migrationsModelDiffer;
            MigrationsIdGenerator = migrationsIdGenerator;
            MigrationCodeGenerator = migrationCodeGenerator;
            HistoryRepository = historyRepository;
            OperationReporter = operationReporter;
            DatabaseProvider = databaseProvider;
            SnapshotModelProcessor = snapshotModelProcessor;
        }

        /// <summary>
        ///     The current DbContext.
        /// </summary>
        public ICurrentDbContext CurrentDbContext { get; }

        /// <summary>
        ///     The model.
        /// </summary>
        public IModel Model { get; }

        /// <summary>
        ///     The migrations assembly.
        /// </summary>
        public IMigrationsAssembly MigrationsAssembly { get; }

        /// <summary>
        ///     The migrations model differ.
        /// </summary>
        public IMigrationsModelDiffer MigrationsModelDiffer { get; }

        /// <summary>
        ///     The migrations ID generator.
        /// </summary>
        public IMigrationsIdGenerator MigrationsIdGenerator { get; }

        /// <summary>
        ///     The migrations code generator.
        /// </summary>
        public IMigrationsCodeGenerator MigrationCodeGenerator { get; }

        /// <summary>
        ///     The history repository.
        /// </summary>
        public IHistoryRepository HistoryRepository { get; }

        /// <summary>
        ///     The operation reporter.
        /// </summary>
        public IOperationReporter OperationReporter { get; }

        /// <summary>
        ///     The database provider.
        /// </summary>
        public IDatabaseProvider DatabaseProvider { get; }

        /// <summary>
        ///     The snapshot model processor.
        /// </summary>
        public ISnapshotModelProcessor SnapshotModelProcessor { get; }

        /// <summary>
        ///     Clones this dependency parameter object with one service replaced.
        /// </summary>
        /// <param name="currentDbContext"> A replacement for the current dependency of this type. </param>
        /// <returns> A new parameter object with the given service replaced. </returns>
        public MigrationsScaffolderDependencies With([NotNull] ICurrentDbContext currentDbContext)
            => new MigrationsScaffolderDependencies(
                currentDbContext,
                Model,
                MigrationsAssembly,
                MigrationsModelDiffer,
                MigrationsIdGenerator,
                MigrationCodeGenerator,
                HistoryRepository,
                OperationReporter,
                DatabaseProvider,
                SnapshotModelProcessor);

        /// <summary>
        ///     Clones this dependency parameter object with one service replaced.
        /// </summary>
        /// <param name="model"> A replacement for the current dependency of this type. </param>
        /// <returns> A new parameter object with the given service replaced. </returns>
        public MigrationsScaffolderDependencies With([NotNull] IModel model)
            => new MigrationsScaffolderDependencies(
                CurrentDbContext,
                model,
                MigrationsAssembly,
                MigrationsModelDiffer,
                MigrationsIdGenerator,
                MigrationCodeGenerator,
                HistoryRepository,
                OperationReporter,
                DatabaseProvider,
                SnapshotModelProcessor);

        /// <summary>
        ///     Clones this dependency parameter object with one service replaced.
        /// </summary>
        /// <param name="migrationsAssembly"> A replacement for the current dependency of this type. </param>
        /// <returns> A new parameter object with the given service replaced. </returns>
        public MigrationsScaffolderDependencies With([NotNull] IMigrationsAssembly migrationsAssembly)
            => new MigrationsScaffolderDependencies(
                CurrentDbContext,
                Model,
                migrationsAssembly,
                MigrationsModelDiffer,
                MigrationsIdGenerator,
                MigrationCodeGenerator,
                HistoryRepository,
                OperationReporter,
                DatabaseProvider,
                SnapshotModelProcessor);

        /// <summary>
        ///     Clones this dependency parameter object with one service replaced.
        /// </summary>
        /// <param name="migrationsModelDiffer"> A replacement for the current dependency of this type. </param>
        /// <returns> A new parameter object with the given service replaced. </returns>
        public MigrationsScaffolderDependencies With([NotNull] IMigrationsModelDiffer migrationsModelDiffer)
            => new MigrationsScaffolderDependencies(
                CurrentDbContext,
                Model,
                MigrationsAssembly,
                migrationsModelDiffer,
                MigrationsIdGenerator,
                MigrationCodeGenerator,
                HistoryRepository,
                OperationReporter,
                DatabaseProvider,
                SnapshotModelProcessor);

        /// <summary>
        ///     Clones this dependency parameter object with one service replaced.
        /// </summary>
        /// <param name="migrationsIdGenerator"> A replacement for the current dependency of this type. </param>
        /// <returns> A new parameter object with the given service replaced. </returns>
        public MigrationsScaffolderDependencies With([NotNull] IMigrationsIdGenerator migrationsIdGenerator)
            => new MigrationsScaffolderDependencies(
                CurrentDbContext,
                Model,
                MigrationsAssembly,
                MigrationsModelDiffer,
                migrationsIdGenerator,
                MigrationCodeGenerator,
                HistoryRepository,
                OperationReporter,
                DatabaseProvider,
                SnapshotModelProcessor);

        /// <summary>
        ///     Clones this dependency parameter object with one service replaced.
        /// </summary>
        /// <param name="migrationCodeGenerator"> A replacement for the current dependency of this type. </param>
        /// <returns> A new parameter object with the given service replaced. </returns>
        public MigrationsScaffolderDependencies With([NotNull] IMigrationsCodeGenerator migrationCodeGenerator)
            => new MigrationsScaffolderDependencies(
                CurrentDbContext,
                Model,
                MigrationsAssembly,
                MigrationsModelDiffer,
                MigrationsIdGenerator,
                migrationCodeGenerator,
                HistoryRepository,
                OperationReporter,
                DatabaseProvider,
                SnapshotModelProcessor);

        /// <summary>
        ///     Clones this dependency parameter object with one service replaced.
        /// </summary>
        /// <param name="historyRepository"> A replacement for the current dependency of this type. </param>
        /// <returns> A new parameter object with the given service replaced. </returns>
        public MigrationsScaffolderDependencies With([NotNull] IHistoryRepository historyRepository)
            => new MigrationsScaffolderDependencies(
                CurrentDbContext,
                Model,
                MigrationsAssembly,
                MigrationsModelDiffer,
                MigrationsIdGenerator,
                MigrationCodeGenerator,
                historyRepository,
                OperationReporter,
                DatabaseProvider,
                SnapshotModelProcessor);

        /// <summary>
        ///     Clones this dependency parameter object with one service replaced.
        /// </summary>
        /// <param name="operationReporter"> A replacement for the current dependency of this type. </param>
        /// <returns> A new parameter object with the given service replaced. </returns>
        public MigrationsScaffolderDependencies With([NotNull] IOperationReporter operationReporter)
            => new MigrationsScaffolderDependencies(
                CurrentDbContext,
                Model,
                MigrationsAssembly,
                MigrationsModelDiffer,
                MigrationsIdGenerator,
                MigrationCodeGenerator,
                HistoryRepository,
                operationReporter,
                DatabaseProvider,
                SnapshotModelProcessor);

        /// <summary>
        ///     Clones this dependency parameter object with one service replaced.
        /// </summary>
        /// <param name="databaseProvider"> A replacement for the current dependency of this type. </param>
        /// <returns> A new parameter object with the given service replaced. </returns>
        public MigrationsScaffolderDependencies With([NotNull] IDatabaseProvider databaseProvider)
            => new MigrationsScaffolderDependencies(
                CurrentDbContext,
                Model,
                MigrationsAssembly,
                MigrationsModelDiffer,
                MigrationsIdGenerator,
                MigrationCodeGenerator,
                HistoryRepository,
                OperationReporter,
                databaseProvider,
                SnapshotModelProcessor);

        /// <summary>
        ///     Clones this dependency parameter object with one service replaced.
        /// </summary>
        /// <param name="snapshotModelProcessor"> A replacement for the current dependency of this type. </param>
        /// <returns> A new parameter object with the given service replaced. </returns>
        public MigrationsScaffolderDependencies With([NotNull] ISnapshotModelProcessor snapshotModelProcessor)
            => new MigrationsScaffolderDependencies(
                CurrentDbContext,
                Model,
                MigrationsAssembly,
                MigrationsModelDiffer,
                MigrationsIdGenerator,
                MigrationCodeGenerator,
                HistoryRepository,
                OperationReporter,
                DatabaseProvider,
                snapshotModelProcessor);
    }
}
