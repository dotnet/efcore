// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.EntityFrameworkCore.Diagnostics
{
    /// <summary>
    ///     <para>
    ///         Abstract base class for <see cref="ISaveChangesInterceptor" /> for use when implementing a subset
    ///         of the interface methods.
    ///     </para>
    /// </summary>
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
            => new ValueTask<InterceptionResult<int>>(result);

        /// <inheritdoc />
        public virtual ValueTask<int> SavedChangesAsync(
            SaveChangesCompletedEventData eventData,
            int result,
            CancellationToken cancellationToken = default)
            => new ValueTask<int>(result);

        /// <inheritdoc />
        public virtual Task SaveChangesFailedAsync(
            DbContextErrorEventData eventData,
            CancellationToken cancellationToken = default)
            => Task.CompletedTask;
    }
}
