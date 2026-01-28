// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Transactions;
using Microsoft.EntityFrameworkCore.InMemory.Internal;
using IsolationLevel = System.Data.IsolationLevel;

namespace Microsoft.EntityFrameworkCore.InMemory.Storage.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class InMemoryTransactionManager : IDbContextTransactionManager, ITransactionEnlistmentManager
{
    private static readonly InMemoryTransaction StubTransaction = new();

    private readonly IDiagnosticsLogger<DbLoggerCategory.Database.Transaction> _logger;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public InMemoryTransactionManager(
        IDiagnosticsLogger<DbLoggerCategory.Database.Transaction> logger)
        => _logger = logger;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual IDbContextTransaction BeginTransaction()
    {
        _logger.TransactionIgnoredWarning();

        return StubTransaction;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual Task<IDbContextTransaction> BeginTransactionAsync(
        CancellationToken cancellationToken = default)
    {
        _logger.TransactionIgnoredWarning();

        return Task.FromResult<IDbContextTransaction>(StubTransaction);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    /// <remarks>
    ///     The <paramref name="isolationLevel" /> parameter is ignored by the in-memory provider since
    ///     transactions are not supported. This method exists to allow code that uses transactions with
    ///     isolation levels to work seamlessly with the in-memory provider for testing purposes.
    /// </remarks>
    /// <param name="isolationLevel">The <see cref="IsolationLevel" /> to use (ignored by this provider).</param>
    /// <returns>A <see cref="IDbContextTransaction" /> that represents a no-op transaction.</returns>
    public virtual IDbContextTransaction BeginTransaction(IsolationLevel isolationLevel)
        => BeginTransaction();

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    /// <remarks>
    ///     The <paramref name="isolationLevel" /> parameter is ignored by the in-memory provider since
    ///     transactions are not supported. This method exists to allow code that uses transactions with
    ///     isolation levels to work seamlessly with the in-memory provider for testing purposes.
    /// </remarks>
    /// <param name="isolationLevel">The <see cref="IsolationLevel" /> to use (ignored by this provider).</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
    /// <returns>
    ///     A task that represents the asynchronous operation. The task result contains a
    ///     <see cref="IDbContextTransaction" /> that represents a no-op transaction.
    /// </returns>
    public virtual Task<IDbContextTransaction> BeginTransactionAsync(
        IsolationLevel isolationLevel, 
        CancellationToken cancellationToken = default)
        => BeginTransactionAsync(cancellationToken);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual void CommitTransaction()
        => _logger.TransactionIgnoredWarning();

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        _logger.TransactionIgnoredWarning();
        return Task.CompletedTask;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual void RollbackTransaction()
        => _logger.TransactionIgnoredWarning();

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        _logger.TransactionIgnoredWarning();
        return Task.CompletedTask;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual IDbContextTransaction? CurrentTransaction
        => null;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual Transaction? EnlistedTransaction
        => null;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual void EnlistTransaction(Transaction? transaction)
        => _logger.TransactionIgnoredWarning();

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual void ResetState()
    {
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual Task ResetStateAsync(CancellationToken cancellationToken = default)
    {
        ResetState();

        return Task.CompletedTask;
    }
}
