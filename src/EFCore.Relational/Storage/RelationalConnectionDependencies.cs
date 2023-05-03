// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Storage.Internal;

namespace Microsoft.EntityFrameworkCore.Storage;

/// <summary>
///     <para>
///         Service dependencies parameter class for <see cref="RelationalConnection" />
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
public sealed record RelationalConnectionDependencies
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
    public RelationalConnectionDependencies(
        IDbContextOptions contextOptions,
        IDiagnosticsLogger<DbLoggerCategory.Database.Transaction> transactionLogger,
        IRelationalConnectionDiagnosticsLogger connectionLogger,
        INamedConnectionStringResolver connectionStringResolver,
        IRelationalTransactionFactory relationalTransactionFactory,
        ICurrentDbContext currentContext,
        IRelationalCommandBuilderFactory relationalCommandBuilderFactory)
    {
        ContextOptions = contextOptions;
        TransactionLogger = transactionLogger;
        ConnectionLogger = connectionLogger;
        ConnectionStringResolver = connectionStringResolver;
        RelationalTransactionFactory = relationalTransactionFactory;
        CurrentContext = currentContext;
        RelationalCommandBuilderFactory = relationalCommandBuilderFactory;
    }

    /// <summary>
    ///     The options for the current context instance.
    /// </summary>
    public IDbContextOptions ContextOptions { get; init; }

    /// <summary>
    ///     The logger to which transaction messages will be written.
    /// </summary>
    public IDiagnosticsLogger<DbLoggerCategory.Database.Transaction> TransactionLogger { get; init; }

    /// <summary>
    ///     The logger to which connection messages will be written.
    /// </summary>
    public IRelationalConnectionDiagnosticsLogger ConnectionLogger { get; init; }

    /// <summary>
    ///     A service for resolving a connection string from a name.
    /// </summary>
    [EntityFrameworkInternal]
    public INamedConnectionStringResolver ConnectionStringResolver { get; init; }

    /// <summary>
    ///     A service for creating <see cref="RelationalTransaction" /> instances.
    /// </summary>
    public IRelationalTransactionFactory RelationalTransactionFactory { get; init; }

    /// <summary>
    ///     Contains the <see cref="DbContext" /> instance currently in use.
    /// </summary>
    public ICurrentDbContext CurrentContext { get; init; }

    /// <summary>
    ///     Contains the <see cref="DbContext" /> instance currently in use.
    /// </summary>
    public IRelationalCommandBuilderFactory RelationalCommandBuilderFactory { get; init; }
}
