// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.ChangeTracking.Internal
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static class StateManagerExtensions
    {
        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
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
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public static IReadOnlyList<InternalEntityEntry> ToList(
            [NotNull] this IStateManager stateManager)
            => stateManager.ToListForState(added: true, modified: true, deleted: true, unchanged: true);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public static string ToDebugString([NotNull] this IStateManager stateManager, StateManagerDebugStringOptions options)
        {
            var builder = new StringBuilder();

            foreach (var entry in stateManager.Entries.OrderBy(e => e, EntityEntryComparer.Instance))
            {
                builder.AppendLine(entry.ToDebugString(options));
            }

            return builder.ToString();
        }

        private sealed class EntityEntryComparer : IComparer<InternalEntityEntry>
        {
            public static EntityEntryComparer Instance = new EntityEntryComparer();

            public int Compare(InternalEntityEntry x, InternalEntityEntry y)
            {
                var result = StringComparer.InvariantCulture.Compare(x.EntityType.Name, y.EntityType.Name);
                if (result != 0)
                {
                    return result;
                }

                var primaryKey = x.EntityType.FindPrimaryKey();
                if (primaryKey != null)
                {
                    var keyProperties = primaryKey.Properties;
                    foreach (var keyProperty in keyProperties)
                    {
                        var comparer = keyProperty.GetCurrentValueComparer();
                        result = comparer.Compare(x, y);
                        if (result != 0)
                        {
                            return result;
                        }
                    }
                }

                return 0;
            }
        }
    }
}
