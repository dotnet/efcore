// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestModels.RelationshipsModel;

namespace Microsoft.EntityFrameworkCore.Query.Relationships.JsonOwnedNavigations;

public class JsonOwnedTypeSqlServerFixture : JsonOwnedNavigationsRelationalFixtureBase, ITestSqlLoggerFactory
{
    protected override string StoreName => "JsonTypeRelationshipsQueryTest";

    protected override ITestStoreFactory TestStoreFactory
        => SqlServerTestStoreFactory.Instance;

    public TestSqlLoggerFactory TestSqlLoggerFactory
        => (TestSqlLoggerFactory)ListLoggerFactory;

    protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
    {
        base.OnModelCreating(modelBuilder, context);

        modelBuilder.Entity<RelationshipsRoot>().OwnsOne(x => x.RequiredReferenceTrunk).HasColumnType("json");
        modelBuilder.Entity<RelationshipsRoot>().OwnsOne(x => x.OptionalReferenceTrunk).HasColumnType("json");
        modelBuilder.Entity<RelationshipsRoot>().OwnsMany(x => x.CollectionTrunk).HasColumnType("json");
    }

    public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
        => base.AddOptions(builder.ConfigureWarnings(b => b.Ignore(SqlServerEventId.JsonTypeExperimental)));
}
