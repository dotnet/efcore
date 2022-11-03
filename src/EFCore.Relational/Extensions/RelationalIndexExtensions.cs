// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Text;
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
        => index.GetDatabaseName(storeObject, null);

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

        var baseName = new StringBuilder()
            .Append("IX_")
            .Append(tableName)
            .Append('_')
            .AppendJoin(index.Properties.Select(p => p.GetColumnName()), "_")
            .ToString();

        return Uniquifier.Truncate(baseName, index.DeclaringEntityType.Model.GetMaxIdentifierLength());
    }

    /// <summary>
    ///     Returns the default name that would be used for this index.
    /// </summary>
    /// <param name="index">The index.</param>
    /// <param name="storeObject">The identifier of the store object.</param>
    /// <returns>The default name that would be used for this index.</returns>
    public static string? GetDefaultDatabaseName(this IReadOnlyIndex index, in StoreObjectIdentifier storeObject)
        => index.GetDefaultDatabaseName(storeObject, null);

    /// <summary>
    ///     Sets the name of the index in the database.
    /// </summary>
    /// <param name="index">The index.</param>
    /// <param name="name">The value to set.</param>
    public static void SetDatabaseName(this IMutableIndex index, string? name)
        => index.SetOrRemoveAnnotation(
            RelationalAnnotationNames.Name,
            Check.NullButNotEmpty(name, nameof(name)));

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
            Check.NullButNotEmpty(name, nameof(name)),
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
            Check.NullButNotEmpty(value, nameof(value)));

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
            Check.NullButNotEmpty(value, nameof(value)),
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
        => (IEnumerable<ITableIndex>?)index.FindRuntimeAnnotationValue(
                RelationalAnnotationNames.TableIndexMappings)
            ?? Enumerable.Empty<ITableIndex>();

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
        Check.NotNull(index, nameof(index));

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
