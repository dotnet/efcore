// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.EntityFrameworkCore.Infrastructure
{
    /// <summary>
    /// An extension for application models.
    /// Should use when you need soft delete
    /// </summary>
    public abstract class ModelExtension : ITimestamps, ISoftDelete
    {
        /// <summary>
        /// Main primary key of model
        /// </summary>
        public long Id { get; set; }

        /// <inheritdoc />
        public DateTime CreatedAt { get; set; }

        /// <inheritdoc />
        public DateTime UpdatedAt { get; set; }

        /// <inheritdoc />
        [DefaultValue(value: null)]
        public DateTime? DeletedAt { get; set; }

        /// <inheritdoc />
        bool ISoftDelete.ForceDelete { get; set; }

        /// <inheritdoc />
        public void SetForceDelete(bool shouldDelete = true)
        {
            (this as ISoftDelete).ForceDelete = shouldDelete;
        }

        /// <inheritdoc />
        public bool GetForcedDelete()
        {
            return (this as ISoftDelete).ForceDelete;
        }

        /// <inheritdoc />
        public abstract Task OnSoftDeleteAsync(
            DbContext context, bool resolveRecursive = false,
            CancellationToken cancellationToken = default);

        /// <inheritdoc />
        public abstract void OnSoftDelete(
            DbContext context, bool resolveRecursive = false);

        /// <inheritdoc />
        public bool WillDelete(DbContext context)
        {
            return context.Entry(this).State == EntityState.Deleted;
        }

        /// <summary>
        /// Load needed relation to delete on soft deleting a record
        /// </summary>
        /// <param name="context">Application DbContext</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Task</returns>
        public abstract Task LoadRelationsAsync(DbContext context, CancellationToken cancellationToken = default);

        /// <summary>
        /// Load needed relation to delete on soft deleting a record
        /// </summary>
        /// <param name="context">Application DbContext</param>
        public abstract void LoadRelations(DbContext context);
    }
}
