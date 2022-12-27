// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Update;

/// <summary>
///     <para>
///         Service dependencies parameter class for <see cref="IModificationCommandBatchFactory" />
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
public sealed record ModificationCommandBatchFactoryDependencies
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
    public ModificationCommandBatchFactoryDependencies(
        IRelationalCommandBuilderFactory commandBuilderFactory,
        ISqlGenerationHelper sqlGenerationHelper,
        IUpdateSqlGenerator updateSqlGenerator,
        ICurrentDbContext currentContext,
        IRelationalCommandDiagnosticsLogger logger,
        IDiagnosticsLogger<DbLoggerCategory.Update> updateLogger)
    {
        CommandBuilderFactory = commandBuilderFactory;
        SqlGenerationHelper = sqlGenerationHelper;
        UpdateSqlGenerator = updateSqlGenerator;
        CurrentContext = currentContext;
        Logger = logger;
        UpdateLogger = updateLogger;
    }

    /// <summary>
    ///     A logger relational-command events.
    /// </summary>
    public IRelationalCommandDiagnosticsLogger Logger { get; init; }

    /// <summary>
    ///     A logger for general update events.
    /// </summary>
    public IDiagnosticsLogger<DbLoggerCategory.Update> UpdateLogger { get; }

    /// <summary>
    ///     The command builder factory.
    /// </summary>
    public IRelationalCommandBuilderFactory CommandBuilderFactory { get; init; }

    /// <summary>
    ///     The SQL generator helper.
    /// </summary>
    public ISqlGenerationHelper SqlGenerationHelper { get; init; }

    /// <summary>
    ///     The update SQL generator.
    /// </summary>
    public IUpdateSqlGenerator UpdateSqlGenerator { get; init; }

    /// <summary>
    ///     Contains the <see cref="DbContext" /> currently in use.
    /// </summary>
    public ICurrentDbContext CurrentContext { get; init; }
}
