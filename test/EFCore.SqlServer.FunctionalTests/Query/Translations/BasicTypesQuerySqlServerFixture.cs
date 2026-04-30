// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestModels.BasicTypesModel;

namespace Microsoft.EntityFrameworkCore.Query.Translations;

public class BasicTypesQuerySqlServerFixture : BasicTypesQueryFixtureBase, ITestSqlLoggerFactory
{
    protected override ITestStoreFactory TestStoreFactory
        => SqlServerTestStoreFactory.Instance;

    public TestSqlLoggerFactory TestSqlLoggerFactory
        => (TestSqlLoggerFactory)ListLoggerFactory;

    public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
        => TestEnvironment.SetCompatibilityLevelFromEnvironment(base.AddOptions(builder));

    protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
    {
        base.OnModelCreating(modelBuilder, context);

        modelBuilder.Entity<BasicTypesEntity>().Property(b => b.Decimal).HasColumnType("decimal(18,2)");
        modelBuilder.Entity<NullableBasicTypesEntity>().Property(b => b.Decimal).HasColumnType("decimal(18,2)");
    }
}
