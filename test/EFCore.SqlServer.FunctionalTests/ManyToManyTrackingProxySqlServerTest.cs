// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore;

public class ManyToManyTrackingProxySqlServerTest
    : ManyToManyTrackingSqlServerTestBase<ManyToManyTrackingProxySqlServerTest.ManyToManyTrackingProxySqlServerFixture>
{
    public ManyToManyTrackingProxySqlServerTest(ManyToManyTrackingProxySqlServerFixture fixture)
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

    public class ManyToManyTrackingProxySqlServerFixture : ManyToManyTrackingSqlServerFixtureBase
    {
        protected override string StoreName { get; } = "ManyToManyTrackingProxies";

        public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
            => base.AddOptions(builder).UseChangeTrackingProxies();

        protected override IServiceCollection AddServices(IServiceCollection serviceCollection)
            => base.AddServices(serviceCollection.AddEntityFrameworkProxies());

        protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
        {
            base.OnModelCreating(modelBuilder, context);

            modelBuilder
                .SharedTypeEntity<Dictionary<string, object>>("JoinOneToThreePayloadFullShared")
                .Ignore("Payload"); // Mutable properties aren't proxyable on Dictionary
        }
    }
}
