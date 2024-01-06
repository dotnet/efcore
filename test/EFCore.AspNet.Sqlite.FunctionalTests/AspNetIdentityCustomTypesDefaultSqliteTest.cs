// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore;

public class AspNetIdentityCustomTypesDefaultSqliteTest(AspNetIdentityCustomTypesDefaultSqliteTest.AspNetIdentityCustomTypesDefaultSqliteFixture fixture)
    : AspNetIdentityCustomTypesDefaultTestBase<AspNetIdentityCustomTypesDefaultSqliteTest.AspNetIdentityCustomTypesDefaultSqliteFixture>(fixture)
{
    public class AspNetIdentityCustomTypesDefaultSqliteFixture : AspNetIdentityFixtureBase
    {
        protected override IServiceCollection AddServices(IServiceCollection serviceCollection)
            => base.AddServices(serviceCollection).AddEntityFrameworkProxies();

        public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
            => base.AddOptions(builder)
                .UseLazyLoadingProxies()
                .ConfigureWarnings(e => e.Ignore(SqliteEventId.SchemaConfiguredWarning));

        public TestSqlLoggerFactory TestSqlLoggerFactory
            => (TestSqlLoggerFactory)ListLoggerFactory;

        protected override ITestStoreFactory TestStoreFactory
            => SqliteTestStoreFactory.Instance;

        protected override string StoreName
            => "AspNetCustomTypesDefaultIdentity";
    }
}
