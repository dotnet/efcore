// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using Xunit;

namespace Microsoft.Data.Entity.Relational.Design.ReverseEngineering.Internal.Tests
{
    public class TableSelectionSetBuilderTests
    {
        [Fact]
        public void Null_results_in_InclusiveAll()
        {
            var tableSelectionSet = TableSelectionSetBuilder.BuildFromString(null);
            Assert.Equal(1, tableSelectionSet.InclusiveSelections.Count);
            Assert.Equal(0, tableSelectionSet.ExclusiveSelections.Count);
            Assert.Equal(TableSelection.InclusiveAll, tableSelectionSet.InclusiveSelections.First());
        }

        [Fact]
        public void Empty_string_results_in_InclusiveAll()
        {
            var tableSelectionSet = TableSelectionSetBuilder.BuildFromString(string.Empty);
            Assert.Equal(1, tableSelectionSet.InclusiveSelections.Count);
            Assert.Equal(0, tableSelectionSet.ExclusiveSelections.Count);
            Assert.Equal(TableSelection.InclusiveAll, tableSelectionSet.InclusiveSelections.First());
        }

        [Fact]
        public void Mix_of_inclusive_and_exclusive_gives_correct_results()
        {
            // Note: schema1:tableN should be ignored as it is covered by schema1:* selection
            var tableSelectionSet = TableSelectionSetBuilder
                .BuildFromString("schema1:*,-schema1:table1,schema2:table2,-schema3:*,schema1:tableN");
            Assert.Equal(2, tableSelectionSet.InclusiveSelections.Count);
            Assert.Equal(2, tableSelectionSet.ExclusiveSelections.Count);

            var inclusiveSelection0 = tableSelectionSet.InclusiveSelections[0];
            Assert.Equal("schema1", inclusiveSelection0.Schema );
            Assert.Equal(TableSelection.Any, inclusiveSelection0.Table);

            var inclusiveSelection1 = tableSelectionSet.InclusiveSelections[1];
            Assert.Equal("schema2", inclusiveSelection1.Schema);
            Assert.Equal("table2", inclusiveSelection1.Table);

            var exclusiveSelection0 = tableSelectionSet.ExclusiveSelections[0];
            Assert.Equal("schema1", exclusiveSelection0.Schema);
            Assert.Equal("table1", exclusiveSelection0.Table);

            var exclusiveSelection1 = tableSelectionSet.ExclusiveSelections[1];
            Assert.Equal("schema3", exclusiveSelection1.Schema);
            Assert.Equal(TableSelection.Any, exclusiveSelection1.Table);
        }
    }
}
