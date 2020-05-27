// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore.Internal
{
    /// <summary>
    ///     Extension methods for tuples.
    /// </summary>
    public static class TupleExtensions
    {
        /// <summary>
        ///     Creates a formatted string representation of the given tables such as is useful
        ///     when throwing exceptions.
        /// </summary>
        /// <param name="tables"> The (Table, Schema) tuples to format. </param>
        /// <returns> The string representation. </returns>
        public static string FormatTables([NotNull] this IEnumerable<(string Table, string Schema)> tables)
            => "{"
                + string.Join(", ", tables.Select(FormatTable))
                + "}";

        /// <summary>
        ///     Creates a formatted string representation of the given table such as is useful
        ///     when throwing exceptions.
        /// </summary>
        /// <param name="table"> The (Table, Schema) tuple to format. </param>
        /// <returns> The string representation. </returns>
        public static string FormatTable(this (string Table, string Schema) table)
            => table.Schema == null ? table.Table : table.Schema + "." + table.Table;
    }
}
