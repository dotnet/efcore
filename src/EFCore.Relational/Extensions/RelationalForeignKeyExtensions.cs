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
    ///     Extension methods for <see cref="IForeignKey" /> for relational database metadata.
    /// </summary>
    public static class RelationalForeignKeyExtensions
    {
        /// <summary>
        ///     Returns the foreign key constraint name.
        /// </summary>
        /// <param name="foreignKey"> The foreign key. </param>
        /// <returns> The foreign key constraint name. </returns>
        public static string GetConstraintName([NotNull] this IForeignKey foreignKey) =>
            (string)foreignKey[RelationalAnnotationNames.Name]
            ?? foreignKey.GetDefaultName();

        /// <summary>
        ///     Returns the default constraint name that would be used for this foreign key.
        /// </summary>
        /// <param name="foreignKey"> The foreign key. </param>
        /// <returns> The default constraint name that would be used for this foreign key. </returns>
        public static string GetDefaultName([NotNull] this IForeignKey foreignKey)
        {
            var baseName = new StringBuilder()
                .Append("FK_")
                .Append(foreignKey.DeclaringEntityType.GetTableName())
                .Append("_")
                .Append(foreignKey.PrincipalEntityType.GetTableName())
                .Append("_")
                .AppendJoin(foreignKey.Properties.Select(p => p.GetColumnName()), "_")
                .ToString();

            return Uniquifier.Truncate(baseName, foreignKey.DeclaringEntityType.Model.GetMaxIdentifierLength());
        }

        /// <summary>
        ///     Sets the foreign key constraint name.
        /// </summary>
        /// <param name="foreignKey"> The foreign key. </param>
        /// <param name="value"> The value to set. </param>
        public static void SetConstraintName([NotNull] this IMutableForeignKey foreignKey, [CanBeNull] string value)
            => foreignKey.SetOrRemoveAnnotation(
                RelationalAnnotationNames.Name,
                Check.NullButNotEmpty(value, nameof(value)));

        /// <summary>
        ///     Sets the foreign key constraint name.
        /// </summary>
        /// <param name="foreignKey"> The foreign key. </param>
        /// <param name="value"> The value to set. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        public static void SetConstraintName(
            [NotNull] this IConventionForeignKey foreignKey, [CanBeNull] string value, bool fromDataAnnotation = false)
            => foreignKey.SetOrRemoveAnnotation(
                RelationalAnnotationNames.Name,
                Check.NullButNotEmpty(value, nameof(value)),
                fromDataAnnotation);

        /// <summary>
        ///     Gets the <see cref="ConfigurationSource" /> for the constraint name.
        /// </summary>
        /// <param name="foreignKey"> The foreign key. </param>
        /// <returns> The <see cref="ConfigurationSource" /> for the constraint name. </returns>
        public static ConfigurationSource? GetConstraintNameConfigurationSource([NotNull] this IConventionForeignKey foreignKey)
            => foreignKey.FindAnnotation(RelationalAnnotationNames.Name)
                ?.GetConfigurationSource();
    }
}
