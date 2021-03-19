// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Design.Internal;
using Microsoft.EntityFrameworkCore.Infrastructure;
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
    public sealed record MigrationsScaffolderDependencies
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
        public MigrationsScaffolderDependencies(
            ICurrentDbContext currentContext,
            IModel model,
            IMigrationsAssembly migrationsAssembly,
            IMigrationsModelDiffer migrationsModelDiffer,
            IMigrationsIdGenerator migrationsIdGenerator,
            IMigrationsCodeGeneratorSelector migrationsCodeGeneratorSelector,
            IHistoryRepository historyRepository,
            IOperationReporter operationReporter,
            IDatabaseProvider databaseProvider,
            ISnapshotModelProcessor snapshotModelProcessor,
            IMigrator migrator)
        {
            Check.NotNull(currentContext, nameof(currentContext));
            Check.NotNull(model, nameof(model));
            Check.NotNull(migrationsAssembly, nameof(migrationsAssembly));
            Check.NotNull(migrationsModelDiffer, nameof(migrationsModelDiffer));
            Check.NotNull(migrationsIdGenerator, nameof(migrationsIdGenerator));
            Check.NotNull(migrationsCodeGeneratorSelector, nameof(migrationsCodeGeneratorSelector));
            Check.NotNull(historyRepository, nameof(historyRepository));
            Check.NotNull(operationReporter, nameof(operationReporter));
            Check.NotNull(databaseProvider, nameof(databaseProvider));
            Check.NotNull(snapshotModelProcessor, nameof(snapshotModelProcessor));
            Check.NotNull(migrator, nameof(migrator));

            CurrentContext = currentContext;
            Model = model;
            MigrationsAssembly = migrationsAssembly;
            MigrationsModelDiffer = migrationsModelDiffer;
            MigrationsIdGenerator = migrationsIdGenerator;
            MigrationsCodeGeneratorSelector = migrationsCodeGeneratorSelector;
            HistoryRepository = historyRepository;
            OperationReporter = operationReporter;
            DatabaseProvider = databaseProvider;
            SnapshotModelProcessor = snapshotModelProcessor;
            Migrator = migrator;
        }

        /// <summary>
        ///     The current DbContext.
        /// </summary>
        public ICurrentDbContext CurrentContext { get; init; }

        /// <summary>
        ///     The model.
        /// </summary>
        public IModel Model { get; init; }

        /// <summary>
        ///     The migrations assembly.
        /// </summary>
        public IMigrationsAssembly MigrationsAssembly { get; init; }

        /// <summary>
        ///     The migrations model differ.
        /// </summary>
        public IMigrationsModelDiffer MigrationsModelDiffer { get; init; }

        /// <summary>
        ///     The migrations ID generator.
        /// </summary>
        public IMigrationsIdGenerator MigrationsIdGenerator { get; init; }

        /// <summary>
        ///     The migrations code generator selector.
        /// </summary>
        public IMigrationsCodeGeneratorSelector MigrationsCodeGeneratorSelector { get; init; }

        /// <summary>
        ///     The history repository.
        /// </summary>
        public IHistoryRepository HistoryRepository { get; init; }

        /// <summary>
        ///     The operation reporter.
        /// </summary>
        [EntityFrameworkInternal]
        public IOperationReporter OperationReporter { get; init; }

        /// <summary>
        ///     The database provider.
        /// </summary>
        public IDatabaseProvider DatabaseProvider { get; init; }

        /// <summary>
        ///     The snapshot model processor.
        /// </summary>
        [EntityFrameworkInternal]
        public ISnapshotModelProcessor SnapshotModelProcessor { get; init; }

        /// <summary>
        ///     The migrator.
        /// </summary>
        public IMigrator Migrator { get; init; }
    }
}
