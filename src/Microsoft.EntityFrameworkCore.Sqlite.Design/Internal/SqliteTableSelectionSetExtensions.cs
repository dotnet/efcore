// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;

namespace Microsoft.EntityFrameworkCore.Scaffolding.Internal
{
    internal static class SqliteTableSelectionSetExtensions
    {
        /// <summary>
        ///     Tests whether the table is allowed by the <see cref="TableSelectionSet" /> and
        ///     updates the <see cref="TableSelectionSet" />'s <see cref="TableSelectionSet.Selection" />(s)
        ///     to mark that they have been matched.
        /// </summary>
        /// <param name="tableSet"> the <see cref="TableSelectionSet" /> to test </param>
        /// <param name="tableName"> name of the database table to check </param>
        /// <returns> whether or not the table is allowed </returns>
        public static bool Allows(this TableSelectionSet tableSet, string tableName)
        {
            if (tableSet == null
                || tableSet.Tables.Count == 0)
            {
                return true;
            }

            //TODO: look into performance for large selection sets and numbers of tables
            var result = false;
            var matchingTableSelections = tableSet.Tables.Where(
                t => t.Text.Equals(tableName, StringComparison.OrdinalIgnoreCase))
                .ToList();
            if (matchingTableSelections.Any())
            {
                matchingTableSelections.ForEach(selection => selection.IsMatched = true);
                result = true;
            }

            return result;
        }
    }
}
