// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Data.Entity.Scaffolding;
using Xunit;

namespace Microsoft.Data.Entity.Relational.Design
{
    public class TableSelectionSetTests
    {
        [Fact]
        public void Modifying_inclusive_all_doesnt_change_static()
        {
            var first = TableSelectionSet.InclusiveAll;
            first.Tables.Add("Table");
            var second = TableSelectionSet.InclusiveAll;
            Assert.Empty(second.Tables);
        }
    }
}
