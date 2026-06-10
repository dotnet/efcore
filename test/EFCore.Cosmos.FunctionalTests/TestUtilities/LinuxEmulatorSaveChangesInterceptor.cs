// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Microsoft.EntityFrameworkCore.TestUtilities;

/// <summary>
///     Interceptor that forces <see cref="AutoTransactionBehavior.Never" /> on every SaveChanges call.
///     The Linux (vnext) Cosmos emulator does not support transactional batches, so this interceptor
///     ensures all writes are sent individually rather than batched.
/// </summary>
public sealed class LinuxEmulatorSaveChangesInterceptor : SaveChangesInterceptor
{
    public static LinuxEmulatorSaveChangesInterceptor Instance { get; } = new();

    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        if (eventData.Context != null)
        {
            eventData.Context.Database.AutoTransactionBehavior = AutoTransactionBehavior.Never;
        }

        return result;
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        if (eventData.Context != null)
        {
            eventData.Context.Database.AutoTransactionBehavior = AutoTransactionBehavior.Never;
        }

        return ValueTask.FromResult(result);
    }
}
