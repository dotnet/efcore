// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore;

#nullable disable

public class ManyToManyLoadProxySqliteTest(ManyToManyLoadProxySqliteTest.ManyToManyLoadProxySqliteFixture fixture)
    : ManyToManyLoadSqliteTestBase<ManyToManyLoadProxySqliteTest.ManyToManyLoadProxySqliteFixture>(fixture)
{
    protected override bool ExpectLazyLoading
        => true;

    public class ManyToManyLoadProxySqliteFixture : ManyToManyLoadSqliteFixtureBase
    {
        protected override string StoreName
            => "ManyToManyLoadProxies";

        public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
            => base.AddOptions(builder).UseLazyLoadingProxies();

        protected override IServiceCollection AddServices(IServiceCollection serviceCollection)
            => base.AddServices(serviceCollection.AddEntityFrameworkProxies());
    }
}
