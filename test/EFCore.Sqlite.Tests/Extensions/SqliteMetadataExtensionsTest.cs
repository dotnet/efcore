// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore;

public class SqliteMetadataExtensionsTest
{
    [ConditionalFact]
    public void Can_get_and_set_srid()
    {
        var modelBuilder = new ModelBuilder();

        var property = modelBuilder
            .Entity<Customer>()
            .Property(e => e.Geometry)
            .Metadata;

        Assert.Null(property.GetSrid());

        property.SetSrid(1);

        Assert.Equal(1, property.GetSrid());

        property.SetSrid(null);

        Assert.Null(property.GetSrid());
    }

    private class Customer
    {
        public int Id { get; set; }
        public string? Geometry { get; set; }
    }
}
