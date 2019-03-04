// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using JetBrains.Annotations;

namespace Microsoft.EntityFrameworkCore.ChangeTracking.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public static class StateManagerExtensions
    {
        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static IReadOnlyList<InternalEntityEntry> ToListForState(
            [NotNull] this IStateManager stateManager,
            bool added = false,
            bool modified = false,
            bool deleted = false,
            bool unchanged = false)
        {
            var list = new List<InternalEntityEntry>(
                stateManager.GetCountForState(added, modified, deleted, unchanged));

            foreach (var entry in stateManager.GetEntriesForState(added, modified, deleted, unchanged))
            {
                list.Add(entry);
            }

            return list;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static IReadOnlyList<InternalEntityEntry> ToList(
            [NotNull] this IStateManager stateManager)
            => stateManager.ToListForState(added: true, modified: true, deleted: true, unchanged: true);
    }
}
