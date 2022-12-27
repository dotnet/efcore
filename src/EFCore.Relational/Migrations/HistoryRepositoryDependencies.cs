// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Migrations;

/// <summary>
///     <para>
///         Service dependencies parameter class for <see cref="HistoryRepository" />
///     </para>
///     <para>
///         This type is typically used by database providers (and other extensions). It is generally
///         not used in application code.
///     </para>
/// </summary>
/// <remarks>
///     <para>
///         Do not construct instances of this class directly from either provider or application code as the
///         constructor signature may change as new dependencies are added. Instead, use this type in
///         your constructor so that an instance will be created and injected automatically by the
///         dependency injection container. To create an instance with some dependent services replaced,
///         first resolve the object from the dependency injection container, then replace selected
///         services using the C# 'with' operator. Do not call the constructor at any point in this process.
///     </para>
///     <para>
///         The service lifetime is <see cref="ServiceLifetime.Scoped" />. This means that each
///         <see cref="DbContext" /> instance will use its own instance of this service.
///         The implementation may depend on other services registered with any lifetime.
///         The implementation does not need to be thread-safe.
///     </para>
/// </remarks>
public sealed record HistoryRepositoryDependencies
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
    public HistoryRepositoryDependencies(
        IRelationalDatabaseCreator databaseCreator,
        IRawSqlCommandBuilder rawSqlCommandBuilder,
        IRelationalConnection connection,
        IDbContextOptions options,
        IMigrationsModelDiffer modelDiffer,
        IMigrationsSqlGenerator migrationsSqlGenerator,
        ISqlGenerationHelper sqlGenerationHelper,
        IConventionSetBuilder conventionSetBuilder,
        ModelDependencies modelDependencies,
        IRelationalTypeMappingSource typeMappingSource,
        ICurrentDbContext currentContext,
        IModelRuntimeInitializer modelRuntimeInitializer,
        IRelationalCommandDiagnosticsLogger commandLogger)
    {
        DatabaseCreator = databaseCreator;
        RawSqlCommandBuilder = rawSqlCommandBuilder;
        Connection = connection;
        Options = options;
        ModelDiffer = modelDiffer;
        MigrationsSqlGenerator = migrationsSqlGenerator;
        SqlGenerationHelper = sqlGenerationHelper;
        ConventionSetBuilder = conventionSetBuilder;
        ModelDependencies = modelDependencies;
        TypeMappingSource = typeMappingSource;
        CurrentContext = currentContext;
        ModelRuntimeInitializer = modelRuntimeInitializer;
        CommandLogger = commandLogger;
    }

    /// <summary>
    ///     The database creator.
    /// </summary>
    public IRelationalDatabaseCreator DatabaseCreator { get; init; }

    /// <summary>
    ///     A command builder for building raw SQL commands.
    /// </summary>
    public IRawSqlCommandBuilder RawSqlCommandBuilder { get; init; }

    /// <summary>
    ///     The connection to the database.
    /// </summary>
    public IRelationalConnection Connection { get; init; }

    /// <summary>
    ///     Options for the current context instance.
    /// </summary>
    public IDbContextOptions Options { get; init; }

    /// <summary>
    ///     The model differ.
    /// </summary>
    public IMigrationsModelDiffer ModelDiffer { get; init; }

    /// <summary>
    ///     The SQL generator for Migrations operations.
    /// </summary>
    public IMigrationsSqlGenerator MigrationsSqlGenerator { get; init; }

    /// <summary>
    ///     Helpers for generating update SQL.
    /// </summary>
    public ISqlGenerationHelper SqlGenerationHelper { get; init; }

    /// <summary>
    ///     The core convention set to use when creating the model.
    /// </summary>
    public IConventionSetBuilder ConventionSetBuilder { get; init; }

    /// <summary>
    ///     The model dependencies.
    /// </summary>
    public ModelDependencies ModelDependencies { get; init; }

    /// <summary>
    ///     The type mapper.
    /// </summary>
    public IRelationalTypeMappingSource TypeMappingSource { get; init; }

    /// <summary>
    ///     Contains the <see cref="DbContext" /> currently in use.
    /// </summary>
    public ICurrentDbContext CurrentContext { get; init; }

    /// <summary>
    ///     The model runtime initializer
    /// </summary>
    public IModelRuntimeInitializer ModelRuntimeInitializer { get; init; }

    /// <summary>
    ///     The command logger
    /// </summary>
    public IRelationalCommandDiagnosticsLogger CommandLogger { get; init; }
}
