// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Scaffolding.Internal;
using Microsoft.EntityFrameworkCore.Scaffolding.Metadata;
using Xunit;

namespace Microsoft.EntityFrameworkCore
{
    public class CSharpUniqueNamerTest
    {
        [ConditionalFact]
        public void Returns_unique_name_for_type()
        {
            var namer = new CSharpUniqueNamer<DatabaseColumn>(s => s.Name, new CSharpUtilities(), null);
            var input1 = new DatabaseColumn { Name = "Id" };
            var input2 = new DatabaseColumn { Name = "Id" };

            Assert.Equal("Id", namer.GetName(input1));
            Assert.Equal("Id", namer.GetName(input1));

            Assert.Equal("Id1", namer.GetName(input2));
        }

        [ConditionalFact]
        public void Uses_comparer()
        {
            var namer = new CSharpUniqueNamer<DatabaseTable>(t => t.Name, new CSharpUtilities(), null);
            var table1 = new DatabaseTable { Name = "A B C" };
            var table2 = new DatabaseTable { Name = "A_B_C" };
            Assert.Equal("A_B_C", namer.GetName(table1));
            Assert.Equal("A_B_C1", namer.GetName(table2));
        }

        [ConditionalTheory]
        [InlineData("Name ending with s", "Name_ending_with_")]
        [InlineData("Name with no s at end", "Name_with_no_s_at_end")]
        public void Singularizes_names(string input, string output)
        {
            var fakePluralizer = new RelationalDatabaseModelFactoryTest.FakePluralizer();
            var namer = new CSharpUniqueNamer<DatabaseTable>(t => t.Name, new CSharpUtilities(), fakePluralizer.Singularize);
            var table = new DatabaseTable { Name = input };
            Assert.Equal(output, namer.GetName(table));
        }

        [ConditionalTheory]
        [InlineData("Name ending with s", "Name_ending_with_s")]
        [InlineData("Name with no s at end", "Name_with_no_s_at_ends")]
        public void Pluralizes_names(string input, string output)
        {
            var fakePluralizer = new RelationalDatabaseModelFactoryTest.FakePluralizer();
            var namer = new CSharpUniqueNamer<DatabaseTable>(t => t.Name, new CSharpUtilities(), fakePluralizer.Pluralize);
            var table = new DatabaseTable { Name = input };
            Assert.Equal(output, namer.GetName(table));
        }
    }
}
