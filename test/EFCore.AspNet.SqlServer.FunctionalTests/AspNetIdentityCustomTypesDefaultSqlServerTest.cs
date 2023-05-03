// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore;

public class AspNetIdentityCustomTypesDefaultSqlServerTest
    : AspNetIdentityCustomTypesDefaultTestBase<
        AspNetIdentityCustomTypesDefaultSqlServerTest.AspNetIdentityCustomTypesDefaultSqlServerFixture>
{
    public AspNetIdentityCustomTypesDefaultSqlServerTest(AspNetIdentityCustomTypesDefaultSqlServerFixture fixture)
        : base(fixture)
    {
    }

    public class AspNetIdentityCustomTypesDefaultSqlServerFixture : AspNetIdentityFixtureBase
    {
        protected override IServiceCollection AddServices(IServiceCollection serviceCollection)
            => base.AddServices(serviceCollection).AddEntityFrameworkProxies();

        public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
            => base.AddOptions(builder).UseLazyLoadingProxies();

        public TestSqlLoggerFactory TestSqlLoggerFactory
            => (TestSqlLoggerFactory)ListLoggerFactory;

        protected override ITestStoreFactory TestStoreFactory
            => SqlServerTestStoreFactory.Instance;

        protected override string StoreName
            => "AspNetCustomTypesDefaultIdentity";
    }
}
