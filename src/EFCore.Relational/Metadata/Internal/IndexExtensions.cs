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
    public static class IndexExtensions
    {
        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static bool AreCompatible([NotNull] this IIndex index, [NotNull] IIndex duplicateIndex, bool shouldThrow)
        {
            if (!index.Properties.Select(p => p.Relational().ColumnName)
                .SequenceEqual(duplicateIndex.Properties.Select(p => p.Relational().ColumnName)))
            {
                if (shouldThrow)
                {
                    throw new InvalidOperationException(
                        RelationalStrings.DuplicateIndexColumnMismatch(
                            Property.Format(index.Properties),
                            index.DeclaringEntityType.DisplayName(),
                            Property.Format(duplicateIndex.Properties),
                            duplicateIndex.DeclaringEntityType.DisplayName(),
                            Format(index.DeclaringEntityType.Relational()),
                            index.Relational().Name,
                            index.Properties.FormatColumns(),
                            duplicateIndex.Properties.FormatColumns()));
                }

                return false;
            }

            if (index.IsUnique != duplicateIndex.IsUnique)
            {
                if (shouldThrow)
                {
                    throw new InvalidOperationException(
                        RelationalStrings.DuplicateIndexUniquenessMismatch(
                            Property.Format(index.Properties),
                            index.DeclaringEntityType.DisplayName(),
                            Property.Format(duplicateIndex.Properties),
                            duplicateIndex.DeclaringEntityType.DisplayName(),
                            Format(index.DeclaringEntityType.Relational()),
                            index.Relational().Name));
                }

                return false;
            }

            return true;
        }

        private static string Format(IRelationalEntityTypeAnnotations annotations)
            => (string.IsNullOrEmpty(annotations.Schema) ? "" : annotations.Schema + ".") + annotations.TableName;
    }
}
