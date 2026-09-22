// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestModels.ManyToManyModel;

namespace Microsoft.EntityFrameworkCore;

#nullable disable

public class ManyToManyTrackingProxyGeneratedKeysSqlServerTest(
    ManyToManyTrackingProxyGeneratedKeysSqlServerTest.ManyToManyTrackingProxyGeneratedKeysSqlServerFixture fixture)
    : ManyToManyTrackingSqlServerTestBase<
        ManyToManyTrackingProxyGeneratedKeysSqlServerTest.ManyToManyTrackingProxyGeneratedKeysSqlServerFixture>(fixture)
{
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

    public class ManyToManyTrackingProxyGeneratedKeysSqlServerFixture : ManyToManyTrackingSqlServerFixtureBase
    {
        protected override string StoreName
            => "ManyToManyTrackingProxyGeneratedKeys";

        public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
            => base.AddOptions(builder).UseChangeTrackingProxies();

        protected override IServiceCollection AddServices(IServiceCollection serviceCollection)
            => base.AddServices(serviceCollection.AddEntityFrameworkProxies());

        public override bool UseGeneratedKeys
            => true;

        protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
        {
            base.OnModelCreating(modelBuilder, context);

            modelBuilder
                .SharedTypeEntity<Dictionary<string, object>>("JoinOneToThreePayloadFullShared")
                .Ignore("Payload"); // Mutable properties aren't proxyable on Dictionary

            modelBuilder
                .SharedTypeEntity<Dictionary<string, object>>("UnidirectionalJoinOneToThreePayloadFullShared")
                .Ignore("Payload"); // Mutable properties aren't proxyable on Dictionary

            modelBuilder.Entity<EntityOne>().Property(e => e.Id).ValueGeneratedOnAdd();
            modelBuilder.Entity<EntityTwo>().Property(e => e.Id).ValueGeneratedOnAdd();
            modelBuilder.Entity<EntityThree>().Property(e => e.Id).ValueGeneratedOnAdd();
            modelBuilder.Entity<EntityCompositeKey>().Property(e => e.Key1).ValueGeneratedOnAdd();
            modelBuilder.Entity<EntityRoot>().Property(e => e.Id).ValueGeneratedOnAdd();
            modelBuilder.Entity<EntityTableSharing1>().Property(e => e.Id).ValueGeneratedOnAdd();
            modelBuilder.Entity<EntityTableSharing2>().Property(e => e.Id).ValueGeneratedOnAdd();
            modelBuilder.SharedTypeEntity<ProxyableSharedType>("PST").IndexerProperty<int>("Id").ValueGeneratedOnAdd();
            modelBuilder.Entity<ImplicitManyToManyA>().Property(e => e.Id).ValueGeneratedOnAdd();
            modelBuilder.Entity<ImplicitManyToManyB>().Property(e => e.Id).ValueGeneratedOnAdd();

            modelBuilder.Entity<UnidirectionalEntityOne>().Property(e => e.Id).ValueGeneratedOnAdd();
            modelBuilder.Entity<UnidirectionalEntityTwo>().Property(e => e.Id).ValueGeneratedOnAdd();
            modelBuilder.Entity<UnidirectionalEntityThree>().Property(e => e.Id).ValueGeneratedOnAdd();
            modelBuilder.Entity<UnidirectionalEntityCompositeKey>().Property(e => e.Key1).ValueGeneratedOnAdd();
            modelBuilder.Entity<UnidirectionalEntityRoot>().Property(e => e.Id).ValueGeneratedOnAdd();
        }
    }
}
