// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable enable

namespace Microsoft.EntityFrameworkCore;

public class MaterializationInterceptionSqliteTest : MaterializationInterceptionTestBase,
    IClassFixture<MaterializationInterceptionSqliteTest.MaterializationInterceptionSqliteFixture>
{
    public MaterializationInterceptionSqliteTest(MaterializationInterceptionSqliteFixture fixture)
        : base(fixture)
    {
    }

    public class MaterializationInterceptionSqliteFixture : SingletonInterceptorsFixtureBase
    {
        protected override string StoreName
            => "MaterializationInterception";

        protected override ITestStoreFactory TestStoreFactory
            => SqliteTestStoreFactory.Instance;

        protected override IServiceCollection InjectInterceptors(
            IServiceCollection serviceCollection,
            IEnumerable<ISingletonInterceptor> injectedInterceptors)
            => base.InjectInterceptors(serviceCollection.AddEntityFrameworkSqlite(), injectedInterceptors);
    }
}
