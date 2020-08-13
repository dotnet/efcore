// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Microsoft.EntityFrameworkCore.Metadata.Internal
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static class RelationalIndexExtensions
    {
        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public static bool AreCompatible(
            [NotNull] this IIndex index,
            [NotNull] IIndex duplicateIndex,
            in StoreObjectIdentifier storeObject,
            bool shouldThrow)
        {
            var columnNames = index.Properties.GetColumnNames(storeObject);
            var duplicateColumnNames = duplicateIndex.Properties.GetColumnNames(storeObject);
            if (columnNames == null
                || duplicateColumnNames == null)
            {
                if (shouldThrow)
                {
                    throw new InvalidOperationException(
                        RelationalStrings.DuplicateIndexTableMismatch(
                            index.Properties.Format(),
                            index.DeclaringEntityType.DisplayName(),
                            duplicateIndex.Properties.Format(),
                            duplicateIndex.DeclaringEntityType.DisplayName(),
                            index.GetDatabaseName(storeObject),
                            index.DeclaringEntityType.GetSchemaQualifiedTableName(),
                            duplicateIndex.DeclaringEntityType.GetSchemaQualifiedTableName()));
                }

                return false;
            }

            if (!columnNames.SequenceEqual(duplicateColumnNames))
            {
                if (shouldThrow)
                {
                    throw new InvalidOperationException(
                        RelationalStrings.DuplicateIndexColumnMismatch(
                            index.Properties.Format(),
                            index.DeclaringEntityType.DisplayName(),
                            duplicateIndex.Properties.Format(),
                            duplicateIndex.DeclaringEntityType.DisplayName(),
                            index.DeclaringEntityType.GetSchemaQualifiedTableName(),
                            index.GetDatabaseName(storeObject),
                            index.Properties.FormatColumns(storeObject),
                            duplicateIndex.Properties.FormatColumns(storeObject)));
                }

                return false;
            }

            if (index.IsUnique != duplicateIndex.IsUnique)
            {
                if (shouldThrow)
                {
                    throw new InvalidOperationException(
                        RelationalStrings.DuplicateIndexUniquenessMismatch(
                            index.Properties.Format(),
                            index.DeclaringEntityType.DisplayName(),
                            duplicateIndex.Properties.Format(),
                            duplicateIndex.DeclaringEntityType.DisplayName(),
                            index.DeclaringEntityType.GetSchemaQualifiedTableName(),
                            index.GetDatabaseName(storeObject)));
                }

                return false;
            }

            return true;
        }
    }
}
