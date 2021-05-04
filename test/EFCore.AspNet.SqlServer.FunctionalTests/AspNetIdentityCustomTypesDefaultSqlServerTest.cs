// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore
{
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
}
