// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Xunit;

namespace Microsoft.Data.Entity.Relational.Design.ReverseEngineering.Internal.Tests
{
    public class TableSelectionSetTests
    {
        [Fact]
        public void Null_schemas_and_tables_results_in_zero_length_lists()
        {
            var tableSelectionSet = new TableSelectionSet(null, null);
            Assert.Equal(0, tableSelectionSet.Schemas.Count);
            Assert.Equal(0, tableSelectionSet.Tables.Count);
        }
    }
}
