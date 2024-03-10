// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestModels.ManyToManyModel;

namespace Microsoft.EntityFrameworkCore;

#nullable disable

public abstract class ManyToManyTrackingSqlServerTestBase<TFixture> : ManyToManyTrackingRelationalTestBase<TFixture>
    where TFixture : ManyToManyTrackingSqlServerTestBase<TFixture>.ManyToManyTrackingSqlServerFixtureBase
{
    protected ManyToManyTrackingSqlServerTestBase(TFixture fixture)
        : base(fixture)
    {
    }

    protected override Dictionary<string, DeleteBehavior> CustomDeleteBehaviors { get; } = new()
    {
        { "EntityBranch.RootSkipShared", DeleteBehavior.ClientCascade },
        { "EntityBranch2.Leaf2SkipShared", DeleteBehavior.ClientCascade },
        { "EntityBranch2.SelfSkipSharedLeft", DeleteBehavior.ClientCascade },
        { "EntityOne.SelfSkipPayloadLeft", DeleteBehavior.ClientCascade },
        { "EntityTableSharing1.TableSharing2Shared", DeleteBehavior.ClientCascade },
        { "EntityTwo.SelfSkipSharedLeft", DeleteBehavior.ClientCascade },
        { "UnidirectionalEntityBranch.UnidirectionalEntityRoot", DeleteBehavior.ClientCascade },
        { "UnidirectionalEntityOne.SelfSkipPayloadLeft", DeleteBehavior.ClientCascade },
        { "UnidirectionalEntityTwo.SelfSkipSharedRight", DeleteBehavior.ClientCascade },
    };

    public class ManyToManyTrackingSqlServerFixtureBase : ManyToManyTrackingRelationalFixture, ITestSqlLoggerFactory
    {
        public TestSqlLoggerFactory TestSqlLoggerFactory
            => (TestSqlLoggerFactory)ListLoggerFactory;

        protected override ITestStoreFactory TestStoreFactory
            => SqlServerTestStoreFactory.Instance;

        protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
        {
            base.OnModelCreating(modelBuilder, context);

            modelBuilder
                .Entity<JoinOneSelfPayload>()
                .Property(e => e.Payload)
                .HasDefaultValueSql("GETUTCDATE()");

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
