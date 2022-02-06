// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestModels.ManyToManyModel;

namespace Microsoft.EntityFrameworkCore;

public class ManyToManyTrackingProxyGeneratedKeysSqlServerTest
    : ManyToManyTrackingSqlServerTestBase<
        ManyToManyTrackingProxyGeneratedKeysSqlServerTest.ManyToManyTrackingProxyGeneratedKeysSqlServerFixture>
{
    public ManyToManyTrackingProxyGeneratedKeysSqlServerTest(ManyToManyTrackingProxyGeneratedKeysSqlServerFixture fixture)
        : base(fixture)
    {
    }

    public override Task Can_insert_many_to_many_shared_with_payload(bool async)
        // Mutable properties aren't proxyable on Dictionary
        => Task.CompletedTask;

    public override void Can_update_many_to_many_shared_with_payload()
    {
        // Mutable properties aren't proxyable on Dictionary
    }

    public override void Can_insert_update_delete_shared_type_entity_type()
    {
        // Mutable properties aren't proxyable on Dictionary
    }

    protected override bool RequiresDetectChanges
        => false;

    public class ManyToManyTrackingProxyGeneratedKeysSqlServerFixture : ManyToManyTrackingSqlServerFixtureBase
    {
        protected override string StoreName { get; } = "ManyToManyTrackingProxyGeneratedKeys";

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

            modelBuilder.Entity<EntityOne>().Property(e => e.Id).ValueGeneratedOnAdd();
            modelBuilder.Entity<EntityTwo>().Property(e => e.Id).ValueGeneratedOnAdd();
            modelBuilder.Entity<EntityThree>().Property(e => e.Id).ValueGeneratedOnAdd();
            modelBuilder.Entity<EntityCompositeKey>().Property(e => e.Key1).ValueGeneratedOnAdd();
            modelBuilder.Entity<EntityRoot>().Property(e => e.Id).ValueGeneratedOnAdd();
            modelBuilder.SharedTypeEntity<ProxyableSharedType>("PST").IndexerProperty<int>("Id").ValueGeneratedOnAdd();
            modelBuilder.Entity<ImplicitManyToManyA>().Property(e => e.Id).ValueGeneratedOnAdd();
            modelBuilder.Entity<ImplicitManyToManyB>().Property(e => e.Id).ValueGeneratedOnAdd();
        }
    }
}
