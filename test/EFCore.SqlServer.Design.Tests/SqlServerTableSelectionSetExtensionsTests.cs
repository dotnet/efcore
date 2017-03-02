// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore.Scaffolding;
using Microsoft.EntityFrameworkCore.Scaffolding.Internal;
using Xunit;

namespace Microsoft.EntityFrameworkCore.SqlServer.Design
{
    public class SqlServerTableSelectionSetExtensionsTests
    {
        [Fact]
        public void Allows_updates_IsMatched_only_for_matching_selections()
        {
            var tableNames = new List<string> { "table0", "[table0]", "table1", "[table1]" };
            var schemaNames = new List<string> { "schemaA", "[schemaA]", "schemaB", "[schemaB]" };
            var tableSelectionSet = new TableSelectionSet(tableNames, schemaNames);

            Assert.Equal(4, tableSelectionSet.Schemas.Count);
            Assert.Equal(4, tableSelectionSet.Tables.Count);

            // Allows() has not run yet - so all selections should have IsMatched false
            foreach (var schema in tableSelectionSet.Schemas)
            {
                Assert.False(schema.IsMatched);
            }
            foreach (var table in tableSelectionSet.Tables)
            {
                Assert.False(table.IsMatched);
            }

            var schemaASelection = tableSelectionSet.Schemas.First();
            var schemaASelectionWithSquareBrackets = tableSelectionSet.Schemas.Skip(1).First();
            var schemaBSelection = tableSelectionSet.Schemas.Skip(2).First();
            var schemaBSelectionWithSquareBrackets = tableSelectionSet.Schemas.Skip(3).First();
            var table0Selection = tableSelectionSet.Tables.First();
            var table0SelectionWithSquareBrackets = tableSelectionSet.Tables.Skip(1).First();
            var table1Selection = tableSelectionSet.Tables.Skip(2).First();
            var table1SelectionWithSquareBrackets = tableSelectionSet.Tables.Skip(3).First();

            // When Allows() runs against a table which matches just on a schema then
            // those schema selections' IsMatched are marked true (regardless of square brackets)
            // but the table selections' IsMatched remain false
            Assert.True(tableSelectionSet.Allows("schemaA", "tableOther"));
            Assert.True(schemaASelection.IsMatched);
            Assert.True(schemaASelectionWithSquareBrackets.IsMatched);
            Assert.False(schemaBSelection.IsMatched);
            Assert.False(schemaBSelectionWithSquareBrackets.IsMatched);
            Assert.False(table0Selection.IsMatched);
            Assert.False(table0SelectionWithSquareBrackets.IsMatched);
            Assert.False(table1Selection.IsMatched);
            Assert.False(table1SelectionWithSquareBrackets.IsMatched);

            // When Allows() runs against a table which matches just on a table then
            // those table selections' IsMatched are marked true (regardless of square brackets)
            // the schema selections' IsMatched remain as they were before
            Assert.True(tableSelectionSet.Allows("schemaOther", "table0"));
            Assert.True(schemaASelection.IsMatched);
            Assert.True(schemaASelectionWithSquareBrackets.IsMatched);
            Assert.False(schemaBSelection.IsMatched);
            Assert.False(schemaBSelectionWithSquareBrackets.IsMatched);
            Assert.True(table0Selection.IsMatched);
            Assert.True(table0SelectionWithSquareBrackets.IsMatched);
            Assert.False(table1Selection.IsMatched);
            Assert.False(table1SelectionWithSquareBrackets.IsMatched);

            // When Allows() runs against a non-selected schema/table no further schema or table selection is marked true
            Assert.False(tableSelectionSet.Allows("schemaOther", "tableOther"));
            Assert.True(schemaASelection.IsMatched);
            Assert.True(schemaASelectionWithSquareBrackets.IsMatched);
            Assert.False(schemaBSelection.IsMatched);
            Assert.False(schemaBSelectionWithSquareBrackets.IsMatched);
            Assert.True(table0Selection.IsMatched);
            Assert.True(table0SelectionWithSquareBrackets.IsMatched);
            Assert.False(table1Selection.IsMatched);
            Assert.False(table1SelectionWithSquareBrackets.IsMatched);

            // When Allows() runs against a table which matches on both schema and table then
            // both the matching schema selections' and the matching table selections' IsMatched
            // are marked true (regardless of square brackets)
            Assert.True(tableSelectionSet.Allows("schemaB", "table1"));
            Assert.True(schemaASelection.IsMatched);
            Assert.True(schemaASelectionWithSquareBrackets.IsMatched);
            Assert.True(schemaBSelection.IsMatched);
            Assert.True(schemaBSelectionWithSquareBrackets.IsMatched);
            Assert.True(table0Selection.IsMatched);
            Assert.True(table0SelectionWithSquareBrackets.IsMatched);
            Assert.True(table1Selection.IsMatched);
            Assert.True(table1SelectionWithSquareBrackets.IsMatched);
        }

        [Fact]
        public void Allows_updates_IsMatched_for_matching_table_selections_which_specify_schema()
        {
            var tableNames = new List<string>
            {
                "schema0.table0", "[schema0].[table0]", "[schema0].table0", "schema0.[table0]",
                "schema0.table1", "[schema0].[table1]", "[schema0].table1", "schema0.[table1]",
                "schema1.table0", "[schema1].[table0]", "[schema1].table0", "schema1.[table0]"
            };
            var tableSelectionSet = new TableSelectionSet(tableNames);

            Assert.Equal(0, tableSelectionSet.Schemas.Count);
            Assert.Equal(12, tableSelectionSet.Tables.Count);

            foreach (var table in tableSelectionSet.Tables)
            {
                Assert.False(table.IsMatched);
            }

            Assert.True(tableSelectionSet.Allows("schema0", "table0"));
            foreach (var table in tableSelectionSet.Tables.Take(4))
            {
                Assert.True(table.IsMatched);
            }
            foreach (var table in tableSelectionSet.Tables.Skip(4))
            {
                Assert.False(table.IsMatched);
            }
        }
    }
}
