// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore.Scaffolding;
using Microsoft.EntityFrameworkCore.Scaffolding.Internal;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Sqlite.Design
{
    public class SqliteTableSelectionSetExtensionsTests
    {
        [Fact]
        public void Allows_updates_IsMatched_for_matching_selections()
        {
            var tableNames = new List<string> { "table0", "table1", "table2" };
            var tableSelectionSet = new TableSelectionSet(tableNames);

            Assert.Equal(3, tableSelectionSet.Tables.Count);

            // Allows() has not run yet - so all table selections should have IsMatched false
            foreach (var table in tableSelectionSet.Tables)
            {
                Assert.False(table.IsMatched);
            }

            // When Allows() runs the matching table selection is marked true
            Assert.True(tableSelectionSet.Allows("table0"));
            var table0Selection = tableSelectionSet.Tables.First();
            var table1Selection = tableSelectionSet.Tables.Skip(1).First();
            var table2Selection = tableSelectionSet.Tables.Last();
            Assert.True(table0Selection.IsMatched);
            Assert.False(table1Selection.IsMatched);
            Assert.False(table2Selection.IsMatched);

            // When Allows() runs again the 2nd table selection is also marked true
            Assert.True(tableSelectionSet.Allows("table1"));
            Assert.True(table0Selection.IsMatched);
            Assert.True(table1Selection.IsMatched);
            Assert.False(table2Selection.IsMatched);

            // When Allows() runs a third time against a non-selected table no further table selection is marked true
            Assert.False(tableSelectionSet.Allows("tableOther"));
            Assert.True(table0Selection.IsMatched);
            Assert.True(table1Selection.IsMatched);
            Assert.False(table2Selection.IsMatched);
        }
    }
}
