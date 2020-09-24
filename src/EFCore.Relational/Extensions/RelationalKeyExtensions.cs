// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

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
    ///     Extension methods for <see cref="IKey" /> for relational database metadata.
    /// </summary>
    public static class RelationalKeyExtensions
    {
        /// <summary>
        ///     Returns the key constraint name for this key.
        /// </summary>
        /// <param name="key"> The key. </param>
        /// <returns> The key constraint name for this key. </returns>
        public static string GetName([NotNull] this IKey key)
            => key.GetName(StoreObjectIdentifier.Table(key.DeclaringEntityType.GetTableName(), key.DeclaringEntityType.GetSchema()));

        /// <summary>
        ///     Returns the key constraint name for this key for a particular table.
        /// </summary>
        /// <param name="key"> The key. </param>
        /// <param name="storeObject"> The identifier of the containing store object. </param>
        /// <returns> The key constraint name for this key. </returns>
        public static string GetName([NotNull] this IKey key, in StoreObjectIdentifier storeObject)
            => (string)key[RelationalAnnotationNames.Name]
                ?? key.GetDefaultName(storeObject);

        /// <summary>
        ///     Returns the default key constraint name that would be used for this key.
        /// </summary>
        /// <param name="key"> The key. </param>
        /// <returns> The default key constraint name that would be used for this key. </returns>
        public static string GetDefaultName([NotNull] this IKey key)
        {
            string name = null;
            var tableName = key.DeclaringEntityType.GetTableName();
            if (key.IsPrimaryKey())
            {
                name = "PK_" + tableName;
            }
            else
            {
                name = new StringBuilder()
                    .Append("AK_")
                    .Append(tableName)
                    .Append("_")
                    .AppendJoin(key.Properties.Select(p => p.GetColumnBaseName()), "_")
                    .ToString();
            }

            return Uniquifier.Truncate(name, key.DeclaringEntityType.Model.GetMaxIdentifierLength());
        }

        /// <summary>
        ///     Returns the default key constraint name that would be used for this key for a particular table.
        /// </summary>
        /// <param name="key"> The key. </param>
        /// <param name="storeObject"> The identifier of the containing store object. </param>
        /// <returns> The default key constraint name that would be used for this key. </returns>
        public static string GetDefaultName([NotNull] this IKey key, in StoreObjectIdentifier storeObject)
        {
            string name = null;
            if (key.IsPrimaryKey())
            {
                var rootKey = key;
                // Limit traversal to avoid getting stuck in a cycle (validation will throw for these later)
                // Using a hashset is detrimental to the perf when there are no cycles
                for (var i = 0; i < Metadata.Internal.RelationalEntityTypeExtensions.MaxEntityTypesSharingTable; i++)
                {
                    var linkingFk = rootKey.DeclaringEntityType.FindRowInternalForeignKeys(storeObject)
                        .FirstOrDefault();
                    if (linkingFk == null)
                    {
                        break;
                    }

                    rootKey = linkingFk.PrincipalEntityType.FindPrimaryKey();
                }

                if (rootKey != null
                    && rootKey != key)
                {
                    return rootKey.GetName(storeObject);
                }

                name = "PK_" + storeObject.Name;
            }
            else
            {
                var propertyNames = key.Properties.GetColumnNames(storeObject);
                if (propertyNames == null)
                {
                    return null;
                }

                var rootKey = key;

                // Limit traversal to avoid getting stuck in a cycle (validation will throw for these later)
                // Using a hashset is detrimental to the perf when there are no cycles
                for (var i = 0; i < Metadata.Internal.RelationalEntityTypeExtensions.MaxEntityTypesSharingTable; i++)
                {
                    IKey linkedKey = null;
                    foreach (var otherKey in rootKey.DeclaringEntityType
                        .FindRowInternalForeignKeys(storeObject)
                        .SelectMany(fk => fk.PrincipalEntityType.GetKeys()))
                    {
                        if (otherKey.Properties.GetColumnNames(storeObject).SequenceEqual(propertyNames))
                        {
                            linkedKey = otherKey;
                            break;
                        }
                    }

                    if (linkedKey == null)
                    {
                        break;
                    }

                    rootKey = linkedKey;
                }

                if (rootKey != key)
                {
                    return rootKey.GetName(storeObject);
                }

                name = new StringBuilder()
                    .Append("AK_")
                    .Append(storeObject.Name)
                    .Append("_")
                    .AppendJoin(propertyNames, "_")
                    .ToString();
            }

            return Uniquifier.Truncate(name, key.DeclaringEntityType.Model.GetMaxIdentifierLength());
        }

        /// <summary>
        ///     Sets the key constraint name for this key.
        /// </summary>
        /// <param name="key"> The key. </param>
        /// <param name="name"> The value to set. </param>
        public static void SetName([NotNull] this IMutableKey key, [CanBeNull] string name)
            => key.SetOrRemoveAnnotation(
                RelationalAnnotationNames.Name,
                Check.NullButNotEmpty(name, nameof(name)));

        /// <summary>
        ///     Sets the key constraint name for this key.
        /// </summary>
        /// <param name="key"> The key. </param>
        /// <param name="name"> The value to set. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns> The configured name. </returns>
        public static string SetName([NotNull] this IConventionKey key, [CanBeNull] string name, bool fromDataAnnotation = false)
        {
            key.SetOrRemoveAnnotation(
                RelationalAnnotationNames.Name,
                Check.NullButNotEmpty(name, nameof(name)),
                fromDataAnnotation);

            return name;
        }

        /// <summary>
        ///     Gets the <see cref="ConfigurationSource" /> for the constraint name.
        /// </summary>
        /// <param name="key"> The key. </param>
        /// <returns> The <see cref="ConfigurationSource" /> for the constraint name. </returns>
        public static ConfigurationSource? GetNameConfigurationSource([NotNull] this IConventionKey key)
            => key.FindAnnotation(RelationalAnnotationNames.Name)?.GetConfigurationSource();

        /// <summary>
        ///     Gets the unique constraints to which the key is mapped.
        /// </summary>
        /// <param name="key"> The key. </param>
        /// <returns> The unique constraints to which the key is mapped. </returns>
        public static IEnumerable<IUniqueConstraint> GetMappedConstraints([NotNull] this IKey key)
            => (IEnumerable<IUniqueConstraint>)key[RelationalAnnotationNames.UniqueConstraintMappings]
                ?? Enumerable.Empty<IUniqueConstraint>();

        /// <summary>
        ///     <para>
        ///         Finds the first <see cref="IKey" /> that is mapped to the same constraint in a shared table-like object.
        ///     </para>
        ///     <para>
        ///         This method is typically used by database providers (and other extensions). It is generally
        ///         not used in application code.
        ///     </para>
        /// </summary>
        /// <param name="key"> The key. </param>
        /// <param name="storeObject"> The identifier of the containing store object. </param>
        /// <returns> The key found, or <see langword="null" /> if none was found.</returns>
        public static IKey FindSharedObjectRootKey([NotNull] this IKey key, in StoreObjectIdentifier storeObject)
        {
            Check.NotNull(key, nameof(key));

            var keyName = key.GetName(storeObject);
            var rootKey = key;

            // Limit traversal to avoid getting stuck in a cycle (validation will throw for these later)
            // Using a hashset is detrimental to the perf when there are no cycles
            for (var i = 0; i < Metadata.Internal.RelationalEntityTypeExtensions.MaxEntityTypesSharingTable; i++)
            {
                IKey linkedKey = null;
                foreach (var otherKey in rootKey.DeclaringEntityType
                    .FindRowInternalForeignKeys(storeObject)
                    .SelectMany(fk => fk.PrincipalEntityType.GetKeys()))
                {
                    if (otherKey.GetName(storeObject) == keyName)
                    {
                        linkedKey = otherKey;
                        break;
                    }
                }

                if (linkedKey == null)
                {
                    break;
                }

                rootKey = linkedKey;
            }

            return rootKey == key ? null : rootKey;
        }

        /// <summary>
        ///     <para>
        ///         Finds the first <see cref="IMutableKey" /> that is mapped to the same constraint in a shared table-like object.
        ///     </para>
        ///     <para>
        ///         This method is typically used by database providers (and other extensions). It is generally
        ///         not used in application code.
        ///     </para>
        /// </summary>
        /// <param name="key"> The key. </param>
        /// <param name="storeObject"> The identifier of the containing store object. </param>
        /// <returns> The key found, or <see langword="null" /> if none was found.</returns>
        public static IMutableKey FindSharedObjectRootKey(
            [NotNull] this IMutableKey key,
            in StoreObjectIdentifier storeObject)
            => (IMutableKey)((IKey)key).FindSharedObjectRootKey(storeObject);

        /// <summary>
        ///     <para>
        ///         Finds the first <see cref="IConventionKey" /> that is mapped to the same constraint in a shared table-like object.
        ///     </para>
        ///     <para>
        ///         This method is typically used by database providers (and other extensions). It is generally
        ///         not used in application code.
        ///     </para>
        /// </summary>
        /// <param name="key"> The key. </param>
        /// <param name="storeObject"> The identifier of the containing store object. </param>
        /// <returns> The key found, or <see langword="null" /> if none was found.</returns>
        public static IConventionKey FindSharedObjectRootKey(
            [NotNull] this IConventionKey key,
            in StoreObjectIdentifier storeObject)
            => (IConventionKey)((IKey)key).FindSharedObjectRootKey(storeObject);
    }
}
