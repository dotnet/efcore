// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Diagnostics;

/// <summary>
///     Abstract base class for <see cref="ISaveChangesInterceptor" /> for use when implementing a subset
///     of the interface methods.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-interceptors">EF Core interceptors</see> for more information and examples.
/// </remarks>
public abstract class SaveChangesInterceptor : ISaveChangesInterceptor
{
    /// <inheritdoc />
    public virtual InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
        => result;

    /// <inheritdoc />
    public virtual int SavedChanges(SaveChangesCompletedEventData eventData, int result)
        => result;

    /// <inheritdoc />
    public virtual void SaveChangesFailed(DbContextErrorEventData eventData)
    {
    }

    /// <inheritdoc />
    public virtual ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
        => new(result);

    /// <inheritdoc />
    public virtual ValueTask<int> SavedChangesAsync(
        SaveChangesCompletedEventData eventData,
        int result,
        CancellationToken cancellationToken = default)
        => new(result);

    /// <inheritdoc />
    public virtual Task SaveChangesFailedAsync(DbContextErrorEventData eventData, CancellationToken cancellationToken = default)
        => Task.CompletedTask;

    /// <inheritdoc />
    public virtual void SaveChangesCanceled(DbContextEventData eventData)
    {
    }

    /// <inheritdoc />
    public virtual Task SaveChangesCanceledAsync(DbContextEventData eventData, CancellationToken cancellationToken = default)
        => Task.CompletedTask;

    /// <inheritdoc />
    public virtual InterceptionResult ThrowingConcurrencyException(ConcurrencyExceptionEventData eventData, InterceptionResult result)
        => result;

    /// <inheritdoc />
    public virtual ValueTask<InterceptionResult> ThrowingConcurrencyExceptionAsync(
        ConcurrencyExceptionEventData eventData,
        InterceptionResult result,
        CancellationToken cancellationToken = default)
        => new(result);
}
