// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Metadata.Internal;

// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore;

public class ScaffoldingMetadataExtensionsTest
{
    [ConditionalFact]
    public void It_sets_gets_entity_type_errors()
    {
        IMutableModel model = new Model();

        Assert.Empty(model.GetReverseEngineeringErrors());

        model.GetOrCreateReverseEngineeringErrors().Add("FAIL!");
        Assert.Equal("FAIL!", Assert.Single(model.GetReverseEngineeringErrors()));

        model.SetReverseEngineeringErrors(new List<string>());
        Assert.Empty(model.GetReverseEngineeringErrors());

        model.GetOrCreateReverseEngineeringErrors().Add("FAIL 2!");
        model.GetOrCreateReverseEngineeringErrors().Clear();
        Assert.Empty(model.GetReverseEngineeringErrors());
    }

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
