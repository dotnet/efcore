// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Utilities;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore
{
    /// <summary>
    ///     Extension methods for <see cref="IIndex" /> for relational database metadata.
    /// </summary>
    public static class RelationalIndexExtensions
    {
        /// <summary>
        ///     Returns the name of the index in the database.
        /// </summary>
        /// <param name="index"> The index. </param>
        /// <returns> The name of the index in the database. </returns>
        public static string GetDatabaseName([NotNull] this IIndex index)
            => (string)index[RelationalAnnotationNames.Name]
                ?? index.Name
                ?? index.GetDefaultDatabaseName();

        /// <summary>
        ///     Returns the name of the index in the database.
        /// </summary>
        /// <param name="index"> The index. </param>
        /// <returns> The name of the index in the database. </returns>
        [Obsolete("Use GetDatabaseName() instead")]
        public static string GetName([NotNull] this IIndex index)
            => GetDatabaseName(index);

        /// <summary>
        ///     Returns the name of the index in the database.
        /// </summary>
        /// <param name="index"> The index. </param>
        /// <param name="storeObject"> The identifier of the store object. </param>
        /// <returns> The name of the index in the database. </returns>
        public static string GetDatabaseName([NotNull] this IIndex index, in StoreObjectIdentifier storeObject)
            => (string)index[RelationalAnnotationNames.Name]
                ?? index.Name
                ?? index.GetDefaultDatabaseName(storeObject);

        /// <summary>
        ///     Returns the default name that would be used for this index.
        /// </summary>
        /// <param name="index"> The index. </param>
        /// <returns> The default name that would be used for this index. </returns>
        public static string GetDefaultDatabaseName([NotNull] this IIndex index)
        {
            var tableName = index.DeclaringEntityType.GetTableName();
            var schema = index.DeclaringEntityType.GetSchema();
            var baseName = new StringBuilder()
                .Append("IX_")
                .Append(tableName)
                .Append("_")
                .AppendJoin(index.Properties.Select(p => p.GetColumnBaseName()), "_")
                .ToString();

            return Uniquifier.Truncate(baseName, index.DeclaringEntityType.Model.GetMaxIdentifierLength());
        }

        /// <summary>
        ///     Returns the default name that would be used for this index.
        /// </summary>
        /// <param name="index"> The index. </param>
        /// <returns> The default name that would be used for this index. </returns>
        [Obsolete("Use GetDefaultDatabaseName() instead")]
        public static string GetDefaultName([NotNull] this IIndex index)
            => GetDefaultDatabaseName(index);

        /// <summary>
        ///     Returns the default name that would be used for this index.
        /// </summary>
        /// <param name="index"> The index. </param>
        /// <param name="storeObject"> The identifier of the store object. </param>
        /// <returns> The default name that would be used for this index. </returns>
        public static string GetDefaultDatabaseName([NotNull] this IIndex index, in StoreObjectIdentifier storeObject)
        {
            var propertyNames = index.Properties.GetColumnNames(storeObject);
            if (propertyNames == null)
            {
                return null;
            }

            var rootIndex = index;

            // Limit traversal to avoid getting stuck in a cycle (validation will throw for these later)
            // Using a hashset is detrimental to the perf when there are no cycles
            for (var i = 0; i < Metadata.Internal.RelationalEntityTypeExtensions.MaxEntityTypesSharingTable; i++)
            {
                IIndex linkedIndex = null;
                foreach (var otherIndex in rootIndex.DeclaringEntityType
                    .FindRowInternalForeignKeys(storeObject)
                    .SelectMany(fk => fk.PrincipalEntityType.GetIndexes()))
                {
                    if (otherIndex.Properties.GetColumnNames(storeObject).SequenceEqual(propertyNames))
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
                .Append("_")
                .AppendJoin(propertyNames, "_")
                .ToString();

            return Uniquifier.Truncate(baseName, index.DeclaringEntityType.Model.GetMaxIdentifierLength());
        }

        /// <summary>
        ///     Sets the name of the index in the database.
        /// </summary>
        /// <param name="index"> The index. </param>
        /// <param name="name"> The value to set. </param>
        public static void SetDatabaseName([NotNull] this IMutableIndex index, [CanBeNull] string name)
        {
            index.SetOrRemoveAnnotation(
                RelationalAnnotationNames.Name,
                Check.NullButNotEmpty(name, nameof(name)));
        }

        /// <summary>
        ///     Sets the name of the index in the database.
        /// </summary>
        /// <param name="index"> The index. </param>
        /// <param name="name"> The value to set. </param>
        [Obsolete("Use SetDatabaseName() instead.")]
        public static void SetName([NotNull] this IMutableIndex index, [CanBeNull] string name)
            => SetDatabaseName(index, name);

        /// <summary>
        ///     Sets the name of the index in the database.
        /// </summary>
        /// <param name="index"> The index. </param>
        /// <param name="name"> The value to set. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns> The configured value. </returns>
        public static string SetDatabaseName(
            [NotNull] this IConventionIndex index,
            [CanBeNull] string name,
            bool fromDataAnnotation = false)
        {
            index.SetOrRemoveAnnotation(
                RelationalAnnotationNames.Name,
                Check.NullButNotEmpty(name, nameof(name)),
                fromDataAnnotation);

            return name;
        }

        /// <summary>
        ///     Sets the name of the index in the database.
        /// </summary>
        /// <param name="index"> The index. </param>
        /// <param name="name"> The value to set. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        [Obsolete("Use SetDatabaseName() instead.")]
        public static void SetName([NotNull] this IConventionIndex index, [CanBeNull] string name, bool fromDataAnnotation = false)
            => SetDatabaseName(index, name, fromDataAnnotation);

        /// <summary>
        ///     Gets the <see cref="ConfigurationSource" /> for the name of the index in the database.
        /// </summary>
        /// <param name="index"> The index. </param>
        /// <returns> The <see cref="ConfigurationSource" /> for the name of the index in the database. </returns>
        public static ConfigurationSource? GetDatabaseNameConfigurationSource([NotNull] this IConventionIndex index)
            => index.FindAnnotation(RelationalAnnotationNames.Name)?.GetConfigurationSource();

        /// <summary>
        ///     Gets the <see cref="ConfigurationSource" /> for the name of the index in the database.
        /// </summary>
        /// <param name="index"> The index. </param>
        /// <returns> The <see cref="ConfigurationSource" /> for the name of the index in the database. </returns>
        [Obsolete("Use GetDatabaseNameConfigurationSource() instead.")]
        public static ConfigurationSource? GetNameConfigurationSource([NotNull] this IConventionIndex index)
            => GetDatabaseNameConfigurationSource(index);

        /// <summary>
        ///     Returns the index filter expression.
        /// </summary>
        /// <param name="index"> The index. </param>
        /// <returns> The index filter expression. </returns>
        public static string GetFilter([NotNull] this IIndex index)
        {
            var annotation = index.FindAnnotation(RelationalAnnotationNames.Filter);
            return annotation != null ? (string)annotation.Value : null;
        }

        /// <summary>
        ///     Returns the index filter expression.
        /// </summary>
        /// <param name="index"> The index. </param>
        /// <param name="storeObject"> The identifier of the containing store object. </param>
        /// <returns> The index filter expression. </returns>
        public static string GetFilter([NotNull] this IIndex index, in StoreObjectIdentifier storeObject)
        {
            var annotation = index.FindAnnotation(RelationalAnnotationNames.Filter);
            if (annotation != null)
            {
                return (string)annotation.Value;
            }

            var sharedTableRootIndex = index.FindSharedObjectRootIndex(storeObject);
            return sharedTableRootIndex?.GetFilter(storeObject);
        }

        /// <summary>
        ///     Sets the index filter expression.
        /// </summary>
        /// <param name="index"> The index. </param>
        /// <param name="value"> The value to set. </param>
        public static void SetFilter([NotNull] this IMutableIndex index, [CanBeNull] string value)
            => index.SetAnnotation(
                RelationalAnnotationNames.Filter,
                Check.NullButNotEmpty(value, nameof(value)));

        /// <summary>
        ///     Sets the index filter expression.
        /// </summary>
        /// <param name="index"> The index. </param>
        /// <param name="value"> The value to set. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns> The configured value. </returns>
        public static string SetFilter([NotNull] this IConventionIndex index, [CanBeNull] string value, bool fromDataAnnotation = false)
        {
            index.SetAnnotation(
                RelationalAnnotationNames.Filter,
                Check.NullButNotEmpty(value, nameof(value)),
                fromDataAnnotation);

            return value;
        }

        /// <summary>
        ///     Gets the <see cref="ConfigurationSource" /> for the index filter expression.
        /// </summary>
        /// <param name="index"> The index. </param>
        /// <returns> The <see cref="ConfigurationSource" /> for the index filter expression. </returns>
        public static ConfigurationSource? GetFilterConfigurationSource([NotNull] this IConventionIndex index)
            => index.FindAnnotation(RelationalAnnotationNames.Filter)?.GetConfigurationSource();

        /// <summary>
        ///     Gets the table indexes to which the index is mapped.
        /// </summary>
        /// <param name="index"> The index. </param>
        /// <returns> The table indexes to which the index is mapped. </returns>
        public static IEnumerable<ITableIndex> GetMappedTableIndexes([NotNull] this IIndex index)
            => (IEnumerable<ITableIndex>)index[RelationalAnnotationNames.TableIndexMappings]
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
        /// <param name="index"> The index. </param>
        /// <param name="storeObject"> The identifier of the containing store object. </param>
        /// <returns> The index found, or <see langword="null" /> if none was found.</returns>
        public static IIndex FindSharedObjectRootIndex([NotNull] this IIndex index, in StoreObjectIdentifier storeObject)
        {
            Check.NotNull(index, nameof(index));

            var indexName = index.GetDatabaseName(storeObject);
            var rootIndex = index;

            // Limit traversal to avoid getting stuck in a cycle (validation will throw for these later)
            // Using a hashset is detrimental to the perf when there are no cycles
            for (var i = 0; i < Metadata.Internal.RelationalEntityTypeExtensions.MaxEntityTypesSharingTable; i++)
            {
                IIndex linkedIndex = null;
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
        /// <param name="index"> The index. </param>
        /// <param name="storeObject"> The identifier of the containing store object. </param>
        /// <returns> The index found, or <see langword="null" /> if none was found.</returns>
        public static IMutableIndex FindSharedObjectRootIndex(
            [NotNull] this IMutableIndex index,
            in StoreObjectIdentifier storeObject)
            => (IMutableIndex)((IIndex)index).FindSharedObjectRootIndex(storeObject);

        /// <summary>
        ///     <para>
        ///         Finds the first <see cref="IConventionIndex" /> that is mapped to the same index in a shared table-like object.
        ///     </para>
        ///     <para>
        ///         This method is typically used by database providers (and other extensions). It is generally
        ///         not used in application code.
        ///     </para>
        /// </summary>
        /// <param name="index"> The index. </param>
        /// <param name="storeObject"> The identifier of the containing store object. </param>
        /// <returns> The index found, or <see langword="null" /> if none was found.</returns>
        public static IConventionIndex FindSharedObjectRootIndex(
            [NotNull] this IConventionIndex index,
            in StoreObjectIdentifier storeObject)
            => (IConventionIndex)((IIndex)index).FindSharedObjectRootIndex(storeObject);
    }
}
