// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Microsoft.EntityFrameworkCore.Infrastructure
{
    /// <summary>
    ///     Relational extension methods for <see cref="IProperty" />.
    /// </summary>
    public static class RelationalPropertyExtensions
    {
        /// <summary>
        ///     Creates a comma-separated list of column names.
        /// </summary>
        /// <param name="properties"> The properties to format. </param>
        /// <param name="tableName"> The table name. </param>
        /// <param name="schema"> The schema. </param>
        /// <returns> A comma-separated list of column names. </returns>
        public static string FormatColumns(
            [NotNull] this IEnumerable<IProperty> properties,
            [NotNull] string tableName,
            [CanBeNull] string schema)
            => "{" + string.Join(", ", properties.Select(p => "'" + p.GetColumnName(tableName, schema) + "'")) + "}";
    }
}
