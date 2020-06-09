// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.SqlServer.Internal;

namespace Microsoft.EntityFrameworkCore.Metadata.Internal
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static class SqlServerIndexExtensions
    {
        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public static bool AreCompatibleForSqlServer(
            [NotNull] this IIndex index,
            [NotNull] IIndex duplicateIndex,
            [NotNull] string tableName,
            [CanBeNull] string schema,
            bool shouldThrow)
        {
            if (index.GetIncludeProperties() != duplicateIndex.GetIncludeProperties())
            {
                if (index.GetIncludeProperties() == null
                    || duplicateIndex.GetIncludeProperties() == null
                    || !index.GetIncludeProperties().Select(
                        p => index.DeclaringEntityType.FindProperty(p).GetColumnName(tableName, schema))
                        .SequenceEqual(
                            duplicateIndex.GetIncludeProperties().Select(
                                p => duplicateIndex.DeclaringEntityType.FindProperty(p).GetColumnName(tableName, schema))))
                {
                    if (shouldThrow)
                    {
                        throw new InvalidOperationException(
                            SqlServerStrings.DuplicateIndexIncludedMismatch(
                                index.Properties.Format(),
                                index.DeclaringEntityType.DisplayName(),
                                duplicateIndex.Properties.Format(),
                                duplicateIndex.DeclaringEntityType.DisplayName(),
                                index.DeclaringEntityType.GetSchemaQualifiedTableName(),
                                index.GetDatabaseName(tableName, schema),
                                FormatInclude(index, tableName, schema),
                                FormatInclude(duplicateIndex, tableName, schema)));
                    }

                    return false;
                }
            }

            if (index.IsCreatedOnline() != duplicateIndex.IsCreatedOnline())
            {
                if (shouldThrow)
                {
                    throw new InvalidOperationException(
                        SqlServerStrings.DuplicateIndexOnlineMismatch(
                            index.Properties.Format(),
                            index.DeclaringEntityType.DisplayName(),
                            duplicateIndex.Properties.Format(),
                            duplicateIndex.DeclaringEntityType.DisplayName(),
                            index.DeclaringEntityType.GetSchemaQualifiedTableName(),
                            index.GetDatabaseName(tableName, schema)));
                }

                return false;
            }

            if (index.IsClustered(tableName, schema) != duplicateIndex.IsClustered(tableName, schema))
            {
                if (shouldThrow)
                {
                    throw new InvalidOperationException(
                        SqlServerStrings.DuplicateIndexClusteredMismatch(
                            index.Properties.Format(),
                            index.DeclaringEntityType.DisplayName(),
                            duplicateIndex.Properties.Format(),
                            duplicateIndex.DeclaringEntityType.DisplayName(),
                            index.DeclaringEntityType.GetSchemaQualifiedTableName(),
                            index.GetDatabaseName(tableName, schema)));
                }

                return false;
            }

            if (index.GetFillFactor() != duplicateIndex.GetFillFactor())
            {
                if (shouldThrow)
                {
                    throw new InvalidOperationException(
                        SqlServerStrings.DuplicateIndexFillFactorMismatch(
                            index.Properties.Format(),
                            index.DeclaringEntityType.DisplayName(),
                            duplicateIndex.Properties.Format(),
                            duplicateIndex.DeclaringEntityType.DisplayName(),
                            index.DeclaringEntityType.GetSchemaQualifiedTableName(),
                            index.GetDatabaseName(tableName, schema)));
                }

                return false;
            }

            return true;
        }

        private static string FormatInclude(IIndex index, string tableName, string schema)
            => index.GetIncludeProperties() == null
                ? "{}"
                : "{'"
                    + string.Join("', '",
                        index.GetIncludeProperties().Select(p => index.DeclaringEntityType.FindProperty(p)?.GetColumnName(tableName, schema)))
                    + "'}";
    }
}
