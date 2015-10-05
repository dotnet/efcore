// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;

namespace Microsoft.Data.Entity.Scaffolding
{
    // TODO:
    // Does not support schema or table names containing FilterSeparator.
    // Does not support schema names (or schemaless table names) that start with ExcludeIndicator.
    // Does not support schema names that contain SchemaTableSeparator.
    public static class TableSelectionSetBuilder
    {
        private static readonly char[] FilterSeparator = new [] { ',' };
        private static readonly string ExcludeIndicator = "-";
        private static readonly char SchemaTableSeparator = ':';

        public static TableSelectionSet BuildFromString([CanBeNull] string filters)
        {
            var tableSelectionSet = new TableSelectionSet();
            if (string.IsNullOrWhiteSpace(filters))
            {
                return tableSelectionSet;
            }

            var filterSet = filters.Split(FilterSeparator, StringSplitOptions.RemoveEmptyEntries);
            foreach(var filterString in filterSet)
            {
                var filter = filterString;
                var exclude = false;
                if (filter.StartsWith(ExcludeIndicator, StringComparison.Ordinal))
                {
                    exclude = true;
                    filter = filter.Substring(1);
                }

                string schema = TableSelection.Any;
                string table = TableSelection.Any;
                var schemaTableIndex = filter.IndexOf(SchemaTableSeparator);
                if (schemaTableIndex > 0)
                {
                    schema = filter.Substring(0, schemaTableIndex);
                }
                table = filter.Substring(schemaTableIndex + 1);
                tableSelectionSet.AddSelections(
                    new TableSelection() { Schema = schema, Table = table, Exclude = exclude });
            }

            return tableSelectionSet;
        }
    }
}
