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
    ///     Extension methods for <see cref="IIndex" /> for relational database metadata.
    /// </summary>
    public static class RelationalIndexExtensions
    {
        /// <summary>
        ///     Returns the name for this index.
        /// </summary>
        /// <param name="index"> The index. </param>
        /// <returns> The name for this index. </returns>
        public static string GetName([NotNull] this IIndex index) =>
            (string)index[RelationalAnnotationNames.Name]
            ?? index.GetDefaultName();

        /// <summary>
        ///     Returns the default name that would be used for this index.
        /// </summary>
        /// <param name="index"> The index. </param>
        /// <returns> The default name that would be used for this index. </returns>
        public static string GetDefaultName([NotNull] this IIndex index)
        {
            var baseName = new StringBuilder()
                .Append("IX_")
                .Append(index.DeclaringEntityType.GetTableName())
                .Append("_")
                .AppendJoin(index.Properties.Select(p => p.GetColumnName()), "_")
                .ToString();

            return Uniquifier.Truncate(baseName, index.DeclaringEntityType.Model.GetMaxIdentifierLength());
        }

        /// <summary>
        ///     Sets the index name.
        /// </summary>
        /// <param name="index"> The index. </param>
        /// <param name="name"> The value to set. </param>
        public static void SetName([NotNull] this IMutableIndex index, [CanBeNull] string name)
            => index.SetOrRemoveAnnotation(
                RelationalAnnotationNames.Name,
                Check.NullButNotEmpty(name, nameof(name)));

        /// <summary>
        ///     Sets the index name.
        /// </summary>
        /// <param name="index"> The index. </param>
        /// <param name="name"> The value to set. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        public static void SetName([NotNull] this IConventionIndex index, [CanBeNull] string name, bool fromDataAnnotation = false)
            => index.SetOrRemoveAnnotation(
                RelationalAnnotationNames.Name,
                Check.NullButNotEmpty(name, nameof(name)),
                fromDataAnnotation);

        /// <summary>
        ///     Gets the <see cref="ConfigurationSource" /> for the index name.
        /// </summary>
        /// <param name="index"> The index. </param>
        /// <returns> The <see cref="ConfigurationSource" /> for the index name. </returns>
        public static ConfigurationSource? GetNameConfigurationSource([NotNull] this IConventionIndex index)
            => index.FindAnnotation(RelationalAnnotationNames.Name)?.GetConfigurationSource();

        /// <summary>
        ///     Returns the index filter expression.
        /// </summary>
        /// <param name="index"> The index. </param>
        /// <returns> The index filter expression. </returns>
        public static string GetFilter([NotNull] this IIndex index)
            => (string)index[RelationalAnnotationNames.Filter];

        /// <summary>
        ///     Sets the index filter expression.
        /// </summary>
        /// <param name="index"> The index. </param>
        /// <param name="value"> The value to set. </param>
        public static void SetFilter([NotNull] this IMutableIndex index, [CanBeNull] string value)
            => index.SetOrRemoveAnnotation(
                RelationalAnnotationNames.Filter,
                Check.NullButNotEmpty(value, nameof(value)));

        /// <summary>
        ///     Sets the index filter expression.
        /// </summary>
        /// <param name="index"> The index. </param>
        /// <param name="value"> The value to set. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        public static void SetFilter([NotNull] this IConventionIndex index, [CanBeNull] string value, bool fromDataAnnotation = false)
            => index.SetOrRemoveAnnotation(
                RelationalAnnotationNames.Filter,
                Check.NullButNotEmpty(value, nameof(value)),
                fromDataAnnotation);

        /// <summary>
        ///     Gets the <see cref="ConfigurationSource" /> for the index filter expression.
        /// </summary>
        /// <param name="index"> The index. </param>
        /// <returns> The <see cref="ConfigurationSource" /> for the index filter expression. </returns>
        public static ConfigurationSource? GetFilterConfigurationSource([NotNull] this IConventionIndex index)
            => index.FindAnnotation(RelationalAnnotationNames.Filter)?.GetConfigurationSource();
    }
}
