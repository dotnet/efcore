// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Microsoft.EntityFrameworkCore.ChangeTracking
{
    /// <summary>
    ///     <para>
    ///         Provides access to change tracking and loading information for a reference (i.e. non-collection)
    ///         navigation property that associates this entity to another entity.
    ///     </para>
    ///     <para>
    ///         Instances of this class are returned from methods when using the <see cref="ChangeTracker" /> API and it is
    ///         not designed to be directly constructed in your application code.
    ///     </para>
    /// </summary>
    public class ReferenceEntry : NavigationEntry
    {
        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public ReferenceEntry([NotNull] InternalEntityEntry internalEntry, [NotNull] string name)
            : base(internalEntry, name, collection: false)
        {
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public ReferenceEntry([NotNull] InternalEntityEntry internalEntry, [NotNull] INavigation navigation)
            : base(internalEntry, navigation)
        {
        }

        /// <summary>
        ///     <para>
        ///         Loads the entity referenced by this navigation property, unless it is already being tracked,
        ///         in which case calling the method is a no-op.
        ///     </para>
        /// </summary>
        public override void Load()
        {
            if (InternalEntry[Metadata] == null)
            {
                base.Load();
            }
        }

        /// <summary>
        ///     <para>
        ///         Loads the entity referenced by this navigation property, unless it is already being tracked,
        ///         in which case calling the method is a no-op.
        ///     </para>
        ///     <para>
        ///         Multiple active operations on the same context instance are not supported.  Use 'await' to ensure
        ///         that any asynchronous operations have completed before calling another method on this context.
        ///     </para>
        /// </summary>
        /// <param name="cancellationToken">
        ///     A <see cref="CancellationToken" /> to observe while waiting for the task to complete.
        /// </param>
        /// <returns>
        ///     A task that represents the asynchronous save operation.
        /// </returns>
        public override Task LoadAsync(CancellationToken cancellationToken = new CancellationToken())
            => InternalEntry[Metadata] == null
                ? base.LoadAsync(cancellationToken)
                : Task.FromResult(0);
    }
}
