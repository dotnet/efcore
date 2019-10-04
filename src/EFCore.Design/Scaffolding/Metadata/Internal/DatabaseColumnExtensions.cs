// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Microsoft.EntityFrameworkCore.Scaffolding.Metadata.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public static class DatabaseColumnExtensions
    {
        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static string DisplayName([NotNull] this DatabaseColumn column)
        {
            var tablePrefix = column.Table?.DisplayName();
            return (!string.IsNullOrEmpty(tablePrefix) ? tablePrefix + "." : "") + column.Name;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static bool IsKeyOrIndex([NotNull] this DatabaseColumn column)
        {
            var table = column.Table;

            if (table.PrimaryKey?.Columns.Contains(column) == true)
            {
                return true;
            }

            if (table.UniqueConstraints.Any(uc => uc.Columns.Contains(column)))
            {
                return true;
            }

            return table.Indexes.Any(uc => uc.Columns.Contains(column));
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static bool IsRowVersion([NotNull] this DatabaseColumn column)
        {
            return column.ValueGenerated == ValueGenerated.OnAddOrUpdate
                   && (bool?)column[ScaffoldingAnnotationNames.ConcurrencyToken] == true;
        }
    }
}
