// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestModels.ManyToManyModel;

namespace Microsoft.EntityFrameworkCore;

#nullable disable

public abstract class ManyToManyLoadSqliteTestBase<TFixture> : ManyToManyLoadTestBase<TFixture>
    where TFixture : ManyToManyLoadSqliteTestBase<TFixture>.ManyToManyLoadSqliteFixtureBase
{
    protected ManyToManyLoadSqliteTestBase(TFixture fixture)
        : base(fixture)
    {
    }

    public class ManyToManyLoadSqliteFixtureBase : ManyToManyLoadFixtureBase, ITestSqlLoggerFactory
    {
        public TestSqlLoggerFactory TestSqlLoggerFactory
            => (TestSqlLoggerFactory)ListLoggerFactory;

        protected override ITestStoreFactory TestStoreFactory
            => SqliteTestStoreFactory.Instance;

        protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
        {
            base.OnModelCreating(modelBuilder, context);

            modelBuilder
                .Entity<JoinOneSelfPayload>()
                .Property(e => e.Payload)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            modelBuilder
                .SharedTypeEntity<Dictionary<string, object>>("JoinOneToThreePayloadFullShared")
                .IndexerProperty<string>("Payload")
                .HasDefaultValue("Generated");

            modelBuilder
                .Entity<JoinOneToThreePayloadFull>()
                .Property(e => e.Payload)
                .HasDefaultValue("Generated");

            modelBuilder
                .Entity<UnidirectionalJoinOneSelfPayload>()
                .Property(e => e.Payload)
                .HasDefaultValueSql("GETUTCDATE()");

            modelBuilder
                .SharedTypeEntity<Dictionary<string, object>>("UnidirectionalJoinOneToThreePayloadFullShared")
                .IndexerProperty<string>("Payload")
                .HasDefaultValue("Generated");

            modelBuilder
                .Entity<UnidirectionalJoinOneToThreePayloadFull>()
                .Property(e => e.Payload)
                .HasDefaultValue("Generated");
        }
    }
}
