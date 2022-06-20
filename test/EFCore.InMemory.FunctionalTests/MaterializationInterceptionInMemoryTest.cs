// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable enable

namespace Microsoft.EntityFrameworkCore;

public class MaterializationInterceptionInMemoryTest : MaterializationInterceptionTestBase,
    IClassFixture<MaterializationInterceptionInMemoryTest.MaterializationInterceptionInMemoryFixture>
{
    public MaterializationInterceptionInMemoryTest(MaterializationInterceptionInMemoryFixture fixture)
        : base(fixture)
    {
    }

    public class MaterializationInterceptionInMemoryFixture : SingletonInterceptorsFixtureBase
    {
        protected override string StoreName
            => "MaterializationInterception";

        protected override ITestStoreFactory TestStoreFactory
            => InMemoryTestStoreFactory.Instance;

        protected override IServiceCollection InjectInterceptors(
            IServiceCollection serviceCollection,
            IEnumerable<ISingletonInterceptor> injectedInterceptors)
            => base.InjectInterceptors(serviceCollection.AddEntityFrameworkInMemoryDatabase(), injectedInterceptors);

        public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
            => base.AddOptions(builder).ConfigureWarnings(c => c.Ignore(InMemoryEventId.TransactionIgnoredWarning));
    }
}
