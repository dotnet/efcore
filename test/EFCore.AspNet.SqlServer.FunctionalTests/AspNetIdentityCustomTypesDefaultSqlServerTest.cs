// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore;

public class AspNetIdentityCustomTypesDefaultSqlServerTest(AspNetIdentityCustomTypesDefaultSqlServerTest.AspNetIdentityCustomTypesDefaultSqlServerFixture fixture)
    : AspNetIdentityCustomTypesDefaultTestBase<
        AspNetIdentityCustomTypesDefaultSqlServerTest.AspNetIdentityCustomTypesDefaultSqlServerFixture>(fixture)
{
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
