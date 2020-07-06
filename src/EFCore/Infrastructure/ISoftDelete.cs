// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.EntityFrameworkCore.Infrastructure
{
    /// <summary>
    /// An interface to implement soft delete on model
    /// </summary>
    public interface ISoftDelete
    {
        /// <summary>
        /// To save record soft delete time
        /// </summary>
        public DateTime? DeletedAt { get; set; }

        /// <summary>
        /// flag to check record should force delete or soft delete
        /// </summary>
        protected internal bool ForceDelete { get; set; }

        /// <summary>
        /// Set record should delete from database or just soft delete
        /// </summary>
        /// <param name="shouldDelete">set false if you need to soft delete as force delete record</param>
        public void SetForceDelete(bool shouldDelete = true)
        {
            ForceDelete = shouldDelete;
        }

        /// <summary>
        /// Check record will force delete or soft delete
        /// </summary>
        /// <returns>true if record will force delete. </returns>
        /// <returns>false if record will soft delete. </returns>
        public bool GetForcedDelete()
        {
            return ForceDelete;
        }

        /// <summary>
        /// The function that calls on deleting record
        /// </summary>
        /// <param name="context">Application DbContext</param>
        /// <param name="resolveRecursive">should resolve and load relation</param>
        /// <param name="cancellationToken">cancellation token</param>
        /// <returns>Task</returns>
        public Task OnSoftDeleteAsync(
            DbContext context, bool resolveRecursive = false,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// The function that calls on deleting record
        /// </summary>
        /// <param name="context">Application DbContext</param>
        /// <param name="resolveRecursive">should resolve and load relation</param>
        public void OnSoftDelete(
            DbContext context, bool resolveRecursive = false);

        /// <summary>
        /// check this entity has Delete flag in DbContext
        /// </summary>
        /// <param name="context">Application DbContext</param>
        /// <returns>entity has delete flag in DbContext</returns>
        public bool WillDelete(DbContext context)
        {
            return context.Entry(this).State == EntityState.Deleted;
        }
    }
}
