// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Xunit;

namespace Microsoft.Data.Entity.Relational.Design.ReverseEngineering.Tests
{
    public class TableSelectionTests
    {
        [Fact]
        public void SpecificSelectionMatchesOnlyThatSchemaAndTable()
        {
            var tableSelection = new TableSelection()
            {
                Schema = "SpecificSchema",
                Table = "SpecificTable"
            };

            Assert.True(tableSelection.Matches("SpecificSchema", "SpecificTable"));
            Assert.False(tableSelection.Matches("OtherSchema", "SpecificTable"));
            Assert.False(tableSelection.Matches("SpecificSchema", "OtherTable"));
            Assert.False(tableSelection.Matches("OtherSchema", "OtherTable"));
            Assert.False(tableSelection.Matches(TableSelection.Any, "SpecificTable"));
            Assert.False(tableSelection.Matches("SpecificSchema", TableSelection.Any));
            Assert.False(tableSelection.Matches(TableSelection.Any, TableSelection.Any));
        }

        [Fact]
        public void AnySchemaSelectionMatchesProvidedTableIsTheSame()
        {
            var tableSelection = new TableSelection()
            {
                Schema = TableSelection.Any,
                Table = "SpecificTable"
            };

            Assert.True(tableSelection.Matches("SpecificSchema", "SpecificTable"));
            Assert.True(tableSelection.Matches("OtherSchema", "SpecificTable"));
            Assert.False(tableSelection.Matches("SpecificSchema", "OtherTable"));
            Assert.False(tableSelection.Matches("OtherSchema", "OtherTable"));
            Assert.True(tableSelection.Matches(TableSelection.Any, "SpecificTable"));
            Assert.False(tableSelection.Matches("SpecificSchema", TableSelection.Any));
            Assert.False(tableSelection.Matches(TableSelection.Any, TableSelection.Any));
        }


        [Fact]
        public void AnyTableSelectionMatchesProvidedSchemaIsTheSame()
        {
            var tableSelection = new TableSelection()
            {
                Schema = "SpecificSchema",
                Table = TableSelection.Any
            };

            Assert.True(tableSelection.Matches("SpecificSchema", "SpecificTable"));
            Assert.False(tableSelection.Matches("OtherSchema", "SpecificTable"));
            Assert.True(tableSelection.Matches("SpecificSchema", "OtherTable"));
            Assert.False(tableSelection.Matches("OtherSchema", "OtherTable"));
            Assert.False(tableSelection.Matches(TableSelection.Any, "SpecificTable"));
            Assert.True(tableSelection.Matches("SpecificSchema", TableSelection.Any));
            Assert.False(tableSelection.Matches(TableSelection.Any, TableSelection.Any));
        }


        [Fact]
        public void AnySchemaAnyTableSelectionMatchesAll()
        {
            var tableSelection = new TableSelection()
            {
                Schema = TableSelection.Any,
                Table = TableSelection.Any
            };

            Assert.True(tableSelection.Matches("SpecificSchema", "SpecificTable"));
            Assert.True(tableSelection.Matches("OtherSchema", "SpecificTable"));
            Assert.True(tableSelection.Matches("SpecificSchema", "OtherTable"));
            Assert.True(tableSelection.Matches("OtherSchema", "OtherTable"));
            Assert.True(tableSelection.Matches(TableSelection.Any, "SpecificTable"));
            Assert.True(tableSelection.Matches("SpecificSchema", TableSelection.Any));
            Assert.True(tableSelection.Matches(TableSelection.Any, TableSelection.Any));
        }
    }
}
