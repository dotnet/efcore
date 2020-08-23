// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Microsoft.EntityFrameworkCore.Diagnostics
{
    /// <summary>
    ///     <para>
    ///         Allows interception of the <see cref="M:DbContext.SaveChanges" /> and <see cref="M:DbContext.SaveChangesAync" /> methods.
    ///     </para>
    ///     <para>
    ///         Command interceptors can be used to view, change, or suppress execution of the SaveChanges call and
    ///         modify the result before it is returned to EF.
    ///     </para>
    ///     <para>
    ///         Consider inheriting from <see cref="SaveChangesInterceptor" /> if not implementing all methods.
    ///     </para>
    ///     <para>
    ///         Use <see cref="DbContextOptionsBuilder.AddInterceptors(Microsoft.EntityFrameworkCore.Diagnostics.IInterceptor[])" />
    ///         to register application interceptors.
    ///     </para>
    ///     <para>
    ///         Extensions can also register interceptors in the internal service provider.
    ///         If both injected and application interceptors are found, then the injected interceptors are run in the
    ///         order that they are resolved from the service provider, and then the application interceptors are run last.
    ///     </para>
    /// </summary>
    public interface ISaveChangesInterceptor : IInterceptor
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
        InterceptionResult<int> SavingChanges(
            [NotNull] DbContextEventData eventData,
            InterceptionResult<int> result);

        /// <summary>
        ///     <para>
        ///         Called at the end of <see cref="M:DbContext.SaveChanges" />.
        ///     </para>
        ///     <para>
        ///         This method is still called if an interceptor suppressed creation of a command in <see cref="SavingChanges" />.
        ///         In this case, <paramref name="result" /> is the result returned by <see cref="SavingChanges" />.
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
        int SavedChanges(
            [NotNull] SaveChangesCompletedEventData eventData,
            int result);

        /// <summary>
        ///     Called when an exception has been thrown in <see cref="M:DbContext.SaveChanges" />.
        /// </summary>
        /// <param name="eventData"> Contextual information about the failure. </param>
        void SaveChangesFailed(
            [NotNull] DbContextErrorEventData eventData);

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
        ValueTask<InterceptionResult<int>> SavingChangesAsync(
            [NotNull] DbContextEventData eventData,
            InterceptionResult<int> result,
            CancellationToken cancellationToken = default);

        /// <summary>
        ///     <para>
        ///         Called at the end of <see cref="M:DbContext.SaveChangesAsync" />.
        ///     </para>
        ///     <para>
        ///         This method is still called if an interceptor suppressed creation of a command in <see cref="SavingChangesAsync" />.
        ///         In this case, <paramref name="result" /> is the result returned by <see cref="SavingChangesAsync" />.
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
        ValueTask<int> SavedChangesAsync(
            [NotNull] SaveChangesCompletedEventData eventData,
            int result,
            CancellationToken cancellationToken = default);

        /// <summary>
        ///     Called when an exception has been thrown in <see cref="M:DbContext.SaveChangesAsync" />.
        /// </summary>
        /// <param name="eventData"> Contextual information about the failure. </param>
        /// <param name="cancellationToken"> The cancellation token. </param>
        /// <returns> A <see cref="Task" /> representing the asynchronous operation. </returns>
        Task SaveChangesFailedAsync(
            [NotNull] DbContextErrorEventData eventData,
            CancellationToken cancellationToken = default);
    }
}
