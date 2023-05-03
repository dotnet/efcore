// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Design.Internal;
using Microsoft.EntityFrameworkCore.Migrations.Internal;

namespace Microsoft.EntityFrameworkCore.Migrations.Design;

/// <summary>
///     <para>
///         Service dependencies parameter class for <see cref="MigrationsScaffolder" />
///     </para>
///     <para>
///         This type is typically used by database providers (and other extensions). It is generally
///         not used in application code.
///     </para>
/// </summary>
/// <remarks>
///     Do not construct instances of this class directly from either provider or application code as the
///     constructor signature may change as new dependencies are added. Instead, use this type in
///     your constructor so that an instance will be created and injected automatically by the
///     dependency injection container. To create an instance with some dependent services replaced,
///     first resolve the object from the dependency injection container, then replace selected
///     services using the C# 'with' operator. Do not call the constructor at any point in this process.
/// </remarks>
public sealed record MigrationsScaffolderDependencies
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    /// <remarks>
    ///     Do not call this constructor directly from either provider or application code as it may change
    ///     as new dependencies are added. Instead, use this type in your constructor so that an instance
    ///     will be created and injected automatically by the dependency injection container. To create
    ///     an instance with some dependent services replaced, first resolve the object from the dependency
    ///     injection container, then replace selected services using the C# 'with' operator. Do not call
    ///     the constructor at any point in this process.
    /// </remarks>
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
