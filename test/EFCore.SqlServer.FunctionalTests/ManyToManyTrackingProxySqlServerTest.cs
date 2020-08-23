// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

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

        protected override bool RequiresDetectChanges
            => false;

        public class ManyToManyTrackingProxySqlServerFixture : ManyToManyTrackingSqlServerFixtureBase
        {
            protected override string StoreName { get; } = "ManyToManyTrackingProxies";

            public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
                => base.AddOptions(builder).UseChangeTrackingProxies();

            protected override IServiceCollection AddServices(IServiceCollection serviceCollection)
                => base.AddServices(serviceCollection.AddEntityFrameworkProxies());
        }
    }
}
