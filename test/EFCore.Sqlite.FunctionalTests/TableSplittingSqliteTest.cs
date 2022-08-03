// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestModels.TransportationModel;

namespace Microsoft.EntityFrameworkCore;

public class TableSplittingSqliteTest : TableSplittingTestBase
{
    public TableSplittingSqliteTest(ITestOutputHelper testOutputHelper)
        : base(testOutputHelper)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Engine>().ToTable("Vehicles")
            .Property(e => e.Computed).HasComputedColumnSql("1");
    }

    protected override ITestStoreFactory TestStoreFactory
        => SqliteTestStoreFactory.Instance;
}
