// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore
{
    public class AspNetIdentityCustomTypesDefaultSqliteTest
        : AspNetIdentityCustomTypesDefaultTestBase<AspNetIdentityCustomTypesDefaultSqliteTest.AspNetIdentityCustomTypesDefaultSqliteFixture>
    {
        public AspNetIdentityCustomTypesDefaultSqliteTest(AspNetIdentityCustomTypesDefaultSqliteFixture fixture)
            : base(fixture)
        {
        }

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
}
