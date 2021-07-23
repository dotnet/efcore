// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.EntityFrameworkCore.Infrastructure
{
    /// <summary>
    ///     <para>
    ///         A service that can be injected into entities to give them the capability
    ///         of loading navigation properties automatically the first time they are accessed.
    ///     </para>
    ///     <para>
    ///         The service lifetime is 'ServiceLifetime.Transient'. This means that each
    ///         entity instance will use its own instance of this service.
    ///         The implementation may depend on other services registered with any lifetime.
    ///         The implementation does not need to be thread-safe.
    ///     </para>
    /// </summary>
    public interface ILazyLoader : IDisposable
    {
        /// <summary>
        ///     Sets the given navigation as known to be completely loaded or known to be
        ///     no longer completely loaded.
        /// </summary>
        /// <param name="entity"> The entity on which the navigation property is located. </param>
        /// <param name="navigationName"> The navigation property name. </param>
        /// <param name="loaded"> Determines whether the navigation is set as loaded or not. </param>
        void SetLoaded(
            object entity,
            [CallerMemberName] string navigationName = "",
            bool loaded = true);

        /// <summary>
        ///     Loads a navigation property if it has not already been loaded.
        /// </summary>
        /// <param name="entity"> The entity on which the navigation property is located. </param>
        /// <param name="navigationName"> The navigation property name. </param>
        void Load(object entity, [CallerMemberName] string navigationName = "");

        /// <summary>
        ///     Loads a navigation property if it has not already been loaded.
        /// </summary>
        /// <param name="entity"> The entity on which the navigation property is located. </param>
        /// <param name="cancellationToken"> A <see cref="CancellationToken" /> to observe while waiting for the task to complete. </param>
        /// <param name="navigationName"> The navigation property name. </param>
        /// <returns> A task that represents the asynchronous operation. </returns>
        /// <exception cref="OperationCanceledException"> If the <see cref="CancellationToken"/> is canceled. </exception>
#pragma warning disable CA1068 // CancellationToken parameters must come last
        Task LoadAsync(
#pragma warning restore CA1068 // CancellationToken parameters must come last
            object entity,
            CancellationToken cancellationToken = default,
            [CallerMemberName] string navigationName = "");
    }
}
