// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Update;

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
        public static string ToDebugString(
            [NotNull] this IStateManager stateManager,
            StateManagerDebugStringOptions options,
            [NotNull] string indent = "")
        {
            var builder = new StringBuilder();

            void AppendValue(object value)
            {
                if (value == null)
                {
                    builder.Append("<null>");
                }
                else if (value.GetType().IsNumeric())
                {
                    builder.Append(value);
                }
                else if (value is byte[] bytes)
                {
                    builder.AppendBytes(bytes);
                }
                else
                {
                    var stringValue = value.ToString();
                    if (stringValue.Length > 63)
                    {
                        stringValue = stringValue.Substring(0, 60) + "...";
                    }

                    builder
                        .Append('\'')
                        .Append(stringValue)
                        .Append('\'');
                }
            }

            void AppendRelatedKey(IEntityType targetType, object value)
            {
                var otherEntry = stateManager.TryGetEntry(value, targetType, throwOnTypeMismatch: false);

                builder.Append(
                    otherEntry == null
                        ? "<not found>"
                        : otherEntry.BuildCurrentValuesString(targetType.FindPrimaryKey().Properties));
            }

            foreach (var item in stateManager.Entries
                .Select(
                    e =>
                        new
                        {
                            KeyString = e.BuildCurrentValuesString(e.EntityType.FindPrimaryKey().Properties),
                            Entry = e
                        })
                .OrderBy(e => e.Entry.EntityType.DisplayName())
                .ThenBy(e => e.KeyString))
            {
                var entry = item.Entry;
                var entityType = entry.EntityType;

                builder
                    .Append(entityType.DisplayName())
                    .Append(' ')
                    .Append(item.KeyString)
                    .Append(' ')
                    .Append(entry.EntityState.ToString());

                if ((options & StateManagerDebugStringOptions.IncludeProperties) != 0)
                {
                    builder.AppendLine();

                    foreach (var property in entityType.GetProperties())
                    {
                        var currentValue = entry.GetCurrentValue(property);
                        builder
                            .Append("  ")
                            .Append(property.Name)
                            .Append(": ");

                        AppendValue(currentValue);

                        if (property.IsPrimaryKey())
                        {
                            builder.Append(" PK");
                        }
                        else if (property.IsKey())
                        {
                            builder.Append(" AK");
                        }

                        if (property.IsForeignKey())
                        {
                            builder.Append(" FK");
                        }

                        if (entry.IsModified(property))
                        {
                            builder.Append(" Modified");
                        }

                        if (entry.HasTemporaryValue(property))
                        {
                            builder.Append(" Temporary");
                        }

                        if (entry.HasOriginalValuesSnapshot
                            && property.GetOriginalValueIndex() != -1)
                        {
                            var originalValue = entry.GetOriginalValue(property);
                            if (!Equals(originalValue, currentValue))
                            {
                                builder.Append(" Originally ");
                                AppendValue(originalValue);
                            }
                        }

                        builder.AppendLine();
                    }
                }
                else
                {
                    foreach (var alternateKey in entityType.GetKeys().Where(k => !k.IsPrimaryKey()))
                    {
                        builder
                            .Append(" AK ")
                            .Append(entry.BuildCurrentValuesString(alternateKey.Properties));
                    }

                    foreach (var foreignKey in entityType.GetForeignKeys())
                    {
                        builder
                            .Append(" FK ")
                            .Append(entry.BuildCurrentValuesString(foreignKey.Properties));
                    }

                    builder.AppendLine();
                }

                if ((options & StateManagerDebugStringOptions.IncludeNavigations) != 0)
                {
                    foreach (var navigation in entityType.GetNavigations())
                    {
                        var currentValue = entry.GetCurrentValue(navigation);
                        var targetType = navigation.GetTargetType();

                        builder
                            .Append("  ")
                            .Append(navigation.Name)
                            .Append(": ");

                        if (currentValue == null)
                        {
                            builder.Append("<null>");
                        }
                        else if (navigation.IsCollection())
                        {
                            builder.Append('[');

                            var relatedEntities = ((IEnumerable)currentValue).Cast<object>().Take(33).ToList();

                            for (var i = 0; i < relatedEntities.Count; i++)
                            {
                                if (i != 0)
                                {
                                    builder.Append(", ");
                                }

                                if (i < 32)
                                {
                                    AppendRelatedKey(targetType, relatedEntities[i]);
                                }
                                else
                                {
                                    builder.Append("...");
                                }
                            }

                            builder.Append(']');
                        }
                        else
                        {
                            AppendRelatedKey(targetType, currentValue);
                        }

                        builder.AppendLine();
                    }
                }
            }

            return builder.ToString();
        }
    }
}
