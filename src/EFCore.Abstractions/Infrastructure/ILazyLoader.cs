// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Microsoft.EntityFrameworkCore.Infrastructure
{
    /// <summary>
    ///     A service that can be injected into entities to give them the capability
    ///     of loading navigation properties automatically the first time they are accessed.
    /// </summary>
    public interface ILazyLoader
    {
        /// <summary>
        ///     Loads a navigation property if it has not already been loaded.
        /// </summary>
        /// <param name="entity"> The entity on which the navigation property is located. </param>
        /// <param name="navigationName"> The navigation property name. </param>
        // ReSharper disable once AssignNullToNotNullAttribute
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
            // ReSharper disable once AssignNullToNotNullAttribute
            [NotNull] [CallerMemberName] string navigationName = null);
    }
}
