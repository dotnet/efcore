// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

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
        public static string GetName([NotNull] this IKey key) =>
            (string)key[RelationalAnnotationNames.Name]
            ?? key.GetDefaultName();

        /// <summary>
        ///     Returns the default key constraint name that would be used for this key.
        /// </summary>
        /// <param name="key"> The key. </param>
        /// <returns> The default key constraint name that would be used for this key. </returns>
        public static string GetDefaultName([NotNull] this IKey key)
        {
            var sharedTablePrincipalPrimaryKeyProperty = key.Properties[0].FindSharedTableRootPrimaryKeyProperty();
            if (sharedTablePrincipalPrimaryKeyProperty != null)
            {
                return sharedTablePrincipalPrimaryKeyProperty.FindContainingPrimaryKey().GetName();
            }

            var builder = new StringBuilder();
            var tableName = key.DeclaringEntityType.GetTableName();

            if (key.IsPrimaryKey())
            {
                builder
                    .Append("PK_")
                    .Append(tableName);
            }
            else
            {
                builder
                    .Append("AK_")
                    .Append(tableName)
                    .Append("_")
                    .AppendJoin(key.Properties.Select(p => p.GetColumnName()), "_");
            }

            return Uniquifier.Truncate(builder.ToString(), key.DeclaringEntityType.Model.GetMaxIdentifierLength());
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
        public static void SetName([NotNull] this IConventionKey key, [CanBeNull] string name, bool fromDataAnnotation = false)
            => key.SetOrRemoveAnnotation(
                RelationalAnnotationNames.Name,
                Check.NullButNotEmpty(name, nameof(name)),
                fromDataAnnotation);

        /// <summary>
        ///     Gets the <see cref="ConfigurationSource" /> for the constraint name.
        /// </summary>
        /// <param name="key"> The key. </param>
        /// <returns> The <see cref="ConfigurationSource" /> for the constraint name. </returns>
        public static ConfigurationSource? GetNameConfigurationSource([NotNull] this IConventionKey key)
            => key.FindAnnotation(RelationalAnnotationNames.Name)?.GetConfigurationSource();
    }
}
