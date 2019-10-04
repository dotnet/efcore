// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Internal;

namespace Microsoft.EntityFrameworkCore.Metadata.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public static class ForeignKeyExtensions
    {
        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static bool AreCompatible([NotNull] this IForeignKey foreignKey, [NotNull] IForeignKey duplicateForeignKey, bool shouldThrow)
        {
            var principalAnnotations = foreignKey.PrincipalEntityType.Relational();
            var duplicatePrincipalAnnotations = duplicateForeignKey.PrincipalEntityType.Relational();
            if (!string.Equals(principalAnnotations.Schema, duplicatePrincipalAnnotations.Schema, StringComparison.OrdinalIgnoreCase)
                || !string.Equals(principalAnnotations.TableName, duplicatePrincipalAnnotations.TableName, StringComparison.OrdinalIgnoreCase))
            {
                if (shouldThrow)
                {
                    throw new InvalidOperationException(
                        RelationalStrings.DuplicateForeignKeyPrincipalTableMismatch(
                            Property.Format(foreignKey.Properties),
                            foreignKey.DeclaringEntityType.DisplayName(),
                            Property.Format(duplicateForeignKey.Properties),
                            duplicateForeignKey.DeclaringEntityType.DisplayName(),
                            Format(foreignKey.DeclaringEntityType.Relational()),
                            foreignKey.Relational().Name,
                            Format(principalAnnotations),
                            Format(duplicatePrincipalAnnotations)));
                }

                return false;
            }

            if (!foreignKey.Properties.Select(p => p.Relational().ColumnName)
                .SequenceEqual(duplicateForeignKey.Properties.Select(p => p.Relational().ColumnName)))
            {
                if (shouldThrow)
                {
                    throw new InvalidOperationException(
                        RelationalStrings.DuplicateForeignKeyColumnMismatch(
                            Property.Format(foreignKey.Properties),
                            foreignKey.DeclaringEntityType.DisplayName(),
                            Property.Format(duplicateForeignKey.Properties),
                            duplicateForeignKey.DeclaringEntityType.DisplayName(),
                            Format(foreignKey.DeclaringEntityType.Relational()),
                            foreignKey.Relational().Name,
                            foreignKey.Properties.FormatColumns(),
                            duplicateForeignKey.Properties.FormatColumns()));
                }

                return false;
            }

            if (!foreignKey.PrincipalKey.Properties
                .Select(p => p.Relational().ColumnName)
                .SequenceEqual(
                    duplicateForeignKey.PrincipalKey.Properties
                        .Select(p => p.Relational().ColumnName)))
            {
                if (shouldThrow)
                {
                    throw new InvalidOperationException(
                        RelationalStrings.DuplicateForeignKeyPrincipalColumnMismatch(
                            Property.Format(foreignKey.Properties),
                            foreignKey.DeclaringEntityType.DisplayName(),
                            Property.Format(duplicateForeignKey.Properties),
                            duplicateForeignKey.DeclaringEntityType.DisplayName(),
                            Format(foreignKey.DeclaringEntityType.Relational()),
                            foreignKey.Relational().Name,
                            foreignKey.PrincipalKey.Properties.FormatColumns(),
                            duplicateForeignKey.PrincipalKey.Properties.FormatColumns()));
                }

                return false;
            }

            if (foreignKey.IsUnique != duplicateForeignKey.IsUnique)
            {
                if (shouldThrow)
                {
                    throw new InvalidOperationException(
                        RelationalStrings.DuplicateForeignKeyUniquenessMismatch(
                            Property.Format(foreignKey.Properties),
                            foreignKey.DeclaringEntityType.DisplayName(),
                            Property.Format(duplicateForeignKey.Properties),
                            duplicateForeignKey.DeclaringEntityType.DisplayName(),
                            Format(foreignKey.DeclaringEntityType.Relational()),
                            foreignKey.Relational().Name));
                }

                return false;
            }

            if (foreignKey.DeleteBehavior != duplicateForeignKey.DeleteBehavior)
            {
                if (shouldThrow)
                {
                    throw new InvalidOperationException(
                        RelationalStrings.DuplicateForeignKeyDeleteBehaviorMismatch(
                            Property.Format(foreignKey.Properties),
                            foreignKey.DeclaringEntityType.DisplayName(),
                            Property.Format(duplicateForeignKey.Properties),
                            duplicateForeignKey.DeclaringEntityType.DisplayName(),
                            Format(foreignKey.DeclaringEntityType.Relational()),
                            foreignKey.Relational().Name,
                            foreignKey.DeleteBehavior,
                            duplicateForeignKey.DeleteBehavior));
                }

                return false;
            }

            return true;
        }

        private static string Format(IRelationalEntityTypeAnnotations annotations)
            => (string.IsNullOrEmpty(annotations.Schema) ? "" : annotations.Schema + ".") + annotations.TableName;
    }
}
