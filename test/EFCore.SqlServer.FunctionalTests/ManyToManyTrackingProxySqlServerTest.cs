// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore
{
    public class ManyToManyTrackingProxySqlServerTest
        : ManyToManyTrackingSqlServerTestBase<ManyToManyTrackingProxySqlServerTest.ManyToManyTrackingProxySqlServerFixture>
    {
        public ManyToManyTrackingProxySqlServerTest(ManyToManyTrackingProxySqlServerFixture fixture)
            : base(fixture)
        {
        }

        public override Task Can_insert_many_to_many_shared_with_payload(bool async)
        {
            // Mutable properties aren't proxyable on Dictionary
            return Task.CompletedTask;
        }

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
}
