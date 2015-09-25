// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using Xunit;

namespace Microsoft.Data.Entity.Relational.Design.ReverseEngineering.Tests
{
    public class TableSelectionSetTests
    {
        [Fact]
        public void It_correctly_assigns_excludes_and_includes()
        {
            var includeA = new TableSelection() { Schema = "A", Table = "A" };
            var includeB = new TableSelection() { Schema = "B", Table = "B" };
            var includeC = new TableSelection() { Schema = "C", Table = "C" };

            var excludeZ = new TableSelection() { Schema = "Z", Table = "Z", Exclude = true };
            var excludeY = new TableSelection() { Schema = "Y", Table = "Y", Exclude = true };
            var excludeX = new TableSelection() { Schema = "X", Table = "X", Exclude = true };

            var tableSelectionSet = new TableSelectionSet();
            tableSelectionSet.AddSelections(new [] { excludeZ, includeB, excludeX });
            tableSelectionSet.AddSelections(new [] { includeA, excludeY, includeC });

            Assert.Equal(new List<TableSelection> { includeB, includeA, includeC }, tableSelectionSet.InclusiveSelections);
            Assert.Equal(new List<TableSelection> { excludeZ, excludeX, excludeY }, tableSelectionSet.ExclusiveSelections);
        }

        [Fact]
        public void Inserting_an_AnySchema_selection_removes_previous_selections_with_that_table()
        {
            var includeA1 = new TableSelection() { Schema = "A", Table = "First" };
            var includeA2 = new TableSelection() { Schema = "A", Table = "Second" };
            var includeB1 = new TableSelection() { Schema = "B", Table = "First" };
            var includeB2 = new TableSelection() { Schema = "B", Table = "Second" };

            var excludeA1 = new TableSelection() { Schema = "A", Table = "First", Exclude = true };
            var excludeA2 = new TableSelection() { Schema = "A", Table = "Second", Exclude = true };
            var excludeB1 = new TableSelection() { Schema = "B", Table = "First", Exclude = true };
            var excludeB2 = new TableSelection() { Schema = "B", Table = "Second", Exclude = true };

            var includeTableFirstAnySchema = new TableSelection() { Schema = TableSelection.Any, Table = "First" };
            var excludeTableSecondAnySchema = new TableSelection() { Schema = TableSelection.Any, Table = "Second", Exclude = true };

            var tableSelectionSet = new TableSelectionSet();
            tableSelectionSet.AddSelections(new [] { includeA1, includeA2, includeB1, includeB2 });
            tableSelectionSet.AddSelections(new [] { excludeA1, excludeA2, excludeB1, excludeB2 });
            tableSelectionSet.AddSelections(new [] { includeTableFirstAnySchema });
            tableSelectionSet.AddSelections(new [] { excludeTableSecondAnySchema });

            Assert.Equal(new List<TableSelection> { includeA2, includeB2, includeTableFirstAnySchema }, tableSelectionSet.InclusiveSelections);
            Assert.Equal(new List<TableSelection> { excludeA1, excludeB1, excludeTableSecondAnySchema }, tableSelectionSet.ExclusiveSelections);
        }

        [Fact]
        public void Inserting_an_AnyTable_selection_removes_previous_selections_with_that_schema()
        {
            var includeA1 = new TableSelection() { Schema = "A", Table = "First" };
            var includeA2 = new TableSelection() { Schema = "A", Table = "Second" };
            var includeB1 = new TableSelection() { Schema = "B", Table = "First" };
            var includeB2 = new TableSelection() { Schema = "B", Table = "Second" };

            var excludeA1 = new TableSelection() { Schema = "A", Table = "First", Exclude = true };
            var excludeA2 = new TableSelection() { Schema = "A", Table = "Second", Exclude = true };
            var excludeB1 = new TableSelection() { Schema = "B", Table = "First", Exclude = true };
            var excludeB2 = new TableSelection() { Schema = "B", Table = "Second", Exclude = true };

            var includeSchemaAAnyTable = new TableSelection() { Schema = "A", Table = TableSelection.Any };
            var excludeSchemaBAnyTable = new TableSelection() { Schema = "B", Table = TableSelection.Any, Exclude = true };

            var tableSelectionSet = new TableSelectionSet();
            tableSelectionSet.AddSelections(new [] { includeA1, includeA2, includeB1, includeB2 });
            tableSelectionSet.AddSelections(new [] { excludeA1, excludeA2, excludeB1, excludeB2 });
            tableSelectionSet.AddSelections(new [] { includeSchemaAAnyTable });
            tableSelectionSet.AddSelections(new [] { excludeSchemaBAnyTable });

            Assert.Equal(new List<TableSelection> { includeB1, includeB2, includeSchemaAAnyTable }, tableSelectionSet.InclusiveSelections);
            Assert.Equal(new List<TableSelection> { excludeA1, excludeA2, excludeSchemaBAnyTable }, tableSelectionSet.ExclusiveSelections);
        }

        [Fact]
        public void Inserting_an_AnySchemaAnyTable_selection_removes_all_previous_selections()
        {
            var includeA1 = new TableSelection() { Schema = "A", Table = "First" };
            var includeA2 = new TableSelection() { Schema = "A", Table = "Second" };
            var includeB1 = new TableSelection() { Schema = "B", Table = "First" };
            var includeB2 = new TableSelection() { Schema = "B", Table = "Second" };

            var excludeA1 = new TableSelection() { Schema = "A", Table = "First", Exclude = true };
            var excludeA2 = new TableSelection() { Schema = "A", Table = "Second", Exclude = true };
            var excludeB1 = new TableSelection() { Schema = "B", Table = "First", Exclude = true };
            var excludeB2 = new TableSelection() { Schema = "B", Table = "Second", Exclude = true };

            var tableSelectionSet = new TableSelectionSet();
            tableSelectionSet.AddSelections(new [] { includeA1, includeA2, includeB1, includeB2 });
            tableSelectionSet.AddSelections(new [] { excludeA1, excludeA2, excludeB1, excludeB2 });
            tableSelectionSet.AddSelections(new [] { TableSelection.InclusiveAll });
            tableSelectionSet.AddSelections(new [] { TableSelection.ExclusiveAll });

            Assert.Equal(new List<TableSelection> { TableSelection.InclusiveAll }, tableSelectionSet.InclusiveSelections);
            Assert.Equal(new List<TableSelection> { TableSelection.ExclusiveAll }, tableSelectionSet.ExclusiveSelections);
        }

        [Fact]
        public void Re_inserting_a_specific_selection_does_not_change_selections()
        {
            var includeA = new TableSelection() { Schema = "A", Table = "A" };
            var includeB = new TableSelection() { Schema = "B", Table = "B" };
            var includeC = new TableSelection() { Schema = "C", Table = "C" };

            var excludeZ = new TableSelection() { Schema = "Z", Table = "Z", Exclude = true };
            var excludeY = new TableSelection() { Schema = "Y", Table = "Y", Exclude = true };
            var excludeX = new TableSelection() { Schema = "X", Table = "X", Exclude = true };

            var tableSelectionSet = new TableSelectionSet();
            tableSelectionSet.AddSelections(new[] { includeA, includeB, includeC });
            tableSelectionSet.AddSelections(new[] { excludeX, excludeY, excludeZ });

            // re-insert same selections
            tableSelectionSet.AddSelections(new[] { includeA });
            tableSelectionSet.AddSelections(new[] { excludeZ });

            Assert.Equal(new List<TableSelection> { includeA, includeB, includeC }, tableSelectionSet.InclusiveSelections);
            Assert.Equal(new List<TableSelection> { excludeX, excludeY, excludeZ }, tableSelectionSet.ExclusiveSelections);
        }

        [Fact]
        public void If_an_AnySchema_selection_exists_adding_a_new_specific_selection_which_matches_does_not_change_selections()
        {
            var includeA1 = new TableSelection() { Schema = "A", Table = "First" };
            var includeA2 = new TableSelection() { Schema = "A", Table = "Second" };
            var includeB1 = new TableSelection() { Schema = "B", Table = "First" };
            var includeB2 = new TableSelection() { Schema = "B", Table = "Second" };

            var excludeA1 = new TableSelection() { Schema = "A", Table = "First", Exclude = true };
            var excludeA2 = new TableSelection() { Schema = "A", Table = "Second", Exclude = true };
            var excludeB1 = new TableSelection() { Schema = "B", Table = "First", Exclude = true };
            var excludeB2 = new TableSelection() { Schema = "B", Table = "Second", Exclude = true };

            var includeTableFirstAnySchema = new TableSelection() { Schema = TableSelection.Any, Table = "First" };
            var excludeTableSecondAnySchema = new TableSelection() { Schema = TableSelection.Any, Table = "Second", Exclude = true };

            var tableSelectionSet = new TableSelectionSet();
            tableSelectionSet.AddSelections(new[] { includeTableFirstAnySchema });
            tableSelectionSet.AddSelections(new[] { excludeTableSecondAnySchema });
            tableSelectionSet.AddSelections(new[] { includeA1, includeA2, includeB1, includeB2 });
            tableSelectionSet.AddSelections(new[] { excludeA1, excludeA2, excludeB1, excludeB2 });

            Assert.Equal(new List<TableSelection> { includeTableFirstAnySchema, includeA2, includeB2 }, tableSelectionSet.InclusiveSelections);
            Assert.Equal(new List<TableSelection> { excludeTableSecondAnySchema, excludeA1, excludeB1 }, tableSelectionSet.ExclusiveSelections);
        }

        [Fact]
        public void If_an_AnyTable_selection_exists_adding_a_new_specific_selection_which_matches_does_not_change_selections()
        {
            var includeA1 = new TableSelection() { Schema = "A", Table = "First" };
            var includeA2 = new TableSelection() { Schema = "A", Table = "Second" };
            var includeB1 = new TableSelection() { Schema = "B", Table = "First" };
            var includeB2 = new TableSelection() { Schema = "B", Table = "Second" };

            var excludeA1 = new TableSelection() { Schema = "A", Table = "First", Exclude = true };
            var excludeA2 = new TableSelection() { Schema = "A", Table = "Second", Exclude = true };
            var excludeB1 = new TableSelection() { Schema = "B", Table = "First", Exclude = true };
            var excludeB2 = new TableSelection() { Schema = "B", Table = "Second", Exclude = true };

            var includeSchemaAAnyTable = new TableSelection() { Schema = "A", Table = TableSelection.Any };
            var excludeSchemaBAnyTable = new TableSelection() { Schema = "B", Table = TableSelection.Any, Exclude = true };

            var tableSelectionSet = new TableSelectionSet();
            tableSelectionSet.AddSelections(new[] { includeSchemaAAnyTable });
            tableSelectionSet.AddSelections(new[] { excludeSchemaBAnyTable });
            tableSelectionSet.AddSelections(new[] { includeA1, includeA2, includeB1, includeB2 });
            tableSelectionSet.AddSelections(new[] { excludeA1, excludeA2, excludeB1, excludeB2 });

            Assert.Equal(new List<TableSelection> { includeSchemaAAnyTable, includeB1, includeB2 }, tableSelectionSet.InclusiveSelections);
            Assert.Equal(new List<TableSelection> { excludeSchemaBAnyTable, excludeA1, excludeA2 }, tableSelectionSet.ExclusiveSelections);
        }

        [Fact]
        public void If_an_AnySchemaAnyTable_selection_exists_adding_any_new_selection_does_not_change_selections()
        {
            var includeA1 = new TableSelection() { Schema = "A", Table = "First" };
            var includeA2 = new TableSelection() { Schema = "A", Table = "Second" };
            var includeB1 = new TableSelection() { Schema = "B", Table = "First" };
            var includeB2 = new TableSelection() { Schema = "B", Table = "Second" };

            var excludeA1 = new TableSelection() { Schema = "A", Table = "First", Exclude = true };
            var excludeA2 = new TableSelection() { Schema = "A", Table = "Second", Exclude = true };
            var excludeB1 = new TableSelection() { Schema = "B", Table = "First", Exclude = true };
            var excludeB2 = new TableSelection() { Schema = "B", Table = "Second", Exclude = true };

            var includeSchemaAAnyTable = new TableSelection() { Schema = "A", Table = TableSelection.Any };
            var excludeSchemaBAnyTable = new TableSelection() { Schema = "B", Table = TableSelection.Any, Exclude = true };
            var includeTableFirstAnySchema = new TableSelection() { Schema = TableSelection.Any, Table = "First" };
            var excludeTableSecondAnySchema = new TableSelection() { Schema = TableSelection.Any, Table = "Second", Exclude = true };

            var tableSelectionSet = new TableSelectionSet();
            tableSelectionSet.AddSelections(new[] { TableSelection.InclusiveAll });
            tableSelectionSet.AddSelections(new[] { TableSelection.ExclusiveAll });
            tableSelectionSet.AddSelections(new[] { includeSchemaAAnyTable, excludeSchemaBAnyTable, includeTableFirstAnySchema, excludeTableSecondAnySchema });

            Assert.Equal(new List<TableSelection> { TableSelection.InclusiveAll }, tableSelectionSet.InclusiveSelections);
            Assert.Equal(new List<TableSelection> { TableSelection.ExclusiveAll }, tableSelectionSet.ExclusiveSelections);

            tableSelectionSet.AddSelections(new[] { includeA1, includeA2, includeB1, includeB2 });
            tableSelectionSet.AddSelections(new[] { excludeA1, excludeA2, excludeB1, excludeB2 });

            Assert.Equal(new List<TableSelection> { TableSelection.InclusiveAll }, tableSelectionSet.InclusiveSelections);
            Assert.Equal(new List<TableSelection> { TableSelection.ExclusiveAll }, tableSelectionSet.ExclusiveSelections);
        }

        [Fact]
        public void IncludeAll_allows_all_selections()
        {
            var tableSelectionSet = new TableSelectionSet();

            Assert.True(tableSelectionSet.Allows("schema1", "table1"));
            Assert.True(tableSelectionSet.Allows("schema2", "table2"));
            Assert.True(tableSelectionSet.Allows("schema3", "table3"));
            Assert.True(tableSelectionSet.Allows("schemaN", "tableN"));

            // this has the same effect as above
            tableSelectionSet.AddSelections(TableSelection.InclusiveAll);

            Assert.True(tableSelectionSet.Allows("schema1", "table1"));
            Assert.True(tableSelectionSet.Allows("schema2", "table2"));
            Assert.True(tableSelectionSet.Allows("schema3", "table3"));
            Assert.True(tableSelectionSet.Allows("schemaN", "tableN"));
        }


        [Fact]
        public void ExcludeAll_disallows_all_selections()
        {
            var tableSelectionSet = new TableSelectionSet();
            tableSelectionSet.AddSelections(TableSelection.ExclusiveAll);

            Assert.False(tableSelectionSet.Allows("schema1", "table1"));
            Assert.False(tableSelectionSet.Allows("schema2", "table2"));
            Assert.False(tableSelectionSet.Allows("schema3", "table3"));
            Assert.False(tableSelectionSet.Allows("schemaN", "tableN"));
        }

        [Fact]
        public void IncludeAnySchema_allows_all_selections_with_that_table()
        {
            var tableSelectionSet = new TableSelectionSet();
            tableSelectionSet.AddSelections(new TableSelection() { Schema = TableSelection.Any, Table = "SpecificTable" });

            Assert.True(tableSelectionSet.Allows("schema1", "SpecificTable"));
            Assert.False(tableSelectionSet.Allows("schema2", "OtherTable"));
            Assert.True(tableSelectionSet.Allows("schema3", "SpecificTable"));
            Assert.False(tableSelectionSet.Allows("schemaN", "OtherTable"));
        }

        [Fact]
        public void ExcludeAnySchema_disallows_all_selections_with_that_table()
        {
            var tableSelectionSet = new TableSelectionSet();
            tableSelectionSet.AddSelections(new TableSelection() { Schema = TableSelection.Any, Table = "SpecificTable", Exclude = true });

            Assert.False(tableSelectionSet.Allows("schema1", "SpecificTable"));
            Assert.True(tableSelectionSet.Allows("schema2", "OtherTable"));
            Assert.False(tableSelectionSet.Allows("schema3", "SpecificTable"));
            Assert.True(tableSelectionSet.Allows("schemaN", "OtherTable"));
        }

        [Fact]
        public void IncludeAnyTable_allows_all_selections_with_that_schema()
        {
            var tableSelectionSet = new TableSelectionSet();
            tableSelectionSet.AddSelections(new TableSelection() { Schema = "SpecificSchema", Table = TableSelection.Any });

            Assert.True(tableSelectionSet.Allows("SpecificSchema", "table1"));
            Assert.False(tableSelectionSet.Allows("OtherSchema", "table2"));
            Assert.True(tableSelectionSet.Allows("SpecificSchema", "table3"));
            Assert.False(tableSelectionSet.Allows("YetAnotherSchema", "table4"));
        }

        [Fact]
        public void ExcludeAnyTable_disallows_all_selections_with_that_schema()
        {
            var tableSelectionSet = new TableSelectionSet();
            tableSelectionSet.AddSelections(new TableSelection() { Schema = "SpecificSchema", Table = TableSelection.Any, Exclude = true });

            Assert.False(tableSelectionSet.Allows("SpecificSchema", "table1"));
            Assert.True(tableSelectionSet.Allows("OtherSchema", "table2"));
            Assert.False(tableSelectionSet.Allows("SpecificSchema", "table3"));
            Assert.True(tableSelectionSet.Allows("YetAnotherSchema", "table4"));
        }

        [Fact]
        public void Include_specific_tables_allows_only_those_specific_selections()
        {
            var include1 = new TableSelection() { Schema = "schema1", Table = "table1" };
            var include2 = new TableSelection() { Schema = "schema2", Table = "table2" };

            var tableSelectionSet = new TableSelectionSet();
            tableSelectionSet.AddSelections(new [] { include1, include2 });

            Assert.True(tableSelectionSet.Allows("schema1", "table1"));
            Assert.True(tableSelectionSet.Allows("schema2", "table2"));
            Assert.False(tableSelectionSet.Allows("schema1", "table2"));
            Assert.False(tableSelectionSet.Allows("schema2", "table1"));
        }

        [Fact]
        public void Exclude_specific_tables_disallows_only_those_specific_selections()
        {
            var exclude1 = new TableSelection() { Schema = "schema1", Table = "table1", Exclude = true };
            var exclude2 = new TableSelection() { Schema = "schema2", Table = "table2", Exclude = true };

            var tableSelectionSet = new TableSelectionSet();
            tableSelectionSet.AddSelections(new[] { exclude1, exclude2 });

            Assert.False(tableSelectionSet.Allows("schema1", "table1"));
            Assert.False(tableSelectionSet.Allows("schema2", "table2"));
            Assert.True(tableSelectionSet.Allows("schema1", "table2"));
            Assert.True(tableSelectionSet.Allows("schema2", "table1"));
        }

        [Fact]
        public void Include_and_Exclude_together_allow_those_selections_included_which_are_not_excluded()
        {
            var include1 = new TableSelection() { Schema = "schema1", Table = TableSelection.Any };
            var include2 = new TableSelection() { Schema = "schema2", Table = "table2" };
            var exclude1 = new TableSelection() { Schema = "schema1", Table = "table1", Exclude = true };
            var exclude2 = new TableSelection() { Schema = "schema3", Table = TableSelection.Any, Exclude = true };

            var tableSelectionSet = new TableSelectionSet();
            tableSelectionSet.AddSelections(new[] { include1, include2, exclude1, exclude2 });

            Assert.False(tableSelectionSet.Allows("schema1", "table1"));
            Assert.True(tableSelectionSet.Allows("schema1", "table2"));
            Assert.True(tableSelectionSet.Allows("schema1", "table3"));
            Assert.False(tableSelectionSet.Allows("schema2", "table1"));
            Assert.True(tableSelectionSet.Allows("schema2", "table2"));
            Assert.False(tableSelectionSet.Allows("schema2", "table3"));
            Assert.False(tableSelectionSet.Allows("schema3", "table1"));
            Assert.False(tableSelectionSet.Allows("schema3", "table2"));
            Assert.False(tableSelectionSet.Allows("schema3", "table3"));
            Assert.False(tableSelectionSet.Allows("schemaN", "table1"));
            Assert.False(tableSelectionSet.Allows("schemaN", "table2"));
            Assert.False(tableSelectionSet.Allows("schemaN", "table3"));
        }
    }
}
