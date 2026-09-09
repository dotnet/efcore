// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Metadata.Internal;

// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore.Extensions;

public class ScaffoldingMetadataExtensionsTest
{
    [ConditionalFact]
    public void It_sets_DbSet_name()
    {
        IMutableModel model = new Model();
        var entity = model.AddEntityType("Blog");
        entity.SetDbSetName("Blogs");

        Assert.Equal("Blogs", entity.GetDbSetName());
    }

    [ConditionalFact]
    public void It_sets_gets_database_name()
    {
        var model = new Model();
        var extensions = model;

        Assert.Null(extensions.GetDatabaseName());

        extensions.SetDatabaseName("Northwind");

        Assert.Equal("Northwind", extensions.GetDatabaseName());

        extensions.SetDatabaseName(null);

        Assert.Null(extensions.GetDatabaseName());
    }
}
