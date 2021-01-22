// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;

#nullable enable

namespace Microsoft.EntityFrameworkCore.Metadata.Internal
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static class RelationalForeignKeyExtensions
    {
        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public static bool AreCompatible(
            [NotNull] this IReadOnlyForeignKey foreignKey,
            [NotNull] IReadOnlyForeignKey duplicateForeignKey,
            in StoreObjectIdentifier storeObject,
            bool shouldThrow)
        {
            var principalType = foreignKey.PrincipalKey.IsPrimaryKey()
                ? foreignKey.PrincipalEntityType
                : foreignKey.PrincipalKey.DeclaringEntityType;
            var principalTable = StoreObjectIdentifier.Create(principalType, storeObject.StoreObjectType);

            var duplicatePrincipalType = duplicateForeignKey.PrincipalKey.IsPrimaryKey()
                ? duplicateForeignKey.PrincipalEntityType
                : duplicateForeignKey.PrincipalKey.DeclaringEntityType;
            var duplicatePrincipalTable = StoreObjectIdentifier.Create(duplicatePrincipalType, storeObject.StoreObjectType);

            var columnNames = foreignKey.Properties.GetColumnNames(storeObject);
            var duplicateColumnNames = duplicateForeignKey.Properties.GetColumnNames(storeObject);
            if (columnNames is null
                || duplicateColumnNames is null)
            {
                if (shouldThrow)
                {
                    throw new InvalidOperationException(
                        RelationalStrings.DuplicateForeignKeyTableMismatch(
                            foreignKey.Properties.Format(),
                            foreignKey.DeclaringEntityType.DisplayName(),
                            duplicateForeignKey.Properties.Format(),
                            duplicateForeignKey.DeclaringEntityType.DisplayName(),
                            principalTable.HasValue
                                ? foreignKey.GetConstraintName(storeObject, principalTable.Value)
                                : foreignKey.GetDefaultName(),
                            foreignKey.DeclaringEntityType.GetSchemaQualifiedTableName(),
                            duplicateForeignKey.DeclaringEntityType.GetSchemaQualifiedTableName()));
                }

                return false;
            }

            if (principalTable is null
                || duplicatePrincipalTable is null
                || principalTable != duplicatePrincipalTable
                || !(foreignKey.PrincipalKey.Properties.GetColumnNames(principalTable.Value)
                    is IReadOnlyList<string> principalColumns)
                || !(duplicateForeignKey.PrincipalKey.Properties.GetColumnNames(principalTable.Value)
                    is IReadOnlyList<string> duplicatePrincipalColumns))
            {
                if (shouldThrow)
                {
                    throw new InvalidOperationException(
                        RelationalStrings.DuplicateForeignKeyPrincipalTableMismatch(
                            foreignKey.Properties.Format(),
                            foreignKey.DeclaringEntityType.DisplayName(),
                            duplicateForeignKey.Properties.Format(),
                            duplicateForeignKey.DeclaringEntityType.DisplayName(),
                            foreignKey.DeclaringEntityType.GetSchemaQualifiedTableName(),
                            principalTable.HasValue
                                ? foreignKey.GetConstraintName(storeObject, principalTable.Value)
                                : foreignKey.GetDefaultName(),
                            principalType.GetSchemaQualifiedTableName(),
                            duplicatePrincipalType.GetSchemaQualifiedTableName()));
                }

                return false;
            }

            if (!columnNames.SequenceEqual(duplicateColumnNames))
            {
                if (shouldThrow)
                {
                    throw new InvalidOperationException(
                        RelationalStrings.DuplicateForeignKeyColumnMismatch(
                            foreignKey.Properties.Format(),
                            foreignKey.DeclaringEntityType.DisplayName(),
                            duplicateForeignKey.Properties.Format(),
                            duplicateForeignKey.DeclaringEntityType.DisplayName(),
                            foreignKey.DeclaringEntityType.GetSchemaQualifiedTableName(),
                            foreignKey.GetConstraintName(storeObject, principalTable.Value),
                            foreignKey.Properties.FormatColumns(storeObject),
                            duplicateForeignKey.Properties.FormatColumns(storeObject)));
                }

                return false;
            }

            if (!principalColumns.SequenceEqual(duplicatePrincipalColumns))
            {
                if (shouldThrow)
                {
                    throw new InvalidOperationException(
                        RelationalStrings.DuplicateForeignKeyPrincipalColumnMismatch(
                            foreignKey.Properties.Format(),
                            foreignKey.DeclaringEntityType.DisplayName(),
                            duplicateForeignKey.Properties.Format(),
                            duplicateForeignKey.DeclaringEntityType.DisplayName(),
                            foreignKey.DeclaringEntityType.GetSchemaQualifiedTableName(),
                            foreignKey.GetConstraintName(storeObject, principalTable.Value),
                            foreignKey.PrincipalKey.Properties.FormatColumns(principalTable.Value),
                            duplicateForeignKey.PrincipalKey.Properties.FormatColumns(principalTable.Value)));
                }

                return false;
            }

            if (foreignKey.IsUnique != duplicateForeignKey.IsUnique)
            {
                if (shouldThrow)
                {
                    throw new InvalidOperationException(
                        RelationalStrings.DuplicateForeignKeyUniquenessMismatch(
                            foreignKey.Properties.Format(),
                            foreignKey.DeclaringEntityType.DisplayName(),
                            duplicateForeignKey.Properties.Format(),
                            duplicateForeignKey.DeclaringEntityType.DisplayName(),
                            foreignKey.DeclaringEntityType.GetSchemaQualifiedTableName(),
                            foreignKey.GetConstraintName(storeObject, principalTable.Value)));
                }

                return false;
            }

            var referentialAction = RelationalModel.ToReferentialAction(foreignKey.DeleteBehavior);
            var duplicateReferentialAction = RelationalModel.ToReferentialAction(duplicateForeignKey.DeleteBehavior);
            if (referentialAction != duplicateReferentialAction)
            {
                if (shouldThrow)
                {
                    throw new InvalidOperationException(
                        RelationalStrings.DuplicateForeignKeyDeleteBehaviorMismatch(
                            foreignKey.Properties.Format(),
                            foreignKey.DeclaringEntityType.DisplayName(),
                            duplicateForeignKey.Properties.Format(),
                            duplicateForeignKey.DeclaringEntityType.DisplayName(),
                            foreignKey.DeclaringEntityType.GetSchemaQualifiedTableName(),
                            foreignKey.GetConstraintName(storeObject, principalTable.Value),
                            referentialAction,
                            duplicateReferentialAction));
                }

                return false;
            }

            return true;
        }
    }
}
