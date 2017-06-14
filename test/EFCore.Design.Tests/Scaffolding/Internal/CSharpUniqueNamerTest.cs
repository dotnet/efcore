// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Scaffolding.Internal;
using Microsoft.EntityFrameworkCore.Scaffolding.Metadata;
using Xunit;

namespace Microsoft.EntityFrameworkCore
{
    public class CSharpUniqueNamerTest
    {
        [Fact]
        public void Returns_unique_name_for_type()
        {
            var namer = new CSharpUniqueNamer<DatabaseColumn>(s => s.Name, new CSharpUtilities());
            var input1 = new DatabaseColumn
            {
                Name = "Id"
            };
            var input2 = new DatabaseColumn
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
            var namer = new CSharpUniqueNamer<DatabaseTable>(t => t.Name, new CSharpUtilities());
            var table1 = new DatabaseTable { Name = "A B C" };
            var table2 = new DatabaseTable { Name = "A_B_C" };
            Assert.Equal("A_B_C", namer.GetName(table1));
            Assert.Equal("A_B_C1", namer.GetName(table2));
        }
    }
}
