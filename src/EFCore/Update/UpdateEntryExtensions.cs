// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections;
using System.Globalization;
using System.Text;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.Update;

/// <summary>
///     Extension methods for <see cref="IUpdateEntry" />.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-providers">Implementation of database providers and extensions</see>
///     for more information and examples.
/// </remarks>
public static class UpdateEntryExtensions
{
    /// <summary>
    ///     Gets the value assigned to the property and converts it to the provider-expected value.
    /// </summary>
    /// <param name="updateEntry">The entry.</param>
    /// <param name="property">The property to get the value for.</param>
    /// <returns>The value for the property.</returns>
    public static object? GetCurrentProviderValue(this IUpdateEntry updateEntry, IProperty property)
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
    ///     Gets the original value that was assigned to the property and converts it to the provider-expected value.
    /// </summary>
    /// <param name="updateEntry">The entry.</param>
    /// <param name="property">The property to get the value for.</param>
    /// <returns>The value for the property.</returns>
    public static object? GetOriginalProviderValue(this IUpdateEntry updateEntry, IProperty property)
    {
        var value = updateEntry.GetOriginalOrCurrentValue(property);
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
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-debug-views">EF Core debug views</see> for more information and examples.
    /// </remarks>
    /// <param name="updateEntry">The entry.</param>
    /// <param name="options">Options for generating the string.</param>
    /// <param name="indent">The number of indent spaces to use before each new line.</param>
    /// <returns>A human-readable representation.</returns>
    public static string ToDebugString(
        this IUpdateEntry updateEntry,
        ChangeTrackerDebugStringOptions options = ChangeTrackerDebugStringOptions.LongDefault,
        int indent = 0)
    {
        var builder = new StringBuilder();
        var indentString = new string(' ', indent);

        try
        {
            var entry = (InternalEntityEntry)updateEntry;

            var keyString = entry.BuildCurrentValuesString(entry.EntityType.FindPrimaryKey()!.Properties);

            builder
                .Append(entry.EntityType.DisplayName())
                .Append(' ')
                .Append(entry.SharedIdentityEntry != null ? "(Shared) " : "")
                .Append(keyString)
                .Append(' ')
                .Append(entry.EntityState.ToString());

            if ((options & ChangeTrackerDebugStringOptions.IncludeProperties) != 0)
            {
                DumpProperties(entry.EntityType, indent + 2);

                void DumpProperties(ITypeBase structuralType, int tempIndent)
                {
                    var tempIndentString = new string(' ', tempIndent);
                    foreach (var property in structuralType.GetProperties())
                    {
                        builder.AppendLine().Append(tempIndentString);

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

                        if (entry.IsUnknown(property))
                        {
                            builder.Append(" Unknown");
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

                    foreach (var complexProperty in structuralType.GetComplexProperties())
                    {
                        builder.AppendLine().Append(tempIndentString);

                        builder
                            .Append("  ")
                            .Append(complexProperty.Name)
                            .Append(" (Complex: ")
                            .Append(complexProperty.ClrType.ShortDisplayName())
                            .Append(")");

                        DumpProperties(complexProperty.ComplexType, tempIndent + 2);
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

            void AppendValue(object? value)
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
                    if (stringValue?.Length > 63)
                    {
                        stringValue = string.Concat(stringValue.AsSpan(0, 60), "...");
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
                        : otherEntry.BuildCurrentValuesString(targetType.FindPrimaryKey()!.Properties));
            }
        }
        catch (Exception exception)
        {
            builder.AppendLine().AppendLine(CoreStrings.DebugViewError(exception.Message));
        }

        return builder.ToString();
    }

    /// <summary>
    ///     Creates a formatted string representation of the given properties and their current
    ///     values such as is useful when throwing exceptions about keys, indexes, etc. that use
    ///     the properties.
    /// </summary>
    /// <param name="entry">The entry from which values will be obtained.</param>
    /// <param name="properties">The properties to format.</param>
    /// <returns>The string representation.</returns>
    public static string BuildCurrentValuesString(
        this IUpdateEntry entry,
        IEnumerable<IPropertyBase> properties)
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
    /// <param name="entry">The entry from which values will be obtained.</param>
    /// <param name="properties">The properties to format.</param>
    /// <returns>The string representation.</returns>
    public static string BuildOriginalValuesString(
        this IUpdateEntry entry,
        IEnumerable<IPropertyBase> properties)
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
