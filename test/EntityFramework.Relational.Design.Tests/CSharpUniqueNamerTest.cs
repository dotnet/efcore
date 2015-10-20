// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Data.Entity.Internal;
using Microsoft.Data.Entity.Relational.Design.ReverseEngineering;
using Microsoft.Data.Entity.Scaffolding.Internal;
using Microsoft.Data.Entity.Scaffolding.Model;
using Xunit;

namespace Microsoft.Data.Entity.Relational.Design
{
    public class CSharpUniqueNamerTest
    {
        [Fact]
        public void Returns_unique_name_for_type()
        {
            var namer = new CSharpUniqueNamer<Column>(s => s.Name);
            var input1 = new Column
            {
                Name = "Id"
            };
            var input2 = new Column
            {
                Name = "Id"
            };

            Assert.Equal("Id", namer.GetName(input1));
            Assert.Equal("Id", namer.GetName(input1));

            Assert.Equal("Id1", namer.GetName(input2));
        }

        [Fact]
        public void Uses_comparer()
        {
            var namer = new CSharpUniqueNamer<Table>(t => t.Name);
            var table1 = new Table { Name = "A B C" };
            var table2 = new Table { Name = "A_B_C" };
            Assert.Equal("A_B_C", namer.GetName(table1));
            Assert.Equal("A_B_C1", namer.GetName(table2));
        }
    }
}
