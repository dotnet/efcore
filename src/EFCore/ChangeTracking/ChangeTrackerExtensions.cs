// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
    /// <seealso href="https://aka.ms/efcore-docs-change-tracking">Documentation for EF Core change tracking.</seealso>
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
            this ChangeTracker changeTracker,
            ChangeTrackerDebugStringOptions options = ChangeTrackerDebugStringOptions.LongDefault,
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
            public static readonly EntityEntryComparer Instance = new();

            public int Compare(InternalEntityEntry? x, InternalEntityEntry? y)
            {
                if (ReferenceEquals(x, y))
                {
                    return 0;
                }

                if (x is null)
                {
                    return -1;
                }

                if (y is null)
                {
                    return 1;
                }

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
