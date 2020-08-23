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
        /// <param name="storeObject"> The identifier of the table-like store object containing the column. </param>
        /// <returns> A comma-separated list of column names. </returns>
        public static string FormatColumns(
            [NotNull] this IEnumerable<IProperty> properties,
            StoreObjectIdentifier storeObject)
            => "{" + string.Join(", ", properties.Select(p => "'" + p.GetColumnName(storeObject) + "'")) + "}";

        /// <summary>
        ///     Creates a list of column names.
        /// </summary>
        /// <param name="properties"> The properties to format. </param>
        /// <param name="storeObject"> The identifier of the table-like store object containing the column. </param>
        /// <returns> A list of column names. </returns>
        public static IReadOnlyList<string> GetColumnNames(
            [NotNull] this IEnumerable<IProperty> properties,
            in StoreObjectIdentifier storeObject)
        {
            var propertyNames = new List<string>();
            foreach (var property in properties)
            {
                var columnName = property.GetColumnName(storeObject);
                if (columnName == null)
                {
                    return null;
                }

                propertyNames.Add(columnName);
            }

            return propertyNames;
        }
    }
}
