// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestModels.GearsOfWarModel;

namespace Microsoft.EntityFrameworkCore.Query;

#nullable disable

public abstract class TPTGearsOfWarQueryRelationalFixture : GearsOfWarQueryFixtureBase, ITestSqlLoggerFactory
{
    protected override string StoreName
        => "TPTGearsOfWarQueryTest";

    public new RelationalTestStore TestStore
        => (RelationalTestStore)base.TestStore;

    public TestSqlLoggerFactory TestSqlLoggerFactory
        => (TestSqlLoggerFactory)ListLoggerFactory;

    protected override bool ShouldLogCategory(string logCategory)
        => logCategory == DbLoggerCategory.Query.Name;

    protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
    {
        base.OnModelCreating(modelBuilder, context);

        modelBuilder.Entity<Gear>().UseTptMappingStrategy();

        modelBuilder.Entity<LocustHorde>().ToTable("LocustHordes");

        modelBuilder.Entity<LocustCommander>().ToTable("LocustCommanders");

        modelBuilder.Entity<Squad>()
            .HasMany(s => s.Members)
            .WithOne(g => g.Squad)
            .HasForeignKey(g => g.SquadId)
            .OnDelete(DeleteBehavior.ClientCascade);
    }
}
