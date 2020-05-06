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
    public static class RelationalKeyExtensions
    {
        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public static bool AreCompatible(
            [NotNull] this IKey key,
            [NotNull] IKey duplicateKey,
            [NotNull] string tableName,
            [CanBeNull] string schema,
            bool shouldThrow)
        {
            if (!key.Properties.Select(p => p.GetColumnName(tableName, schema))
                .SequenceEqual(duplicateKey.Properties.Select(p => p.GetColumnName(tableName, schema))))
            {
                if (shouldThrow)
                {
                    throw new InvalidOperationException(
                    RelationalStrings.DuplicateKeyColumnMismatch(
                        key.Properties.Format(),
                        key.DeclaringEntityType.DisplayName(),
                        duplicateKey.Properties.Format(),
                        duplicateKey.DeclaringEntityType.DisplayName(),
                        key.DeclaringEntityType.GetSchemaQualifiedTableName(),
                        key.GetName(tableName, schema),
                        key.Properties.FormatColumns(tableName, schema),
                        duplicateKey.Properties.FormatColumns(tableName, schema)));
                }

                return false;
            }

            return true;
        }
    }
}
