// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Data.Entity.Relational.Design.Model;
using Microsoft.Data.Entity.Sqlite.Design.ReverseEngineering;
using Xunit;

namespace Microsoft.Data.Entity.Sqlite.Design
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
    }
}
