// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Text;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore;

/// <summary>
///     Index extension methods for relational database metadata.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-indexes">Indexes</see> for more information and examples.
/// </remarks>
public static class RelationalIndexExtensions
{
    /// <summary>
    ///     Returns the name of the index in the database.
    /// </summary>
    /// <param name="index">The index.</param>
    /// <returns>The name of the index in the database.</returns>
    public static string? GetDatabaseName(this IReadOnlyIndex index)
        => index.DeclaringEntityType.GetTableName() == null
            ? null
            : (string?)index[RelationalAnnotationNames.Name]
            ?? index.Name
            ?? index.GetDefaultDatabaseName();

    /// <summary>
    ///     Returns the name of the index in the database.
    /// </summary>
    /// <param name="index">The index.</param>
    /// <param name="storeObject">The identifier of the store object.</param>
    /// <returns>The name of the index in the database.</returns>
    public static string? GetDatabaseName(this IReadOnlyIndex index, in StoreObjectIdentifier storeObject)
    {
        if (storeObject.StoreObjectType != StoreObjectType.Table)
        {
            return null;
        }

        var defaultName = index.GetDefaultDatabaseName(storeObject);
        var annotation = index.FindAnnotation(RelationalAnnotationNames.Name);
        return annotation != null && defaultName != null
            ? (string?)annotation.Value
            : defaultName != null
                ? index.Name ?? defaultName
                : defaultName;
    }

    /// <summary>
    ///     Returns the default name that would be used for this index.
    /// </summary>
    /// <param name="index">The index.</param>
    /// <returns>The default name that would be used for this index.</returns>
    public static string? GetDefaultDatabaseName(this IReadOnlyIndex index)
    {
        var tableName = index.DeclaringEntityType.GetTableName();
        if (tableName == null)
        {
            return null;
        }

        var columnNames = index.GetColumnNames();
        if (columnNames == null)
        {
            return null;
        }

        var baseName = new StringBuilder()
            .Append("IX_")
            .Append(tableName)
            .Append('_');
        AppendProperties(index, columnNames, baseName);

        return Uniquifier.Truncate(baseName.ToString(), index.DeclaringEntityType.Model.GetMaxIdentifierLength());
    }

    /// <summary>
    ///     Returns the default name that would be used for this index.
    /// </summary>
    /// <param name="index">The index.</param>
    /// <param name="storeObject">The identifier of the store object.</param>
    /// <returns>The default name that would be used for this index.</returns>
    public static string? GetDefaultDatabaseName(this IReadOnlyIndex index, in StoreObjectIdentifier storeObject)
    {
        if (storeObject.StoreObjectType != StoreObjectType.Table)
        {
            return null;
        }

        var columnNames = index.GetColumnNames(storeObject);
        if (columnNames == null)
        {
            return null;
        }

        var rootIndex = index;

        // Limit traversal to avoid getting stuck in a cycle (validation will throw for these later)
        // Using a hashset is detrimental to the perf when there are no cycles
        for (var i = 0; i < Metadata.Internal.RelationalEntityTypeExtensions.MaxEntityTypesSharingTable; i++)
        {
            IReadOnlyIndex? linkedIndex = null;
            foreach (var otherIndex in rootIndex.DeclaringEntityType
                         .FindRowInternalForeignKeys(storeObject)
                         .SelectMany(fk => fk.PrincipalEntityType.GetIndexes()))
            {
                var otherColumnNames = otherIndex.GetColumnNames(storeObject);
                if ((otherColumnNames != null)
                    && otherColumnNames.SequenceEqual(columnNames))
                {
                    linkedIndex = otherIndex;
                    break;
                }
            }

            if (linkedIndex == null)
            {
                break;
            }

            rootIndex = linkedIndex;
        }

        if (rootIndex != index)
        {
            return rootIndex.GetDatabaseName(storeObject);
        }

        var baseName = new StringBuilder()
            .Append("IX_")
            .Append(storeObject.Name)
            .Append('_');
        AppendProperties(index, columnNames, baseName);

        return Uniquifier.Truncate(baseName.ToString(), index.DeclaringEntityType.Model.GetMaxIdentifierLength());
    }

    private static void AppendProperties(IReadOnlyIndex index, IReadOnlyList<string> columnNames, StringBuilder builder)
    {
        // For an index on properties contained inside a JSON-mapped complex type, the index covers
        // a single JSON container column, so naming purely by column would produce ambiguous default
        // names when multiple JSON-path indexes share a column. Use the property path through the
        // complex-type chain (e.g. "Items_Value") instead so each path gets a distinct default name.
        var startLength = builder.Length;
        var first = true;
        foreach (var property in index.Properties)
        {
            IReadOnlyTypeBase current;
            string leafName;
            switch (property)
            {
                case IReadOnlyProperty scalar
                    when scalar.DeclaringType is IReadOnlyComplexType complexType && complexType.IsMappedToJson():
                    leafName = scalar.Name;
                    current = scalar.DeclaringType;
                    break;

                case IReadOnlyComplexProperty { ComplexType: var ct } complexProperty when ct.IsMappedToJson():
                    leafName = complexProperty.Name;
                    current = complexProperty.DeclaringType;
                    break;

                default:
                    // If any property in the index isn't inside a JSON-mapped complex type, fall back to the
                    // column names provided by the caller.
                    builder.Length = startLength;
                    builder.AppendJoin(columnNames, "_");
                    return;
            }

            if (!first)
            {
                builder.Append('_');
            }

            first = false;

            var pathStart = builder.Length;
            builder.Append(leafName);
            while (current is IReadOnlyComplexType parent)
            {
                builder.Insert(pathStart, '_');
                builder.Insert(pathStart, parent.ComplexProperty.Name);
                current = parent.ComplexProperty.DeclaringType;
            }
        }
    }

    /// <summary>
    ///     Sets the name of the index in the database.
    /// </summary>
    /// <param name="index">The index.</param>
    /// <param name="name">The value to set.</param>
    public static void SetDatabaseName(this IMutableIndex index, string? name)
        => index.SetOrRemoveAnnotation(
            RelationalAnnotationNames.Name,
            Check.NullButNotEmpty(name));

    /// <summary>
    ///     Sets the name of the index in the database.
    /// </summary>
    /// <param name="index">The index.</param>
    /// <param name="name">The value to set.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>The configured value.</returns>
    public static string? SetDatabaseName(
        this IConventionIndex index,
        string? name,
        bool fromDataAnnotation = false)
        => (string?)index.SetOrRemoveAnnotation(
            RelationalAnnotationNames.Name,
            Check.NullButNotEmpty(name),
            fromDataAnnotation)?.Value;

    /// <summary>
    ///     Gets the <see cref="ConfigurationSource" /> for the name of the index in the database.
    /// </summary>
    /// <param name="index">The index.</param>
    /// <returns>The <see cref="ConfigurationSource" /> for the name of the index in the database.</returns>
    public static ConfigurationSource? GetDatabaseNameConfigurationSource(this IConventionIndex index)
        => index.FindAnnotation(RelationalAnnotationNames.Name)?.GetConfigurationSource();

    /// <summary>
    ///     Returns the index filter expression.
    /// </summary>
    /// <param name="index">The index.</param>
    /// <returns>The index filter expression.</returns>
    public static string? GetFilter(this IReadOnlyIndex index)
        => (string?)index.FindAnnotation(RelationalAnnotationNames.Filter)?.Value;

    /// <summary>
    ///     Returns the index filter expression.
    /// </summary>
    /// <param name="index">The index.</param>
    /// <param name="storeObject">The identifier of the containing store object.</param>
    /// <returns>The index filter expression.</returns>
    public static string? GetFilter(this IReadOnlyIndex index, in StoreObjectIdentifier storeObject)
    {
        var annotation = index.FindAnnotation(RelationalAnnotationNames.Filter);
        if (annotation != null)
        {
            return (string?)annotation.Value;
        }

        var sharedTableRootIndex = index.FindSharedObjectRootIndex(storeObject);
        return sharedTableRootIndex?.GetFilter(storeObject);
    }

    /// <summary>
    ///     Sets the index filter expression.
    /// </summary>
    /// <param name="index">The index.</param>
    /// <param name="value">The value to set.</param>
    public static void SetFilter(this IMutableIndex index, string? value)
        => index.SetAnnotation(
            RelationalAnnotationNames.Filter,
            Check.NullButNotEmpty(value));

    /// <summary>
    ///     Sets the index filter expression.
    /// </summary>
    /// <param name="index">The index.</param>
    /// <param name="value">The value to set.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>The configured value.</returns>
    public static string? SetFilter(this IConventionIndex index, string? value, bool fromDataAnnotation = false)
        => (string?)index.SetAnnotation(
            RelationalAnnotationNames.Filter,
            Check.NullButNotEmpty(value),
            fromDataAnnotation)?.Value;

    /// <summary>
    ///     Gets the <see cref="ConfigurationSource" /> for the index filter expression.
    /// </summary>
    /// <param name="index">The index.</param>
    /// <returns>The <see cref="ConfigurationSource" /> for the index filter expression.</returns>
    public static ConfigurationSource? GetFilterConfigurationSource(this IConventionIndex index)
        => index.FindAnnotation(RelationalAnnotationNames.Filter)?.GetConfigurationSource();

    /// <summary>
    ///     Gets the table indexes to which the index is mapped.
    /// </summary>
    /// <param name="index">The index.</param>
    /// <returns>The table indexes to which the index is mapped.</returns>
    public static IEnumerable<ITableIndex> GetMappedTableIndexes(this IIndex index)
    {
        index.DeclaringEntityType.Model.EnsureRelationalModel();
        return (IEnumerable<ITableIndex>?)index.FindRuntimeAnnotationValue(
                RelationalAnnotationNames.TableIndexMappings)
            ?? [];
    }

    /// <summary>
    ///     <para>
    ///         Finds the first <see cref="IIndex" /> that is mapped to the same index in a shared table-like object.
    ///     </para>
    ///     <para>
    ///         This method is typically used by database providers (and other extensions). It is generally
    ///         not used in application code.
    ///     </para>
    /// </summary>
    /// <param name="index">The index.</param>
    /// <param name="storeObject">The identifier of the containing store object.</param>
    /// <returns>The index found, or <see langword="null" /> if none was found.</returns>
    public static IReadOnlyIndex? FindSharedObjectRootIndex(this IReadOnlyIndex index, in StoreObjectIdentifier storeObject)
    {
        Check.NotNull(index);

        var indexName = index.GetDatabaseName(storeObject);
        var rootIndex = index;

        // Limit traversal to avoid getting stuck in a cycle (validation will throw for these later)
        // Using a hashset is detrimental to the perf when there are no cycles
        for (var i = 0; i < Metadata.Internal.RelationalEntityTypeExtensions.MaxEntityTypesSharingTable; i++)
        {
            IReadOnlyIndex? linkedIndex = null;
            foreach (var otherIndex in rootIndex.DeclaringEntityType
                         .FindRowInternalForeignKeys(storeObject)
                         .SelectMany(fk => fk.PrincipalEntityType.GetIndexes()))
            {
                if (otherIndex.GetDatabaseName(storeObject) == indexName)
                {
                    linkedIndex = otherIndex;
                    break;
                }
            }

            if (linkedIndex == null)
            {
                break;
            }

            rootIndex = linkedIndex;
        }

        return rootIndex == index ? null : rootIndex;
    }

    /// <summary>
    ///     <para>
    ///         Finds the first <see cref="IMutableIndex" /> that is mapped to the same index in a shared table-like object.
    ///     </para>
    ///     <para>
    ///         This method is typically used by database providers (and other extensions). It is generally
    ///         not used in application code.
    ///     </para>
    /// </summary>
    /// <param name="index">The index.</param>
    /// <param name="storeObject">The identifier of the containing store object.</param>
    /// <returns>The index found, or <see langword="null" /> if none was found.</returns>
    public static IMutableIndex? FindSharedObjectRootIndex(
        this IMutableIndex index,
        in StoreObjectIdentifier storeObject)
        => (IMutableIndex?)((IReadOnlyIndex)index).FindSharedObjectRootIndex(storeObject);

    /// <summary>
    ///     <para>
    ///         Finds the first <see cref="IConventionIndex" /> that is mapped to the same index in a shared table-like object.
    ///     </para>
    ///     <para>
    ///         This method is typically used by database providers (and other extensions). It is generally
    ///         not used in application code.
    ///     </para>
    /// </summary>
    /// <param name="index">The index.</param>
    /// <param name="storeObject">The identifier of the containing store object.</param>
    /// <returns>The index found, or <see langword="null" /> if none was found.</returns>
    public static IConventionIndex? FindSharedObjectRootIndex(
        this IConventionIndex index,
        in StoreObjectIdentifier storeObject)
        => (IConventionIndex?)((IReadOnlyIndex)index).FindSharedObjectRootIndex(storeObject);

    /// <summary>
    ///     <para>
    ///         Finds the first <see cref="IConventionIndex" /> that is mapped to the same index in a shared table-like object.
    ///     </para>
    ///     <para>
    ///         This method is typically used by database providers (and other extensions). It is generally
    ///         not used in application code.
    ///     </para>
    /// </summary>
    /// <param name="index">The index.</param>
    /// <param name="storeObject">The identifier of the containing store object.</param>
    /// <returns>The index found, or <see langword="null" /> if none was found.</returns>
    public static IIndex? FindSharedObjectRootIndex(
        this IIndex index,
        in StoreObjectIdentifier storeObject)
        => (IIndex?)((IReadOnlyIndex)index).FindSharedObjectRootIndex(storeObject);
}
