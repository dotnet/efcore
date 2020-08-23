// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.Update
{
    /// <summary>
    ///     Extension methods for <see cref="IUpdateEntry" />.
    /// </summary>
    public static class UpdateEntryExtensions
    {
        /// <summary>
        ///     Gets the value assigned to the property and converts it to the provider-expected value.
        /// </summary>
        /// <param name="updateEntry"> The entry. </param>
        /// <param name="property"> The property to get the value for. </param>
        /// <returns> The value for the property. </returns>
        public static object GetCurrentProviderValue([NotNull] this IUpdateEntry updateEntry, [NotNull] IProperty property)
        {
            var value = updateEntry.GetCurrentValue(property);
            var typeMapping = property.GetTypeMapping();
            value = value?.GetType().IsInteger() == true && typeMapping.ClrType.UnwrapNullableType().IsEnum
                ? Enum.ToObject(typeMapping.ClrType.UnwrapNullableType(), value)
                : value;

            var converter = typeMapping.Converter;
            if (converter != null)
            {
                value = converter.ConvertToProvider(value);
            }

            return value;
        }

        /// <summary>
        ///     <para>
        ///         Creates a human-readable representation of the given <see cref="IUpdateEntry" />.
        ///     </para>
        ///     <para>
        ///         Warning: Do not rely on the format of the returned string.
        ///         It is designed for debugging only and may change arbitrarily between releases.
        ///     </para>
        /// </summary>
        /// <param name="updateEntry"> The entry. </param>
        /// <param name="options"> Options for generating the string. </param>
        /// <param name="indent"> The number of indent spaces to use before each new line. </param>
        /// <returns> A human-readable representation. </returns>
        public static string ToDebugString(
            [NotNull] this IUpdateEntry updateEntry,
            ChangeTrackerDebugStringOptions options,
            int indent = 0)
        {
            var builder = new StringBuilder();
            var indentString = new string(' ', indent);

            var entry = (InternalEntityEntry)updateEntry;

            var keyString = entry.BuildCurrentValuesString(entry.EntityType.FindPrimaryKey().Properties);

            builder
                .Append(entry.EntityType.DisplayName())
                .Append(' ')
                .Append(entry.SharedIdentityEntry != null ? "(Shared) " : "")
                .Append(keyString)
                .Append(' ')
                .Append(entry.EntityState.ToString());

            if ((options & ChangeTrackerDebugStringOptions.IncludeProperties) != 0)
            {
                foreach (var property in entry.EntityType.GetProperties())
                {
                    builder.AppendLine().Append(indentString);

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
                }
            }
            else
            {
                foreach (var alternateKey in entry.EntityType.GetKeys().Where(k => !k.IsPrimaryKey()))
                {
                    builder
                        .Append(" AK ")
                        .Append(entry.BuildCurrentValuesString(alternateKey.Properties));
                }

                foreach (var foreignKey in entry.EntityType.GetForeignKeys())
                {
                    builder
                        .Append(" FK ")
                        .Append(entry.BuildCurrentValuesString(foreignKey.Properties));
                }
            }

            if ((options & ChangeTrackerDebugStringOptions.IncludeNavigations) != 0)
            {
                foreach (var navigation in entry.EntityType.GetNavigations()
                    .Concat<INavigationBase>(entry.EntityType.GetSkipNavigations()))
                {
                    builder.AppendLine().Append(indentString);

                    var currentValue = entry.GetCurrentValue(navigation);
                    var targetType = navigation.TargetEntityType;

                    builder
                        .Append("  ")
                        .Append(navigation.Name)
                        .Append(": ");

                    if (currentValue == null)
                    {
                        builder.Append("<null>");
                    }
                    else if (navigation.IsCollection)
                    {
                        builder.Append('[');

                        const int maxRelatedToShow = 32;
                        var relatedEntities = ((IEnumerable)currentValue).Cast<object>().Take(maxRelatedToShow + 1).ToList();

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
                }
            }

            return builder.ToString();

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
                var otherEntry = entry.StateManager.TryGetEntry(value, targetType, throwOnTypeMismatch: false);

                builder.Append(
                    otherEntry == null
                        ? "<not found>"
                        : otherEntry.BuildCurrentValuesString(targetType.FindPrimaryKey().Properties));
            }
        }

        /// <summary>
        ///     Creates a formatted string representation of the given properties and their current
        ///     values such as is useful when throwing exceptions about keys, indexes, etc. that use
        ///     the properties.
        /// </summary>
        /// <param name="entry"> The entry from which values will be obtained. </param>
        /// <param name="properties"> The properties to format. </param>
        /// <returns> The string representation. </returns>
        public static string BuildCurrentValuesString(
            [NotNull] this IUpdateEntry entry,
            [NotNull] IEnumerable<IPropertyBase> properties)
            => "{"
                + string.Join(
                    ", ", properties.Select(
                        p =>
                        {
                            var currentValue = entry.GetCurrentValue(p);
                            return p.Name
                                + ": "
                                + (currentValue == null
                                    ? "<null>"
                                    : Convert.ToString(currentValue, CultureInfo.InvariantCulture));
                        }))
                + "}";

        /// <summary>
        ///     Creates a formatted string representation of the given properties and their original
        ///     values such as is useful when throwing exceptions about keys, indexes, etc. that use
        ///     the properties.
        /// </summary>
        /// <param name="entry"> The entry from which values will be obtained. </param>
        /// <param name="properties"> The properties to format. </param>
        /// <returns> The string representation. </returns>
        public static string BuildOriginalValuesString(
            [NotNull] this IUpdateEntry entry,
            [NotNull] IEnumerable<IPropertyBase> properties)
            => "{"
                + string.Join(
                    ", ", properties.Select(
                        p =>
                        {
                            var originalValue = entry.GetOriginalValue(p);
                            return p.Name
                                + ": "
                                + (originalValue == null
                                    ? "<null>"
                                    : Convert.ToString(originalValue, CultureInfo.InvariantCulture));
                        }))
                + "}";
    }
}
