// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Design.Internal;
using Microsoft.EntityFrameworkCore.Scaffolding.Metadata;

namespace Microsoft.EntityFrameworkCore.Scaffolding.Internal;

public class CSharpUniqueNamerTest
{
    [ConditionalFact]
    public void Returns_unique_name_for_type()
    {
        var namer = new CSharpUniqueNamer<DatabaseColumn>(s => s.Name, new CSharpUtilities(), null, true);
        var table = new DatabaseTable { Database = new DatabaseModel(), Name = "foo" };
        var input1 = new DatabaseColumn
        {
            Table = table,
            Name = "Id",
            StoreType = "int"
        };
        var input2 = new DatabaseColumn
        {
            Table = table,
            Name = "Id",
            StoreType = "int"
        };

        Assert.Equal("Id", namer.GetName(input1));
        Assert.Equal("Id", namer.GetName(input1));

        Assert.Equal("Id1", namer.GetName(input2));
    }

    [ConditionalFact]
    public void Uses_comparer()
    {
        var namer = new CSharpUniqueNamer<DatabaseTable>(t => t.Name, new CSharpUtilities(), null, true);
        var database = new DatabaseModel();
        var table1 = new DatabaseTable { Database = database, Name = "A B C" };
        var table2 = new DatabaseTable { Database = database, Name = "A_B_C" };
        Assert.Equal("A_B_C", namer.GetName(table1));
        Assert.Equal("A_B_C1", namer.GetName(table2));
    }

    [ConditionalTheory]
    [InlineData("Name ending with s", "Name_ending_with_")]
    [InlineData("Name with no s at end", "Name_with_no_s_at_end")]
    public void Singularizes_names(string input, string output)
    {
        var pluralizer = new HumanizerPluralizer();
        var namer = new CSharpUniqueNamer<DatabaseTable>(t => t.Name, new CSharpUtilities(), pluralizer.Singularize, true);
        var table = new DatabaseTable { Database = new DatabaseModel(), Name = input };
        Assert.Equal(output, namer.GetName(table));
    }

    [ConditionalTheory]
    [InlineData("Name ending with s", "Name_ending_with_s")]
    [InlineData("Name with no s at end", "Name_with_no_s_at_ends")]
    public void Pluralizes_names(string input, string output)
    {
        var pluralizer = new HumanizerPluralizer();
        var namer = new CSharpUniqueNamer<DatabaseTable>(t => t.Name, new CSharpUtilities(), pluralizer.Pluralize, true);
        var table = new DatabaseTable { Database = new DatabaseModel(), Name = input };
        Assert.Equal(output, namer.GetName(table));
    }
}
