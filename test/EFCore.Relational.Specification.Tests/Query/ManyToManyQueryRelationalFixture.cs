// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestModels.ManyToManyModel;

namespace Microsoft.EntityFrameworkCore.Query;

#nullable disable

public abstract class ManyToManyQueryRelationalFixture : ManyToManyQueryFixtureBase, ITestSqlLoggerFactory
{
    public TestSqlLoggerFactory TestSqlLoggerFactory
        => (TestSqlLoggerFactory)ListLoggerFactory;

    protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
    {
        base.OnModelCreating(modelBuilder, context);

        modelBuilder.Entity<EntityTableSharing1>().ToTable("TableSharing");
        modelBuilder.Entity<EntityTableSharing2>(
            b =>
            {
                b.HasOne<EntityTableSharing1>().WithOne().HasForeignKey<EntityTableSharing2>(e => e.Id);
                b.ToTable("TableSharing");
            });
    }
}
