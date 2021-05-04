// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore
{
    public class ManyToManyLoadProxySqliteTest
        : ManyToManyLoadSqliteTestBase<ManyToManyLoadProxySqliteTest.ManyToManyLoadProxySqliteFixture>
    {
        public ManyToManyLoadProxySqliteTest(ManyToManyLoadProxySqliteFixture fixture)
            : base(fixture)
        {
        }

        protected override bool ExpectLazyLoading
            => true;

        public class ManyToManyLoadProxySqliteFixture : ManyToManyLoadSqliteFixtureBase
        {
            protected override string StoreName { get; } = "ManyToManyLoadProxies";

            public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
                => base.AddOptions(builder).UseLazyLoadingProxies();

            protected override IServiceCollection AddServices(IServiceCollection serviceCollection)
                => base.AddServices(serviceCollection.AddEntityFrameworkProxies());
        }
    }
}
