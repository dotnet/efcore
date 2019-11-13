// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.EntityFrameworkCore.Diagnostics
{
    /// <summary>
    ///     <para>
    ///         Allows interception of operations related to a <see cref="SaveChangesInterceptor" />.
    ///     </para>
    ///     <para>
    ///         Save Changes interceptors can be used to view, change, or suppress operations on <see cref="SaveChangesInterceptor" />, and
    ///         to modify the result before it is returned to EF.
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
        /// Invoked just before execution the <see cref="DbContext.SaveChanges()" />.
        /// </summary>
        /// <param name="eventData">Event data.</param>
        void SavingChanges([NotNull] DbContextEventData eventData);

        /// <summary>
        /// Invoked just before the execution of <see cref="DbContext.SaveChangesAsync(CancellationToken)" />.
        /// </summary>
        /// <param name="eventData">Event data</param>
        /// <param name="result">Result</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        Task<InterceptionResult<int>> SavingChangesAsync([NotNull] DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default);

        /// <summary>
        /// Invoked just after the execution of <see cref="DbContext.SaveChanges()" />
        /// </summary>
        /// <param name="eventData">Event data</param>
        /// <param name="result">Result</param>
        /// <returns>Entities saved.</returns>
        int SavedChanges([NotNull] SaveChangesCompletedEventData eventData, int result);

        /// <summary>
        /// Invoked just after the execution of <see cref="DbContext.SaveChangesAsync(CancellationToken)" />.
        /// </summary>
        /// <param name="eventData">Event data</param>
        /// <param name="result">Result</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Entities Saved</returns>
        Task<int> SavedChangesAsync([NotNull] SaveChangesCompletedEventData eventData, int result, CancellationToken cancellationToken = default);

        /// <summary>
        /// Invoked if <see cref="DbContext.SaveChanges()" /> or <see cref="DbContext.SaveChangesAsync(CancellationToken)" /> failed.
        /// </summary>
        /// <param name="eventData">Event data</param>
        void SavingChangesFailed([NotNull] DbContextErrorEventData eventData);
    }
}
