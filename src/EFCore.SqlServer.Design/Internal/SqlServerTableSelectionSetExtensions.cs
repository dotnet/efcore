// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;

namespace Microsoft.EntityFrameworkCore.Scaffolding.Internal
{
    internal static class SqlServerTableSelectionSetExtensions
    {
        private static readonly List<string> _schemaPatterns = new List<string>
        {
            "{schema}",
            "[{schema}]"
        };

        private static readonly List<string> _tablePatterns = new List<string>
        {
            "{schema}.{table}",
            "[{schema}].[{table}]",
            "{schema}.[{table}]",
            "[{schema}].{table}",
            "{table}",
            "[{table}]"
        };

        /// <summary>
        ///     Tests whether the schema/table is allowed by the <see cref="TableSelectionSet" />
        ///     and updates the <see cref="TableSelectionSet" />'s <see cref="TableSelectionSet.Selection" />(s)
        ///     to mark that they have been matched.
        /// </summary>
        /// <param name="tableSelectionSet"> the <see cref="TableSelectionSet" /> to test </param>
        /// <param name="schemaName"> name of the database schema to check </param>
        /// <param name="tableName"> name of the database table to check </param>
        /// <returns> whether or not the schema/table is allowed </returns>
        public static bool Allows(this TableSelectionSet tableSelectionSet, [CanBeNull] string schemaName, [NotNull] string tableName)
        {
            if (tableSelectionSet == null
                || (tableSelectionSet.Schemas.Count == 0
                    && tableSelectionSet.Tables.Count == 0))
            {
                return true;
            }

            var result = false;

            //TODO: look into performance for large selection sets and numbers of tables
            if (schemaName != null)
            {
                foreach (var pattern in _schemaPatterns)
                {
                    var patternToMatch = pattern.Replace("{schema}", schemaName);
                    var matchingSchemaSelections = tableSelectionSet.Schemas.Where(
                        s => s.Text.Equals(patternToMatch, StringComparison.OrdinalIgnoreCase))
                        .ToList();
                    if (matchingSchemaSelections.Any())
                    {
                        matchingSchemaSelections.ForEach(selection => selection.IsMatched = true);
                        result = true;
                    }
                }
            }

            foreach (var pattern in _tablePatterns)
            {
                var patternToMatch = pattern.Replace("{schema}", schemaName).Replace("{table}", tableName);
                var matchingTableSelections = tableSelectionSet.Tables.Where(
                    t => t.Text.Equals(patternToMatch, StringComparison.OrdinalIgnoreCase))
                    .ToList();
                if (matchingTableSelections.Any())
                {
                    matchingTableSelections.ForEach(selection => selection.IsMatched = true);
                    result = true;
                }
            }

            return result;
        }
    }
}
