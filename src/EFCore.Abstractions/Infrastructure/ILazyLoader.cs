// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;

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
            [NotNull] object entity,
            [NotNull] [CallerMemberName] string navigationName = "",
            bool loaded = true);

        /// <summary>
        ///     Loads a navigation property if it has not already been loaded.
        /// </summary>
        /// <param name="entity"> The entity on which the navigation property is located. </param>
        /// <param name="navigationName"> The navigation property name. </param>
        void Load([NotNull] object entity, [NotNull] [CallerMemberName] string navigationName = null);

        /// <summary>
        ///     Loads a navigation property if it has not already been loaded.
        /// </summary>
        /// <param name="entity"> The entity on which the navigation property is located. </param>
        /// <param name="cancellationToken"> A <see cref="CancellationToken" /> to observe while waiting for the task to complete. </param>
        /// <param name="navigationName"> The navigation property name. </param>
        /// <returns> A task that represents the asynchronous operation. </returns>
#pragma warning disable CA1068 // CancellationToken parameters must come last
        Task LoadAsync(
#pragma warning restore CA1068 // CancellationToken parameters must come last
            [NotNull] object entity,
            CancellationToken cancellationToken = default,
            [NotNull] [CallerMemberName] string navigationName = null);
    }
}
