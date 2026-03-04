// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestModels.ManyToManyModel;

namespace Microsoft.EntityFrameworkCore;

#nullable disable

public class ManyToManyTrackingProxySqlServerTest(ManyToManyTrackingProxySqlServerTest.ManyToManyTrackingProxySqlServerFixture fixture)
    : ManyToManyTrackingSqlServerTestBase<ManyToManyTrackingProxySqlServerTest.ManyToManyTrackingProxySqlServerFixture>(fixture)
{
    protected override Dictionary<string, DeleteBehavior> CustomDeleteBehaviors { get; } = new()
    {
        { "EntityBranch.RootSkipShared", DeleteBehavior.ClientCascade },
        { "EntityBranch2.Leaf2SkipShared", DeleteBehavior.ClientCascade },
        { "EntityBranch2.SelfSkipSharedLeft", DeleteBehavior.Restrict },
        { "EntityBranch2.SelfSkipSharedRight", DeleteBehavior.Restrict },
        { "EntityOne.SelfSkipPayloadLeft", DeleteBehavior.ClientCascade },
        { "EntityTwo.SelfSkipSharedLeft", DeleteBehavior.ClientCascade },
        { "EntityTableSharing1.TableSharing2Shared", DeleteBehavior.ClientCascade },
        { "UnidirectionalEntityBranch.UnidirectionalEntityRoot", DeleteBehavior.ClientCascade },
        { "UnidirectionalEntityOne.SelfSkipPayloadLeft", DeleteBehavior.ClientCascade },
        { "UnidirectionalEntityTwo.SelfSkipSharedRight", DeleteBehavior.ClientCascade }
    };

    public override Task Can_insert_many_to_many_shared_with_payload(bool async)
        // Mutable properties aren't proxyable on Dictionary
        => Task.CompletedTask;

    public override Task Can_update_many_to_many_shared_with_payload()
        // Mutable properties aren't proxyable on Dictionary
        => Task.CompletedTask;

    public override Task Can_insert_update_delete_shared_type_entity_type()
        // Mutable properties aren't proxyable on Dictionary
        => Task.CompletedTask;

    public override Task Can_insert_many_to_many_shared_with_payload_unidirectional(bool async)
        // Mutable properties aren't proxyable on Dictionary
        => Task.CompletedTask;

    public override Task Can_update_many_to_many_shared_with_payload_unidirectional()
        // Mutable properties aren't proxyable on Dictionary
        => Task.CompletedTask;

    protected override bool RequiresDetectChanges
        => false;

    public class ManyToManyTrackingProxySqlServerFixture : ManyToManyTrackingSqlServerFixtureBase
    {
        protected override string StoreName
            => "ManyToManyTrackingProxies";

        public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
            => base.AddOptions(builder).UseChangeTrackingProxies();

        protected override IServiceCollection AddServices(IServiceCollection serviceCollection)
            => base.AddServices(serviceCollection.AddEntityFrameworkProxies());

        protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
        {
            base.OnModelCreating(modelBuilder, context);

            modelBuilder.Entity<EntityBranch2>()
                .HasMany(e => e.SelfSkipSharedLeft)
                .WithMany(e => e.SelfSkipSharedRight)
                .UsingEntity<Dictionary<string, object>>(
                    "EntityBranch2EntityBranch2",
                    r => r.HasOne<EntityBranch2>().WithMany().HasForeignKey("SelfSkipSharedRightId").OnDelete(DeleteBehavior.Restrict),
                    l => l.HasOne<EntityBranch2>().WithMany().HasForeignKey("SelfSkipSharedLeftId").OnDelete(DeleteBehavior.Restrict));

            modelBuilder
                .SharedTypeEntity<Dictionary<string, object>>("JoinOneToThreePayloadFullShared")
                .Ignore("Payload"); // Mutable properties aren't proxyable on Dictionary

            modelBuilder
                .SharedTypeEntity<Dictionary<string, object>>("UnidirectionalJoinOneToThreePayloadFullShared")
                .Ignore("Payload"); // Mutable properties aren't proxyable on Dictionary
        }
    }
}
