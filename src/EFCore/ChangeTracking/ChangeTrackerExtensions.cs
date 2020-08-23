// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Update;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore
{
    /// <summary>
    ///     Extension methods for <see cref="ChangeTracker" />.
    /// </summary>
    public static class ChangeTrackerExtensions
    {
        /// <summary>
        ///     <para>
        ///         Creates a human-readable representation of the given metadata.
        ///     </para>
        ///     <para>
        ///         Warning: Do not rely on the format of the returned string.
        ///         It is designed for debugging only and may change arbitrarily between releases.
        ///     </para>
        /// </summary>
        /// <param name="changeTracker"> The metadata item. </param>
        /// <param name="options"> Options for generating the string. </param>
        /// <param name="indent"> The number of indent spaces to use before each new line. </param>
        /// <returns> A human-readable representation. </returns>
        public static string ToDebugString(
            [NotNull] this ChangeTracker changeTracker,
            ChangeTrackerDebugStringOptions options,
            int indent = 0)
        {
            var builder = new StringBuilder();
            var indentString = new string(' ', indent);

            var stateManager = changeTracker.Context.GetService<IStateManager>();
            foreach (var entry in stateManager.Entries.OrderBy(e => e, EntityEntryComparer.Instance))
            {
                builder.Append(indentString).AppendLine(entry.ToDebugString(options, indent));
            }

            return builder.ToString();
        }

        private sealed class EntityEntryComparer : IComparer<InternalEntityEntry>
        {
            public static readonly EntityEntryComparer Instance = new EntityEntryComparer();

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
