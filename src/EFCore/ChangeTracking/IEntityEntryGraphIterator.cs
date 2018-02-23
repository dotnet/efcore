// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Microsoft.EntityFrameworkCore.ChangeTracking
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public interface IEntityEntryGraphIterator
    {
        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        void TraverseGraph(
            [NotNull] EntityEntryGraphNode node,
            [CanBeNull] object state,
            [NotNull] Func<EntityEntryGraphNode, object, bool> handleNode);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        Task TraverseGraphAsync(
            [NotNull] EntityEntryGraphNode node,
            [CanBeNull] object state,
            [NotNull] Func<EntityEntryGraphNode, object, CancellationToken, Task<bool>> handleNode,
            CancellationToken cancellationToken = default);
    }
}
