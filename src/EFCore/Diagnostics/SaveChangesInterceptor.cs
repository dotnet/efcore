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
        /// <summary>
        ///     Called at the start of <see cref="M:DbContext.SaveChanges" />.
        /// </summary>
        /// <param name="eventData"> Contextual information about the <see cref="DbContext" /> being used. </param>
        /// <param name="result">
        ///     Represents the current result if one exists.
        ///     This value will have <see cref="InterceptionResult{Int32}.HasResult" /> set to <see langword="true" /> if some previous
        ///     interceptor suppressed execution by calling <see cref="InterceptionResult{Int32}.SuppressWithResult" />.
        ///     This value is typically used as the return value for the implementation of this method.
        /// </param>
        /// <returns>
        ///     If <see cref="InterceptionResult{Int32}.HasResult" /> is false, the EF will continue as normal.
        ///     If <see cref="InterceptionResult{Int32}.HasResult" /> is true, then EF will suppress the operation it
        ///     was about to perform and use <see cref="InterceptionResult{Int32}.Result" /> instead.
        ///     A normal implementation of this method for any interceptor that is not attempting to change the result
        ///     is to return the <paramref name="result" /> value passed in.
        /// </returns>
        public virtual InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
            => result;

        /// <summary>
        ///     <para>
        ///         Called at the end of <see cref="M:DbContext.SaveChanges" />.
        ///     </para>
        ///     <para>
        ///         This method is still called if an interceptor suppressed creation of a command in
        ///         <see cref="ISaveChangesInterceptor.SavingChanges" />.
        ///         In this case, <paramref name="result" /> is the result returned by <see cref="ISaveChangesInterceptor.SavingChanges" />.
        ///     </para>
        /// </summary>
        /// <param name="eventData"> Contextual information about the <see cref="DbContext" /> being used. </param>
        /// <param name="result">
        ///     The result of the call to <see cref="M:DbContext.SaveChanges" />.
        ///     This value is typically used as the return value for the implementation of this method.
        /// </param>
        /// <returns>
        ///     The result that EF will use.
        ///     A normal implementation of this method for any interceptor that is not attempting to change the result
        ///     is to return the <paramref name="result" /> value passed in.
        /// </returns>
        public virtual int SavedChanges(SaveChangesCompletedEventData eventData, int result)
            => result;

        /// <summary>
        ///     Called when an exception has been thrown in <see cref="M:DbContext.SaveChanges" />.
        /// </summary>
        /// <param name="eventData"> Contextual information about the failure. </param>
        public virtual void SaveChangesFailed(DbContextErrorEventData eventData)
        {
        }

        /// <summary>
        ///     Called at the start of <see cref="M:DbContext.SaveChangesAsync" />.
        /// </summary>
        /// <param name="eventData"> Contextual information about the <see cref="DbContext" /> being used. </param>
        /// <param name="result">
        ///     Represents the current result if one exists.
        ///     This value will have <see cref="InterceptionResult{Int32}.HasResult" /> set to <see langword="true" /> if some previous
        ///     interceptor suppressed execution by calling <see cref="InterceptionResult{Int32}.SuppressWithResult" />.
        ///     This value is typically used as the return value for the implementation of this method.
        /// </param>
        /// <param name="cancellationToken"> The cancellation token. </param>
        /// <returns>
        ///     If <see cref="InterceptionResult{Int32}.HasResult" /> is false, the EF will continue as normal.
        ///     If <see cref="InterceptionResult{Int32}.HasResult" /> is true, then EF will suppress the operation it
        ///     was about to perform and use <see cref="InterceptionResult{Int32}.Result" /> instead.
        ///     A normal implementation of this method for any interceptor that is not attempting to change the result
        ///     is to return the <paramref name="result" /> value passed in.
        /// </returns>
        public virtual ValueTask<InterceptionResult<int>> SavingChangesAsync(
            DbContextEventData eventData,
            InterceptionResult<int> result,
            CancellationToken cancellationToken = default)
            => new ValueTask<InterceptionResult<int>>(result);

        /// <summary>
        ///     <para>
        ///         Called at the end of <see cref="M:DbContext.SaveChangesAsync" />.
        ///     </para>
        ///     <para>
        ///         This method is still called if an interceptor suppressed creation of a command in
        ///         <see cref="ISaveChangesInterceptor.SavingChangesAsync" />.
        ///         In this case, <paramref name="result" /> is the result returned by <see cref="ISaveChangesInterceptor.SavingChangesAsync" />.
        ///     </para>
        /// </summary>
        /// <param name="eventData"> Contextual information about the <see cref="DbContext" /> being used. </param>
        /// <param name="result">
        ///     The result of the call to <see cref="M:DbContext.SaveChangesAsync" />.
        ///     This value is typically used as the return value for the implementation of this method.
        /// </param>
        /// <param name="cancellationToken"> The cancellation token. </param>
        /// <returns>
        ///     The result that EF will use.
        ///     A normal implementation of this method for any interceptor that is not attempting to change the result
        ///     is to return the <paramref name="result" /> value passed in.
        /// </returns>
        public virtual ValueTask<int> SavedChangesAsync(
            SaveChangesCompletedEventData eventData,
            int result,
            CancellationToken cancellationToken = default)
            => new ValueTask<int>(result);

        /// <summary>
        ///     Called when an exception has been thrown in <see cref="M:DbContext.SaveChangesAsync" />.
        /// </summary>
        /// <param name="eventData"> Contextual information about the failure. </param>
        /// <param name="cancellationToken"> The cancellation token. </param>
        /// <returns> A <see cref="Task" /> representing the asynchronous operation. </returns>
        public virtual Task SaveChangesFailedAsync(
            DbContextErrorEventData eventData,
            CancellationToken cancellationToken = default)
            => Task.CompletedTask;
    }
}
